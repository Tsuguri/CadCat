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
		private Image targetImage;
		public RenderingContext(LineDrawData lineData, Image image)
		{
			targetImage = image;
			ldd = lineData;
		}

		public void UpdatePoints(Size size)
		{
			if (targetImage.Width > 10 && targetImage.Width < 1000)
				throw new Exception("lol");
			DrawingVisual dv = new DrawingVisual();
			Pen pen = new Pen(Brushes.Black, 5);
			if (buffer == null)
			{
				buffer = new RenderTargetBitmap(500,500, 96, 96, PixelFormats.Pbgra32);
				targetImage.Source = buffer;
			}
			buffer.Clear();
			using (DrawingContext dc = dv.RenderOpen())
			{
				foreach (Line l in ldd.GetLines())
					dc.DrawLine(pen, l.from, l.to);
				dc.DrawLine(pen, new Point(0, 0), new Point(size.Width, size.Height));
				dc.DrawRectangle(Brushes.Black, pen, new Rect(0, 0, 200, 200));
				dc.Close();
			}

			buffer.Render(dv);
			targetImage.InvalidateVisual();
		}

		public void Resize(double width, double height)
		{
			buffer = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
			targetImage.Source = buffer;

		}
	}
}
