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
		public CatPoint ClickedModel;

		public ClickData(double distance, CatPoint clicked)
		{
			Distance = distance;
			ClickedModel = clicked;
		}
	}
}
