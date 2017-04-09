using CadCat.ModelInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using CadCat.Math;
using System.Windows.Input;

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

		protected abstract void DeleteSelectedPoints();

		#endregion

		#region BezierComputations

		protected void CountBezierPoints(List<CatPoint> pts)
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

		#endregion

		#region PointsManipulation

		protected abstract void AddPoint(CatPoint point, bool generateLater = true);

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

		#region DelegateMethods

		protected virtual void OnPointDeleted(CatPoint point)
		{
			RemovePoint(point, false);

		}

		protected abstract void OnPointChanged(CatPoint point);

		#endregion

		public override Matrix4 GetMatrix(bool overrideScale, Vector3 newScale)
		{
			return Matrix4.CreateIdentity();
		}
	}
}