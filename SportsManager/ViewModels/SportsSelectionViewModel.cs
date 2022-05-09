using SportsManager.Commands;
using SportsManager.Stores;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SportsManager.ViewModels
{
    /// <summary>
    /// Viewmodel for the sports selection.
    /// </summary>
    public class SportsSelectionViewModel : NotifyPropertyChanged
    {
        /// <summary>
        /// Collection of all available sports.
        /// </summary>
        public ObservableCollection<Sport> Sports { get; } = new ObservableCollection<Sport>(SportsData.SportsList);

        public ICommand NavigateSportCommand { get; }

        /// <summary>
        /// Instantiates new SportsSelectionViewModel.
        /// </summary>
        /// <param name="navigationStore">Current instance of the NavigationStore.</param>
        public SportsSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateSportCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new CompetitionsSelectionViewModel(navigationStore)));
        }
    }
}