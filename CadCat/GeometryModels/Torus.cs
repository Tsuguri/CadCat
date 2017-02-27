using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;

namespace CadCat.GeometryModels
{
    class Torus : DataStructures.Model
    {
		public override IEnumerable<Line> GetLines()
		{
			var line1 = new Line();
			var line2 = new Line();
			line1.from.X = line1.from.Y = 0;
			line1.to.X = line1.to.Y = 500;
			line2.from.X = line2.to.Y = 500;
			line2.from.Y = line2.to.X = 0;
			yield return line1;
			yield return line2;
		}
	}
}
