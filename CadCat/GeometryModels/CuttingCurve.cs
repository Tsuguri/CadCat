using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using CadCat.DataStructures;
using CadCat.Math;
using CadCat.ModelInterfaces;
using CadCat.Rendering;
using CadCat.Utilities;

namespace CadCat.GeometryModels
{
	public class CuttingCurve : Model
	{
		private IIntersectable Q;
		private IIntersectable P;
		private List<Vector4> points;
		private readonly List<CatPoint> catPoints;
		private readonly SceneData scene;
		private readonly List<int> indices = new List<int>();
		private bool cyclic;
		private ICommand convertToInterpolationCurve;

		public bool IsIntersectableP { get; private set; }
		private List<Vector2> qPolygon;
		private List<Vector2> pPolygon;
		private List<Tuple<Vector2, Vector2>> qPolygonBoundary;
		private List<Tuple<Vector2, Vector2>> pPolygonBoundary;
		private bool[,] PtestSet;
		private bool[,] QtestSet;
		public bool IsIntersectableQ { get; private set; }

		private bool showPoints = true;

		public bool ShowPoints
		{
			get { return showPoints; }
			set
			{
				if (showPoints != value)
				{
					showPoints = value;
					catPoints.ForEach(x => x.Visible = value);
				}
			}
		}

		public ICommand ConvertToInterpolation => convertToInterpolationCurve ?? (convertToInterpolationCurve = new CommandHandler(Convert));

		public CuttingCurve(List<Vector4> points, IIntersectable P, IIntersectable Q, SceneData scene, bool cyclic, double minimumStep = 0.1)
		{
			this.Q = Q;
			this.P = P;
			this.scene = scene;
			this.cyclic = cyclic;

			this.points = new List<Vector4>(points.Count / 2);
			Vector3 prevPoint = P.GetPosition(points[0].X, points[0].Y);

			for (int i = 1; i < points.Count; i++)
			{
				var actualPoint = P.GetPosition(points[i].X, points[i].Y);
				if ((actualPoint - prevPoint).Length() > minimumStep)
				{
					prevPoint = actualPoint;
					this.points.Add(points[i]);
				}
			}
			if (this.points.Count < 2)
				this.points.Add(points[points.Count - 1]);

			catPoints = new List<CatPoint>(this.points.Count);
			foreach (var point in this.points)
			{
				var pos = P.GetPosition(point.X, point.Y);
				catPoints.Add(scene.CreateHiddenCatPoint(pos));
			}
			indices.Clear();
			indices.Add(0);
			for (int i = 1; i < this.points.Count - 1; i++)
			{
				indices.Add(i);
				indices.Add(i);
			}
			indices.Add(this.points.Count - 1);
			Console.WriteLine($"Cyclic: {cyclic}");
			if (cyclic)
			{
				indices.Add(this.points.Count - 1);
				indices.Add(0);
			}

			IsIntersectableP = CalculatePolygon(true, out pPolygon, out pPolygonBoundary);
			IsIntersectableQ = CalculatePolygon(false, out qPolygon, out qPolygonBoundary);


			if (IsIntersectableQ)
			{
				QtestSet = new bool[100, 100];

				for (int i = 0; i < 100; i++)
					for (int j = 0; j < 100; j++)
						QtestSet[i, j] = true;
				PointsContainedByCurve(QtestSet, true, Q, 0, Q.FirstParamLimit, 0, Q.SecondParamLimit, true);
			}

		}

		public override void CleanUp()
		{
			base.CleanUp();
			foreach (var point in catPoints)
			{
				scene.RemovePoint(point);
			}
			P?.RemoveCurve(this);
			Q?.RemoveCurve(this);
		}

		public override void Render(BaseRenderer renderer)
		{
			base.Render(renderer);
			renderer.Points = catPoints.Select(x => x.Position).ToList();
			renderer.Indices = indices;
			renderer.UseIndices = true;
			renderer.ModelMatrix = Matrix4.CreateIdentity();
			renderer.SelectedColor = IsSelected ? Colors.DarkViolet : Colors.White;

			renderer.Transform();
			renderer.DrawLines();
		}

		public override string GetName()
		{
			return "Cutting curve" + base.GetName();
		}

		private void Convert()
		{
			var cnt = points.Count / 5.0;
			var pts = new List<Vector4>();
			for (int i = 0; i < cnt; i++)
			{
				pts.Add(points[i * 5]);
			}
			if (cyclic)
				pts.Add(points[0]);

			var interp = new BsplineInterpolator(pts.Select(x => scene.CreateCatPoint(P.GetPosition(x.X, x.Y))), scene);
			scene.AddNewModel(interp);
			//scene.RemoveModel(this);
		}

