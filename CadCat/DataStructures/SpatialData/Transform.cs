using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.Math;

namespace CadCat.DataStructures.SpatialData
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

		public Transform(Vector3 position)
		{
			Position = position;
			Rotation = new Vector3();
			Scale = new Vector3(1.0, 1.0, 1.0);
		}

		public Matrix4 CreateTransformMatrix(bool overrideScale = false, Vector3 newScale=new Vector3())
		{
			return Matrix4.CreateTranslation(Position) * Matrix4.CreateRotation(Rotation) * (Matrix4.CreateScale(overrideScale ? newScale : Scale));
		}

	}
}
