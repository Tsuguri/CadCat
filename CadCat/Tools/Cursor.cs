using System;
using System.Collections.Generic;
using System.Linq;
using CadCat.DataStructures;
using System.Windows.Input;
using CadCat.Rendering;
// ReSharper disable RedundantArgumentDefaultValue

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
		readonly SceneData scene;
		private readonly List<Math.Vector3> points;
		private readonly List<int> indices;
		private bool visible;
		private double radius = 1.0;

		public double Radius
		{
			get { return radius; }
			set
			{
				if (value >= 0.0)
				{
					radius = value;
					PositionChanged();
					OnPropertyChanged();
				}
			}
		}

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

		public ICommand CatchCommand => catchCommand ?? (catchCommand = new Utilities.CommandHandler(Catch));

		public ICommand CenterCommand => centerCommand ?? (centerCommand = new Utilities.CommandHandler(Center));

		public ICommand ReleaseCommand => releaseCommand ?? (releaseCommand = new Utilities.CommandHandler(Release));


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
			var pts = scene.GetPoints().Where(x => x.IsSelected).ToList();
			if (pts.Count < 1)
				return;
			var pos = pts.Select(x => x.Position).Sum();
			pos = pos / pts.Count;
			Transform.Position = pos;
			catchedPoints = pts.Select(x => new Tuple<CatPoint, Math.Vector3>(x, x.Position - Transform.Position)).ToList();

			InvalidatePosition();
		}

		private void Center()
		{
			scene.ActiveCamera.LookingAt = Transform.Position;
		}

		private void Release()
		{
			catchedPoints = null;
		}

		public override void Render(BaseRenderer renderer)
		{
			if (!Visible)
				return;
			base.Render(renderer);
			var cursorScale = (Transform.Position - scene.ActiveCamera.CameraPosition).Length() / 10;

			renderer.ModelMatrix = Transform.CreateTransformMatrix(true, new Math.Vector3(cursorScale, cursorScale, cursorScale));
			renderer.Points = points;
			renderer.Indices = indices;
			renderer.UseIndices = true;
			renderer.Transform();
			renderer.DrawLines();
		}

		protected override void PositionChanged()
		{
			base.PositionChanged();

			if (catchedPoints != null)
				foreach (var point in catchedPoints)
				{
					var pos = point.Item2*Radius + Transform.Position;
					point.Item1.X = pos.X;
					point.Item1.Y = pos.Y;
					point.Item1.Z = pos.Z;
				}
		}
	}
}
