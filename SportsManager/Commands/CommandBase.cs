using System;
using System.Windows.Input;

namespace SportsManager.Commands
{
    /// <summary>
    /// Basic ICommand interface implementation.
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Executes commands action.
        /// </summary>
        /// <param name="parameter"></param>
        public abstract void Execute(object parameter);

        protected void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}
