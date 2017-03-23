using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadCat.DataStructures
{
	struct ClickData
	{
		public double Distance;
		public GeometryModels.Model ClickedModel;

		public ClickData(double distance, GeometryModels.Model clicked)
		{
			Distance = distance;
			ClickedModel = clicked;
		}
	}
}
