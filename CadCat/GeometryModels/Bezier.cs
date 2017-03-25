using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using CadCat.Math;

namespace CadCat.GeometryModels
{
	class Bezier : Model, IChangeablePointCount
	{

		private List<DataStructures.CatPoint> points;

		public bool ShowPolygon { get; set; }


		public Bezier(IEnumerable<DataStructures.CatPoint> pts)
		{
			points = pts.ToList();

		}

		public override IEnumerable<Line> GetLines()
		{
			var line = new Line();
			for (int i = 0; i < points.Count - 1; i++)
			{
				line.from = points[i].Position;
				line.to = points[i + 1].Position;
				yield return line;
			}
		}

		public void AddPoint(CatPoint point)
		{
			points.Add(point);
		}

		public void RemovePoint(CatPoint point)
		{
			points.Remove(point);
		}

		public override Matrix4 GetMatrix(bool overrideScale, Vector3 newScale)
		{
			return Matrix4.CreateIdentity();
		}

		public override string GetName()
		{
			return "Bezier "+base.GetName();
		}

	}
}
