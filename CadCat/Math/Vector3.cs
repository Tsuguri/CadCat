using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadCat.Math
{
	using Real = System.Double;

	public struct Vector3
    {
        public Real X { get; set; }
        public Real Y { get; set; }
        public Real Z { get; set; }

        public Vector3(Real x = 0.0f, Real y = 0.0f, Real z = 0.0f)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
