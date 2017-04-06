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
		public delegate void DeletedHandler(CatPoint sender);
		public delegate void ChangedHandler(CatPoint sender);

		public event DeletedHandler OnDeleted;
		public event ChangedHandler OnChanged;

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
			set
			{
				X = value.X;
				Y = value.Y;
				Z = value.Z;
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
				var tmp = position.X;
				position.X = value;
				if (tmp != value)
					OnChanged?.Invoke(this);
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
				var tmp = position.Y;
				position.Y = value;
				if (tmp != value)
					OnChanged?.Invoke(this);
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
				var tmp = position.Z;
				position.Z = value;
				if (tmp != value)
					OnChanged?.Invoke(this);
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
		private bool visible = true;
		public bool Visible
		{
			get
			{
				return visible;
			}
			set
			{
				visible = value;
				OnPropertyChanged();
			}
		}

		public bool AddAble { get; set; }

		public void CleanUp()
		{
			OnDeleted?.Invoke(this);
		}

	}
}
