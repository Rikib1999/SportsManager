using System;
using System.Windows.Input;

namespace SportsManager.Commands
{
    /// <summary>
    /// Used for executing methods after some button click or other input action.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// Creates new RelayCommand instance.
        /// </summary>
        /// <param name="execute">Method to execute later.</param>
        public RelayCommand(Action<object> execute) : this(execute, null) { }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Executes the saved method with provided parameter.
        /// </summary>
        /// <param name="parameter">Parameter for method to execute with.</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}