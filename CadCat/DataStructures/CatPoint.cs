using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using CadCat.Math;
using CadCat.Rendering.Packets;

namespace CadCat.DataStructures
{
	public class CatPoint : Utilities.BindableObject
	{
		private static int id = 0;

		Vector3 position;
		string name;
		bool isSelected = false;

		public CatPoint(Vector3 position)
		{
			this.Name = "Point " + id;
			id++;
			this.position = position;
		}

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

		public Vector3 Position
		{
			get
			{
				return position;
			}
		}

		public double X
		{
			get
			{
				return position.X;
			}
			set
			{
				position.X = value;
				OnPropertyChanged();
			}
		}

		public double Y
		{
			get
			{
				return position.Y;
			}
			set
			{
				position.Y = value;
				OnPropertyChanged();
			}
		}

		public double Z
		{
			get
			{
				return position.Z;
			}
			set
			{
				position.Z = value;
				OnPropertyChanged();
			}
		}

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

	}
}
