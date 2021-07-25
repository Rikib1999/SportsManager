using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class CompetitionViewModel : ViewModelBase
    {
        public Competition CurrentCompetition { get; set; }

        public ICommand NavigateAddEditCompetitionCommand { get; }

        public CompetitionViewModel(NavigationStore navigationStore)
        {
            NavigateAddEditCompetitionCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new AddEditCompetitionViewModel(navigationStore)));

            CurrentCompetition = SportsData.competition;
        }
    }
}