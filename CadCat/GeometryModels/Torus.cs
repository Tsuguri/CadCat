using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;

namespace CadCat.GeometryModels
{
	using Rendering;
	using Real = System.Double;
	class Torus : ParametrizedModel
	{
		//private List<ModelLine> lines;
		private List<int> indices;
		private List<Math.Vector3> points;

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
			points = new List<Math.Vector3>(bigAngleDensity * smallAngleDensity);
			for (int i=0; i<bigAngleDensity;  i++)
			{
				Real bigAngle = bigStep * i;
				for (int j=0; j<smallAngleDensity; j++)
				{
					Real smallAngle = j * smallStep;
					points.Insert(i * smallAngleDensity + j, CalculatePoint(bigAngle, smallAngle, bigRadius, smallRadius));
				}
			}
			indices = new List<int>(bigAngleDensity * smallAngleDensity * 2);
			for (int i = 0; i < bigAngleDensity; i++)
			{
				int circleStart = i * smallAngleDensity;
				for (int j = 0; j < smallAngleDensity - 1; j++)
				{
					indices.Add(circleStart + j);
					indices.Add(circleStart + j + 1);
				}
				indices.Add(circleStart + smallAngleDensity - 1);
				indices.Add(circleStart);
			}

			int vertexCount = points.Count;
			for (int i = 0; i < bigAngleDensity; i++)
			{
				int circleStart = i * smallAngleDensity;
				for (int j = 0; j < smallAngleDensity; j++)
				{
					indices.Add(circleStart + j);
					indices.Add((circleStart + j + smallAngleDensity) % vertexCount);
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

		public override void Render(BaseRenderer renderer)
		{
			base.Render(renderer);
			if (!modelReady)
				GenerateModel(bigRadius, smallRadius, bigAngleDensity, smallAngleDensity);
			renderer.UseIndices = true;
			renderer.Points = points;
			renderer.Indices = indices;
			renderer.ModelMatrix = transform.CreateTransformMatrix();
			renderer.Transform();
			renderer.DrawLines();
		}
		public override string GetName()
		{
			return "Torus"+ base.GetName();
		}
	}
}
