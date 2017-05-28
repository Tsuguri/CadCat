using CadCat.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using CadCat.Rendering;
using System.Windows.Media;
using CadCat.Math;

namespace CadCat.GeometryModels
{
	class BezierPatch : Patch
	{

		public BezierPatch(CatPoint[,] pts) : base(pts)
		{
		}


		private static double[,] temp = new double[2, 4];
		private Vector3 EvaluatePointValue(double u, double v)
		{
			temp[0, 0] = EvaluateBerenstein(0, u);
			temp[0, 1] = EvaluateBerenstein(1, u);
			temp[0, 2] = EvaluateBerenstein(2, u);
			temp[0, 3] = EvaluateBerenstein(3, u);

			temp[1, 0] = EvaluateBerenstein(0, v);
			temp[1, 1] = EvaluateBerenstein(1, v);
			temp[1, 2] = EvaluateBerenstein(2, v);
			temp[1, 3] = EvaluateBerenstein(3, v);

			return SumFunctions();
		}

		private Vector3 EvaluateUDerivative(double u, double v)
		{
			temp[0, 0] = EvalueateBerensteinDerivative(0, u);
			temp[0, 1] = EvalueateBerensteinDerivative(1, u);
			temp[0, 2] = EvalueateBerensteinDerivative(2, u);
			temp[0, 3] = EvalueateBerensteinDerivative(3, u);

			temp[1, 0] = EvaluateBerenstein(0, v);
			temp[1, 1] = EvaluateBerenstein(1, v);
			temp[1, 2] = EvaluateBerenstein(2, v);
			temp[1, 3] = EvaluateBerenstein(3, v);

			return SumFunctions() * 3;
		}

		private Vector3 EvaluateVDerivative(double u, double v)
		{
			temp[0, 0] = EvaluateBerenstein(0, u);
			temp[0, 1] = EvaluateBerenstein(1, u);
			temp[0, 2] = EvaluateBerenstein(2, u);
			temp[0, 3] = EvaluateBerenstein(3, u);

			temp[1, 0] = EvalueateBerensteinDerivative(0, v);
			temp[1, 1] = EvalueateBerensteinDerivative(1, v);
			temp[1, 2] = EvalueateBerensteinDerivative(2, v);
			temp[1, 3] = EvalueateBerensteinDerivative(3, v);

			return SumFunctions() * 3;
		}

		private Vector3 SumFunctions()
		{
			var sum = new Vector3();
			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
					sum += pointsOrdererd[j, i].Position * temp[0, i] * temp[1, j];


			return sum;
		}

		private double EvaluateBerenstein(int n, double val)
		{
			double neg = 1 - val;
			switch (n)
			{
				case 0:
					return neg * neg * neg;
				case 1:
					return 3 * neg * neg * val;
				case 2:
					return 3 * neg * val * val;
				case 3:
					return val * val * val;
				default:
					throw new ArgumentException("bad n value");
			}
		}

		private double EvalueateBerensteinDerivative(int n, double val)
		{
			double neg = 1 - val;
			switch (n)
			{
				case 0:
					return -3 * neg * neg;
				case 1:
					return 3 * (1 - 4 * val + 3 * val * val);
				case 2:
					return 3 * (2 * val - 3 * val * val);
				case 3:
					return 3 * val * val;
				default:
					throw new ArgumentException("bad n value");
			}
		}

		public override string GetName()
		{
			return "Bezier patch " + base.GetName();
		}


		public bool ContainsTwoInCorners(List<CatPoint> catPoints)
		{
			return false;
			int contained = 0;

			if (catPoints.Contains(pointsOrdererd[0, 0]))
				contained++;
			if (catPoints.Contains(pointsOrdererd[0, 3]))
				contained++;
			if (catPoints.Contains(pointsOrdererd[3, 0]))
				contained++;
			if (catPoints.Contains(pointsOrdererd[3, 3]))
				contained++;

			return contained >= 2;
		}

		public bool ContainsInCorner(CatPoint catPoint)
		{
			return false;
			return pointsOrdererd[0, 0] == catPoint || pointsOrdererd[3, 0] == catPoint || pointsOrdererd[0, 3] == catPoint || pointsOrdererd[3, 3] == catPoint;
		}

