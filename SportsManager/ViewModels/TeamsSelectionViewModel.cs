using SportsManager.Commands;
using SportsManager.Models;
using SportsManager.Others;
using SportsManager.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace SportsManager.ViewModels
{
    public class TeamsSelectionViewModel : TemplateSelectionDataGridViewModel<Team>
    {
        public TeamsSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateEntityCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new TeamViewModel(navigationStore, SelectedEntity)));
            LoadData();
        }

        protected override void LoadData()
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("", connection);

            #region unused code
            /* old query, query each team and stat individually
            string querry = "SELECT team_id, t.name AS team_name, t.status AS status, t.country AS country, t.date_of_creation AS date_of_creation, " +
                                                "season_id, s.competition_id AS competition_id " +
                                                "FROM team_enlistment " +
                                                "INNER JOIN team AS t ON t.id = team_id " +
                                                "INNER JOIN seasons AS s ON s.id = season_id";
            querry += " WHERE team_id <> -1";
            if (SportsData.competition.id != (int)EntityState.NotSelected && SportsData.competition.id != (int)EntityState.AddNew)
            {
                cmd.CommandText += " AND competition_id = " + SportsData.competition.id;
                if (SportsData.season.id != (int)EntityState.NotSelected && SportsData.season.id != (int)EntityState.AddNew)
                {
                    cmd.CommandText += " AND season_id = " + SportsData.season.id;
                }
            }
            querry += " GROUP BY team_id";
            */
            #endregion

            string homeMatchCountQuery = "SELECT home_competitor, COUNT(*) AS home_match_count " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1";
            homeMatchCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            homeMatchCountQuery += " GROUP BY home_competitor";

            string awayMatchCountQuery = "SELECT away_competitor, COUNT(*) AS away_match_count " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1";
            awayMatchCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            awayMatchCountQuery += " GROUP BY away_competitor";

            string homeWinsQuery = "SELECT home_competitor, COUNT(*) AS home_wins " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND overtime = 0 AND shootout = 0 AND home_score > away_score";
            homeWinsQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            homeWinsQuery += " GROUP BY home_competitor";

            string awayWinsQuery = "SELECT away_competitor, COUNT(*) AS away_wins " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND overtime = 0 AND shootout = 0 AND home_score < away_score";
            awayWinsQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            awayWinsQuery += " GROUP BY away_competitor";

            string homeOTWinsQuery = "SELECT home_competitor, COUNT(*) AS home_ot_wins " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND (overtime = 1 OR shootout = 1) AND home_score > away_score";
            homeOTWinsQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            homeOTWinsQuery += " GROUP BY home_competitor";

            string awayOTWinsQuery = "SELECT away_competitor, COUNT(*) AS away_ot_wins " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND (overtime = 1 OR shootout = 1) AND home_score < away_score";
            awayOTWinsQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            awayOTWinsQuery += " GROUP BY away_competitor";

            string homeTiesQuery = "SELECT home_competitor, COUNT(*) AS home_ties " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND home_score = away_score";
            homeTiesQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            homeTiesQuery += " GROUP BY home_competitor";

            string awayTiesQuery = "SELECT away_competitor, COUNT(*) AS away_ties " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND home_score = away_score";
            awayTiesQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            awayTiesQuery += " GROUP BY away_competitor";

            string homeOTLossesQuery = "SELECT home_competitor, COUNT(*) AS home_ot_losses " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND (overtime = 1 OR shootout = 1) AND home_score < away_score";
            homeOTLossesQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            homeOTLossesQuery += " GROUP BY home_competitor";

            string awayOTLossesQuery = "SELECT away_competitor, COUNT(*) AS away_ot_losses " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND (overtime = 1 OR shootout = 1) AND home_score > away_score";
            awayOTLossesQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            awayOTLossesQuery += " GROUP BY away_competitor";

            string homeLossesQuery = "SELECT home_competitor, COUNT(*) AS home_losses " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND overtime = 0 AND shootout = 0 AND home_score < away_score";
            homeLossesQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            homeLossesQuery += " GROUP BY home_competitor";

            string awayLossesQuery = "SELECT away_competitor, COUNT(*) AS away_losses " +
                                    "FROM matches " +
                                    "INNER JOIN seasons ON seasons.id = season_id " +
                                    "WHERE played = 1 AND overtime = 0 AND shootout = 0 AND home_score > away_score";
            awayLossesQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            awayLossesQuery += " GROUP BY away_competitor";

            string goalCountQuery = "SELECT team_id, COUNT(*) AS goal_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                "WHERE own_goal = 0";
            goalCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            goalCountQuery += " GROUP BY team_id";

            string goalsAgainstCountQuery = "SELECT opponent_team_id, COUNT(*) AS goals_against_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                "WHERE own_goal = 0";
            goalsAgainstCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            goalsAgainstCountQuery += " GROUP BY opponent_team_id";

            string assistCountQuery = "SELECT team_id, COUNT(*) AS assist_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                "WHERE assist_player_id <> -1";
            assistCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            assistCountQuery += " GROUP BY team_id";

            string penaltyMinutesQuery = "SELECT team_id, COALESCE(SUM(penalty_type.minutes), 0) AS penalty_minutes " +
                                                "FROM penalties " +
                                                "INNER JOIN matches ON matches.id = match_id " +
                                                "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                                "INNER JOIN penalty_type ON penalty_type.code = penalty_type_id";
            penaltyMinutesQuery += DatabaseHandler.WhereSeasonCompetitionQuery(true, "matches.season_id", "seasons.competition_id");
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
            if (SportsData.IsCompetitionSet())
            {
                teamStatsQuery += " AND competition_id = " + SportsData.COMPETITION.ID;
                if (SportsData.IsSeasonSet())
                {
                    teamStatsQuery += " AND season_id = " + SportsData.SEASON.ID;
                }
            }
            teamStatsQuery += " GROUP BY t.id";

            cmd.CommandText = teamStatsQuery;

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                Entities = new ObservableCollection<Team>();

                foreach (DataRow row in dataTable.Rows)
                {
                    Team t = new()
                    {
                        ID = int.Parse(row["t_id"].ToString()),
                        Name = row["team_name"].ToString(),
                        Status = bool.Parse(row["status"].ToString()),
                        Country = new Country { CodeTwo = row["country"].ToString() },
                        DateOfCreation = DateTime.Parse(row["date_of_creation"].ToString())
                    };

                    string[] imgPath = System.IO.Directory.GetFiles(SportsData.TeamLogosPath, SportsData.SPORT.Name + t.ID + ".*");
                    if (imgPath.Length != 0)
                    {
                        t.ImagePath = imgPath.First();
                    }

                    //t.Stats = new TeamStats(t);
                    t.Stats = new TeamStats(int.Parse(row["match_count"].ToString()),
                                            int.Parse(row["goal_count"].ToString()),
                                            int.Parse(row["goals_against_count"].ToString()),
                                            int.Parse(row["assist_count"].ToString()),
                                            int.Parse(row["penalty_minutes"].ToString()),
                                            int.Parse(row["wins"].ToString()),
                                            int.Parse(row["ot_wins"].ToString()),
                                            int.Parse(row["ties"].ToString()),
                                            int.Parse(row["ot_losses"].ToString()),
                                            int.Parse(row["losses"].ToString()));

                    Entities.Add(t);
                }
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