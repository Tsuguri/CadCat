using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.Math;
using CadCat.DataStructures;

namespace CadCat.GeometryModels
{


	internal struct ModelLine
	{
		public int from;
		public int to;

		public ModelLine(int from, int to)
		{
			this.from = from;
			this.to = to;
		}
	}
	public class Model : Utilities.BindableObject
	{

		public int ModelID
		{
			get;
			private set;
		}
		private string name;
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
				OnPropertyChanged();
			}
		}
		private bool isSelected = false;
		public bool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				isSelected = value;
				OnPropertyChanged();
			}
		}

		public IEnumerable<string> Interfaces
		{
			get
			{
				return this.GetType().GetInterfaces().Select(x=> x.ToString());
			}
		}

		private static int idCounter = 0;

		public Model()
		{
			ModelID = idCounter;
			name = GetName();
			idCounter++;
		}
		public virtual Rendering.Packets.RenderingPacket GetRenderingPacket()
		{
			Rendering.Packets.RenderingPacket packet = new Rendering.Packets.RenderingPacket();
			packet.model = this;
			packet.type = Rendering.Packets.PacketType.LinePacket;
			packet.overrideScale = false;
			return packet;
		}

		public virtual bool Collide(Ray ray, out double distance)
		{
			distance = 0.0;
			return false;
		}

		public virtual IEnumerable<Line> GetLines()
		{
			var line = new Line();
			line.from = new Vector3(10, 10);
			line.to = new Vector3(100, 100);

			for (int i = 0; i < 32; i++)
				for (int j = 0; j < 32; j++)
				{
					line.from.X = i * 40;
					line.from.Y = j * 40 + 100;
					line.to.X = 50;
					line.to.Y = 50;
					yield return line;
				}

		}

		public virtual IEnumerable<Vector3> GetPoints()
		{
			yield return new Vector3(1, 1, 1);
		}

		public virtual IEnumerable<CatPoint> EnumerateCatPoints()
		{
			return Enumerable.Empty<CatPoint>();
		}

		public virtual string GetName()
		{
			return ModelID.ToString();
		}

		public virtual Matrix4 GetMatrix(bool overrideScale, Vector3 newScale)
		{
			return Matrix4.CreateIdentity();
		}
	}
}
