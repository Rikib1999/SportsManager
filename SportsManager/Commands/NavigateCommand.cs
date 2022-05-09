using SportsManager.Stores;
using SportsManager.ViewModels;
using System;

namespace SportsManager.Commands
{
    /// <summary>
    /// Used for navigation between viewmodels.
    /// </summary>
    /// <typeparam name="TViewModel">Constructor for new viewmodel.</typeparam>
    public class NavigateCommand<TViewModel> : CommandBase where TViewModel : NotifyPropertyChanged
    {
        private readonly NavigationStore _navigationStore;
        private readonly Func<TViewModel> _createViewModel;

        /// <summary>
        /// When executed, it navigates to the newly created viewmodel.
        /// </summary>
        /// <param name="navigationStore">Current navigation store instance.</param>
        /// <param name="createViewModel">Constructor for new viewmodel to navigate.</param>
        public NavigateCommand(NavigationStore navigationStore, Func<TViewModel> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }

        /// <summary>
        /// Navigates to new viewmodel.
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