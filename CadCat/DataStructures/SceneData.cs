using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CadCat.Math;
using CadCat.Rendering;
using System.Windows.Input;
using System.Windows.Controls;
using CadCat.GeometryModels;
using System.Collections.ObjectModel;

namespace CadCat.DataStructures
{
	public class Line
	{
		public Vector3 from;
		public Vector3 to;
	}
	public class SceneData : Utilities.BindableObject
	{
		#region PropertiesAndFields

		#region Rendering

		public RendererType RenderingMode
		{
			get; set;
		}

		private BaseRenderer renderer = null;

		public BaseRenderer Renderer
		{
			get
			{
				return renderer;
			}
			set
			{
				renderer = value;
				OnPropertyChanged();
			}
		}

		public Vector2 ScreenSize
		{
			get; set;
		}

		#endregion


		#region Models

		private ObservableCollection<Model> models = new ObservableCollection<Model>();
		public ObservableCollection<Model> Models { get { return models; } }

		private Model selectedModel = null;

		public Model SelectedModel
		{
			get
			{
				return selectedModel;
			}
			set
			{
				selectedModel = value;
				OnPropertyChanged();
			}
		}

		private Tools.Cursor cursor;

		public Tools.Cursor Cursor
		{
			get
			{
				return cursor;
			}
		}

		#endregion

		#region MouseData

		public Point MousePosition
		{
			get
			{
				return mousePosition;
			}
			set
			{
				previousMousePosition = mousePosition;
				mousePosition = value;
				Delta = mousePosition - previousMousePosition;
				OnPropertyChanged();
			}
		}

		public Vector Delta
		{
			get; set;
		}

		private Point mousePosition;
		private Point previousMousePosition;

		#endregion

		#endregion

		#region Commands

		private ImageMouseController imageMouse;

		public ImageMouseController ImageMouse
		{
			get
			{
				return imageMouse;
			}
		}

		private ICommand createTorusCommand;
		private ICommand createCubeCommand;
		private ICommand createPointCommand;
		private ICommand goToSelectedCommand;
		private ICommand deselectCommand;
		private ICommand removeCommand;

		public ICommand CreateTorusCommand
		{
			get
			{
				return createTorusCommand ?? (createTorusCommand = new Utilities.CommandHandler(CreateTorus));
			}
		}

		public ICommand CreateCubeCommand
		{
			get
			{
				return createCubeCommand ?? (createCubeCommand = new Utilities.CommandHandler(CreateCube));
			}
		}

		public ICommand CreatePointCommand
		{
			get
			{
				return createPointCommand ?? (createPointCommand = new Utilities.CommandHandler(CreatePoint));
			}
		}

		public ICommand GoToSelectedCommand
		{
			get
			{
				return goToSelectedCommand ?? (goToSelectedCommand = new Utilities.CommandHandler(GoToSelected));
			}
		}

		public ICommand DeselectCommand
		{
			get
			{
				return deselectCommand ?? (deselectCommand = new Utilities.CommandHandler(DeselectSelected));
			}
		}

		public ICommand RemoveCommand
		{
			get
			{
				return removeCommand ?? (removeCommand = new Utilities.CommandHandler(RemoveSelected));
			}
		}

		#endregion

		#region CreatingModels

		private void AddNewModel(Model model)
		{
			model.transform.Position = ActiveCamera.LookingAt;
			models.Add(model);
			SelectedModel = model;
		}
		private void CreateTorus()
		{
			AddNewModel(new Torus());
		}

		private void CreateCube()
		{
			AddNewModel(new Cube());
		}

		private void CreatePoint()
		{
			AddNewModel(new CatPoint());
		}

		#endregion

		double rotateSpeed = 0.2;


		public Camera ActiveCamera
		{
			get; set;
		}

		public SceneData()
		{
			imageMouse = new ImageMouseController(this);
			cursor = new Tools.Cursor(this);
		}

		public IEnumerable<Line> GetLines(Rendering.ModelData modelData)
		{
			foreach (var model in models)
			{
				modelData.transform = model.transform;
				modelData.ModelID = model.ModelID;
				foreach (var line in model.GetLines())
					yield return line;
			}
			yield break;

		}

		public IEnumerable<Model> GetModels()
		{
			foreach (var model in models)
				yield return model;
		}

		public IEnumerable<Rendering.Packets.RenderingPacket> GetPackets()
		{
			foreach (var model in models)
			{
				yield return model.GetRenderingPacket();
			}

			yield return cursor.GetRenderingPacket();
			yield break;
		}


		internal void UpdateFrameData()
		{
			var delta = Delta;
			if (delta.X != -1 && delta.Length > 0.001 && Mouse.LeftButton == MouseButtonState.Pressed)
			{
				if (Keyboard.IsKeyDown(Key.A))
				{
					ActiveCamera.Radius = ActiveCamera.Radius + delta.Y * 0.05;
				}
				else if (Keyboard.IsKeyDown(Key.LeftCtrl))
				{
					ActiveCamera.Rotate(delta.Y * rotateSpeed, delta.X * rotateSpeed);

				}
				else if (Keyboard.IsKeyDown(Key.LeftAlt))
				{
					ActiveCamera.Move(0, 0, delta.Y);
				}
				else if (Keyboard.IsKeyDown(Key.C))
				{
					Cursor.transform.Position += (ActiveCamera.UpVector * delta.Y * 0.05 + ActiveCamera.RightVector * delta.X * 0.05);
					Cursor.InvalidateAll();
					Cursor.CatchedModel?.InvalidateAll();
				}
				else
				{
					ActiveCamera.Move(delta.X, delta.Y);
				}
			}
			var pos = (ActiveCamera.ViewProjectionMatrix * new Vector4(Cursor.transform.Position, 1.0)).ToNormalizedVector3();
			Cursor.ScreenPosX = pos.X;
			cursor.ScreenPosY = pos.Y;

		}

		internal void SceneClicked(Vector2 position)
		{
			Ray cameraRay = ActiveCamera.GetViewRay(position);
			List<ClickData> clicks = new List<ClickData>();
			var cameraMatrix = ActiveCamera.ViewProjectionMatrix;
			foreach (var packet in GetPackets())
			{
				if (packet.type == Rendering.Packets.PacketType.PointPacket)
				{
					var activeMatrix = cameraMatrix * packet.model.transform.CreateTransformMatrix();
					foreach (var point in packet.model.GetPoints())
					{
						var pt = (activeMatrix * new Vector4(point, 1.0)).ToNormalizedVector3();
						var dt = new Vector2(pt.X, pt.Y) - position;
						var distance = dt.Length();
						if (distance < 0.009)
							clicks.Add(new ClickData(pt.Z, packet.model));
					}
				}
			}

			if (clicks.Count > 0)
			{
				clicks.Sort((x, y) => y.Distance.CompareTo(x.Distance));

				var clicked = clicks.First();
				if (clicked.ClickedModel != null)
				{
					SelectedModel = clicked.ClickedModel;
				}
			}

		}

		private void GoToSelected()
		{
			if (SelectedModel != null)
			{
				ActiveCamera.LookingAt = SelectedModel.transform.Position;
			}
		}

		private void DeselectSelected()
		{
			SelectedModel = null;
		}

		private void RemoveSelected()
		{
			if (SelectedModel != null)
			{
				models.Remove(SelectedModel);
				SelectedModel = null;
			}
		}
	}
}
