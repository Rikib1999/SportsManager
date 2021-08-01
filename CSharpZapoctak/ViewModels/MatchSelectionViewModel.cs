using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class MatchSelectionViewModel : ViewModelBase
    {
        //season    partOfSeason    date    time    home    score   away    goals   assists     penalties
        public class MatchStats : IStats
        {
            public string PartOfSeason { get; set; }
            public DateTime Date { get; set; }
            public DateTime Time { get; set; }
            public string Score { get; set; }
            public int Goals { get; set; }
            public int Assists { get; set; }
            public int Penalties { get; set; }
        }

        public ICommand NavigateMatchCommand { get; set; }

        private ICommand checkNavigateMatchCommand;
        public ICommand CheckNavigateMatchCommand
        {
            get
            {
                if (checkNavigateMatchCommand == null)
                {
                    checkNavigateMatchCommand = new RelayCommand(param => CheckNavigateMatch());
                }
                return checkNavigateMatchCommand;
            }
        }

        public Match SelectedMatch { get; set; }

        public ObservableCollection<Match> Matches { get; set; }

        public MatchSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateMatchCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new MatchViewModel(navigationStore)));
            SelectedMatch = null;

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT c.name AS winner, seasons.id, competition_id, seasons.name, seasons.info, qualification_count, qualification_rounds, group_count, play_off_rounds, play_off_best_of FROM matches", connection);
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

                Matches = new ObservableCollection<Match>();

                //season    partOfSeason    date    time    home    score   away    goals   assists     penalties
                //season    partOfSeason    date    time    home    score   away    goals   assists     yellow cards   red cards
                //season    partOfSeason    date    time    home    score   away    #sets   service%    breaks  ...
                foreach (DataRow row in dataTable.Rows)
                {
                    Match m = new Match
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

                    MatchStats mStats = new MatchStats
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
                    m.Stats = mStats;

                    Matches.Add(m);
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

        private void CheckNavigateMatch()
        {
            if (SelectedMatch != null)
            {
                NavigateMatchCommand.Execute(SelectedMatch);
            }
        }
    }
}