using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using CadCat.Math;
using CadCat.ModelInterfaces;
using System.Windows.Input;

namespace CadCat.GeometryModels
{
	class Bezier : PointModel, IChangeablePointCount
	{
		public bool ShowPolygon { get; set; } = true;

		private ICommand deletePointsCommand;

		public ICommand DeletePointsCommand
		{
			get
			{
				return deletePointsCommand ?? (deletePointsCommand = new Utilities.CommandHandler(DeleteSelectedPoints));
			}
		}

		public Bezier(IEnumerable<DataStructures.CatPoint> pts)
		{
			foreach (var p in pts)
			{
				p.OnDeleted += OnPointDeleted;
				points.Add(new PointWrapper(p));

			}
		}

		public override IEnumerable<Line> GetLines()
		{
			var line = new Line();
			if (ShowPolygon)
				for (int i = 0; i < points.Count - 1; i++)
				{
					line.from = points[i].Point.Position;
					line.to = points[i + 1].Point.Position;
					yield return line;
				}
		}

		public void AddPoint(CatPoint point)
		{
			point.OnDeleted += OnPointDeleted;
			points.Add(new PointWrapper(point));
		}

		public void RemovePoint(CatPoint point)
		{
			RemovePoint(point, true);
		}

		private void DeleteSelectedPoints()
		{
			var list = points.Where((x) => x.IsSelected).ToList();
			foreach (var point in list)
				RemovePoint(point, true);
		}

		private void RemovePoint(PointWrapper wrapper, bool removeDelegate)
		{
			if (removeDelegate)
				wrapper.Point.OnDeleted -= OnPointDeleted;
			points.Remove(wrapper);
		}

		public void RemovePoint(CatPoint point, bool removeDelegate)
		{
			var pt = points.Where((x) => x.Point == point).FirstOrDefault();
			if (pt != null)
			{
				RemovePoint(pt, removeDelegate);
			}
		}

		private void OnPointDeleted(CatPoint point)
		{
			RemovePoint(point, false);
		}

		public override Matrix4 GetMatrix(bool overrideScale, Vector3 newScale)
		{
			return Matrix4.CreateIdentity();
		}

		public override string GetName()
		{
			return "Bezier " + base.GetName();
		}

	}
}
