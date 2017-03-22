using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadCat.Math
{
	using Real = System.Double;

	public struct Vector3
    {
        public Real X { get; set; }
        public Real Y { get; set; }
        public Real Z { get; set; }

        public Vector3(Real x = 0.0f, Real y = 0.0f, Real z = 0.0f)
        {
            X = x;
            Y = y;
            Z = z;
        }

		public static Vector3 operator +(Vector3 vec1, Vector3 vec2)
		{
			return new Vector3(vec1.X+vec2.X,vec1.Y+vec2.Y,vec1.Z+vec2.Z);
		}

		public static Vector3 operator -(Vector3 vec1, Vector3 vec2)
		{
			return new Vector3(vec1.X - vec2.X, vec1.Y - vec2.Y, vec1.Z - vec2.Z);
		}

		public static Vector3 operator *(Vector3 vec, double scalar)
		{
			return new Vector3(vec.X * scalar, vec.Y * scalar, vec.Z * scalar);
		}

		public Vector3 Normalized()
		{
			var length = System.Math.Sqrt(X * X + Y * Y + Z * Z);
			if (System.Math.Abs(length) < 0.001)
				return this;
			return new Vector3(X / length, Y / length, Z / length);
		}
    }
}
