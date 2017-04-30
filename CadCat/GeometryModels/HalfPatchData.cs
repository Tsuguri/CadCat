using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;

namespace CadCat.GeometryModels
{
	class HalfPatchData
	{
		public CatPoint[] Nearest;
		public CatPoint[] Back;

		public HalfPatchData(CatPoint[] back, CatPoint[] nearest)
		{
			this.Back = back;
			this.Nearest = nearest;
		}
	}
}
