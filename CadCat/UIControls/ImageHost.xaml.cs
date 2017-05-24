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
	/// Interaction logic for ImageHost.xaml
	/// </summary>
	public partial class ImageHost : UserControl
	{
		public ImageHost()
		{
			InitializeComponent();
		}

		public void SetImage(WriteableBitmap bmp)
		{
			image.Source = bmp;
			//image.InvalidateVisual();
		}

	}
}
