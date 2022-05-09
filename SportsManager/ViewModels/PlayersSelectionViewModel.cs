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
    /// <summary>
    /// Viewmodel for viewing all players and theirs statistics in one sortable table with filtering. Serves for navigating to a player detail viewmodel.
    /// </summary>
    public class PlayersSelectionViewModel : TemplateSelectionDataGridViewModel<Player>
    {
        /// <summary>
        /// Instantiates new PlayersSelectionViewModel.
        /// </summary>
        /// <param name="navigationStore">Current instance of NavigationStore.</param>
        public PlayersSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateEntityCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new PlayerViewModel(navigationStore, SelectedEntity)));
            LoadData();
        }

        /// <summary>
        /// Loads the players with their statistics from the database.
        /// </summary>
        protected override void LoadData()
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("", connection);

            string matchCountQuery = "SELECT player_id, COUNT(*) AS match_count " +
                                    "FROM player_matches " +
                                    "INNER JOIN matches ON matches.id = match_id " +
                                    "INNER JOIN seasons ON seasons.id = matches.season_id";
            matchCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(true, "matches.season_id", "seasons.competition_id");
            matchCountQuery += " GROUP BY player_id";

            string goalCountQuery = "SELECT player_id, COUNT(*) AS goal_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                "WHERE own_goal = 0";
            goalCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            goalCountQuery += " GROUP BY player_id";

            string assistCountQuery = "SELECT assist_player_id, COUNT(*) AS assist_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id";
            assistCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(true, "matches.season_id", "seasons.competition_id");
            assistCountQuery += " GROUP BY assist_player_id";

            string ppGoalsCountQuery = "SELECT player_id, COUNT(*) AS pp_goal_count " +
                    "FROM goals " +
                    "INNER JOIN matches ON matches.id = match_id " +
                    "INNER JOIN seasons ON seasons.id = matches.season_id " +
                    "INNER JOIN strength ON strength.id = strength_id " +
                    "WHERE STRCMP(strength.advantage, 'PP') = 0 AND own_goal = 0";
            ppGoalsCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            ppGoalsCountQuery += " GROUP BY player_id";

            string shGoalsCountQuery = "SELECT player_id, COUNT(*) AS sh_goal_count " +
                    "FROM goals " +
                    "INNER JOIN matches ON matches.id = match_id " +
                    "INNER JOIN seasons ON seasons.id = matches.season_id " +
                    "INNER JOIN strength ON strength.id = strength_id " +
                    "WHERE STRCMP(strength.advantage, 'SH') = 0 AND own_goal = 0";
            shGoalsCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            shGoalsCountQuery += " GROUP BY player_id";

            string evGoalsCountQuery = "SELECT player_id, COUNT(*) AS ev_goal_count " +
                    "FROM goals " +
                    "INNER JOIN matches ON matches.id = match_id " +
                    "INNER JOIN seasons ON seasons.id = matches.season_id " +
                    "INNER JOIN strength ON strength.id = strength_id " +
                    "WHERE STRCMP(strength.advantage, 'EV') = 0 AND own_goal = 0";
            evGoalsCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            evGoalsCountQuery += " GROUP BY player_id";

            string penaltyMinutesQuery = "SELECT player_id, COALESCE(SUM(penalty_type.minutes), 0) AS penalty_minutes " +
                                                "FROM penalties " +
                                                "INNER JOIN matches ON matches.id = match_id " +
                                                "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                                "INNER JOIN penalty_type ON penalty_type.code = penalty_type_id";
            penaltyMinutesQuery += DatabaseHandler.WhereSeasonCompetitionQuery(true, "matches.season_id", "seasons.competition_id");
            penaltyMinutesQuery += " GROUP BY player_id";

            string penaltyShotCountQuery = "SELECT player_id, COUNT(*) AS penalty_shot_count " +
                    "FROM penalty_shots " +
                    "INNER JOIN matches ON matches.id = match_id " +
                    "INNER JOIN seasons ON seasons.id = matches.season_id";
            penaltyShotCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(true, "matches.season_id", "seasons.competition_id");
            penaltyShotCountQuery += " GROUP BY player_id";

            string penaltyShotGoalCountQuery = "SELECT player_id, COUNT(*) AS penalty_shot_goal_count " +
                    "FROM penalty_shots " +
                    "INNER JOIN matches ON matches.id = match_id " +
                    "INNER JOIN seasons ON seasons.id = matches.season_id " +
                    "WHERE was_goal = 1";
            penaltyShotGoalCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            penaltyShotGoalCountQuery += " GROUP BY player_id";

            string emptyNetGoalCountQuery = "SELECT player_id, COUNT(*) AS empty_net_goal_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                "WHERE empty_net = 1";
            emptyNetGoalCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            emptyNetGoalCountQuery += " GROUP BY player_id";

            string delayedPenaltyGoalCountQuery = "SELECT player_id, COUNT(*) AS delayed_penalty_goal_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                "WHERE delayed_penalty = 1";
            delayedPenaltyGoalCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            delayedPenaltyGoalCountQuery += " GROUP BY player_id";

            string ownGoalCountQuery = "SELECT player_id, COUNT(*) AS own_goal_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                "WHERE own_goal = 1";
            ownGoalCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            ownGoalCountQuery += " GROUP BY player_id";

            string gwGoalCountQuery = "SELECT player_id, COUNT(*) AS gwg_goal_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                "WHERE gwg_glog = 1";
            gwGoalCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            gwGoalCountQuery += " GROUP BY player_id";

            string goalWithoutAssistCountQuery = "SELECT player_id, COUNT(*) AS goal_without_assist_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                "WHERE assist_player_id = -1 AND own_goal = 0 AND penalty_shot = 0";
            goalWithoutAssistCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            goalWithoutAssistCountQuery += " GROUP BY player_id";

            string playerStatsQuery = "SELECT p.*, s.competition_id, " +
                                              "IFNULL(match_count, 0) AS match_count, " +
                                              "IFNULL(goal_count, 0) AS goal_count, " +
                                              "IFNULL(assist_count, 0) AS assist_count, " +
                                              "IFNULL(pp_goal_count, 0) AS pp_goal_count, " +
                                              "IFNULL(sh_goal_count, 0) AS sh_goal_count, " +
                                              "IFNULL(ev_goal_count, 0) AS ev_goal_count, " +
                                              "IFNULL(penalty_minutes, 0) AS penalty_minutes, " +
                                              "IFNULL(penalty_shot_count, 0) AS penalty_shot_count, " +
                                              "IFNULL(penalty_shot_goal_count, 0) AS penalty_shot_goal_count, " +
                                              "IFNULL(empty_net_goal_count, 0) AS empty_net_goal_count, " +
                                              "IFNULL(delayed_penalty_goal_count, 0) AS delayed_penalty_goal_count, " +
                                              "IFNULL(own_goal_count, 0) AS own_goal_count, " +
                                              "IFNULL(gwg_goal_count, 0) AS gwg_goal_count, " +
                                              "IFNULL(goal_without_assist_count, 0) AS goal_without_assist_count " +
                                      "FROM player_enlistment " +
                                      "RIGHT JOIN player AS p ON p.id = player_id " +
                                      "INNER JOIN seasons AS s ON s.id = season_id ";

            playerStatsQuery += "LEFT JOIN (" + matchCountQuery + ") AS m ON m.player_id = p.id " +
                                "LEFT JOIN (" + goalCountQuery + ") AS g ON g.player_id = p.id " +
                                "LEFT JOIN (" + assistCountQuery + ") AS a ON a.assist_player_id = p.id " +
                                "LEFT JOIN (" + ppGoalsCountQuery + ") AS ppg ON ppg.player_id = p.id " +
                                "LEFT JOIN (" + shGoalsCountQuery + ") AS shg ON shg.player_id = p.id " +
                                "LEFT JOIN (" + evGoalsCountQuery + ") AS evg ON evg.player_id = p.id " +
                                "LEFT JOIN (" + penaltyMinutesQuery + ") AS pm ON pm.player_id = p.id " +
                                "LEFT JOIN (" + penaltyShotCountQuery + ") AS ps ON ps.player_id = p.id " +
                                "LEFT JOIN (" + penaltyShotGoalCountQuery + ") AS psg ON psg.player_id = p.id " +
                                "LEFT JOIN (" + emptyNetGoalCountQuery + ") AS eng ON eng.player_id = p.id " +
                                "LEFT JOIN (" + delayedPenaltyGoalCountQuery + ") AS dpg ON dpg.player_id = p.id " +
                                "LEFT JOIN (" + ownGoalCountQuery + ") AS og ON og.player_id = p.id " +
                                "LEFT JOIN (" + gwGoalCountQuery + ") AS gwg ON gwg.player_id = p.id " +
                                "LEFT JOIN (" + goalWithoutAssistCountQuery + ") AS gwoa ON gwoa.player_id = p.id";

            playerStatsQuery += " WHERE p.id <> -1";
            if (SportsData.IsCompetitionSet())
            {
                playerStatsQuery += " AND s.competition_id = " + SportsData.COMPETITION.ID;
                if (SportsData.IsSeasonSet())
                {
                    playerStatsQuery += " AND season_id = " + SportsData.SEASON.ID;
                }
            }
            playerStatsQuery += " GROUP BY p.id";

            cmd.CommandText = playerStatsQuery;

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                Entities = new ObservableCollection<Player>();

                foreach (DataRow row in dataTable.Rows)
                {
                    Player p = new()
                    {
                        ID = int.Parse(row["id"].ToString()),
                        FirstName = row["first_name"].ToString(),
                        LastName = row["last_name"].ToString(),
                        Birthdate = DateTime.Parse(row["birthdate"].ToString()),
                        Gender = row["gender"].ToString(),
                        Height = int.Parse(row["height"].ToString()),
                        Weight = int.Parse(row["weight"].ToString()),
                        PlaysWith = row["plays_with"].ToString(),
                        Citizenship = SportsData.Countries.First(x => x.CodeTwo == row["citizenship"].ToString()),
                        BirthplaceCity = row["birthplace_city"].ToString(),
                        BirthplaceCountry = SportsData.Countries.First(x => x.CodeTwo == row["birthplace_country"].ToString()),
                        Status = Convert.ToBoolean(int.Parse(row["status"].ToString())),
                        Info = row["info"].ToString()
                    };

                    string[] imgPath = System.IO.Directory.GetFiles(SportsData.PlayerPhotosPath, SportsData.SPORT.Name + p.ID + ".*");
                    p.ImagePath = imgPath.Length != 0
                        ? imgPath.First()
                        : p.Gender == "M" ? SportsData.ResourcesPath + "\\male.png" : SportsData.ResourcesPath + "\\female.png";

                    //p.Stats = new PlayerStats(p, SportsData.season.id, SportsData.competition.id);
                    p.Stats = new PlayerStats(int.Parse(row["match_count"].ToString()),
                                              int.Parse(row["goal_count"].ToString()),
                                              int.Parse(row["assist_count"].ToString()),
                                              int.Parse(row["pp_goal_count"].ToString()),
                                              int.Parse(row["sh_goal_count"].ToString()),
                                              int.Parse(row["ev_goal_count"].ToString()),
                                              int.Parse(row["penalty_minutes"].ToString()),
                                              int.Parse(row["penalty_shot_count"].ToString()),
                                              int.Parse(row["penalty_shot_goal_count"].ToString()),
                                              int.Parse(row["empty_net_goal_count"].ToString()),
                                              int.Parse(row["delayed_penalty_goal_count"].ToString()),
                                              int.Parse(row["own_goal_count"].ToString()),
                                              int.Parse(row["gwg_goal_count"].ToString()),
                                              int.Parse(row["goal_without_assist_count"].ToString()));

                    Entities.Add(p);
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