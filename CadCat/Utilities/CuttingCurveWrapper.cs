using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.GeometryModels;
using CadCat.ModelInterfaces;

namespace CadCat.Utilities
{
	public class CuttingCurveWrapper : BindableObject
	{
		public readonly CuttingCurve curve;
		public string Name => curve.Name;
		private bool side;
		private IIntersectable parent;


		public CuttingCurveWrapper(CuttingCurve curve, IIntersectable parent)
		{
			this.parent = parent;
			this.curve = curve;
		}
		public bool Side
		{
			get { return side; }
			set
			{
				if (side != value)
				{
					side = value;
					parent.SideChanged();
					OnPropertyChanged();
				}

			}
		}
	}
}
