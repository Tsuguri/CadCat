using System.CodeDom;
using CadCat.Math;
using System.Diagnostics;

namespace CadCat.DataStructures
{
	public class CatPoint : Utilities.BindableObject
	{
		public delegate void DeletedHandler(CatPoint sender);
		public delegate void ChangedHandler(CatPoint sender);
		public delegate void ReplacedHandler(CatPoint sender, CatPoint replacement);

		public event DeletedHandler OnDeleted;
		public event ChangedHandler OnChanged;
		public event ReplacedHandler OnReplace;

		private static int id = 0;

		public static void ResetID()
		{
			id = 0;
		}
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

		public int SerializationId { get; set; }

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

		public bool Removeable => DependentUnremovable==0;

		public uint DependentUnremovable { get; set; } = 0;


		public void CleanUp()
		{
			if (!Removeable)
				throw new System.Exception("Tried to remove unremovable point");
			OnDeleted?.Invoke(this);
		}

		internal void Replace(CatPoint replacement)
		{
			if (this != replacement)
			{
				OnReplace?.Invoke(this, replacement);
			}
		}

	}
}
