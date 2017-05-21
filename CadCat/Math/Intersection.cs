﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using CadCat.ModelInterfaces;

namespace CadCat.Math
{
	public class Intersection
	{
		private static SceneData data;
		public static void Intersect(IIntersectable P, IIntersectable Q, SceneData scene)
		{

			double d = 0.01;
			data = scene;
			var pPoints = P.GetPointsForSearch(8, 8).ToList();
			var qPoints = Q.GetPointsForSearch(8, 8).ToList();

			var closestDistance = double.MaxValue;
			var pPoint = new ParametrizedPoint();
			var qPoint = new ParametrizedPoint();

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
			Func<Math.Vector4, double> distanceFun = (Vector4 arg) => (P.GetPosition(arg.X, arg.Y) - Q.GetPosition(arg.Z, arg.W)).LengthSquared();

			Func<Vector4, Vector4> distanceGradient = (Vector4 arg) =>
			{
				var diff = P.GetPosition(arg.X, arg.Y) - Q.GetPosition(arg.Z, arg.W);

				var du = P.GetFirstParamDerivative(arg.X, arg.Y);
				var dv = P.GetSecondParamDerivative(arg.X, arg.Y);
				var ds = Q.GetFirstParamDerivative(arg.Z, arg.W);
				var dt = Q.GetSecondParamDerivative(arg.Z, arg.W);

				return new Vector4(Vector3.DotProduct(diff, du), Vector3.DotProduct(diff, dv), -Vector3.DotProduct(diff, ds), -Vector3.DotProduct(diff, dt)) * 2.0;
			};

			var startPoint = SimpleGradient(distanceFun, distanceGradient, pPoint, qPoint, P, Q);

			scene.CreateHiddenCatPoint(P.GetPosition(startPoint.X, startPoint.Y));

			Func<Vector4, bool, Tuple<Matrix4, Vector3>> jacobian = (Vector4 arg, bool invert) =>
			  {
				  var du = P.GetFirstParamDerivative(arg.X, arg.Y);
				  var dv = P.GetSecondParamDerivative(arg.X, arg.Y);
				  var ds = Q.GetFirstParamDerivative(arg.Y, arg.Z);
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


				  var np = Vector3.CrossProduct(du, dv);
				  var nq = Vector3.CrossProduct(ds, dt);
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

			for (int i = 0; i < 100; i++)
				try
				{

					Vector4 point = startPoint;
					Vector4 prevPoint;
					do
					{
						prevPoint = point;
						var jacob = jacobian(point, false);
						var funRes = function(point, P.GetPosition(point.X, point.Y), jacob.Item2);

						var nextP = point - jacob.Item1.Inversed() * funRes;
						point = nextP;
						scene.CreateHiddenCatPoint(P.GetPosition(point.X, point.Y));
					} while ((P.GetPosition(point.X, point.Y) - P.GetPosition(prevPoint.X, prevPoint.Y)).Length() > 0.0001);
					startPoint = point;
				}
				catch (Exception e)
				{

				}


		}

		private static Vector4 SimpleGradient(Func<Vector4, double> distanceFun, Func<Vector4, Vector4> grad,
			ParametrizedPoint pPoint, ParametrizedPoint qPoint, IIntersectable P, IIntersectable Q)
		{
			double alpha = 0.01;

			var startingpoint = new Vector4(pPoint.Parametrization.X, pPoint.Parametrization.Y, qPoint.Parametrization.X, qPoint.Parametrization.Y);

			var point = startingpoint + grad(startingpoint) * alpha;
			var distance = distanceFun(point);
			double dist = distance;
			Vector4 pt = point;
			try
			{

				do
				{
					point = pt;
					distance = dist;

					var grd = grad(point);
					pt = point - grd * alpha;
					dist = distanceFun(pt);
				} while (System.Math.Abs(distance - dist) > double.Epsilon);
			}
			catch (Exception e)
			{
				return new Vector4(-1, -1, -1, -1);
			}

			return pt;
		}
	}
}