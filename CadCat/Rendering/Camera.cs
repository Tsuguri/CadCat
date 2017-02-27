using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using CadCat.Math;
namespace CadCat.Rendering
{
	class Camera
	{
		public Transform transform = new Transform();

		Matrix4 viewProjection;

		double aspectRatio = 4 / 3;

		public double AspectRatio
		{
			get
			{
				return aspectRatio;
			}
			set
			{
				if (aspectRatio != value)
					viewProjection = null;
				aspectRatio = value;
			}
		}

		public Matrix4 ViewProjectionMatrix
		{
			get
			{
				if (viewProjection == null)
					CreateViewProjectionMatrix();
				return viewProjection;
			}
		}

		private void CreateViewProjectionMatrix()
		{
			var view = Matrix4.CreateFrustum(Utils.DegToRad(60), aspectRatio, 0.1, 20);
			var rot = Matrix4.CreateRotation(-transform.Rotation.X, -transform.Rotation.Y, -transform.Rotation.Z);
			var trans = Matrix4.CreateTranslation(-transform.Position.X, -transform.Position.Y, -transform.Position.Z);
			viewProjection = view * rot * trans;
		}
		public Camera()
		{

		}
	}
}
