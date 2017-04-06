using CadCat.ModelInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using CadCat.Math;
using System.Windows.Input;
using CadCat.Rendering;
using System.Windows.Media;

namespace CadCat.GeometryModels
{
	class BezierC2 : PointModel, IChangeablePointCount, ITypeChangeable
	{
		private class BezierPoints
		{
			public int PointsCount = 0;
			public Vector3 p0;
			public Vector3 p1;
			public Vector3 p2;
			public Vector3 p3;

		}

		private enum BezierType
		{
			Berenstein,
			BSpline
		}

		private BezierType currentType = BezierType.Berenstein;

		SceneData scene;

		private List<Vector3> curvePoints;
		private List<Vector3> berensteinPoints;

		public bool ShowPolygon { get; set; } = true;

		public BezierC2(IEnumerable<DataStructures.CatPoint> pts, SceneData data)
		{
			foreach (var p in pts)
			{
				AddPoint(p);
			}
			scene = data;
		}

		private void CountBezierPoints(List<Vector3> pts)
		{
			int curveDivision = 10;
			curvePoints = new List<Vector3>();
			var bp = new BezierPoints();
			Vector3 tempVec = new Vector3();
			Action<double> lambda = (x) =>
			{
				double x2 = x * x;
				double x3 = x2 * x;
				double x11 = (1 - x);
				double x12 = x11 * (1 - x);
				double x13 = x12 * (1 - x);

				tempVec.X = bp.p0.X * x13 + 3 * bp.p1.X * x12 * x
					+ 3 * bp.p2.X * x2 * x11 + bp.p3.X * x3;

				tempVec.Y = bp.p0.Y * x13 + 3 * bp.p1.Y * x12 * x
					+ 3 * bp.p2.Y * x2 * x11 + bp.p3.Y * x3;

				tempVec.Z = bp.p0.Z * x13 + 3 * bp.p1.Z * x12 * x
					+ 3 * bp.p2.Z * x2 * x11 + bp.p3.Z * x3;

				curvePoints.Add(tempVec);
			};
			Action<double> lambda2 = (x) =>
			{
				double x2 = x * x;
				double x11 = (1 - x);
				double x12 = x11 * (1 - x);

				tempVec.X = bp.p0.X * x12 + 2 * bp.p1.X * x11 * x
					+ bp.p2.X * x2;

				tempVec.Y = bp.p0.Y * x12 + 2 * bp.p1.Y * x11 * x
					+ bp.p2.Y * x2;

				tempVec.Z = bp.p0.Z * x12 + 2 * bp.p1.Z * x11 * x
					+ bp.p2.Z * x2;

				curvePoints.Add(tempVec);
			};
			var cameraMatrix = scene.ActiveCamera.ViewProjectionMatrix;
			int max = pts.Count;
			int current = 0;
			while (current + 4 <= max)
			{
				bp.p0 = pts[current];
				bp.p1 = pts[current + 1];
				bp.p2 = pts[current + 2];
				bp.p3 = pts[current + 3];

				var rectPts = pts.Skip(current).Take(4).Select(x => (cameraMatrix * new Vector4(x, 1.0)).ToNormalizedVector3()).ToList();
				var xMin = rectPts.Select(x => x.X).Min();
				var yMin = rectPts.Select(x => x.Y).Min();
				var xMax = rectPts.Select(x => x.X).Max();
				var yMax = rectPts.Select(x => x.Y).Max();
				var size = new Vector2(xMax - xMin, yMax - yMin);
				size.X = size.X * scene.ScreenSize.X;
				size.Y = size.Y * scene.ScreenSize.Y;
				curveDivision = (int)(System.Math.Max(size.X, size.Y) / 5);
				curveDivision = System.Math.Min(curveDivision, 500);

				for (int i = 0; i <= curveDivision; i++)
					lambda(i / (double)curveDivision);
				current += 3;
			}
			if (current < max - 1)
			{
				if (max - 1 - current == 2)
				{
					bp.p0 = pts[current];
					bp.p1 = pts[current + 1];
					bp.p2 = pts[current + 2];

					for (int i = 0; i <= curveDivision; i++)
						lambda2(i / (double)curveDivision);
				}
				else
				{
					curvePoints.Add(pts[current]);
					curvePoints.Add(pts[current + 1]);
				}

			}


		}

