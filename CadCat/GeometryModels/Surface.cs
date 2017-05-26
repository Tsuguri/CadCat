using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CadCat.DataStructures;
using CadCat.Math;
using CadCat.ModelInterfaces;
using CadCat.Utilities;
using Xceed.Wpf.DataGrid;

namespace CadCat.GeometryModels
{


	public class Surface : Model, IIntersectable
	{
		private readonly ObservableCollection<CuttingCurveWrapper> cuttingCurves = new ObservableCollection<CuttingCurveWrapper>();
		private bool eachOrAny;

		public bool EachOrAny
		{
			get { return eachOrAny; }
			set
			{
				if (eachOrAny != value)
				{
					eachOrAny = value;
					OnPropertyChanged();
					SideChanged();
				}
			}
		}
		public ObservableCollection<CuttingCurveWrapper> CuttingCurves => cuttingCurves;
		private readonly List<Patch> patches;
		private readonly List<CatPoint> catPoints;
		private readonly SceneData scene;

		private readonly bool uLooped;
		private readonly bool vLooped;
		private bool showPolygon;
		private bool showPoints;

		private ICommand bothDivUpCommand;
		private ICommand bothDivDownCommand;

		private readonly Patch[,] orderedPatches;

		public int PatchesU { get; set; }
		public int PatchesV { get; set; }
		public ICommand BothDivUpCommand => bothDivUpCommand ?? (bothDivUpCommand = new CommandHandler(BothDivUp));

		public ICommand BothDivDownCommand => bothDivDownCommand ?? (bothDivDownCommand = new CommandHandler(BothDivDown));

		public bool ShowPolygon
		{
			get { return showPolygon; }
			set
			{
				showPolygon = value;
				OnPropertyChanged();
				foreach (var bezierPatch in patches)
				{
					bezierPatch.ShowPolygon = showPolygon;
				}
			}
		}

		private bool showNormals;
		public bool ShowNormals
		{
			get { return showNormals; }
			set
			{
				showNormals = value;
				OnPropertyChanged();
				foreach (var bezierPatch in patches)
				{
					bezierPatch.ShowNormal = showNormals;
				}
			}
		}

		public bool ShowPoints
		{
			get { return showPoints; }
			set
			{
				showPoints = value;
				OnPropertyChanged();
				foreach (var catPoint in catPoints)
				{
					catPoint.Visible = showPoints;
				}
			}
		}

		public Proxys.SurfaceType SurfaceType { get; }

		public IEnumerable<Patch> GetPatches()
		{
			return patches;
		}

		public override void CleanUp()
		{
			base.CleanUp();

			foreach (var bezierPatch in patches)
			{
				scene.RemoveModel(bezierPatch);
			}

			foreach (var catPoint in catPoints)
			{
				catPoint.DependentUnremovable -= 1;
				scene.RemovePoint(catPoint);
			}

		}

		public Surface(Proxys.SurfaceType surfacetype, Patch[,] patches, List<CatPoint> catPoints, SceneData scene, bool uLooped, bool vLooped)
		{
			SurfaceType = surfacetype;
			this.patches = new List<Patch>();
			this.uLooped = uLooped;
			this.vLooped = vLooped;
			foreach (var patch in patches)
			{
				this.patches.Add(patch);
				patch.SetSurface(this);
			}
			this.catPoints = catPoints;
			this.scene = scene;
			PatchesU = patches.GetLength(1);
			PatchesV = patches.GetLength(0);

			this.catPoints.ForEach(x =>
			{
				x.OnReplace += OnPointReplaced;
				x.DependentUnremovable += 1;
			});

			orderedPatches = patches;
			ShowPoints = ShowPoints;
		}

		private void BothDivUp()
		{
			foreach (var patch in patches)
			{
				patch.HeightDiv += 1;
				patch.WidthDiv += 1;
			}
		}