		public void Draw(WriteableBitmap bitmap)
		{
			bitmap.Clear(Colors.White);
			double width = bitmap.Width;
			double halfWidth = width / 2;
			width -= 1;
			double height = bitmap.Height;
			double uParam = P.FirstParamLimit;
			double vParam = P.SecondParamLimit;
			double sParam = Q.FirstParamLimit;
			double tParam = Q.SecondParamLimit;
			using (bitmap.GetBitmapContext())
			{
				if (pPolygon != null)
				{
					var minSize = System.Math.Min(P.FirstParamLimit, P.SecondParamLimit) * 0.8;
					for (int i = 0; i < pPolygon.Count - 1 + (cyclic ? 1 : 0); i++)
					{
						var item1 = pPolygon[i];
						var item2 = pPolygon[(i + 1) % pPolygon.Count];
						if (System.Math.Abs(item1.X - item2.X) < minSize && System.Math.Abs(item1.Y - item2.Y) < minSize)
							bitmap.DrawLineDDA((int)(item1.X / uParam * halfWidth), (int)(item1.Y / vParam * height), (int)(item2.X / uParam * halfWidth), (int)(item2.Y / vParam * height), Colors.BlueViolet);

					}
				}

				if (IsIntersectableP)
				{
					if (pPolygonBoundary != null)
						foreach (var tuple in pPolygonBoundary)
						{
							bitmap.DrawLineDDA((int)(tuple.Item1.X / uParam * halfWidth), (int)(tuple.Item1.Y / vParam * height), (int)(tuple.Item2.X / uParam * halfWidth), (int)(tuple.Item2.Y / vParam * height), Colors.BlueViolet);
						}


				}

				if (qPolygon != null)
				{
					var minSize = System.Math.Min(Q.FirstParamLimit, Q.SecondParamLimit) * 0.8;
					for (int i = 0; i < qPolygon.Count - 1 + (cyclic ? 1 : 0); i++)
					{
						var item1 = qPolygon[i];
						var item2 = qPolygon[(i + 1) % qPolygon.Count];
						if (System.Math.Abs(item1.X - item2.X) < minSize && System.Math.Abs(item1.Y - item2.Y) < minSize)
							bitmap.DrawLineDDA((int)(item1.X / sParam * (halfWidth - 3) + halfWidth), (int)(item1.Y / tParam * height), (int)(item2.X / sParam * (halfWidth - 3) + halfWidth), (int)(item2.Y / tParam * height), Colors.Gray);

					}
				}

				if (IsIntersectableQ)
				{
					if (qPolygonBoundary != null)
						foreach (var tuple in qPolygonBoundary)
						{
							bitmap.DrawLineDDA((int)(tuple.Item1.X / sParam * (halfWidth - 3) + halfWidth), (int)(tuple.Item1.Y / tParam * height), (int)(tuple.Item2.X / sParam * (halfWidth - 3) + halfWidth), (int)(tuple.Item2.Y / tParam * height), Colors.Gray);
						}



				}

				//if (PtestSet != null)
				//{
				//	var uStep = P.FirstParamLimit / (PtestSet.GetLength(1) - 1.0);
				//	var vStep = P.SecondParamLimit / (PtestSet.GetLength(0) - 1.0);


				//	for (int i = 0; i < PtestSet.GetLength(1); i++)
				//		for (int j = 0; j < PtestSet.GetLength(0); j++)
				//		{
				//			bitmap.DrawEllipseCentered((int)(i * uStep / uParam * halfWidth), (int)(j * vStep / vParam * height), 1, 1, PtestSet[j, i] ? Colors.Green : Colors.Red);
				//		}
				//}

				if (QtestSet != null)
				{
					var uStep = Q.FirstParamLimit / (QtestSet.GetLength(1) - 1.0);
					var vStep = Q.SecondParamLimit / (QtestSet.GetLength(0) - 1.0);


					for (int i = 0; i < QtestSet.GetLength(1); i++)
						for (int j = 0; j < QtestSet.GetLength(0); j++)
						{
							bitmap.DrawEllipseCentered((int)(i * uStep / sParam * (halfWidth-3) + halfWidth), (int)(j * vStep / tParam * height), 1, 1, QtestSet[j, i] ? Colors.Green : Colors.Red);
						}
				}

			}

		}

		private class BoundaryIntersection
		{
			public enum IntersectionType
			{
				In,
				Out
			}

