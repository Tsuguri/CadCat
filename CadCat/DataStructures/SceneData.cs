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
using System.Windows.Media;

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

		private ModelListGet getSelectedModels;

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


		private List<CatPoint> hiddenPoints = new List<CatPoint>();
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

		private bool isAnyPointSelected = false;
		public bool IsAnyPointSelected
		{
			get
			{
				return isAnyPointSelected;
			}
			set
			{
				isAnyPointSelected = value;
				OnPropertyChanged();
			}
		}

		public CatPoint SelectedPoint
		{
			get
			{
				return selectedPoint;
			}
			set
			{
				selectedPoint = value;
				IsAnyPointSelected = selectedPoint != null;
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
		private ICommand createBezierC2Command;
		private ICommand createBSplineInterpolatorCommand;

		private ICommand createPointCommand;

		//private ICommand addPointsToCurrendIChangeablePointCount;
		private ICommand removeSelectedPointsCommand;
		private ICommand selectPointsCommand;
		private ICommand addSelectedPointToSelectedItemCommand;
		private ICommand changeObjectTypeCommand;


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

		public ICommand CreateBezierC2Command
		{
			get
			{
				return createBezierC2Command ?? (createBezierC2Command = new Utilities.CommandHandler(CreateBezierC2));
			}
		}

		public ICommand CreateBSplineInterpolatorCommand
		{
			get
			{
				return createBSplineInterpolatorCommand ?? (createBSplineInterpolatorCommand = new Utilities.CommandHandler(CreateBSplineInterpolator));
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

		public ICommand ChangeObjectTypeCommand
		{
			get
			{
				return changeObjectTypeCommand ?? (changeObjectTypeCommand = new Utilities.CommandHandler(ChangeObjectType));
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
			var selected = GetFilteredSelected();
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
				AddNewModel(new Bezier(selected, this));

			}
		}

		private void CreateBezierC2()
		{
			var selected = GetFilteredSelected();
			if (selected.Count < 4)
			{
				var sampleMessageDialog = new MessageHost
				{
					Message = { Text = "Not enough points for C2 Bezier Curve (at least 4)." }
				};

				DialogHost.Show(sampleMessageDialog, "RootDialog");
			}
			else
			{
				AddNewModel(new BezierC2(selected, this));

			}
		}

		private void CreateBSplineInterpolator()
		{
			var selected = GetFilteredSelected();
			if (selected.Count < 2)
			{
				var sampleMessageDialog = new MessageHost
				{
					Message = { Text = "Not enough points for BSpline interpolation (at least 2)." }
				};

				DialogHost.Show(sampleMessageDialog, "RootDialog");
			}
			else
			{
				AddNewModel(new BsplineInterpolator(selected, this));
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

		internal CatPoint CreateCatPoint(Vector3 pos, bool addToSelected = true)
		{
			var point = new CatPoint(pos);
			points.Add(point);

			if (addToSelected)
			{
				var selected = getSelectedModels.Invoke().ToList();
				if (selected.Count > 0 && selected[0] is IChangeablePointCount)
				{
					var p = selected[0] as IChangeablePointCount;
					p.AddPoint(point);
				}
			}
			return point;
		}

		internal CatPoint CreateHiddenCatPoint(Vector3 pos)
		{
			var point = new CatPoint(pos);
			hiddenPoints.Add(point);

			return point;
		}

		internal void RemovePoint(CatPoint point)
		{
			point.CleanUp();
			Points.Remove(point);
		}

		private void CreatePoint(Vector3 pos)
		{
			CreateCatPoint(pos);
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

		public IEnumerable<CatPoint> GetPoints()
		{
			return points.Concat(hiddenPoints);
		}

		public void Render(BaseRenderer renderer)
		{
			renderer.SelectedColor = Colors.White;
			foreach (var model in models)
			{
				model.Render(renderer);
			}
			renderer.UseIndices = false;
			renderer.ModelMatrix = Matrix4.CreateIdentity();
			var points = new List<Vector3>(1);
			points.Insert(0, new Vector3());
			renderer.Points = points;
			foreach (var point in Points)
			{
				if (!point.Visible)
					continue;
				points[0] = point.Position;
				renderer.SelectedColor = point.IsSelected ? Colors.LimeGreen : Colors.White;
				renderer.Transform();
				renderer.DrawPoints();
			}

			if (hiddenPoints != null)
				foreach (var point in hiddenPoints)
				{
					if (!point.Visible)
						continue;
					points[0] = point.Position;
					renderer.SelectedColor = point.IsSelected ? Colors.LimeGreen : Colors.White;
					renderer.Transform();
					renderer.DrawPoints();

				}

			Cursor.Render(renderer);
		}

		internal void UpdateFrameData()
		{
			var delta = Delta;
			if (delta.X != -1 && delta.Length > 0.001 && Mouse.LeftButton == MouseButtonState.Pressed && imageMouse.ClickedOnImage)
			{
				switch (dragMode)
				{
					case MouseDragMode.CameraRadiusChange:
						ActiveCamera.Radius = ActiveCamera.Radius + delta.Y * 0.05;

						break;
					case MouseDragMode.CameraRotate:
						ActiveCamera.Rotate(delta.Y * rotateSpeed, delta.X * rotateSpeed);

						break;
					case MouseDragMode.CameraMove:
						ActiveCamera.Move(0, 0, delta.Y);

						break;
					case MouseDragMode.CursorMove:
						Cursor.transform.Position += (ActiveCamera.UpVector * delta.Y * 0.05 + ActiveCamera.RightVector * delta.X * 0.05);
						Cursor.InvalidateAll();
						break;
					case MouseDragMode.PointMove:

						var cameraRay = ActiveCamera.GetViewRay(new Math.Vector2(mousePosition.X / ScreenSize.X, mousePosition.Y / ScreenSize.Y) * 2 - 1);
						double distance;
						if (pointPlane != null && pointPlane.RayIntersection(cameraRay, out distance))
						{
							var newPointPosition = cameraRay.GetPoint(distance);
							draggedPoint.Position = newPointPosition;
						}

						break;
					case MouseDragMode.None:
						ActiveCamera.Move(delta.X, delta.Y);
						break;
					default:
						break;
				}
			}
			var pos = (ActiveCamera.ViewProjectionMatrix * new Vector4(Cursor.transform.Position, 1.0)).ToNormalizedVector3();
			Cursor.ScreenPosX = pos.X;
			cursor.ScreenPosY = pos.Y;

		}

		private enum MouseDragMode
		{
			CameraRadiusChange,
			CameraRotate,
			CameraMove,
			CursorMove,
			PointMove,
			None

		}
		private MouseDragMode dragMode = MouseDragMode.None;
		private CatPoint draggedPoint = null;
		private Plane pointPlane = null;


		public void OnLeftMousePressed(Vector2 mousePos)
		{
			if (Keyboard.IsKeyDown(Key.A))
				dragMode = MouseDragMode.CameraRadiusChange;
			else if (Keyboard.IsKeyDown(Key.LeftCtrl))
				dragMode = MouseDragMode.CameraRotate;
			else if (Keyboard.IsKeyDown(Key.LeftAlt))
				dragMode = MouseDragMode.CameraMove;
			else if (Keyboard.IsKeyDown(Key.C))
				dragMode = MouseDragMode.CursorMove;
			else // dragging point
			{
				CatPoint clicked;
				if (SelectClickedPoint(mousePos, out clicked))
				{
					dragMode = MouseDragMode.PointMove;
					draggedPoint = clicked;
					pointPlane = new Plane(draggedPoint.Position, (draggedPoint.Position - ActiveCamera.CameraPosition).Normalized());
				}
			}
		}

		public void OnLeftMouseReleased()
		{
			dragMode = MouseDragMode.None;
			draggedPoint = null;
		}

		private bool SelectClickedPoint(Vector2 position, out CatPoint model)
		{
			//Ray cameraRay = ActiveCamera.GetViewRay(position);
			List<ClickData> clicks = new List<ClickData>();
			var cameraMatrix = ActiveCamera.ViewProjectionMatrix;
			var pos = ScreenSize * position;

			foreach (var point in GetPoints())
			{
				if (!point.Visible)
					continue;
				var pt = (cameraMatrix * new Vector4(point.Position, 1.0)).ToNormalizedVector3();
				var dt = new Vector2(pt.X, pt.Y) * ScreenSize - pos;
				var distance = dt.Length();
				if (distance < 8)
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
				if (SelectClickedPoint(position, out clickedPoint) && !hiddenPoints.Contains(clickedPoint))
				{
					if (!Keyboard.IsKeyDown(Key.LeftCtrl))
					{
						var list = Points.Where(x => x.IsSelected).ToList();
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
				SelectedModel.CleanUp();
				models.Remove(SelectedModel);

				SelectedModel = null;
			}
		}

		private void RemoveSelectedPoints()
		{
			var pointList = GetFilteredSelected();
			foreach (var point in pointList)
			{
				RemovePoint(point);
			}
		}


		private void SelectPoints()
		{
			var pointList = Points.Where((x) => x.IsSelected).ToList();
			pointList.ForEach((x) => { x.IsSelected = false; });

			foreach (var pt in SelectedModel.EnumerateCatPoints())
				pt.IsSelected = true;
		}

		private List<CatPoint> GetFilteredSelected()
		{
			return Points.Where(x => (x.IsSelected && !x.AddAble)).ToList();
		}

		private void AddSelectedPointToSelectedItem()
		{
			if ((SelectedModel is IChangeablePointCount))
			{
				var mod = SelectedModel as IChangeablePointCount;
				foreach (var point in GetFilteredSelected())
				{
					mod.AddPoint(point);
				}
			}
		}

		private void ChangeObjectType()
		{
			var mod = SelectedModel as ITypeChangeable;
			mod?.ChangeType();
		}
	}
}
