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
        public Vector3 Position { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Rotation { get; set; }


		public Matrix4 CreateTransformMatrix()
		{
			return Matrix4.CreateTranslation(Position.X, Position.Y, Position.Z) * Matrix4.CreateRotation(Rotation.X, Rotation.Y, Rotation.Z);
		}

    }
}
