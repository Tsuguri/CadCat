using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Media;
using CadCat.DataStructures;
using CadCat.Math;
using CadCat.ModelInterfaces;
using CadCat.Rendering;
// ReSharper disable ExplicitCallerInfoArgument

namespace CadCat.GeometryModels.Proxys
{
	using Real = Double;

	public enum SurfaceType
	{
		Bezier,
		BSpline,
		Nurbs
	}

	class TempSurface : ParametrizedModel, IConvertibleToPoints
	{


		//private List<ModelLine> lines;
		private readonly List<int> indices = new List<int>();
		private readonly List<int> outlineIndices = new List<int>();
		private readonly List<Math.Vector3> points = new List<Vector3>();


		private int uDensity = 3;
		private int vDensity = 3;

		private Real width = 10.0;
		private Real height = 10.0;
		private Real radius = 1.0;

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
				OnPropertyChanged(nameof(IsCylinder));
			}
		}

		public Real CurvatureAngle
		{
			get { return curvatureAngle; }
			set
			{
				if (System.Math.Abs(curvatureAngle - value) > Math.Utils.Eps && value >= 1.0 && value <= 360.0)
				{
					curvatureAngle = value;
					changed = true;
					OnPropertyChanged();
					OnPropertyChanged(nameof(IsCylinder));
				}
			}
		}

		public bool IsCylinder => Curved && System.Math.Abs(curvatureAngle - 360.0) < Utils.Eps;

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

		public Real Radius
		{
			get { return radius; }
			set
			{
				if (System.Math.Abs(radius - value) > Math.Utils.Eps && value > 0)
				{
					radius = value;
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
			if (IsCylinder && UDensity < 4)
				UDensity = 4;

			int widthPoints = UDensity * 3 + 1;
			int heightPoints = VDensity * 3 + 1;
			points.Capacity = System.Math.Max(points.Capacity, widthPoints * heightPoints);
			indices.Capacity = System.Math.Max(indices.Capacity, ((widthPoints - 1) * (heightPoints - 1) + (widthPoints - 1) + (heightPoints - 1)) * 4);
			outlineIndices.Capacity = System.Math.Max(outlineIndices.Capacity, (UDensity + 1 + VDensity + 1) * 2);
			points.Clear();
			indices.Clear();
			outlineIndices.Clear();


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
					Real x = radius * System.Math.Sin(angle);
					for (int j = 0; j < heightPoints; j++)
					{
						Real y = heightStep * j - Height / 2;
						Real z = radius * System.Math.Cos(angle);
						points.Insert(i * heightPoints + j, new Vector3(x, y, z));
					}
				}

			}

			for (int i = 0; i < widthPoints - 1; i++)
				for (int j = 0; j < heightPoints - 1; j++)
				{
					int ind = i * heightPoints + j;
					bool horizontalOutline = ind % heightPoints % 3 == 0;
					bool verticalOutline = i % 3 == 0;

					if (verticalOutline)
					{
						outlineIndices.Add(i * heightPoints + j);
						outlineIndices.Add(i * heightPoints + j + 1);
					}
					else
					{
						indices.Add(i * heightPoints + j);
						indices.Add(i * heightPoints + j + 1);
					}

					if (horizontalOutline)
					{
						outlineIndices.Add(i * heightPoints + j);
						outlineIndices.Add((i + 1) * heightPoints + j);
					}
					else
					{
						indices.Add(i * heightPoints + j);
						indices.Add((i + 1) * heightPoints + j);
					}
				}
			for (int i = 0; i < widthPoints - 1; i++)
			{
				outlineIndices.Add(heightPoints * (i + 1) - 1);
				outlineIndices.Add(heightPoints * (i + 2) - 1);
			}

			for (int j = 0; j < heightPoints - 1; j++)
			{
				outlineIndices.Add((widthPoints - 1) * heightPoints + j);
				outlineIndices.Add((widthPoints - 1) * heightPoints + j + 1);
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

			renderer.Indices = outlineIndices;
			renderer.SelectedColor = IsSelected ? Colors.OrangeRed : Colors.White;
			renderer.DrawLines();

		}

		public override string GetName()
		{
			return "New Surface Patch" + base.GetName();
		}

		private void ConvertToBezierPatches(SceneData scene)
		{

			int widthPoints = UDensity * 3 + 1;
			int heightPoints = VDensity * 3 + 1;
			var catPoints = new CatPoint[widthPoints, heightPoints];
			var matrix = GetMatrix(false, new Vector3());
			if (!Curved)
				for (int i = 0; i < widthPoints; i++)
					for (int j = 0; j < heightPoints; j++)
					{
						var pt = points[i * heightPoints + j];
						catPoints[i, j] = scene.CreateCatPoint((matrix * new Vector4(pt)).ClipToVector3());
					}
			else
			{
				bool cylinder = System.Math.Abs(CurvatureAngle - 360) < Utils.Eps;

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
						catPoints[i, j] = scene.CreateCatPoint((matrix * new Vector4(pt)).ClipToVector3());
					}
				if (cylinder)
					for (int j = 0; j < heightPoints; j++)
						catPoints[widthPoints - 1, j] = catPoints[0, j];
			}
			scene.RemoveModel(this);
			var subArray = new CatPoint[4, 4];
			var ptchs = new Patch[UDensity, VDensity];
			for (int i = 0; i < UDensity; i++)
				for (int j = 0; j < VDensity; j++)
				{
					for (int x = 0; x < 4; x++)
						for (int y = 0; y < 4; y++)
							subArray[x, y] = catPoints[i * 3 + x, j * 3 + y];
					var patch = new BezierPatch(subArray)
					{
						UPos = i,
						VPos = j
					};
					scene.AddNewModel(patch);
					ptchs[i, j] = patch;

				}
			var catPointsList = new List<CatPoint>(catPoints.GetLength(0) * catPoints.GetLength(1));
			foreach (var catPoint in catPoints)
			{
				catPointsList.Add(catPoint);
			}
			scene.AddNewModel(new Surface(SurfaceType.Bezier, ptchs, catPointsList, scene, IsCylinder, false) { PatchesU = UDensity, PatchesV = VDensity });

		}

		private void ConvertToBSpline(SceneData scene)
		{

			int widthPoints = UDensity + 3;
			int heightPoints = VDensity + 3;

			var pts = new Vector3[widthPoints - 2, heightPoints - 2];

			for (int i = 0; i < UDensity + 1; i++)
				for (int j = 0; j < VDensity + 1; j++)
					pts[i, j] = points[i * 3 * (VDensity * 3 + 1) + j * 3];

			var catPoints = new CatPoint[widthPoints, heightPoints];
			var matrix = GetMatrix(false, new Vector3());
			if (!Curved)
			{
				for (int i = 1; i < widthPoints - 1; i++)
					for (int j = 1; j < heightPoints - 1; j++)
					{
						var pt = pts[(i - 1), (j - 1)];
						catPoints[i, j] = scene.CreateCatPoint((matrix * new Vector4(pt)).ClipToVector3());
					}

				for (int i = 1; i < widthPoints - 1; i++)
				{
					int j = 0;
					var pt = pts[i - 1, 0] * 2 - pts[i - 1, 1];
					catPoints[i, j] = scene.CreateCatPoint((matrix * new Vector4(pt)).ClipToVector3());
					j = heightPoints - 1;
					pt = pts[i - 1, j - 2] * 2 - pts[i - 1, j - 3];
					catPoints[i, j] = scene.CreateCatPoint((matrix * new Vector4(pt)).ClipToVector3());
				}
				for (int j = 1; j < heightPoints - 1; j++)
				{
					int i = 0;
					var pt = pts[0, j - 1] * 2 - pts[1, j - 1];
					catPoints[i, j] = scene.CreateCatPoint((matrix * new Vector4(pt)).ClipToVector3());
					i = widthPoints - 1;
					pt = pts[i - 2, j - 1] * 2 - pts[i - 3, j - 1];
					catPoints[i, j] = scene.CreateCatPoint((matrix * new Vector4(pt)).ClipToVector3());
				}

				catPoints[0, 0] = scene.CreateCatPoint((matrix * new Vector4(pts[0, 0] * 2 - pts[1, 1])).ClipToVector3());
				catPoints[widthPoints - 1, heightPoints - 1] = scene.CreateCatPoint(
					(matrix * new Vector4(pts[pts.GetLength(0) - 1, pts.GetLength(1) - 1] * 2 -
										  pts[pts.GetLength(0) - 2, pts.GetLength(1) - 2])).ClipToVector3());

				catPoints[widthPoints - 1, 0] =
					scene.CreateCatPoint((matrix * new Vector4(pts[pts.GetLength(0) - 1, 0] * 2 - pts[pts.GetLength(0) - 2, 1]))
						.ClipToVector3());
				catPoints[0, heightPoints - 1] =
					scene.CreateCatPoint((matrix * new Vector4(pts[0, pts.GetLength(1) - 1] * 2 - pts[1, pts.GetLength(1) - 2]))
						.ClipToVector3());
			}
			else
			{
				int curvePoints = widthPoints - (IsCylinder ? 3 : 0);
				Real angleStep = CurvatureAngle / curvePoints;
				Real heightStep = Height / (heightPoints - 1);
				//r * (4sqrt(2) / (6n))
				Real newRadius = Radius * (1 + 2 * System.Math.Sqrt(2) / (3.0 * (double)(UDensity - 2)));

				for (int i = 0; i < curvePoints; i++)
				{
					Real angle = Utils.DegToRad(i * angleStep - CurvatureAngle / 2);
					Real x = newRadius * System.Math.Sin(angle);
					for (int j = 0; j < heightPoints; j++)
					{
						Real y = heightStep * j - Height / 2;
						Real z = newRadius * System.Math.Cos(angle);
						catPoints[i, j] = scene.CreateCatPoint((matrix * new Vector4(x, y, z, 1.0)).ClipToVector3());
					}
				}

				if (IsCylinder)
					for (int j = 0; j < heightPoints; j++)
					{
						catPoints[widthPoints - 3, j] = catPoints[0, j];
						catPoints[widthPoints - 2, j] = catPoints[1, j];
						catPoints[widthPoints - 1, j] = catPoints[2, j];

					}
			}

			scene.RemoveModel(this);
			var subArray = new CatPoint[4, 4];
			var ptches = new Patch[UDensity, VDensity];
			for (int i = 0; i < UDensity; i++)
				for (int j = 0; j < VDensity; j++)
				{
					for (int x = 0; x < 4; x++)
						for (int y = 0; y < 4; y++)
							subArray[x, y] = catPoints[i + x, j + y];
					var patch = new BSplinePatch(subArray)
					{ UPos = i, VPos = j };
					scene.AddNewModel(patch);
					ptches[j, i] = patch;

				}
			var catPointsList = new List<CatPoint>(catPoints.GetLength(0) * catPoints.GetLength(1));
			foreach (var catPoint in catPoints)
			{
				catPointsList.Add(catPoint);
			}
			scene.AddNewModel(new Surface(SurfaceType.BSpline, ptches, catPointsList, scene, IsCylinder, false) { PatchesU = VDensity, PatchesV = UDensity });


		}

		private void ConvertToNurbs(SceneData scene)
		{
			throw new NotImplementedException();
		}

		public void Convert(SceneData scene)
		{
			switch (Type)
			{
				case SurfaceType.Bezier:
					ConvertToBezierPatches(scene);
					break;
				case SurfaceType.BSpline:
					ConvertToBSpline(scene);
					break;
				case SurfaceType.Nurbs:
					ConvertToNurbs(scene);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
