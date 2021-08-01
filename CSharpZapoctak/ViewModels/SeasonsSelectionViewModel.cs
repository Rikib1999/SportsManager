using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class SeasonsSelectionViewModel : ViewModelBase
    {
        public class SeasonStats : IStats
        {
            public string Format { get; set; }
            public int Matches { get; set; }
            public int Teams { get; set; }
            public int Players { get; set; }
            public int Goals { get; set; }
            public float GoalsPerGame { get; set; }
            public int Assists { get; set; }
            public float AssistsPerGame { get; set; }
            public int Penalties { get; set; }
            public float PenaltiesPerGame { get; set; }
        }

        public ICommand NavigateAddSeasonCommand { get; set; }

        public ICommand NavigateSeasonCommand { get; set; }

        private ICommand checkNavigateSeasonCommand;
        public ICommand CheckNavigateSeasonCommand
        {
            get
            {
                if (checkNavigateSeasonCommand == null)
                {
                    checkNavigateSeasonCommand = new RelayCommand(param => CheckNavigateSeason());
                }
                return checkNavigateSeasonCommand;
            }
        }

        public Season SelectedSeason { get; set; }

        public ObservableCollection<Season> Seasons { get; set; }

        public SeasonsSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateAddSeasonCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new AddSeasonViewModel(navigationStore)));
            NavigateSeasonCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new SeasonViewModel(navigationStore)));
            SelectedSeason = null;

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT c.name AS winner, seasons.id, competition_id, seasons.name, seasons.info, qualification_count, qualification_rounds, group_count, play_off_rounds, play_off_best_of FROM seasons", connection);
            if (SportsData.sport.name == "tennis")
            {
                cmd.CommandText += " INNER JOIN player AS c ON c.id = seasons.winner_id";
            }
            else
            {
                cmd.CommandText += " INNER JOIN team AS c ON c.id = seasons.winner_id";
            }
            if (SportsData.competition.Name != "" && SportsData.competition.id != (int)EntityState.NotSelected && SportsData.competition.id != (int)EntityState.AddNew)
            {
                cmd.CommandText += " WHERE competition_id = " + SportsData.competition.id;
            }

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                Seasons = new ObservableCollection<Season>();

                Season all = new Season
                {
                    id = (int)EntityState.NotSelected,
                    Name = "All ()",
                };
                SeasonStats allStats = new SeasonStats();

                //name  format  winner  #matches    #teams      #players    goals   goals/g     assists     assists/g   penalties   penalties/g
                //name  format  winner  #matches    #teams      #players    goals   goals/g     assists     assists/g   yellow cards   red cards
                //name  format  winner  #matches    #players    #sets       service%            breaks      ...
                foreach (DataRow ssn in dataTable.Rows)
                {
                    Season s = new Season
                    {
                        id = int.Parse(ssn["id"].ToString()),
                        competitionID = int.Parse(ssn["competition_id"].ToString()),
                        Name = ssn["name"].ToString(),
                        Info = ssn["info"].ToString(),
                        QualificationCount = int.Parse(ssn["qualification_count"].ToString()),
                        QualificationRounds = int.Parse(ssn["qualification_rounds"].ToString()),
                        GroupCount = int.Parse(ssn["group_count"].ToString()),
                        PlayOffRounds = int.Parse(ssn["play_off_rounds"].ToString()),
                        PlayOffBestOf = int.Parse(ssn["play_off_best_of"].ToString()),
                        Winner = ssn["winner"].ToString()
                    };
                    string[] imgPath = System.IO.Directory.GetFiles(SportsData.SeasonLogosPath, SportsData.sport.name + ssn["id"].ToString() + ".*");
                    if (imgPath.Length != 0)
                    {
                        s.LogoPath = imgPath.First();
                    }
                    
                    SeasonStats sStats = new SeasonStats
                    {
                        Format = s.Format(),
                        /*matches = 1,
                        teams;
                        players;
                        goals;
                        goalsPerGame;
                        assists;
                        assistsPerGame;
                        penalties;
                        penaltiesPerGame;*/
                    };
                    s.Stats = sStats;
                    //TO DO: All stats sum

                    Seasons.Add(s);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        private void CheckNavigateSeason()
        {
            if (SelectedSeason != null)
            {
                NavigateSeasonCommand.Execute(SelectedSeason);
            }
        }
    }
}