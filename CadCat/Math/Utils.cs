using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CadCat.Math
{
	using Real = System.Double;
	class Utils
	{
		/// <summary>
		/// Math constant: PI ~= 3.14
		/// </summary>
		public static Real PI
		{
			get
			{
				return System.Math.PI;
			}
		}
		public static Real DegToRad(Real deg)
		{
			return deg / 180.0 * PI;
		}

		public static Real RadToDeg(Real rad)
		{
			return rad / PI * 180.0;
		}

		public static Real Eps
		{
			get
			{
				return 0.00001;
			}
		}

		public static List<Vector3> SolveMultidiagonalMatrixEquation(Real[] underDiagonal, Real[] diagonal, Real[] overDiagonal, Vector3[] results)
		{
			Debug.Assert(underDiagonal.Length == diagonal.Length - 1);
			Debug.Assert(overDiagonal.Length == diagonal.Length - 1);
			Debug.Assert(results.Length == diagonal.Length);

			var u = new double[diagonal.Length];
			var l = new double[diagonal.Length];
			var y = new Vector3[diagonal.Length];

			
			u[0] = diagonal[0];
			for (int i = 1; i < diagonal.Length; i++)
			{
				l[i]=(underDiagonal[i-1] / u[i-1]);
				u[i]=(diagonal[i] - l[i] * overDiagonal[i-1]);
			}
			y[0] = results[0];
			for (int i = 1; i < diagonal.Length; i++)
			{
				y[i]=results[i] - y[i-1] * l[i];
			}

			var x = new Vector3[diagonal.Length];
			x[x.Length - 1] = y[y.Length - 1] / u[u.Length - 1];
			for(int i=x.Length-2;i>=0;i--)
			{
				x[i] = (y[i] -  x[i + 1]*overDiagonal[i]) / u[i];
			}
			return x.ToList();
		}
	}
}
