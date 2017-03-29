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
	/// Interaction logic for NumericSlider.xaml
	/// </summary>
	public partial class NumericSlider : UserControl
	{
		public NumericSlider()
		{
			this.BorderBrush = Brushes.LightGray;

			InitializeComponent();
			((FrameworkElement)Content).DataContext = this;
		}

		public double Value
		{
			get { return (double)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumericSlider),new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public double Multiplier
		{
			get { return (double)GetValue(MultiplierProperty); }
			set { SetValue(MultiplierProperty, value); }
		}

		public static readonly DependencyProperty MultiplierProperty = DependencyProperty.Register(nameof(Multiplier), typeof(double), typeof(UserControl), new PropertyMetadata(1.0));

		public double Increment
		{
			get { return (double)GetValue(IncrementProperty); }
			set { SetValue(IncrementProperty, value); }
		}

		public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(nameof(Increment), typeof(double), typeof(NumericSlider), new PropertyMetadata(1.0));

		public int Precision
		{
			get { return (int)GetValue(PrecisionProperty); }
			set { SetValue(PrecisionProperty, value); }
		}

		public static readonly DependencyProperty PrecisionProperty = DependencyProperty.Register(nameof(Precision), typeof(int), typeof(NumericSlider), new PropertyMetadata(10));

		public double ScrollWidth
		{
			get { return (double)GetValue(ScrollWidthProperty); }
			set { SetValue(ScrollWidthProperty, value); }
		}

		public static readonly DependencyProperty ScrollWidthProperty = DependencyProperty.Register(nameof(ScrollWidth), typeof(double), typeof(NumericSlider), new PropertyMetadata(5.0));




		private Point pressPoint;
		private bool inRange = false;
		private bool isMoving = false;
		private double startValue;

		private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!inRange)
				return;

			UIElement inputBox = (UIElement)sender;

			isMoving = true;
			inputBox.CaptureMouse();

			startValue = Value;
			pressPoint = e.GetPosition(inputBox);
		}

		private void inputBox_MouseEnter(object sender, MouseEventArgs e)
		{
			Cursor = Cursors.SizeWE;
			inRange = true;
		}

		private void inputBox_MouseLeave(object sender, MouseEventArgs e)
		{
			inRange = false;

			if (!isMoving)
				Cursor = Cursors.Arrow;
		}

		private void inputBox_MouseMove(object sender, MouseEventArgs e)
		{
			if (!isMoving)
				return;

			UIElement inputBox = (UIElement)sender;

			var diff = e.GetPosition(inputBox) - pressPoint;

			int diffValue = (int)(diff.X / (double)Precision);

			Value = startValue + diffValue * Multiplier;
		}

		private void inputBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (!isMoving)
				return;

			UIElement inputBox = (UIElement)sender;

			isMoving = false;
			inputBox.ReleaseMouseCapture();

			if (!inRange)
				Cursor = Cursors.Arrow;
		}

		private void UpClick(object sender, RoutedEventArgs e)
		{
			Value += Increment;
		}

		private void DownClick(object sender, RoutedEventArgs e)
		{
			Value -= Increment;
		}
	}
}
