using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;

namespace CadCat.GeometryModels
{
	using Real = System.Double;
	class Torus : DataStructures.Model
	{
		private List<ModelLine> lines;
		private Math.Vector3[] points;

		public Torus()
		{
			Real bigRadius = 5;
			Real smallRadius = 1;
			int bigAngleCount = 16;
			int smallAngleCount = 16;
			GenerateModel(bigRadius, smallRadius, bigAngleCount, smallAngleCount);
		}

		private void GenerateModel(Real bigRadius, Real smallRadius, int bigAngleDensity, int smallAngleDensity)
		{
			Real bigStep = Math.Utils.PI * 2 / bigAngleDensity;
			Real smallStep = Math.Utils.PI * 2 / smallAngleDensity;
			lines = new List<ModelLine>(bigAngleDensity * smallAngleDensity * 2);
			points = new Math.Vector3[bigAngleDensity * smallAngleDensity];
			int i = 0, j = 0;
			for (Real bigAngle = 0.0; bigAngle < 2 * Math.Utils.PI; bigAngle += bigStep, i++)
			{
				j = 0;
				for (Real smallAngle = 0.0; smallAngle < 2 * Math.Utils.PI; smallAngle += smallStep, j++)
				{
					points[i * bigAngleDensity + j] = CalculatePoint(bigAngle, smallAngle, bigRadius, smallRadius);
				}
			}

			for (i = 0; i < bigAngleDensity; i++)
			{
				int circleStart = i * bigAngleDensity;
				for (j = 0; j < smallAngleDensity - 1; j++)
				{
					lines.Add(new ModelLine(circleStart + j, circleStart + j + 1));
				}
				lines.Add(new ModelLine(circleStart + j, circleStart));
			}
			int vertexCount = points.Length;
			for(i=0;i<bigAngleDensity;i++)
			{
				int circleStart = i * bigAngleDensity;
				for(j=0;j<smallAngleDensity;j++)
				{
					lines.Add(new ModelLine(circleStart+j,(circleStart+j+bigAngleDensity)%vertexCount));
				}
			}

		}

		private Math.Vector3 CalculatePoint(Real bigAngle, Real smallAngle, Real bigRadius, Real smallRadius)
		{
			Math.Vector3 ret = new Math.Vector3();
			ret.X = System.Math.Cos(bigAngle) * (bigRadius + smallRadius * System.Math.Cos(smallAngle));
			ret.Y = System.Math.Sin(bigAngle) * (bigRadius + smallRadius * System.Math.Cos(smallAngle));
			ret.Z = smallRadius * System.Math.Sin(smallAngle);
			return ret;
		}
		public override IEnumerable<Line> GetLines()
		{
			var resultLine = new Line();
			foreach (var line in lines)
			{
				resultLine.from = points[line.from];
				resultLine.to = points[line.to];
				yield return resultLine;
			}
		}
		public override string GetName()
		{
			return "Torus"+ base.GetName();
		}
	}
}
