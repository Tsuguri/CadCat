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
using MaterialDesignThemes.Wpf;
using CadCat.UIControls;
using CadCat.ModelInterfaces;

namespace CadCat.DataStructures
{
	public class Line
	{
		public Vector3 from;
		public Vector3 to;
	}
	public class SceneData : Utilities.BindableObject
	{
		public delegate IEnumerable<DataStructures.CatPoint> PointListGet();
		public delegate IEnumerable<Model> ModelListGet();

		private PointListGet getSelectedPoints;
		private ModelListGet getSelectedModels;

		public void SetSelectedPointsGetter(PointListGet del)
		{
			this.getSelectedPoints = del;
		}

		public void SetSelectedModelsGetter(ModelListGet del)
		{
			this.getSelectedModels = del;
		}

		private bool onMousePoint = false;
		public bool OnMousePoint
		{
			get
			{
				return onMousePoint;
			}
			set
			{
				if (!value || (value && SelectedModel != null))
				{
					onMousePoint = value;
					OnPropertyChanged();
				}
			}
		}
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

		private ObservableCollection<CatPoint> points = new ObservableCollection<CatPoint>();
		public ObservableCollection<CatPoint> Points { get { return points; } }

		private Model selectedModel = null;

		public Model SelectedModel
		{
			get
			{
				return selectedModel;
			}
			set
			{
				if (value == null)
					OnMousePoint = false;
				selectedModel = value;
				OnPropertyChanged();
			}
		}

		private CatPoint selectedPoint = null;

		public CatPoint SelectedPoint
		{
			get
			{
				return selectedPoint;
			}
			set
			{
				selectedPoint = value;
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
		private ICommand createBezierCommand;

		private ICommand createPointCommand;

		//private ICommand addPointsToCurrendIChangeablePointCount;
		private ICommand removeSelectedPointsCommand;
		private ICommand selectPointsCommand;
		private ICommand addSelectedPointToSelectedItemCommand;


		private ICommand goToSelectedCommand;
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

		public ICommand CreateBezierCommand
		{
			get
			{
				return createBezierCommand ?? (createBezierCommand = new Utilities.CommandHandler(CreateBezier));
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

		public ICommand RemoveCommand
		{
			get
			{
				return removeCommand ?? (removeCommand = new Utilities.CommandHandler(RemoveSelected));
			}
		}

		public ICommand RemoveSelectedPointsCommand
		{
			get
			{
				return removeSelectedPointsCommand ?? (removeSelectedPointsCommand = new Utilities.CommandHandler(RemoveSelectedPoints));
			}
		}

		public ICommand SelectPointsCommand
		{
			get
			{
				return selectPointsCommand ?? (selectPointsCommand = new Utilities.CommandHandler(SelectPoints));
			}
		}

		public ICommand AddSelectedPointToSelectedItemCommand
		{
			get
			{
				return addSelectedPointToSelectedItemCommand ?? (addSelectedPointToSelectedItemCommand = new Utilities.CommandHandler(AddSelectedPointToSelectedItem));
			} 
		}

		#endregion

		#region CreatingModels

		private void AddNewParametrizedModel(ParametrizedModel model)
		{
			model.transform.Position = ActiveCamera.LookingAt;
			AddNewModel(model);
		}

		private void AddNewModel(Model model)
		{
			models.Add(model);
			SelectedModel = model;
		}
		private void CreateTorus()
		{
			AddNewParametrizedModel(new Torus());
		}

		private void CreateCube()
		{
			AddNewModel(new Cube());
		}

		private void CreateBezier()
		{
			var selected = getSelectedPoints.Invoke().ToList();
			if (selected.Count < 1)
			{
				var sampleMessageDialog = new MessageHost
				{
					Message = { Text = "Not enough points for Bezier Curve (at least 1)." }
				};

				DialogHost.Show(sampleMessageDialog, "RootDialog");
			}
			else
			{
				AddNewModel(new Bezier(selected));

			}
		}

		private void CreatePoint()
		{
			Vector3 pos;
			if (Cursor.Visible)
				pos = Cursor.transform.Position;
			else
				pos = ActiveCamera.LookingAt;
			CreatePoint(pos);
		}
		private void CreatePoint(Vector3 pos)
		{


			var point = new CatPoint(pos);
			points.Add(point);

			var selected = getSelectedModels.Invoke().ToList();
			if (selected.Count > 0 && selected[0] is IChangeablePointCount)
			{
				var p = selected[0] as IChangeablePointCount;
				p.AddPoint(point);
			}
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

		public IEnumerable<DataStructures.CatPoint> GetPoints()
		{
			return points;
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
					//Cursor.CatchedModel?.InvalidateAll(); TODO
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

		private bool SelectClickedModel(Vector2 position, out CatPoint model)
		{
			Ray cameraRay = ActiveCamera.GetViewRay(position);
			List<ClickData> clicks = new List<ClickData>();
			var cameraMatrix = ActiveCamera.ViewProjectionMatrix;

			foreach (var point in GetPoints())
			{
				var pt = (cameraMatrix * new Vector4(point.Position, 1.0)).ToNormalizedVector3();
				var dt = new Vector2(pt.X, pt.Y) - position;
				var distance = dt.Length();
				if (distance < 0.009)
					clicks.Add(new ClickData(pt.Z, point));
			}

			ClickData selected;
			selected.ClickedModel = null;
			if (clicks.Count > 0)
			{
				clicks.Sort((x, y) => y.Distance.CompareTo(x.Distance));

				var clicked = clicks.First();
				if (clicked.ClickedModel != null)
				{
					selected = clicked;
				}
			}
			if (selected.ClickedModel != null)
			{
				model = selected.ClickedModel;
				return true;
			}
			model = null;
			return false;
		}

		internal void SceneClicked(Vector2 position)
		{

			var pos = ActiveCamera.GetScreenPointOnViewPlane(position);

			if (!OnMousePoint)
			{
				CatPoint clickedPoint;
				if (SelectClickedModel(position, out clickedPoint))
				{
					if (!Keyboard.IsKeyDown(Key.LeftCtrl))
					{
						var list = getSelectedPoints.Invoke().ToList();
						foreach (var selectedPoint in list)
						{
							selectedPoint.IsSelected = false;
						}

					}
					clickedPoint.IsSelected = !clickedPoint.IsSelected;
				}
			}
			else
			{
				var newPointPos = ActiveCamera.GetScreenPointOnViewPlane(position);
				CreatePoint(newPointPos);
			}



		}

		private void GoToSelected()
		{
			if (SelectedModel != null)
			{
				// TODO: ActiveCamera.LookingAt = SelectedModel.transform.Position;
			}
		}

		private void RemoveSelected()
		{
			if (SelectedModel != null)
			{
				models.Remove(SelectedModel);
				SelectedModel = null;
			}
		}

		private void RemoveSelectedPoints()
		{
			var pointList = Points.Where((x) => x.IsSelected).ToList();
			foreach (var point in pointList)
			{
				point.CleanUp();
				Points.Remove(point);
			}
		}

		private void SelectPoints()
		{
			var pointList = Points.Where((x) => x.IsSelected).ToList();
			pointList.ForEach((x) => { x.IsSelected = false; });

			foreach (var pt in SelectedModel.EnumerateCatPoints())
				pt.IsSelected = true;
		}

		private void AddSelectedPointToSelectedItem()
		{
			if( (SelectedModel is IChangeablePointCount))
			{
				var mod = SelectedModel as IChangeablePointCount;
				foreach (var point in getSelectedPoints.Invoke())
				{
					mod.AddPoint(point);
				}
			}
		}
	}
}
