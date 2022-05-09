using SportsManager.Stores;
using System.Collections.Generic;
using System.Windows;

namespace SportsManager.ViewModels
{
    /// <summary>
    /// Viewmodel for the currently selected sport. Contains navigation bar and currently selected viewmodel.
    /// </summary>
    public class SportViewModel : NotifyPropertyChanged
    {
        /// <summary>
        /// Currently selected viewmodel.
        /// </summary>
        public NotifyPropertyChanged CurrentViewModel { get; set; }

        /// <summary>
        /// Navigation bar viewmodel.
        /// </summary>
        public NotifyPropertyChanged NavBarViewModel { get; set; }

        /// <summary>
        /// Current header of the view.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Updates header with the currenlty selected sport, competition and season names.
        /// </summary>
        public void UpdateHeader()
        {
            Header = SportsData.SPORT.FormattedName;

            if (SportsData.IsCompetitionSet())
            {
                Header += " - " + SportsData.COMPETITION.Name;
            }
            if (SportsData.IsSeasonSet())
            {
                Header += " - " + SportsData.SEASON.Name;
            }
        }

        /// <summary>
        /// Instantiates new SportViewModel.
        /// </summary>
        /// <param name="navigationStore">Current instance of NavigationStore.</param>
        /// <param name="newViewModel">Instance of the new subviewmodel.</param>
        public SportViewModel(NavigationStore navigationStore, NotifyPropertyChanged newViewModel)
        {
            UpdateHeader();

            CurrentViewModel = newViewModel;

            string checkedButton = "";
            switch (CurrentViewModel)
            {
                case CompetitionsSelectionViewModel:
                    checkedButton = "CompetitionsSelection";
                    break;
                case CompetitionViewModel:
                    checkedButton = "Competition";
                    break;
                case SeasonsSelectionViewModel:
                    checkedButton = "SeasonsSelection";
                    break;
                case SeasonViewModel:
                    checkedButton = "Season";
                    break;
                case StandingsViewModel:
                    checkedButton = "Standings";
                    break;
                case ScheduleViewModel:
                    checkedButton = "Schedule";
                    break;
                case MatchesSelectionViewModel:
                    checkedButton = "Matches";
                    break;
                case TeamsSelectionViewModel:
                    checkedButton = "Teams";
                    break;
                case PlayersSelectionViewModel:
                    checkedButton = "Players";
                    break;
                case GoaliesSelectionViewModel:
                    checkedButton = "Goalies";
                    break;
                default:
                    break;
            }

            Dictionary<string, Visibility> buttonsVisibilities = new() {
                { "Competition", Visibility.Visible },
                { "Season", Visibility.Visible },
                { "Standings", Visibility.Visible },
                { "Schedule", Visibility.Visible },
                { "Teams", Visibility.Visible },
                { "Goalies", Visibility.Visible }
            };

            if (!SportsData.IsCompetitionSet())
            {
                buttonsVisibilities["Competition"] = Visibility.Collapsed;
            }
            if (!SportsData.IsSeasonSet())
            {
                buttonsVisibilities["Season"] = Visibility.Collapsed;
                buttonsVisibilities["Standings"] = Visibility.Collapsed;
                buttonsVisibilities["Schedule"] = Visibility.Collapsed;
            }

            NavBarViewModel = new NavigationBarViewModel(navigationStore, checkedButton, buttonsVisibilities);
        }
    }
}
