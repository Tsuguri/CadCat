using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadCat.Rendering.Packets
{
	public enum PacketType
	{
		LinePacket,
		PointPacket
	}
	public struct RenderingPacket
	{
		public GeometryModels.Model model;
		public PacketType type;
	}
}
