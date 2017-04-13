using CadCat.DataStructures;
using System.Windows.Media.Imaging;
using System.Windows.Media;

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
