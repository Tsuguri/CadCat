using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	}
}
