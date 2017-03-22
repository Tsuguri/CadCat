using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadCat.Math
{
	public struct Ray
	{
		public Vector3 Origin;

		public Vector3 Direction;

		public Ray(Vector3 origin, Vector3 direction)
		{
			this.Origin = origin;
			this.Direction = direction;
		}

		public Vector3 GetPoint(double distance)
		{
			return Origin + Direction * distance;
		}
	}
}
