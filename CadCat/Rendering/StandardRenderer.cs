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
		private BitmapContext bufferContext;

		public override void AfterRendering(SceneData scene)
		{
			bufferContext.Dispose();
		}

		public override void BeforeRendering(SceneData scene)
		{
			base.BeforeRendering(scene);
			bufferBitmap.Clear(Colors.Black);
			CameraMatrix = scene.ActiveCamera.ViewProjectionMatrix;
			bufferContext = bufferBitmap.GetBitmapContext();
		}
	}
}
