using SportsManager.Commands;
using SportsManager.Models;
using SportsManager.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SportsManager.ViewModels
{
    /// <summary>
    /// Viewmodel for group stage schedule.
    /// </summary>
    class GroupsScheduleViewModel : NotifyPropertyChanged
    {
        /// <summary>
        /// Representation of the match overview as a stat.
        /// </summary>
        public class MatchStats : IStats
        {
            public string Overview { get; set; }
            public string HomeScore { get; set; }
            public string AwayScore { get; set; }
            public string Datetime { get; set; }
        }

        /// <summary>
        /// Current instance of the NavigationStore.
        /// </summary>
        private readonly NavigationStore ns;

        private readonly object roundsLock = new();

        private ObservableCollection<Round> rounds;
        /// <summary>
        /// Collection of all rounds of the group stage of the current season.
        /// </summary>
        public ObservableCollection<Round> Rounds
        {
            get => rounds;
            set
            {
                rounds = value;
                OnPropertyChanged();
            }
        }

        private ICommand deleteRoundCommand;
        /// <summary>
        /// When executed, it deletes the selected round.
        /// </summary>
        public ICommand DeleteRoundCommand
        {
            get
            {
                if (deleteRoundCommand == null)
                {
                    deleteRoundCommand = new RelayCommand(param => DeleteRound((Round)param));
                }
                return deleteRoundCommand;
            }
        }

        private ICommand matchDetailCommand;
        /// <summary>
        /// Command that navigates to the selected match detail viewmodel after execution.
        /// </summary>
        public ICommand MatchDetailCommand
        {
            get
            {
                if (matchDetailCommand == null)
                {
                    matchDetailCommand = new RelayCommand(param => MatchDetail((Match)param));
                }
                return matchDetailCommand;
            }
        }

        private ICommand addMatchCommand;
        /// <summary>
        /// Command that navigates to viewmodel for adding a new match to the selected serie after execution.
        /// </summary>
        public ICommand AddMatchCommand
        {
            get
            {
                if (addMatchCommand == null)
                {
                    addMatchCommand = new RelayCommand(param => AddMatch(((Round)param).ID));
                }
                return addMatchCommand;
            }
        }

        private ICommand addRoundCommand;
        /// <summary>
        /// Command that creates a new round after execution.
        /// </summary>
        public ICommand AddRoundCommand
        {
            get
            {
                if (addRoundCommand == null)
                {
                    addRoundCommand = new RelayCommand(param => AddRound());
                }
                return addRoundCommand;
            }
        }

        /// <summary>
        /// True if the editing of the rounds is allowed, otherwise false.
        /// </summary>
        public bool IsEnabled { get; private set; } = true;

        /// <summary>
        /// Instantiates the new GroupsScheduleViewModel.
        /// </summary>
        /// <param name="navigationStore">Current instance of the NavigationStore.</param>
        public GroupsScheduleViewModel(NavigationStore navigationStore)
        {
            ns = navigationStore;
            if (SportsData.SEASON.PlayOffStarted || SportsData.SEASON.WinnerID != SportsData.NOID) { IsEnabled = false; }
            LoadRounds();
        }

        /// <summary>
        /// Loads all the rounds of the current season from the database.
        /// </summary>
        private void LoadRounds()
        {
            Rounds = new ObservableCollection<Round>();

            lock (roundsLock)
            {
                MySqlConnection connection = new(SportsData.ConnectionStringSport);
                MySqlCommand cmd = new("SELECT id, season_id, name FROM rounds WHERE season_id = " + SportsData.SEASON.ID, connection);

                try
                {
                    connection.Open();
                    DataTable dataTable = new();
                    dataTable.Load(cmd.ExecuteReader());

                    foreach (DataRow row in dataTable.Rows)
                    {
                        Round r = new()
                        {
                            ID = int.Parse(row["id"].ToString()),
                            Name = row["name"].ToString(),
                            SeasonID = int.Parse(row["season_id"].ToString()),
                            Matches = new ObservableCollection<Match>()
                        };

                        cmd = new MySqlCommand("SELECT h.name AS home_name, a.name AS away_name, " +
                                                "matches.id, played, datetime, home_score, away_score, overtime, shootout, forfeit " +
                                                "FROM matches", connection);
                        if (SportsData.SPORT.Name == "tennis")
                        {
                            cmd.CommandText += " INNER JOIN player AS h ON h.id = matches.home_competitor";
                            cmd.CommandText += " INNER JOIN player AS a ON a.id = matches.away_competitor";
                        }
                        else
                        {
                            cmd.CommandText += " INNER JOIN team AS h ON h.id = matches.home_competitor";
                            cmd.CommandText += " INNER JOIN team AS a ON a.id = matches.away_competitor";
                        }
                        cmd.CommandText += " WHERE round = " + r.ID + " AND serie_match_number < 1";

                        DataTable dt = new();
                        dt.Load(cmd.ExecuteReader());

                        //load matches
                        foreach (DataRow dtRow in dt.Rows)
                        {
                            Team home = new();
                            home.Name = dtRow["home_name"].ToString();
                            Team away = new();
                            away.Name = dtRow["away_name"].ToString();

                            Match m = new()
                            {
                                ID = int.Parse(dtRow["id"].ToString()),
                                Datetime = DateTime.Parse(dtRow["datetime"].ToString()),
                                Played = Convert.ToBoolean(int.Parse(dtRow["played"].ToString())),
                                HomeTeam = home,
                                AwayTeam = away,
                                HomeScore = int.Parse(dtRow["home_score"].ToString()),
                                AwayScore = int.Parse(dtRow["away_score"].ToString()),
                                Overtime = Convert.ToBoolean(int.Parse(dtRow["overtime"].ToString())),
                                Shootout = Convert.ToBoolean(int.Parse(dtRow["shootout"].ToString())),
                                Forfeit = Convert.ToBoolean(int.Parse(dtRow["forfeit"].ToString()))
                            };

                            MatchStats mStats = new()
                            {
                                Overview = m.Overview(),
                                Datetime = m.Datetime.ToString("g"),
                                HomeScore = m.HomeTeam.Name + "   " + m.HomeScore,
                                AwayScore = m.AwayScore + m.ResultType() + "   " + m.AwayTeam.Name
                            };
                            if (!m.Played)
                            {
                                mStats.HomeScore = m.HomeTeam.Name + "   -";
                                mStats.AwayScore = "-   " + m.AwayTeam.Name;
                            }
                            m.Stats = mStats;

                            r.Matches.Add(m);
                        }
                        Rounds.Add(r);
                    }
                }
                catch (Exception)
                {
                    _ = MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Deletes round and all its matches from the database.
        /// </summary>
        /// <param name="r">Instance of the round to delete.</param>
        private void DeleteRound(Round r)
        {
            MessageBoxResult msgResult = MessageBox.Show("Do you really want to delete " + r.Name + "?.", "Delete round", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (msgResult == MessageBoxResult.Yes)
            {
                //delete round from DB
                MySqlConnection connection = new(SportsData.ConnectionStringSport);
                MySqlTransaction transaction = null;
                string roundDeletionQuerry = "DELETE FROM rounds WHERE id = " + r.ID;

                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    MySqlCommand cmd = new(roundDeletionQuerry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();

                    //get all match ids of round r
                    string querry = "SELECT id FROM matches WHERE serie_match_number < 1 AND round = " + r.ID;
                    cmd = new MySqlCommand(querry, connection)
                    {
                        Transaction = transaction
                    };
                    DataTable dataTable = new();
                    dataTable.Load(cmd.ExecuteReader());

                    StringBuilder sb = new();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        _ = sb.Append(row["id"].ToString() + ",");
                    }
                    if (sb.Length > 0) { _ = sb.Remove(sb.Length - 1, 1); }

                    if (sb.Length > 0)
                    {
                        //delete matches from DB
                        querry = "DELETE FROM matches WHERE bracket_index < 1 AND round = " + r.ID;
                        cmd = new MySqlCommand(querry, connection)
                        {
                            Transaction = transaction
                        };
                        _ = cmd.ExecuteNonQuery();

                        //delete all player/goalie match enlistments and all stats from all matches
                        List<string> databases = new() { "player_matches", "goalie_matches", "penalties", "goals", "penalty_shots", "shutouts", "shifts", "shootout_shots", "time_outs", "period_score", "game_state" };
                        foreach (string db in databases)
                        {
                            querry = "DELETE FROM " + db + " WHERE match_id IN (" + sb + ")";
                            cmd = new MySqlCommand(querry, connection)
                            {
                                Transaction = transaction
                            };
                            _ = cmd.ExecuteNonQuery();
                        }
                    }

                    //rename all rounds after round r
                    int roundNumber = int.Parse(r.Name.Split(' ')[1]);
                    for (int i = Rounds.IndexOf(r) + 1; i < Rounds.Count; i++)
                    {
                        querry = "UPDATE rounds SET name = 'Round " + roundNumber  + "' WHERE id = " + Rounds[i].ID;
                        cmd = new MySqlCommand(querry, connection)
                        {
                            Transaction = transaction
                        };
                        _ = cmd.ExecuteNonQuery();

                        Rounds[i].Name = "Round " + roundNumber;
                        roundNumber++;
                    }

                    _ = Rounds.Remove(r);

                    transaction.Commit();
                    connection.Close();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    _ = MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Navigates to a viewmodel of match detail.
        /// </summary>
        /// <param name="m">Instance of the selected match.</param>
        private void MatchDetail(Match m)
        {
            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new MatchViewModel(ns, m, new GroupsScheduleViewModel(ns)))).Execute(null);
        }

        /// <summary>
        /// Navigates to a viewmodel for adding a new match.
        /// </summary>
        /// <param name="roundID">Identification number of the round in which the match will be added.</param>
        private void AddMatch(int roundID)
        {
            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new AddMatchViewModel(ns, roundID))).Execute(null);
        }

        /// <summary>
        /// Creates new round and saves it to the database.
        /// </summary>
        private void AddRound()
        {
            Round r = new();
            r.SeasonID = SportsData.SEASON.ID;
            r.Name = "Round " + (Rounds.Count + 1);
            r.Matches = new ObservableCollection<Match>();

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("INSERT INTO rounds(season_id, name) VALUES ('" + r.SeasonID + "', '" + r.Name + "')", connection);

            try
            {
                connection.Open();
                _ = cmd.ExecuteNonQuery();
                r.ID = (int)cmd.LastInsertedId;

                Rounds.Add(r);
            }
            catch (Exception)
            {
                _ = MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}