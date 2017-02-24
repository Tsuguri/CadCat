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
using System.Windows.Threading;

namespace CadCat
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		RenderingContext ctx = null;
		LineDrawData data;
		DispatcherTimer timer;
		DispatcherTimer resizeTimer;
		Size imageSize;

		public MainWindow()
		{
			InitializeComponent();
			//image.Loaded += Image_Initialized;
			data = new LineDrawData();
			ctx = new RenderingContext(data, image);

			image.SizeChanged += Image_SizeChanged;
			resizeTimer = new DispatcherTimer();
			resizeTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
			RunTimer();
		}

		private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			imageSize = e.NewSize;
			resizeTimer.Stop();
			resizeTimer.Tick += (o, g) =>
			{
				Resize(e.NewSize.Width, e.NewSize.Height);
				resizeTimer.Stop();
			};
			resizeTimer.Start();
		}

		private void Image_Initialized(object sender, EventArgs e)
		{
			RunTimer();
		}

		private void Resize(double width, double height)
		{
				ctx.Resize(width, height);
		}
		private void RunTimer()
		{
			timer = new DispatcherTimer();
			timer.Tick += (o, e) =>
			{
				data.RandomizePoints();
				ctx.UpdatePoints(imageSize);
			};
			timer.Interval = new TimeSpan(0, 0, 0,0,33);
			timer.Start();
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
		}
	}
}
