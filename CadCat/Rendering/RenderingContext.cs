using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CadCat.DataStructures;
using CadCat.Math;

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
		private SceneData Scene;
		private Image targetImage;
		private BaseRenderer renderer = new StandardRenderer();
		//private Image targetImage;
		DrawingVisual visual = new DrawingVisual();
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
			Scene = lineData;
			Thickness = 1;
			SelectedItemThickness = 3;
			LineColor = Colors.Gold;
		}

		public void UpdatePoints()
		{
			if(Scene.RenderingMode!=rendererType)
			{
				switch (Scene.RenderingMode)
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
				rendererType = Scene.RenderingMode;
				Scene.Renderer = renderer;
			}

			renderer.Render(Scene);
			
		}


		public void Resize(double width, double height)
		{
			renderer.Resize(width, height);
			imageSize.X = width;
			imageSize.Y = height;
		}
	}
}
