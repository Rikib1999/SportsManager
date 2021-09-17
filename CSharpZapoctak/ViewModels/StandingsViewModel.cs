using CSharpZapoctak.Models;
using CSharpZapoctak.Others;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows;

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

        public TeamTableStats(Team t)
        {
            CalculateStats(t.id).Await();
        }

        public async Task CalculateStats(int teamID)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Run(() => CountMatches(teamID)));
            tasks.Add(Task.Run(() => CountGoals(teamID)));
            tasks.Add(Task.Run(() => CountGoalsAgainst(teamID)));
            tasks.Add(Task.Run(() => CountPenaltyMinutes(teamID)));
            await Task.WhenAll(tasks);
            GoalDifference = Goals - GoalsAgainst;
        }

        private async Task CountMatches(int teamID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT home_competitor, away_competitor, home_score, away_score, overtime, shootout " +
                                                "FROM matches " +
                                                "WHERE (home_competitor = " + teamID + " OR away_competitor = " + teamID + ") AND played = 1 " +
                                                "AND qualification_id = -1 AND serie_match_number = -1 AND season_id = " + SportsData.season.id, connection);
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
                                Points += (int)SportsData.season.PointsForOTWin;
                                WinsOT++;
                            }
                            else
                            {
                                Points += (int)SportsData.season.PointsForWin;
                                Wins++;
                            }
                        }
                        else if (homeScore < awayScore)
                        {
                            if (overtime || shootout)
                            {
                                Points += (int)SportsData.season.PointsForOTLoss;
                                LossesOT++;
                            }
                            else
                            {
                                Points += (int)SportsData.season.PointsForLoss;
                                Losses++;
                            }
                        }
                        else
                        {
                            Points += (int)SportsData.season.PointsForTie;
                            Ties++;
                        }
                    }
                    else
                    {
                        if (homeScore > awayScore)
                        {
                            if (overtime || shootout)
                            {
                                Points += (int)SportsData.season.PointsForOTLoss;
                                LossesOT++;
                            }
                            else
                            {
                                Points += (int)SportsData.season.PointsForLoss;
                                Losses++;
                            }
                        }
                        else if (homeScore < awayScore)
                        {
                            if (overtime || shootout)
                            {
                                Points += (int)SportsData.season.PointsForOTWin;
                                WinsOT++;
                            }
                            else
                            {
                                Points += (int)SportsData.season.PointsForWin;
                                Wins++;
                            }
                        }
                        else
                        {
                            Points += (int)SportsData.season.PointsForTie;
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

        private async Task CountGoals(int teamID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM goals " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "WHERE m.season_id = " + SportsData.season.id + " AND team_id = " + teamID + " AND m.qualification_id = -1 AND m.serie_match_number = -1", connection);
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

        private async Task CountGoalsAgainst(int teamID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM goals " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "WHERE m.season_id = " + SportsData.season.id + " AND opponent_team_id = " + teamID + " AND m.qualification_id = -1 AND m.serie_match_number = -1", connection);
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

        private async Task CountPenaltyMinutes(int teamID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT SUM(p.minutes) FROM penalties " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN penalty_type AS p ON p.code = penalty_type_id " +
                                                "WHERE m.season_id = " + SportsData.season.id + " AND team_id = " + teamID + " AND m.qualification_id = -1 AND m.serie_match_number = -1", connection);
            try
            {
                connection.Open();
                PenaltyMinutes = (int)(long)await cmd.ExecuteScalarAsync();
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

        /*
            ORDERING RULES:
                1.   points
                2.   points in H2H matches
                3.   score in H2H matches
                4.   goals  in H2H matches
                5.   penalty minutes
                6.   random
        */
        public int CompareTo(TeamTableStats other, bool onlyPoints)
        {
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
        private ObservableCollection<Team> teams = new ObservableCollection<Team>();
        public ObservableCollection<Team> Teams
        {
            get { return teams; }
            set
            {
                teams = value;
                OnPropertyChanged();
            }
        }

        public StandingsViewModel()
        {

        }
    }
}
