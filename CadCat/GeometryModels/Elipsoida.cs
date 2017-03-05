using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadCat.GeometryModels
{
	class Elipsoide : Model
	{
		private double a = 1;
		private double b = 1;
		private double c = 1;

		public double A
		{
			get
			{
				return a;
			}
			set
			{
				a = value;
				OnPropertyChanged();
			}
		}

		public double B
		{
			get
			{
				return b;
			}
			set
			{
				b = value;
				OnPropertyChanged();
			}
		}

		public double C
		{
			get
			{
				return c;
			}
			set
			{
				c = value;
				OnPropertyChanged();
			}
		}

		private int lightIntensity = 2;
		public int LightIntensity
		{
			get
			{
				return lightIntensity;
			}
			set
			{
				lightIntensity = value;
				OnPropertyChanged();
			}
		}

		public override string GetName()
		{
			return "Elipsoida " + base.GetName();
		}
	}
}
