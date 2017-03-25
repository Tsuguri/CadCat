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
		private double eyeDistance=0.1;
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

		public override void Resize(double width, double height)
		{
			base.Resize(width, height);
			rightBitmap = new WriteableBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32, null);
		}
		public override void Render(SceneData scene)
		{
			base.Render(scene);

			var tmpLine = new Line();

			#region GetContextAndClear

			var rightContext = rightBitmap.GetBitmapContext();
			rightBitmap.Clear(Colors.Black);
			var leftContext = bufferBitmap.GetBitmapContext();
			bufferBitmap.Clear(Colors.Black);

			#endregion

			var cameraLeftMatrix = scene.ActiveCamera.GetLeftEyeMatrix(EyeDistance / 2.0,  DepthMultiplier);
			var cameraRightMatrix = scene.ActiveCamera.GetRightEyeMatrix(EyeDistance / 2.0,  DepthMultiplier);

			Matrix4 activeLeftMatrix = cameraLeftMatrix;
			Matrix4 activeRightMatrix = cameraRightMatrix;
			int stroke = 1;

			foreach (var packet in scene.GetPackets())
			{
				var modelmat = packet.model.GetMatrix(packet.overrideScale, packet.newScale);
				activeLeftMatrix = cameraLeftMatrix * modelmat;
				activeRightMatrix = cameraRightMatrix * modelmat;
				stroke = (scene.SelectedModel != null && scene.SelectedModel.ModelID == packet.model.ModelID) ? 2 : 1;

				switch (packet.type)
				{
					case Packets.PacketType.LinePacket:
						foreach (var line in packet.model.GetLines())
						{
							ProcessLine(bufferBitmap, line, activeLeftMatrix, leftEye, stroke);
							ProcessLine(rightBitmap, line, activeRightMatrix, rightEye, stroke);
						}
						break;
					case Packets.PacketType.PointPacket:
						foreach (var point in packet.model.GetPoints())
						{
							ProcessPoint(bufferBitmap, point, activeLeftMatrix, leftEye);
							ProcessPoint(rightBitmap, point, activeRightMatrix, rightEye);
						}
						break;
					default:
						break;
				}

			}




			#region BlitAndDispose
			bufferBitmap.Blit(new System.Windows.Rect(0, 0, bufferBitmap.Width, bufferBitmap.Height), rightBitmap, new System.Windows.Rect(0, 0, rightBitmap.Width, rightBitmap.Height), WriteableBitmapExtensions.BlendMode.Additive);
			rightContext.Dispose();
			leftContext.Dispose();
			#endregion
		}
	}
}
