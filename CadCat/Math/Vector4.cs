using System;
using System.Dynamic;

namespace CadCat.Math
{
	using Real = System.Double;

	public struct Vector4
	{
		public Real X, Y, Z, W;

		public Vector4(Real x, Real y, Real z, Real w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public Vector4(Vector3 v3, Real point = 1.0)
		{
			X = v3.X;
			Y = v3.Y;
			Z = v3.Z;
			W = point;
		}

		public Vector4(Vector2 v2, Real z, Real w = 1.0)
		{
			X = v2.X;
			Y = v2.Y;
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

		public Vector3 ClipToVector3()
		{
			return new Vector3(X, Y, Z);
		}

		public Matrix4 MatrixMultiply(Vector4 vec2)
		{
			var mat = new Matrix4();
			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
					mat[i, j] = this[i] * vec2[j];
			return mat;
		}

		public static Vector4 operator /(Vector4 vec, double val)
		{
			return new Vector4(vec.X / val, vec.Y / val, vec.Z / val, vec.W / val);
		}

		public static Vector4 operator *(Vector4 vec, double val)
		{
			return new Vector4(vec.X * val, vec.Y * val, vec.Z * val, vec.W * val);
		}

		public static Vector4 operator +(Vector4 vec, Vector4 vec2)
		{
			return new Vector4(vec.X + vec2.X, vec.Y + vec2.Y, vec.Z + vec2.Z, vec.W + vec2.W);
		}

		public static Vector4 operator -(Vector4 vec, Vector4 vec2)
		{
			return new Vector4(vec.X - vec2.X, vec.Y - vec2.Y, vec.Z - vec2.Z, vec.W - vec2.W);
		}

		public double this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return X;
					case 1:
						return Y;
					case 2:
						return Z;
					case 3:
						return W;
					default:
						throw new ArgumentException("Index out of boundary values");
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						X = value;
						return;
					case 1:
						Y = value;
						return;
					case 2:
						Z = value;
						return;
					case 3:
						W = value;
						return;
					default:
						throw new ArgumentException("Index out of boundary values");
				}
			}
		}
	}
}
