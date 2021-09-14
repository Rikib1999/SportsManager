using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class MatchesSelectionViewModel : ViewModelBase
    {
        //season    partOfSeason    date    time    home    score   away    goals   assists     penalties
        public class MatchStats : IStats
        {
            public string PartOfSeason { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
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

        public MatchesSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateMatchCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new MatchViewModel(navigationStore, SelectedMatch, new MatchesSelectionViewModel(navigationStore))));
            SelectedMatch = null;

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT h.name AS home_name, a.name AS away_name, s.competition_id AS competition_id, s.name AS season_name, c.name AS competition_name, " +
                                                "matches.id, datetime, home_score, away_score, overtime, shootout, forfeit, qualification_id, serie_match_number FROM matches " +
                                                "INNER JOIN seasons AS s ON s.id = matches.season_id " +
                                                "INNER JOIN competitions AS c ON c.id = competition_id", connection);
            if (SportsData.sport.name == "tennis")
            {
                cmd.CommandText += " INNER JOIN player AS h ON h.id = matches.home_competitor";
                cmd.CommandText += " INNER JOIN player AS a ON a.id = matches.away_competitor";
            }
            else
            {
                cmd.CommandText += " INNER JOIN team AS h ON h.id = matches.home_competitor";
                cmd.CommandText += " INNER JOIN team AS a ON a.id = matches.away_competitor";
            }
            cmd.CommandText += " WHERE matches.played = 1";
            if (SportsData.competition.Name != "" && SportsData.competition.id != (int)EntityState.NotSelected && SportsData.competition.id != (int)EntityState.AddNew)
            {
                cmd.CommandText += " AND competition_id = " + SportsData.competition.id;
                if (SportsData.season.Name != "" && SportsData.season.id != (int)EntityState.NotSelected && SportsData.season.id != (int)EntityState.AddNew)
                {
                    cmd.CommandText += " AND season_id = " + SportsData.season.id;
                }
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
                    Competition c = new Competition();
                    c.Name = row["competition_name"].ToString();
                    Season s = new Season();
                    s.Name = row["season_name"].ToString();
                    Team home = new Team();
                    home.Name = row["home_name"].ToString();
                    Team away = new Team();
                    away.Name = row["away_name"].ToString();

                    Match m = new Match
                    {
                        id = int.Parse(row["id"].ToString()),
                        Competition = c,
                        Season = s,
                        Datetime = DateTime.Parse(row["datetime"].ToString()),
                        HomeTeam = home,
                        AwayTeam = away,
                        HomeScore = int.Parse(row["home_score"].ToString()),
                        AwayScore = int.Parse(row["away_score"].ToString()),
                        Overtime = Convert.ToBoolean(int.Parse(row["overtime"].ToString())),
                        Shootout = Convert.ToBoolean(int.Parse(row["shootout"].ToString())),
                        Forfeit = Convert.ToBoolean(int.Parse(row["forfeit"].ToString()))
                    };

                    string partOfSeason = "Play-off";
                    if (int.Parse(row["serie_match_number"].ToString()) < 1)
                    {
                        partOfSeason = "Group";
                    }
                    else if (int.Parse(row["qualification_id"].ToString()) > 0)
                    {
                        partOfSeason = "Qualification";
                    }

                    MatchStats mStats = new MatchStats
                    {
                        PartOfSeason = partOfSeason,
                        Date = m.Datetime.ToLongDateString(),
                        Time = m.Datetime.ToShortTimeString(),
                        Score = m.Score()
                        /*goals
                         assists
                        penaties*/
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