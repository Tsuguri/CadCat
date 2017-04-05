using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using CadCat.Rendering.Packets;
using System.Windows.Input;
using CadCat.DataStructures.SpatialData;
using CadCat.Rendering;

namespace CadCat.Tools
{

	public static class SumExtensions
	{

		public static Math.Vector3 Sum(this IEnumerable<Math.Vector3> source)
		{
			return source.Aggregate((x, y) => x + y);
		}

		public static Math.Vector3 Sum<T>(this IEnumerable<T> source, Func<T, Math.Vector3> selector)
		{
			return source.Select(selector).Aggregate((x, y) => x + y);
		}
	}
	public class Cursor : GeometryModels.ParametrizedModel
	{
		SceneData scene;
		private List<Math.Vector3> points;
		private List<int> indices;
		private bool visible = false;
		public bool Visible
		{
			get
			{
				return visible;
			}
			set
			{
				visible = value;
				OnPropertyChanged();
			}
		}

		public Math.Vector2 ScreenPos { get; set; }

		public double ScreenPosX
		{
			get
			{
				return ScreenPos.X;
			}
			set
			{
				var p = ScreenPos;
				p.X = value;
				ScreenPos = p;
				OnPropertyChanged();
			}
		}

		public double ScreenPosY
		{
			get
			{
				return ScreenPos.Y;
			}
			set
			{
				var p = ScreenPos;
				p.Y = value;
				ScreenPos = p;
				OnPropertyChanged();
			}
		}

		List<Tuple<CatPoint, Math.Vector3>> catchedPoints;


		private ICommand catchCommand;
		private ICommand centerCommand;
		private ICommand releaseCommand;

		public ICommand CatchCommand
		{
			get
			{
				return catchCommand ?? (catchCommand = new Utilities.CommandHandler(Catch));
			}
		}

		public ICommand CenterCommand
		{
			get
			{
				return centerCommand ?? (centerCommand = new Utilities.CommandHandler(Center));
			}
		}

		public ICommand ReleaseCommand
		{
			get
			{
				return releaseCommand ?? (releaseCommand = new Utilities.CommandHandler(Release));
			}
		}



		public Cursor(SceneData data)
		{
			scene = data;
			points = new List<Math.Vector3>(6);
			indices = new List<int>(6)
			{
				0,1,2,3,4,5
			};
			points.Add(new Math.Vector3(0, 0, -0.25));
			points.Add(new Math.Vector3(0, 0, 0.25));
			points.Add(new Math.Vector3(0, -0.25, 0));
			points.Add(new Math.Vector3(0, 0.25, 0));
			points.Add(new Math.Vector3(-0.25, 0, 0));
			points.Add(new Math.Vector3(0.25, 0, 0));
		}

		private void Catch()
		{
			if (scene.SelectedPoint != null)
			{
				var pts = scene.Points.Where(x => x.IsSelected).ToList();
				var pos = pts.Select(x => x.Position).Sum();
				pos = pos / pts.Count();
				transform.Position = pos;
				catchedPoints = pts.Select(x => new Tuple<CatPoint, Math.Vector3>(x, x.Position - transform.Position)).ToList();

				InvalidatePosition();

			}
		}

		private void Center()
		{
			scene.ActiveCamera.LookingAt = this.transform.Position;
		}

		private void Release()
		{
			catchedPoints = null;
		}

		public override void Render(BaseRenderer renderer)
		{
			base.Render(renderer);
			var cursorScale = (transform.Position - scene.ActiveCamera.CameraPosition).Length() / 10;

			renderer.ModelMatrix = transform.CreateTransformMatrix(true, new Math.Vector3(cursorScale, cursorScale, cursorScale));
			renderer.Points = points;
			renderer.Indices = indices;
			renderer.Transform();
			renderer.DrawLines();
		}

		protected override void PositionChanged()
		{
			base.PositionChanged();

			if (catchedPoints != null)
				foreach (var point in catchedPoints)
				{
					var pos = point.Item2 + transform.Position;
					point.Item1.X = pos.X;
					point.Item1.Y = pos.Y;
					point.Item1.Z = pos.Z;
				}
		}
	}
}