		private void GenerateBerensteinPoints()
		{
			berensteinPoints = new List<Vector3>();

			for (int i = 0; i < points.Count - 3; i++)
			{
				var ptLeft = Vector3.Lerp(points[i].Point.Position, points[i + 1].Point.Position, 2 / 3.0);
				var point2 = Vector3.Lerp(points[i + 1].Point.Position, points[i + 2].Point.Position, 1 / 3.0);
				var point3 = Vector3.Lerp(points[i + 1].Point.Position, points[i + 2].Point.Position, 2 / 3.0);
				var ptRight = Vector3.Lerp(points[i + 2].Point.Position, points[i + 3].Point.Position, 1 / 3.0);
				var point4 = (point3 + ptRight) / 2;

				if (i == 0)
				{
					var point1 = (ptLeft + point2) / 2;
					berensteinPoints.Add(point1);
				}
				berensteinPoints.Add(point2);
				berensteinPoints.Add(point3);
				berensteinPoints.Add(point4);

			}
		}

		public override void Render(BaseRenderer renderer)
		{
			base.Render(renderer);

			GenerateBerensteinPoints();
			CountBezierPoints(berensteinPoints);

			renderer.ModelMatrix = Matrix4.CreateIdentity();
			renderer.UseIndices = false;
			if (ShowPolygon)
			{
				renderer.Points = points.Select(x => x.Point.Position).ToList();
				renderer.Transform();
				renderer.DrawLines();

				if (currentType == BezierType.Berenstein)
				{
					renderer.Points = berensteinPoints;
					renderer.Transform();
					renderer.DrawLines();
					renderer.SelectedColor = Colors.SkyBlue;
					renderer.DrawPoints();
				}


			}
			renderer.SelectedColor = !IsSelected ? Colors.White : Colors.LightGreen;

			renderer.Points = curvePoints;
			renderer.Transform();
			renderer.DrawLines();
		}

		public void AddPoint(CatPoint point)
		{
			point.OnDeleted += OnPointDeleted;
			point.OnChanged += OnPointChanged;
			points.Add(new PointWrapper(point));
		}

		public void RemovePoint(CatPoint point)
		{
			RemovePoint(point, true);

		}

		public void ChangeType()
		{
			switch (currentType)
			{
				case BezierType.Berenstein:
					currentType = BezierType.BSpline;
					break;
				case BezierType.BSpline:
					currentType = BezierType.Berenstein;
					break;
				default:
					break;
			}
		}

		private void DeleteSelectedPoints()
		{
			var list = points.Where((x) => x.IsSelected).ToList();
			foreach (var point in list)
				RemovePoint(point, true);
		}

		private void RemovePoint(PointWrapper wrapper, bool removeDelegate)
		{
			if (removeDelegate)
			{
				wrapper.Point.OnDeleted -= OnPointDeleted;
			}
			wrapper.Point.OnChanged -= OnPointChanged;
			points.Remove(wrapper);
		}

		public void RemovePoint(CatPoint point, bool removeDelegate)
		{
			var pt = points.Where((x) => x.Point == point).FirstOrDefault();
			if (pt != null)
			{
				RemovePoint(pt, removeDelegate);
			}
		}

		private void OnPointDeleted(CatPoint point)
		{
			RemovePoint(point, false);
		}

		private void OnPointChanged(CatPoint point)
		{
		}

		public override Matrix4 GetMatrix(bool overrideScale, Vector3 newScale)
		{
			return Matrix4.CreateIdentity();
		}

		public override string GetName()
		{
			return "Bezier C2 " + base.GetName();
		}
	}
}
