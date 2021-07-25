using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class SeasonViewModel : ViewModelBase
    {
        public Competition CurrentSeason { get; set; }

        public ICommand NavigateEditCompetitionCommand { get; }

        public SeasonViewModel(NavigationStore navigationStore)
        {
            NavigateEditCompetitionCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new AddEditCompetitionViewModel(navigationStore)));

            CurrentSeason = SportsData.season;
        }
    }
}