using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Media;
using CadCat.DataStructures;
using CadCat.Math;
using CadCat.ModelInterfaces;
using CadCat.Rendering;

namespace CadCat.GeometryModels.Proxys
{
	using Real = Double;
	class TempSurface : ParametrizedModel, IConvertibleToPoints
	{

		public enum SurfaceType
		{
			Bezier,
			BSpline
		}
		//private List<ModelLine> lines;
		private readonly List<int> indices = new List<int>();
		private readonly List<Math.Vector3> points = new List<Vector3>();


		private int uDensity = 3;
		private int vDensity = 3;

		private Real width = 10.0;
		private Real height = 10.0;

		private bool changed;

		private Real curvatureAngle = 180.0;
		private bool curved;

		private double curvatureDisplacement = 0;

		private SurfaceType type = SurfaceType.Bezier;


		public int UDensity
		{
			get
			{
				return uDensity;
			}
			set
			{
				if (uDensity != value && value > 0)
				{
					uDensity = value;
					changed = true;
					OnPropertyChanged();
				}
			}
		}

		public int VDensity
		{
			get
			{
				return vDensity;
			}
			set
			{
				if (vDensity != value && value > 0)
				{
					vDensity = value;
					changed = true;
					OnPropertyChanged();
				}
			}
		}

		public bool Curved
		{
			get
			{
				return curved;

			}
			set
			{
				curved = value;
				changed = true;
				OnPropertyChanged();
			}
		}

		public Real CurvatureAngle
		{
			get { return curvatureAngle; }
			set
			{
				if (System.Math.Abs(curvatureAngle - value) > Math.Utils.Eps && value>1)
				{
					curvatureAngle = value;
					changed = true;
					OnPropertyChanged();
				}
			}
		}


		public Real Width
		{
			get
			{
				return width;
			}
			set
			{
				if (System.Math.Abs(width - value) > Math.Utils.Eps && value > 0)
				{
					width = value;
					changed = true;
					OnPropertyChanged();
				}
			}
		}

		public Real Height
		{
			get
			{
				return height;
			}
			set
			{
				if (System.Math.Abs(height - value) > Math.Utils.Eps && value > 0)
				{
					height = value;
					changed = true;
					OnPropertyChanged();
				}
			}
		}

		public TempSurface()
		{
			changed = true;
		}

		public SurfaceType Type
		{
			get { return type; }
			set
			{
				type = value;
				OnPropertyChanged();
			}
		}

		private void GenerateModel()
		{
			int widthPoints = UDensity * 3 + 1;
			int heightPoints = VDensity * 3 + 1;
			points.Capacity = System.Math.Max(points.Capacity, widthPoints * heightPoints);
			indices.Capacity = System.Math.Max(indices.Capacity, ((widthPoints - 1) * (heightPoints - 1) + (widthPoints - 1) + (heightPoints - 1)) * 4);

			points.Clear();
			indices.Clear();



			if (!Curved)
			{
				Real widthStep = Width / (widthPoints - 1);
				Real heightStep = Height / (heightPoints - 1);


				for (int i = 0; i < widthPoints; i++)
				{
					Real x = widthStep * i - Width / 2;
					for (int j = 0; j < heightPoints; j++)
					{
						Real y = heightStep * j - Height / 2;
						points.Insert(i * heightPoints + j, new Vector3(x, y));
					}
				}
			}
			else
			{
				Real angleStep = CurvatureAngle / (widthPoints - 1);
				Real heightStep = Height / (heightPoints - 1);

				for (int i = 0; i < widthPoints; i++)
				{
					Real angle = Utils.DegToRad(i * angleStep - CurvatureAngle / 2);
					Real x = System.Math.Sin(angle);
					for (int j = 0; j < heightPoints; j++)
					{
						Real y = heightStep * j - Height / 2;
						Real z = System.Math.Cos(angle);
						points.Insert(i * heightPoints + j, new Vector3(x, y, z));
					}
				}

			}

			for (int i = 0; i < widthPoints - 1; i++)
				for (int j = 0; j < heightPoints - 1; j++)
				{
					indices.Add(i * heightPoints + j);
					indices.Add(i * heightPoints + j + 1);
					indices.Add(i * heightPoints + j);
					indices.Add((i + 1) * heightPoints + j);
				}
			for (int i = 0; i < widthPoints - 1; i++)
			{
				indices.Add(heightPoints * (i + 1) - 1);
				indices.Add(heightPoints * (i + 2) - 1);
			}

			for (int j = 0; j < heightPoints - 1; j++)
			{
				indices.Add((widthPoints - 1) * heightPoints + j);
				indices.Add((widthPoints - 1) * heightPoints + j + 1);
			}


			changed = false;
		}

