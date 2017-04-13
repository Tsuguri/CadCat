using System.Collections.Generic;
using System.Linq;
using CadCat.DataStructures;
using CadCat.Rendering;

namespace CadCat.GeometryModels
{
	class Bezier : BezierCurveBase
	{

		public Bezier(IEnumerable<DataStructures.CatPoint> pts, SceneData data) : base(pts,data)
		{
		}

		public override void Render(BaseRenderer renderer)
		{
			CountBezierPoints(points.Select(x => x.Point.Position).ToList());
			base.Render(renderer);

			var pointList = points.Select(x => x.Point.Position).ToList();
			if (ShowPolygon)
			{
				renderer.Points = pointList;
				renderer.Transform();
				renderer.DrawLines();
			}

		}

		public override string GetName()
		{
			return "Bezier " + base.GetName();
		}

	}
}
