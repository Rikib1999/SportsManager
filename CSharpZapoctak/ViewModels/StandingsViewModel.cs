using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Others;
using CSharpZapoctak.Stores;
using LiveCharts;
using LiveCharts.Wpf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class TeamTableStats : ViewModelBase, IStats
    {
        #region Properties
        private int gamesPlayed = 0;
        public int GamesPlayed
        {
            get { return gamesPlayed; }
            set
            {
                gamesPlayed = value;
                OnPropertyChanged();
            }
        }

        private int wins = 0;
        public int Wins
        {
            get { return wins; }
            set
            {
                wins = value;
                OnPropertyChanged();
            }
        }

        private int winsOT = 0;
        public int WinsOT
        {
            get { return winsOT; }
            set
            {
                winsOT = value;
                OnPropertyChanged();
            }
        }

        private int ties = 0;
        public int Ties
        {
            get { return ties; }
            set
            {
                ties = value;
                OnPropertyChanged();
            }
        }

        private int lossesOT = 0;
        public int LossesOT
        {
            get { return lossesOT; }
            set
            {
                lossesOT = value;
                OnPropertyChanged();
            }
        }

        private int losses = 0;
        public int Losses
        {
            get { return losses; }
            set
            {
                losses = value;
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

        private int goalsAgainst = 0;
        public int GoalsAgainst
        {
            get { return goalsAgainst; }
            set
            {
                goalsAgainst = value;
                OnPropertyChanged();
            }
        }

        private int goalDifference = 0;
        public int GoalDifference
        {
            get { return goalDifference; }
            set
            {
                goalDifference = value;
                OnPropertyChanged();
            }
        }

        private int penaltyMinutes = 0;
        public int PenaltyMinutes
        {
            get { return penaltyMinutes; }
            set
            {
                penaltyMinutes = value;
                OnPropertyChanged();
            }
        }

        private int points = 0;
        public int Points
        {
            get { return points; }
            set
            {
                points = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public TeamTableStats() { }

        public TeamTableStats(Team t, Round r)
        {
            CalculateStats(t.id, r.id);
        }

        public void CalculateStats(int teamID, int roundID)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Run(() => CountMatches(teamID, roundID)));
            tasks.Add(Task.Run(() => CountGoals(teamID, roundID)));
            tasks.Add(Task.Run(() => CountGoalsAgainst(teamID, roundID)));
            tasks.Add(Task.Run(() => CountPenaltyMinutes(teamID, roundID)));
            Task.WaitAll(tasks.ToArray());
            GoalDifference = Goals - GoalsAgainst;
        }

        public async Task CountMatches(int teamID, int roundID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT home_competitor, away_competitor, home_score, away_score, overtime, shootout, round " +
                                                "FROM matches " +
                                                "WHERE (home_competitor = " + teamID + " OR away_competitor = " + teamID + ") AND played = 1 " +
                                                "AND qualification_id = -1 AND serie_match_number = -1 AND round <= " + roundID, connection);
            if (SportsData.season.id > 0) { cmd.CommandText += " AND matches.season_id = " + SportsData.season.id; }
            try
            {
                connection.Open();

                DataTable dataTable = new DataTable();
                dataTable.Load(await cmd.ExecuteReaderAsync());

                foreach (DataRow row in dataTable.Rows)
                {
                    int homeScore = int.Parse(row["home_score"].ToString());
                    int awayScore = int.Parse(row["away_score"].ToString());
                    int homeID = int.Parse(row["home_competitor"].ToString());
                    int awayID = int.Parse(row["away_competitor"].ToString());
                    bool overtime = Convert.ToBoolean(int.Parse(row["overtime"].ToString()));
                    bool shootout = Convert.ToBoolean(int.Parse(row["shootout"].ToString()));

                    GamesPlayed++;
                    if (homeID == teamID)
                    {
                        if (homeScore > awayScore)
                        {
                            if (overtime || shootout)
                            {
                                if (SportsData.season.id > 0) { Points += (int)SportsData.season.PointsForOTWin; }
                                WinsOT++;
                            }
                            else
                            {
                                if (SportsData.season.id > 0) { Points += (int)SportsData.season.PointsForWin; }
                                Wins++;
                            }
                        }
                        else if (homeScore < awayScore)
                        {
                            if (overtime || shootout)
                            {
                                if (SportsData.season.id > 0) { Points += (int)SportsData.season.PointsForOTLoss; }
                                LossesOT++;
                            }
                            else
                            {
                                if (SportsData.season.id > 0) { Points += (int)SportsData.season.PointsForLoss; }
                                Losses++;
                            }
                        }
                        else
                        {
                            if (SportsData.season.id > 0) { Points += (int)SportsData.season.PointsForTie; }
                            Ties++;
                        }
                    }
                    else
                    {
                        if (homeScore > awayScore)
                        {
                            if (overtime || shootout)
                            {
                                if (SportsData.season.id > 0) { Points += (int)SportsData.season.PointsForOTLoss; }
                                LossesOT++;
                            }
                            else
                            {
                                if (SportsData.season.id > 0) { Points += (int)SportsData.season.PointsForLoss; }
                                Losses++;
                            }
                        }
                        else if (homeScore < awayScore)
                        {
                            if (overtime || shootout)
                            {
                                if (SportsData.season.id > 0) { Points += (int)SportsData.season.PointsForOTWin; }
                                WinsOT++;
                            }
                            else
                            {
                                if (SportsData.season.id > 0) { Points += (int)SportsData.season.PointsForWin; }
                                Wins++;
                            }
                        }
                        else
                        {
                            if (SportsData.season.id > 0) { Points += (int)SportsData.season.PointsForTie; }
                            Ties++;
                        }
                    }
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

        public async Task CountGoals(int teamID, int roundID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM goals " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "WHERE team_id = " + teamID + " AND m.qualification_id = -1 AND m.serie_match_number = -1 AND round <= " + roundID, connection);
            if (SportsData.season.id > 0) { cmd.CommandText += " AND m.season_id = " + SportsData.season.id; }
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

        public async Task CountGoalsAgainst(int teamID, int roundID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM goals " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "WHERE opponent_team_id = " + teamID + " AND m.qualification_id = -1 AND m.serie_match_number = -1 AND round <= " + roundID, connection);
            if (SportsData.season.id > 0) { cmd.CommandText += " AND m.season_id = " + SportsData.season.id; }
            try
            {
                connection.Open();
                GoalsAgainst = (int)(long)await cmd.ExecuteScalarAsync();
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

        public async Task CountPenaltyMinutes(int teamID, int roundID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COALESCE(SUM(p.minutes), 0) FROM penalties " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN penalty_type AS p ON p.code = penalty_type_id " +
                                                "WHERE team_id = " + teamID + " AND m.qualification_id = -1 AND m.serie_match_number = -1 AND round <= " + roundID, connection);
            if (SportsData.season.id > 0) { cmd.CommandText += " AND m.season_id = " + SportsData.season.id; }
            try
            {
                connection.Open();
                PenaltyMinutes = Convert.ToInt32(await cmd.ExecuteScalarAsync());
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

        public int CompareTo(TeamTableStats other, bool onlyPoints)
        {
            /*ORDERING RULES:
                  1.   points
                  2.   points in H2H matches
                  3.   score in H2H matches
                  4.   goals  in H2H matches
                  5.   penalty minutes
                  6.   random   */
            if (Points < other.Points)
            {
                return -1;
            }
            else if (Points == other.Points)
            {
                if (onlyPoints)
                {
                    return 0;
                }
                else
                {
                    if (GoalDifference < other.GoalDifference)
                    {
                        return -1;
                    }
                    else if (GoalDifference == other.GoalDifference)
                    {
                        if (Goals < other.Goals)
                        {
                            return -1;
                        }
                        else if (Goals == other.Goals)
                        {
                            if (PenaltyMinutes > other.PenaltyMinutes)
                            {
                                return -1;
                            }
                            else if (PenaltyMinutes == other.PenaltyMinutes)
                            {
                                return 0;
                            }
                        }
                    }
                }
            }
            return 1;
        }
    }

    class StandingsViewModel : ViewModelBase
    {
        #region Visibilities
        private Visibility winnerIsSetVisibility = Visibility.Collapsed;
        public Visibility WinnerIsSetVisibility
        {
            get { return winnerIsSetVisibility; }
            set
            {
                winnerIsSetVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility winnerIsNotSetVisibility = Visibility.Visible;
        public Visibility WinnerIsNotSetVisibility
        {
            get { return winnerIsNotSetVisibility; }
            set
            {
                winnerIsNotSetVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility roundsVisibility = Visibility.Visible;
        public Visibility RoundsVisibility
        {
            get { return roundsVisibility; }
            set
            {
                roundsVisibility = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        private ICommand declareWinnerCommand;
        public ICommand DeclareWinnerCommand
        {
            get
            {
                if (declareWinnerCommand == null)
                {
                    declareWinnerCommand = new RelayCommand(param => DeclareWinner());
                }
                return declareWinnerCommand;
            }
        }

        private ICommand deleteWinnerCommand;
        public ICommand DeleteWinnerCommand
        {
            get
            {
                if (deleteWinnerCommand == null)
                {
                    deleteWinnerCommand = new RelayCommand(param => DeleteWinner());
                }
                return deleteWinnerCommand;
            }
        }

        private ICommand exportCommand;
        public ICommand ExportCommand
        {
            get
            {
                if (exportCommand == null)
                {
                    exportCommand = new RelayCommand(param => Exports.ExportStandings(Groups, param.ToString(), LastRound.Name));
                }
                return exportCommand;
            }
        }
        #endregion

        private SeriesCollection goalsSeries = new SeriesCollection();
        public SeriesCollection GoalsSeries
        {
            get { return goalsSeries; }
            set
            {
                goalsSeries = value;
                OnPropertyChanged();
            }
        }

        private SeriesCollection goalsAgainstSeries = new SeriesCollection();
        public SeriesCollection GoalsAgainstSeries
        {
            get { return goalsAgainstSeries; }
            set
            {
                goalsAgainstSeries = value;
                OnPropertyChanged();
            }
        }

        private SeriesCollection penaltyMinutesSeries = new SeriesCollection();
        public SeriesCollection PenaltyMinutesSeries
        {
            get { return penaltyMinutesSeries; }
            set
            {
                penaltyMinutesSeries = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Group> groups = new ObservableCollection<Group>();
        public ObservableCollection<Group> Groups
        {
            get { return groups; }
            set
            {
                groups = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Team> enlistedTeams = new ObservableCollection<Team>();
        public ObservableCollection<Team> EnlistedTeams
        {
            get { return enlistedTeams; }
            set
            {
                enlistedTeams = value;
                OnPropertyChanged();
            }
        }

        private Team winner = new Team();
        public Team Winner
        {
            get { return winner; }
            set
            {
                winner = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Round> rounds = new ObservableCollection<Round>();
        public ObservableCollection<Round> Rounds
        {
            get { return rounds; }
            set
            {
                rounds = value;
                OnPropertyChanged();
            }
        }

        private Round lastRound = new Round();
        public Round LastRound
        {
            get { return lastRound; }
            set
            {
                lastRound = value;
                LoadGroups();
                SortGroups();
                OnPropertyChanged();
            }
        }

        public NavigationStore ns;

        public StandingsViewModel(NavigationStore ns)
        {
            this.ns = ns;

            if (SportsData.season.WinnerID != -1)
            {
                Winner.Name = SportsData.season.WinnerName;
                WinnerIsNotSetVisibility = Visibility.Collapsed;
                WinnerIsSetVisibility = Visibility.Visible;
            }

            LoadRounds();
            LoadGroups();
            SortGroups();
            LoadEnlistedTeams();
            LoadPieChartsSeries();
        }

        private void LoadEnlistedTeams()
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT team_id, t.name AS team_name " +
                                                "FROM team_enlistment " +
                                                "INNER JOIN team AS t ON t.id = team_id " +
                                                "WHERE season_id = " + SportsData.season.id, connection);
            try
            {
                connection.Open();
                DataTable teamTable = new DataTable();
                teamTable.Load(cmd.ExecuteReader());

                foreach (DataRow t in teamTable.Rows)
                {
                    Team team = new Team
                    {
                        id = int.Parse(t["team_id"].ToString()),
                        Name = t["team_name"].ToString(),
                    };
                    EnlistedTeams.Add(team);
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

        private void DeclareWinner()
        {
            if (Winner.id < 1) { return; }

            //update database
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("UPDATE seasons SET winner_id = " + Winner.id + " WHERE id = " + SportsData.season.id, connection);

            try
            {   //execute querry
                connection.Open();
                cmd.ExecuteNonQuery();

                //update sportsdata season winner
                SportsData.season.WinnerID = Winner.id;
                SportsData.season.WinnerName = Winner.Name;

                //reload view
                new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new StandingsViewModel(ns))).Execute(null);
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

        private void DeleteWinner()
        {
            //update database
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("UPDATE seasons SET winner_id = -1 WHERE id = " + SportsData.season.id, connection);

            try
            {   //execute querry
                connection.Open();
                cmd.ExecuteNonQuery();

                //update sportsdata season winner
                SportsData.season.WinnerID = -1;
                SportsData.season.WinnerName = "";

                //reload view
                new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new StandingsViewModel(ns))).Execute(null);
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

        private void SortGroup(Group g)
        {
            g.Teams = MergeSort(g.Teams, true);
        }

        private void SortGroups()
        {
            foreach (Group g in Groups)
            {
                SortGroup(g);

                //group teams by points
                Dictionary<int, ObservableCollection<Team>> pointGroups = new Dictionary<int, ObservableCollection<Team>>();

                if (g.Teams.Count > 1 && ((TeamTableStats)g.Teams[0].Stats).Points == ((TeamTableStats)g.Teams[1].Stats).Points)
                {
                    pointGroups.Add(((TeamTableStats)g.Teams[0].Stats).Points, new ObservableCollection<Team>());
                    pointGroups[((TeamTableStats)g.Teams[0].Stats).Points].Add(g.Teams[0]);
                }
                for (int i = 1; i < g.Teams.Count; i++)
                {
                    if (((TeamTableStats)g.Teams[i].Stats).Points == ((TeamTableStats)g.Teams[i - 1].Stats).Points)
                    {
                        if (!pointGroups.ContainsKey(((TeamTableStats)g.Teams[i].Stats).Points))
                        {
                            pointGroups.Add(((TeamTableStats)g.Teams[i].Stats).Points, new ObservableCollection<Team>());
                        }
                        pointGroups[((TeamTableStats)g.Teams[i].Stats).Points].Add(g.Teams[i]);
                    }
                }

                //sort them by tie-breaker criteria
                foreach (KeyValuePair<int, ObservableCollection<Team>> pg in pointGroups)
                {
                    if (pg.Value.Count > 1)
                    {
                        ObservableCollection<Team> sortedTeams = MergeSort(pg.Value, false);
                        foreach (Team t in sortedTeams)
                        {
                            g.Teams.Remove(t);
                        }
                        for (int i = 0; i <= g.Teams.Count; i++)
                        {
                            if (i == g.Teams.Count)
                            {
                                for (int j = 0; j < sortedTeams.Count; j++)
                                {
                                    g.Teams.Add(sortedTeams[j]);
                                }
                                break;
                            }
                            if (((TeamTableStats)g.Teams[i].Stats).Points < pg.Key)
                            {
                                for (int j = 0; j < sortedTeams.Count; j++)
                                {
                                    g.Teams.Insert(i + j, sortedTeams[j]);
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        private ObservableCollection<Team> MergeSort(ObservableCollection<Team> unsorted, bool onlyPoints)
        {
            if (unsorted.Count <= 1)
                return unsorted;

            ObservableCollection<Team> left = new ObservableCollection<Team>();
            ObservableCollection<Team> right = new ObservableCollection<Team>();

            int middle = unsorted.Count / 2;
            for (int i = 0; i < middle; i++)
            {
                left.Add(unsorted[i]);
            }
            for (int i = middle; i < unsorted.Count; i++)
            {
                right.Add(unsorted[i]);
            }

            left = MergeSort(left, onlyPoints);
            right = MergeSort(right, onlyPoints);
            return Merge(left, right, onlyPoints);
        }

        private ObservableCollection<Team> Merge(ObservableCollection<Team> left, ObservableCollection<Team> right, bool onlyPoints)
        {
            ObservableCollection<Team> result = new ObservableCollection<Team>();

            while (left.Count > 0 || right.Count > 0)
            {
                if (left.Count > 0 && right.Count > 0)
                {
                    if (((TeamTableStats)left.First().Stats).CompareTo((TeamTableStats)right.First().Stats, onlyPoints) > 0)
                    {
                        result.Add(left.First());
                        left.Remove(left.First());
                    }
                    else
                    {
                        result.Add(right.First());
                        right.Remove(right.First());
                    }
                }
                else if (left.Count > 0)
                {
                    result.Add(left.First());
                    left.Remove(left.First());
                }
                else if (right.Count > 0)
                {
                    result.Add(right.First());

                    right.Remove(right.First());
                }
            }
            return result;
        }

        private void LoadGroups()
        {
            Groups = new ObservableCollection<Group>();
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT id, name FROM groups WHERE season_id = " + SportsData.season.id, connection);
            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    Group g = new Group
                    {
                        id = int.Parse(row["id"].ToString()),
                        SeasonID = SportsData.season.id,
                        Name = row["name"].ToString(),
                        Teams = new ObservableCollection<Team>()
                    };
                    Groups.Add(g);

                    cmd = new MySqlCommand("SELECT team_id, t.name AS team_name FROM team_enlistment " +
                                           "INNER JOIN team AS t ON t.id = team_id " +
                                           "WHERE group_id = " + g.id, connection);
                    DataTable teamTable = new DataTable();
                    teamTable.Load(cmd.ExecuteReader());

                    foreach (DataRow t in teamTable.Rows)
                    {
                        Team team = new Team
                        {
                            id = int.Parse(t["team_id"].ToString()),
                            Name = t["team_name"].ToString(),
                        };
                        team.Stats = new TeamTableStats(team, LastRound);
                        g.Teams.Add(team);
                    }
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

        private void LoadRounds()
        {
            Rounds = new ObservableCollection<Round>();
            LastRound = new Round();
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT id, name FROM rounds WHERE season_id = " + SportsData.season.id, connection);
            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    Round r = new Round
                    {
                        id = int.Parse(row["id"].ToString()),
                        SeasonID = SportsData.season.id,
                        Name = row["name"].ToString()
                    };
                    Rounds.Add(r);
                }

                if (Rounds.Count > 0)
                {
                    LastRound = Rounds.OrderBy(x => x.id).Last();
                }
                else
                {
                    RoundsVisibility = Visibility.Collapsed;
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

        private void LoadPieChartsSeries()
        {
            foreach (Group g in Groups)
            {
                foreach (Team t in g.Teams)
                {
                    GoalsSeries.Add(new PieSeries
                    {
                        Values = new ChartValues<int> { ((TeamTableStats)t.Stats).Goals },
                        Title = t.Name,
                        LabelPoint = chartPoint => chartPoint.Y.ToString(),
                        DataLabels = true,
                        FontSize = 26
                    });
                    GoalsAgainstSeries.Add(new PieSeries
                    {
                        Values = new ChartValues<int> { ((TeamTableStats)t.Stats).GoalsAgainst },
                        Title = t.Name,
                        LabelPoint = chartPoint => chartPoint.Y.ToString(),
                        DataLabels = true,
                        FontSize = 26
                    });
                    PenaltyMinutesSeries.Add(new PieSeries
                    {
                        Values = new ChartValues<int> { ((TeamTableStats)t.Stats).PenaltyMinutes },
                        Title = t.Name,
                        LabelPoint = chartPoint => chartPoint.Y.ToString(),
                        DataLabels = true,
                        FontSize = 26
                    });
                }
            }
        }
    }
}