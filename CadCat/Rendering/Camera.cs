using CadCat.Math;
namespace CadCat.Rendering
{
	public class Camera
	{
		private Vector3 lookingAt;
		public Vector3 LookingAt
		{
			get
			{
				return lookingAt;
			}
			set
			{
				lookingAt = value;
				MatrixReset = null;
			}
		}

		private Vector3 cameraPos;
		public Vector3 CameraPosition => cameraPos;

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

				MatrixReset = null;
				radius = value;
				if (radius < 1)
					radius = 1;

			}
		}

		private Matrix4 MatrixReset
		{
			// ReSharper disable once ValueParameterNotUsed
			set
			{
				viewProjection = null;
				viewMatrix = null;
				projectionMatrix = null;
			}
		}

		Matrix4 viewProjection;
		Matrix4 viewMatrix;
		Matrix4 projectionMatrix;

		double aspectRatio = 1.33333333333333D;

		public double AspectRatio
		{
			get
			{
				return aspectRatio;
			}
			set
			{
				if (System.Math.Abs(aspectRatio - value) > Utils.Eps)
					MatrixReset = null;
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

		public Matrix4 ViewMatrix
		{
			get
			{
				if (viewMatrix == null)
					CreateViewMatrix();
				return viewMatrix;
			}
		}

		public Matrix4 ProjectionMatrix => projectionMatrix ?? (projectionMatrix = CreateFrustum());

		public Vector3 UpVector => (ViewMatrix.Inversed() * new Vector4(0, 1, 0, 0)).ClipToVector3().Normalized();

		public Vector3 RightVector => (ViewMatrix.Inversed() * new Vector4(1, 0, 0, 0)).ClipToVector3().Normalized();


		private void CreateViewProjectionMatrix()
		{
			viewProjection = ProjectionMatrix * ViewMatrix;
		}

		private void CreateViewMatrix()
		{
			var transRadius = CreateTransRadius();
			var rot = CreateAngleRotation();
			var trans = CreateTargetTranslation();

			var revRot = CreateReversedRotation();
			var pos = LookingAt + (revRot * new Vector4(0, 0, -Radius, 1)).ToNormalizedVector3();
			cameraPos = new Vector3(pos.X, pos.Y, pos.Z);
			//Console.WriteLine($"camera: {cameraPos.X}, {cameraPos.Y}, {cameraPos.Z}");
			viewMatrix = transRadius * rot * trans;
		}

		public Matrix4 GetRightEyeMatrix(double halfEyeSeparation, double focusDepth)
		{
			double fovy = Utils.DegToRad(45);
			double zNear = 1;
			double zFar = 60;
			double top = zNear * System.Math.Tan(0.5 * fovy);
			double bottom = -top;
			double left = bottom * AspectRatio;
			double right = top * AspectRatio;


			double frustumShift = halfEyeSeparation * zNear / focusDepth;


			var view = Matrix4.CreatePerspectiveOffCenter(left - frustumShift, right - frustumShift, bottom, top, zNear, zFar) * Matrix4.CreateTranslation((float)halfEyeSeparation);
			var transRadius = CreateTransRadius();
			var rot = CreateAngleRotation();
			var trans = CreateTargetTranslation();
			return view * transRadius * rot * trans;
		}

		public Matrix4 GetLeftEyeMatrix(double halfEyeSeparation, double focusDepth)
		{
			double fovy = Utils.DegToRad(45);
			double zNear = 1;
			double zFar = 60;
			double top = zNear * System.Math.Tan(0.5 * fovy);
			double bottom = -top;
			double left = bottom * AspectRatio;
			double right = top * AspectRatio;


			double frustumShift = halfEyeSeparation * zNear / focusDepth;


			var view = Matrix4.CreatePerspectiveOffCenter(left + frustumShift, right + frustumShift, bottom, top, zNear, zFar) * Matrix4.CreateTranslation(-(float)halfEyeSeparation);
			var transRadius = CreateTransRadius();
			var rot = CreateAngleRotation();
			var trans = CreateTargetTranslation();
			return view * transRadius * rot * trans;
		}

		public Matrix4 CreateFrustum()
		{
			return Matrix4.CreateFrustum(Utils.DegToRad(60), aspectRatio, 0.1, 100);
		}


		public Matrix4 CreateTransRadius()
		{
			return Matrix4.CreateTranslation(0, 0, Radius);
		}
		public Matrix4 CreateAngleRotation()
		{
			return Matrix4.CreateRotation(Utils.DegToRad(-VerticalAngle), Utils.DegToRad(-HorizontalAngle), 0);
		}
		private Matrix4 CreateReversedRotation()
		{
			//return Matrix4.CreateRotation(Utils.DegToRad(VerticalAngle), Utils.DegToRad(HorizontalAngle), 0);
			return Matrix4.CreateRotationY(Utils.DegToRad(HorizontalAngle)) * Matrix4.CreateRotationX(Utils.DegToRad(VerticalAngle));
		}
		public Matrix4 CreateTargetTranslation()
		{
			return Matrix4.CreateTranslation(-LookingAt.X, -LookingAt.Y, -LookingAt.Z);
		}

		public void Rotate(double vertical, double horizontal)
		{
			HorizontalAngle += horizontal;
			VerticalAngle -= vertical;
			if (HorizontalAngle < 0)
				HorizontalAngle += 360;
			if (HorizontalAngle > 360)
				HorizontalAngle -= 360;

			if (VerticalAngle > 90)
				VerticalAngle = 90;
			if (VerticalAngle < -90)
				VerticalAngle = -90;

			MatrixReset = null;
		}

		public void Move(double dX, double dY, double dZ = 0.0)
		{
			var trans = new Vector3(dX, -dY, dZ * Radius);
			var trans2 = Matrix4.CreateRotationY(Utils.DegToRad(180 + HorizontalAngle)) * Matrix4.CreateRotationX(Utils.DegToRad(-VerticalAngle)) * trans;
			LookingAt += trans2 * Radius * 0.001;
			MatrixReset = null;
		}

		public Ray GetViewRay(Vector2 position)
		{
			var inverseView = ViewMatrix.Inversed();
			var inverseProjection = ProjectionMatrix.Inversed();

			Vector4 direction = new Vector4(position, 1.0f);

			direction = inverseView * inverseProjection * direction;

			Vector3 origin = CameraPosition;
			var normDir = (direction.ToNormalizedVector3() - origin).Normalized();
			return new Ray(origin, normDir);
		}

		public Vector3 GetScreenPointOnViewPlane(Vector2 position)
		{
			Ray ray = GetViewRay(position);

			var plane = new Plane(LookingAt, (LookingAt - CameraPosition).Normalized());
			double distance;
			if (plane.RayIntersection(ray, out distance))
			{
				return ray.GetPoint(distance);
			}
			return new Vector3(-1, -1, -1);

		}
	}
}
