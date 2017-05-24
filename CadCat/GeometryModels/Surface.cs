using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CadCat.DataStructures;
using CadCat.Math;
using CadCat.ModelInterfaces;
using CadCat.Utilities;

namespace CadCat.GeometryModels
{
	class Surface : Model, IIntersectable
	{

		private readonly List<Patch> patches;
		private readonly List<CatPoint> catPoints;
		private readonly SceneData scene;

		private bool uLooped;
		private bool vLooped;
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
		private Proxys.SurfaceType surfacetype;
		public Proxys.SurfaceType SurfaceType => surfacetype;

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
			this.surfacetype = surfacetype;
			this.patches = new List<Patch>();
			this.uLooped = uLooped;
			this.vLooped = vLooped;
			foreach (var patch in patches)
			{
				this.patches.Add(patch);
			}
			this.catPoints = catPoints;
			this.scene = scene;
			this.PatchesU = patches.GetLength(1);
			this.PatchesV = patches.GetLength(0);

			this.catPoints.ForEach(x =>
			{
				x.OnReplace += OnPointReplaced;
				x.DependentUnremovable += 1;
			});

			this.orderedPatches = patches;
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



		public Vector3 GetPosition(double firstParam, double secondParam)
		{
			int U = (int)System.Math.Floor(firstParam);
			int V = (int)System.Math.Floor(secondParam);
			double uNormalized = firstParam - U;
			double vNormalized = secondParam - V;

			if (U == PatchesU && uNormalized < 0.005 || V == PatchesV && vNormalized < 0.005f)
			{
				var newU = U == PatchesU ? U - 1 : U;
				var newUnormalized = U == PatchesU ? 1.0f : uNormalized;
				var newV = V == PatchesV ? V - 1 : V;
				var newVNormalized = V == PatchesV ? 1.0f : vNormalized;
				return orderedPatches[newV, newU].GetPoint(newUnormalized, newVNormalized);
			}

			if (U >= PatchesU || V >= PatchesV)
			{
				throw new ArgumentException("Invalid argument, parametrization out of scope!");
			}
			return orderedPatches[V, U].GetPoint(uNormalized, vNormalized);
		}

		public Vector3 GetFirstParamDerivative(double firstParam, double secondParam)
		{
			int U = (int)System.Math.Floor(firstParam);
			int V = (int)System.Math.Floor(secondParam);
			double uNormalized = firstParam - U;
			double vNormalized = secondParam - V;

			if (U == PatchesU && uNormalized < 0.0001 || V == PatchesV && vNormalized < 0.0001f)
			{
				var newU = U == PatchesU ? U - 1 : U;
				var newUnormalized = U == PatchesU ? 1.0f : uNormalized;
				var newV = V == PatchesV ? V - 1 : V;
				var newVNormalized = V == PatchesV ? 1.0f : vNormalized;
				return orderedPatches[newU, newV].GetPoint(newUnormalized, newVNormalized);
			}

			if (U >= PatchesU || V >= PatchesV)
			{
				throw new ArgumentException("Invalid argument, parametrization out of scope!");
			}
			return orderedPatches[V, U].GetUDerivative(uNormalized, vNormalized);
		}

		public Vector3 GetSecondParamDerivative(double firstParam, double secondParam)
		{
			int U = (int)System.Math.Floor(firstParam);
			int V = (int)System.Math.Floor(secondParam);
			double uNormalized = firstParam - U;
			double vNormalized = secondParam - V;

			if (U == PatchesU && uNormalized < 0.0001 || V == PatchesV && vNormalized < 0.0001f)
			{
				var newU = U == PatchesU ? U - 1 : U;
				var newUnormalized = U == PatchesU ? 1.0f : uNormalized;
				var newV = V == PatchesV ? V - 1 : V;
				var newVNormalized = V == PatchesV ? 1.0f : vNormalized;
				return orderedPatches[newU, newV].GetPoint(newUnormalized, newVNormalized);
			}

			if (U >= PatchesU || V >= PatchesV)
			{
				throw new ArgumentException("Invalid argument, parametrization out of scope!");
			}
			return orderedPatches[V, U].GetVDerivative(uNormalized, vNormalized);
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

		#endregion

	}
}
