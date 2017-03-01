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
		private SceneData Scene;
		private WriteableBitmap bufferBitmap;
		private ModelData modelData = new ModelData();

		private Image targetImage;
		DrawingVisual visual = new DrawingVisual();
		public int Thickness
		{
			get;
			set;
		}
		public Color LineColor
		{
			get;
			set;
		}

		public RenderingContext(SceneData lineData, Image image)
		{
			targetImage = image;
			Scene = lineData;
			Thickness = 1;
			LineColor = Colors.Gold;
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
			var tmpLine = new Line();
			using (bufferBitmap.GetBitmapContext())
			{
				bufferBitmap.Clear(Colors.Black);

				var cameraMatrix = Scene.ActiveCamera.ViewProjectionMatrix;

				Matrix4 activeMatrix = cameraMatrix;
				var activeModel = -1;
				var width = bufferBitmap.PixelWidth;
				var height = bufferBitmap.PixelHeight;

				var view = Scene.ActiveCamera.CreateFrustum();
				var transRadius = Scene.ActiveCamera.CreateTransRadius();
				var rot = Scene.ActiveCamera.CreateAngleRotation();
				var trans = Scene.ActiveCamera.CreateTargetTranslation();
				foreach (var line in Scene.GetLines(modelData))
				{
					if (modelData.ModelID != activeModel)
					{
						activeModel = modelData.ModelID;
						//var modelmat = modelData.transform.CreateTransformMatrix();
						activeMatrix = cameraMatrix;// * modelmat;
					}
					var from = (activeMatrix * new Vector4(line.from, 1)).ToNormalizedVector3();
					from.X = from.X / 2 + 0.5;
					from.Y = from.Y / 2 + 0.5;
					from.Z = from.Z / 2 + 0.5;
					from.Y = 1 - from.Y;

					var to = (activeMatrix * new Vector4(line.to, 1)).ToNormalizedVector3();

					to.X = to.X / 2 + 0.5;
					to.Y = to.Y / 2 + 0.5;
					to.Z = to.Z / 2 + 0.5;
					to.Y = 1 - to.Y;

					tmpLine.from = from;
					tmpLine.to = to;
					if (Clip(tmpLine, 0.99, 0.01))
						bufferBitmap.DrawLineAa((int)(tmpLine.from.X * width), (int)(tmpLine.from.Y * height), (int)(tmpLine.to.X * width), (int)(tmpLine.to.Y * height), LineColor, Thickness);
				}
			}
		}

		private double CountClipParameter(double from, double to, double margin)
		{
			return System.Math.Abs(to - margin) / System.Math.Abs(to - from);
		}

		private bool ClipAxis(Line clipped, double fromAxis, double toAxis, double farMargin, double closeMargin)
		{
			if ((fromAxis > farMargin && toAxis > farMargin) || (fromAxis < closeMargin && toAxis < closeMargin))
				return false;
			if (toAxis > farMargin)
			{
				var l = CountClipParameter(fromAxis, toAxis, farMargin);//System.Math.Abs(clipped.to.X - farMargin) / System.Math.Abs(clipped.to.X - clipped.from.X);
				clipped.to = clipped.to * (1 - l) + clipped.from * l;
			}
			if (fromAxis > farMargin)
			{
				var l = CountClipParameter(toAxis, fromAxis, farMargin);// System.Math.Abs(clipped.from.X - farMargin) / System.Math.Abs(clipped.to.X - clipped.from.X);
				clipped.from = clipped.to * l + clipped.from * (1 - l);
			}

			if (toAxis < closeMargin)
			{
				var l = CountClipParameter(fromAxis, toAxis, closeMargin);//System.Math.Abs(clipped.to.X - farMargin) / System.Math.Abs(clipped.to.X - clipped.from.X);
				clipped.to = clipped.to * (1 - l) + clipped.from * l;
			}
			if (fromAxis < closeMargin)
			{
				var l = CountClipParameter(toAxis, fromAxis, closeMargin);// System.Math.Abs(clipped.from.X - farMargin) / System.Math.Abs(clipped.to.X - clipped.from.X);
				clipped.from = clipped.to * l + clipped.from * (1 - l);
			}
			return true;
		}

		private bool Clip(Line clipped, double farMargin = 1.0, double closeMargin = 0.0)
		{
			return clipped.from.Z > 0.0 && clipped.to.Z > 0.0 &&
				ClipAxis(clipped, clipped.from.X, clipped.to.X, farMargin, closeMargin) &&
				ClipAxis(clipped, clipped.from.Y, clipped.to.Y, farMargin, closeMargin);
		}

		public void Resize(double width, double height)
		{
			bufferBitmap = new WriteableBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32, null);
			targetImage.Source = bufferBitmap;
		}
	}
}
