using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Others;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class MatchesSelectionViewModel : ViewModelBase
    {
        public class MatchStats : ViewModelBase, IStats
        {
            #region Properties
            private string partOfSeason = "";
            public string PartOfSeason
            {
                get { return partOfSeason; }
                set
                {
                    partOfSeason = value;
                    OnPropertyChanged();
                }
            }

            private string date = "";
            public string Date
            {
                get { return date; }
                set
                {
                    date = value;
                    OnPropertyChanged();
                }
            }

            private string time = "";
            public string Time
            {
                get { return time; }
                set
                {
                    time = value;
                    OnPropertyChanged();
                }
            }

            private string score = "";
            public string Score
            {
                get { return score; }
                set
                {
                    score = value;
                    OnPropertyChanged();
                }
            }

            private int goals = 0;
            public int Goals
            {
                get { return goals; }
                set
                {
                    goals = value;
                    OnPropertyChanged();
                }
            }

            private int assists = 0;
            public int Assists
            {
                get { return assists; }
                set
                {
                    assists = value;
                    OnPropertyChanged();
                }
            }

            private int penalties = 0;
            public int Penalties
            {
                get { return penalties; }
                set
                {
                    penalties = value;
                    OnPropertyChanged();
                }
            }
            #endregion

            public MatchStats(Match m)
            {
                CalculateStats(m.id).Await();
            }

            public async Task CalculateStats(int matchID)
            {
                List<Task> tasks = new List<Task>();
                tasks.Add(Task.Run(() => CountGoals(matchID)));
                tasks.Add(Task.Run(() => CountAssists(matchID)));
                tasks.Add(Task.Run(() => CountPenalties(matchID)));
                await Task.WhenAll(tasks);
            }
            private async Task CountGoals(int matchID)
            {
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM goals " +
                                                    "WHERE match_id = " + matchID, connection);
                try
                {
                    connection.Open();
                    Goals = (int)(long)await cmd.ExecuteScalarAsync();
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

            private async Task CountAssists(int matchID)
            {
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM goals " +
                                                    "WHERE assist_player_id <> -1 AND match_id = " + matchID, connection);
                try
                {
                    connection.Open();
                    Assists = (int)(long)await cmd.ExecuteScalarAsync();
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

            private async Task CountPenalties(int matchID)
            {
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM penalties " +
                                                    "WHERE match_id = " + matchID, connection);
                try
                {
                    connection.Open();
                    Penalties = (int)(long)await cmd.ExecuteScalarAsync();
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
        }

        #region Commands
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

        private ICommand exportPDFCommand;
        public ICommand ExportPDFCommand
        {
            get
            {
                if (exportPDFCommand == null)
                {
                    exportPDFCommand = new RelayCommand(param => Exports.Export((System.Windows.Controls.DataGrid)param, "PDF"));
                }
                return exportPDFCommand;
            }
        }

        private ICommand exportXLSXCommand;
        public ICommand ExportXLSXCommand
        {
            get
            {
                if (exportXLSXCommand == null)
                {
                    exportXLSXCommand = new RelayCommand(param => Exports.Export((System.Windows.Controls.DataGrid)param, "XLSX"));
                }
                return exportXLSXCommand;
            }
        }
        #endregion

        #region Visibilities
        private bool showInfo = true;
        public bool ShowInfo
        {
            get { return showInfo; }
            set
            {
                showInfo = value;
                InfoVisibility = showInfo ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

        private bool showStats = true;
        public bool ShowStats
        {
            get { return showStats; }
            set
            {
                showStats = value;
                StatsVisibility = showStats ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

        private Visibility infoVisibility = Visibility.Visible;
        public Visibility InfoVisibility
        {
            get { return infoVisibility; }
            set
            {
                infoVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility statsVisibility = Visibility.Visible;
        public Visibility StatsVisibility
        {
            get { return statsVisibility; }
            set
            {
                statsVisibility = value;
                OnPropertyChanged();
            }
        }
        #endregion

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
            cmd.CommandText += " ORDER BY matches.datetime DESC";

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                Matches = new ObservableCollection<Match>();

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

                    m.Stats = new MatchStats(m);
                    ((MatchStats)m.Stats).PartOfSeason = partOfSeason;
                    ((MatchStats)m.Stats).Date = m.Datetime.ToLongDateString();
                    ((MatchStats)m.Stats).Time = m.Datetime.ToShortTimeString();
                    ((MatchStats)m.Stats).Score = m.Score();

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