using CSharpZapoctak.Stores;
using System.Collections.Generic;
using System.Windows;

namespace CSharpZapoctak.ViewModels
{
    public class SportViewModel : ViewModelBase
    {
        public ViewModelBase CurrentViewModel { get; set; }

        public ViewModelBase NavBarViewModel { get; set; }

        public string Header { get; set; }

        public void UpdateHeader()
        {
            string cName = SportsData.competition.Name;
            string sName = SportsData.season.Name;
            if (cName != "" && SportsData.competition.id != (int)EntityState.AddNew && SportsData.competition.id != (int)EntityState.NotSelected)
            {
                cName = " - " + cName;
            }
            else
            {
                cName = "";
            }
            if (sName != "")
            {
                sName = " - " + sName;
            }
            Header = char.ToUpper(SportsData.sport.name[0]) + SportsData.sport.name.Substring(1) + cName + sName;
        }

        public SportViewModel(NavigationStore navigationStore, ViewModelBase newViewModel)
        {
            UpdateHeader();

            if (SportsData.competition.id == (int)EntityState.AddNew)
            {
                CurrentViewModel = new AddEditCompetitionViewModel(navigationStore);
                SportsData.competition.id = (int)EntityState.NotSelected;
            }
            else
            {
                CurrentViewModel = newViewModel;
            }

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

            Dictionary<string, Visibility> buttonsVisibilities = new Dictionary<string, Visibility>() {
                { "Competition", Visibility.Visible },
                { "Season", Visibility.Visible },
                { "Standings", Visibility.Visible },
                { "Schedule", Visibility.Visible },
                { "Teams", Visibility.Visible },
                { "Goalies", Visibility.Visible }
            };

            if (SportsData.competition.Name == "" || SportsData.competition.id == (int)EntityState.NotSelected || SportsData.competition.id == (int)EntityState.AddNew)
            {
                buttonsVisibilities["Competition"] = Visibility.Collapsed;
            }
            if (SportsData.season.Name == "" || SportsData.season.id == (int)EntityState.NotSelected || SportsData.season.id == (int)EntityState.AddNew)
            {
                buttonsVisibilities["Season"] = Visibility.Collapsed;
                buttonsVisibilities["Standings"] = Visibility.Collapsed;
                buttonsVisibilities["Schedule"] = Visibility.Collapsed;
            }
            if (SportsData.sport.name == "tennis")
            {
                buttonsVisibilities["Teams"] = Visibility.Collapsed;
                buttonsVisibilities["Goalies"] = Visibility.Collapsed;
            }

            NavBarViewModel = new NavigationBarViewModel(navigationStore, checkedButton, buttonsVisibilities);
        }
    }
}
