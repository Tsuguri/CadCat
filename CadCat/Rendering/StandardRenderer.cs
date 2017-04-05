using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CadCat.DataStructures;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using CadCat.Math;

namespace CadCat.Rendering
{
	class StandardRenderer : BaseRenderer
	{
		private BitmapContext bufferContext;
		//public override void Render(SceneData scene)
		//{
		//	base.Render(scene);
		//	var tmpLine = new Line();
		//	using (bufferBitmap.GetBitmapContext())
		//	{


		//		foreach (var packet in scene.GetPackets())
		//		{
		//			activeMatrix = cameraMatrix * packet.model.GetMatrix(packet.overrideScale,packet.newScale);
		//			stroke = (scene.SelectedModel != null && scene.SelectedModel.ModelID == packet.model.ModelID) ? 2 : 1;

		//			switch (packet.type)
		//			{
		//				case Packets.PacketType.LinePacket:
		//					foreach (var line in packet.model.GetLines())
		//					{
		//						ProcessLine(bufferBitmap, line, activeMatrix, packet.model.IsSelected ? Colors.White : Colors.Yellow, stroke);
		//					}
		//					break;
		//				case Packets.PacketType.PointPacket:
		//					foreach (var point in packet.model.GetPoints())
		//					{
		//						ProcessPoint(bufferBitmap, point, activeMatrix, Colors.Yellow);
		//					}
		//					break;
		//				default:
		//					break;
		//			}

		//		}

		//		foreach (var point in scene.GetPoints())
		//		{
		//			ProcessPoint(bufferBitmap, point.Position, cameraMatrix, point.IsSelected ? Colors.Azure : Colors.Yellow);
		//		}

		//	}
		//}
		public override void AfterRendering(SceneData scene)
		{
			bufferContext.Dispose();
		}

		public override void BeforeRendering(SceneData scene)
		{
			base.BeforeRendering(scene);
			bufferBitmap.Clear(Colors.Black);
			CameraMatrix = scene.ActiveCamera.ViewProjectionMatrix;
			bufferContext = bufferBitmap.GetBitmapContext();
		}
	}
}
