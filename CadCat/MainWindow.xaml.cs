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
using CadCat.Rendering;
using CadCat.DataStructures;

namespace CadCat
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		RenderingContext ctx = null;
		ModelsData data;
		DispatcherTimer timer;
		DispatcherTimer resizeTimer;
		Size imageSize;

		public MainWindow()
		{
			InitializeComponent();
			//image.Loaded += Image_Initialized;
			data = new ModelsData();
			ctx = new RenderingContext(data, image);
			var cam = new Camera();
			cam.transform.Position = new Math.Vector3(0.2,0.2,-2);
			cam.transform.Rotation = new Math.Vector3(0, 0, 0);
			ctx.ActiveCamera = cam;
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
				ctx.ActiveCamera.AspectRatio = e.NewSize.Width / e.NewSize.Height;
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
				ctx.UpdatePoints();
			};
			timer.Interval = new TimeSpan(0, 0, 0,0,33);
			timer.Start();
		}

		private void AddTorus_Click(object sender, RoutedEventArgs e)
		{
			var torus = new GeometryModels.Torus();
			Random rand = new Random();
			var pos = torus.transform.Position;
			pos.X = rand.Next(200);
			pos.Y = rand.Next(200);
			torus.transform.Position = pos;
			data.AddModel(torus);
		}

		private void AddCube_Click(object sender, RoutedEventArgs e)
		{
			var cube = new GeometryModels.Cube();
			Random rand = new Random();
			var pos = cube.transform.Position;
			pos.X = 0;
			pos.Y = 0;
			cube.transform.Position = pos;
			data.AddModel(cube);
		}
	}
}
