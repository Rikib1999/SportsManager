using CSharpZapoctak.Stores;
using CSharpZapoctak.ViewModels;
using System;

namespace CSharpZapoctak.Commands
{
    public class NavigateCommand<TViewModel> : CommandBase where TViewModel : NotifyPropertyChanged
    {
        private readonly NavigationStore _navigationStore;
        private readonly Func<TViewModel> _createViewModel;

        public NavigateCommand(NavigationStore navigationStore, Func<TViewModel> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }

        /// <summary>
        /// Navigate to viewmodel.
        /// </summary>
        /// <param name="parameter">Sets new sport/competition/season.</param>
        public override void Execute(object parameter)
        {
            //sets current competition/season
            SportsData.Set(parameter);
            if (_navigationStore.CurrentViewModel is AddMatchViewModel model)
            {
                model.gamesheetLoadingThread.Interrupt();
                model.gamesheetLoadingThread.Join();
            }
            _navigationStore.CurrentViewModel = _createViewModel();
        }
    }
}