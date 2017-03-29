using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CadCat.UIControls
{

	/// <summary>
	/// Interaction logic for PointList.xaml
	/// </summary>
	public partial class PointList : UserControl
	{
		public IEnumerable<CadCat.DataStructures.CatPoint> Points
		{
			get
			{
				foreach (var item in list.SelectedItems)
				{
					yield return (DataStructures.CatPoint)item;
				}
			}
		}

		public PointList()
		{
			InitializeComponent();
		}
	}
}
