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
			return new Vector3(vec1.X + vec2.X, vec1.Y + vec2.Y, vec1.Z + vec2.Z);
		}

		public static Vector3 operator +(Vector3 vec1, float val)
		{
			return new Vector3(vec1.X+val,vec1.Y+val,vec1.Z+val);
		}

		public static Vector3 operator -(Vector3 vec1, Vector3 vec2)
		{
			return new Vector3(vec1.X - vec2.X, vec1.Y - vec2.Y, vec1.Z - vec2.Z);
		}

		public static Vector3 operator *(Vector3 vec, double scalar)
		{
			return new Vector3(vec.X * scalar, vec.Y * scalar, vec.Z * scalar);
		}

		public static Vector3 operator /(Vector3 vec, double scalar)
		{
			return vec * (1 / scalar);
		}


		public static Vector3 operator *(Vector3 vec1, Vector3 vec2)
		{
			return new Vector3(vec1.X * vec2.X, vec1.Y * vec2.Y, vec1.Z * vec2.Z);
		}

		public Real Sum()
		{
			return X + Y + Z;
		}

		public Vector3 Normalized()
		{
			var length = System.Math.Sqrt(X * X + Y * Y + Z * Z);
			if (System.Math.Abs(length) < 0.001)
				return this;
			return new Vector3(X / length, Y / length, Z / length);
		}

		public Real LengthSquared()
		{
			return (this * this).Sum();
		}

		public Real Length()
		{
			return System.Math.Sqrt(LengthSquared());
		}

		public static Vector3 CrossProduct(Vector3 first, Vector3 second)
		{

			var cx = first.Y * second.Z - first.Z * second.Y;
			var cy = first.Z * second.X - first.X * second.Z;
			var cz = first.X * second.Y - first.Y * second.X;
			return new Vector3(cx, cy, cz);
		}

		public static double DotProduct(Vector3 first, Vector3 second)
		{
			return (first * second).Sum();
		}

		public static Vector3 Lerp(Vector3 vec1, Vector3 vec2, double point)
		{
			return vec1 * (1 - point) + vec2 * point;
		}

		public GM1.Serialization.Vector3 ToShitpoint()
		{
			return new GM1.Serialization.Vector3 {X = (float)X, Y = (float)Y, Z = (float)Z};
		}
	}
}