		public bool ContainsNearby(CatPoint prev, CatPoint next)
		{
			return false;
			int ind;
			if (pointsOrdererd[0, 0] == prev)
				ind = 0;
			else if (pointsOrdererd[0, 3] == prev)
				ind = 3;
			else if (pointsOrdererd[3, 0] == prev)
				ind = 12;
			else if (pointsOrdererd[3, 3] == prev)
				ind = 15;
			else
				throw new Exception("Something failed");

			switch (ind)
			{
				case 0:
					return pointsOrdererd[0, 3] == next || pointsOrdererd[3, 0] == next;
				case 3:
					return pointsOrdererd[0, 0] == next || pointsOrdererd[3, 3] == next;
				case 12:
					return pointsOrdererd[0, 0] == next || pointsOrdererd[3, 3] == next;
				case 15:
					return pointsOrdererd[0, 3] == next || pointsOrdererd[3, 0] == next;

			}
			return false;
		}

		private Vector2 IndexOf(CatPoint point)
		{
			for (int i = 0; i < pointsOrdererd.GetLength(1); i++)
				for (int j = 0; j < pointsOrdererd.GetLength(0); j++)
					if (pointsOrdererd[j, i] == point)
						return new Vector2(i, j);
			return new Vector2(-1, -1);
		}



		public HalfPatchData GetDataBetweenPoints(CatPoint first, CatPoint second)
		{
			//var firstInd = IndexOf(first);
			//var secondInd = IndexOf(second);
			//var back = new CatPoint[4];
			//var nearest = new CatPoint[4];

			//switch (firstInd)
			//{
			//	case 0:
			//		switch (secondInd)
			//		{
			//			case 3:
			//				nearest[0] = points[0];
			//				nearest[1] = points[1];
			//				nearest[2] = points[2];
			//				nearest[3] = points[3];
			//				back[0] = points[4];
			//				back[1] = points[5];
			//				back[2] = points[6];
			//				back[3] = points[7];
			//				break;
			//			case 12:
			//				nearest[0] = points[0];
			//				nearest[1] = points[4];
			//				nearest[2] = points[8];
			//				nearest[3] = points[12];
			//				back[0] = points[1];
			//				back[1] = points[5];
			//				back[2] = points[9];
			//				back[3] = points[13];
			//				break;
			//		}
			//		break;
			//	case 3:
			//		switch (secondInd)
			//		{
			//			case 0:
			//				nearest[0] = points[3];
			//				nearest[1] = points[2];
			//				nearest[2] = points[1];
			//				nearest[3] = points[0];
			//				back[0] = points[7];
			//				back[1] = points[6];
			//				back[2] = points[5];
			//				back[3] = points[4];
			//				break;
			//			case 15:
			//				nearest[0] = points[3];
			//				nearest[1] = points[7];
			//				nearest[2] = points[11];
			//				nearest[3] = points[15];
			//				back[0] = points[2];
			//				back[1] = points[6];
			//				back[2] = points[10];
			//				back[3] = points[14];
			//				break;
			//		}
			//		break;
			//	case 12:
			//		switch (secondInd)
			//		{
			//			case 0:
			//				nearest[0] = points[12];
			//				nearest[1] = points[8];
			//				nearest[2] = points[4];
			//				nearest[3] = points[0];
			//				back[0] = points[13];
			//				back[1] = points[9];
			//				back[2] = points[5];
			//				back[3] = points[1];
			//				break;
			//			case 15:
			//				nearest[0] = points[12];
			//				nearest[1] = points[13];
			//				nearest[2] = points[14];
			//				nearest[3] = points[15];
			//				back[0] = points[8];
			//				back[1] = points[9];
			//				back[2] = points[10];
			//				back[3] = points[11];
			//				break;
			//		}
			//		break;
			//	case 15:
			//		switch (secondInd)
			//		{
			//			case 3:
			//				nearest[0] = points[15];
			//				nearest[1] = points[11];
			//				nearest[2] = points[7];
			//				nearest[3] = points[3];
			//				back[0] = points[14];
			//				back[1] = points[10];
			//				back[2] = points[6];
			//				back[3] = points[2];
			//				break;
			//			case 12:
			//				nearest[0] = points[15];
			//				nearest[1] = points[14];
			//				nearest[2] = points[13];
			//				nearest[3] = points[12];
			//				back[0] = points[11];
			//				back[1] = points[10];
			//				back[2] = points[9];
			//				back[3] = points[8];
			//				break;
			//		}
			//		break;
			//}


			return null;// new HalfPatchData(back, nearest);
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
