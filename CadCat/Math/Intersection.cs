using System;
using System.Collections.Generic;
using System.Linq;
using CadCat.DataStructures;
using CadCat.ModelInterfaces;

namespace CadCat.Math
{
	public class Intersection
	{
		//private static SceneData _data;
		public static List<Vector4> Intersect(IIntersectable P, IIntersectable Q, SceneData scene, double newtonStep, out bool cycleIntersection)
		{
			if (P == null) throw new ArgumentNullException(nameof(P));
			if (Q == null) throw new ArgumentNullException(nameof(Q));
			double d = newtonStep;
			cycleIntersection = false;
			//_data = scene;
			var pPoints = P.GetPointsForSearch(8, 8).ToList();
			var qPoints = Q.GetPointsForSearch(8, 8).ToList();

			var closestDistance = double.MaxValue;
			var pPoint = new ParametrizedPoint();
			var qPoint = new ParametrizedPoint();

			if (scene.Cursor.Visible)
			{
				var cursorPos = scene.Cursor.Transform.Position;
				foreach (var pP in pPoints)
				{
					if ((pP.Position - cursorPos).LengthSquared() < closestDistance)
					{
						closestDistance = (pP.Position - cursorPos).LengthSquared();
						pPoint = pP;
					}
				}
				closestDistance = double.MaxValue;
				foreach (var qP in qPoints)
				{
					if ((qP.Position - cursorPos).LengthSquared() < closestDistance)
					{
						closestDistance = (qP.Position - cursorPos).LengthSquared();
						qPoint = qP;
					}
				}

			}
			else
			{
				foreach (var pP in pPoints)
				{
					foreach (var qP in qPoints)
					{
						if ((qP.Position - pP.Position).LengthSquared() < closestDistance)
						{
							closestDistance = (qP.Position - pP.Position).LengthSquared();
							pPoint = pP;
							qPoint = qP;
						}
					}
				}
			}
			Func<Vector4, double> distanceFun = arg => (P.GetPosition(arg.X, arg.Y) - Q.GetPosition(arg.Z, arg.W)).LengthSquared();

			Func<Vector4, Vector4> distanceGradient = arg =>
			{
				var diff = P.GetPosition(arg.X, arg.Y) - Q.GetPosition(arg.Z, arg.W);

				var du = P.GetFirstParamDerivative(arg.X, arg.Y);
				var dv = P.GetSecondParamDerivative(arg.X, arg.Y);
				var ds = Q.GetFirstParamDerivative(arg.Z, arg.W);
				var dt = Q.GetSecondParamDerivative(arg.Z, arg.W);

				return new Vector4(Vector3.DotProduct(diff, du), Vector3.DotProduct(diff, dv), -Vector3.DotProduct(diff, ds), -Vector3.DotProduct(diff, dt)) * 2.0;
			};


			var startPoint = SimpleGradient(distanceFun, distanceGradient, pPoint, qPoint, P, Q);


			if (startPoint == null)
				return null;

			//scene.CreateHiddenCatPoint(P.GetPosition(startPoint.X, startPoint.Y));

			Func<Vector4, bool, Tuple<Matrix4, Vector3>> jacobian = (arg, invert) =>
			  {
				  var du = P.GetFirstParamDerivative(arg.X, arg.Y);
				  var dv = P.GetSecondParamDerivative(arg.X, arg.Y);
				  var ds = Q.GetFirstParamDerivative(arg.Z, arg.W);
				  var dt = Q.GetSecondParamDerivative(arg.Z, arg.W);

				  Matrix4 jacob = new Matrix4
				  {
					  [0, 0] = du.X,
					  [1, 0] = du.Y,
					  [2, 0] = du.Z,
					  [0, 1] = dv.X,
					  [1, 1] = dv.Y,
					  [2, 1] = dv.Z,
					  [0, 2] = -ds.X,
					  [1, 2] = -ds.Y,
					  [2, 2] = -ds.Z,
					  [0, 3] = -dt.X,
					  [1, 3] = -dt.Y,
					  [2, 3] = -dt.Z,
					  [3, 3] = 0,
					  [3, 2] = 0
				  };


				  var np = Vector3.CrossProduct(du, dv);//.Normalized();
				  var nq = Vector3.CrossProduct(ds, dt);//.Normalized();
				  var t = (invert ? Vector3.CrossProduct(nq, np) : Vector3.CrossProduct(np, nq)).Normalized();
				  jacob[3, 0] = Vector3.DotProduct(du, t);
				  jacob[3, 1] = Vector3.DotProduct(dv, t);

				  return new Tuple<Matrix4, Vector3>(jacob, t);
			  };

			Func<Vector4, Vector3, Vector3, Vector4> function = (args, previousPoint, t) =>
			 {
				 var p = P.GetPosition(args.X, args.Y);
				 var q = Q.GetPosition(args.Z, args.W);
				 var temp = Vector3.DotProduct(p - previousPoint, t) - d;
				 return new Vector4(p - q, temp);
			 };

			bool cyclic;
			var pts = Newton(P, Q, startPoint.Value, function, jacobian, false, d, out cyclic);

			if (pts.Count < 2)
				return null;
			cycleIntersection = cyclic;
			if (cyclic) return pts;
			var pts2 = Newton(P, Q, startPoint.Value, function, jacobian, true, d, out cyclic);
			pts.Reverse();
			pts.AddRange(pts2);
			return pts;
		}

