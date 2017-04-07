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
		protected Line tmpLine = new Line();

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

		public enum RenderingType
		{
			Points,
			Lines
		}

		protected List<Vector3> points;
		protected List<Vector3> transformedPoints = new List<Vector3>();

		public List<Vector3> Points
		{
			set
			{
				points = value;
			}
		}

		protected List<int> indices;

		public List<int> Indices
		{
			set
			{
				indices = value;
			}
		}

		protected Matrix4 modelMatrix;

		public Matrix4 ModelMatrix
		{
			set
			{
				modelMatrix = value;
			}
		}

		protected Matrix4 cameraMatrix;

		public Matrix4 CameraMatrix
		{
			set
			{
				cameraMatrix = value;
			}
		}

		protected Color selectedColor;

		public Color SelectedColor
		{
			set
			{
				selectedColor = value;
			}
		}

		public bool UseIndices
		{
			protected get; set;
		}

		public int LineStroke
		{
			protected get; set;
		}

		public RenderingType RenderingMode = RenderingType.Points;

		public virtual void BeforeRendering(SceneData scene)
		{
			if (bufferBitmap == null) //stupid hack
			{
				bufferBitmap = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Pbgra32, null);
				targetImage.Source = bufferBitmap;
			}

			width = bufferBitmap.PixelWidth;
			height = bufferBitmap.PixelHeight;
		}

		public abstract void AfterRendering(SceneData scene);


		public virtual void Transform()
		{
			var activeMatrix = cameraMatrix * modelMatrix;
			transformedPoints = TransformVertexBuffer(activeMatrix);

		}

		protected List<Vector3> TransformVertexBuffer(Matrix4 mat)
		{
			var pts = new List<Vector3>();

			for (int i = 0; i < points.Count; i++)
			{
				pts.Insert(i, NormalizeToBitmapSpace((mat * new Vector4(points[i], 1)).ToNormalizedVector3()));
			}
			return pts;
		}

		protected virtual void RenderLine(int index1, int index2)
		{
			tmpLine.from = transformedPoints[index1];
			tmpLine.to = transformedPoints[index2];
			if (Clip(tmpLine, 0.99, 0.01))
				DrawLine(bufferBitmap, tmpLine, selectedColor, LineStroke);
		}

		protected virtual void RenderPoint(int index)
		{
			if (ClipPoint(transformedPoints[index]))
				DrawPoint(bufferBitmap, transformedPoints[index], selectedColor);
		}

		public void DrawLines()
		{
			if (UseIndices)
				for (int i = 0; i < indices.Count - 1; i += 2)
				{
					RenderLine(indices[i], indices[i + 1]);
				}
			else
				for (int i = 0; i < transformedPoints.Count - 1; i++)
					RenderLine(i, i + 1);
		}

		public void DrawPoints()
		{
			Enumerable.Range(0, transformedPoints.Count).ToList().ForEach(x => RenderPoint(x));
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
			bitmap.DrawLineDDA((int)(line.from.X * width), (int)(line.from.Y * height), (int)(line.to.X * width), (int)(line.to.Y * height), color);

		}

		protected void DrawPoint(WriteableBitmap bitmap, Vector3 point, Color color)
		{
			bitmap.FillEllipse((int)(point.X * width - 3), (int)(point.Y * height - 3), (int)(point.X * width + 3), (int)(point.Y * height + 3), color);
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