		private void BothDivDown()
		{
			foreach (var patch in patches)
			{
				patch.HeightDiv -= 1;
				patch.WidthDiv -= 1;
			}
		}

		private void OnPointReplaced(CatPoint point, CatPoint newPoint)
		{
			point.OnReplace -= OnPointReplaced;
			point.DependentUnremovable -= 1;
			catPoints.Remove(point);
			if (!catPoints.Contains(newPoint))
			{
				catPoints.Add(newPoint);
				newPoint.DependentUnremovable += 1;
				newPoint.OnReplace += OnPointReplaced;
			}
		}

		public override string GetName()
		{
			return "Surface " + base.GetName();
		}

		#region Intersectable



		public float FirstParamLimit => PatchesU;
		public float SecondParamLimit => PatchesV;
		public bool FirstParamLooped => false;
		public bool SecondParamLooped => false;

		public bool[,] GetAvaiablePatch(int u, int v, int uDiv, int vDiv)
		{
			var avaiable = new bool[vDiv, uDiv];
			bool val = eachOrAny || cuttingCurves.Count == 0;
			for (int i = 0; i < uDiv; i++)
			{
				for (int j = 0; j < vDiv; j++)
					avaiable[j, i] = val;
			}

			foreach (var cuttingCurveWrapper in cuttingCurves)
			{
				cuttingCurveWrapper.curve.PointsContainedByCurve(avaiable, cuttingCurveWrapper.Side, this, u, u + 1, v, v + 1, eachOrAny);
			}

			return avaiable;
		}

		public bool IsPointAvaiable(int u, int v, Vector2 point)
		{
			point.X += u;
			point.Y += v;
			if (!eachOrAny)
				return cuttingCurves.Any(x => x.curve.PointAvaiable(this, point, x.Side));
			return cuttingCurves.All(x => x.curve.PointAvaiable(this, point, x.Side));
		}

		public Vector3 GetPosition(double firstParam, double secondParam)
		{


			var prs = ConfirmParams(firstParam, secondParam);
			if (prs == null)
				throw new ArgumentException("given parameters exceeds surface bounds");

			int u = (int)System.Math.Floor(prs.Value.X);
			int v = (int)System.Math.Floor(prs.Value.Y);
			double uNormalized = prs.Value.X - u;
			double vNormalized = prs.Value.Y - v;

			if (u == PatchesU && uNormalized < 0.01 || v == PatchesV && vNormalized < 0.01)
			{
				var newU = u == PatchesU ? u - 1 : u;
				var newUnormalized = u == PatchesU ? 1.0f : uNormalized;
				var newV = v == PatchesV ? v - 1 : v;
				var newVNormalized = v == PatchesV ? 1.0f : vNormalized;
				return orderedPatches[newV, newU].GetPoint(newUnormalized, newVNormalized);
			}

			if (u >= PatchesU || v >= PatchesV)
			{
				throw new ArgumentException("Invalid argument, parametrization out of scope!");
			}

			return orderedPatches[v, u].GetPoint(uNormalized, vNormalized);
		}

		public Vector3 GetFirstParamDerivative(double firstParam, double secondParam)
		{
			int u = (int)System.Math.Floor(firstParam);
			int v = (int)System.Math.Floor(secondParam);
			double uNormalized = firstParam - u;
			double vNormalized = secondParam - v;

			if (u == PatchesU && uNormalized < 0.0001 || v == PatchesV && vNormalized < 0.0001f)
			{
				var newU = u == PatchesU ? u - 1 : u;
				var newUnormalized = u == PatchesU ? 1.0f : uNormalized;
				var newV = v == PatchesV ? v - 1 : v;
				var newVNormalized = v == PatchesV ? 1.0f : vNormalized;
				return orderedPatches[newU, newV].GetUDerivative(newUnormalized, newVNormalized);
			}

			if (u >= PatchesU || v >= PatchesV)
			{
				throw new ArgumentException("Invalid argument, parametrization out of scope!");
			}
			return orderedPatches[v, u].GetUDerivative(uNormalized, vNormalized);
		}

