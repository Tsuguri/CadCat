using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CadCat.DataStructures;

namespace CadCat.Rendering
{
	public enum RendererType
	{
		Standard,
		Stereoscopic
	}
	class RenderingContext
	{
		Point imageSize;
		RendererType rendererType;
		private readonly SceneData scene;
		private readonly Image targetImage;
		private BaseRenderer renderer = new StandardRenderer();

		public int Thickness
		{
			get;
			set;
		}
		public int SelectedItemThickness
		{
			get; set;
		}
		public Color LineColor
		{
			get;
			set;
		}

		public RenderingContext(SceneData lineData, Image image)
		{
			targetImage = image;
			rendererType = lineData.RenderingMode;
			renderer.SetImageContent(image);
			scene = lineData;
			Thickness = 1;
			SelectedItemThickness = 3;
			LineColor = Colors.Gold;
		}

		public void UpdatePoints()
		{
			if(scene.RenderingMode!=rendererType)
			{
				switch (scene.RenderingMode)
				{
					case RendererType.Standard:
						renderer = new StandardRenderer();
						break;
					case RendererType.Stereoscopic:
						renderer = new StereoscopicRender();
						break;
					default:
						renderer = new StandardRenderer();
						break;
				}
				renderer.SetImageContent(targetImage);
				renderer.Resize(imageSize.X, imageSize.Y);
				rendererType = scene.RenderingMode;
				scene.CurrentRenderer = renderer;
			}

			renderer.BeforeRendering(scene);

			scene.Render(renderer);

			renderer.AfterRendering(scene);
			
		}


		public void Resize(double width, double height)
		{
			renderer.Resize(width, height);
			imageSize.X = width;
			imageSize.Y = height;
		}
	}
}
