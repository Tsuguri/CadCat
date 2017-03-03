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
			Scene = lineData;
			Thickness = 1;
			SelectedItemThickness = 3;
			LineColor = Colors.Gold;
		}

		private double SolveEquation(double A, double B, double C)
		{
			double delta = System.Math.Sqrt(B * B - 4 * A * C);
			if (delta >= 0)
			{
				double sqrt = System.Math.Sqrt(delta);
				double x1 = (-B - sqrt) / (2 * A);
				double x2 = (-B + sqrt) / (2 * A);
				return System.Math.Abs( System.Math.Min(x1, x2));
			}
			else
				return -1.0;
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

			int k = 0;
			GeometryModels.Elipsoide ellipsoide = null;
			while (ellipsoide == null && k < Scene.Models.Count)
			{
				ellipsoide = Scene.Models[k] as GeometryModels.Elipsoide;
				k++;
			}
			if (ellipsoide == null)
				return;

			Matrix4 diagonal = Matrix4.CreateIdentity();
			diagonal[0, 0] = ellipsoide.A;
			diagonal[1, 1] = ellipsoide.B;
			diagonal[2, 2] = ellipsoide.C;
			diagonal[3, 3] = -1.0;

			Matrix4 mat = Matrix4.CreateTranslation(0,0,-3) ;// Scene.ActiveCamera.ViewProjectionMatrix * ellipsoide.transform.CreateTransformMatrix(); // zmienić na coś mądrzejszego
			Matrix4 invmat = mat.Inversed().GetTransposed();

			Matrix4 em =invmat * diagonal * mat;


			var tmpLine = new Line();
			using (bufferBitmap.GetBitmapContext())
			{
				bufferBitmap.Clear(Colors.Black);
				for (int i = 0; i < bufferBitmap.Width; i++)
					for (int j = 0; j < bufferBitmap.Height; j++)
					{
						double x = 10*(i / bufferBitmap.Width - 0.5);
						double y = 10*(j / bufferBitmap.Height - 0.5);


						double A = em[2, 2];
						double B = em[2, 0] * x + em[2, 1] * y + em[2, 3] + em[0, 2] * x + em[1, 2] * y + em[3, 2];
						double C = em[0, 0] * x * x + em[0, 1] * x * y + em[0, 3] * x +
							em[1, 0] * x * y + em[1, 1] * y * y + em[1, 3] * y +
							em[3, 0] * x + em[3, 1] * y + em[3, 3];

						double result = SolveEquation(A, B, C);
						if (result <= 0)
							continue;
						bufferBitmap.SetPixel(i, j, (byte)(255 * (result)), 0, 0);
					}
				//var cameraMatrix = Scene.ActiveCamera.ViewProjectionMatrix;

				//Matrix4 activeMatrix = cameraMatrix;
				//var activeModel = -1;
				//var width = bufferBitmap.PixelWidth;
				//var height = bufferBitmap.PixelHeight;
				//int stroke = Thickness;

				//var view = Scene.ActiveCamera.CreateFrustum();
				//var transRadius = Scene.ActiveCamera.CreateTransRadius();
				//var rot = Scene.ActiveCamera.CreateAngleRotation();
				//var trans = Scene.ActiveCamera.CreateTargetTranslation();
				//foreach (var line in Scene.GetLines(modelData))
				//{
				//	if (modelData.ModelID != activeModel)
				//	{
				//		activeModel = modelData.ModelID;
				//		var modelmat = modelData.transform.CreateTransformMatrix();
				//		activeMatrix = cameraMatrix * modelmat;
				//		stroke = (Scene.SelectedModel != null && Scene.SelectedModel.ModelID == modelData.ModelID) ? SelectedItemThickness : Thickness;
				//	}
				//	var from = (activeMatrix * new Vector4(line.from, 1)).ToNormalizedVector3();
				//	from.X = from.X / 2 + 0.5;
				//	from.Y = from.Y / 2 + 0.5;
				//	from.Z = from.Z / 2 + 0.5;
				//	from.Y = 1 - from.Y;

				//	var to = (activeMatrix * new Vector4(line.to, 1)).ToNormalizedVector3();

				//	to.X = to.X / 2 + 0.5;
				//	to.Y = to.Y / 2 + 0.5;
				//	to.Z = to.Z / 2 + 0.5;
				//	to.Y = 1 - to.Y;

				//	tmpLine.from = from;
				//	tmpLine.to = to;
				//	if (Clip(tmpLine, 0.99, 0.01))
				//		bufferBitmap.DrawLineAa((int)(tmpLine.from.X * width), (int)(tmpLine.from.Y * height), (int)(tmpLine.to.X * width), (int)(tmpLine.to.Y * height), LineColor, stroke);
				//}
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
