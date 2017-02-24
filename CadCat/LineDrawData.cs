using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace CadCat
{
	class Line
	{
		public Point from;
		public Point to;
	}
	class LineDrawData
	{
		Line line;
		
		public void RandomizePoints()
		{
			Random rand = new Random();
			line = new Line();
			line.from = new Point(rand.Next(100), rand.Next(100));
			line.to = new Point(rand.Next(100)+100, rand.Next(100)+100);
		}

		public IEnumerable<Line> GetLines()
		{
			yield return line;
			yield break;
		}


	}
}
