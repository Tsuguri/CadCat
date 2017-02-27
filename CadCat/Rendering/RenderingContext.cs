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
	class RenderingContext
	{
		private ModelsData models;
		private WriteableBitmap bufferBitmap;
		private ModelData modelData = new ModelData();

		private Image targetImage;
		private Brush color;
		private Pen linePen;
		DrawingVisual visual = new DrawingVisual();
		public int thickness
		{
			get; set;
		}
		public Brush LineColor
		{
			get { return color; }
			set
			{
				color = value;
				linePen = new Pen(color, thickness);
			}
		}
		public Camera ActiveCamera
		{
			get; set;
		}
		public RenderingContext(ModelsData lineData, Image image)
		{
			targetImage = image;
			models = lineData;
			thickness = 1;
			LineColor = Brushes.Black;
		}

		public void UpdatePoints()
		{
			#region stupid hack
			if (bufferBitmap == null) //stupid hack
			{
				bufferBitmap = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Pbgra32, null);
				targetImage.Source = bufferBitmap;
			}
			#endregion

			using (bufferBitmap.GetBitmapContext())
			{
				bufferBitmap.Clear(Colors.Black);

				var cameraMatrix = ActiveCamera.ViewProjectionMatrix;

				Matrix4 activeMatrix = cameraMatrix;
				var activeModel = -1;
				var width = bufferBitmap.PixelWidth;
				var height = bufferBitmap.PixelHeight;

				foreach (var line in models.GetLines(modelData))
				{
					if (modelData.ModelID != activeModel)
					{
						activeModel = modelData.ModelID;
						var modelmat = modelData.transform.CreateTransformMatrix();
						activeMatrix = cameraMatrix * modelmat;
					}
					var from = (activeMatrix * new Vector4 (line.from,1)).ToNormalizedVector3();
					var to = (activeMatrix * new Vector4 (line.to,1)).ToNormalizedVector3();

					from.X += 0.5;
					from.Y += 0.5;
					from.Y = 1 - from.Y;

					to.X += 0.5;
					to.Y += 0.5;
					to.Y = 1 - to.Y;
					bufferBitmap.DrawLineAa((int)(from.X * width), (int)(from.Y * height), (int)(to.X * width), (int)(to.Y * height), Colors.Gold, 4);
				}
			}
		}

		public void Resize(double width, double height)
		{
			bufferBitmap = new WriteableBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32, null);
			targetImage.Source = bufferBitmap;
		}
	}
}
