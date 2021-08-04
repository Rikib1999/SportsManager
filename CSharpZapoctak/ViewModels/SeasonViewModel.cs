using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class SeasonViewModel : ViewModelBase
    {
        public Season CurrentSeason { get; set; }

        public ICommand NavigateEditSeasonCommand { get; }

        public SeasonViewModel(NavigationStore navigationStore)
        {
            NavigateEditSeasonCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new EditSeasonViewModel(navigationStore)));

            CurrentSeason = SportsData.season;
        }
    }
}