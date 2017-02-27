using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;

namespace CadCat.GeometryModels
{
	class Cube : DataStructures.Model
	{
		public override IEnumerable<Line> GetLines()
		{
			var line = new Line();
			line.from.X = 0;
			line.from.Y = 0;
			line.from.Z = 0;
			line.to.X = 1;
			line.to.Y = 0;
			line.to.Z = 0;
			yield return line;
			line.to.X = 0;
			line.to.Y = 1;
			yield return line;
			line.to.Y = 0;
			line.to.Z = 1;
			yield return line;
			line.from.X = 1;
			line.from.Y = 1;
			line.to.X = 1;
			line.to.Y = 0;
			line.to.Z = 0;
			yield return line;
			line.to.X = 0;
			line.to.Y = 1;
			yield return line;
			line.to.Y = 0;
			line.to.Z = 1;
			line.to.X = 1;
			yield return line;
			line.from.X = 0;
			line.from.Y = 1;
			line.from.Z = 1;
			line.to.X = 0;
			line.to.Y = 0;
			line.to.Z = 1;
			yield return line;
			line.to.Y = 1;
			line.to.Z = 0;
			yield return line;
			line.to.X = 1;
			line.to.Z = 1;
			yield return line;


			line.from.X = 1;
			line.from.Z = 1;
			line.from.Y = 0;
			line.to.X = 1;
			line.to.Y = 0;
			line.to.Z = 0;
			yield return line;
			line.to.X = 0;
			line.to.Z = 1;
			yield return line;
			line.to.X = 1;
			line.to.Y = 1;
			yield return line;
		}
	}
}
