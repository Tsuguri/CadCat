using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CadCat
{
	class RenderingContext
	{
		private LineDrawData ldd;
		private RenderTargetBitmap buffer;
		private WriteableBitmap bufferBitmap;

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
		public RenderingContext(LineDrawData lineData, Image image)
		{
			targetImage = image;
			ldd = lineData;
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
				
				bufferBitmap.Clear(Colors.Bisque);
				for (int i = 0; i < 32; i++)
					for (int j = 0; j < 32; j++)
						bufferBitmap.DrawLineAa(i*40 + (int)ldd.points[0].X, j*40+100, 50+(int)ldd.points[1].X, 50 + (int)ldd.points[1].Y, Colors.Black);
			}

		}

		public void Resize(double width, double height)
		{
			buffer = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
			bufferBitmap = new WriteableBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32, null);
			targetImage.Source = bufferBitmap;

		}
	}
}
