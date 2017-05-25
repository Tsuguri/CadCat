using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using CadCat.DataStructures;
using CadCat.Math;
using CadCat.Rendering;

namespace CadCat.GeometryModels
{
	class BSplinePatch : Patch
	{
		private readonly CatPoint[] points = new CatPoint[16];
		private readonly CatPoint[,] pointsOrdererd = new CatPoint[4, 4];
		private List<Vector2> parametrizationPoints;
		private List<Vector3> mesh = new List<Vector3>();
		private readonly List<Vector3> normals = new List<Vector3>();
		private List<int> meshIndices = new List<int>();
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
					var pt = scene.CreateHiddenCatPoint(new Vector3(i, System.Math.Sin(Utils.Pi * (i * 0.5 + j * 0.1) / 2.0), j));
					points[i + j * 4] = pt;
					pt.OnChanged += OnBezierPointChanged;
				}
			ParametrizationChanged = true;
			Changed = true;
			owner = true;
		}

		public BSplinePatch(CatPoint[,] pts)
		{
			Debug.Assert(points.Length == 16);

			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
				{
					points[i * 4 + j] = pts[i, j]; // i -U, j- V
					pts[i, j].OnChanged += OnBezierPointChanged;
					pts[i, j].OnReplace += OnBezierPointReplaced;
				}

			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
				{
					pointsOrdererd[i, j] = pts[i, j];
				}
			ParametrizationChanged = true;
			Changed = true;
			owner = false;
		}

		private void OnBezierPointChanged(CatPoint point)
		{
			Changed = true;
		}

		private void OnBezierPointReplaced(CatPoint point, CatPoint newPoint)
		{
			if (point != newPoint)
			{

				for (int i = 0; i < points.Length; i++)
					if (points[i] == point)
						points[i] = newPoint;
				ParametrizationChanged = true;
				Changed = true;
				point.OnChanged -= OnBezierPointChanged;
				newPoint.OnChanged += OnBezierPointChanged;
				newPoint.OnReplace += OnBezierPointReplaced;
			}

		}



		public override void Render(BaseRenderer renderer)
		{
			if (ParametrizationChanged)
				RecalculateParametrizationPoints();
			if (Changed)
				RecalculatePoints();
			base.Render(renderer);

			renderer.ModelMatrix = GetMatrix(false, new Vector3());
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
			mesh = parametrizationPoints.Select(x => GetPoint(x.X, x.Y)).ToList();
			Changed = false;

		}

		private void RecalculateParametrizationPoints()
		{
			var avai = Surface.GetAvaiablePatch(UPos, VPos, WidthDiv, HeightDiv);

			Func<Vector2, bool> check = vector2 => Surface.IsPointAvaiable(UPos, VPos, vector2);
			var aszk = SurfaceFilling.MarchingAszklars(avai, 1, 1, false, false, check);

			meshIndices = aszk.Item2;
			parametrizationPoints = aszk.Item1;
			ParametrizationChanged = false;
			Changed = true;
		}

		private static Matrix4 _tempMtx;
		private Vector3 EvaluatePointValue(double u, double v)
		{

			var uVal = EvaluateBSpline(u, 3);
			var vVal = EvaluateBSpline(v, 3);
			_tempMtx = uVal.MatrixMultiply(vVal);


			return Sum();
		}

		private Vector3 Sum()
		{
			var sum = new Vector3();
			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
				{
					sum += pointsOrdererd[j, i].Position * _tempMtx[i, j];
				}

			return sum;
		}

		private Vector3 EvaluateUDerivative(double u, double v)
		{
			var uVal = EvaluateBSpline(u, 2);// wyniki w xyz
			var vVal = EvaluateBSpline(v, 3);
			_tempMtx = uVal.MatrixMultiply(vVal);

			var sum = new Vector3();
			for (int i = 0; i < 3; i++)
				for (int j = 0; j < 4; j++)
				{
					sum += (pointsOrdererd[j, i + 1].Position - pointsOrdererd[j, i].Position) * _tempMtx[i, j];
				}

			return sum * 1;
		}

		private Vector3 EvaluateVDerivative(double u, double v)
		{
			var uVal = EvaluateBSpline(u, 3);
			var vVal = EvaluateBSpline(v, 2);// wyniki w xyz
			_tempMtx = uVal.MatrixMultiply(vVal);

			var sum = new Vector3();
			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 3; j++)
				{
					sum += (pointsOrdererd[j + 1, i].Position - pointsOrdererd[j, i].Position) * _tempMtx[i, j];
				}

			return sum * 1;
		}

		private Vector4 EvaluateBSpline(double t, int degree)
		{
			var n = new Vector4 { [0] = 1.0 };
			double tm = 1.0 - t;
			for (int j = 1; j <= degree; j++)
			{
				double saved = 0.0;
				for (int k = 1; k <= j; k++)
				{
					double term = n[k - 1] / ((tm + k - 1.0) + (t + j - k));
					n[k - 1] = saved + (tm + k - 1.0) * term;
					saved = (t + j - k) * term;
				}
				n[j] = saved;
			}

			return n;
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

		public override CatPoint GetCatPoint(int u, int v)
		{
			//points[j * 4 + i] = pts[i, j]
			return pointsOrdererd[v, u];
		}
	}
}
