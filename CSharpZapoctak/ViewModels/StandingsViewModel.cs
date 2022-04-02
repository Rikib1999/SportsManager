using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Others;
using CSharpZapoctak.Stores;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CSharpZapoctak.ViewModels
{
    class TeamTableStats : NotifyPropertyChanged, IStats
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

        private int penaltyMinutesAgainst = 0;
        public int PenaltyMinutesAgainst
        {
            get { return penaltyMinutesAgainst; }
            set
            {
                penaltyMinutesAgainst = value;
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
            tasks.Add(Task.Run(() => CountAssists(teamID, roundID)));
            tasks.Add(Task.Run(() => CountGoalsAgainst(teamID, roundID)));
            tasks.Add(Task.Run(() => CountPenaltyMinutes(teamID, roundID)));
            tasks.Add(Task.Run(() => CountPenaltyMinutesAgainst(teamID, roundID)));
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

        private async Task CountAssists(int teamID, int roundID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM goals " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "WHERE assist_player_id <> -1 AND team_id = " + teamID + " AND m.qualification_id = -1 AND m.serie_match_number = -1 AND round <= " + roundID, connection);
            if (SportsData.season.id > 0) { cmd.CommandText += " AND m.season_id = " + SportsData.season.id; }
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

        public async Task CountPenaltyMinutesAgainst(int teamID, int roundID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COALESCE(SUM(p.minutes), 0) FROM penalties " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN penalty_type AS p ON p.code = penalty_type_id " +
                                                "WHERE opponent_team_id = " + teamID + " AND m.qualification_id = -1 AND m.serie_match_number = -1 AND round <= " + roundID, connection);
            if (SportsData.season.id > 0) { cmd.CommandText += " AND m.season_id = " + SportsData.season.id; }
            try
            {
                connection.Open();
                PenaltyMinutesAgainst = Convert.ToInt32(await cmd.ExecuteScalarAsync());
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

    class StandingsViewModel : NotifyPropertyChanged
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

        private ICommand exportStandingsCommand;
        public ICommand ExportStandingsCommand
        {
            get
            {
                if (exportStandingsCommand == null)
                {
                    exportStandingsCommand = new RelayCommand(param => Exports.ExportStandings(Groups, param.ToString(), LastRound.Name));
                }
                return exportStandingsCommand;
            }
        }

        private ICommand exportChartCommand;
        public ICommand ExportChartCommand
        {
            get
            {
                if (exportChartCommand == null)
                {
                    exportChartCommand = new RelayCommand(param => Exports.ExportChart((FrameworkElement)param));
                }
                return exportChartCommand;
            }
        }
        #endregion

        #region Charting data
        public Func<double, string> AxisFormatterScatter { get; set; } = value => value.ToString("N2");

        public double GoalsYSectionMedian { get; set; }

        public double GoalsYSectionEnd { get; set; }

        public double GoalsXSectionMedian { get; set; }

        public double GoalsXSectionEnd { get; set; }

        public double AssistsYSectionMedian { get; set; }

        public double AssistsYSectionEnd { get; set; }

        public double AssistsXSectionMedian { get; set; }

        public double AssistsXSectionEnd { get; set; }

        public double PenaltiesYSectionMedian { get; set; }

        public double PenaltiesYSectionEnd { get; set; }

        public double PenaltiesXSectionMedian { get; set; }

        public double PenaltiesXSectionEnd { get; set; }

        private VisualElementsCollection goalsVisuals = new VisualElementsCollection();
        public VisualElementsCollection GoalsVisuals
        {
            get { return goalsVisuals; }
            set
            {
                goalsVisuals = value;
                OnPropertyChanged();
            }
        }

        private VisualElementsCollection assistsVisuals = new VisualElementsCollection();
        public VisualElementsCollection AssistsVisuals
        {
            get { return assistsVisuals; }
            set
            {
                assistsVisuals = value;
                OnPropertyChanged();
            }
        }

        private VisualElementsCollection penaltiesVisuals = new VisualElementsCollection();
        public VisualElementsCollection PenaltiesVisuals
        {
            get { return penaltiesVisuals; }
            set
            {
                penaltiesVisuals = value;
                OnPropertyChanged();
            }
        }

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

        private SeriesCollection goalsScatterSeries = new SeriesCollection();
        public SeriesCollection GoalsScatterSeries
        {
            get { return goalsScatterSeries; }
            set
            {
                goalsScatterSeries = value;
                OnPropertyChanged();
            }
        }

        private SeriesCollection assistsScatterSeries = new SeriesCollection();
        public SeriesCollection AssistsScatterSeries
        {
            get { return assistsScatterSeries; }
            set
            {
                assistsScatterSeries = value;
                OnPropertyChanged();
            }
        }

        private SeriesCollection penaltiesScatterSeries = new SeriesCollection();
        public SeriesCollection PenaltiesScatterSeries
        {
            get { return penaltiesScatterSeries; }
            set
            {
                penaltiesScatterSeries = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Data
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

        private Round lastRound;
        public Round LastRound
        {
            get { return lastRound; }
            set
            {
                if (lastRound == value) { return; }
                lastRound = value;
                LoadGroups();
                SortGroups();
                LoadPieChartsSeries();
                LoadGoalsScatterSeries();
                LoadAssistsScatterSeries();
                LoadPenaltiesScatterSeries();
                OnPropertyChanged();
            }
        }

        public NavigationStore ns;
        #endregion

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
            LoadEnlistedTeams();
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
                            Name = t["team_name"].ToString()
                        };
                        team.Stats = new TeamTableStats(team, LastRound);

                        string[] imgPath = System.IO.Directory.GetFiles(SportsData.TeamLogosPath, SportsData.sport.name + team.id + ".*");
                        if (imgPath.Length != 0)
                        {
                            team.LogoPath = imgPath.First();
                        }

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
            GoalsSeries = new SeriesCollection();
            GoalsAgainstSeries = new SeriesCollection();
            PenaltyMinutesSeries = new SeriesCollection();

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

        private void LoadGoalsScatterSeries()
        {
            GoalsScatterSeries = new SeriesCollection();
            foreach (var gv in GoalsVisuals) { GoalsVisuals.Remove(gv); }
            double maxG = 0.0;
            double maxGA = 0.0;
            double medianG = 0.0;
            double medianGA = 0.0;
            List<double> goals = new List<double>();
            List<double> goalsAgainst = new List<double>();

            foreach (Group g in Groups)
            {
                foreach (Team t in g.Teams)
                {
                    double matches = ((TeamTableStats)t.Stats).GamesPlayed;
                    if (matches < 1.0) { continue; }
                    double goalsPerGame = (double)Math.Round(((TeamTableStats)t.Stats).Goals / matches, 2);
                    goals.Add(goalsPerGame);
                    double goalsAgainstPerGame = (double)Math.Round(((TeamTableStats)t.Stats).GoalsAgainst / matches, 2);
                    goalsAgainst.Add(goalsAgainstPerGame);

                    if (maxG < goalsPerGame) { maxG = goalsPerGame; }
                    if (maxGA < goalsAgainstPerGame) { maxGA = goalsAgainstPerGame; }

                    GoalsScatterSeries.Add(new ScatterSeries
                    {
                        Values = new ChartValues<ScatterPoint> { new ScatterPoint(goalsPerGame, goalsAgainstPerGame, 10) },
                        Title = t.Name,
                        LabelPoint = chartPoint => t.Name,
                        DataLabels = true,
                        FontSize = 16,
                        MaxPointShapeDiameter = 40,
                        MinPointShapeDiameter = 40,
                        Foreground = Brushes.White
                    });

                    //add logo marker
                    BitmapImage logo = new BitmapImage();
                    if (t.LogoPath != "")
                    {
                        logo.BeginInit();
                        logo.UriSource = new Uri(t.LogoPath);
                        logo.EndInit();
                    }

                    GoalsVisuals.Add(new VisualElement
                    {
                        X = goalsPerGame,
                        Y = goalsAgainstPerGame,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,

                        UIElement = new Image
                        {
                            Source = logo,
                            MaxWidth = 50,
                            MaxHeight = 50
                        }
                    });
                }
            }

            //add points to cornes for stretching the view
            GoalsScatterSeries.Add(new ScatterSeries
            {
                Values = new ChartValues<ScatterPoint> { new ScatterPoint(0, 0, 0) },
                Title = null,
                Fill = Brushes.Transparent
            });
            GoalsScatterSeries.Add(new ScatterSeries
            {
                Values = new ChartValues<ScatterPoint> { new ScatterPoint(Math.Round(maxG * 1.1, 2), Math.Round(maxGA * 1.1, 2), 0) },
                Title = null,
                Fill = Brushes.Transparent
            });

            if (goals.Count > 1 && goalsAgainst.Count > 1)
            {
                goals.Sort();
                goalsAgainst.Sort();
                medianG = (goals[(goals.Count / 2) - 1] + goals[goals.Count / 2]) / 2.0;
                medianGA = (goalsAgainst[(goalsAgainst.Count / 2) - 1] + goalsAgainst[goalsAgainst.Count / 2]) / 2.0;
            }

            GoalsYSectionMedian = Math.Round(medianGA, 2);
            GoalsYSectionEnd = Math.Round(maxGA * 1.1, 2);
            GoalsXSectionMedian = Math.Round(medianG, 2);
            GoalsXSectionEnd = Math.Round(maxG * 1.1, 2);

            //labels for sections
            GoalsVisuals.Add(new VisualElement
            {
                X = GoalsXSectionMedian / 2.0,
                Y = GoalsYSectionMedian / 2.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

                UIElement = new TextBlock
                {
                    Text = "Weak",
                    FontWeight = FontWeights.Bold,
                    FontSize = 20,
                    Opacity = 0.6
                }
            });
            GoalsVisuals.Add(new VisualElement
            {
                X = (GoalsXSectionMedian + GoalsXSectionEnd) / 2.0,
                Y = GoalsYSectionMedian / 2.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

                UIElement = new TextBlock
                {
                    Text = "Offensive play",
                    FontWeight = FontWeights.Bold,
                    FontSize = 20,
                    Opacity = 0.6
                }
            });
            GoalsVisuals.Add(new VisualElement
            {
                X = (GoalsXSectionMedian + GoalsXSectionEnd) / 2.0,
                Y = (GoalsYSectionMedian + GoalsYSectionEnd) / 2.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

                UIElement = new TextBlock
                {
                    Text = "Good",
                    FontWeight = FontWeights.Bold,
                    FontSize = 20,
                    Opacity = 0.6
                }
            });
            GoalsVisuals.Add(new VisualElement
            {
                X = GoalsXSectionMedian / 2.0,
                Y = (GoalsYSectionMedian + GoalsYSectionEnd) / 2.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

                UIElement = new TextBlock
                {
                    Text = "Defensive play",
                    FontWeight = FontWeights.Bold,
                    FontSize = 20,
                    Opacity = 0.6
                }
            });
        }

        private void LoadAssistsScatterSeries()
        {
            AssistsScatterSeries = new SeriesCollection();
            foreach (var av in AssistsVisuals) { AssistsVisuals.Remove(av); }
            double maxIG = 0.0;
            double maxA = 0.0;
            double medianIG = 0.0;
            double medianA = 0.0;
            List<double> individualGoals = new List<double>();
            List<double> assists = new List<double>();

            foreach (Group g in Groups)
            {
                foreach (Team t in g.Teams)
                {
                    double matches = ((TeamTableStats)t.Stats).GamesPlayed;
                    if (matches < 1.0) { continue; }
                    double individualGoalsPerGame = (double)Math.Round((((TeamTableStats)t.Stats).Goals - ((TeamTableStats)t.Stats).Assists) / matches, 2);
                    individualGoals.Add(individualGoalsPerGame);
                    double assistsPerGame = (double)Math.Round(((TeamTableStats)t.Stats).Assists / matches, 2);
                    assists.Add(assistsPerGame);

                    if (maxIG < individualGoalsPerGame) { maxIG = individualGoalsPerGame; }
                    if (maxA < assistsPerGame) { maxA = assistsPerGame; }

                    AssistsScatterSeries.Add(new ScatterSeries
                    {
                        Values = new ChartValues<ScatterPoint> { new ScatterPoint(individualGoalsPerGame, assistsPerGame, 10) },
                        Title = t.Name,
                        LabelPoint = chartPoint => t.Name,
                        DataLabels = true,
                        FontSize = 16,
                        MaxPointShapeDiameter = 40,
                        MinPointShapeDiameter = 40,
                        Foreground = Brushes.White
                    });

                    //add logo marker
                    BitmapImage logo = new BitmapImage();
                    if (t.LogoPath != "")
                    {
                        logo.BeginInit();
                        logo.UriSource = new Uri(t.LogoPath);
                        logo.EndInit();
                    }

                    AssistsVisuals.Add(new VisualElement
                    {
                        X = individualGoalsPerGame,
                        Y = assistsPerGame,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,

                        UIElement = new Image
                        {
                            Source = logo,
                            MaxWidth = 50,
                            MaxHeight = 50
                        }
                    });
                }
            }

            //add points to cornes for stretching the view
            AssistsScatterSeries.Add(new ScatterSeries
            {
                Values = new ChartValues<ScatterPoint> { new ScatterPoint(0, 0, 0) },
                Title = null,
                Fill = Brushes.Transparent
            });
            AssistsScatterSeries.Add(new ScatterSeries
            {
                Values = new ChartValues<ScatterPoint> { new ScatterPoint(Math.Round(maxIG * 1.1, 2), Math.Round(maxA * 1.1, 2), 0) },
                Title = null,
                Fill = Brushes.Transparent
            });

            if (individualGoals.Count > 1 && assists.Count > 1)
            {
                individualGoals.Sort();
                assists.Sort();
                medianIG = (individualGoals[(individualGoals.Count / 2) - 1] + individualGoals[individualGoals.Count / 2]) / 2.0;
                medianA = (assists[(assists.Count / 2) - 1] + assists[assists.Count / 2]) / 2.0;
            }

            AssistsYSectionMedian = Math.Round(medianA, 2);
            AssistsYSectionEnd = Math.Round(maxA * 1.1, 2);
            AssistsXSectionMedian = Math.Round(medianIG, 2);
            AssistsXSectionEnd = Math.Round(maxIG * 1.1, 2);

            //labels for sections
            AssistsVisuals.Add(new VisualElement
            {
                X = AssistsXSectionMedian / 2.0,
                Y = AssistsYSectionMedian / 2.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

                UIElement = new TextBlock
                {
                    Text = "Weak scoring",
                    FontWeight = FontWeights.Bold,
                    FontSize = 20,
                    Opacity = 0.6
                }
            });
            AssistsVisuals.Add(new VisualElement
            {
                X = (AssistsXSectionMedian + AssistsXSectionEnd) / 2.0,
                Y = AssistsYSectionMedian / 2.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

                UIElement = new TextBlock
                {
                    Text = "Individual play",
                    FontWeight = FontWeights.Bold,
                    FontSize = 20,
                    Opacity = 0.6
                }
            });
            AssistsVisuals.Add(new VisualElement
            {
                X = (AssistsXSectionMedian + AssistsXSectionEnd) / 2.0,
                Y = (AssistsYSectionMedian + AssistsYSectionEnd) / 2.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

                UIElement = new TextBlock
                {
                    Text = "Scorers",
                    FontWeight = FontWeights.Bold,
                    FontSize = 20,
                    Opacity = 0.6
                }
            });
            AssistsVisuals.Add(new VisualElement
            {
                X = AssistsXSectionMedian / 2.0,
                Y = (AssistsYSectionMedian + AssistsYSectionEnd) / 2.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

                UIElement = new TextBlock
                {
                    Text = "Team play",
                    FontWeight = FontWeights.Bold,
                    FontSize = 20,
                    Opacity = 0.6
                }
            });
        }

        private void LoadPenaltiesScatterSeries()
        {
            PenaltiesScatterSeries = new SeriesCollection();
            foreach (var pv in PenaltiesVisuals) { PenaltiesVisuals.Remove(pv); }
            double maxP = 0.0;
            double maxPA = 0.0;
            double medianP = 0.0;
            double medianPA = 0.0;
            List<double> penalties = new List<double>();
            List<double> penaltiesAgainst = new List<double>();

            foreach (Group g in Groups)
            {
                foreach (Team t in g.Teams)
                {
                    double matches = ((TeamTableStats)t.Stats).GamesPlayed;
                    if (matches < 1.0) { continue; }
                    double penaltyMinutesPerGame = (double)Math.Round(((TeamTableStats)t.Stats).PenaltyMinutes / matches, 2);
                    penalties.Add(penaltyMinutesPerGame);
                    double penlatyMinutesAgainstPerGame = (double)Math.Round(((TeamTableStats)t.Stats).PenaltyMinutesAgainst / matches, 2);
                    penaltiesAgainst.Add(penlatyMinutesAgainstPerGame);

                    if (maxP < penaltyMinutesPerGame) { maxP = penaltyMinutesPerGame; }
                    if (maxPA < penlatyMinutesAgainstPerGame) { maxPA = penlatyMinutesAgainstPerGame; }

                    PenaltiesScatterSeries.Add(new ScatterSeries
                    {
                        Values = new ChartValues<ScatterPoint> { new ScatterPoint(penaltyMinutesPerGame, penlatyMinutesAgainstPerGame, 10) },
                        Title = t.Name,
                        LabelPoint = chartPoint => t.Name,
                        DataLabels = true,
                        FontSize = 16,
                        MaxPointShapeDiameter = 40,
                        MinPointShapeDiameter = 40,
                        Foreground = Brushes.White
                    });

                    //add logo marker
                    BitmapImage logo = new BitmapImage();
                    if (t.LogoPath != "")
                    {
                        logo.BeginInit();
                        logo.UriSource = new Uri(t.LogoPath);
                        logo.EndInit();
                    }

                    PenaltiesVisuals.Add(new VisualElement
                    {
                        X = penaltyMinutesPerGame,
                        Y = penlatyMinutesAgainstPerGame,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,

                        UIElement = new Image
                        {
                            Source = logo,
                            MaxWidth = 50,
                            MaxHeight = 50
                        }
                    });
                }
            }

            //add points to cornes for stretching the view
            PenaltiesScatterSeries.Add(new ScatterSeries
            {
                Values = new ChartValues<ScatterPoint> { new ScatterPoint(0, 0, 0) },
                Title = null,
                Fill = Brushes.Transparent
            });
            PenaltiesScatterSeries.Add(new ScatterSeries
            {
                Values = new ChartValues<ScatterPoint> { new ScatterPoint(Math.Round(maxP * 1.1, 2), Math.Round(maxPA * 1.1, 2), 0) },
                Title = null,
                Fill = Brushes.Transparent
            });

            if (penalties.Count > 1 && penaltiesAgainst.Count > 1)
            {
                penalties.Sort();
                penaltiesAgainst.Sort();
                medianP = (penalties[(penalties.Count / 2) - 1] + penalties[penalties.Count / 2]) / 2.0;
                medianPA = (penaltiesAgainst[(penaltiesAgainst.Count / 2) - 1] + penaltiesAgainst[penaltiesAgainst.Count / 2]) / 2.0;
            }

            PenaltiesYSectionMedian = Math.Round(medianPA, 2);
            PenaltiesYSectionEnd = Math.Round(maxPA * 1.1, 2);
            PenaltiesXSectionMedian = Math.Round(medianP, 2);
            PenaltiesXSectionEnd = Math.Round(maxP * 1.1, 2);

            //labels for sections
            PenaltiesVisuals.Add(new VisualElement
            {
                X = PenaltiesXSectionMedian / 2.0,
                Y = PenaltiesYSectionMedian / 2.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

                UIElement = new TextBlock
                {
                    Text = "Friendly",
                    FontWeight = FontWeights.Bold,
                    FontSize = 20,
                    Opacity = 0.6
                }
            });
            PenaltiesVisuals.Add(new VisualElement
            {
                X = (PenaltiesXSectionMedian + PenaltiesXSectionEnd) / 2.0,
                Y = PenaltiesYSectionMedian / 2.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

                UIElement = new TextBlock
                {
                    Text = "Undisciplined",
                    FontWeight = FontWeights.Bold,
                    FontSize = 20,
                    Opacity = 0.6
                }
            });
            PenaltiesVisuals.Add(new VisualElement
            {
                X = (PenaltiesXSectionMedian + PenaltiesXSectionEnd) / 2.0,
                Y = (PenaltiesYSectionMedian + PenaltiesYSectionEnd) / 2.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

                UIElement = new TextBlock
                {
                    Text = "Rough",
                    FontWeight = FontWeights.Bold,
                    FontSize = 20,
                    Opacity = 0.6
                }
            });
            PenaltiesVisuals.Add(new VisualElement
            {
                X = PenaltiesXSectionMedian / 2.0,
                Y = (PenaltiesYSectionMedian + PenaltiesYSectionEnd) / 2.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

                UIElement = new TextBlock
                {
                    Text = "Disciplined",
                    FontWeight = FontWeights.Bold,
                    FontSize = 20,
                    Opacity = 0.6
                }
            });
        }
    }
}