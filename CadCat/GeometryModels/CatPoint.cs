using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using CadCat.Math;
using CadCat.Rendering.Packets;

namespace CadCat.GeometryModels
{
	class CatPoint : Model
	{
		public override string GetName()
		{
			return "Point "+base.GetName();
		}

		public override IEnumerable<Line> GetLines()
		{
			return Enumerable.Empty<Line>();
		}

		public override IEnumerable<Vector3> GetPoints()
		{
			yield return new Vector3(0, 0, 0);
		}

		public override RenderingPacket GetRenderingPacket()
		{
			var packet =  base.GetRenderingPacket();
			packet.type = PacketType.PointPacket;
			return packet;
		}
	}
}
