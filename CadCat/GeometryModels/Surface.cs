using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CadCat.DataStructures;
using CadCat.Utilities;

namespace CadCat.GeometryModels
{
	class Surface : Model
	{

		private readonly List<Patch> patches;
		private readonly List<CatPoint> catPoints;
		private readonly SceneData scene;

		private bool showPolygon = true;
		private bool showPoints = true;

		private ICommand bothDivUpCommand;
		private ICommand bothDivDownCommand;

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
				catPoint.Removeable = true;
				scene.RemovePoint(catPoint);
			}

		}

		public Surface(Proxys.SurfaceType surfacetype, List<Patch> patches, List<CatPoint> catPoints, SceneData scene)
		{
			this.surfacetype = surfacetype;
			this.patches = patches;
			this.catPoints = catPoints;
			this.scene = scene;

			this.catPoints.ForEach(x => x.OnReplace += OnPointReplaced);
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
			catPoints.Remove(point);
			if (!catPoints.Contains(newPoint))
			{
				catPoints.Add(newPoint);
				newPoint.Removeable = false;
				newPoint.OnReplace += OnPointReplaced;
			}
		}

		public override string GetName()
		{
			return "Surface " + base.GetName();
		}
	}
}
