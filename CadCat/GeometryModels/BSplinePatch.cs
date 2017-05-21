using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Navigation;
using CadCat.DataStructures;
using CadCat.Math;
using CadCat.Rendering;
using Microsoft.Win32;

namespace CadCat.GeometryModels
{
	class BSplinePatch : Patch
	{
		private readonly CatPoint[] points = new CatPoint[16];
		private readonly List<Vector3> mesh = new List<Vector3>();
		private readonly List<Vector3> normals = new List<Vector3>();
		private readonly List<int> meshIndices = new List<int>();
		private readonly List<int> normalindices = new List<int>();
		private readonly SceneData scene;

		private readonly bool owner;


		private static readonly List<int> Indices = new List<int>
		{
			0,1,
			1,2,
			2,3,

			4,5,
			5,6,
			6,7,

			8,9,
			9,10,
			10,11,

			12,13,
			13,14,
			14,15,

			0,4,
			4,8,
			8,12,

			1,5,
			5,9,
			9,13,

			2,6,
			6,10,
			10,14,

			3,7,
			7,11,
			11,15
		};

		public BSplinePatch(SceneData scene)
		{
			this.scene = scene;

			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
				{
					var pt = scene.CreateHiddenCatPoint(new Math.Vector3(i, System.Math.Sin(Math.Utils.Pi * (i * 0.5 + j * 0.1) / 2.0), j));
					points[i + j * 4] = pt;
					pt.OnChanged += OnBezierPointChanged;
				}
			changed = true;
			owner = true;
		}

		public BSplinePatch(CatPoint[,] pts)
		{
			Debug.Assert(points.Length == 16);

			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
				{
					points[j * 4 + i] = pts[i, j]; // i -U, j- V
					pts[i, j].OnChanged += OnBezierPointChanged;
					pts[i, j].OnReplace += OnBezierPointReplaced;
				}
			changed = true;
			owner = false;
		}

		private void OnBezierPointChanged(CatPoint point)
		{
			changed = true;
		}

		private void OnBezierPointReplaced(CatPoint point, CatPoint newPoint)
		{
			if (point != newPoint)
			{

				for (int i = 0; i < points.Length; i++)
					if (points[i] == point)
						points[i] = newPoint;
				changed = true;
				point.OnChanged -= OnBezierPointChanged;
				newPoint.OnChanged += OnBezierPointChanged;
				newPoint.OnReplace += OnBezierPointReplaced;
			}

		}



		public override void Render(BaseRenderer renderer)
		{

			if (changed)
				RecalculatePoints();
			base.Render(renderer);

			renderer.ModelMatrix = GetMatrix(false, new Math.Vector3());
			renderer.SelectedColor = IsSelected ? Colors.LimeGreen : Colors.White;
			renderer.UseIndices = true;

			if (ShowPolygon)
			{
				renderer.Indices = Indices;
				renderer.Points = points.Select(x => x.Position).ToList();


				renderer.Transform();
				renderer.DrawLines();
			}
			if (ShowNormal)
			{
				renderer.Indices = normalindices;
				renderer.Points = normals;
				renderer.Transform();
				renderer.DrawLines();
			}


			renderer.Indices = meshIndices;
			renderer.Points = mesh;

			renderer.Transform();
			renderer.DrawLines();
		}

