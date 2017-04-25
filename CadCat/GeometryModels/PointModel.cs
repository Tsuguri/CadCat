using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CadCat.DataStructures;

namespace CadCat.GeometryModels
{
	class PointModel : Model
	{
		public class PointWrapper : Utilities.BindableObject
		{
			private CadCat.DataStructures.CatPoint point;

			public DataStructures.CatPoint Point
			{
				get { return point; }
				set
				{
					point = value;
				}
			}

			internal PointWrapper(DataStructures.CatPoint point)
			{
				this.point = point;
			}

			public string Name => point.Name;

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

			internal void InvalidateName()
			{
				OnPropertyChanged(nameof(Name));
			}

		}



		protected ObservableCollection<PointWrapper> points = new ObservableCollection<PointWrapper>();

		public ObservableCollection<PointWrapper> Points => points;

		public override IEnumerable<CatPoint> EnumerateCatPoints()
		{
			return Points.Select(x => x.Point);
		}
	}
}
