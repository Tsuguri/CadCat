using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using CadCat.DataStructures;
using CadCat.Math;
using CadCat.ModelInterfaces;
using CadCat.Rendering;
using CadCat.Utilities;

namespace CadCat.GeometryModels
{
	public class CuttingCurve : Model
	{
		private IIntersectable Q;
		private IIntersectable P;
		private List<Vector4> points;
		private readonly List<CatPoint> catPoints;
		private readonly SceneData scene;
		private readonly List<int> indices = new List<int>();
		private bool cyclic;
		private ICommand convertToInterpolationCurve;

		public ICommand ConvertToInterpolation => convertToInterpolationCurve ?? (convertToInterpolationCurve = new CommandHandler(Convert));

		public CuttingCurve(List<Vector4> points, IIntersectable P, IIntersectable Q, SceneData scene, bool cyclic, double minimumStep = 0.1)
		{
			this.Q = Q;
			this.P = P;
			this.scene = scene;
			this.cyclic = cyclic;

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
			if (this.points.Count < 2)
				this.points.Add(points[points.Count - 1]);

			catPoints = new List<CatPoint>(this.points.Count);
			foreach (var point in this.points)
			{
				var pos = P.GetPosition(point.X, point.Y);
				catPoints.Add(scene.CreateHiddenCatPoint(pos));
			}
			indices.Clear();
			indices.Add(0);
			for (int i = 1; i < this.points.Count - 1; i++)
			{
				indices.Add(i);
				indices.Add(i);
			}
			indices.Add(this.points.Count - 1);
			Console.WriteLine($"Cyclic: {cyclic}");
			if (cyclic)
			{
				indices.Add(this.points.Count - 1);
				indices.Add(0);
			}
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

		private void Convert()
		{
			var cnt = points.Count / 5.0;
			var pts = new List<Vector4>();
			for (int i = 0; i < cnt; i++)
			{
				pts.Add(points[i * 5]);
			}
			if (cyclic)
				pts.Add(points[0]);

			var interp = new BsplineInterpolator(pts.Select(x => scene.CreateCatPoint(P.GetPosition(x.X, x.Y))), scene);
			scene.AddNewModel(interp);
			//scene.RemoveModel(this);
		}

		public void Draw(WriteableBitmap bitmap)
		{
			bitmap.Clear(Colors.White);
			double width = bitmap.Width;
			double halfWidth = width / 2;
			double height = bitmap.Height;
			double uParam = P.FirstParamLimit;
			double vParam = P.SecondParamLimit;
			double sParam = Q.FirstParamLimit;
			double tParam = Q.SecondParamLimit;
			using (bitmap.GetBitmapContext())
			{
				for (int i = 0; i < points.Count - 1 + (cyclic ? 1 : 0); i++)
				{
					var from = points[i];
					var to = points[(i + 1) % points.Count];
					if (System.Math.Abs(from.X - to.X) < 0.1 && System.Math.Abs(from.Y - to.Y) < 0.1)
						bitmap.DrawLineDDA((int)(from.X / uParam * halfWidth), (int)(from.Y / vParam * height), (int)(to.X / uParam * halfWidth), (int)(to.Y / vParam * height), Colors.DarkOrchid);
					if (System.Math.Abs(from.Z - to.Z) < 0.1 && System.Math.Abs(from.W - to.W) < 0.1)
						bitmap.DrawLineDDA((int)(from.Z / sParam * halfWidth + halfWidth), (int)(from.W / tParam * height), (int)(to.Z / sParam * halfWidth + halfWidth), (int)(to.W / tParam * height), Colors.DarkGreen);
				}
				bitmap.DrawLineDDA((int)halfWidth, 0, (int)halfWidth, (int)height, Colors.Red);
			}

		}
	}
}
