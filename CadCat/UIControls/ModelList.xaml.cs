using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using CadCat.GeometryModels;

namespace CadCat.UIControls
{
	/// <summary>
	/// Interaction logic for ModelList.xaml
	/// </summary>
	public partial class ModelList : UserControl
	{

		public IEnumerable<Model> Models => models.SelectedItems.Cast<Model>();

		public ModelList()
		{
			InitializeComponent();
		}
	}
}
