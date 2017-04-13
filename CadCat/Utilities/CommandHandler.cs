using System;
using System.Windows.Input;

namespace CadCat.Utilities
{
	public class CommandHandler : ICommand
	{
		private readonly Action action;
		private readonly bool canExecute;
		public CommandHandler(Action action, bool canExecute = true)
		{
			this.action = action;
			this.canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			return canExecute;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			action();
		}
	}

	class RelayCommand : ICommand
	{
		private readonly Action action;
		private readonly Func<bool> func;

		public RelayCommand(Action action, Func<bool> func)
		{
			this.action = action;
			this.func = func;
		}

		public void RaiseCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, new EventArgs());
		}

		#region ICommand Members

		public bool CanExecute(object parameter)
		{
			if (func != null)
				return func();
			return true;
		}



		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			action();
		}

		#endregion
	}
}
