using CadCat.ModelInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using CadCat.Rendering;
using System.Windows.Input;

namespace CadCat.GeometryModels
{
	class BsplineInterpolator : PointModel, IChangeablePointCount
	{
		bool changed = false;

		public BsplineInterpolator(List<CatPoint> points)
		{
			changed = false;
			foreach (var point in points)
			{
				AddPoint(point);
			}
		}

		private ICommand deletePointsCommand;

		public ICommand DeletePointsCommand
		{
			get
			{
				return deletePointsCommand ?? (deletePointsCommand = new Utilities.CommandHandler(DeleteSelectedPoints));
			}
		}
		private void DeleteSelectedPoints()
		{
			var list = points.Where((x) => x.IsSelected).ToList();
			foreach (var point in list)
				RemovePoint(point, true);
		}

		public void AddPoint(CatPoint point)
		{
			points.Add(new PointWrapper(point));
			point.OnDeleted += OnPointDelete;
			point.OnChanged += OnPointChanged;
		}

		private void OnPointDelete(CatPoint point)
		{
			RemovePoint(point, false);
		}

		private void OnPointChanged(CatPoint point)
		{
			changed = true;
		}

		private void RemovePoint(PointWrapper point, bool removeDelegate)
		{
			points.Remove(point);
			if (removeDelegate)
			{
				point.Point.OnDeleted -= OnPointDelete;
				point.Point.OnChanged -= OnPointChanged;
			}
		}

		public void RemovePoint(CatPoint point)
		{
			RemovePoint(point, true);
		}

		private void RemovePoint(CatPoint point, bool removeDelegate)
		{
			var pt = points.FirstOrDefault(x => x.Point == point);
			if (pt != null)
			{
				RemovePoint(pt, removeDelegate);
			}
		}

		public override string GetName()
		{
			return "BSplineInterpolator" + base.GetName();
		}

		public override void Render(BaseRenderer renderer)
		{
			base.Render(renderer);

		}
	}
}
