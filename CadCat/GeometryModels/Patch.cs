﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.Math;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace CadCat.GeometryModels
{
	public abstract class Patch : Model
	{
		protected bool changed;
		private bool showPolygon = true;

		public bool ShowPolygon
		{
			get { return showPolygon; }
			set
			{
				showPolygon = value;
				OnPropertyChanged();
			}
		}


		private bool showNormal = true;

		public bool ShowNormal
		{
			get { return showNormal; }
			set
			{
				showNormal = value;
				OnPropertyChanged();
			}
		}


		private int heightDiv = 3;
		private int widthDiv = 3;


		public int UPos { get; set; }
		public int VPos { get; set; }

		public int WidthDiv
		{
			get { return widthDiv; }
			set
			{
				if (widthDiv != value && value > 0)
				{
					widthDiv = value;
					changed = true;
					OnPropertyChanged();
				}
			}
		}

		public int HeightDiv
		{
			get { return heightDiv; }
			set
			{
				if (heightDiv != value && value > 0)
				{
					heightDiv = value;
					changed = true;
					OnPropertyChanged();
				}
			}
		}

		public abstract Vector3 GetPoint(double u, double v);
		public abstract Vector3 GetUDerivative(double u, double v);
		public abstract Vector3 GetVDerivative(double u, double v);
	}
}
