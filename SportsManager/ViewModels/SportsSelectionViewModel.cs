using SportsManager.Commands;
using SportsManager.Stores;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SportsManager.ViewModels
{
    public class SportsSelectionViewModel : NotifyPropertyChanged
    {
        public ObservableCollection<Sport> Sports { get; } = new ObservableCollection<Sport>(SportsData.SportsList);

        public ICommand NavigateSportCommand { get; }

        public SportsSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateSportCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new CompetitionsSelectionViewModel(navigationStore)));
        }
    }
}