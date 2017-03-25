using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;

namespace CadCat.GeometryModels
{
	using Real = System.Double;
	class Torus : ParametrizedModel
	{
		private List<ModelLine> lines;
		private Math.Vector3[] points;

		private bool modelReady = false;

		private Real bigRadius = 5.0;
		private Real smallRadius = 1.0;
		private int bigAngleDensity = 16;
		private int smallAngleDensity = 16;

		public Real R
		{
			get
			{
				return bigRadius;
			}
			set
			{
				if(bigRadius!=value)
				{
					bigRadius = value;
					modelReady = false;
					OnPropertyChanged();
				}
			}
		}

		public Real r
		{
			get
			{
				return smallRadius;
			}
			set
			{
				if (bigRadius != value)
				{
					smallRadius = value;
					modelReady = false;
					OnPropertyChanged();
				}
			}
		}

		public int RDensity
		{
			get
			{
				return bigAngleDensity;
			}
			set
			{
				if (bigAngleDensity != value)
				{
					bigAngleDensity = value;
					modelReady = false;
					OnPropertyChanged();
				}
			}
		}

		public int rDensity
		{
			get
			{
				return smallAngleDensity;
			}
			set
			{
				if (smallAngleDensity != value)
				{
					smallAngleDensity = value;
					modelReady = false;
					OnPropertyChanged();
				}
			}
		}

		public Torus()
		{
		}

		private void GenerateModel(Real bigRadius, Real smallRadius, int bigAngleDensity, int smallAngleDensity)
		{
			Real bigStep = Math.Utils.PI * 2 / bigAngleDensity;
			Real smallStep = Math.Utils.PI * 2 / smallAngleDensity;
			lines = new List<ModelLine>(bigAngleDensity * smallAngleDensity * 2);
			points = new Math.Vector3[bigAngleDensity * smallAngleDensity];
			for (int i=0; i<bigAngleDensity;  i++)
			{
				Real bigAngle = bigStep * i;
				for (int j=0; j<smallAngleDensity; j++)
				{
					Real smallAngle = j * smallStep;
					points[i * smallAngleDensity + j] = CalculatePoint(bigAngle, smallAngle, bigRadius, smallRadius);
				}
			}

			for (int i = 0; i < bigAngleDensity; i++)
			{
				int circleStart = i * smallAngleDensity;
				for (int j = 0; j < smallAngleDensity - 1; j++)
				{
					lines.Add(new ModelLine(circleStart + j, circleStart + j + 1));
				}
				lines.Add(new ModelLine(circleStart + smallAngleDensity-1, circleStart));
			}
			int vertexCount = points.Length;
			for (int i = 0; i < bigAngleDensity; i++)
			{
				int circleStart = i * smallAngleDensity;
				for (int j = 0; j < smallAngleDensity; j++)
				{
					lines.Add(new ModelLine(circleStart + j, (circleStart + j + smallAngleDensity) % vertexCount));
				}
			}
			modelReady = true;
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
			if (!modelReady)
				GenerateModel(bigRadius,smallRadius,bigAngleDensity,smallAngleDensity);
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
