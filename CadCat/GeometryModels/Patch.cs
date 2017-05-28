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

		public bool ShowPolygon
		{
			get { return showPolygon; }
			set
			{
				showPolygon = value;
				OnPropertyChanged();
			}
		}


		private bool showNormal;

		public bool ShowNormal
		{
			get { return showNormal; }
			set
			{
				showNormal = value;
				OnPropertyChanged();
			}
		}


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
