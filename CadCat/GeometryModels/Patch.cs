using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using CadCat.Math;
using CadCat.DataStructures;
using CadCat.Rendering;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace CadCat.GeometryModels
{
	public abstract class Patch : Model
	{
		protected bool ParametrizationChanged;
		protected bool Changed;
		private bool showPolygon;


		protected Surface Surface;
		protected bool owner;
		protected SceneData scene;
		//protected CatPoint[] points = new CatPoint[16];
		protected readonly CatPoint[,] pointsOrdererd = new CatPoint[4, 4];

		public bool ShowPolygon
		{
			get { return showPolygon; }
			set
			{
				showPolygon = value;
				OnPropertyChanged();
			}
		}
		protected static readonly List<int> Indices = new List<int>()
		{
			0,1,
			1,2,
			2,3,

			4,5,
			5,6,
			6,7,

			8,9,
			9,10,
			10,11,

			12,13,
			13,14,
			14,15,

			0,4,
			4,8,
			8,12,

			1,5,
			5,9,
			9,13,

			2,6,
			6,10,
			10,14,

			3,7,
			7,11,
			11,15
		};

		private int heightDiv = 3;
		private int widthDiv = 3;


		public void SetSurface(Surface surf)
		{
			Surface = surf;
		}
		public void ShouldRegenerate()
		{
			ParametrizationChanged = true;
		}

		public int UPos { get; set; }
		public int VPos { get; set; }

		public int WidthDiv
		{
			get { return widthDiv; }
			set
			{
				if (widthDiv != value && value > 0)
				{
					widthDiv = value;
					ParametrizationChanged = true;
					Changed = true;
					OnPropertyChanged();
				}
			}
		}

		public int HeightDiv
		{
			get { return heightDiv; }
			set
			{
				if (heightDiv != value && value > 0)
				{
					heightDiv = value;
					Changed = true;
					ParametrizationChanged = true;
					OnPropertyChanged();
				}
			}
		}

		protected Patch(CatPoint[,] pts)
		{
			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
				{
					//points[i * 4 + j] = pts[i, j]; // i -U, j- V
					pts[i, j].OnChanged += OnBezierPointChanged;
					pts[i, j].OnReplace += OnBezierPointReplaced;
				}

			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
				{
					pointsOrdererd[i, j] = pts[i, j];
				}
			ParametrizationChanged = true;
			Changed = true;
			owner = false;
		}

		protected void RecalculateParametrizationPoints()
		{
			var avai = Surface.GetAvaiablePatch(UPos, VPos, WidthDiv, HeightDiv);

			Func<Vector2, bool> check = vector2 => Surface.IsPointAvaiable(UPos, VPos, vector2);
			var aszk = SurfaceFilling.MarchingAszklars(avai, 1, 1, false, false, check);

			meshIndices = aszk.Item2;
			parametrizationPoints = aszk.Item1;
			ParametrizationChanged = false;
			Changed = true;
		}

		private void RecalculatePoints()
		{
			mesh = parametrizationPoints.Select(x => GetPoint(x.X, x.Y)).ToList();
			Changed = false;
		}

		protected List<Vector2> parametrizationPoints;
		protected List<Vector3> mesh = new List<Vector3>();
		protected List<int> meshIndices = new List<int>();

		public abstract Vector3 GetPoint(double u, double v);

		public CatPoint GetCatPoint(int u, int v)
		{
			//points[j * 4 + i] = pts[i, j]
			return pointsOrdererd[v, u];
		}
		public abstract Vector3 GetUDerivative(double u, double v);
		public abstract Vector3 GetVDerivative(double u, double v);

		protected void OnBezierPointChanged(CatPoint point)
		{
			Changed = true;
		}

		protected void OnBezierPointReplaced(CatPoint point, CatPoint newPoint)
		{
			if (point != newPoint)
			{

				//for (int i = 0; i < points.Length; i++)
				//	if (points[i] == point)
				//		points[i] = newPoint;
				for (int i = 0; i < pointsOrdererd.GetLength(1); i++)
					for (int j = 0; j < pointsOrdererd.GetLength(0); j++)
						if (pointsOrdererd[j, i] == point)
						{
							pointsOrdererd[j, i] = newPoint;
						}


				ParametrizationChanged = true;
				Changed = true;
				point.OnChanged -= OnBezierPointChanged;
				if (owner)
					point.DependentUnremovable -= 1;
				newPoint.OnChanged += OnBezierPointChanged;
				if (owner)
					newPoint.DependentUnremovable += 1;
				newPoint.OnChanged += OnBezierPointChanged;
				newPoint.OnReplace += OnBezierPointReplaced;
			}

		}

		public override void Render(BaseRenderer renderer)
		{
			if (ParametrizationChanged)
				RecalculateParametrizationPoints();
			if (Changed)
				RecalculatePoints();
			base.Render(renderer);

			renderer.ModelMatrix = GetMatrix(false, new Vector3());
			renderer.SelectedColor = IsSelected ? Colors.LimeGreen : Colors.White;
			renderer.UseIndices = true;

			if (ShowPolygon)
			{
				renderer.Indices = Indices;
				renderer.Points = EnumerateCatPoints().Select(x => x.Position).ToList();


				renderer.Transform();
				renderer.DrawLines();
			}


			renderer.Indices = meshIndices;
			renderer.Points = mesh;

			renderer.Transform();
			renderer.DrawLines();
		}

		public override IEnumerable<CatPoint> EnumerateCatPoints()
		{
			for (int i = 0; i < pointsOrdererd.GetLength(1); i++)
				for (int j = 0; j < pointsOrdererd.GetLength(0); j++)
					yield return pointsOrdererd[j, i];
		}

		public override void CleanUp()
		{
			base.CleanUp();
			if (owner)
			{
				for (int i = 0; i < pointsOrdererd.GetLength(1); i++)
					for (int j = 0; j < pointsOrdererd.GetLength(0); j++)
					{
						pointsOrdererd[j, i].DependentUnremovable -= 1;
						scene.RemovePoint(pointsOrdererd[j, i]);

					}
			}
		}



	}
}