			public IntersectionType type;
			public double X;
			public double Y;

			public int AssociatedVertexIndex;

		}

		private bool CalculatePolygon(bool p, out List<Vector2> polygon, out List<Tuple<Vector2, Vector2>> boundary)
		{
			boundary = null;
			var upperEdge = new List<BoundaryIntersection>();
			var lowerEdge = new List<BoundaryIntersection>();
			var rightEdge = new List<BoundaryIntersection>();
			var leftEdge = new List<BoundaryIntersection>();
			var inters = p ? P : Q;
			var pts = p ? points.Select(x => new Vector2(x.X, x.Y)).ToList() : points.Select(x => new Vector2(x.Z, x.W)).ToList();
			polygon = pts;

			var addEdges = new List<Tuple<Vector2, Vector2>>();
			var last = pts.Count - 1;

			#region NonCyclic

			// non looped surface with cyclic curve - always can trim
			if (cyclic && !inters.FirstParamLooped && !inters.SecondParamLooped)
				return true;
			int cnt = 0;
			int u = 0;
			int v = 0;

			if (!cyclic)
			{

				if (!inters.FirstParamLooped)
				{
					if (System.Math.Abs(System.Math.Abs(inters.FirstParamLimit / 2.0 - pts[0].X) - inters.FirstParamLimit / 2.0) < 0.01)
					{
						var tmp = pts[0];
						cnt++;
						v++;
						addEdges.Add(tmp.X < inters.FirstParamLimit / 2.0
							? new Tuple<Vector2, Vector2>(new Vector2(-0.001, tmp.Y), new Vector2(0.001, tmp.Y))
							: new Tuple<Vector2, Vector2>(new Vector2(inters.FirstParamLimit - 0.001, tmp.Y),
								new Vector2(inters.FirstParamLimit + 0.001, tmp.Y)));
					}
					if (System.Math.Abs(System.Math.Abs(inters.FirstParamLimit / 2.0 - pts[last].X) - inters.FirstParamLimit / 2.0) < 0.01)
					{
						var tmp = pts[last];
						cnt++;
						v++;
						addEdges.Add(tmp.X < inters.FirstParamLimit / 2.0
							? new Tuple<Vector2, Vector2>(new Vector2(-0.001, tmp.Y), new Vector2(0.001, tmp.Y))
							: new Tuple<Vector2, Vector2>(new Vector2(inters.FirstParamLimit - 0.001, tmp.Y),
								new Vector2(inters.FirstParamLimit + 0.001, tmp.Y)));
					}
				}


				if (System.Math.Abs(System.Math.Abs(inters.SecondParamLimit / 2.0 - pts[0].Y) - inters.SecondParamLimit / 2.0) < 0.01)
				{
					//should count as intersection only if second param is not looped.
					if (!inters.SecondParamLooped)
					{
						cnt++;
						u++;
					}
					if (pts[0].Y < inters.SecondParamLimit / 2)
						upperEdge.Add(new BoundaryIntersection { AssociatedVertexIndex = 0, X = pts[0].X });
				}
				if (System.Math.Abs(System.Math.Abs(inters.SecondParamLimit / 2.0 - pts[last].Y) - inters.SecondParamLimit / 2.0) < 0.01)
				{
					if (!inters.SecondParamLooped)
					{
						cnt++;
						u++;
					}
					if (pts[last].Y < inters.SecondParamLimit / 2)
						upperEdge.Add(new BoundaryIntersection { AssociatedVertexIndex = 0, X = pts[last].X });

				}

				//if more/less than 2 boundaries intersection - non trimming edge.
				if (cnt != 2)
					return false;

				var up = Cut(upperEdge.OrderBy(x => x.X), new Vector2(-0.001, -0.001), new Vector2(inters.FirstParamLimit + 0.001, -0.001)).Concat(addEdges).ToList();

				// if 2 intersections on non-looped surface, or 2 intersections on non-looped edge of cylinder-like surface.
				if (!inters.FirstParamLooped && !inters.SecondParamLooped)
				{
					boundary = up;
					return true;
				}

				if ((inters.FirstParamLooped && u != 2) || (inters.SecondParamLooped && v != 2))
					return false;
			}
			#endregion
			//tu są odsiane: niecykliczna krzywa na nieloopniętej powierzchni, cykliczna na nieloopnietej.

			for (int i = 0; i < pts.Count - (cyclic ? 0 : 1); i++)
			{
				if ((pts[i] - pts[(i + 1) % pts.Count]).Length() > 0.5)
				{
					var pt1 = pts[i];
					var pt2 = pts[(i + 1) % pts.Count];
					if (System.Math.Abs(pt1.X - pt2.X) > 0.5)
					{
						var right = pt1.X > pt2.X;
						var avgheigh = (pt1.Y + pt2.Y) / 2;

						addEdges.Add(new Tuple<Vector2, Vector2>(new Vector2(-0.001, avgheigh), new Vector2(0.001, avgheigh)));
						addEdges.Add(new Tuple<Vector2, Vector2>(new Vector2(inters.FirstParamLimit - 0.001, avgheigh), new Vector2(inters.FirstParamLimit + 0.001, avgheigh)));


						leftEdge.Add(new BoundaryIntersection { X = 0, Y = avgheigh, type = !right ? BoundaryIntersection.IntersectionType.Out : BoundaryIntersection.IntersectionType.In });
						rightEdge.Add(new BoundaryIntersection { X = inters.FirstParamLimit, Y = avgheigh, type = right ? BoundaryIntersection.IntersectionType.Out : BoundaryIntersection.IntersectionType.In });
					}

					if (System.Math.Abs(pt1.Y - pt2.Y) > 0.5)
					{
						var up = pt1.Y < pt2.Y;
						var avgwidth = (pt1.X + pt2.X) / 2;
						upperEdge.Add(new BoundaryIntersection { X = avgwidth, Y = 0, type = up ? BoundaryIntersection.IntersectionType.Out : BoundaryIntersection.IntersectionType.In });
						lowerEdge.Add(new BoundaryIntersection { X = avgwidth, Y = inters.SecondParamLimit, type = !up ? BoundaryIntersection.IntersectionType.Out : BoundaryIntersection.IntersectionType.In });
					}
				}
			}

			//walec o zawiniętej jednej powierzchni:

			if (u == 2 || v == 2 || !inters.FirstParamLooped || !inters.SecondParamLooped)
			{
				var up = Cut(upperEdge.OrderBy(x => x.X), new Vector2(-0.001, -0.001), new Vector2(inters.FirstParamLimit + 0.001, -0.001)).Concat(addEdges).ToList();
				boundary = up;
				return true;
			}

			//torus
			var boundaryIntersections = upperEdge.OrderBy(x => x.X).Concat(rightEdge.OrderBy(x => x.Y)).Concat(lowerEdge.OrderByDescending(x => x.X)).Concat(leftEdge.OrderByDescending(x => x.Y)).ToList();

			if (boundaryIntersections.Count == 2)
			{
				return false;
			}

			var lastIntersection = boundaryIntersections.First().type;

			foreach (var boundaryIntersection in boundaryIntersections.Skip(1))
			{
				if (lastIntersection == boundaryIntersection.type)
					return false;
				lastIntersection = boundaryIntersection.type;
			}

			boundary = Cut(upperEdge.OrderBy(x => x.X), new Vector2(-0.001, 0), new Vector2(inters.FirstParamLimit + 0.001, 0)).Concat(addEdges).ToList();
			return true;
		}

