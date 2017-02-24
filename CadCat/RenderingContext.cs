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
			if (targetImage.Width > 10 && targetImage.Width < 1000)
				throw new Exception("lol");
			DrawingVisual dv = new DrawingVisual();

			#region stupid hack
			if (buffer == null) //stupid hack
			{
				buffer = new RenderTargetBitmap(1, 1, 96, 96, PixelFormats.Pbgra32);
				targetImage.Source = buffer;
			}
			#endregion

			buffer.Clear();
			using (DrawingContext dc = dv.RenderOpen())
			{
				for (int i = 0; i < 300;i++)
					dc.DrawLine(linePen, ldd.points[0], ldd.points[1]);
		//			foreach (Line l in ldd.GetLines())
						//dc.DrawLine(linePen, l.from, l.to);
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
