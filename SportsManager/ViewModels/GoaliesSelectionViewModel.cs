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
    /// Viewmodel for viewing all goaltenders and theirs statistics in one sortable table with filtering. Serves for navigating to a goaltender detail viewmodel.
    /// </summary>
    public class GoaliesSelectionViewModel : TemplateSelectionDataGridViewModel<Player>
    {
        /// <summary>
        /// Instantiates new GoaliesSelectionViewModel.
        /// </summary>
        /// <param name="navigationStore">Current instance of NavigationStore.</param>
        public GoaliesSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateEntityCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new PlayerViewModel(navigationStore, SelectedEntity)));
            LoadData();
        }

        /// <summary>
        /// Loads the goaltenders with their statistics from the database.
        /// </summary>
        protected override void LoadData()
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("", connection);

            string matchCountQuery = "SELECT player_id, COUNT(*) AS match_count " +
                                    "FROM goalie_matches " +
                                    "INNER JOIN matches ON matches.id = match_id " +
                                    "INNER JOIN seasons ON seasons.id = matches.season_id";
            matchCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(true, "matches.season_id", "seasons.competition_id");
            matchCountQuery += " GROUP BY player_id";

            string goalsAgainstCountQuery = "SELECT goalie_id, COUNT(*) AS goals_against_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id";
            goalsAgainstCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(true, "matches.season_id", "seasons.competition_id");
            goalsAgainstCountQuery += " GROUP BY goalie_id";

            string shutoutCountQuery = "SELECT goalie_id, COUNT(*) AS shutout_count " +
                                "FROM shutouts " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id";
            shutoutCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(true, "matches.season_id", "seasons.competition_id");
            shutoutCountQuery += " GROUP BY goalie_id";

            string timeInGameQuery = "SELECT player_id, COALESCE(SUM(duration), 0) AS time_in_game " +
                                                "FROM shifts " +
                                                "INNER JOIN matches ON matches.id = match_id " +
                                                "INNER JOIN seasons ON seasons.id = matches.season_id";
            timeInGameQuery += DatabaseHandler.WhereSeasonCompetitionQuery(true, "matches.season_id", "seasons.competition_id");
            timeInGameQuery += " GROUP BY player_id";

            string penaltyShotCountQuery = "SELECT goalie_id, COUNT(*) AS penalty_shot_count " +
                                "FROM penalty_shots " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id";
            penaltyShotCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(true, "matches.season_id", "seasons.competition_id");
            penaltyShotCountQuery += " GROUP BY goalie_id";

            string penaltyShotGoalsAgainstCountQuery = "SELECT goalie_id, COUNT(*) AS penalty_shot_goals_against_count " +
                                "FROM penalty_shots " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                "WHERE was_goal = 1";
            penaltyShotGoalsAgainstCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            penaltyShotGoalsAgainstCountQuery += " GROUP BY goalie_id";

            string shootoutShotCountQuery = "SELECT goalie_id, COUNT(*) AS shootout_shot_count " +
                                "FROM shootout_shots " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id";
            shootoutShotCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(true, "matches.season_id", "seasons.competition_id");
            shootoutShotCountQuery += " GROUP BY goalie_id";

            string shootoutShotGoalsAgainstCountQuery = "SELECT goalie_id, COUNT(*) AS shootout_shot_goals_against_count " +
                                "FROM shootout_shots " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                "WHERE was_goal = 1";
            shootoutShotGoalsAgainstCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            shootoutShotGoalsAgainstCountQuery += " GROUP BY goalie_id";

            string goalieStatsQuery = "SELECT p.*, s.competition_id, " +
                                      "IFNULL(match_count, 0) AS match_count, " +
                                      "IFNULL(goals_against_count, 0) AS goals_against_count, " +
                                      "IFNULL(shutout_count, 0) AS shutout_count, " +
                                      "IFNULL(time_in_game, 0) AS time_in_game, " +
                                      "IFNULL(penalty_shot_count, 0) AS penalty_shot_count, " +
                                      "IFNULL(penalty_shot_goals_against_count, 0) AS penalty_shot_goals_against_count, " +
                                      "IFNULL(shootout_shot_count, 0) AS shootout_shot_count, " +
                                      "IFNULL(shootout_shot_goals_against_count, 0) AS shootout_shot_goals_against_count " +
                                      "FROM goalie_matches " +
                                      "RIGHT JOIN player AS p ON p.id = player_id " +
                                      "INNER JOIN matches AS m ON m.id = match_id " +
                                      "INNER JOIN seasons AS s ON s.id = m.season_id ";

            goalieStatsQuery += "LEFT JOIN (" + matchCountQuery + ") AS m ON m.player_id = goalie_matches.player_id " +
                                "LEFT JOIN (" + goalsAgainstCountQuery + ") AS g ON g.goalie_id = goalie_matches.player_id " +
                                "LEFT JOIN (" + shutoutCountQuery + ") AS so ON so.goalie_id = goalie_matches.player_id " +
                                "LEFT JOIN (" + timeInGameQuery + ") AS t ON t.player_id = goalie_matches.player_id " +
                                "LEFT JOIN (" + penaltyShotCountQuery + ") AS ps ON ps.goalie_id = goalie_matches.player_id " +
                                "LEFT JOIN (" + penaltyShotGoalsAgainstCountQuery + ") AS psg ON psg.goalie_id = goalie_matches.player_id " +
                                "LEFT JOIN (" + shootoutShotCountQuery + ") AS ss ON ss.goalie_id = goalie_matches.player_id " +
                                "LEFT JOIN (" + shootoutShotGoalsAgainstCountQuery + ") AS ssg ON ssg.goalie_id = goalie_matches.player_id";

            goalieStatsQuery += " WHERE goalie_matches.player_id <> -1";
            if (SportsData.IsCompetitionSet())
            {
                goalieStatsQuery += " AND s.competition_id = " + SportsData.COMPETITION.ID;
                if (SportsData.IsSeasonSet())
                {
                    goalieStatsQuery += " AND season_id = " + SportsData.SEASON.ID;
                }
            }
            goalieStatsQuery += " GROUP BY goalie_matches.player_id";

            cmd.CommandText = goalieStatsQuery;

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
                        Citizenship = new Country { CodeTwo = row["citizenship"].ToString() },
                        BirthplaceCity = row["birthplace_city"].ToString(),
                        BirthplaceCountry = new Country { CodeTwo = row["birthplace_country"].ToString() },
                        Status = Convert.ToBoolean(int.Parse(row["status"].ToString())),
                        Info = row["info"].ToString()
                    };

                    string[] imgPath = System.IO.Directory.GetFiles(SportsData.PlayerPhotosPath, SportsData.SPORT.Name + p.ID + ".*");
                    p.ImagePath = imgPath.Length != 0
                        ? imgPath.First()
                        : p.Gender == "M" ? SportsData.ResourcesPath + "\\male.png" : SportsData.ResourcesPath + "\\female.png";

                    //p.Stats = new GoalieStats(p, SportsData.SEASON.ID, SportsData.COMPETITION.ID);
                    p.Stats = new GoalieStats(int.Parse(row["match_count"].ToString()),
                                              int.Parse(row["goals_against_count"].ToString()),
                                              int.Parse(row["shutout_count"].ToString()),
                                              int.Parse(row["time_in_game"].ToString()),
                                              int.Parse(row["penalty_shot_count"].ToString()),
                                              int.Parse(row["penalty_shot_goals_against_count"].ToString()),
                                              int.Parse(row["shootout_shot_count"].ToString()),
                                              int.Parse(row["shootout_shot_goals_against_count"].ToString()));

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