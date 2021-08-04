using CSharpZapoctak.Commands;
using CSharpZapoctak.Stores;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class NavigationBarViewModel : ViewModelBase
    {
        public Dictionary<string, bool> AreButtonsChecked { get; set; } = new Dictionary<string, bool>() {
            { "CompetitionsSelection", false },
            { "Competition", false },
            { "SeasonsSelection", false },
            { "Season", false },
            { "Standings", false },
            { "Matches", false },
            { "Teams", false },
            { "Players", false },
            { "Goalies", false }
        };

        public Dictionary<string, Visibility> ButtonsVisibilities { get; set; } = new Dictionary<string, Visibility>() {
            { "Competition", Visibility.Visible },
            { "Season", Visibility.Visible },
            { "Standings", Visibility.Visible },
            { "Teams", Visibility.Visible },
            { "Goalies", Visibility.Visible }
        };

        public string CurrentCompetition { get; set; }
        public string CurrentSeason { get; set; }

        public ICommand NavigateSportsCommand { get; }
        public ICommand NavigateCompetitionsCommand { get; }
        public ICommand NavigateCompetitionDetailCommand { get; }
        public ICommand NavigateSeasonsCommand { get; }
        public ICommand NavigateSeasonDetailCommand { get; }
        public ICommand NavigateStandingsCommand { get; }
        public ICommand NavigateScheduleCommand { get; }
        public ICommand NavigateMatchesCommand { get; }
        public ICommand NavigateTeamsCommand { get; }
        public ICommand NavigatePlayersCommand { get; }
        public ICommand NavigateGoaliesCommand { get; }

        public NavigationBarViewModel(NavigationStore navigationStore, string buttonName, Dictionary<string, Visibility> buttonsVisibilities)
        {
            /*if (SportsData.sport.name == "tennis")
            {
                //tennis player view = navigate command
            }*/
            NavigateSportsCommand = new NavigateCommand<SportsSelectionViewModel>(navigationStore, () => new SportsSelectionViewModel(navigationStore));
            NavigateCompetitionsCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new CompetitionsSelectionViewModel(navigationStore)));
            NavigateCompetitionDetailCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new CompetitionViewModel(navigationStore)));
            NavigateSeasonsCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new SeasonsSelectionViewModel(navigationStore)));
            NavigateSeasonDetailCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new SeasonViewModel(navigationStore)));
            //NavigateStandingsCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore));
            NavigateScheduleCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new ScheduleViewModel(navigationStore)));
            NavigateMatchesCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new MatchesSelectionViewModel(navigationStore)));
            NavigateTeamsCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new TeamsSelectionViewModel(navigationStore)));
            //NavigatePlayersCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore));
            //NavigateGoaliesCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore));

            AreButtonsChecked[buttonName] = true;
            ButtonsVisibilities = buttonsVisibilities;

            if (SportsData.competition.Name.Length > 15)
            {
                CurrentCompetition = "- " + SportsData.competition.Name.Substring(0, 12) + "...";
            }
            else
            {
                CurrentCompetition = "- " + SportsData.competition.Name;
            }

            if (SportsData.season.Name.Length > 13)
            {
                CurrentSeason = "- " + SportsData.season.Name.Substring(0, 12) + "...";
            }
            else
            {
                CurrentSeason = "- " + SportsData.season.Name;
            }
        }
    }
}