		public Vector3 GetSecondParamDerivative(double firstParam, double secondParam)
		{
			int u = (int)System.Math.Floor(firstParam);
			int v = (int)System.Math.Floor(secondParam);
			double uNormalized = firstParam - u;
			double vNormalized = secondParam - v;

			if (u == PatchesU && uNormalized < 0.0001 || v == PatchesV && vNormalized < 0.0001f)
			{
				var newU = u == PatchesU ? u - 1 : u;
				var newUnormalized = u == PatchesU ? 1.0f : uNormalized;
				var newV = v == PatchesV ? v - 1 : v;
				var newVNormalized = v == PatchesV ? 1.0f : vNormalized;
				return orderedPatches[newU, newV].GetVDerivative(newUnormalized, newVNormalized);
			}

			if (u >= PatchesU || v >= PatchesV)
			{
				throw new ArgumentException("Invalid argument, parametrization out of scope!");
			}
			return orderedPatches[v, u].GetVDerivative(uNormalized, vNormalized);
		}

		public ParametrizedPoint GetClosestPointParams(Vector3 point)
		{
			throw new NotImplementedException();
		}

		public Vector2? ConfirmParams(double u, double v)
		{
			

			if ((u < 0.0 || u > PatchesU) && !uLooped)
				return null;
			if ((v < 0.0 || v > PatchesV) && !vLooped)
				return null;
			Vector2 ret = new Vector2(u, v);
			if (ret.X < 0)
				ret.X += PatchesU;
			if (ret.X > PatchesU)
				ret.X -= PatchesU;
			if (ret.Y < 0)
				ret.Y += PatchesV;
			if (ret.Y > PatchesV)
				ret.Y -= PatchesV;
			return ret;
		}

		public Vector2 ClipParams(double u, double v)
		{
			double uu = 0, vv = 0;
			if (!uLooped)
				uu = System.Math.Min(System.Math.Max(0, u), PatchesU);
			if (!vLooped)
				vv = System.Math.Min(System.Math.Max(0, v), PatchesV);
			if (uLooped && u < 0)
				uu = u + PatchesU;
			if (uLooped && u > PatchesU)
				uu = u - PatchesU;
			if (vLooped && v < 0)
				vv = v + PatchesV;
			if (vLooped && v > PatchesV)
				vv = v - PatchesV;
			return new Vector2(uu, vv);

		}

		public void RemoveCurve(CuttingCurve curve)
		{
			var p = CuttingCurves.Where(x => x.curve == curve);
			var cur = p.FirstOrDefault();
			if (cur != null)
				cuttingCurves.Remove(cur);
			SideChanged();
		}

		public IEnumerable<ParametrizedPoint> GetPointsForSearch(int firstParamDiv, int secondParamDiv)
		{
			float uDiv = PatchesU / (float)(firstParamDiv + 2);
			float vDiv = PatchesV / (float)(secondParamDiv + 2);

			for (int i = 1; i < firstParamDiv + 1; i++)
			{
				for (int j = 1; j < secondParamDiv + 1; j++)
				{
					yield return new ParametrizedPoint { Parametrization = new Vector2(i * uDiv, j * vDiv), Position = GetPosition(i * uDiv, j * vDiv) };
				}
			}
		}

		public void SetCuttingCurve(CuttingCurve curve)
		{
			if (!curve.IsIntersectable(this))
				return;

			cuttingCurves.Add(new CuttingCurveWrapper(curve, this));
			foreach (var orderedPatch in orderedPatches)
			{
				orderedPatch.ShouldRegenerate();
			}
		}

		public void SideChanged()
		{
			foreach (var orderedPatch in orderedPatches)
			{
				orderedPatch.ShouldRegenerate();
			}
		}

		#endregion

	}
}
