using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.Math;
using CadCat.DataStructures;

namespace CadCat.GeometryModels
{
	using Real = System.Double;

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
	public class Model : Utilities.BindableObject
	{
		public Transform transform;

		public int ModelID
		{
			get;
			private set;
		}

		public string Name
		{
			get
			{
				return GetName();
			}
		}

		public Real TrPosX
		{
			get
			{
				return transform.Position.X;
			}
			set
			{
				transform.Position.X = value;
				OnPropertyChanged();
			}
		}
		public Real TrPosY
		{
			get
			{
				return transform.Position.Y;
			}
			set
			{
				transform.Position.Y = value;
				OnPropertyChanged();
			}
		}
		public Real TrPosZ
		{
			get
			{
				return transform.Position.Z;
			}
			set
			{
				transform.Position.Z = value;
				OnPropertyChanged();
			}
		}

		public Real TrRotX
		{
			get
			{
				return Math.Utils.RadToDeg(transform.Rotation.X);
			}
			set
			{
				Real deg = value;
				if (deg > 360)
					deg -= 360.0;
				if (deg < 0)
					deg += 360.0;
				transform.Rotation.X = Math.Utils.DegToRad(deg);
				OnPropertyChanged();
			}
		}
		public Real TrRotY
		{
			get
			{
				return Math.Utils.RadToDeg(transform.Rotation.Y);
			}
			set
			{
				Real deg = value;
				if (deg > 360)
					deg -= 360.0;
				if (deg < 0)
					deg += 360.0;
				transform.Rotation.Y = Math.Utils.DegToRad(deg);
				OnPropertyChanged();
			}
		}
		public Real TrRotZ
		{
			get
			{
				return Math.Utils.RadToDeg(transform.Rotation.Z);
			}
			set
			{
				Real deg = value;
				if (deg > 360)
					deg -= 360.0;
				if (deg < 0)
					deg += 360.0;
				transform.Rotation.Z = Math.Utils.DegToRad(deg);
				OnPropertyChanged();
			}
		}

		public Real TrScaleX
		{
			get
			{
				return transform.Scale.X;
			}
			set
			{
				transform.Scale.X = value;
				OnPropertyChanged();
			}
		}
		public Real TrScaleY
		{
			get
			{
				return transform.Scale.Y;
			}
			set
			{
				transform.Scale.Y = value;
				OnPropertyChanged();
			}
		}
		public Real TrScaleZ
		{
			get
			{
				return transform.Scale.Z;
			}
			set
			{
				transform.Scale.Z = value;
				OnPropertyChanged();
			}
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