		private void RecalculatePoints()
		{
			mesh.Clear();
			mesh.Capacity = System.Math.Max(mesh.Capacity, (WidthDiv + 1) * (HeightDiv + 1));
			normals.Clear();
			normals.Capacity = System.Math.Max(normals.Capacity, (WidthDiv + 1) * (HeightDiv + 1) * 2);
			meshIndices.Clear();
			normalindices.Clear();
			double widthStep = 1.0 / WidthDiv;
			double heightStep = 1.0 / HeightDiv;
			int widthPoints = WidthDiv + 1;
			int heightPoints = HeightDiv + 1;

			for (int i = 0; i < widthPoints; i++)
				for (int j = 0; j < heightPoints; j++)
				{
					var pt = EvaluatePointValue(i * widthStep, j * heightStep);
					var normal = Vector3.CrossProduct(EvaluateUDerivative(i * widthStep, j * heightStep),
						EvaluateVDerivative(i * widthStep, j * heightStep));
					mesh.Insert(i * heightPoints + j, pt);
					normals.Insert((i * heightPoints + j) * 2, pt);
					normals.Insert((i * heightPoints + j) * 2 + 1, pt + normal);
					normalindices.Add((i * heightPoints + j) * 2);
					normalindices.Add((i * heightPoints + j) * 2 + 1);
				}

			for (int i = 0; i < widthPoints - 1; i++)
				for (int j = 0; j < heightPoints - 1; j++)
				{
					meshIndices.Add(i * heightPoints + j);
					meshIndices.Add(i * heightPoints + j + 1);
					meshIndices.Add(i * heightPoints + j);
					meshIndices.Add((i + 1) * heightPoints + j);
				}
			for (int i = 0; i < widthPoints - 1; i++)
			{
				meshIndices.Add(heightPoints * (i + 1) - 1);
				meshIndices.Add(heightPoints * (i + 2) - 1);
			}
			for (int j = 0; j < heightPoints - 1; j++)
			{
				meshIndices.Add((widthPoints - 1) * heightPoints + j);
				meshIndices.Add((widthPoints - 1) * heightPoints + j + 1);
			}
			changed = false;
		}

		private static double[,] temp = new double[2, 4];
		private static Matrix4 tempMtx;
		private Vector3 EvaluatePointValue(double u, double v)
		{

			var uVal = EvaluateBSpline(u, 3);
			var vVal = EvaluateBSpline(v, 3);
			tempMtx = uVal.MatrixMultiply(vVal);


			return Sum();
		}

		private Vector3 Sum()
		{
			var sum = new Vector3();
			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
				{
					sum += points[i + j * 4].Position * tempMtx[i, j];
				}

			return sum;
		}

		private Vector3 EvaluateUDerivative(double u, double v)
		{
			var uVal = EvaluateBSpline(u, 2);// wyniki w xyz
			var vVal = EvaluateBSpline(v, 3);
			tempMtx = uVal.MatrixMultiply(vVal);

			var sum = new Vector3();
			for (int i = 0; i < 3; i++)
				for (int j = 0; j < 4; j++)
				{
					sum += (points[(i + 1) + j * 4].Position - points[i + j * 4].Position) * tempMtx[i, j];
				}

			return sum * 1;
		}

		private Vector3 EvaluateVDerivative(double u, double v)
		{
			var uVal = EvaluateBSpline(u, 3);
			var vVal = EvaluateBSpline(v, 2);// wyniki w xyz
			tempMtx = uVal.MatrixMultiply(vVal);

			var sum = new Vector3();
			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 3; j++)
				{
					sum += (points[i + (j + 1) * 4].Position - points[i + j * 4].Position) * tempMtx[i, j];
				}

			return sum * 1;
		}

		private Vector4 EvaluateBSpline(double t, int degree)
		{
			var N = new Vector4 { [0] = 1.0 };
			double tm = 1.0 - t;
			for (int j = 1; j <= degree; j++)
			{
				double saved = 0.0;
				for (int k = 1; k <= j; k++)
				{
					double term = N[k - 1] / ((tm + k - 1.0) + (t + j - k));
					N[k - 1] = saved + (tm + k - 1.0) * term;
					saved = (t + j - k) * term;
				}
				N[j] = saved;
			}

			return N;
		}

		public override void CleanUp()
		{
			base.CleanUp();
			if (owner)
			{
				foreach (var pt in points)
				{
					scene.RemovePoint(pt);
				}
			}
		}

		public override IEnumerable<CatPoint> EnumerateCatPoints()
		{
			return points;
		}

		public override string GetName()
		{
			return "BSpline patch " + base.GetName();
		}

		public override Vector3 GetPoint(double u, double v)
		{
			return EvaluatePointValue(u, v);
		}

		public override Vector3 GetUDerivative(double u, double v)
		{
			return EvaluateUDerivative(u, v);
		}

		public override Vector3 GetVDerivative(double u, double v)
		{
			return EvaluateVDerivative(u, v);
		}
	}
}
