﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.Math;

namespace CadCat.DataStructures
{
	internal struct ModelLine
	{
		public int from;
		public int to;

		public ModelLine(int from, int to)
		{
			this.from = from;
			this.to = to;
		}
	}
	public class Model
	{
		public Transform transform;
		public int ModelID
		{
			get;
			private set;
		}

		private static int idCounter = 0;

		public Model()
		{
			transform = new Transform();
			ModelID = idCounter;
			idCounter++;
		}
		public virtual IEnumerable<Line> GetLines()
		{
			var line = new Line();
			line.from = new Vector3(10, 10);
			line.to = new Vector3(100, 100);

			for (int i = 0; i < 32; i++)
				for (int j = 0; j < 32; j++)
				{
					line.from.X = i * 40;
					line.from.Y = j * 40 + 100;
					line.to.X = 50;
					line.to.Y = 50;
					yield return line;
				}

		}

		public virtual string GetName()
		{
			return ModelID.ToString();
		}
	}
}
