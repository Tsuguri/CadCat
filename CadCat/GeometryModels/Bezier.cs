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


		private List<int> curveSizes;


		public bool ShowPolygon { get; set; } = true;

		//private bool changed = false;

		

		public Bezier(IEnumerable<DataStructures.CatPoint> pts, SceneData data) : base(pts,data)
		{
		}

		public override void Render(BaseRenderer renderer)
		{
			base.Render(renderer);
			renderer.ModelMatrix = Matrix4.CreateIdentity();
			renderer.UseIndices = false;
			var pointList = points.Select(x => x.Point.Position).ToList();
			if (ShowPolygon)
			{
				renderer.Points = pointList;
				renderer.Transform();
				renderer.DrawLines();
			}

			curveSizes = null;
			CountBezierPoints(points.Select(x=>x.Point).ToList());

			renderer.Points = curvePoints;
			renderer.Transform();
			renderer.DrawLines();

		}

		protected override void AddPoint(CatPoint point, bool generateLater=true)//bool param is stub
		{
			point.OnDeleted += OnPointDeleted;
			point.OnChanged += OnPointChanged;
			points.Add(new PointWrapper(point));
		//	changed = true;
		}



		protected override void DeleteSelectedPoints()
		{
			var list = points.Where((x) => x.IsSelected).ToList();
			foreach (var point in list)
				RemovePoint(point, true);
		}


		protected override void OnPointChanged(CatPoint point)
		{
		}

		public override string GetName()
		{
			return "Bezier " + base.GetName();
		}

	}
}
