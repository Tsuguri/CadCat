using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using CadCat.Math;
using CadCat.ModelInterfaces;
using System.Windows.Input;
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
			CountBezierPoints(points.Select(x => x.Point).ToList());
			base.Render(renderer);

			var pointList = points.Select(x => x.Point.Position).ToList();
			if (ShowPolygon)
			{
				renderer.Points = pointList;
				renderer.Transform();
				renderer.DrawLines();
			}

		}

		protected override void AddPoint(CatPoint point, bool generateLater=true)//bool param is stub
		{
			point.OnDeleted += OnPointDeleted;
			point.OnChanged += OnPointChanged;
			points.Add(new PointWrapper(point));
		//	changed = true;
		}

		public override string GetName()
		{
			return "Bezier " + base.GetName();
		}

	}
}
