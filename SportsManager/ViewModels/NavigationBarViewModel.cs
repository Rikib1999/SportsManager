using SportsManager.Commands;
using SportsManager.Stores;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace SportsManager.ViewModels
{
    /// <summary>
    /// Viewmodel for the navigation bar.
    /// </summary>
    public class NavigationBarViewModel : NotifyPropertyChanged
    {
        /// <summary>
        /// Dictionary containing each button name as the key and boolen as value, which tells if the button is highlighted, what means the section is currently selected.
        /// </summary>
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

        /// <summary>
        /// Dictionary containing button names as keys and their visibilities as values.
        /// </summary>
        public Dictionary<string, Visibility> ButtonsVisibilities { get; set; } = new Dictionary<string, Visibility>() {
            { "Competition", Visibility.Visible },
            { "Season", Visibility.Visible },
            { "Standings", Visibility.Visible },
            { "Schedule", Visibility.Visible },
            { "Teams", Visibility.Visible },
            { "Goalies", Visibility.Visible }
        };

        /// <summary>
        /// Currently selected competition.
        /// </summary>
        public string CurrentCompetition { get; set; }
        /// <summary>
        /// Currently selected season.
        /// </summary>
        public string CurrentSeason { get; set; }

        /// <summary>
        /// Command that navigates sports selection viewmodel after execution.
        /// </summary>
        public ICommand NavigateSportsCommand { get; }
        /// <summary>
        /// Command that navigates competition selection viewmodel after execution.
        /// </summary>
        public ICommand NavigateCompetitionsCommand { get; }
        /// <summary>
        /// Command that navigates competition detail viewmodel after execution.
        /// </summary>
        public ICommand NavigateCompetitionDetailCommand { get; }
        /// <summary>
        /// Command that navigates seasons selection viewmodel after execution.
        /// </summary>
        public ICommand NavigateSeasonsCommand { get; }
        /// <summary>
        /// Command that navigates season detail viewmodel after execution.
        /// </summary>
        public ICommand NavigateSeasonDetailCommand { get; }
        /// <summary>
        /// Command that navigates standings viewmodel of the current season after execution.
        /// </summary>
        public ICommand NavigateStandingsCommand { get; }
        /// <summary>
        /// Command that navigates schedule viewmodel of the current season after execution.
        /// </summary>
        public ICommand NavigateScheduleCommand { get; }
        /// <summary>
        /// Command that navigates matches selection viewmodel after execution.
        /// </summary>
        public ICommand NavigateMatchesCommand { get; }
        /// <summary>
        /// Command that navigates teams selection viewmodel after execution.
        /// </summary>
        public ICommand NavigateTeamsCommand { get; }
        /// <summary>
        /// Command that navigates players selection viewmodel after execution.
        /// </summary>
        public ICommand NavigatePlayersCommand { get; }
        /// <summary>
        /// Command that navigates goaltenders selection viewmodel after execution.
        /// </summary>
        public ICommand NavigateGoaliesCommand { get; }

        /// <summary>
        /// Instantiates new navigation bar viewmodel.
        /// </summary>
        /// <param name="ns">Current instance of NavigationStore.</param>
        /// <param name="buttonName">Name of selected button in navigation bar.</param>
        /// <param name="buttonsVisibilities">Dictionary of visibilities of navigation bar buttons.</param>
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

            CurrentCompetition = SportsData.COMPETITION.Name.Length > 15
                ? "- " + SportsData.COMPETITION.Name.Substring(0, 12) + "..."
                : "- " + SportsData.COMPETITION.Name;

            CurrentSeason = SportsData.SEASON.Name.Length > 13
                ? "- " + SportsData.SEASON.Name.Substring(0, 12) + "..."
                : "- " + SportsData.SEASON.Name;
        }
    }
}
