using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadCat.Math
{
	using Real = System.Double;

	public class Matrix4
	{
		Real m11, m12, m13, m14;
		Real m21, m22, m23, m24;
		Real m31, m32, m33, m34;
		Real m41, m42, m43, m44;

		public Matrix4(Real m11 = 0, Real m12 = 0, Real m13 = 0, Real m14 = 0,
						Real m21 = 0, Real m22 = 0, Real m23 = 0, Real m24 = 0,
						Real m31 = 0, Real m32 = 0, Real m33 = 0, Real m34 = 0,
						Real m41 = 0, Real m42 = 0, Real m43 = 0, Real m44 = 0)
		{
			this.m11 = m11;
			this.m12 = m12;
			this.m13 = m13;
			this.m14 = m14;
			this.m21 = m21;
			this.m22 = m22;
			this.m23 = m23;
			this.m24 = m24;
			this.m31 = m31;
			this.m32 = m32;
			this.m33 = m33;
			this.m34 = m34;
			this.m41 = m41;
			this.m42 = m42;
			this.m43 = m43;
			this.m44 = m44;
		}

		public Real this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return m11;
					case 1:
						return m12;
					case 2:
						return m13;
					case 3:
						return m14;
					case 4:
						return m21;
					case 5:
						return m22;
					case 6:
						return m23;
					case 7:
						return m24;
					case 8:
						return m31;
					case 9:
						return m32;
					case 10:
						return m33;
					case 11:
						return m34;
					case 12:
						return m41;
					case 13:
						return m42;
					case 14:
						return m43;
					case 15:
						return m44;
					default:
						throw new Exception("matrix index out of range");
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						m11 = value;
						break;
					case 1:
						m12 = value;
						break;
					case 2:
						m13 = value;
						break;
					case 3:
						m14 = value;
						break;
					case 4:
						m21 = value;
						break;
					case 5:
						m22 = value;
						break;
					case 6:
						m23 = value;
						break;
					case 7:
						m24 = value;
						break;
					case 8:
						m31 = value;
						break;
					case 9:
						m32 = value;
						break;
					case 10:
						m33 = value;
						break;
					case 11:
						m34 = value;
						break;
					case 12:
						m41 = value;
						break;
					case 13:
						m42 = value;
						break;
					case 14:
						m43 = value;
						break;
					case 15:
						m44 = value;
						break;
					default:
						throw new Exception("matrix index out of range");
				}
			}
		}
		public Real this[int row, int column]
		{
			get
			{
				switch (row)
				{
					case 0:
						switch (column)
						{
							case 0:
								return m11;
							case 1:
								return m12;
							case 2:
								return m13;
							case 3:
								return m14;
							default:
								throw new Exception("matrix column index out of range");
						}
					case 1:
						switch (column)
						{
							case 0:
								return m21;
							case 1:
								return m22;
							case 2:
								return m23;
							case 3:
								return m24;
							default:
								throw new Exception("matrix column index out of range");
						}
					case 2:
						switch (column)
						{
							case 0:
								return m31;
							case 1:
								return m32;
							case 2:
								return m33;
							case 3:
								return m34;
							default:
								throw new Exception("matrix column index out of range");
						}
					case 3:
						switch (column)
						{
							case 0:
								return m41;
							case 1:
								return m42;
							case 2:
								return m43;
							case 3:
								return m44;
							default:
								throw new Exception("matrix column index out of range");
						}
					default:
						throw new Exception("matrix row index out of range");
				}
			}
			set
			{
				switch (row)
				{
					case 0:
						switch (column)
						{
							case 0:
								m11 = value;
								break;
							case 1:
								m12 = value;
								break;
							case 2:
								m13 = value;
								break;
							case 3:
								m14 = value;
								break;
							default:
								throw new Exception("matrix column index out of range");
						}
						break;
					case 1:
						switch (column)
						{
							case 0:
								m21 = value;
								break;
							case 1:
								m22 = value;
								break;
							case 2:
								m23 = value;
								break;
							case 3:
								m24 = value;
								break;
							default:
								throw new Exception("matrix column index out of range");
						}
						break;
					case 2:
						switch (column)
						{
							case 0:
								m31 = value;
								break;
							case 1:
								m32 = value;
								break;
							case 2:
								m33 = value;
								break;
							case 3:
								m34 = value;
								break;
							default:
								throw new Exception("matrix column index out of range");
						}
						break;
					case 3:
						switch (column)
						{
							case 0:
								m41 = value;
								break;
							case 1:
								m42 = value;
								break;
							case 2:
								m43 = value;
								break;
							case 3:
								m44 = value;
								break;
							default:
								throw new Exception("matrix column index out of range");
						}
						break;
					default:
						throw new Exception("matrix row index out of range");
				}
			}
		}

		public static Vector4 operator *(Matrix4 mat, Vector4 vec)
		{
			var result = new Vector4();
			result.X = mat.m11 * vec.X + mat.m12 * vec.Y + mat.m13 * vec.Z + mat.m14 * vec.W;
			result.Y = mat.m21 * vec.X + mat.m22 * vec.Y + mat.m23 * vec.Z + mat.m24 * vec.W;
			result.Z = mat.m31 * vec.X + mat.m32 * vec.Y + mat.m33 * vec.Z + mat.m34 * vec.W;
			result.W = mat.m41 * vec.X + mat.m42 * vec.Y + mat.m43 * vec.Z + mat.m44 * vec.W;
			return result;
		}

		/// <summary>
		/// Multiplies vector by matrix using 1 in place of fourth vector value.
		/// </summary>
		/// <param name="mat"></param>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector3 operator *(Matrix4 mat, Vector3 vec)
		{
			var result = new Vector3();
			result.X = mat.m11 * vec.X + mat.m12 * vec.Y + mat.m13 * vec.Z + mat.m14;
			result.Y = mat.m21 * vec.X + mat.m22 * vec.Y + mat.m23 * vec.Z + mat.m24;
			result.Z = mat.m31 * vec.X + mat.m32 * vec.Y + mat.m33 * vec.Z + mat.m34;
			return result;
		}
		public static Matrix4 operator *(Matrix4 mat, Real scalar)
		{
			var result = new Matrix4();
			for (int i = 0; i < 16; i++)
				result[i] = mat[i] * scalar;
			return result;
		}

		public static Matrix4 operator *(Matrix4 mat1, Matrix4 mat2)
		{
			var mat = new Matrix4();
			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
					for (int k = 0; k < 4; k++)
					{
						mat[i, j] += mat1[i, k] * mat2[k, j];
					}
			return mat;
		}

		public static Matrix4 CreateIdentity()
		{
			Matrix4 mat = new Matrix4();
			mat.m11 = mat.m22 = mat.m33 = mat.m44 = 1;
			return mat;
		}

		public static Matrix4 CreateTranslation(Vector3 vec)
		{
			return CreateTranslation(vec.X, vec.Y, vec.Z);
		}
		public static Matrix4 CreateTranslation(Real x = 0, Real y = 0, Real z = 0)
		{
			Matrix4 mat = CreateIdentity();
			mat.m14 = x;
			mat.m24 = y;
			mat.m34 = z;
			return mat;
		}

		public static Matrix4 CreateRotationX(Real angle)
		{
			Matrix4 mat = CreateIdentity();
			mat.m22 = mat.m33 = System.Math.Cos(angle);
			mat.m32 = System.Math.Sin(angle);
			mat.m23 = -mat.m32;
			return mat;
		}

		public static Matrix4 CreateRotationY(Real angle)
		{
			Matrix4 mat = CreateIdentity();
			mat.m11 = mat.m33 = System.Math.Cos(angle);
			mat.m13 = System.Math.Sin(angle);
			mat.m31 = -mat.m13;
			return mat;
		}

		public static Matrix4 CreateRotationZ(Real angle)
		{
			Matrix4 mat = CreateIdentity();
			mat.m11 = mat.m22 = System.Math.Cos(angle);
			mat.m12 = -(mat.m21 = System.Math.Sin(angle));
			return mat;
		}

		public static Matrix4 CreateRotation(Vector3 vec)
		{
			return CreateRotation(vec.X, vec.Y, vec.Z);
		}
		public static Matrix4 CreateRotation(Real x, Real y, Real z)
		{
			return CreateRotationX(x) * CreateRotationY(y) * CreateRotationZ(z);
		}

		public static Matrix4 CreateScale(Vector3 vec)
		{
			return CreateScale(vec.X, vec.Y, vec.Z);
		}
		public static Matrix4 CreateScale(Real scale)
		{
			return CreateScale(scale, scale, scale);
		}
		public static Matrix4 CreateScale(Real xScale, Real yScale, Real zScale)
		{
			var mat = CreateIdentity();
			mat.m11 = xScale;
			mat.m22 = yScale;
			mat.m33 = zScale;
			return mat;
		}

		public static Matrix4 CreateProjection(Real r)
		{
			var mat = CreateIdentity();
			mat.m33 = 0;
			mat.m43 = 1 / r;
			return mat;
		}

		public static Matrix4 CreateFrustum(Real fovy, Real aspect, Real near, Real far)
		{
			// Use for LESS depth testing
			Real cot = 1.0 / System.Math.Tan(fovy / 2.0);
			Real diff = 1.0 / (far - near);

			//return new Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 / 5, 0);
			return new Matrix4(
				cot / aspect, 0, 0, 0,
				0, cot, 0, 0,
				0, 0, -(far + near) * diff, 2 * far * near * diff,
				0, 0, 1, 0
			);
		}

		public static Matrix4 CreatePerspectiveOffCenter(double left, double right, double bottom, double top, double zNear, double zFar)
		{

			double sizeX = right - left;
			double sizeY = top - bottom;
			double sizeZ = zFar - zNear;

			double centerX = right + left;
			double centerY = top + bottom;
			double centerZ = zFar + zNear;

			float num1 = (float)(2.0 * zNear / sizeX);
			float num2 = (float)(2.0 * zNear / sizeY);
			float num3 = (float)(centerX / sizeX);
			float num4 = (float)(centerY / sizeY);
			//return new Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 / 5, 0);
			return new Matrix4(
				num1, 0, num3, 0,
				0, num2, num4, 0,
				0, 0, -(centerZ) /sizeZ, 2 * zFar * zNear /sizeZ,
				0, 0, 1, 0
			);
		}

		public Matrix4 Inversed()
		{
			Real tmp1 =
			  m22 * m33 * m44 -
			  m22 * m34 * m43 -
			  m32 * m23 * m44 +
			  m32 * m24 * m43 +
			  m42 * m23 * m34 -
			  m42 * m24 * m33;

			Real tmp2 =
			  -m21 * m33 * m44 +
			  m21 * m34 * m43 +
			  m31 * m23 * m44 -
			  m31 * m24 * m43 -
			  m41 * m23 * m34 +
			  m41 * m24 * m33;

			Real tmp3 =
			  m21 * m32 * m44 -
			  m21 * m34 * m42 -
			  m31 * m22 * m44 +
			  m31 * m24 * m42 +
			  m41 * m22 * m34 -
			  m41 * m24 * m32;

			Real tmp4 =
			  -m21 * m32 * m43 +
			  m21 * m33 * m42 +
			  m31 * m22 * m43 -
			  m31 * m23 * m42 -
			  m41 * m22 * m33 +
			  m41 * m23 * m32;

			Real det = m11 * tmp1 + m12 * tmp2 + m13 * tmp3 + m14 * tmp4;

			if (det == 0.0)
				return new Matrix4();

			Real invdet = 1.0 / det;

			return new Matrix4(
			  tmp1 * invdet,

			  (-m12 * m33 * m44 +
			  m12 * m34 * m43 +
			  m32 * m13 * m44 -
			  m32 * m14 * m43 -
			  m42 * m13 * m34 +
			  m42 * m14 * m33) * invdet,

			  (m12 * m23 * m44 -
			  m12 * m24 * m43 -
			  m22 * m13 * m44 +
			  m22 * m14 * m43 +
			  m42 * m13 * m24 -
			  m42 * m14 * m23) * invdet,

			  (-m12 * m23 * m34 +
			  m12 * m24 * m33 +
			  m22 * m13 * m34 -
			  m22 * m14 * m33 -
			  m32 * m13 * m24 +
			  m32 * m14 * m23) * invdet,

			  tmp2 * invdet,

			  (m11 * m33 * m44 -
			  m11 * m34 * m43 -
			  m31 * m13 * m44 +
			  m31 * m14 * m43 +
			  m41 * m13 * m34 -
			  m41 * m14 * m33) * invdet,

			  (-m11 * m23 * m44 +
			  m11 * m24 * m43 +
			  m21 * m13 * m44 -
			  m21 * m14 * m43 -
			  m41 * m13 * m24 +
			  m41 * m14 * m23) * invdet,

			  (m11 * m23 * m34 -
			  m11 * m24 * m33 -
			  m21 * m13 * m34 +
			  m21 * m14 * m33 +
			  m31 * m13 * m24 -
			  m31 * m14 * m23) * invdet,

			  tmp3 * invdet,

			  (-m11 * m32 * m44 +
			  m11 * m34 * m42 +
			  m31 * m12 * m44 -
			  m31 * m14 * m42 -
			  m41 * m12 * m34 +
			  m41 * m14 * m32) * invdet,

			  (m11 * m22 * m44 -
			  m11 * m24 * m42 -
			  m21 * m12 * m44 +
			  m21 * m14 * m42 +
			  m41 * m12 * m24 -
			  m41 * m14 * m22) * invdet,

			  (-m11 * m22 * m34 +
			  m11 * m24 * m32 +
			  m21 * m12 * m34 -
			  m21 * m14 * m32 -
			  m31 * m12 * m24 +
			  m31 * m14 * m22) * invdet,

			  tmp4 * invdet,

			  (m11 * m32 * m43 -
			  m11 * m33 * m42 -
			  m31 * m12 * m43 +
			  m31 * m13 * m42 +
			  m41 * m12 * m33 -
			  m41 * m13 * m32) * invdet,

			  (-m11 * m22 * m43 +
			  m11 * m23 * m42 +
			  m21 * m12 * m43 -
			  m21 * m13 * m42 -
			  m41 * m12 * m23 +
			  m41 * m13 * m22) * invdet,

			  (m11 * m22 * m33 -
			  m11 * m23 * m32 -
			  m21 * m12 * m33 +
			  m21 * m13 * m32 +
			  m31 * m12 * m23 -
			  m31 * m13 * m22) * invdet
			);
		}
	}
}