		//private bool met;

		private IEnumerable<Tuple<Vector2, Vector2>> Cut(IEnumerable<BoundaryIntersection> edge, Vector2 from, Vector2 to)
		{
			var meet = false;
			var lst = new BoundaryIntersection() { X = from.X, Y = from.Y };
			foreach (var bndr in edge)
			{

				meet = !meet;
				//met = meet;
				if (!meet)
					lst = bndr;
				else
				{
					yield return new Tuple<Vector2, Vector2>(new Vector2(lst.X, lst.Y - 0.0001), new Vector2(bndr.X, bndr.Y - 0.0001));
				}
			}
			if (!meet)
			{
				yield return new Tuple<Vector2, Vector2>(new Vector2(lst.X, lst.Y - 0.0001), new Vector2(to.X, to.Y - 0.0001));
			}
		}

		private List<Vector2> poly;
		private List<Tuple<Vector2, Vector2>> boundary;


		private void CheckColumn(int[,] pts, IIntersectable sender, double u, int column, int from, int to, double vStep, double vFrom)
		{
			//pts[j, i] = PointBelongs(partA, sender, new Vector2(i * uStep + uFrom, j * vStep + vFrom));
			if (pts[from, column] == -1)
			{
				pts[from, column] = PointBelongs(sender, new Vector2(u, from * vStep + vFrom));
			}
			if (pts[to, column] == -1)
			{
				pts[to, column] = PointBelongs(sender, new Vector2(u, to * vStep + vFrom));

			}

			if (from == to || from + 1 == to)
				return;


			if (pts[from, column] == pts[to, column])
			{

				for (int i = from + 1; i < to; i++)
					pts[i, column] = pts[from, column];
			}
			else
			{
				var newPt = (from + to) / 2;

				CheckColumn(pts, sender, u, column, from, newPt, vStep, vFrom);
				CheckColumn(pts, sender, u, column, newPt, to, vStep, vFrom);
			}



		}

