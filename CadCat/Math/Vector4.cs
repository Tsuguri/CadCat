using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadCat.Math
{
	using Real = System.Double;

	public struct Vector4
    {
		public Real X, Y, Z, W;


		public Vector4(Vector3 v3, Real point = 1.0f)
		{
			X = v3.X;
			Y = v3.Y;
			Z = v3.Z;
			W = point;
		}

		public Vector4(Real x, Real y, Real z, Real w=1.0)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public Vector3 ToNormalizedVector3()
		{
			Vector3 vec = new Vector3();
			vec.X = X / W;
			vec.Y = Y / W;
			vec.Z = Z / W;
			return vec;
		}
    }
}
