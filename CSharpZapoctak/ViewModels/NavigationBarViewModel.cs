using CSharpZapoctak.Commands;
using CSharpZapoctak.Stores;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class NavigationBarViewModel : NotifyPropertyChanged
    {
        public Dictionary<string, bool> AreButtonsChecked { get; set; } = new Dictionary<string, bool>() {
            { "Sports", false },
            { "CompetitionsSelection", false },
            { "Competition", false },
            { "SeasonsSelection", false },
            { "Season", false },
            { "Standings", false },
            { "Schedule", false },
            { "Matches", false },
            { "Teams", false },
            { "Players", false },
            { "Goalies", false }
        };

        public Dictionary<string, Visibility> ButtonsVisibilities { get; set; } = new Dictionary<string, Visibility>() {
            { "Competition", Visibility.Visible },
            { "Season", Visibility.Visible },
            { "Standings", Visibility.Visible },
            { "Schedule", Visibility.Visible },
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

        public NavigationBarViewModel(NavigationStore ns, string buttonName, Dictionary<string, Visibility> buttonsVisibilities)
        {
            NavigateSportsCommand = new NavigateCommand<SportsSelectionViewModel>(ns, () => new SportsSelectionViewModel(ns));
            NavigateCompetitionsCommand = new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new CompetitionsSelectionViewModel(ns)));
            NavigateCompetitionDetailCommand = new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new CompetitionViewModel(ns)));
            NavigateSeasonsCommand = new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new SeasonsSelectionViewModel(ns)));
            NavigateSeasonDetailCommand = new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new SeasonViewModel(ns)));
            NavigateStandingsCommand = new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new StandingsViewModel(ns)));
            NavigateScheduleCommand = new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new ScheduleViewModel(ns)));
            NavigateMatchesCommand = new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new MatchesSelectionViewModel(ns)));
            NavigateTeamsCommand = new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new TeamsSelectionViewModel(ns)));
            NavigatePlayersCommand = new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new PlayersSelectionViewModel(ns)));
            NavigateGoaliesCommand = new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new GoaliesSelectionViewModel(ns)));

            AreButtonsChecked[buttonName] = true;
            ButtonsVisibilities = buttonsVisibilities;

            if (SportsData.COMPETITION.Name.Length > 15)
            {
                CurrentCompetition = "- " + SportsData.COMPETITION.Name.Substring(0, 12) + "...";
            }
            else
            {
                CurrentCompetition = "- " + SportsData.COMPETITION.Name;
            }

            if (SportsData.SEASON.Name.Length > 13)
            {
                CurrentSeason = "- " + SportsData.SEASON.Name.Substring(0, 12) + "...";
            }
            else
            {
                CurrentSeason = "- " + SportsData.SEASON.Name;
            }
        }
    }
}
