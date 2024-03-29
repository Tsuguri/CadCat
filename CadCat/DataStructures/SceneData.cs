﻿using System;
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
using System.Windows.Media.Imaging;
using CadCat.GeometryModels.Proxys;
using CadCat.Utilities;
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

        #region Intersections

        private double newtonStep = 0.01;

        public double NewtonStep
        {
            get { return newtonStep; }
            set
            {
                if (System.Math.Abs(newtonStep - value) > double.Epsilon && value >= 0.005 && value < 0.2)
                {
                    newtonStep = value;
                    OnPropertyChanged();
                }
            }
        }

        private double cuttingCurveApproximation = 0.01;

        public double CuttingCurveApproximation
        {
            get
            {
                return cuttingCurveApproximation;
            }
            set
            {
                if (System.Math.Abs(cuttingCurveApproximation - value) > double.Epsilon && value >= 0.01 && value < 0.4)
                {
                    cuttingCurveApproximation = value;
                    OnPropertyChanged();
                }
            }
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

        public DialogHost Host { get; set; }

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
        private ICommand intersectSurfacesCommand;
        private ICommand drawIntersection;

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

        public ICommand IntersectSurfacesCommand => intersectSurfacesCommand ??
                                                    (intersectSurfacesCommand = new CommandHandler(IntersectSurfaces));


        public ICommand DrawIntersection => drawIntersection ??
                                                    (drawIntersection = new CommandHandler(DrawIntersectionInWindow));
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

        internal void CreateBezier()
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

        internal void CreateBezierPatch()
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
            PatchCycle cycle;

            try
            {
                cycle = CreatePatchCycle(pts, patches);

            }
            catch (Exception)
            {
                cycle = null;
            }
            if (cycle == null)
            {
                var sampleMessageDialog = new MessageHost
                {
                    Message = { Text = "Selected points and their patches does not create proper cycle." }
                };


                DialogHost.Show(sampleMessageDialog, "RootDialog");
            }
            else
                AddNewModel(new GregoryPatch(cycle, this));
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

        private void IntersectSurfaces()
        {
            List<Vector4> intersection = null;
            IIntersectable first = null;
            IIntersectable second = null;
            bool cyclic = false;
            try
            {

                var selected = models.Where(x => x.IsSelected).ToList();
                var sel = selected.Select(x => x as IIntersectable).Distinct().ToList();
                if (sel.Count == 2)
                {
                    first = sel[0];
                    second = sel[1];
                    if (first != null && second != null)
                        intersection = Intersection.Intersect(sel[0], sel[1], this, NewtonStep, out cyclic);

                }

            }
            catch (Exception e)
            {
                intersection = null;
            }

            if (intersection != null && intersection.Count > 2)
            {
                //intersection.ForEach(x => CreateHiddenCatPoint(sel[0].GetPosition(x.X, x.Y)));
                var curv = new GeometryModels.CuttingCurve(intersection, first, second, this, cyclic, CuttingCurveApproximation);
                AddNewModel(curv);
                first.SetCuttingCurve(curv);
                second.SetCuttingCurve(curv);
            }
            else
            {
                var sampleMessageDialog = new MessageHost
                {
                    Message = { Text = "Proper intersection could not be found." }
                };

                DialogHost.Show(sampleMessageDialog, "RootDialog");
            }
        }

        private void DrawIntersectionInWindow()
        {
            var selected = models.Where(x => x.IsSelected && x is GeometryModels.CuttingCurve).ToList();

            if (selected.Count != 1)
            {
                var sampleMessageDialog = new MessageHost
                {
                    Message = { Text = "Select one curve to draw." }
                };

                DialogHost.Show(sampleMessageDialog, "RootDialog");
            }
            else
            {
                var bufferBitmap = new WriteableBitmap(400, 200, 96, 96, PixelFormats.Pbgra32, null);
                var curve = selected[0] as GeometryModels.CuttingCurve;
                curve?.Draw(bufferBitmap);
                var sampleMessageDialog = new ImageHost();
                sampleMessageDialog.SetImage(bufferBitmap);

                var oldWidth = Host.Width;
                var oldHeigth = Host.Height;
                Host.Height = 350;
                Host.Width = 400;
                DialogHost.Show(sampleMessageDialog, "RootDialog");
                Host.Width = oldWidth;
                Host.Height = oldHeigth;
            }
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
        }

        private void LoadFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == false)
                return;

            LoadFromFile(openFileDialog.FileName);
        }

        public void LoadFromFile(string filename)
        {

            ClearScene();
        }

        private void ClearScene()
        {
            CatPoint.ResetID();
            Model.ResetId();

            Models.Clear();
            Points.Clear();
            hiddenPoints.Clear();
        }

    }
}
