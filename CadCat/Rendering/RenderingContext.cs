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
				foreach (var line in models.GetLines(modelData))
				{
					//apply transformations based on modelData transform and stuff
					var transformedLine = line;
					var from = transformedLine.from;
					from.X += modelData.transform.Position.X;
					from.Y += modelData.transform.Position.Y;
					transformedLine.from = from;
					transformedLine.to.X += modelData.transform.Position.X;
					transformedLine.to.Y += modelData.transform.Position.Y;

					bufferBitmap.DrawLineAa((int)transformedLine.from.X, (int)transformedLine.from.Y, (int)transformedLine.to.X, (int)transformedLine.to.Y, Colors.Gold);
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
