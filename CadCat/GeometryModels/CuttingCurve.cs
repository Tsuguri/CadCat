using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CadCat.DataStructures;
using CadCat.Math;
using CadCat.ModelInterfaces;
using CadCat.Rendering;

namespace CadCat.GeometryModels
{
	public class CuttingCurve : Model
	{
		private IIntersectable Q;
		private IIntersectable P;
		private List<Vector4> points;
		private List<CatPoint> catPoints;
		private readonly SceneData scene;
		private readonly List<int> indices = new List<int>();
		public CuttingCurve(List<Vector4> points, IIntersectable P, IIntersectable Q, SceneData scene, double minimumStep = 0.1)
		{
			this.Q = Q;
			this.P = P;
			this.points = points;
			this.scene = scene;

			this.points = new List<Vector4>(points.Count / 2);
			Vector3 prevPoint = P.GetPosition(points[0].X, points[0].Y);

			for (int i = 1; i < points.Count; i++)
			{
				var actualPoint = P.GetPosition(points[i].X, points[i].Y);
				if ((actualPoint - prevPoint).Length() > minimumStep)
				{
					prevPoint = actualPoint;
					this.points.Add(points[i]);
				}
			}

			catPoints = new List<CatPoint>(this.points.Count);
			foreach (var point in this.points)
			{
				var pos = P.GetPosition(point.X, point.Y);
				catPoints.Add(scene.CreateHiddenCatPoint(pos));
			}
			indices.Clear();
			indices.Add(0);
			for(int i = 1; i < this.points.Count-1; i++)
			{
				indices.Add(i);
				indices.Add(i);
			}
			indices.Add(this.points.Count-1);
		}

		public override void CleanUp()
		{
			base.CleanUp();
			foreach (var point in catPoints)
			{
				scene.RemovePoint(point);
			}
		}

		public override void Render(BaseRenderer renderer)
		{
			base.Render(renderer);
			renderer.Points = catPoints.Select(x => x.Position).ToList();
			renderer.Indices = indices;
			renderer.UseIndices = true;
			renderer.ModelMatrix = Matrix4.CreateIdentity();
			renderer.SelectedColor = IsSelected ? Colors.DarkViolet : Colors.White;

			renderer.Transform();
			renderer.DrawLines();
		}

		public override string GetName()
		{
			return "Cutting curve" + base.GetName();
		}
	}
}
