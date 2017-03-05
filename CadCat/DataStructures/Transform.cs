using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.Math;

namespace CadCat.DataStructures
{
    public class Transform
    {
		public Vector3 Position;
		public Vector3 Scale;
		public Vector3 Rotation;

		public Transform()
		{
			Position = new Vector3();
			Rotation = new Vector3();
			Scale = new Vector3(1.0, 1.0, 1.0);
		}

		public Matrix4 CreateTransformMatrix()
		{
			return Matrix4.CreateTranslation(Position * (-1.0)) * Matrix4.CreateRotation(Rotation * (-1.0)) * Matrix4.CreateScale(Scale);
		}

    }
}
