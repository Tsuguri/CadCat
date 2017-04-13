using System.Collections.Generic;
using System.Linq;
using CadCat.DataStructures;
using CadCat.Rendering;
using CadCat.Math;

namespace CadCat.GeometryModels
{
	class BsplineInterpolator : BezierCurveBase
	{
		bool changed = false;

		private List<Vector3> berensteinPoints;
		private List<Vector3> renderPoints;
		public BsplineInterpolator(IEnumerable<CatPoint> points, SceneData scene) : base(points, scene)
		{
			changed = true;
			ShowPolygon = true;
		}

		private void CalculateWhatever()
		{
			if (!changed)
				return;
			changed = false;
			if (points.Count < 3)
			{
				berensteinPoints?.Clear();
				return;
			}
			var pts = points.Select(pt => pt.Point.Position).ToList();
			var distances = new double[pts.Count - 1];
			var knots = new double[pts.Count];

			double sum = 0;
			for (int i = 0; i < pts.Count - 1; i++)
			{
				distances[i] = (pts[i + 1] - pts[i]).Length();
				knots[i] = sum;
				sum += distances[i];
			}
			knots[knots.Length - 1] = sum;

			if (pts.Count < 4)
			{
				berensteinPoints = new List<Vector3>();
				if (pts.Count == 3)
				{
					berensteinPoints.Add(pts[0]);
					berensteinPoints.Add(pts[1]);
					berensteinPoints.Add(pts[1]);
					berensteinPoints.Add(pts[2]);
				}
				else if (pts.Count == 2)
				{
					berensteinPoints.Add(pts[0]);
					berensteinPoints.Add(pts[1]);
				}
				return;
			}

			var alphas = new double[pts.Count - 3];
			var betas = new double[pts.Count - 3];
			var eras = new Vector3[pts.Count - 2];
			int j = 0;
			for (j = 1; j < pts.Count - 2; j++)
			{
				alphas[j - 1] = distances[j] / (distances[j] + distances[j + 1]);
				betas[j - 1] = distances[j] / (distances[j] + distances[j - 1]);

				eras[j - 1] = (((pts[j + 1] - pts[j]) / distances[j]) - ((pts[j] - pts[j - 1]) / distances[j - 1])) * 3 / (distances[j] + distances[j - 1]);
			}

			eras[j - 1] = (((pts[j + 1] - pts[j]) / distances[j]) - ((pts[j] - pts[j - 1]) / distances[j - 1])) * 3 / (distances[j] + distances[j - 1]); ;
			var diagonal = new double[pts.Count - 2];
			for (j = 0; j < diagonal.Length; j++)
				diagonal[j] = 2;

			var results = Utils.SolveMultidiagonalMatrixEquation(alphas, diagonal, betas, eras);


			var a = pts.ToArray();
			var b = new Vector3[pts.Count - 1];
			var c = new Vector3[pts.Count];
			var d = new Vector3[pts.Count - 1];

			results.ToArray().CopyTo(c, 1);

			for (int i = 0; i < d.Length; i++)
			{
				d[i] = (c[i + 1] - c[i]) / (3.0);
			}

			for (int i = 0; i < b.Length; i++)
				b[i] = (a[i + 1] - a[i]) / distances[i] - (c[i] + d[i]) * distances[i];

			berensteinPoints = new List<Vector3>();
			for (int i = 0; i < Points.Count - 1; i++)
			{
				var ptA = a[i];
				var ptB = b[i] * distances[i];
				var ptC = c[i] * distances[i] * distances[i];
				var ptD = d[i] * distances[i] * distances[i];

				PowerToBerenStein(ptA, ptB, ptC, ptD, out ptA, out ptB, out ptC, out ptD);

				if (i == 0)
					berensteinPoints.Add(ptA);
				berensteinPoints.Add(ptB);
				berensteinPoints.Add(ptC);
				berensteinPoints.Add(ptD);
			}




			int x = 0;

		}

		private void PowerToBerenStein(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out Vector3 newA, out Vector3 newB, out Vector3 newC, out Vector3 newD)
		{
			newA = a;
			newB = a + b * (1 / 3.0);
			newC = a + b * (2 / 3.0) + c * (1 / 3.0);
			newD = a + b + c + d;
		}

		protected override void AddPoint(CatPoint point, bool generateLater = true)
		{
			base.AddPoint(point, generateLater);
			changed = true;
		}

		protected override void OnPointChanged(CatPoint point)
		{
			changed = true;
		}

		protected override void OnPointDeleted(CatPoint point)
		{
			base.OnPointDeleted(point);
			changed = true;
		}

		private void RemovePoint(PointWrapper point, bool removeDelegate)
		{
			base.RemovePoint(point, removeDelegate);
			changed = true;
		}

		public override void RemovePoint(CatPoint point)
		{
			base.RemovePoint(point);
			changed = true;
		}
		protected override void RemovePoint(PointWrapper wrapper, bool removeDelegate, bool generateLater = false)
		{
			base.RemovePoint(wrapper, removeDelegate, generateLater);
			changed = true;
		}

		public override void RemovePoint(CatPoint point, bool removeDelegate)
		{
			base.RemovePoint(point, removeDelegate);
			changed = true;
		}

		public override string GetName()
		{
			return "BSplineInterpolator" + base.GetName();
		}

		public override void Render(BaseRenderer renderer)
		{
			CalculateWhatever();
			if (berensteinPoints != null && berensteinPoints.Count > 1)
			{
				CountBezierPoints(berensteinPoints);
				base.Render(renderer);

			}


			if (ShowPolygon && berensteinPoints != null && berensteinPoints.Count > 1)
			{

				renderer.Points = berensteinPoints;
				renderer.Transform();
				renderer.DrawLines();
			}
			//renderer.Points = renderPoints;
			//renderer.UseIndices = false;
			//renderer.Transform();
			//renderer.DrawLines();


		}
	}
}
