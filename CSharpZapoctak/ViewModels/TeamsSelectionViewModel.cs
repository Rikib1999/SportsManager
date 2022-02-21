using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Others;
using CSharpZapoctak.Stores;
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
    class TeamsSelectionViewModel : ViewModelBase
    {
        public class TeamStats : TeamTableStats
        {
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

            public TeamStats(Team t, string status, int gamesPlayed, int goals, int goalsAgainst, int assists, int penaltyMinutes)
            {
                Status = status;
                DateOfCreation = t.DateOfCreation.ToShortDateString();
                GamesPlayed = gamesPlayed;
                Goals = goals;
                GoalsAgainst = goalsAgainst;
                GoalDifference = goals - goalsAgainst;
                Assists = assists;
                PenaltyMinutes = penaltyMinutes;
                Points = goals + assists;
            }

            public TeamStats(Team t, string status)
            {
                Status = status;
                DateOfCreation = t.DateOfCreation.ToShortDateString();
                CalculateStats(t.id).Await();
            }

            public new async Task CalculateStats(int teamID)
            {
                List<Task> tasks = new List<Task>();
                tasks.Add(Task.Run(() => CountMatches(teamID, int.MaxValue)));
                tasks.Add(Task.Run(() => CountGoals(teamID, int.MaxValue)));
                tasks.Add(Task.Run(() => CountAssists(teamID)));
                tasks.Add(Task.Run(() => CountGoalsAgainst(teamID, int.MaxValue)));
                tasks.Add(Task.Run(() => CountPenaltyMinutes(teamID, int.MaxValue)));
                await Task.WhenAll(tasks);
                GoalDifference = Goals - GoalsAgainst;
            }

            private async Task CountAssists(int teamID)
            {
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM goals " +
                                                    "INNER JOIN matches AS m ON m.id = match_id " +
                                                    "WHERE assist_player_id <> -1 AND team_id = " + teamID, connection);
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
        }

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

        public Team SelectedTeam { get; set; }

        public ObservableCollection<Team> Teams { get; set; }

        public TeamsSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateTeamCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new TeamViewModel(navigationStore, SelectedTeam)));
            SelectedTeam = null;

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT team_id, t.name AS team_name, t.status AS status, t.country AS country, t.date_of_creation AS date_of_creation, " +
                                                "season_id, s.competition_id AS competition_id " +
                                                "FROM team_enlistment " +
                                                "INNER JOIN team AS t ON t.id = team_id " +
                                                "INNER JOIN seasons AS s ON s.id = season_id", connection);
            cmd.CommandText += " WHERE team_id <> -1";
            if (SportsData.competition.id != (int)EntityState.NotSelected && SportsData.competition.id != (int)EntityState.AddNew)
            {
                cmd.CommandText += " AND competition_id = " + SportsData.competition.id;
                if (SportsData.season.id != (int)EntityState.NotSelected && SportsData.season.id != (int)EntityState.AddNew)
                {
                    cmd.CommandText += " AND season_id = " + SportsData.season.id;
                }
            }
            cmd.CommandText += " GROUP BY team_id";

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

            //TODO: wins, otwins... points
            string winsQuery = "SELECT team_id, COUNT(*) AS wins " +                                                                          //HOME AWAY
                                    "FROM matches " +                                                                                         //HOME AWAY
                                    "INNER JOIN seasons ON seasons.id = season_id " +                                                 //HOME AWAY
                                    "WHERE played = 1 AND team_id";                                                                           //HOME AWAY
            if (SportsData.season.id > 0) { winsQuery += " AND matches.season_id = " + SportsData.season.id; }                                //HOME AWAY
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { winsQuery += " AND"; }                                           //HOME AWAY
            if (SportsData.competition.id > 0) { winsQuery += " seasons.competition_id = " + SportsData.competition.id; }                     //HOME AWAY
            winsQuery += " GROUP BY team_id";                                                                                                 //HOME AWAY

            string goalCountQuery = "SELECT team_id, COUNT(*) AS goal_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id";
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { goalCountQuery += " WHERE"; }
            if (SportsData.season.id > 0) { goalCountQuery += " matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { goalCountQuery += " AND"; }
            if (SportsData.competition.id > 0) { goalCountQuery += " seasons.competition_id = " + SportsData.competition.id; }
            goalCountQuery += " GROUP BY team_id";

            string goalsAgainstCountQuery = "SELECT opponent_team_id, COUNT(*) AS goals_against_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id";
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { goalsAgainstCountQuery += " WHERE"; }
            if (SportsData.season.id > 0) { goalsAgainstCountQuery += " matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { goalsAgainstCountQuery += " AND"; }
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
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { penaltyMinutesQuery += " AND"; }
            if (SportsData.competition.id > 0) { penaltyMinutesQuery += " seasons.competition_id = " + SportsData.competition.id; }
            penaltyMinutesQuery += " GROUP BY team_id";

            string teamStatsQuery = "SELECT team_enlistment.team_id AS t_id, t.name AS team_name, t.status AS status, t.country AS country, t.date_of_creation AS date_of_creation, " +
                                            "IFNULL(home_match_count, 0) + IFNULL(away_match_count, 0) AS match_count, " +
                                            "IFNULL(goal_count, 0) AS goal_count, " +
                                            "IFNULL(goals_against_count, 0) AS goals_against_count, " +
                                            "IFNULL(assist_count, 0) AS assist_count, " +
                                            "IFNULL(penalty_minutes, 0) AS penalty_minutes " +
                                "FROM team_enlistment " +
                                "RIGHT JOIN team AS t ON t.id = team_enlistment.team_id " +
                                "INNER JOIN seasons AS s ON s.id = season_id";

            teamStatsQuery += " LEFT JOIN (" + homeMatchCountQuery + ") AS hm ON hm.home_competitor = t.id " +
                                "LEFT JOIN (" + awayMatchCountQuery + ") AS am ON am.away_competitor = t.id " +
                                "LEFT JOIN (" + goalCountQuery + ") AS g ON g.team_id = t.id " +
                                "LEFT JOIN (" + goalsAgainstCountQuery + ") AS ga ON ga.opponent_team_id = t.id " +
                                "LEFT JOIN (" + assistCountQuery + ") AS a ON a.team_id = t.id " +
                                "LEFT JOIN (" + penaltyMinutesQuery + ") AS pm ON pm.team_id = t.id";

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
                                                       int.Parse(row["penalty_minutes"].ToString()));/*,
                                                       int.Parse(row["wins"].ToString()),
                                                       int.Parse(row["ot_wins"].ToString()),
                                                       int.Parse(row["ties"].ToString()),
                                                       int.Parse(row["loses"].ToString()),
                                                       int.Parse(row["ot_loses"].ToString()));*/

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