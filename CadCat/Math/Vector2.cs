using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadCat.Math
{
	using Real = System.Double;

	public struct Vector2
	{
		public Real X { get; set; }
		public Real Y { get; set; }

		public Vector2(Real x = 0.0f, Real y = 0.0f)
		{
			X = x;
			Y = y;
		}

		public Real LengthSquared()
		{
			return (this * this).Sum();
		}

		public Real Length()
		{
			return System.Math.Sqrt(LengthSquared());
		}

		public Real Sum()
		{
			return X + Y;
		}

		public static Vector2 operator +(Vector2 vec1, Vector2 vec2)
		{
			return new Vector2(vec1.X + vec2.X, vec1.Y + vec2.Y);
		}

		public static Vector2 operator -(Vector2 vec1, Vector2 vec2)
		{
			return new Vector2(vec1.X - vec2.X, vec1.Y - vec2.Y);
		}

		public static Vector2 operator -(Vector2 vec, Real scalar)
		{
			return new Vector2(vec.X - scalar, vec.Y - scalar);
		}

		public static Vector2 operator *(Vector2 vec, double scalar)
		{
			return new Vector2(vec.X * scalar, vec.Y * scalar);
		}

		public static Vector2 operator *(Vector2 vec1, Vector2 vec2)
		{
			return new Vector2(vec1.X * vec2.X, vec1.Y * vec2.Y);
		}

		public override string ToString()
		{
			return "v2: "+X + " " + Y;
		}
	}
}
