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
		public Point[] points;
		Line line = new Line();
		Random rand = new Random();
		public LineDrawData()
		{
			points = new Point[2];
			points[0] = new Point(rand.Next(100), rand.Next(100));
			points[1] = new Point(rand.Next(100) + 100, rand.Next(100) + 100);

		}
		public void RandomizePoints()
		{
			line.from = new Point(rand.Next(100), rand.Next(100));
			line.to = new Point(rand.Next(100)+100, rand.Next(100)+100);
			points[0] = line.from;
			points[1] = line.to;
		}

		public IEnumerable<Line> GetLines()
		{
			for (int i = 0; i < 1000; i++)
				yield return line;
			yield break;
		}


	}
}
