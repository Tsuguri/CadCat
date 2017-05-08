using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CadCat.Math;
using CadCat.Rendering;
using System.Windows.Input;
using CadCat.GeometryModels;
using System.Collections.ObjectModel;
using MaterialDesignThemes.Wpf;
using CadCat.UIControls;
using CadCat.ModelInterfaces;
using System.Windows.Media;
using CadCat.GeometryModels.Proxys;
using CadCat.Utilities;
using GM1.Serialization;
using Microsoft.Win32;
using Camera = CadCat.Rendering.Camera;
using Point = System.Windows.Point;
using Vector3 = CadCat.Math.Vector3;
using Vector4 = CadCat.Math.Vector4;

namespace CadCat.DataStructures
{

	public class SceneData : BindableObject
	{
		#region Types

		public delegate IEnumerable<CatPoint> PointListGet();
		public delegate IEnumerable<Model> ModelListGet();

		#endregion

		private bool onMousePoint;

		public bool OnMousePoint
		{
			get
			{
				return onMousePoint;
			}
			set
			{
				if (value && SelectedModel == null) return;
				onMousePoint = value;
				OnPropertyChanged();
			}
		}

		#region PropertiesAndFields

		#region Rendering

		public RendererType RenderingMode
		{
			get; set;
		}

		private BaseRenderer currentRenderer;

