using SportsManager.Stores;
using System.Collections.Generic;
using System.Windows;

namespace SportsManager.ViewModels
{
    public class SportViewModel : NotifyPropertyChanged
    {
        public NotifyPropertyChanged CurrentViewModel { get; set; }

        public NotifyPropertyChanged NavBarViewModel { get; set; }

        public string Header { get; set; }

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
