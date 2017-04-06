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

		private BezierType currentType = BezierType.BSpline;

		SceneData scene;

		private List<Vector3> curvePoints;
		private List<CatPoint> berensteinPoints = new List<CatPoint>();
		private bool listenToBerensteinChanges = true;
		public bool ShowPolygon { get; set; } = true;

		public BezierC2(IEnumerable<DataStructures.CatPoint> pts, SceneData data)
		{
			scene = data;
			foreach (var p in pts)
			{

				AddPoint(p, true);
			}
			GenerateBerensteinPoints();

		}

		private ICommand deletePointsCommand;
		private ICommand changeTypeCommand;

		public ICommand DeletePointsCommand
		{
			get
			{
				return deletePointsCommand ?? (deletePointsCommand = new Utilities.CommandHandler(DeleteSelectedPoints));
			}
		}

		public ICommand ChangeTypeCommand
		{
			get
			{
				return changeTypeCommand ?? (changeTypeCommand = new Utilities.CommandHandler(ChangeType));
			}
		}

		private void CountBezierPoints(List<CatPoint> pts)
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
				bp.p0 = pts[current].Position;
				bp.p1 = pts[current + 1].Position;
				bp.p2 = pts[current + 2].Position;
				bp.p3 = pts[current + 3].Position;

				var rectPts = pts.Skip(current).Take(4).Select(x => (cameraMatrix * new Vector4(x.Position, 1.0)).ToNormalizedVector3()).ToList();
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
					bp.p0 = pts[current].Position;
					bp.p1 = pts[current + 1].Position;
					bp.p2 = pts[current + 2].Position;

					for (int i = 0; i <= curveDivision; i++)
						lambda2(i / (double)curveDivision);
				}
				else
				{
					curvePoints.Add(pts[current].Position);
					curvePoints.Add(pts[current + 1].Position);
				}

			}


		}

		private void ClearBerensteinPoints()
		{
			foreach (var point in berensteinPoints)
			{
				scene.RemovePoint(point);

			}
			berensteinPoints.Clear();
		}

		private void GenerateBerensteinPoints()
		{
			//ClearBerensteinPoints();

			int actAmount = berensteinPoints.Count;
			int desiredAmount = (points.Count - 3) * 3 + 1;
			if (actAmount > desiredAmount)
			{
				for (int i = desiredAmount; i < actAmount; i++)
					scene.RemovePoint(berensteinPoints[i]);
				berensteinPoints.RemoveRange(desiredAmount, actAmount - desiredAmount);
			}
			else if (actAmount < desiredAmount)
				for (int i = 0; i < desiredAmount - actAmount; i++)
				{
					var pt = scene.CreateCatPoint(new Vector3(), false);
					pt.OnChanged += OnBerensteinPointChanged;
					berensteinPoints.Add(pt);
					switch (currentType)
					{
						case BezierType.Berenstein:
							pt.Visible = true;
							break;
						case BezierType.BSpline:
							pt.Visible = false;
							break;
						default:
							break;
					}
				}


			UpdateBerensteinPoints();
		}

		private void UpdateBerensteinPoints()
		{
			listenToBerensteinChanges = false;
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
					berensteinPoints[i].Position = point1;
				}
				berensteinPoints[3 * i + 1].Position = point2;
				berensteinPoints[3 * i + 2].Position = point3;
				berensteinPoints[3 * i + 3].Position = point4;

			}
			listenToBerensteinChanges = true;
		}

		public override void Render(BaseRenderer renderer)
		{
			base.Render(renderer);

			CountBezierPoints(berensteinPoints);

			renderer.ModelMatrix = Matrix4.CreateIdentity();
			renderer.UseIndices = false;
			if (ShowPolygon)
			{
				switch (currentType)
				{
					case BezierType.Berenstein:
						renderer.Points = berensteinPoints.Select(x=>x.Position).ToList();
						break;
					case BezierType.BSpline:
						renderer.Points = points.Select(x => x.Point.Position).ToList();

						break;
					default:
						break;
				}
				renderer.Transform();
				renderer.DrawLines();
			}
			renderer.SelectedColor = !IsSelected ? Colors.White : Colors.LightGreen;

			renderer.Points = curvePoints;
			renderer.Transform();
			renderer.DrawLines();
		}

		public void AddPoint(CatPoint point, bool generateLater = true)
		{
			point.OnDeleted += OnPointDeleted;
			point.OnChanged += OnPointChanged;
			points.Add(new PointWrapper(point));
			switch (currentType)
			{
				case BezierType.Berenstein:
					point.Visible = false;
					break;
				case BezierType.BSpline:
					point.Visible = true;
					break;
				default:
					break;
			}
			if (!generateLater)
				GenerateBerensteinPoints();

		}

		public void AddPoint(CatPoint point)
		{
			AddPoint(point, false);
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
					foreach (var pt in berensteinPoints)
					{
						pt.Visible = false;
					}
					foreach (var pt in points)
					{
						pt.Point.Visible = true;
					}
					break;
				case BezierType.BSpline:
					currentType = BezierType.Berenstein;
					foreach (var pt in berensteinPoints)
					{
						pt.Visible = true;
					}
					foreach (var pt in points)
					{
						pt.Point.Visible = false;
					}
					break;
				default:
					break;
			}
		}

		private void DeleteSelectedPoints()
		{
			var list = points.Where((x) => x.IsSelected).ToList();
			foreach (var point in list)
				RemovePoint(point, true, true);
		}

		private void RemovePoint(PointWrapper wrapper, bool removeDelegate, bool generateLater = false)
		{
			if (removeDelegate)
			{
				wrapper.Point.OnDeleted -= OnPointDeleted;
			}
			wrapper.Point.OnChanged -= OnPointChanged;
			points.Remove(wrapper);
			if (!generateLater)
				GenerateBerensteinPoints();

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
			UpdateBerensteinPoints();
		}

		private void OnBerensteinPointChanged(CatPoint point)
		{
			if (listenToBerensteinChanges)
			{
				listenToBerensteinChanges = false;

				int berensteinPtNumber = berensteinPoints.IndexOf(point);
				if (berensteinPtNumber < 0 || berensteinPtNumber >= berensteinPoints.Count)
					throw new InvalidOperationException("Invalid berenstein point index");

				if (berensteinPtNumber % 3 == 0)
				{
					var A = berensteinPtNumber / 3;
					var B = A + 1;
					var C = A + 2;
					var Apt = points[A];
					var Bpt = points[B];
					var Cpt = points[C];

					Bpt.Point.Position = (point.Position * 6 - Apt.Point.Position - Cpt.Point.Position)/4;

					UpdateBerensteinPoints();
				}
				else
				{
					int ber = berensteinPtNumber / 3; // number of berenstein polygon
					int prev = ber + 1;
					int next = ber + 2;

					if(berensteinPtNumber % 3 ==1)
					{
						var tmp = prev;
						prev = next;
						next = tmp;
					}

					var prevPt = points[prev];
					var nextPt = points[next];

					nextPt.Point.Position = prevPt.Point.Position + (point.Position - prevPt.Point.Position) * (3/2.0f);

				}


				listenToBerensteinChanges = true;
			}
		}

		public override Matrix4 GetMatrix(bool overrideScale, Vector3 newScale)
		{
			return Matrix4.CreateIdentity();
		}

		public override string GetName()
		{
			return "Bezier C2 " + base.GetName();
		}

		public override void CleanUp()
		{
			base.CleanUp();
			foreach (var pt in berensteinPoints)
				scene.RemovePoint(pt);
		}


	}
}
