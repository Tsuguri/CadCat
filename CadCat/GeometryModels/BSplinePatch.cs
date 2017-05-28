using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using CadCat.DataStructures;
using CadCat.Math;
using CadCat.Rendering;

namespace CadCat.GeometryModels
{
	class BSplinePatch : Patch
	{

		public BSplinePatch(CatPoint[,] pts) : base(pts)
		{
		}










		private static Matrix4 _tempMtx;
		private Vector3 EvaluatePointValue(double u, double v)
		{

			var uVal = EvaluateBSpline(u, 3);
			var vVal = EvaluateBSpline(v, 3);
			_tempMtx = uVal.MatrixMultiply(vVal);


			return Sum();
		}

		private Vector3 Sum()
		{
			var sum = new Vector3();
			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
				{
					sum += pointsOrdererd[j, i].Position * _tempMtx[i, j];
				}

			return sum;
		}

		private Vector3 EvaluateUDerivative(double u, double v)
		{
			var uVal = EvaluateBSpline(u, 2);// wyniki w xyz
			var vVal = EvaluateBSpline(v, 3);
			_tempMtx = uVal.MatrixMultiply(vVal);

			var sum = new Vector3();
			for (int i = 0; i < 3; i++)
				for (int j = 0; j < 4; j++)
				{
					sum += (pointsOrdererd[j, i + 1].Position - pointsOrdererd[j, i].Position) * _tempMtx[i, j];
				}

			return sum * 1;
		}

		private Vector3 EvaluateVDerivative(double u, double v)
		{
			var uVal = EvaluateBSpline(u, 3);
			var vVal = EvaluateBSpline(v, 2);// wyniki w xyz
			_tempMtx = uVal.MatrixMultiply(vVal);

			var sum = new Vector3();
			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 3; j++)
				{
					sum += (pointsOrdererd[j + 1, i].Position - pointsOrdererd[j, i].Position) * _tempMtx[i, j];
				}

			return sum * 1;
		}

		private Vector4 EvaluateBSpline(double t, int degree)
		{
			var n = new Vector4 { [0] = 1.0 };
			double tm = 1.0 - t;
			for (int j = 1; j <= degree; j++)
			{
				double saved = 0.0;
				for (int k = 1; k <= j; k++)
				{
					double term = n[k - 1] / ((tm + k - 1.0) + (t + j - k));
					n[k - 1] = saved + (tm + k - 1.0) * term;
					saved = (t + j - k) * term;
				}
				n[j] = saved;
			}

			return n;
		}





		public override string GetName()
		{
			return "BSpline patch " + base.GetName();
		}

		public override Vector3 GetPoint(double u, double v)
		{
			return EvaluatePointValue(u, v);
		}

		public override Vector3 GetUDerivative(double u, double v)
		{
			return EvaluateUDerivative(u, v);
		}

		public override Vector3 GetVDerivative(double u, double v)
		{
			return EvaluateVDerivative(u, v);
		}
	}
}
