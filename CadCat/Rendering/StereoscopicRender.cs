using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		public WriteableBitmap rightBitmap;
		private Color leftEye = Color.FromRgb(0, 240, 255);
		private Color rightEye = Colors.Red;
		protected List<Vector3> transformedRightPoints = new List<Vector3>();
		BitmapContext leftContext;
		BitmapContext rightContext;
		SceneData data;


		public override void Resize(double width, double height)
		{
			base.Resize(width, height);
			rightBitmap = new WriteableBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32, null);
		}

		public override void Transform()
		{
			var activeMatrix = data.ActiveCamera.GetLeftEyeMatrix(EyeDistance / 2.0, DepthMultiplier) * modelMatrix;
			transformedPoints = TransformVertexBuffer(activeMatrix);
			activeMatrix = data.ActiveCamera.GetRightEyeMatrix(EyeDistance / 2.0, DepthMultiplier) * modelMatrix;
			transformedRightPoints = TransformVertexBuffer(activeMatrix);
		}
		protected override void RenderLine(int index1, int index2)
		{
			SelectedColor = leftEye;
			base.RenderLine(index1, index2);
			tmpLine.from = transformedRightPoints[index1];
			tmpLine.to = transformedRightPoints[index2];
			if (Clip(tmpLine, 0.99, 0.01))
				DrawLine(rightBitmap, tmpLine, rightEye, LineStroke);
		}

		protected override void RenderPoint(int index)
		{
			SelectedColor = leftEye;
			base.RenderPoint(index);
			if (ClipPoint(transformedRightPoints[index]))
				DrawPoint(rightBitmap, transformedRightPoints[index], rightEye);

		}

		public override void BeforeRendering(SceneData scene)
		{
			base.BeforeRendering(scene);
			rightContext = rightBitmap.GetBitmapContext();
			rightBitmap.Clear(Colors.Black);
			leftContext = bufferBitmap.GetBitmapContext();
			bufferBitmap.Clear(Colors.Black);
			this.data = scene;
		}
		public override void AfterRendering(SceneData scene)
		{
			bufferBitmap.Blit(new System.Windows.Rect(0, 0, bufferBitmap.Width, bufferBitmap.Height), rightBitmap, new System.Windows.Rect(0, 0, rightBitmap.Width, rightBitmap.Height), WriteableBitmapExtensions.BlendMode.Additive);
			rightContext.Dispose();
			leftContext.Dispose();
		}
	}
}
