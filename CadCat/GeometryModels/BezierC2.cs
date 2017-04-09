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
	class BezierC2 : BezierCurveBase, ITypeChangeable
	{


		#region Types

		private enum BezierType
		{
			Berenstein,
			BSpline
		}

		#endregion

		#region Fields

		private BezierType currentType = BezierType.BSpline;

		private List<CatPoint> berensteinPoints = new List<CatPoint>();

		private bool listenToBerensteinChanges = true;

		private ICommand changeTypeCommand;

		#endregion

		#region Properties

		public ICommand ChangeTypeCommand
		{
			get
			{
				return changeTypeCommand ?? (changeTypeCommand = new Utilities.CommandHandler(ChangeType));
			}
		}

		#endregion

		public BezierC2(IEnumerable<DataStructures.CatPoint> pts, SceneData data) : base(pts,data)
		{

			GenerateBerensteinPoints();

		}

		#region BerensteinPointsManipulation

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
			int actAmount = berensteinPoints.Count;
			int desiredAmount = (points.Count - 3) * 3 + 1;
			desiredAmount = System.Math.Max(0, desiredAmount);
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
					pt.AddAble = false;
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

		#endregion

		public override void Render(BaseRenderer renderer)
		{

			CountBezierPoints(berensteinPoints);
			base.Render(renderer);

			if (ShowPolygon)
			{
				switch (currentType)
				{
					case BezierType.Berenstein:
						renderer.Points = berensteinPoints.Select(x => x.Position).ToList();
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

		}

		protected override void AddPoint(CatPoint point, bool generateLater = true)
		{
			base.AddPoint(point, generateLater);
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

		protected override void DeleteSelectedPoints()
		{
			base.DeleteSelectedPoints();
			GenerateBerensteinPoints();
		}

		protected override void RemovePoint(PointWrapper wrapper, bool removeDelegate, bool generateLater = false)
		{
			base.RemovePoint(wrapper, removeDelegate, generateLater);
			if (!generateLater)
				GenerateBerensteinPoints();

		}

		protected override void OnPointChanged(CatPoint point)
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

					Bpt.Point.Position = (point.Position * 6 - Apt.Point.Position - Cpt.Point.Position) / 4;

					UpdateBerensteinPoints();
				}
				else
				{
					int ber = berensteinPtNumber / 3; // number of berenstein polygon
					int prev = ber + 1;
					int next = ber + 2;

					if (berensteinPtNumber % 3 == 1)
					{
						var tmp = prev;
						prev = next;
						next = tmp;
					}

					var prevPt = points[prev];
					var nextPt = points[next];

					nextPt.Point.Position = prevPt.Point.Position + (point.Position - prevPt.Point.Position) * (3 / 2.0f);

				}


				listenToBerensteinChanges = true;
			}
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
