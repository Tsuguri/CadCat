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

			if (IsIntersectableP)
			{
				var size = 10;
				PtestSet = new bool[size, size];
				for (int i = 0; i < size; i++)
					for (int j = 0; j < size; j++)
						PtestSet[i, j] = true;

				PointsContainedByCurve(PtestSet, true, P);
			}

			if (IsIntersectableQ)
			{
				var size = 10;
				QtestSet = new bool[size, size];
				for (int i = 0; i < size; i++)
				for (int j = 0; j < size; j++)
					QtestSet[i, j] = true;

				PointsContainedByCurve(QtestSet, true, Q);
			}

		}

		public override void CleanUp()
		{
			base.CleanUp();
			foreach (var point in catPoints)
			{
				scene.RemovePoint(point);
			}
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
					for (int i = 0; i < pPolygon.Count - 1 + (cyclic ? 1 : 0); i++)
					{
						var item1 = pPolygon[i];
						var item2 = pPolygon[(i + 1) % pPolygon.Count];
						if (System.Math.Abs(item1.X - item2.X) < 0.1 && System.Math.Abs(item1.Y - item2.Y) < 0.1)
							bitmap.DrawLineDDA((int)(item1.X / uParam * halfWidth), (int)(item1.Y / vParam * height), (int)(item2.X / uParam * halfWidth), (int)(item2.Y / vParam * height), Colors.Gray);

					}

				if (IsIntersectableP)
				{
					if (pPolygonBoundary != null)
						foreach (var tuple in pPolygonBoundary)
						{
							bitmap.DrawLineDDA((int)(tuple.Item1.X / uParam * halfWidth), (int)(tuple.Item1.Y / vParam * height), (int)(tuple.Item2.X / uParam * halfWidth), (int)(tuple.Item2.Y / vParam * height), Colors.Gray);
						}


				}

				if (qPolygon != null)
					for (int i = 0; i < qPolygon.Count - 1 + (cyclic ? 1 : 0); i++)
					{
						var item1 = qPolygon[i];
						var item2 = qPolygon[(i + 1) % qPolygon.Count];
						if (System.Math.Abs(item1.X - item2.X) < 0.1 && System.Math.Abs(item1.Y - item2.Y) < 0.1)
							bitmap.DrawLineDDA((int)(item1.X / sParam * (halfWidth - 3) + halfWidth), (int)(item1.Y / tParam * height), (int)(item2.X / sParam * (halfWidth - 3) + halfWidth), (int)(item2.Y / tParam * height), Colors.Gray);

					}

				if (IsIntersectableQ)
				{
					if (qPolygonBoundary != null)
						foreach (var tuple in qPolygonBoundary)
						{
							bitmap.DrawLineDDA((int)(tuple.Item1.X / sParam * (halfWidth - 3) + halfWidth), (int)(tuple.Item1.Y / tParam * height), (int)(tuple.Item2.X / sParam * (halfWidth - 3) + halfWidth), (int)(tuple.Item2.Y / tParam * height), Colors.Gray);
						}



				}

				if (PtestSet != null)
				{
					var uStep = P.FirstParamLimit / (PtestSet.GetLength(1) - 1.0);
					var vStep = P.SecondParamLimit / (PtestSet.GetLength(0) - 1.0);


					for (int i = 0; i < PtestSet.GetLength(1); i++)
						for (int j = 0; j < PtestSet.GetLength(0); j++)
						{
								bitmap.DrawEllipseCentered((int)(i * uStep / uParam * halfWidth), (int)(j * vStep / vParam * height), 1, 1, PtestSet[j,i] ? Colors.Green : Colors.Red);
						}
				}

				if (QtestSet != null)
				{
					var uStep = Q.FirstParamLimit / (QtestSet.GetLength(1) - 1.0);
					var vStep = Q.SecondParamLimit / (QtestSet.GetLength(0) - 1.0);


					for (int i = 0; i < QtestSet.GetLength(1); i++)
					for (int j = 0; j < QtestSet.GetLength(0); j++)
					{
						bitmap.DrawEllipseCentered((int)(i * uStep / uParam * halfWidth + halfWidth), (int)(j * vStep / vParam * height), 1, 1, QtestSet[j, i] ? Colors.Green : Colors.Red);
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
			polygon = null;
			boundary = null;
			var upperEdge = new List<BoundaryIntersection>();
			var lowerEdge = new List<BoundaryIntersection>();
			var rightEdge = new List<BoundaryIntersection>();
			var leftEdge = new List<BoundaryIntersection>();
			var inters = p ? P : Q;
			var pts = p ? points.Select(x => new Vector2(x.X, x.Y)).ToList() : points.Select(x => new Vector2(x.Z, x.W)).ToList();
			polygon = pts;
			#region NonCyclic

			if (!cyclic && (inters.FirstParamLooped || inters.SecondParamLooped))
				return false;


			if (!cyclic)
			{
				var last = pts.Count - 1;
				if (System.Math.Abs(System.Math.Abs(inters.FirstParamLimit / 2.0 - pts[0].X) - inters.FirstParamLimit / 2.0) < 0.01)
				{
					if (pts[0].X < inters.FirstParamLimit / 2.0)
					{
						var tmp = pts[0];
						tmp.X = -0.001;
						pts[0] = tmp;
						leftEdge.Add(new BoundaryIntersection { AssociatedVertexIndex = 0, type = BoundaryIntersection.IntersectionType.In, X = 0, Y = pts[0].Y });
					}
					else
					{
						var tmp = pts[0];
						tmp.X = inters.FirstParamLimit + 0.001;
						pts[0] = tmp;
						rightEdge.Add(new BoundaryIntersection { AssociatedVertexIndex = 0, type = BoundaryIntersection.IntersectionType.In, X = inters.FirstParamLimit, Y = pts[0].Y });
					}
				}
				else if (System.Math.Abs(System.Math.Abs(inters.SecondParamLimit / 2.0 - pts[0].Y) - inters.SecondParamLimit / 2.0) < 0.01)
				{
					if (pts[0].Y < inters.SecondParamLimit / 2.0)
					{
						var tmp = pts[0];
						tmp.Y = -0.001;
						pts[0] = tmp;
						upperEdge.Add(new BoundaryIntersection { AssociatedVertexIndex = 0, type = BoundaryIntersection.IntersectionType.In, Y = 0, X = pts[0].X });
					}
					else
					{
						var tmp = pts[0];
						tmp.Y = inters.SecondParamLimit + 0.001;
						pts[0] = tmp;
						lowerEdge.Add(new BoundaryIntersection { AssociatedVertexIndex = 0, type = BoundaryIntersection.IntersectionType.In, Y = inters.SecondParamLimit, X = pts[0].X });

					}
				}

				if (System.Math.Abs(System.Math.Abs(inters.FirstParamLimit / 2.0 - pts[last].X) - inters.FirstParamLimit / 2.0) < 0.01)
				{
					if (pts[last].X < inters.FirstParamLimit / 2.0)
					{
						var tmp = pts[last];
						tmp.X = -0.001;
						pts[last] = tmp;
						leftEdge.Add(new BoundaryIntersection { AssociatedVertexIndex = last, type = BoundaryIntersection.IntersectionType.Out, X = 0, Y = pts[last].Y });
					}
					else
					{
						var tmp = pts[last];
						tmp.X = inters.FirstParamLimit + 0.001;
						pts[last] = tmp;
						rightEdge.Add(new BoundaryIntersection { AssociatedVertexIndex = 0, type = BoundaryIntersection.IntersectionType.Out, X = inters.FirstParamLimit, Y = pts[last].Y });

					}
				}
				else if (System.Math.Abs(System.Math.Abs(inters.SecondParamLimit / 2.0 - pts[last].Y) - inters.SecondParamLimit / 2.0) < 0.01)
				{
					if (pts[last].Y < inters.SecondParamLimit / 2.0)
					{
						var tmp = pts[last];
						tmp.Y = -0.001;
						pts[last] = tmp;
						upperEdge.Add(new BoundaryIntersection { AssociatedVertexIndex = last, type = BoundaryIntersection.IntersectionType.Out, Y = 0, X = pts[last].X });
					}
					else
					{
						var tmp = pts[last];
						tmp.Y = inters.SecondParamLimit + 0.001;
						pts[last] = tmp;
						lowerEdge.Add(new BoundaryIntersection { AssociatedVertexIndex = last, type = BoundaryIntersection.IntersectionType.Out, Y = inters.SecondParamLimit, X = pts[last].X });

					}
				}

				if (leftEdge.Count + rightEdge.Count + upperEdge.Count + lowerEdge.Count != 2)
					return false;


				var up = Cut(upperEdge.OrderBy(x => x.X), new Vector2(-0.001, -0.001), new Vector2(inters.FirstParamLimit + 0.001, -0.001)).ToList();
				boundary = up;

				return true;
			}

			#endregion

			#region Cyclic

			var addEdges = new List<Tuple<Vector2, Vector2>>();

			for (int i = 0; i < pts.Count; i++)
			{
				if ((pts[i] - pts[(i + 1) % pts.Count]).Length() > 0.1)
				{
					var pt1 = pts[i];
					var pt2 = pts[(i + 1) % pts.Count];
					if (System.Math.Abs(pt1.X - pt2.X) > 0.1)
					{
						var right = pt1.X > pt2.X;
						var avgheigh = (pt1.Y + pt2.Y) / 2;

						addEdges.Add(new Tuple<Vector2, Vector2>(new Vector2(-0.001, avgheigh),new Vector2(0.001, avgheigh) ));
						addEdges.Add(new Tuple<Vector2, Vector2>(new Vector2(inters.FirstParamLimit - 0.001, avgheigh),new Vector2(inters.FirstParamLimit+0.001, avgheigh) ));


						leftEdge.Add(new BoundaryIntersection { X = 0, Y = avgheigh, type = !right ? BoundaryIntersection.IntersectionType.Out : BoundaryIntersection.IntersectionType.In });
						rightEdge.Add(new BoundaryIntersection { X = inters.FirstParamLimit, Y = avgheigh, type = right ? BoundaryIntersection.IntersectionType.Out : BoundaryIntersection.IntersectionType.In });
					}

					if (System.Math.Abs(pt1.Y - pt2.Y) > 0.1)
					{
						var up = pt1.Y < pt2.Y;
						var avgwidth = (pt1.X + pt2.X) / 2;
						upperEdge.Add(new BoundaryIntersection { X = avgwidth, Y = 0, type = up ? BoundaryIntersection.IntersectionType.Out : BoundaryIntersection.IntersectionType.In });
						lowerEdge.Add(new BoundaryIntersection { X = avgwidth, Y = inters.SecondParamLimit, type = !up ? BoundaryIntersection.IntersectionType.Out : BoundaryIntersection.IntersectionType.In });
					}
				}
			}

			var boundaryIntersections = upperEdge.OrderBy(x => x.X).Concat(rightEdge.OrderBy(x => x.Y)).Concat(lowerEdge.OrderByDescending(x => x.X)).Concat(leftEdge.OrderByDescending(x => x.Y)).ToList();

			if (boundaryIntersections.Count == 0 || (!inters.FirstParamLooped || !inters.SecondParamLooped))
			{
				var up = Cut(upperEdge.OrderBy(x => x.X), new Vector2(-0.001, -0.001), new Vector2(inters.FirstParamLimit + 0.001, -0.001)).Concat(addEdges).ToList();
				boundary = up;
				return true;
			}

			//check dla dwucykliczności

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

			boundary = Cut(upperEdge.OrderBy(x => x.X), new Vector2(-0.001, 0), new Vector2(inters.FirstParamLimit + 0.001, 0)).ToList();
			boundary = boundary.Select(x =>
			{
				var tmp = x.Item1;
				tmp.Y -= 0.001;
				var tmp2 = x.Item2;
				tmp2.Y -= 0.001;
				return new Tuple<Vector2, Vector2>(tmp, tmp2);
			}).Concat(addEdges).ToList();

			#endregion
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
					yield return new Tuple<Vector2, Vector2>(new Vector2(lst.X, lst.Y), new Vector2(bndr.X, bndr.Y));
				}
			}
			if (!meet)
			{
				yield return new Tuple<Vector2, Vector2>(new Vector2(lst.X, lst.Y), new Vector2(to.X, to.Y));
			}
		}

		private List<Vector2> poly;
		private List<Tuple<Vector2, Vector2>> boundary;

		public void PointsContainedByCurve(bool[,] pts, bool partA, IIntersectable sender)
		{
			var uStep = sender.FirstParamLimit / (pts.GetLength(1) - 1.0);
			var vStep = sender.SecondParamLimit / (pts.GetLength(0) - 1.0);

			poly = sender == P ? pPolygon : qPolygon;
			boundary = sender == P ? pPolygonBoundary : qPolygonBoundary;
			for (int i = 0; i < pts.GetLength(1); i++)
				for (int j = 0; j < pts.GetLength(0); j++)
				{
					if (pts[j, i])
					{
						pts[j, i] = PointBelongs(partA, sender, new Vector2(i * uStep, j * vStep));
					}
				}
		}

		public bool PointBelongs(bool partA, IIntersectable sender, Vector2 point)
		{
			

			var lineTo = point;
			lineTo.Y -= 2 * sender.SecondParamLimit;

			int p = boundary?.Count(tuple => IntersectLines(tuple.Item1, tuple.Item2, point, lineTo)) ?? 0;

			for (int i = 0; i < poly.Count - 1; i++)
			{
				if ((poly[i] - poly[i + 1]).Length() < 0.1)
					if (IntersectLines(poly[i], poly[i + 1], point, lineTo))
						p++;
			}

			if (p % 2 == 0)
				return partA;
			return !partA;
		}

		private bool IntersectLines(Vector2 firstFrom, Vector2 firstTo, Vector2 secondFrom, Vector2 secondTo)
		{
			var d1 = (secondTo - secondFrom).Cross(firstFrom - secondFrom);
			var d2 = (secondTo - secondFrom).Cross(firstTo - secondFrom);
			var d3 = (firstTo - firstFrom).Cross(secondFrom - firstFrom);
			var d4 = (firstTo - firstFrom).Cross(secondTo - firstFrom);

			var d12 = d1 * d2;
			var d34 = d3 * d4;
			if (d12 > 0 && d34 > 0)
				return false;
			if (d12 < 0 && d34 < 0)
				return true;

			return OnRectiange(firstFrom, secondFrom, secondTo) || OnRectiange(firstTo, secondFrom, secondTo)
				   || OnRectiange(secondFrom, firstFrom, firstTo) || OnRectiange(secondTo, firstFrom, firstTo);
		}

		private bool OnRectiange(Vector2 q, Vector2 p1, Vector2 p2)
		{
			return System.Math.Min(p1.X, p2.X) < q.X && q.X <= System.Math.Max(p1.X, p2.X)
				   && System.Math.Min(p1.Y, p2.Y) < q.Y && q.Y <= System.Math.Max(p1.Y, p2.Y);
		}

	}

}
