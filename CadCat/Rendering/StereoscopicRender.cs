using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using System.Windows.Media.Imaging;
using CadCat.Math;
using System.Windows.Media;

namespace CadCat.Rendering
{
	class StereoscopicRender : BaseRenderer
	{
		private ModelData modelData = new ModelData();
		//public WriteableBitmap leftBitmap;
		public WriteableBitmap rightBitmap;
		private Color leftEye = Color.FromRgb(0, 240, 255);
		private Color rightEye = Colors.Red;

		public override void Resize(double width, double height)
		{
			base.Resize(width, height);
			rightBitmap = new WriteableBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32, null);
		}
		public override void Render(SceneData scene)
		{
			base.Render(scene);

			var tmpLine = new Line();

			#region GetContextAndClear

			var rightContext = rightBitmap.GetBitmapContext();
			rightBitmap.Clear(Colors.Black);
			var leftContext = bufferBitmap.GetBitmapContext();
			bufferBitmap.Clear(Colors.Black);

			#endregion

			var cameraLeftMatrix = scene.ActiveCamera.GetLeftEyeMatrix(scene.EyeDistance/2.0,scene.ActiveCamera.Radius*5);
			var cameraRightMatrix = scene.ActiveCamera.GetRightEyeMatrix(scene.EyeDistance / 2.0, scene.ActiveCamera.Radius*5);

			Matrix4 activeLeftMatrix = cameraLeftMatrix;
			Matrix4 activeRightMatrix = cameraRightMatrix;
			var activeModel = -1;
			int stroke = 1;

			foreach (var line in scene.GetLines(modelData))
			{
				if (modelData.ModelID != activeModel)
				{
					activeModel = modelData.ModelID;
					var modelmat = modelData.transform.CreateTransformMatrix();
					activeLeftMatrix = cameraLeftMatrix * modelmat;
					activeRightMatrix = cameraRightMatrix * modelmat;
					stroke = (scene.SelectedModel != null && scene.SelectedModel.ModelID == modelData.ModelID) ? 2 : 1;
				}
				ProcessLine(bufferBitmap, line, activeLeftMatrix, leftEye, stroke);
				ProcessLine(rightBitmap, line, activeRightMatrix, rightEye, stroke);
			}

			#region BlitAndDispose
			bufferBitmap.Blit(new System.Windows.Rect(0, 0, bufferBitmap.Width, bufferBitmap.Height), rightBitmap, new System.Windows.Rect(0, 0, rightBitmap.Width, rightBitmap.Height), WriteableBitmapExtensions.BlendMode.Additive);
			rightContext.Dispose();
			leftContext.Dispose();
			#endregion
		}
	}
}