		public void PointsContainedByCurve(bool[,] pts, bool partA, IIntersectable sender, double uFrom, double uTo, double vFrom, double vTo, bool additive)
		{
			var uStep = (uTo - uFrom) / (pts.GetLength(1) - 1.0);
			var vStep = (vTo - vFrom) / (pts.GetLength(0) - 1.0);
			poly = sender == P ? pPolygon : qPolygon;
			boundary = sender == P ? pPolygonBoundary : qPolygonBoundary;


			var ptss = new int[pts.GetLength(0), pts.GetLength(1)];
			for (int i = 0; i < ptss.GetLength(1); i++)
				for (int j = 0; j < ptss.GetLength(0); j++)
				{
					ptss[j, i] = -1;
				}

			for (int i = 0; i < ptss.GetLength(1); i++)
			{
				CheckColumn(ptss, sender, i * uStep + uFrom, i, 0, ptss.GetLength(0) - 1, vStep, vFrom);
			}

			//if (sender == P)
			//{
			//	PtestSet = new bool[ptss.GetLength(0), ptss.GetLength(1)];
			//	for (int i = 0; i < PtestSet.GetLength(0); i++)
			//		for (int j = 0; j < PtestSet.GetLength(1); j++)
			//			PtestSet[i, j] = ptss[i, j] % 2 == 0;
			//}

			//if (sender == Q)
			//{
			//	QtestSet = new bool[ptss.GetLength(0), ptss.GetLength(1)];
			//	for (int i = 0; i < QtestSet.GetLength(0); i++)
			//		for (int j = 0; j < QtestSet.GetLength(1); j++)
			//			QtestSet[i, j] = ptss[i, j] % 2 == 0;
			//}

			if (additive)
			{
				for (int i = 0; i < pts.GetLength(1); i++)
					for (int j = 0; j < pts.GetLength(0); j++)
					{
						pts[j, i] = pts[j, i] && (ptss[j, i] % 2 == 0) == partA;
					}
			}
			else
			{
				for (int i = 0; i < pts.GetLength(1); i++)
					for (int j = 0; j < pts.GetLength(0); j++)
					{
						pts[j, i] = pts[j, i] || (ptss[j, i] % 2 == 0) == partA;
					}
			}
		}

		public bool PointAvaiable(IIntersectable sender, Vector2 point, bool partA)
		{
			return (PointBelongs(sender, point) % 2 == 0) == partA;
		}

		public int PointBelongs(IIntersectable sender, Vector2 point)
		{
			counter++;
			int p = boundary?.Count(tuple => IntersectLines(tuple.Item1, tuple.Item2, point)) ?? 0;
			Vector2 p1, p2;
			int cnt = poly.Count - 1;
			var minSize = System.Math.Min(sender.FirstParamLimit, sender.SecondParamLimit)*0.8;

			for (int i = 0; i < cnt; i++)
			{
				p1 = poly[i];
				p2 = poly[i + 1];
				if ((p1 - p2).Length() < minSize)
					if (IntersectLines(p1, p2, point))
						p++;
			}
			if (cyclic)
				if ((poly[cnt] - poly[0]).Length() < minSize)
				{
					if (IntersectLines(poly[cnt], poly[0], point))
						p++;
				}

			return p;
		}

		private bool IntersectLines(Vector2 segment1, Vector2 segment2, Vector2 point)
		{
			if ((segment1.X - point.X) * (segment2.X - point.X) > 0)
				return false;

			return segment1.Y + (segment2.Y - segment1.Y) * (point.X - segment1.X) / (segment2.X - segment1.X) < point.Y;
		}

		private static int counter = 0;
		public static void ResetCounter()
		{
			counter = 0;
		}

		public static void DrawResult(int shouldBe)
		{
			Console.WriteLine($"Should be {shouldBe}, is: {counter}.");
		}
		public bool IsIntersectable(IIntersectable sender)
		{
			if (sender == P)
				return IsIntersectableP;
			if (sender == Q)
				return IsIntersectableQ;
			return false;
		}

	}

}
