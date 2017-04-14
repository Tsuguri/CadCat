using System.Collections.Generic;
using CadCat.DataStructures;
using System.Windows.Media.Imaging;
using CadCat.Math;
using System.Windows.Media;

namespace CadCat.Rendering
{
	class StereoscopicRender : BaseRenderer
	{
		private double eyeDistance = 0.1;
		public double EyeDistance
		{
			get
			{
				return eyeDistance;
			}
			set
			{
				eyeDistance = value;
				OnPropertyChanged();
			}
		}

		private double depthMultiplier = 5;
		public double DepthMultiplier
		{
			get
			{
				return depthMultiplier;
			}
			set
			{
				depthMultiplier = value;
				OnPropertyChanged();
			}
		}

		protected WriteableBitmap RightBitmap;
		private readonly Color leftEye = Color.FromRgb(0, 240, 255);
		private readonly Color rightEye = Colors.Red;
		protected List<Vector3> TransformedRightPoints = new List<Vector3>();
		BitmapContext leftContext;
		BitmapContext rightContext;
		SceneData data;


		public override void Resize(double newWidth, double newHeight)
		{
			base.Resize(newWidth, newHeight);
			RightBitmap = new WriteableBitmap((int)newWidth, (int)newHeight, 96, 96, PixelFormats.Pbgra32, null);
		}

		public override void Transform()
		{
			var activeMatrix = data.ActiveCamera.GetLeftEyeMatrix(EyeDistance / 2.0, DepthMultiplier) * modelMatrix;
			transformedPoints = TransformVertexBuffer(activeMatrix);
			activeMatrix = data.ActiveCamera.GetRightEyeMatrix(EyeDistance / 2.0, DepthMultiplier) * modelMatrix;
			TransformedRightPoints = TransformVertexBuffer(activeMatrix);
		}
		protected override void RenderLine(int index1, int index2)
		{
			SelectedColor = leftEye;
			base.RenderLine(index1, index2);
			tmpLine.from = TransformedRightPoints[index1];
			tmpLine.to = TransformedRightPoints[index2];
			if (Clip(tmpLine, 0.99, 0.01))
				DrawLine(RightBitmap, tmpLine, rightEye);
		}

		protected override void RenderPoint(int index)
		{
			SelectedColor = leftEye;
			base.RenderPoint(index);
			if (ClipPoint(TransformedRightPoints[index]))
				DrawPoint(RightBitmap, TransformedRightPoints[index], rightEye);

		}

		public override void BeforeRendering(SceneData scene)
		{
			base.BeforeRendering(scene);
			rightContext = RightBitmap.GetBitmapContext();
			RightBitmap.Clear(Colors.Black);
			leftContext = bufferBitmap.GetBitmapContext();
			bufferBitmap.Clear(Colors.Black);
			data = scene;
		}
		public override void AfterRendering(SceneData scene)
		{
			bufferBitmap.Blit(new System.Windows.Rect(0, 0, bufferBitmap.Width, bufferBitmap.Height), RightBitmap, new System.Windows.Rect(0, 0, RightBitmap.Width, RightBitmap.Height), WriteableBitmapExtensions.BlendMode.Additive);
			rightContext.Dispose();
			leftContext.Dispose();
		}
	}
}
