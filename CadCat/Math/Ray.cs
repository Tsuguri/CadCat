namespace CadCat.Math
{
	public struct Ray
	{
		public Vector3 Origin;

		public Vector3 Direction;

		public Ray(Vector3 origin, Vector3 direction)
		{
			Origin = origin;
			Direction = direction;
		}

		public Vector3 GetPoint(double distance)
		{
			return Origin + Direction * distance;
		}
	}
}
