using System;

namespace CadCat.GeometryModels
{
	using Math;
	using Real = Double;
	public class ParametrizedModel : Model
	{
		public DataStructures.SpatialData.Transform Transform;

		public ParametrizedModel()
		{
			Transform = new DataStructures.SpatialData.Transform();

		}
		public Real TrPosX
		{
			get
			{
				return Transform.Position.X;
			}
			set
			{
				Transform.Position.X = value;
				PositionChanged();
				OnPropertyChanged();
			}
		}
		public Real TrPosY
		{
			get
			{
				return Transform.Position.Y;
			}
			set
			{
				Transform.Position.Y = value;
				PositionChanged();
				OnPropertyChanged();
			}
		}
		public Real TrPosZ
		{
			get
			{
				return Transform.Position.Z;
			}
			set
			{
				Transform.Position.Z = value;
				PositionChanged();
				OnPropertyChanged();
			}
		}

		public Real TrRotX
		{
			get
			{
				return Utils.RadToDeg(Transform.Rotation.X);
			}
			set
			{
				Real deg = value;
				if (deg > 360)
					deg -= 360.0;
				if (deg < 0)
					deg += 360.0;
				Transform.Rotation.X = Utils.DegToRad(deg);
				OnPropertyChanged();
			}
		}
		public Real TrRotY
		{
			get
			{
				return Utils.RadToDeg(Transform.Rotation.Y);
			}
			set
			{
				Real deg = value;
				if (deg > 360)
					deg -= 360.0;
				if (deg < 0)
					deg += 360.0;
				Transform.Rotation.Y = Utils.DegToRad(deg);
				OnPropertyChanged();
			}
		}
		public Real TrRotZ
		{
			get
			{
				return Utils.RadToDeg(Transform.Rotation.Z);
			}
			set
			{
				Real deg = value;
				if (deg > 360)
					deg -= 360.0;
				if (deg < 0)
					deg += 360.0;
				Transform.Rotation.Z = Utils.DegToRad(deg);
				OnPropertyChanged();
			}
		}

		public Real TrScaleX
		{
			get
			{
				return Transform.Scale.X;
			}
			set
			{
				Transform.Scale.X = value;
				OnPropertyChanged();
			}
		}
		public Real TrScaleY
		{
			get
			{
				return Transform.Scale.Y;
			}
			set
			{
				Transform.Scale.Y = value;
				OnPropertyChanged();
			}
		}
		public Real TrScaleZ
		{
			get
			{
				return Transform.Scale.Z;
			}
			set
			{
				Transform.Scale.Z = value;
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
			PositionChanged();
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

		public override Matrix4 GetMatrix(bool overrideScale, Vector3 newScale)
		{
			return Transform.CreateTransformMatrix(overrideScale, newScale);
		}

		protected virtual void PositionChanged()
		{

		}


	}
}
