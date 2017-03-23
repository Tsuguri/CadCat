using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CadCat.Utilities
{
	public class CommandHandler : ICommand
	{
		private Action _action;
		private bool _canExecute;
		public CommandHandler(Action action, bool canExecute = true)
		{
			_action = action;
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			return _canExecute;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			_action();
		}
	}

	class RelayCommand : ICommand
	{
		private Action _action;
		private Func<bool> _func;

		public RelayCommand(Action action, Func<bool> func)
		{
			_action = action;
			_func = func;
		}

		public void RaiseCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, new EventArgs());
		}

		#region ICommand Members

		public bool CanExecute(object parameter)
		{
			if (_func != null)
				return _func();
			return true;
		}



		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			_action();
		}

		#endregion
	}
}
