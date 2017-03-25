using CadCat.DataStructures;
using CadCat.Math;
using CadCat.Rendering.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadCat.GeometryModels
{
	public class PointGroup : Model
	{
		List<Vector3> points = new List<Vector3>();
		public PointGroup()
		{
			for(int i=0;i<100;i++)
				for(int j=0;j<100;j++)
				{
					points.Add(new Vector3(i * 0.05, System.Math.Sin(i * 0.127 + j*0.04), j * 0.05));
				}
		}
		public override string GetName()
		{
			return "Point " + base.GetName();
		}

		public override IEnumerable<Line> GetLines()
		{
			return Enumerable.Empty<Line>();
		}

		public override IEnumerable<Vector3> GetPoints()
		{
			return points;
		}

		public override RenderingPacket GetRenderingPacket()
		{
			var packet = base.GetRenderingPacket();
			packet.type = PacketType.PointPacket;
			return packet;
		}
	}
}
