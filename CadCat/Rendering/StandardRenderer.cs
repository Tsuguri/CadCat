using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CadCat.DataStructures;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using CadCat.Math;

namespace CadCat.Rendering
{
	class StandardRenderer : BaseRenderer
	{
		private ModelData modelData = new ModelData();

		public override void Render(SceneData scene)
		{
			base.Render(scene);
			var tmpLine = new Line();
			using (bufferBitmap.GetBitmapContext())
			{
				bufferBitmap.Clear(Colors.Black);

				var cameraMatrix = scene.ActiveCamera.ViewProjectionMatrix;

				Matrix4 activeMatrix = cameraMatrix;
				var activeModel = -1;

				int stroke = 1;

				foreach (var line in scene.GetLines(modelData))
				{
					if (modelData.ModelID != activeModel)
					{
						activeModel = modelData.ModelID;
						var modelmat = modelData.transform.CreateTransformMatrix();
						activeMatrix = cameraMatrix * modelmat;
						stroke = (scene.SelectedModel != null && scene.SelectedModel.ModelID == modelData.ModelID) ? 2 : 1;
					}

					ProcessLine(bufferBitmap, line, activeMatrix,Colors.Yellow, stroke);

				}
			}
		}
	}
}