		public override void Render(BaseRenderer renderer)
		{
			base.Render(renderer);
			if (changed)
				GenerateModel();
			renderer.SelectedColor = IsSelected ? Colors.LimeGreen : Colors.White;
			renderer.UseIndices = true;
			renderer.Points = points;
			renderer.Indices = indices;
			renderer.ModelMatrix = Transform.CreateTransformMatrix();
			renderer.Transform();
			renderer.DrawLines();
		}
		public override string GetName()
		{
			return "New Surface Patch" + base.GetName();
		}

		public void Convert(SceneData scene)
		{
			switch (Type)
			{
				case SurfaceType.Bezier:
					int widthPoints = UDensity * 3 + 1;
					int heightPoints = VDensity * 3 + 1;
					var catPoints = new CatPoint[widthPoints, heightPoints];
					var matrix = GetMatrix(false, new Vector3());
					if (!Curved)
						for (int i = 0; i < widthPoints; i++)
							for (int j = 0; j < heightPoints; j++)
							{
								var pt = points[i * heightPoints + j];
								catPoints[i, j] = scene.CreateHiddenCatPoint((matrix * new Vector4(pt)).ClipToVector3());
							}
					else
					{
						bool cylinder = System.Math.Abs(CurvatureAngle - 360) < Utils.Eps;

						for (int j = 0; j < heightPoints; j++)
							for (int i = 0; i < widthPoints - (cylinder ? 1 : 0); i++)
							{

							}


						for (int i = 0; i < widthPoints - (cylinder ? 1 : 0); i++)
							for (int j = 0; j < heightPoints; j++)
							{
								Vector3 pt;
								if (i % 3 != 0)
								{
									int ai = i - i % 3;
									int ci = ai + 3;
									var p = (points[(ai + 1) * heightPoints + j] + points[(ai + 2) * heightPoints + j]);
									var b = p - (points[ai * heightPoints + j] + points[ci * heightPoints + j]) / 2;

									var pot = i % 3 == 1 ? points[ai * heightPoints + j] : points[ci * heightPoints + j];

									pt = b * 2 / 3.0 + pot * 1 / 3.0;
								}
								else
									pt = points[i * heightPoints + j];
								catPoints[i, j] = scene.CreateHiddenCatPoint((matrix * new Vector4(pt)).ClipToVector3());
							}
						if (cylinder)
							for (int j = 0; j < heightPoints; j++)
								catPoints[widthPoints - 1, j] = catPoints[0, j];
					}
					scene.RemoveModel(this);
					var subArray = new CatPoint[4, 4];
					var patches = new List<BezierPatch>();
					for (int i = 0; i < UDensity; i++)
						for (int j = 0; j < VDensity; j++)
						{
							for (int x = 0; x < 4; x++)
								for (int y = 0; y < 4; y++)
									subArray[x, y] = catPoints[i * 3 + x, j * 3 + y];
							var patch = new BezierPatch(subArray);
							scene.AddNewModel(patch);
							patches.Add(patch);

						}
					var catPointsList = new List<CatPoint>(catPoints.GetLength(0)*catPoints.GetLength(1));
					foreach (var catPoint in catPoints)
					{
						catPointsList.Add(catPoint);
					}
					scene.AddNewModel(new Surface(patches,catPointsList,scene));

					break;
				case SurfaceType.BSpline:
					throw new NotImplementedException();

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