		public BaseRenderer CurrentRenderer
		{
			get
			{
				return currentRenderer;
			}
			set
			{
				currentRenderer = value;
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
		public ObservableCollection<Model> Models => models;

		private readonly ObservableCollection<CatPoint> points = new ObservableCollection<CatPoint>();
		public ObservableCollection<CatPoint> Points => points;


		private readonly List<CatPoint> hiddenPoints = new List<CatPoint>();
		private Model selectedModel;

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

		private CatPoint selectedPoint;

		private bool isAnyPointSelected;

		/// <summary>
		/// For Point List View
		/// </summary>
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

		private readonly Tools.Cursor cursor;

		public Tools.Cursor Cursor => cursor;

		#endregion

		//to change
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

		private readonly ImageMouseController imageMouse;

		public ImageMouseController ImageMouse => imageMouse;

		private ICommand createTorusCommand;
		private ICommand createCubeCommand;
		private ICommand createBezierCommand;
		private ICommand createBezierC2Command;
		private ICommand createBSplineInterpolatorCommand;
		private ICommand createBezierPatchCommand;
		private ICommand createGregoryPatchCommand;

		private ICommand createPointCommand;

		private ICommand removeSelectedPointsCommand;
		private ICommand selectPointsCommand;
		private ICommand addSelectedPointToSelectedItemCommand;
		private ICommand changeObjectTypeCommand;
		private ICommand convertToPointsCommand;
		private ICommand mergePointsCommand;


		private ICommand goToSelectedCommand;
		private ICommand removeCommand;
		private ICommand sortCommand;


		private ICommand saveFileCommand;
		private ICommand loadFileCommand;

		public ICommand LoadFileCommand => loadFileCommand ?? (loadFileCommand = new CommandHandler(LoadFile));
		public ICommand SaveFileCommand => saveFileCommand ?? (saveFileCommand = new CommandHandler(SaveFile));
		public ICommand SortCommand => sortCommand ?? (sortCommand = new CommandHandler(SortModels));

		public ICommand CreateTorusCommand => createTorusCommand ?? (createTorusCommand = new CommandHandler(CreateTorus));

		public ICommand CreateCubeCommand => createCubeCommand ?? (createCubeCommand = new CommandHandler(CreateCube));

		public ICommand CreateBezierCommand => createBezierCommand ?? (createBezierCommand = new CommandHandler(CreateBezier));

		public ICommand CreateBezierC2Command => createBezierC2Command ?? (createBezierC2Command = new CommandHandler(CreateBezierC2));

		public ICommand CreateBSplineInterpolatorCommand => createBSplineInterpolatorCommand ?? (createBSplineInterpolatorCommand = new CommandHandler(CreateBSplineInterpolator));

		public ICommand CreateBezierPatchCommand => createBezierPatchCommand ?? (createBezierPatchCommand = new CommandHandler(CreateBezierPatch));

		public ICommand CreateGregoryPatchCommand => createGregoryPatchCommand ?? (createGregoryPatchCommand = new CommandHandler(CreateGregoryPatch));

		public ICommand CreatePointCommand => createPointCommand ?? (createPointCommand = new CommandHandler(CreatePoint));

		public ICommand GoToSelectedCommand => goToSelectedCommand ?? (goToSelectedCommand = new CommandHandler(GoToSelected));

		public ICommand RemoveCommand => removeCommand ?? (removeCommand = new CommandHandler(RemoveSelected));

		public ICommand MergePointsCommand => mergePointsCommand ?? (mergePointsCommand = new CommandHandler(MergePoints));

		public ICommand RemoveSelectedPointsCommand => removeSelectedPointsCommand ?? (removeSelectedPointsCommand = new CommandHandler(RemoveSelectedPoints));

		public ICommand SelectPointsCommand => selectPointsCommand ?? (selectPointsCommand = new CommandHandler(SelectPoints));

		public ICommand AddSelectedPointToSelectedItemCommand => addSelectedPointToSelectedItemCommand ?? (addSelectedPointToSelectedItemCommand = new CommandHandler(AddSelectedPointToSelectedItem));

		public ICommand ChangeObjectTypeCommand => changeObjectTypeCommand ?? (changeObjectTypeCommand = new CommandHandler(ChangeObjectType));

		public ICommand ConvertToPointsCommand => convertToPointsCommand ??
												  (convertToPointsCommand = new CommandHandler(ConvertToPoints));
		#endregion

		#region CreatingModels

		private void AddNewParametrizedModel(ParametrizedModel model)
		{
			model.Transform.Position = ActiveCamera.LookingAt;
			AddNewModel(model);
		}

		public void AddNewModel(Model model)
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
			if (GetFilteredSelected().Count() < 2)
			{
				var sampleMessageDialog = new MessageHost
				{
					Message = { Text = "Not enough points for Bezier Curve (at least 1)." }
				};


				DialogHost.Show(sampleMessageDialog, "RootDialog");
			}
			else
			{
				AddNewModel(new Bezier(GetFilteredSelected(), this));

			}
		}

		private void CreateBezierC2()
		{
			if (GetFilteredSelected().Count() < 4)
			{
				var sampleMessageDialog = new MessageHost
				{
					Message = { Text = "Not enough points for C2 Bezier Curve (at least 4)." }
				};

				DialogHost.Show(sampleMessageDialog, "RootDialog");
			}
			else
			{
				AddNewModel(new BezierC2(GetFilteredSelected(), this));
			}
		}

		private void CreateBSplineInterpolator()
		{
			if (GetFilteredSelected().Count() < 2)
			{
				var sampleMessageDialog = new MessageHost
				{
					Message = { Text = "Not enough points for BSpline interpolation (at least 2)." }
				};

				DialogHost.Show(sampleMessageDialog, "RootDialog");
			}
			else
			{
				AddNewModel(new BsplineInterpolator(GetFilteredSelected(), this));
			}
		}

		private void CreateBezierPatch()
		{
			AddNewModel(new TempSurface());
		}

		private void CreateGregoryPatch()
		{

			var selectedPoints = GetPoints().Where(x => x.IsSelected).ToList();

			var patches = Models.Where(x => x is BezierPatch).Cast<BezierPatch>().Where(x => x.ContainsTwoInCorners(selectedPoints)).ToList();


			var pts = selectedPoints.Where(x =>
				{
					int counter = 0;
					patches.ForEach(y =>
					{
						if (y.ContainsInCorner(x))
							counter++;
					});
					return counter >= 2;
				})
				.ToList();
			var cycle = CreatePatchCycle(pts, patches);
			if (cycle == null)
			{
				var sampleMessageDialog = new MessageHost
				{
					Message = { Text = "Selected points and their patches does not create proper cycle." }
				};


				DialogHost.Show(sampleMessageDialog, "RootDialog");
			}
			else
				AddNewModel(new GregoryPatch(cycle,this));
		}

		private PatchCycle CreatePatchCycle(List<CatPoint> points, List<BezierPatch> patches)
		{
			var cycle = new List<BezierPatch>();
			var cyclePoints = new List<CatPoint>();
			var actualPatch = patches[0];
			var firstPoint = points.First(x => actualPatch.ContainsInCorner(x));
			var secondPoint = points.First(x => actualPatch.ContainsNearby(firstPoint, x));
			patches.Remove(actualPatch);
			cycle.Add(actualPatch);
			cyclePoints.Add(firstPoint);
			cyclePoints.Add(secondPoint);
			points.Remove(firstPoint);
			points.Remove(secondPoint);

			while (patches.Count > 0)
			{
				var nextPatch = patches.FirstOrDefault(x => x.ContainsInCorner(secondPoint));
				if (nextPatch == null)
					return null;
				patches.Remove(nextPatch);
				cycle.Add(nextPatch);

				if (points.Count == 0 && nextPatch.ContainsNearby(secondPoint, firstPoint))
					break;

				var pt = points.FirstOrDefault(x => nextPatch.ContainsNearby(secondPoint, x));
				if (pt == null)
					return null;
				cyclePoints.Add(pt);
				points.Remove(pt);
				secondPoint = pt;

			}


			return new PatchCycle(cycle, cyclePoints);
		}


		private void CreatePoint()
		{
			var pos = Cursor.Visible ? Cursor.Transform.Position : ActiveCamera.LookingAt;
			CreatePoint(pos);
		}

		internal CatPoint CreateCatPoint(Vector3 pos, bool addToSelected = true)
		{
			var point = new CatPoint(pos);
			points.Add(point);

			if (addToSelected)
			{
				var selected = Models.Where(x => x.IsSelected).ToList();
				if (selected.Count > 0 && selected[0] is IChangeablePointCount)
				{
					var p = (IChangeablePointCount)selected[0];
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
			if (!point.Removeable)
				return;
			point.CleanUp();
			Points.Remove(point);
			hiddenPoints.Remove(point);
		}

		private void CreatePoint(Vector3 pos)
		{
			CreateCatPoint(pos);
		}

		private void MergePoints()
		{
			var pt = GetFilteredSelected().FirstOrDefault();
			if (pt != null)
			{
				Vector3 pos = new Vector3();
				var pts = GetFilteredSelected().ToList();
				var newPt = CreateCatPoint(new Vector3());
				int count = 0;
				foreach (var point in pts)
				{
					pos += point.Position;
					point.Replace(newPt);
					count += 1;
					RemovePoint(point);
				}
				newPt.Position = pos / count;
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

		public IEnumerable<CatPoint> GetPoints()
		{
			return points;
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
			var renderingPoints = new List<Vector3>(1);
			renderingPoints.Insert(0, new Vector3());
			renderer.Points = renderingPoints;
			foreach (var point in Points)
			{
				if (!point.Visible)
					continue;
				renderingPoints[0] = point.Position;
				renderer.SelectedColor = point.IsSelected ? Colors.LimeGreen : Colors.White;
				renderer.Transform();
				renderer.DrawPoints();
			}

			if (hiddenPoints != null)
				foreach (var point in hiddenPoints)
				{
					if (!point.Visible)
						continue;
					renderingPoints[0] = point.Position;
					renderer.SelectedColor = Colors.BlueViolet;// point.IsSelected ? Colors.LimeGreen : Colors.White;
					renderer.Transform();
					renderer.DrawPoints();

				}

			if (dragMode == MouseDragMode.SelectPoints)
			{
				var minX = System.Math.Min(dragActualPosition.X, dragStartPosition.X) / ScreenSize.X;
				var maxX = System.Math.Max(dragActualPosition.X, dragStartPosition.X) / ScreenSize.X;
				var minY = System.Math.Min(dragActualPosition.Y, dragStartPosition.Y) / ScreenSize.Y;
				var maxY = System.Math.Max(dragActualPosition.Y, dragStartPosition.Y) / ScreenSize.Y;
				renderer.DrawRectangle(minX, maxX, 1.0 - minY, 1.0 - maxY, Colors.DarkOrchid);
			}
			Cursor.Render(renderer);
		}

		internal void UpdateFrameData()
		{
			var delta = Delta;
			// ReSharper disable once CompareOfFloatsByEqualityOperator
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
						Cursor.Transform.Position += (ActiveCamera.UpVector * delta.Y * 0.05 + ActiveCamera.RightVector * delta.X * 0.05);
						Cursor.InvalidateAll();
						break;
					case MouseDragMode.PointMove:

						var cameraRay = ActiveCamera.GetViewRay(new Vector2(mousePosition.X / ScreenSize.X, mousePosition.Y / ScreenSize.Y) * 2 - 1);
						// ReSharper disable once TooWideLocalVariableScope
						double distance;
						if (pointPlane != null && pointPlane.RayIntersection(cameraRay, out distance))
						{
							var newPointPosition = cameraRay.GetPoint(distance);
							draggedPoint.Position = newPointPosition;
						}

						break;
					case MouseDragMode.SelectPoints:
						dragActualPosition = MousePosition;
						break;
					case MouseDragMode.None:
						ActiveCamera.Move(delta.X, delta.Y);
						break;
				}
			}
			var pos = (ActiveCamera.ViewProjectionMatrix * new Vector4(Cursor.Transform.Position)).ToNormalizedVector3();
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
			SelectPoints,
			None

		}
		private MouseDragMode dragMode = MouseDragMode.None;
		private CatPoint draggedPoint;
		private Plane pointPlane;
		private Point dragStartPosition;
		private Point dragActualPosition;


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
			else if (Keyboard.IsKeyDown(Key.S))
			{
				dragMode = MouseDragMode.SelectPoints;
				dragStartPosition = dragActualPosition = MousePosition;
			}
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
			if (dragMode == MouseDragMode.SelectPoints)
			{
				var dragEnd = MousePosition;
				var minX = System.Math.Min(dragEnd.X, dragStartPosition.X) / ScreenSize.X;
				var maxX = System.Math.Max(dragEnd.X, dragStartPosition.X) / ScreenSize.X;
				var minY = System.Math.Min(dragEnd.Y, dragStartPosition.Y) / ScreenSize.Y;
				var maxY = System.Math.Max(dragEnd.Y, dragStartPosition.Y) / ScreenSize.Y;

				var ptsList = new List<CatPoint>();
				var cameraMatrix = ActiveCamera.ViewProjectionMatrix;

				foreach (var catPoint in GetPoints().Where(x => x.Visible))
				{

					var pt = (cameraMatrix * new Vector4(catPoint.Position)).ToNormalizedVector3() / 2 + 0.5f;
					if (pt.X < maxX && pt.X > minX && pt.Y < maxY && pt.Y > minY)
						ptsList.Add(catPoint);
				}
				if (!Keyboard.IsKeyDown(Key.LeftCtrl))
					foreach (var catPoint in Points)
					{
						catPoint.IsSelected = false;
					}
				foreach (var catPoint in ptsList)
				{
					catPoint.IsSelected = true;
				}

			}
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
				var pt = (cameraMatrix * new Vector4(point.Position)).ToNormalizedVector3();
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


			if (!OnMousePoint)
			{
				CatPoint clickedPoint;
				if (SelectClickedPoint(position, out clickedPoint)) // && !hiddenPoints.Contains(clickedPoint)
				{
					if (!Keyboard.IsKeyDown(Key.LeftCtrl))
					{
						var list = GetPoints().Where(x => x.IsSelected).ToList();
						foreach (var point in list)
						{
							point.IsSelected = false;
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
			var model = SelectedModel as ParametrizedModel;
			if (model != null)
				ActiveCamera.LookingAt = model.Transform.Position;
		}

		private void RemoveSelected()
		{
			var l = Models.Where(x => x.IsSelected).ToList();
			foreach (var model in l)
			{
				RemoveModel(model);
			}
		}

		public void RemoveModel(Model model)
		{
			if (models.Remove(model))
			{
				model.CleanUp();
			}
		}

		private void RemoveSelectedPoints()
		{
			var toRemove = GetFilteredSelected().Where(x => x.Removeable).ToList();
			foreach (var point in toRemove)
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

		private IEnumerable<CatPoint> GetFilteredSelected()
		{
			return Points.Where(x => (x.IsSelected));
		}

		private void AddSelectedPointToSelectedItem()
		{
			var count = SelectedModel as IChangeablePointCount;
			if (count != null)
			{
				var mod = count;
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

		private void ConvertToPoints()
		{
			var mod = SelectedModel as IConvertibleToPoints;
			mod?.Convert(this);


		}

		private void SortModels()
		{
			models = new ObservableCollection<Model>(Models.OrderBy(x => x.Name));
			// ReSharper disable once ExplicitCallerInfoArgument
			OnPropertyChanged(nameof(Models));
		}

		private void SaveFile()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			if (saveFileDialog.ShowDialog() == false)
				return;

			SaveToFile(saveFileDialog.FileName);
		}

		private void SaveToFile(string filename)
		{
			var serializer = new GM1.Serialization.XMLSerializer();
			var scene = new Scene();

			var ptCount = points.Count + hiddenPoints.Count;
			scene.Points = new GM1.Serialization.Point[ptCount];
			int i = 0;
			foreach (var catPoint in points)
			{
				scene.Points[i] = new GM1.Serialization.Point() { Position = catPoint.Position.ToShitpoint(), Name = catPoint.Name };
				catPoint.SerializationId = i;
				i++;
			}
			List<BezierCurveC0> beziersc0 = new List<BezierCurveC0>();
			List<BezierCurveC2> beziersc2 = new List<BezierCurveC2>();
			List<InterpolationBezierCurveC2> interpolators = new List<InterpolationBezierCurveC2>();
			List<BezierSurfaceC0> surfacesc0 = new List<BezierSurfaceC0>();
			List<BezierSurfaceC2> surfacesc2 = new List<BezierSurfaceC2>();

			foreach (var model in models)
			{
				var type = model.GetType();
				if (type == typeof(Bezier))
				{
					var bezier = model as Bezier;
					var sceneBezier = new BezierCurveC0
					{
						Name = bezier.Name,
						DisplayPolygon = bezier.ShowPolygon,
						DisplayPolygonSpecified = true,
						Points = bezier.Points.Select(x => x.Point.SerializationId).ToArray()
					};
					beziersc0.Add(sceneBezier);

				}
				else if (type == typeof(BezierC2))
				{
					var bezier = model as BezierC2;
					var sceneBezier = new BezierCurveC2
					{
						Name = bezier.Name,
						DisplayPolygon = bezier.ShowPolygon,
						DisplayPolygonSpecified = true,
						Points = bezier.Points.Select(x => x.Point.SerializationId).ToArray()
					};
					beziersc2.Add(sceneBezier);
				}
				else if (type == typeof(BsplineInterpolator))
				{
					var interpolationBezier = model as BsplineInterpolator;
					var sceneBezier = new InterpolationBezierCurveC2()
					{
						Name = interpolationBezier.Name,
						DisplayPolygon = interpolationBezier.ShowPolygon,
						DisplayPolygonSpecified = true,
						Points = interpolationBezier.Points.Select(x => x.Point.SerializationId).ToArray()
					};
					interpolators.Add(sceneBezier);
				}
				else if (type == typeof(Surface))
				{
					var surface = model as Surface;

					switch (surface.SurfaceType)
					{
						case SurfaceType.Bezier:
							var bezierSurf = new BezierSurfaceC0();
							bezierSurf.Name = surface.Name;
							bezierSurf.PatchesU = surface.PatchesU;
							bezierSurf.PatchesV = surface.PatchesV;
							var ptchs = new List<BezierSurfaceC0Patch>();
							foreach (var patch in surface.GetPatches().Cast<BezierPatch>())
							{
								var scenePatch = new BezierSurfaceC0Patch()
								{
									Name = patch.Name,
									SurfaceDivisionsU = patch.WidthDiv,
									SurfaceDivisionsV = patch.HeightDiv,
									PatchU = patch.VPos,
									PatchV = patch.UPos,
									Points = new PointsU4V4()
								};
								int ind = 0;
								foreach (var pt in patch.EnumerateCatPoints().Select(x => x.SerializationId))
								{
									scenePatch.Points[ind] = pt;
									ind++;
								}
								ptchs.Add(scenePatch);

							}
							bezierSurf.Patches = ptchs.ToArray();
							surfacesc0.Add(bezierSurf);

							break;
						case SurfaceType.BSpline:
							var sceneSurf = new BezierSurfaceC2();
							sceneSurf.Name = surface.Name;
							sceneSurf.PatchesU = surface.PatchesU;
							sceneSurf.PatchesV = surface.PatchesV;
							var ptch = new List<BezierSurfaceC2Patch>();
							foreach (var patch in surface.GetPatches().Cast<BSplinePatch>())
							{
								var scenePatch = new BezierSurfaceC2Patch()
								{
									Name = patch.Name,
									SurfaceDivisionsU = patch.WidthDiv,
									SurfaceDivisionsV = patch.HeightDiv,
									PatchU = patch.UPos,
									PatchV = patch.VPos,
									Points = new PointsU4V4()
								};
								int ind = 0;
								foreach (var pt in patch.EnumerateCatPoints().Select(x => x.SerializationId))
								{
									scenePatch.Points[ind] = pt;
									ind++;
								}
								ptch.Add(scenePatch);


							}
							sceneSurf.Patches = ptch.ToArray();
							surfacesc2.Add(sceneSurf);

							break;
						case SurfaceType.Nurbs:
							break;
						default:
							break;
					}
				}
			}

			scene.BezierSurfacesC0 = surfacesc0.ToArray();
			scene.BezierSurfacesC2 = surfacesc2.ToArray();
			scene.BezierCurvesC0 = beziersc0.ToArray();
			scene.BezierCurvesC2 = beziersc2.ToArray();
			scene.InterpolationBezierCurvesC2 = interpolators.ToArray();

			serializer.SerializeToFile(filename, scene);
		}

		private void LoadFile()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == false)
				return;

			LoadFromFile(openFileDialog.FileName);
		}

		private void LoadFromFile(string filename)
		{

			var p = new XMLSerializer();
			Scene scene;
			try
			{
				scene = p.DeserializeFromFile(filename);
			}
			catch (Exception e)
			{
				return;
			}


			ClearScene();

			var catPoints = new List<CatPoint>(scene.Points.Length);
			for (int i = 0; i < scene.Points.Length; i++)
			{
				var pos = scene.Points[i].Position;
				var pt = CreateCatPoint(new Vector3(pos.X, pos.Y, pos.Z), false);
				pt.Name = scene.Points[i].Name;
				catPoints.Add(pt);
			}


			foreach (var bezierCurveC0 in scene.BezierCurvesC0)
			{
				var curve = new Bezier(bezierCurveC0.Points.Select(x => catPoints[x]), this) { Name = bezierCurveC0.Name };
				if (bezierCurveC0.DisplayPolygonSpecified)
					curve.ShowPolygon = bezierCurveC0.DisplayPolygon;
				AddNewModel(curve);
			}

			foreach (var bezierCurveC2 in scene.BezierCurvesC2)
			{
				var curve = new BezierC2(bezierCurveC2.Points.Select(x => catPoints[x]), this) { Name = bezierCurveC2.Name };
				if (bezierCurveC2.DisplayPolygonSpecified)
					curve.ShowPolygon = bezierCurveC2.DisplayPolygon;
				AddNewModel(curve);
			}

			foreach (var interpolationBezierCurveC2 in scene.InterpolationBezierCurvesC2)
			{
				var curve = new BsplineInterpolator(interpolationBezierCurveC2.Points.Select(x => catPoints[x]), this) { Name = interpolationBezierCurveC2.Name };
				if (interpolationBezierCurveC2.DisplayPolygonSpecified)
					curve.ShowPolygon = interpolationBezierCurveC2.DisplayPolygon;
				AddNewModel(curve);
			}

			foreach (var bezierSurfaceC0 in scene.BezierSurfacesC0)
			{
				var patches = new List<Patch>();
				foreach (var bezierSurfaceC0Patch in bezierSurfaceC0.Patches)
				{
					var pts = new CatPoint[4, 4];
					for (int i = 0; i < 4; i++)
						for (int j = 0; j < 4; j++)
							pts[i, j] = catPoints[bezierSurfaceC0Patch.Points[i, j]];
					var patch = new BezierPatch(pts)
					{
						ShowPolygon = false,
						HeightDiv = bezierSurfaceC0Patch.SurfaceDivisionsV,
						WidthDiv = bezierSurfaceC0Patch.SurfaceDivisionsU,
						Name = bezierSurfaceC0Patch.Name,
						UPos = bezierSurfaceC0Patch.PatchU,
						VPos = bezierSurfaceC0Patch.PatchV
					};
					AddNewModel(patch);
					patches.Add(patch);
				}

				var surfacePoints = patches.SelectMany(x => x.EnumerateCatPoints()).Distinct().ToList();
				var surface = new Surface(SurfaceType.Bezier, patches, surfacePoints, this)
				{
					Name = bezierSurfaceC0.Name,
					PatchesU = bezierSurfaceC0.PatchesU,
					PatchesV = bezierSurfaceC0.PatchesV
				};
				AddNewModel(surface);
			}

			foreach (var bezierSurfaceC2 in scene.BezierSurfacesC2)
			{
				var patches = new List<Patch>();
				foreach (var bezierSurfaceC0Patch in bezierSurfaceC2.Patches)
				{
					var pts = new CatPoint[4, 4];
					for (int i = 0; i < 4; i++)
						for (int j = 0; j < 4; j++)
							pts[i, j] = catPoints[bezierSurfaceC0Patch.Points[i, j]];
					var patch = new BSplinePatch(pts)
					{
						ShowPolygon = false,
						HeightDiv = bezierSurfaceC0Patch.SurfaceDivisionsV,
						WidthDiv = bezierSurfaceC0Patch.SurfaceDivisionsU,
						Name = bezierSurfaceC0Patch.Name,
						UPos = bezierSurfaceC0Patch.PatchU,
						VPos = bezierSurfaceC0Patch.PatchV
					};
					AddNewModel(patch);
					patches.Add(patch);
				}

				var surfacePoints = patches.SelectMany(x => x.EnumerateCatPoints()).Distinct().ToList();
				var surface = new Surface(SurfaceType.BSpline, patches, surfacePoints, this)
				{
					Name = bezierSurfaceC2.Name,
					PatchesU = bezierSurfaceC2.PatchesU,
					PatchesV = bezierSurfaceC2.PatchesV
				};
				AddNewModel(surface);
			}




		}

		private void ClearScene()
		{
			CatPoint.ResetID();
			Model.ResetId();

			Models.Clear();
			Points.Clear();
		}

	}
}
