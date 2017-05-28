using System;
using System.Collections.Generic;
using CadCat.Math;
using CadCat.DataStructures;

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
		protected CatPoint[] points = new CatPoint[16];

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

		protected List<Vector2> parametrizationPoints;
		protected List<Vector3> mesh = new List<Vector3>();
		protected List<int> meshIndices = new List<int>();

		public abstract Vector3 GetPoint(double u, double v);
		public abstract CatPoint GetCatPoint(int u, int v);
		public abstract Vector3 GetUDerivative(double u, double v);
		public abstract Vector3 GetVDerivative(double u, double v);
	}
}
