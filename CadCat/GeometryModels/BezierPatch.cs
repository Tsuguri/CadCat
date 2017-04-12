using CadCat.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace CadCat.GeometryModels
{
	class BezierPatch : Model
	{
		private CatPoint[,] points;

		public BezierPatch()
		{

		}

		public BezierPatch(CatPoint[,] points)
		{
			Debug.Assert(points.Length == 16);
			Debug.Assert(points.GetLength(0) == 4);


		}
	}
}
