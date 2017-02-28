using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using CadCat.Math;
namespace CadCat.Rendering
{
	public class Camera
	{
		public Vector3 LookingAt
		{
			get; set;
		}
		public double HorizontalAngle { get; set; }
		public double VerticalAngle { get; set; }
		private double radius;
		public double Radius
		{
			get
			{
				return radius;
			}
			set
			{

				viewProjection = null;
				radius = value;
				if (radius < 1)
					radius = 1;

			}
		}

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
			var view = CreateFrustum();
			var transRadius = CreateTransRadius();
			var rot = CreateAngleRotation();
			var trans = CreateTargetTranslation();
			viewProjection = view * transRadius * rot * trans;
		}

		public Matrix4 CreateFrustum()
		{
			return Matrix4.CreateFrustum(Utils.DegToRad(60), aspectRatio, 0.1, 20);
		}
		public Matrix4 CreateTransRadius()
		{
			return Matrix4.CreateTranslation(0, 0, Radius);
		}
		public Matrix4 CreateAngleRotation()
		{
			return Matrix4.CreateRotation(Utils.DegToRad(-VerticalAngle), Utils.DegToRad(-HorizontalAngle), 0);
		}
		public Matrix4 CreateTargetTranslation()
		{
			return Matrix4.CreateTranslation(-LookingAt.X, -LookingAt.Y, -LookingAt.Z);
		}

		public Camera()
		{

		}

		public void Rotate(double vertical, double horizontal)
		{
			HorizontalAngle += horizontal;
			VerticalAngle += vertical;
			if (HorizontalAngle < 0)
				HorizontalAngle += 360;
			if (HorizontalAngle > 360)
				HorizontalAngle -= 360;

			if (VerticalAngle > 90)
				VerticalAngle = 90;
			if (VerticalAngle < -90)
				VerticalAngle = -90;

			viewProjection = null;
		}
	}
}
