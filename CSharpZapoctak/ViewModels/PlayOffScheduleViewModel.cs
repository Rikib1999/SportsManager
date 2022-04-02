using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class TeamPoints : IStats
    {
        public TeamPoints(int points)
        {
            this.points = points;
        }

        public int points;
    }

    class PlayOffScheduleViewModel : NotifyPropertyChanged
    {
        private readonly NavigationStore ns;

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

        private ICommand matchDetailCommand;
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
        public ICommand AddMatchCommand
        {
            get
            {
                if (addMatchCommand == null)
                {
                    addMatchCommand = new RelayCommand(param => AddMatch((Serie)param));
                }
                return addMatchCommand;
            }
        }

        private ICommand removeFirstTeamFromSerieCommand;
        public ICommand RemoveFirstTeamFromSerieCommand
        {
            get
            {
                if (removeFirstTeamFromSerieCommand == null)
                {
                    removeFirstTeamFromSerieCommand = new RelayCommand(param => RemoveFirstTeamFromSerie((Serie)param));
                }
                return removeFirstTeamFromSerieCommand;
            }
        }

        private ICommand addFirstTeamToSerieCommand;
        public ICommand AddFirstTeamToSerieCommand
        {
            get
            {
                if (addFirstTeamToSerieCommand == null)
                {
                    addFirstTeamToSerieCommand = new RelayCommand(param => AddFirstTeamToSerie((Serie)param));
                }
                return addFirstTeamToSerieCommand;
            }
        }

        private ICommand removeSecondTeamFromSerieCommand;
        public ICommand RemoveSecondTeamFromSerieCommand
        {
            get
            {
                if (removeSecondTeamFromSerieCommand == null)
                {
                    removeSecondTeamFromSerieCommand = new RelayCommand(param => RemoveSecondTeamFromSerie((Serie)param));
                }
                return removeSecondTeamFromSerieCommand;
            }
        }

        private ICommand addSecondTeamToSerieCommand;
        public ICommand AddSecondTeamToSerieCommand
        {
            get
            {
                if (addSecondTeamToSerieCommand == null)
                {
                    addSecondTeamToSerieCommand = new RelayCommand(param => AddSecondTeamToSerie((Serie)param));
                }
                return addSecondTeamToSerieCommand;
            }
        }
        #endregion

        private ObservableCollection<Team> notSelectedTeams;
        public ObservableCollection<Team> NotSelectedTeams
        {
            get { return notSelectedTeams; }
            set
            {
                notSelectedTeams = value;
                OnPropertyChanged();
            }
        }

        private Bracket bracket;
        public Bracket Bracket
        {
            get { return bracket; }
            set
            {
                bracket = value;
                OnPropertyChanged();
            }
        }

        private bool seedCompetitors = false;
        public bool SeedCompetitors
        {
            get { return seedCompetitors; }
            set
            {
                seedCompetitors = value;
                OnPropertyChanged();
            }
        }

        private Visibility notStartedVisibility = Visibility.Visible;
        public Visibility NotStartedVisibility
        {
            get { return notStartedVisibility; }
            set
            {
                notStartedVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility startedVisibility = Visibility.Collapsed;
        public Visibility StartedVisibility
        {
            get { return startedVisibility; }
            set
            {
                startedVisibility = value;
                OnPropertyChanged();
            }
        }

        public bool IsEnabled { get; private set; } = true;

        public PlayOffScheduleViewModel(NavigationStore navigationStore)
        {
            ns = navigationStore;
            if (SportsData.season.WinnerID != -1) { IsEnabled = false; }

            if (SportsData.season.PlayOffStarted)
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

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT team_id, t.name AS team_name FROM team_enlistment " +
                                                "INNER JOIN team AS t ON t.id = team_id " +
                                                "WHERE season_id = " + SportsData.season.id, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow tm in dataTable.Rows)
                {
                    Team t = new Team
                    {
                        id = int.Parse(tm["team_id"].ToString()),
                        Name = tm["team_name"].ToString(),
                    };

                    if (t.id != -1)
                    {
                        NotSelectedTeams.Add(t);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlTransaction transaction = null;
            MySqlCommand cmd = null;
            string querry = "UPDATE seasons SET play_off_started = 0 WHERE id = " + SportsData.season.id;

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                //update season
                cmd = new MySqlCommand(querry, connection);
                cmd.Transaction = transaction;
                cmd.ExecuteNonQuery();

                //clear bracket
                cmd = new MySqlCommand("DELETE FROM matches WHERE qualification_id = -1 AND bracket_index <> -1 AND season_id = " + SportsData.season.id, connection);
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                transaction.Commit();
                connection.Close();

                //refresh view
                SportsData.season.PlayOffStarted = false;
                ScheduleViewModel vm = new ScheduleViewModel(ns);
                vm.CurrentViewModel = new PlayOffScheduleViewModel(ns);
                vm.GroupsSet = false;
                vm.QualificationSet = false;
                vm.PlayOffSet = true;
                new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, vm)).Execute(null);
            }
            catch (Exception)
            {
                transaction.Rollback();
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlTransaction transaction = null;
            MySqlCommand cmd = null;
            string querry = "UPDATE seasons SET play_off_started = 1 WHERE id = " + SportsData.season.id;

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                //update season
                cmd = new MySqlCommand(querry, connection);
                cmd.Transaction = transaction;
                cmd.ExecuteNonQuery();

                //seed competitors
                if (SeedCompetitors && (SportsData.season.GroupCount > 0 || SportsData.season.QualificationCount > 0))
                {
                    Dictionary<int, List<Team>> groups = new Dictionary<int, List<Team>>();

                    //clear bracket
                    cmd = new MySqlCommand("DELETE FROM matches WHERE qualification_id = -1 AND bracket_index <> -1 AND season_id = " + SportsData.season.id, connection);
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    //if season had qualification
                    if (SportsData.season.QualificationCount > 0)
                    {
                        //select teams
                        querry = "SELECT home_score, away_score, home_competitor, away_competitor " +
                                 "FROM matches " +
                                 "WHERE season_id = " + SportsData.season.id + " AND played = 1 AND round = " + (SportsData.season.QualificationRounds - 1);

                        cmd = new MySqlCommand(querry, connection);
                        cmd.Transaction = transaction;
                        DataTable dataTable = new DataTable();
                        dataTable.Load(cmd.ExecuteReader());

                        List<Team> teams = new List<Team>();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            int homeScore = int.Parse(row["home_score"].ToString());
                            int awayScore = int.Parse(row["away_score"].ToString());
                            Team t = new Team();
                            if (homeScore > awayScore)
                            {
                                t.id = int.Parse(row["home_competitor"].ToString());
                            }
                            else if (homeScore < awayScore)
                            {
                                t.id = int.Parse(row["away_competitor"].ToString());
                            }
                            else
                            {
                                continue;
                            }

                            teams.Add(t);
                        }
                        groups.Add(-1, teams);
                    }
                    //if season had groups
                    else if (SportsData.season.GroupCount > 0)
                    {
                        //select teams
                        querry = "SELECT team_id, group_id " +
                                 "FROM team_enlistment " +
                                 "WHERE season_id = " + SportsData.season.id;

                        cmd = new MySqlCommand(querry, connection);
                        cmd.Transaction = transaction;
                        DataTable dataTable = new DataTable();
                        dataTable.Load(cmd.ExecuteReader());

                        Dictionary<int, int> points = new Dictionary<int, int>();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            Team t = new Team();
                            t.id = int.Parse(row["team_id"].ToString());

                            int groupID = int.Parse(row["group_id"].ToString());

                            if (!groups.ContainsKey(groupID)) { groups.Add(groupID, new List<Team>()); }
                            groups[groupID].Add(t);
                            points.Add(t.id, 0);
                        }

                        //select matches
                        querry = "SELECT home_score, away_score, home_competitor, away_competitor, overtime, shootout " +
                                 "FROM matches " +
                                 "WHERE season_id = " + SportsData.season.id + " AND played = 1 AND serie_match_number = -1";

                        cmd = new MySqlCommand(querry, connection);
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
                                    points[homeID] += (int)SportsData.season.PointsForOTWin;
                                    points[awayID] += (int)SportsData.season.PointsForOTLoss;
                                }
                                else
                                {
                                    points[homeID] += (int)SportsData.season.PointsForWin;
                                    points[awayID] += (int)SportsData.season.PointsForLoss;
                                }
                            }
                            else if (homeScore < awayScore)
                            {
                                if (overtime || shootout)
                                {
                                    points[homeID] += (int)SportsData.season.PointsForOTLoss;
                                    points[awayID] += (int)SportsData.season.PointsForOTWin;
                                }
                                else
                                {
                                    points[homeID] += (int)SportsData.season.PointsForLoss;
                                    points[awayID] += (int)SportsData.season.PointsForWin;
                                }
                            }
                            else
                            {
                                points[homeID] += (int)SportsData.season.PointsForTie;
                                points[awayID] += (int)SportsData.season.PointsForTie;
                            }
                        }

                        Dictionary<int, List<Team>> tmp = new Dictionary<int, List<Team>>();
                        foreach (KeyValuePair<int, List<Team>> g in groups)
                        {
                            foreach (Team t in g.Value)
                            {
                                t.Stats = new TeamPoints(points[t.id]);
                            }
                            tmp.Add(g.Key, g.Value.OrderByDescending(x => ((TeamPoints)x.Stats).points).ToList());
                        }
                        groups = tmp;
                    }

                    //sort teams into one list
                    List<Team> allTeams = new List<Team>();
                    List<List<Team>> groupLists = new List<List<Team>>();
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
                    int firstRoundPlaces = (int)Math.Pow(2, SportsData.season.PlayOffRounds);
                    int[] places = new int[firstRoundPlaces];
                    for (int r = 0; r <= (int)Math.Log(firstRoundPlaces, 2); r++)
                    {
                        for (int N = 1; N <= firstRoundPlaces; ++N)
                        {
                            int myRank = (N - 1) / (int)Math.Pow(2, r) + 1;
                            places[N - 1] += myRank % 4 / 2 * (int)Math.Pow(2, (int)Math.Log(firstRoundPlaces, 2) - r - 1);
                        }
                    }
                    //create matches
                    //for each place from 0 to number of teams
                    for (int i = 0; i < places.Length; i += 2)
                    {
                        //if there is team in current place
                        int first = places[i] > allTeams.Count - 1 ? -1 : allTeams[places[i]].id;
                        int second = places[i + 1] > allTeams.Count - 1 ? -1 : allTeams[places[i + 1]].id;
                        if(first == -1 && second == -1) { continue; }

                        querry = "INSERT INTO matches(season_id, played, qualification_id, bracket_index, round, serie_match_number, home_competitor, away_competitor, bracket_first_team) " +
                                              "VALUES (" + SportsData.season.id + ", 0, -1, " + (places[i] / 2) + ", 0, -1, " + first + ", " + second + ", " + first + ")";
                        cmd = new MySqlCommand(querry, connection);
                        cmd.Transaction = transaction;
                        cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                connection.Close();

                //refresh view
                SportsData.season.PlayOffStarted = true;
                ScheduleViewModel vm = new ScheduleViewModel(ns);
                vm.CurrentViewModel = new PlayOffScheduleViewModel(ns);
                vm.GroupsSet = false;
                vm.QualificationSet = false;
                vm.PlayOffSet = true;
                new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, vm)).Execute(null);
            }
            catch (Exception)
            {
                transaction.Rollback();
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            Bracket = new Bracket(-1, "Play-off", SportsData.season.id, SportsData.season.PlayOffRounds);

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + "; convert zero datetime=True";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT h.name AS home_name, a.name AS away_name, home_competitor, away_competitor, " +
                                            "matches.id, played, datetime, home_score, away_score, bracket_index, round, serie_match_number, bracket_first_team " +
                                            "FROM matches", connection);
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
            cmd.CommandText += " WHERE qualification_id = -1 AND bracket_index <> -1 AND season_id = " + SportsData.season.id + " ORDER BY round, bracket_index";

            try
            {
                connection.Open();
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                //load matches
                foreach (DataRow dtRow in dt.Rows)
                {
                    Team home = new Team();
                    home.Name = dtRow["home_name"].ToString();
                    home.id = int.Parse(dtRow["home_competitor"].ToString());
                    Team away = new Team();
                    away.Name = dtRow["away_name"].ToString();
                    away.id = int.Parse(dtRow["away_competitor"].ToString());

                    Match m = new Match
                    {
                        id = int.Parse(dtRow["id"].ToString()),
                        Datetime = DateTime.Parse(dtRow["datetime"].ToString()),
                        Played = Convert.ToBoolean(int.Parse(dtRow["played"].ToString())),
                        HomeTeam = home,
                        AwayTeam = away,
                        HomeScore = int.Parse(dtRow["home_score"].ToString()),
                        AwayScore = int.Parse(dtRow["away_score"].ToString()),
                        serieNumber = int.Parse(dtRow["serie_match_number"].ToString())
                    };

                    int round = int.Parse(dtRow["round"].ToString());
                    int index = int.Parse(dtRow["bracket_index"].ToString());
                    int firstTeamID = int.Parse(dtRow["bracket_first_team"].ToString());

                    Bracket.Series[round][index].InsertMatch(m, firstTeamID, (SportsData.season.PlayOffBestOf / 2) + 1);

                    if (firstTeamID != -1)
                    {
                        Bracket.IsEnabledTreeAfterInsertionAt(round, index, 1, 1);
                    }
                    if (home.id != -1 && away.id != -1)
                    {
                        Bracket.IsEnabledTreeAfterInsertionAt(round, index, 2, 1);
                    }
                    else if (firstTeamID == -1)
                    {
                        Bracket.IsEnabledTreeAfterInsertionAt(round, index, 2, 1);
                    }

                    if (NotSelectedTeams.Count(x => x.id == home.id) == 1)
                    {
                        NotSelectedTeams.Remove(NotSelectedTeams.First(x => x.id == home.id));
                    }
                    if (NotSelectedTeams.Count(x => x.id == away.id) == 1)
                    {
                        NotSelectedTeams.Remove(NotSelectedTeams.First(x => x.id == away.id));
                    }
                }

                Bracket.PrepareSeries();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void MatchDetail(Match m)
        {
            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new MatchViewModel(ns, m, new PlayOffScheduleViewModel(ns)))).Execute(null);
        }

        private void AddMatch(Serie s)
        {
            (int, int) roundIndex = Bracket.GetSerieRoundIndex(s);

            int matchNumber = s.Matches.Count(x => x.Played) + 1;

            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new AddMatchViewModel(ns, new PlayOffScheduleViewModel(ns), Bracket.id, roundIndex.Item2, roundIndex.Item1, matchNumber, s.FirstTeam, s.SecondTeam))).Execute(null);
        }

        private void RemoveFirstTeamFromSerie(Serie s)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd;

            if (s.Matches[0].HomeTeam.id == -1 || s.Matches[0].AwayTeam.id == -1)
            {
                //DELETE
                cmd = new MySqlCommand("DELETE FROM matches WHERE id = " + s.Matches[0].id, connection);
                s.Matches.RemoveAt(0);
            }
            else
            {
                //UPDATE
                if (s.Matches[0].HomeTeam.id == s.FirstTeam.id)
                {
                    //delete home
                    cmd = new MySqlCommand("UPDATE matches SET home_competitor = -1 WHERE id = " + s.Matches[0].id, connection);
                    s.Matches[0].HomeTeam = new Team { id = -1 };
                }
                else
                {
                    //delete away
                    cmd = new MySqlCommand("UPDATE matches SET away_competitor = -1 WHERE id = " + s.Matches[0].id, connection);
                    s.Matches[0].AwayTeam = new Team { id = -1 };
                }
            }

            try
            {
                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            NotSelectedTeams.Add(s.FirstTeam);
            s.FirstTeam = new Team();

            (int, int) roundIndex = Bracket.GetSerieRoundIndex(s);
            Bracket.ResetSeriesAdvanced(roundIndex.Item1, roundIndex.Item2, 1);
            Bracket.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 1, -1);
            Bracket.PrepareSeries();
        }

        private void AddFirstTeamToSerie(Serie s)
        {
            (int, int) roundIndex = Bracket.GetSerieRoundIndex(s);

            if (s.FirstSelectedTeam == null || !NotSelectedTeams.Contains(s.FirstSelectedTeam))
            {
                return;
            }

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd;

            if (s.Matches.Count == 0)
            {
                //INSERT
                cmd = new MySqlCommand("INSERT INTO matches(season_id, played, qualification_id, bracket_index, round, serie_match_number, home_competitor, away_competitor, bracket_first_team) " +
                                       "VALUES (" + SportsData.season.id + ", 0, " + Bracket.id + ", " + roundIndex.Item2 + "," +
                                       " " + roundIndex.Item1 + ", -1, " + s.FirstSelectedTeam.id + ", -1, " + s.FirstSelectedTeam.id + ")", connection);
            }
            else
            {
                //UPDATE
                if (s.Matches[0].HomeTeam.id == -1)
                {
                    //home
                    cmd = new MySqlCommand("UPDATE matches SET home_competitor = " + s.FirstSelectedTeam.id + ", bracket_first_team = " + s.FirstSelectedTeam.id + " WHERE id = " + s.Matches[0].id, connection);
                    s.Matches[0].HomeTeam = s.FirstSelectedTeam;
                }
                else
                {
                    //away
                    cmd = new MySqlCommand("UPDATE matches SET away_competitor = " + s.FirstSelectedTeam.id + ", bracket_first_team = " + s.FirstSelectedTeam.id + " WHERE id = " + s.Matches[0].id, connection);
                    s.Matches[0].AwayTeam = s.FirstSelectedTeam;
                }
            }

            int matchID = -1;
            try
            {
                connection.Open();
                cmd.ExecuteNonQuery();
                matchID = (int)cmd.LastInsertedId;
                connection.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            s.FirstTeam = s.FirstSelectedTeam;
            s.FirstSelectedTeam = new Team();
            s.winner = new Team { id = -1 };
            s.RemoveFirstTeamVisibility = Visibility.Visible;
            s.RemoveSecondTeamVisibility = Visibility.Visible;
            NotSelectedTeams.Remove(s.FirstTeam);

            if (s.Matches.Count == 0)
            {
                Match m = new Match { id = matchID, Played = false, HomeTeam = s.FirstTeam, AwayTeam = new Team { id = -1 }, serieNumber = -1 };
                s.InsertMatch(m, s.FirstTeam.id, 1);
            }
            if (roundIndex.Item1 < Bracket.Series.Count - 1)
            {
                int newPosition = 2;
                if (roundIndex.Item2 % 2 == 0) { newPosition = 1; }
                Bracket.ResetSeriesAdvanced(roundIndex.Item1 + 1, roundIndex.Item2 / 2, newPosition);
            }

            Bracket.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 1, 1);
            Bracket.PrepareSeries();
        }

        private void RemoveSecondTeamFromSerie(Serie s)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd;

            if (s.Matches[0].HomeTeam.id == -1 || s.Matches[0].AwayTeam.id == -1)
            {
                //DELETE
                cmd = new MySqlCommand("DELETE FROM matches WHERE id = " + s.Matches[0].id, connection);
                s.Matches.RemoveAt(0);
            }
            else
            {
                //UPDATE
                if (s.Matches[0].HomeTeam.id == s.SecondTeam.id)
                {
                    //delete home
                    cmd = new MySqlCommand("UPDATE matches SET home_competitor = -1 WHERE id = " + s.Matches[0].id, connection);
                    s.Matches[0].HomeTeam = new Team { id = -1 };
                }
                else
                {
                    //delete away
                    cmd = new MySqlCommand("UPDATE matches SET away_competitor = -1 WHERE id = " + s.Matches[0].id, connection);
                    s.Matches[0].AwayTeam = new Team { id = -1 };
                }
            }

            try
            {
                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            NotSelectedTeams.Add(s.SecondTeam);
            s.SecondTeam = new Team();

            (int, int) roundIndex = Bracket.GetSerieRoundIndex(s);
            Bracket.ResetSeriesAdvanced(roundIndex.Item1, roundIndex.Item2, 2);
            Bracket.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 2, -1);
            Bracket.PrepareSeries();
        }

        private void AddSecondTeamToSerie(Serie s)
        {
            (int, int) roundIndex = Bracket.GetSerieRoundIndex(s);

            if (s.SecondSelectedTeam == null || !NotSelectedTeams.Contains(s.SecondSelectedTeam))
            {
                return;
            }

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd;

            if (s.Matches.Count == 0)
            {
                //INSERT
                cmd = new MySqlCommand("INSERT INTO matches(season_id, played, qualification_id, bracket_index, round, serie_match_number, home_competitor, away_competitor, bracket_first_team) " +
                                       "VALUES (" + SportsData.season.id + ", 0, " + Bracket.id + ", " + roundIndex.Item2 + "," +
                                       " " + roundIndex.Item1 + ", -1, -1, " + s.SecondSelectedTeam.id + ", -1)", connection);
            }
            else
            {
                //UPDATE
                if (s.Matches[0].HomeTeam.id == -1)
                {
                    //home
                    cmd = new MySqlCommand("UPDATE matches SET home_competitor = " + s.SecondSelectedTeam.id + " WHERE id = " + s.Matches[0].id, connection);
                    s.Matches[0].HomeTeam = s.SecondSelectedTeam;
                }
                else
                {
                    //away
                    cmd = new MySqlCommand("UPDATE matches SET away_competitor = " + s.SecondSelectedTeam.id + " WHERE id = " + s.Matches[0].id, connection);
                    s.Matches[0].AwayTeam = s.SecondSelectedTeam;
                }
            }

            int matchID = -1;
            try
            {
                connection.Open();
                cmd.ExecuteNonQuery();
                matchID = (int)cmd.LastInsertedId;
                connection.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            s.SecondTeam = s.SecondSelectedTeam;
            s.SecondSelectedTeam = new Team();
            s.winner = new Team { id = -1 };
            s.RemoveFirstTeamVisibility = Visibility.Visible;
            s.RemoveSecondTeamVisibility = Visibility.Visible;
            NotSelectedTeams.Remove(s.SecondTeam);

            if (s.Matches.Count == 0)
            {
                Match m = new Match { id = matchID, Played = false, AwayTeam = s.SecondTeam, HomeTeam = new Team { id = -1 }, serieNumber = -1 };
                s.InsertMatch(m, -1, 1);
            }
            if (roundIndex.Item1 < Bracket.Series.Count - 1)
            {
                int newPosition = 2;
                if (roundIndex.Item2 % 2 == 0) { newPosition = 1; }
                Bracket.ResetSeriesAdvanced(roundIndex.Item1 + 1, roundIndex.Item2 / 2, newPosition);
            }

            Bracket.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 2, 1);
            Bracket.PrepareSeries();
        }
    }
}