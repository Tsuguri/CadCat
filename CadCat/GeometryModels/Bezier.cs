using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using CadCat.Math;
using CadCat.ModelInterfaces;
using System.Windows.Input;

namespace CadCat.GeometryModels
{
	class Bezier : PointModel, IChangeablePointCount
	{
		private class BezierPoints
		{
			public int PointsCount = 0;
			public Vector3 p0;
			public Vector3 p1;
			public Vector3 p2;
			public Vector3 p3;

		}

		private List<int> curveSizes;

		SceneData scene;

		private List<Vector3> curvePoints;

		public bool ShowPolygon { get; set; } = true;

		//private bool changed = false;

		private ICommand deletePointsCommand;

		public ICommand DeletePointsCommand
		{
			get
			{
				return deletePointsCommand ?? (deletePointsCommand = new Utilities.CommandHandler(DeleteSelectedPoints));
			}
		}

		public Bezier(IEnumerable<DataStructures.CatPoint> pts, SceneData data)
		{
			foreach (var p in pts)
			{
				AddPoint(p);

			}
			scene = data;
		}

		private void CountBezierPoints()
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
			int max = points.Count;
			int current = 0;
			while (current + 4 <= max)
			{
				bp.p0 = points[current].Point.Position;
				bp.p1 = points[current + 1].Point.Position;
				bp.p2 = points[current + 2].Point.Position;
				bp.p3 = points[current + 3].Point.Position;

				var pts = points.Skip(current).Take(4).Select(x => x.Point.Position).Select(x => (cameraMatrix * new Vector4(x, 1.0)).ToNormalizedVector3()).ToList();
				var xMin = pts.Select(x => x.X).Min();
				var yMin = pts.Select(x => x.Y).Min();
				var xMax = pts.Select(x => x.X).Max();
				var yMax = pts.Select(x => x.Y).Max();
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
					bp.p0 = points[current].Point.Position;
					bp.p1 = points[current + 1].Point.Position;
					bp.p2 = points[current + 2].Point.Position;

					for (int i = 0; i <= curveDivision; i++)
						lambda2(i / (double)curveDivision);
				}
				else
				{
					curvePoints.Add(points[current].Point.Position);
					curvePoints.Add(points[current + 1].Point.Position);
				}

			}


		}

		public override IEnumerable<Line> GetLines()
		{
			var line = new Line();
			if (ShowPolygon)
				for (int i = 0; i < points.Count - 1; i++)
				{
					line.from = points[i].Point.Position;
					line.to = points[i + 1].Point.Position;
					yield return line;
				}

			//if (changed)
			//{
			//	changed = false;
			curveSizes = null;
			CountBezierPoints();
			//}
			for (int i = 0; i < curvePoints.Count - 1; i++)
			{
				line.from = curvePoints[i];
				line.to = curvePoints[i + 1];
				yield return line;
			}
		}

		public void AddPoint(CatPoint point)
		{
			point.OnDeleted += OnPointDeleted;
			point.OnChanged += OnPointChanged;
			points.Add(new PointWrapper(point));
		//	changed = true;
		}

		public void RemovePoint(CatPoint point)
		{
			RemovePoint(point, true);

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
			//changed = true;
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
			//changed = true;
		}

		public override Matrix4 GetMatrix(bool overrideScale, Vector3 newScale)
		{
			return Matrix4.CreateIdentity();
		}

		public override string GetName()
		{
			return "Bezier " + base.GetName();
		}

	}
}
