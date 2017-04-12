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
	abstract class BezierCurveBase : PointModel, IChangeablePointCount
	{

		#region Types

		protected class BezierPoints
		{
			public int PointsCount = 0;
			public Vector3 p0;
			public Vector3 p1;
			public Vector3 p2;
			public Vector3 p3;

		}

		#endregion

		#region Fields

		protected SceneData scene;
		protected List<Vector3> curvePoints;

		private ICommand deletePointsCommand;


		#endregion

		#region Properties

		public bool ShowPolygon { get; set; } = true;

		#endregion

		#region Commands

		public ICommand DeletePointsCommand
		{
			get
			{
				return deletePointsCommand ?? (deletePointsCommand = new Utilities.CommandHandler(DeleteSelectedPoints));
			}
		}

		#endregion

		#region Constructor

		protected BezierCurveBase(IEnumerable<CatPoint> pts, SceneData scene)
		{
			this.scene = scene;
			foreach (var p in pts)
			{
				AddPoint(p);
			}
		}

		#endregion

		#region IChangeablePointCount

		public virtual void AddPoint(CatPoint point)
		{
			AddPoint(point, false);
		}

		public virtual void RemovePoint(CatPoint point)
		{
			RemovePoint(point, true);
		}

		#endregion


		#region DeleteSelected

		protected virtual void DeleteSelectedPoints()
		{
			var list = points.Where((x) => x.IsSelected).ToList();
			foreach (var point in list)
				RemovePoint(point, true, true);
		}

		#endregion

		#region BezierComputations

		protected void CountBezierPoints(List<Vector3> pts)
		{
			int curveDivision = 10;
			curvePoints = new List<Vector3>();
			var bp = new BezierPoints();
			Vector3 tempVec = new Vector3();
			int current = 0;
			var cameraMatrix = scene.ActiveCamera.ViewProjectionMatrix;

			Action<int> GetSize = (y) =>
			 {
				 var rectPts = pts.Skip(current).Take(y).Select(x => (cameraMatrix * new Vector4(x, 1.0)).ToNormalizedVector3()).ToList();
				 var xMin = rectPts.Select(x => x.X).Min();
				 var yMin = rectPts.Select(x => x.Y).Min();
				 var xMax = rectPts.Select(x => x.X).Max();
				 var yMax = rectPts.Select(x => x.Y).Max();
				 var size = new Vector2(xMax - xMin, yMax - yMin);
				 size.X = size.X * scene.ScreenSize.X;
				 size.Y = size.Y * scene.ScreenSize.Y;
				 curveDivision = (int)(System.Math.Max(size.X, size.Y) / 5);
				 curveDivision = System.Math.Min(curveDivision, 500);
			 };

			Action<double> Berenstein4Points = (x) =>
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
			Action<double> Berenstein3Points = (x) =>
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
			int max = pts.Count;
			while (current + 4 <= max)
			{
				bp.p0 = pts[current];
				bp.p1 = pts[current + 1];
				bp.p2 = pts[current + 2];
				bp.p3 = pts[current + 3];

				GetSize(4);

				for (int i = 0; i <= curveDivision; i++)
					Berenstein4Points(i / (double)curveDivision);
				current += 3;
			}
			if (current < max - 1)
			{
				if (max - 1 - current == 2)
				{
					bp.p0 = pts[current];
					bp.p1 = pts[current + 1];
					bp.p2 = pts[current + 2];

					GetSize(3);

					for (int i = 0; i <= curveDivision; i++)
						Berenstein3Points(i / (double)curveDivision);
				}
				else
				{
					curvePoints.Add(pts[current]);
					curvePoints.Add(pts[current + 1]);
				}

			}


		}

		#endregion

		#region PointsManipulation

		protected virtual void AddPoint(CatPoint point, bool generateLater = true)
		{
			point.OnDeleted += OnPointDeleted;
			point.OnChanged += OnPointChanged;
			points.Add(new PointWrapper(point));
		}

		protected virtual void RemovePoint(PointWrapper wrapper, bool removeDelegate, bool generateLater = false)
		{
			if (removeDelegate)
			{
				wrapper.Point.OnDeleted -= OnPointDeleted;
			}
			wrapper.Point.OnChanged -= OnPointChanged;
			points.Remove(wrapper);
		}

		public virtual void RemovePoint(CatPoint point, bool removeDelegate)
		{
			var pt = points.Where((x) => x.Point == point).FirstOrDefault();
			if (pt != null)
			{
				RemovePoint(pt, removeDelegate);
			}
		}


		#endregion

		#region Render

		public override void Render(BaseRenderer renderer)
		{
			base.Render(renderer);
			renderer.ModelMatrix = Matrix4.CreateIdentity();
			renderer.UseIndices = false;

			renderer.SelectedColor = !IsSelected ? Colors.White : Colors.LightGreen;

			renderer.Points = curvePoints;
			renderer.Transform();
			renderer.DrawLines();
		}

		#endregion

		#region DelegateMethods

		protected virtual void OnPointDeleted(CatPoint point)
		{
			RemovePoint(point, false);

		}

		protected virtual void OnPointChanged(CatPoint point)
		{

		}

		#endregion

		public override Matrix4 GetMatrix(bool overrideScale, Vector3 newScale)
		{
			return Matrix4.CreateIdentity();
		}
	}
}