using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadCat.ModelInterfaces
{
	interface IChangeablePointCount
	{
		void AddPoint(DataStructures.CatPoint point);
		void RemovePoint(DataStructures.CatPoint point);
	}
}
