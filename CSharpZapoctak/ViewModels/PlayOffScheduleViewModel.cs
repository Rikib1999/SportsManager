using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    public class TeamPoints : IStats
    {
        public int Points { get; set; }
        public TeamPoints(int points)
        {
            Points = points;
        }
    }

    public class PlayOffScheduleViewModel : TemplateBracketScheduleViewModel
    {
        #region Commands
        private ICommand startPlayOffCommand;
        public ICommand StartPlayOffCommand
        {
            get
            {
                if (startPlayOffCommand == null)
                {
                    startPlayOffCommand = new RelayCommand(param => StartPlayOff());
                }
                return startPlayOffCommand;
            }
        }

        private ICommand deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new RelayCommand(param => Delete());
                }
                return deleteCommand;
            }
        }
        #endregion

        private bool seedCompetitors;
        public bool SeedCompetitors
        {
            get => seedCompetitors;
            set
            {
                seedCompetitors = value;
                OnPropertyChanged();
            }
        }

        private Visibility notStartedVisibility = Visibility.Visible;
        public Visibility NotStartedVisibility
        {
            get => notStartedVisibility;
            set
            {
                notStartedVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility startedVisibility = Visibility.Collapsed;
        public Visibility StartedVisibility
        {
            get => startedVisibility;
            set
            {
                startedVisibility = value;
                OnPropertyChanged();
            }
        }

        public PlayOffScheduleViewModel(NavigationStore navigationStore)
        {
            Ns = navigationStore;
            Constructor = GetType().GetConstructor(new Type[] { typeof(NavigationStore) });

            if (SportsData.SEASON.WinnerID != SportsData.NOID) { IsEnabled = false; }

            if (SportsData.SEASON.PlayOffStarted)
            {
                NotStartedVisibility = Visibility.Collapsed;
                StartedVisibility = Visibility.Visible;
                LoadNotSelectedTeams();
                LoadBracket();
            }
        }

        private void LoadNotSelectedTeams()
        {
            NotSelectedTeams = new ObservableCollection<Team>();

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT team_id, t.name AS team_name FROM team_enlistment " +
                                                "INNER JOIN team AS t ON t.id = team_id " +
                                                "WHERE season_id = " + SportsData.SEASON.ID, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow tm in dataTable.Rows)
                {
                    Team t = new()
                    {
                        ID = int.Parse(tm["team_id"].ToString()),
                        Name = tm["team_name"].ToString(),
                    };

                    if (t.ID != SportsData.NOID)
                    {
                        NotSelectedTeams.Add(t);
                    }
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

        private void Delete()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Do you really want to delete whole play-off? All play-off matches will be deleted.", "Delete play-off", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (messageBoxResult == MessageBoxResult.No) { return; }

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlTransaction transaction = null;
            MySqlCommand cmd = null;
            string querry = "UPDATE seasons SET play_off_started = 0 WHERE id = " + SportsData.SEASON.ID;

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                //update season
                cmd = new(querry, connection)
                {
                    Transaction = transaction
                };
                _ = cmd.ExecuteNonQuery();

                //clear bracket
                cmd = new("DELETE FROM matches WHERE qualification_id = -1 AND bracket_index <> -1 AND season_id = " + SportsData.SEASON.ID, connection);
                DataTable dt = new();
                dt.Load(cmd.ExecuteReader());

                transaction.Commit();
                connection.Close();

                //refresh view
                SportsData.SEASON.PlayOffStarted = false;
                ScheduleViewModel vm = new(Ns);
                vm.CurrentViewModel = new PlayOffScheduleViewModel(Ns);
                vm.GroupsSet = false;
                vm.QualificationSet = false;
                vm.PlayOffSet = true;
                new NavigateCommand<SportViewModel>(Ns, () => new SportViewModel(Ns, vm)).Execute(null);
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

        private void StartPlayOff()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("After start of play-off modification of previous matches will not be possible unless play-off will be deleted with all of its matches.", "Play-off start", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (messageBoxResult == MessageBoxResult.Cancel) { return; }

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlTransaction transaction = null;
            MySqlCommand cmd = null;
            string querry = "UPDATE seasons SET play_off_started = 1 WHERE id = " + SportsData.SEASON.ID;

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                //update season
                cmd = new(querry, connection);
                cmd.Transaction = transaction;
                _ = cmd.ExecuteNonQuery();

                //seed competitors
                if (SeedCompetitors && (SportsData.SEASON.GroupCount > 0 || SportsData.SEASON.QualificationCount > 0))
                {
                    Dictionary<int, List<Team>> groups = new();

                    //clear bracket
                    cmd = new("DELETE FROM matches WHERE qualification_id = -1 AND bracket_index <> -1 AND season_id = " + SportsData.SEASON.ID, connection);
                    DataTable dt = new();
                    dt.Load(cmd.ExecuteReader());

                    //if season had qualification
                    if (SportsData.SEASON.QualificationCount > 0)
                    {
                        //select teams
                        querry = "SELECT home_score, away_score, home_competitor, away_competitor " +
                                 "FROM matches " +
                                 "WHERE season_id = " + SportsData.SEASON.ID + " AND played = 1 AND round = " + (SportsData.SEASON.QualificationRounds - 1);

                        cmd = new(querry, connection);
                        cmd.Transaction = transaction;
                        DataTable dataTable = new();
                        dataTable.Load(cmd.ExecuteReader());

                        List<Team> teams = new();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            int homeScore = int.Parse(row["home_score"].ToString());
                            int awayScore = int.Parse(row["away_score"].ToString());
                            Team t = new();
                            if (homeScore > awayScore)
                            {
                                t.ID = int.Parse(row["home_competitor"].ToString());
                            }
                            else if (homeScore < awayScore)
                            {
                                t.ID = int.Parse(row["away_competitor"].ToString());
                            }
                            else
                            {
                                continue;
                            }

                            teams.Add(t);
                        }
                        groups.Add(SportsData.NOID, teams);
                    }
                    //if season had groups
                    else if (SportsData.SEASON.GroupCount > 0)
                    {
                        //select teams
                        querry = "SELECT team_id, group_id " +
                                 "FROM team_enlistment " +
                                 "WHERE season_id = " + SportsData.SEASON.ID;

                        cmd = new(querry, connection);
                        cmd.Transaction = transaction;
                        DataTable dataTable = new();
                        dataTable.Load(cmd.ExecuteReader());

                        Dictionary<int, int> points = new();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            Team t = new();
                            t.ID = int.Parse(row["team_id"].ToString());

                            int groupID = int.Parse(row["group_id"].ToString());

                            if (!groups.ContainsKey(groupID)) { groups.Add(groupID, new List<Team>()); }
                            groups[groupID].Add(t);
                            points.Add(t.ID, 0);
                        }

                        //select matches
                        querry = "SELECT home_score, away_score, home_competitor, away_competitor, overtime, shootout " +
                                 "FROM matches " +
                                 "WHERE season_id = " + SportsData.SEASON.ID + " AND played = 1 AND serie_match_number = -1";

                        cmd = new(querry, connection);
                        cmd.Transaction = transaction;
                        dataTable = new DataTable();
                        dataTable.Load(cmd.ExecuteReader());

                        //calculate points
                        foreach (DataRow row in dataTable.Rows)
                        {
                            int homeScore = int.Parse(row["home_score"].ToString());
                            int awayScore = int.Parse(row["away_score"].ToString());
                            int homeID = int.Parse(row["home_competitor"].ToString());
                            int awayID = int.Parse(row["away_competitor"].ToString());
                            bool overtime = Convert.ToBoolean(int.Parse(row["overtime"].ToString()));
                            bool shootout = Convert.ToBoolean(int.Parse(row["shootout"].ToString()));

                            if (homeScore > awayScore)
                            {
                                if (overtime || shootout)
                                {
                                    points[homeID] += (int)SportsData.SEASON.PointsForOTWin;
                                    points[awayID] += (int)SportsData.SEASON.PointsForOTLoss;
                                }
                                else
                                {
                                    points[homeID] += (int)SportsData.SEASON.PointsForWin;
                                    points[awayID] += (int)SportsData.SEASON.PointsForLoss;
                                }
                            }
                            else if (homeScore < awayScore)
                            {
                                if (overtime || shootout)
                                {
                                    points[homeID] += (int)SportsData.SEASON.PointsForOTLoss;
                                    points[awayID] += (int)SportsData.SEASON.PointsForOTWin;
                                }
                                else
                                {
                                    points[homeID] += (int)SportsData.SEASON.PointsForLoss;
                                    points[awayID] += (int)SportsData.SEASON.PointsForWin;
                                }
                            }
                            else
                            {
                                points[homeID] += (int)SportsData.SEASON.PointsForTie;
                                points[awayID] += (int)SportsData.SEASON.PointsForTie;
                            }
                        }

                        Dictionary<int, List<Team>> tmp = new();
                        foreach (KeyValuePair<int, List<Team>> g in groups)
                        {
                            foreach (Team t in g.Value)
                            {
                                t.Stats = new TeamPoints(points[t.ID]);
                            }
                            tmp.Add(g.Key, g.Value.OrderByDescending(x => ((TeamPoints)x.Stats).Points).ToList());
                        }
                        groups = tmp;
                    }

                    //sort teams into one list
                    List<Team> allTeams = new();
                    List<List<Team>> groupLists = new();
                    foreach (KeyValuePair<int, List<Team>> g in groups)
                    {
                        groupLists.Add(g.Value);
                    }

                    int maxCount = groupLists.OrderByDescending(x => x.Count).First().Count;
                    for (int i = 0; i < maxCount; i++)
                    {
                        for (int j = 0; j < groupLists.Count; j++)
                        {
                            if (groupLists[j].Count <= maxCount)
                            {
                                allTeams.Add(groupLists[j][i]);
                            }
                        }
                    }

                    //seed teams
                    //find out seed places
                    int firstRoundPlaces = (int)Math.Pow(2, SportsData.SEASON.PlayOffRounds);
                    int[] places = new int[firstRoundPlaces];
                    for (int r = 0; r <= (int)Math.Log(firstRoundPlaces, 2); r++)
                    {
                        for (int N = 1; N <= firstRoundPlaces; ++N)
                        {
                            int myRank = ((N - 1) / (int)Math.Pow(2, r)) + 1;
                            places[N - 1] += myRank % 4 / 2 * (int)Math.Pow(2, (int)Math.Log(firstRoundPlaces, 2) - r - 1);
                        }
                    }
                    //create matches
                    //for each place from 0 to number of teams
                    for (int i = 0; i < places.Length; i += 2)
                    {
                        //if there is team in current place
                        int firstID = places[i] > allTeams.Count - 1 ? SportsData.NOID : allTeams[places[i]].ID;
                        int secondID = places[i + 1] > allTeams.Count - 1 ? SportsData.NOID : allTeams[places[i + 1]].ID;
                        if (firstID == SportsData.NOID && secondID == SportsData.NOID) { continue; }

                        querry = "INSERT INTO matches(season_id, played, qualification_id, bracket_index, round, serie_match_number, home_competitor, away_competitor, bracket_first_team) " +
                                              "VALUES (" + SportsData.SEASON.ID + ", 0, -1, " + (places[i] / 2) + ", 0, -1, " + firstID + ", " + secondID + ", " + firstID + ")";
                        cmd = new(querry, connection);
                        cmd.Transaction = transaction;
                        _ = cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                connection.Close();

                //refresh view
                SportsData.SEASON.PlayOffStarted = true;
                ScheduleViewModel vm = new(Ns);
                vm.CurrentViewModel = new PlayOffScheduleViewModel(Ns);
                vm.GroupsSet = false;
                vm.QualificationSet = false;
                vm.PlayOffSet = true;
                new NavigateCommand<SportViewModel>(Ns, () => new SportViewModel(Ns, vm)).Execute(null);
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

        private void LoadBracket()
        {
            CurrentBracket = new Bracket(SportsData.NOID, "Play-off", SportsData.SEASON.ID, SportsData.SEASON.PlayOffRounds);

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT h.name AS home_name, a.name AS away_name, home_competitor, away_competitor, " +
                                            "matches.id, played, datetime, home_score, away_score, bracket_index, round, serie_match_number, bracket_first_team " +
                                            "FROM matches " +
                                            "INNER JOIN team AS h ON h.id = matches.home_competitor " +
                                            "INNER JOIN team AS a ON a.id = matches.away_competitor " +
                                            "WHERE qualification_id = -1 AND bracket_index <> -1 AND season_id = " + SportsData.SEASON.ID + " ORDER BY round, bracket_index", connection);

            try
            {
                connection.Open();
                DataTable dt = new();
                dt.Load(cmd.ExecuteReader());

                //load matches
                foreach (DataRow dtRow in dt.Rows)
                {
                    Team home = new();
                    home.Name = dtRow["home_name"].ToString();
                    home.ID = int.Parse(dtRow["home_competitor"].ToString());
                    Team away = new();
                    away.Name = dtRow["away_name"].ToString();
                    away.ID = int.Parse(dtRow["away_competitor"].ToString());

                    Match m = new()
                    {
                        ID = int.Parse(dtRow["id"].ToString()),
                        Datetime = DateTime.Parse(dtRow["datetime"].ToString()),
                        Played = Convert.ToBoolean(int.Parse(dtRow["played"].ToString())),
                        HomeTeam = home,
                        AwayTeam = away,
                        HomeScore = int.Parse(dtRow["home_score"].ToString()),
                        AwayScore = int.Parse(dtRow["away_score"].ToString()),
                        SerieNumber = int.Parse(dtRow["serie_match_number"].ToString())
                    };

                    int round = int.Parse(dtRow["round"].ToString());
                    int index = int.Parse(dtRow["bracket_index"].ToString());
                    int firstTeamID = int.Parse(dtRow["bracket_first_team"].ToString());

                    CurrentBracket.Series[round][index].InsertMatch(m, firstTeamID, (SportsData.SEASON.PlayOffBestOf / 2) + 1);

                    if (firstTeamID != SportsData.NOID)
                    {
                        CurrentBracket.IsEnabledTreeAfterInsertionAt(round, index, 1, 1);
                    }
                    if (home.ID != SportsData.NOID && away.ID != SportsData.NOID)
                    {
                        CurrentBracket.IsEnabledTreeAfterInsertionAt(round, index, 2, 1);
                    }
                    else if (firstTeamID == SportsData.NOID)
                    {
                        CurrentBracket.IsEnabledTreeAfterInsertionAt(round, index, 2, 1);
                    }

                    if (NotSelectedTeams.Count(x => x.ID == home.ID) == 1)
                    {
                        _ = NotSelectedTeams.Remove(NotSelectedTeams.First(x => x.ID == home.ID));
                    }
                    if (NotSelectedTeams.Count(x => x.ID == away.ID) == 1)
                    {
                        _ = NotSelectedTeams.Remove(NotSelectedTeams.First(x => x.ID == away.ID));
                    }
                }

                CurrentBracket.PrepareSeries();
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
}