		private static List<Vector4> Newton(IIntersectable p, IIntersectable q, Vector4 startPoint, Func<Vector4, Vector3, Vector3, Vector4> function, Func<Vector4, bool, Tuple<Matrix4, Vector3>> jacobian, bool inverse, double step, out bool cyclic)
		{
			var points = new List<Vector4>();
			//var startestPoint = startPoint;
			bool end = false;
			cyclic = false;
			while (!end)
			{
				Vector4 point = startPoint;
				Vector4 prevPoint;
				int i = 0;
				do
				{
					prevPoint = point;
					var jacob = jacobian(point, inverse);
					var funRes = function(point, p.GetPosition(startPoint.X, startPoint.Y), jacob.Item2);

					var nextP = point - jacob.Item1.Inversed() * funRes;
					var pP = p.ConfirmParams(nextP.X, nextP.Y);
					var qP = q.ConfirmParams(nextP.Z, nextP.W);
					if (qP == null || pP == null)
					{
						end = true;
						break;
					}
					i++;
					point = new Vector4(pP.Value.X, pP.Value.Y, qP.Value.X, qP.Value.Y);

				} while ((p.GetPosition(point.X, point.Y) - p.GetPosition(prevPoint.X, prevPoint.Y)).Length() > 0.0001 && i < 1000);
				if (points.Count > 3 && (p.GetPosition(point.X, point.Y) - p.GetPosition(startPoint.X, startPoint.Y)).Length() <
					0.0001)
					break;

				if (points.Count > 2 && (p.GetPosition(point.X, point.Y) - p.GetPosition(points[0].X, points[0].Y)).Length() < step)
				{
					points.Add(point);

					cyclic = true;
					break;
				}
				startPoint = point;
				points.Add(startPoint);

				//break;

			}
			return points;
		}

		private static Vector4? SimpleGradient(Func<Vector4, double> distanceFun, Func<Vector4, Vector4> grad,
			ParametrizedPoint pPoint, ParametrizedPoint qPoint, IIntersectable p, IIntersectable q)
		{
			double alpha = 0.01;

			var startingpoint = new Vector4(pPoint.Parametrization.X, pPoint.Parametrization.Y, qPoint.Parametrization.X, qPoint.Parametrization.Y);

			var point = startingpoint - grad(startingpoint) * alpha;
			var distance = distanceFun(point);
			double dist = distance;
			Vector4 pt = point;
			try
			{

				do
				{
					point = pt;
					distance = dist;
					var tmpAlpha = alpha;
					var grd = grad(point);
					int i = 0;
					do
					{
						pt = point - grd * tmpAlpha;

						//pt = new Vector4(P.ClipParams(pt.X, pt.Y), Q.ClipParams(pt.Z, pt.W));
						dist = distanceFun(pt);
						tmpAlpha /= 2;
						i++;

					} while (dist > distance && i < 200);

					var pPos = p.ConfirmParams(pt.X, pt.Y);
					var qPos = q.ConfirmParams(pt.Z, pt.W);
					if (qPos == null || pPos == null)
						return null;
					pt = new Vector4(pPos.Value, qPos.Value);
				} while (System.Math.Abs(distance - dist) > double.Epsilon);
			}
			catch (Exception e)
			{
				return null;
			}

			return pt;
		}
	}
}
