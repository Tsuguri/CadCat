using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;

namespace CadCat.GeometryModels
{
	class Surface : PointModel
	{

		private List<BezierPatch> patches;
		private List<CatPoint> catPoints;
		private SceneData scene;

		public override void CleanUp()
		{
			base.CleanUp();

			foreach (var bezierPatch in patches)
			{
				scene.RemoveModel(bezierPatch);
			}

			foreach (var catPoint in catPoints)
			{
				scene.RemovePoint(catPoint);
			}

		}

		public Surface(List<BezierPatch> patches, List<CatPoint> catPoints, SceneData scene)
		{
			this.patches = patches;
			this.catPoints = catPoints;
			this.scene = scene;
		}

		public override string GetName()
		{
			return "Surface "+base.GetName();
		}
	}
}
