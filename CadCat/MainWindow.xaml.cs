﻿using System;
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
using CadCat.GeometryModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace CadCat
{


	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	/// 
	public partial class MainWindow : Window, INotifyPropertyChanged
	{

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}


		RenderingContext ctx = null;
		SceneData data;
		DispatcherTimer timer;
		DispatcherTimer resizeTimer;
		Size imageSize;




		public MainWindow()
		{
			data = new SceneData();
			DataContext = data;
			InitializeComponent();
			ctx = new RenderingContext(data, image);
			var cam = new Camera();
			cam.LookingAt = new Math.Vector3(0, 0, 0);
			cam.Radius = 6;
			cam.HorizontalAngle = cam.VerticalAngle = 0;
			ctx.Thickness = 1;
			data.ActiveCamera = cam;
			image.SizeChanged += Image_SizeChanged;
			resizeTimer = new DispatcherTimer();
			resizeTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
			RunTimer();



			var mat = Math.Matrix4.CreateFrustum(Math.Utils.DegToRad(60), 1, 0.1, 100).Inversed();
			var p1 = (mat * new Math.Vector4(0, 0, 1, 1)).ToNormalizedVector3();
			var p2 = (mat * new Math.Vector4(1, 1, 1, 1)).ToNormalizedVector3().Normalized();
			var p3 = (mat * new Math.Vector4(-1, 0, 1, 1)).ToNormalizedVector3();
			

		}

		private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			imageSize = e.NewSize;
			resizeTimer.Stop();
			resizeTimer.Tick += (o, g) =>
			{
				Resize(e.NewSize.Width, e.NewSize.Height);
				data.ActiveCamera.AspectRatio = e.NewSize.Width / e.NewSize.Height;
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
			data.ScreenSize = new Math.Vector2(width, height);
		}
		private void RunTimer()
		{
			timer = new DispatcherTimer();

			timer.Tick += (o, e) =>
			{
				var point = Mouse.GetPosition(image);
				var pt2 = Mouse.GetPosition((FrameworkElement)image);
				if (point.X < 0 || point.Y < 0 || point.X > imageSize.Width || point.Y > imageSize.Height)
					point.X = point.Y = -1;
				else
					point.Y = imageSize.Height - point.Y;
				data.MousePosition = point;

				data.UpdateFrameData();
				ctx.UpdatePoints();
			};
			timer.Interval = new TimeSpan(0, 0, 0, 0, 33);
			timer.Start();
		}

		private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			data.ImageMouse.LeftMouseUpCommand.Execute(sender);
		}

		private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			data.ImageMouse.LeftMouseDownCommand.Execute(sender);
		}
	}
}
