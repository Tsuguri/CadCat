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
				return System.Math.Min(x1, x2);
			}
			else
				return -1.0;
		}

		Vector4 ComputeNormal(Vector4 position, GeometryModels.Elipsoide ellipsoide)
		{
			return new Vector4( new Vector3(position.X * ellipsoide.A, position.Y * ellipsoide.B, position.Z * ellipsoide.C).Normalized());
		}

		private Vector3 lightColor = new Vector3(1.0, 1.0, 0.0);
		private Vector3 ellipseColor = new Vector3(1.0, 1.0, 0.0);
		private double lightIntensity = 2.0;
		private double specularStrength = 0.6;
		Vector3 ComputePhong(Vector3 normal,Vector3 lightDir)
		{
			double ambient = 0.1;
			Vector3 ambientColor = lightColor * ambient;
			double diff = System.Math.Max(normal.DotProduct(lightDir), 0.0);
			Vector3 diffuse = lightColor * diff;

			double spec = System.Math.Pow(System.Math.Max(lightDir.DotProduct(normal), 0.0f), lightIntensity);
			Vector3 specular = lightColor * spec * specularStrength;
			return (/*ambientColor + diffuse + */specular) * ellipseColor;

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

			lightIntensity = ellipsoide.LightIntensity;

			Matrix4 diagonal = Matrix4.CreateIdentity();
			diagonal[0, 0] = ellipsoide.A;
			diagonal[1, 1] = ellipsoide.B;
			diagonal[2, 2] = ellipsoide.C;
			diagonal[3, 3] = -1.0;

			Matrix4 cameraMatrix = Scene.ActiveCamera.ViewProjectionMatrix;
			Matrix4 modelMatrix = ellipsoide.transform.CreateTransformMatrix();
			Matrix4 mat = cameraMatrix * modelMatrix; // zmienić na coś mądrzejszego
			mat = mat.Inversed();
			Matrix4 invmat = mat.GetTransposed();
			Matrix4 tmp = diagonal * mat;
			Matrix4 em =invmat * tmp;

			Matrix4 inversedCamera = Scene.ActiveCamera.ViewProjectionMatrix.Inversed();
			Matrix4 inversedModel = ellipsoide.transform.CreateTransformMatrix().Inversed();

			Matrix4 transposedInverseViewModel = (inversedModel * inversedCamera).GetTransposed();
			var tmpLine = new Line();
			double ratio = bufferBitmap.Width / bufferBitmap.Height;
			using (bufferBitmap.GetBitmapContext())
			{
				bufferBitmap.Clear(Colors.DarkBlue);
				for (int i = 0; i < bufferBitmap.Width; i++)
					for (int j = 0; j < bufferBitmap.Height; j++)
					{
						double x = (2*(i / bufferBitmap.Width) - 1.0)*ratio;
						double y = 2*(j / bufferBitmap.Height) - 1.0;


						double A = em[2, 2];
						double B = em[2, 0] * x + em[2, 1] * y + em[2, 3] + em[0, 2] * x + em[1, 2] * y + em[3, 2];
						double C = em[0, 0] * x * x + em[0, 1] * x * y + em[0, 3] * x +
							em[1, 0] * x * y + em[1, 1] * y * y + em[1, 3] * y +
							em[3, 0] * x + em[3, 1] * y + em[3, 3];

						double result = SolveEquation(A, B, C);
						if (result <= 0)
							continue;

						Vector4 worldPosition = inversedCamera * new Vector4(x, y, result);
						Vector4 modelPosition = inversedModel * worldPosition;
						Vector4 modelSpaceNormal = ComputeNormal(modelPosition, ellipsoide);
						Vector4 viewNormal = transposedInverseViewModel * modelSpaceNormal;
						Vector3 normal = new Vector3(viewNormal.X, viewNormal.Y, viewNormal.Z).Normalized();

						Vector3 color = ComputePhong(normal, (new Vector3(-x,-y,-result)).Normalized());

						color.X = System.Math.Min(color.X, 1.0);
						color.Y = System.Math.Min(color.Y, 1.0);
						color.Z = System.Math.Min(color.Z, 1.0);
						bufferBitmap.SetPixel(i, j, (byte)(255 * (color.X)),(byte)(255 * (color.Y)), (byte)(255 * (color.Z)));
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
