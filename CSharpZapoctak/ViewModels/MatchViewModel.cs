using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class MatchViewModel : ViewModelBase
    {
        public Competition CurrentSeason { get; set; }

        public ICommand NavigateEditCompetitionCommand { get; }

        public MatchViewModel(NavigationStore navigationStore, Match m)
        {
            NavigateEditCompetitionCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new AddEditCompetitionViewModel(navigationStore)));

            CurrentSeason = SportsData.season;
        }
    }
}