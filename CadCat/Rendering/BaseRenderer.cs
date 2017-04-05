using CadCat.DataStructures;
using CadCat.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CadCat.Rendering
{
	public abstract class BaseRenderer : Utilities.BindableObject
	{
		public WriteableBitmap bufferBitmap;
		private Image targetImage;
		private Line tmpLine = new Line();

		protected int width, height;


		public virtual void Resize(double width, double height)
		{
			bufferBitmap = new WriteableBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32, null);
			targetImage.Source = bufferBitmap;
		}

		public virtual void SetImageContent(Image img)
		{
			targetImage = img;
			img.Source = bufferBitmap;
		}
		public virtual void Render(SceneData scene)
		{
			if (bufferBitmap == null) //stupid hack
			{
				bufferBitmap = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Pbgra32, null);
				targetImage.Source = bufferBitmap;
			}

			width = bufferBitmap.PixelWidth;
			height = bufferBitmap.PixelHeight;

		}

		private double CountClipParameter(double from, double to, double margin)
		{
			return System.Math.Abs(to - margin) / System.Math.Abs(to - from);
		}

		private bool ClipAxis(DataStructures.Line clipped, double fromAxis, double toAxis, double farMargin, double closeMargin)
		{
			if ((fromAxis > farMargin && toAxis > farMargin) || (fromAxis < closeMargin && toAxis < closeMargin))
				return false;
			if (toAxis > farMargin)
			{
				var l = CountClipParameter(fromAxis, toAxis, farMargin);
				clipped.to = clipped.to * (1 - l) + clipped.from * l;
			}
			if (fromAxis > farMargin)
			{
				var l = CountClipParameter(toAxis, fromAxis, farMargin);
				clipped.from = clipped.to * l + clipped.from * (1 - l);
			}

			if (toAxis < closeMargin)
			{
				var l = CountClipParameter(fromAxis, toAxis, closeMargin);
				clipped.to = clipped.to * (1 - l) + clipped.from * l;
			}
			if (fromAxis < closeMargin)
			{
				var l = CountClipParameter(toAxis, fromAxis, closeMargin);
				clipped.from = clipped.to * l + clipped.from * (1 - l);
			}
			return true;
		}


		public bool Clip(DataStructures.Line clipped, double farMargin = 1.0, double closeMargin = 0.0)
		{
			return clipped.from.Z > 0.0 && clipped.to.Z > 0.0 &&
					ClipAxis(clipped, clipped.from.X, clipped.to.X, farMargin, closeMargin) &&
					ClipAxis(clipped, clipped.from.Y, clipped.to.Y, farMargin, closeMargin);
		}

		public bool ClipPoint(Vector3 point, double farMargin = 1.0, double closeMargin = 0.0)
		{
			return point.Z > 0.0 && point.X < farMargin && point.X > closeMargin && point.Y < farMargin && point.Y > closeMargin;
		}

		public static Vector3 NormalizeToBitmapSpace(Vector3 vec)
		{
			vec.X = vec.X / 2 + 0.5;
			vec.Y = vec.Y / 2 + 0.5;
			vec.Z = vec.Z / 2 + 0.5;
			vec.Y = 1 - vec.Y;
			return vec;
		}

		protected void DrawLine(WriteableBitmap bitmap, Line line, Color color, int stroke)
		{
			//bitmap.DrawLineAa((int)(line.from.X * width), (int)(line.from.Y * height), (int)(line.to.X * width), (int)(line.to.Y * height), color, stroke);
			bitmap.DrawLineDDA((int)(line.from.X * width), (int)(line.from.Y * height), (int)(line.to.X * width), (int)(line.to.Y * height), color);
		}

		protected void DrawPoint(WriteableBitmap bitmap, Vector3 point, Color color)
		{
			bitmap.FillEllipse((int)(point.X * width - 4), (int)(point.Y * height - 4), (int)(point.X * width + 4), (int)(point.Y * height + 4), color);
			//bitmap.FillRectangle((int)(point.X * width-1), (int)(point.Y * height-1), (int)(point.X * width + 1), (int)(point.Y * height + 1), color);
		}

		protected void ProcessLine(WriteableBitmap bitmap, Line line, Matrix4 matrix, Color color, int stroke)
		{
			var from = (matrix * new Vector4(line.from, 1)).ToNormalizedVector3();
			from = NormalizeToBitmapSpace(from);

			var to = (matrix * new Vector4(line.to, 1)).ToNormalizedVector3();
			to = NormalizeToBitmapSpace(to);

			tmpLine.from = from;
			tmpLine.to = to;
			if (Clip(tmpLine, 0.99, 0.01))
				DrawLine(bitmap, tmpLine, color, stroke);
		}

		protected void ProcessPoint(WriteableBitmap bitmap, Vector3 point, Matrix4 matrix, Color color)
		{
			var pt = (matrix * new Vector4(point, 1)).ToNormalizedVector3();
			pt = NormalizeToBitmapSpace(pt);

			if (ClipPoint(pt))
				DrawPoint(bitmap, pt, color);
		}
	}
}
