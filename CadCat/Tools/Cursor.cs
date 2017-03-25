using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using CadCat.Rendering.Packets;
using System.Windows.Input;
using CadCat.DataStructures.SpatialData;

namespace CadCat.Tools
{
	public class Cursor : GeometryModels.ParametrizedModel
	{
		SceneData scene;
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

		private GeometryModels.Model catchedModel = null;
		public GeometryModels.Model CatchedModel
		{
			get
			{
				return catchedModel;
			}
			set
			{
				catchedModel = value;
				transform = new Transform(scene.ActiveCamera.LookingAt);
				OnPropertyChanged();
				OnPropertyChanged(nameof(ScreenPosX));
				OnPropertyChanged(nameof(ScreenPosY));
				OnPropertyChanged(nameof(TrPosX));
				OnPropertyChanged(nameof(TrPosY));
				OnPropertyChanged(nameof(TrPosZ));
			}
		}

		private ICommand catchSelectedCommand;
		private ICommand catchNearestCommand;
		private ICommand releaseCatchedCommand;

		public ICommand CatchSelectedCommand
		{
			get
			{
				return catchSelectedCommand ?? (catchSelectedCommand = new Utilities.CommandHandler(CatchSelected));
			}
		}

		public ICommand CatchNearestComman
		{
			get
			{
				return catchNearestCommand ?? (catchNearestCommand = new Utilities.CommandHandler(CatchNearest));
			}
		}

		public ICommand ReleaseCatchedCommand
		{
			get
			{
				return releaseCatchedCommand ?? (releaseCatchedCommand = new Utilities.CommandHandler(Release));
			}
		}



		public Cursor(SceneData data)
		{
			scene = data;
		}

		public override IEnumerable<Line> GetLines()
		{
			if (!Visible)
				yield break;
			var line = new Line();
			line.from = new Math.Vector3(0, 0, -0.25);
			line.to = new Math.Vector3(0, 0, 0.25);
			yield return line;
			line.from = new Math.Vector3(0, -0.25, 0);
			line.to = new Math.Vector3(0, 0.25, 0);
			yield return line;
			line.from = new Math.Vector3(-0.25, 0, 0);
			line.to = new Math.Vector3(0.25, 0, 0);
			yield return line;
		}

		private void CatchSelected()
		{
			if (scene.SelectedModel != null)
				CatchedModel = scene.SelectedModel;
		}

		private void CatchNearest()
		{

		}

		private void Release()
		{
			CatchedModel = null;
		}
		public override RenderingPacket GetRenderingPacket()
		{
			var pack = base.GetRenderingPacket();
			pack.overrideScale = true;
			var cursorScale = (transform.Position - scene.ActiveCamera.CameraPosition).Length() / 10;

			pack.newScale = new Math.Vector3(cursorScale, cursorScale, cursorScale);
			return pack;
		}
	}
}
