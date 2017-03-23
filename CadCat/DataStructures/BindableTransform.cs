using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadCat.DataStructures
{
	using Real = System.Double;

	public class BindableTransform : Utilities.BindableObject
	{
		public Transform transform;

		public Real TrPosX
		{
			get
			{
				return transform.Position.X;
			}
			set
			{
				transform.Position.X = value;
				OnPropertyChanged();
			}
		}
		public Real TrPosY
		{
			get
			{
				return transform.Position.Y;
			}
			set
			{
				transform.Position.Y = value;
				OnPropertyChanged();
			}
		}
		public Real TrPosZ
		{
			get
			{
				return transform.Position.Z;
			}
			set
			{
				transform.Position.Z = value;
				OnPropertyChanged();
			}
		}

		public Real TrRotX
		{
			get
			{
				return Math.Utils.RadToDeg(transform.Rotation.X);
			}
			set
			{
				Real deg = value;
				if (deg > 360)
					deg -= 360.0;
				if (deg < 0)
					deg += 360.0;
				transform.Rotation.X = Math.Utils.DegToRad(deg);
				OnPropertyChanged();
			}
		}
		public Real TrRotY
		{
			get
			{
				return Math.Utils.RadToDeg(transform.Rotation.Y);
			}
			set
			{
				Real deg = value;
				if (deg > 360)
					deg -= 360.0;
				if (deg < 0)
					deg += 360.0;
				transform.Rotation.Y = Math.Utils.DegToRad(deg);
				OnPropertyChanged();
			}
		}
		public Real TrRotZ
		{
			get
			{
				return Math.Utils.RadToDeg(transform.Rotation.Z);
			}
			set
			{
				Real deg = value;
				if (deg > 360)
					deg -= 360.0;
				if (deg < 0)
					deg += 360.0;
				transform.Rotation.Z = Math.Utils.DegToRad(deg);
				OnPropertyChanged();
			}
		}

		public Real TrScaleX
		{
			get
			{
				return transform.Scale.X;
			}
			set
			{
				transform.Scale.X = value;
				OnPropertyChanged();
			}
		}
		public Real TrScaleY
		{
			get
			{
				return transform.Scale.Y;
			}
			set
			{
				transform.Scale.Y = value;
				OnPropertyChanged();
			}
		}
		public Real TrScaleZ
		{
			get
			{
				return transform.Scale.Z;
			}
			set
			{
				transform.Scale.Z = value;
				OnPropertyChanged();
			}
		}

		public void InvalidateAll()
		{
			InvalidatePosition();
			InvalidateRotation();
			InvalidateScale();
		}

		public void InvalidatePosition()
		{
			OnPropertyChanged(nameof(TrPosX));
			OnPropertyChanged(nameof(TrPosY));
			OnPropertyChanged(nameof(TrPosZ));
		}

		public void InvalidateRotation()
		{
			OnPropertyChanged(nameof(TrRotX));
			OnPropertyChanged(nameof(TrRotY));
			OnPropertyChanged(nameof(TrRotZ));
		}

		public void InvalidateScale()
		{
			OnPropertyChanged(nameof(TrScaleX));
			OnPropertyChanged(nameof(TrScaleY));
			OnPropertyChanged(nameof(TrScaleZ));
		}
	}
}
