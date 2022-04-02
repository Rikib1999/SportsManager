using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Others;
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
    class TeamsSelectionViewModel : NotifyPropertyChanged
    {
        public class TeamStats : NotifyPropertyChanged, IStats
        {
            #region Properties
            private string dateOfCreation;
            public string DateOfCreation
            {
                get { return dateOfCreation; }
                set
                {
                    dateOfCreation = value;
                    OnPropertyChanged();
                }
            }

            private string status;
            public string Status
            {
                get { return status; }
                set
                {
                    status = value;
                    OnPropertyChanged();
                }
            }

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

            public TeamStats(Team t, string status, int gamesPlayed, int goals, int goalsAgainst, int assists, int penaltyMinutes, int wins, int winsOT, int ties, int lossesOT, int losses)
            {
                Status = status;
                DateOfCreation = t.DateOfCreation.ToShortDateString();
                GamesPlayed = gamesPlayed;
                Goals = goals;
                GoalsAgainst = goalsAgainst;
                GoalDifference = goals - goalsAgainst;
                Assists = assists;
                PenaltyMinutes = penaltyMinutes;
                Wins = wins;
                WinsOT = winsOT;
                Ties = ties;
                LossesOT = lossesOT;
                Losses = losses;
            }
        }

        #region Commands
        public ICommand NavigateTeamCommand { get; set; }

        private ICommand checkNavigateTeamCommand;
        public ICommand CheckNavigateTeamCommand
        {
            get
            {
                if (checkNavigateTeamCommand == null)
                {
                    checkNavigateTeamCommand = new RelayCommand(param => CheckNavigateTeam());
                }
                return checkNavigateTeamCommand;
            }
        }

        private ICommand exportPDFCommand;
        public ICommand ExportPDFCommand
        {
            get
            {
                if (exportPDFCommand == null)
                {
                    exportPDFCommand = new RelayCommand(param => Exports.ExportTable((System.Windows.Controls.DataGrid)param, "PDF", ExportTop));
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
                    exportXLSXCommand = new RelayCommand(param => Exports.ExportTable((System.Windows.Controls.DataGrid)param, "XLSX", ExportTop));
                }
                return exportXLSXCommand;
            }
        }
        #endregion

        private int? exportTop;
        public int? ExportTop
        {
            get { return exportTop; }
            set
            {
                exportTop = value;
                OnPropertyChanged();
            }
        }

        #region Visibilities
        private bool showPhoto = true;
        public bool ShowPhoto
        {
            get { return showPhoto; }
            set
            {
                showPhoto = value;
                PhotoVisibility = showPhoto ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

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

        private Visibility photoVisibility = Visibility.Visible;
        public Visibility PhotoVisibility
        {
            get { return photoVisibility; }
            set
            {
                photoVisibility = value;
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

        public Team SelectedTeam { get; set; }

        public ObservableCollection<Team> Teams { get; set; }

        public TeamsSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateTeamCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new TeamViewModel(navigationStore, SelectedTeam)));
            SelectedTeam = null;

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("", connection);

            //string querry = "SELECT team_id, t.name AS team_name, t.status AS status, t.country AS country, t.date_of_creation AS date_of_creation, " +
            //                                    "season_id, s.competition_id AS competition_id " +
            //                                    "FROM team_enlistment " +
            //                                    "INNER JOIN team AS t ON t.id = team_id " +
            //                                    "INNER JOIN seasons AS s ON s.id = season_id";
            //querry += " WHERE team_id <> -1";
            //if (SportsData.competition.id != (int)EntityState.NotSelected && SportsData.competition.id != (int)EntityState.AddNew)
            //{
            //    cmd.CommandText += " AND competition_id = " + SportsData.competition.id;
            //    if (SportsData.season.id != (int)EntityState.NotSelected && SportsData.season.id != (int)EntityState.AddNew)
            //    {
            //        cmd.CommandText += " AND season_id = " + SportsData.season.id;
            //    }
            //}
            //querry += " GROUP BY team_id";

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            string homeMatchCountQuery = "SELECT home_competitor, COUNT(*) AS home_match_count " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1";
            if (SportsData.season.id > 0) { homeMatchCountQuery += " AND matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { homeMatchCountQuery += " AND"; }
            if (SportsData.competition.id > 0) { homeMatchCountQuery += " seasons.competition_id = " + SportsData.competition.id; }
            homeMatchCountQuery += " GROUP BY home_competitor";

            string awayMatchCountQuery = "SELECT away_competitor, COUNT(*) AS away_match_count " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1";
            if (SportsData.season.id > 0) { awayMatchCountQuery += " AND matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { awayMatchCountQuery += " AND"; }
            if (SportsData.competition.id > 0) { awayMatchCountQuery += " seasons.competition_id = " + SportsData.competition.id; }
            awayMatchCountQuery += " GROUP BY away_competitor";

            string homeWinsQuery = "SELECT home_competitor, COUNT(*) AS home_wins " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND overtime = 0 AND shootout = 0 AND home_score > away_score";
            if (SportsData.season.id > 0) { homeWinsQuery += " AND matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { homeWinsQuery += " AND"; }
            if (SportsData.competition.id > 0) { homeWinsQuery += " seasons.competition_id = " + SportsData.competition.id; }
            homeWinsQuery += " GROUP BY home_competitor";

            string awayWinsQuery = "SELECT away_competitor, COUNT(*) AS away_wins " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND overtime = 0 AND shootout = 0 AND home_score < away_score";
            if (SportsData.season.id > 0) { awayWinsQuery += " AND matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { awayWinsQuery += " AND"; }
            if (SportsData.competition.id > 0) { awayWinsQuery += " seasons.competition_id = " + SportsData.competition.id; }
            awayWinsQuery += " GROUP BY away_competitor";

            string homeOTWinsQuery = "SELECT home_competitor, COUNT(*) AS home_ot_wins " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND (overtime = 1 OR shootout = 1) AND home_score > away_score";
            if (SportsData.season.id > 0) { homeOTWinsQuery += " AND matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { homeOTWinsQuery += " AND"; }
            if (SportsData.competition.id > 0) { homeOTWinsQuery += " seasons.competition_id = " + SportsData.competition.id; }
            homeOTWinsQuery += " GROUP BY home_competitor";

            string awayOTWinsQuery = "SELECT away_competitor, COUNT(*) AS away_ot_wins " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND (overtime = 1 OR shootout = 1) AND home_score < away_score";
            if (SportsData.season.id > 0) { awayOTWinsQuery += " AND matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { awayOTWinsQuery += " AND"; }
            if (SportsData.competition.id > 0) { awayOTWinsQuery += " seasons.competition_id = " + SportsData.competition.id; }
            awayOTWinsQuery += " GROUP BY away_competitor";

            string homeTiesQuery = "SELECT home_competitor, COUNT(*) AS home_ties " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND home_score = away_score";
            if (SportsData.season.id > 0) { homeTiesQuery += " AND matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { homeTiesQuery += " AND"; }
            if (SportsData.competition.id > 0) { homeTiesQuery += " seasons.competition_id = " + SportsData.competition.id; }
            homeTiesQuery += " GROUP BY home_competitor";

            string awayTiesQuery = "SELECT away_competitor, COUNT(*) AS away_ties " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND home_score = away_score";
            if (SportsData.season.id > 0) { awayTiesQuery += " AND matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { awayTiesQuery += " AND"; }
            if (SportsData.competition.id > 0) { awayTiesQuery += " seasons.competition_id = " + SportsData.competition.id; }
            awayTiesQuery += " GROUP BY away_competitor";

            string homeOTLossesQuery = "SELECT home_competitor, COUNT(*) AS home_ot_losses " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND (overtime = 1 OR shootout = 1) AND home_score < away_score";
            if (SportsData.season.id > 0) { homeOTLossesQuery += " AND matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { homeOTLossesQuery += " AND"; }
            if (SportsData.competition.id > 0) { homeOTLossesQuery += " seasons.competition_id = " + SportsData.competition.id; }
            homeOTLossesQuery += " GROUP BY home_competitor";

            string awayOTLossesQuery = "SELECT away_competitor, COUNT(*) AS away_ot_losses " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND (overtime = 1 OR shootout = 1) AND home_score > away_score";
            if (SportsData.season.id > 0) { awayOTLossesQuery += " AND matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { awayOTLossesQuery += " AND"; }
            if (SportsData.competition.id > 0) { awayOTLossesQuery += " seasons.competition_id = " + SportsData.competition.id; }
            awayOTLossesQuery += " GROUP BY away_competitor";

            string homeLossesQuery = "SELECT home_competitor, COUNT(*) AS home_losses " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND overtime = 0 AND shootout = 0 AND home_score < away_score";
            if (SportsData.season.id > 0) { homeLossesQuery += " AND matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { homeLossesQuery += " AND"; }
            if (SportsData.competition.id > 0) { homeLossesQuery += " seasons.competition_id = " + SportsData.competition.id; }
            homeLossesQuery += " GROUP BY home_competitor";

            string awayLossesQuery = "SELECT away_competitor, COUNT(*) AS away_losses " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND overtime = 0 AND shootout = 0 AND home_score > away_score";
            if (SportsData.season.id > 0) { awayLossesQuery += " AND matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { awayLossesQuery += " AND"; }
            if (SportsData.competition.id > 0) { awayLossesQuery += " seasons.competition_id = " + SportsData.competition.id; }
            awayLossesQuery += " GROUP BY away_competitor";

            string goalCountQuery = "SELECT team_id, COUNT(*) AS goal_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id";
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { goalCountQuery += " WHERE"; }
            if (SportsData.season.id > 0) { goalCountQuery += " matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 && SportsData.competition.id > 0) { goalCountQuery += " AND"; }
            if (SportsData.competition.id > 0) { goalCountQuery += " seasons.competition_id = " + SportsData.competition.id; }
            goalCountQuery += " GROUP BY team_id";

            string goalsAgainstCountQuery = "SELECT opponent_team_id, COUNT(*) AS goals_against_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id";
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { goalsAgainstCountQuery += " WHERE"; }
            if (SportsData.season.id > 0) { goalsAgainstCountQuery += " matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 && SportsData.competition.id > 0) { goalsAgainstCountQuery += " AND"; }
            if (SportsData.competition.id > 0) { goalsAgainstCountQuery += " seasons.competition_id = " + SportsData.competition.id; }
            goalsAgainstCountQuery += " GROUP BY opponent_team_id";

            string assistCountQuery = "SELECT team_id, COUNT(*) AS assist_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                "WHERE assist_player_id <> -1";
            if (SportsData.season.id > 0) { assistCountQuery += " AND matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { assistCountQuery += " AND"; }
            if (SportsData.competition.id > 0) { assistCountQuery += " seasons.competition_id = " + SportsData.competition.id; }
            assistCountQuery += " GROUP BY team_id";

            string penaltyMinutesQuery = "SELECT team_id, COALESCE(SUM(penalty_type.minutes), 0) AS penalty_minutes " +
                                                "FROM penalties " +
                                                "INNER JOIN matches ON matches.id = match_id " +
                                                "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                                "INNER JOIN penalty_type ON penalty_type.code = penalty_type_id";
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { penaltyMinutesQuery += " WHERE"; }
            if (SportsData.season.id > 0) { penaltyMinutesQuery += " matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 && SportsData.competition.id > 0) { penaltyMinutesQuery += " AND"; }
            if (SportsData.competition.id > 0) { penaltyMinutesQuery += " seasons.competition_id = " + SportsData.competition.id; }
            penaltyMinutesQuery += " GROUP BY team_id";

            string teamStatsQuery = "SELECT team_enlistment.team_id AS t_id, t.name AS team_name, t.status AS status, t.country AS country, t.date_of_creation AS date_of_creation, " +
                                            "IFNULL(home_match_count, 0) + IFNULL(away_match_count, 0) AS match_count, " +
                                            "IFNULL(goal_count, 0) AS goal_count, " +
                                            "IFNULL(goals_against_count, 0) AS goals_against_count, " +
                                            "IFNULL(assist_count, 0) AS assist_count, " +
                                            "IFNULL(penalty_minutes, 0) AS penalty_minutes, " +
                                            "IFNULL(home_wins, 0) + IFNULL(away_wins, 0) AS wins, " +
                                            "IFNULL(home_ot_wins, 0) + IFNULL(away_ot_wins, 0) AS ot_wins, " +
                                            "IFNULL(home_ties, 0) + IFNULL(away_ties, 0) AS ties, " +
                                            "IFNULL(home_ot_losses, 0) + IFNULL(away_ot_losses, 0) AS ot_losses, " +
                                            "IFNULL(home_losses, 0) + IFNULL(away_losses, 0) AS losses " +
                                "FROM team_enlistment " +
                                "RIGHT JOIN team AS t ON t.id = team_enlistment.team_id " +
                                "INNER JOIN seasons AS s ON s.id = season_id";

            teamStatsQuery += " LEFT JOIN (" + homeMatchCountQuery + ") AS hm ON hm.home_competitor = t.id " +
                                "LEFT JOIN (" + awayMatchCountQuery + ") AS am ON am.away_competitor = t.id " +
                                "LEFT JOIN (" + goalCountQuery + ") AS g ON g.team_id = t.id " +
                                "LEFT JOIN (" + goalsAgainstCountQuery + ") AS ga ON ga.opponent_team_id = t.id " +
                                "LEFT JOIN (" + assistCountQuery + ") AS a ON a.team_id = t.id " +
                                "LEFT JOIN (" + penaltyMinutesQuery + ") AS pm ON pm.team_id = t.id " +
                                "LEFT JOIN (" + homeWinsQuery + ") AS hw ON hw.home_competitor = t.id " +
                                "LEFT JOIN (" + awayWinsQuery + ") AS aw ON aw.away_competitor = t.id " +
                                "LEFT JOIN (" + homeOTWinsQuery + ") AS hotw ON hotw.home_competitor = t.id " +
                                "LEFT JOIN (" + awayOTWinsQuery + ") AS aotw ON aotw.away_competitor = t.id " +
                                "LEFT JOIN (" + homeTiesQuery + ") AS ht ON ht.home_competitor = t.id " +
                                "LEFT JOIN (" + awayTiesQuery + ") AS at ON at.away_competitor = t.id " +
                                "LEFT JOIN (" + homeOTLossesQuery + ") AS hotl ON hotl.home_competitor = t.id " +
                                "LEFT JOIN (" + awayOTLossesQuery + ") AS aotl ON aotl.away_competitor = t.id " +
                                "LEFT JOIN (" + homeLossesQuery + ") AS hl ON hl.home_competitor = t.id " +
                                "LEFT JOIN (" + awayLossesQuery + ") AS al ON al.away_competitor = t.id";

            teamStatsQuery += " WHERE t.id <> -1";
            if (SportsData.competition.id != (int)EntityState.NotSelected && SportsData.competition.id != (int)EntityState.AddNew)
            {
                teamStatsQuery += " AND competition_id = " + SportsData.competition.id;
                if (SportsData.season.id != (int)EntityState.NotSelected && SportsData.season.id != (int)EntityState.AddNew)
                {
                    teamStatsQuery += " AND season_id = " + SportsData.season.id;
                }
            }
            teamStatsQuery += " GROUP BY t.id";

            cmd.CommandText = teamStatsQuery;
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                Teams = new ObservableCollection<Team>();

                foreach (DataRow row in dataTable.Rows)
                {
                    Team t = new Team
                    {
                        id = int.Parse(row["t_id"].ToString()),
                        Name = row["team_name"].ToString(),
                        Status = bool.Parse(row["status"].ToString()),
                        Country = new Country { CodeTwo = row["country"].ToString() },
                        DateOfCreation = DateTime.Parse(row["date_of_creation"].ToString())
                    };

                    string[] imgPath = System.IO.Directory.GetFiles(SportsData.TeamLogosPath, SportsData.sport.name + t.id + ".*");
                    if (imgPath.Length != 0)
                    {
                        t.LogoPath = imgPath.First();
                    }

                    string status = "inactive";
                    if (t.Status) { status = "active"; }

                    //t.Stats = new TeamStats(t, status);
                    t.Stats = new TeamStats(t, status, int.Parse(row["match_count"].ToString()),
                                                       int.Parse(row["goal_count"].ToString()),
                                                       int.Parse(row["goals_against_count"].ToString()),
                                                       int.Parse(row["assist_count"].ToString()),
                                                       int.Parse(row["penalty_minutes"].ToString()),
                                                       int.Parse(row["wins"].ToString()),
                                                       int.Parse(row["ot_wins"].ToString()),
                                                       int.Parse(row["ties"].ToString()),
                                                       int.Parse(row["ot_losses"].ToString()),
                                                       int.Parse(row["losses"].ToString()));

                    Teams.Add(t);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to connect to databse."+e.Message, "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        private void CheckNavigateTeam()
        {
            if (SelectedTeam != null)
            {
                NavigateTeamCommand.Execute(null);
            }
        }
    }
}