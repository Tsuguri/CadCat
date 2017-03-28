using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadCat.Math
{
	class Plane
	{
		public Vector3 Normal;
		public Vector3 Point;

		public Plane(Vector3 point, Vector3 normal)
		{
			Normal = normal;
			Point = point;
		}
		public bool RayIntersection(Ray ray, out double distance)
		{
			var dot = Vector3.DotProduct(ray.Direction, Normal);

			if (dot < Utils.Eps)
			{
				distance = 0.0f;
				return false;
			}

			distance = Vector3.DotProduct((Point - ray.Origin), Normal) / dot;
			return true;
		}
	}
}
