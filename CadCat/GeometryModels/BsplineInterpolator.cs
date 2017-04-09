using CadCat.ModelInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using CadCat.Rendering;
using System.Windows.Input;
using CadCat.Math;

namespace CadCat.GeometryModels
{
	class BsplineInterpolator : PointModel, IChangeablePointCount, ITypeChangeable
	{
		bool changed = false;


		private List<Vector3> renderPoints;
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


		private void CalculateWhatever()
		{
			var pts = points.Select(pt => pt.Point.Position).ToList();
			var distances = new double[pts.Count - 1];
			var knots = new double[pts.Count];

			double sum = 0;
			for (int i = 0; i < pts.Count - 1; i++)
			{
				distances[i] = (pts[i + 1] - pts[i]).Length();
				knots[i] = sum;
				sum += distances[i];
			}
			knots[knots.Length - 1] = sum;

			var alphas = new double[pts.Count - 3];
			var betas = new double[pts.Count - 3];
			var eras = new Vector3[pts.Count - 2];
			int j = 0;
			for (j = 1; j < pts.Count - 2; j++)
			{
				alphas[j - 1] = distances[j] / (distances[j] + distances[j + 1]);
				betas[j - 1] = distances[j] / (distances[j] + distances[j - 1]);

				eras[j - 1] = (((pts[j + 1] - pts[j]) / distances[j]) - ((pts[j] - pts[j - 1]) / distances[j - 1])) * 3 / (distances[j] + distances[j - 1]);
			}

			eras[j - 1] = (((pts[j + 1] - pts[j]) / distances[j]) - ((pts[j] - pts[j - 1]) / distances[j - 1])) * 3 / (distances[j] + distances[j - 1]); ;
			var diagonal = new double[pts.Count - 2];
			for (j = 0; j < diagonal.Length; j++)
				diagonal[j] = 2;

			var results = Utils.SolveMultidiagonalMatrixEquation(alphas, diagonal, betas, eras);


			var a = pts.ToArray();
			var b = new Vector3[pts.Count - 1];
			var c = new Vector3[pts.Count];
			var d = new Vector3[pts.Count - 1];

			results.ToArray().CopyTo(c, 1);

			for (int i = 0; i < d.Length; i++)
			{
				d[i] = (c[i + 1] - c[i]) / (3 * distances[i]);
			}

			for (int i = 0; i < b.Length; i++)
				b[i] = (a[i + 1] - a[i]) / distances[i] - (c[i] + d[i] * distances[i]) * distances[i];


			double step = sum / 100;
			double travelled = 0;
			int interval = 0;
			renderPoints = new List<Vector3>();
			renderPoints.Add(pts[0]);
			for (int i = 0; i < 100; i++)
			{
				travelled += step;
				while (interval < distances.Length && distances[interval] < travelled)
				{
					interval += 1;
					travelled -= distances[interval - 1];
				}

				if (interval >= distances.Length)
					break;
				var pos = a[interval] + (b[interval] + (c[interval] + d[interval] * travelled) * travelled) * travelled;
				renderPoints.Add(pos);
			}
			renderPoints.Add(pts[pts.Count - 1]);

			int x = 0;

		}

		public void AddPoint(CatPoint point)
		{
			points.Add(new PointWrapper(point));
			point.OnDeleted += OnPointDelete;
			point.OnChanged += OnPointChanged;
			changed = true;
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
			changed = true;
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

			CalculateWhatever();
			renderer.Points = renderPoints;
			renderer.UseIndices = false;
			renderer.Transform();
			renderer.DrawLines();

		}

		public void ChangeType()
		{
			CalculateWhatever();
		}
	}
}
