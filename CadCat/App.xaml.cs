using System.Diagnostics;
using System.Windows;

namespace CadCat
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{

		public App()
		{
			FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			PresentationTraceSources.Refresh();
			PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
			PresentationTraceSources.DataBindingSource.Listeners.Add(new DebugTraceListener());
			PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;
			base.OnStartup(e);
		}
	}

	public class DebugTraceListener : TraceListener
	{
		public override void Write(string message)
		{
		}

		public override void WriteLine(string message)
		{
			Debugger.Break();
		}
	}
}