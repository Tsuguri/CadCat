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


		public Elipsoide()
		{
			Handler();
		}

		private void Handler()
		{
			base.PropertyChanged += Elipsoide_PropertyChanged;
			changed = true;
		}

		private void Elipsoide_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			changed = true;
		}

		public double A
		{
			get
			{
				return a;
			}
			set
			{
				changed = true;
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
				changed = true;
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
				changed = true;
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
				changed = true;
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
