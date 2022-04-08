using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
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
    public class SeasonsSelectionViewModel : TemplateSelectionDataGridViewModel<Season>
    {
        public ICommand NavigateAddSeasonCommand { get; set; }

        private ICommand checkNavigateEntityCommand;
        public new ICommand CheckNavigateEntityCommand
        {
            get
            {
                if (checkNavigateEntityCommand == null)
                {
                    checkNavigateEntityCommand = new RelayCommand(param => CheckNavigateEntity(SelectedEntity));
                }
                return checkNavigateEntityCommand;
            }
        }

        public SeasonsSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateAddSeasonCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new AddSeasonViewModel(navigationStore)));
            NavigateEntityCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new SeasonViewModel(navigationStore)));
            LoadData();
        }

        protected override void LoadData()
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("", connection);

            string matchCountQuery = "SELECT matches.season_id AS season_id, COUNT(*) AS match_count " +
                                     "FROM matches " +
                                     "WHERE played = 1 " +
                                     "GROUP BY season_id";

            string teamCountQuery = "SELECT team_enlistment.season_id AS season_id, COUNT(*) AS team_count " +
                                    "FROM team_enlistment " +
                                    "GROUP BY season_id";

            string playerCountQuery = "SELECT player_enlistment.season_id AS season_id, COUNT(DISTINCT player_id) AS player_count " +
                                      "FROM player_enlistment " +
                                      "GROUP BY season_id";

            string goalCountQuery = "SELECT matches.season_id AS season_id, COUNT(*) AS goal_count " +
                                    "FROM goals " +
                                    "INNER JOIN matches ON matches.id = match_id " +
                                    "GROUP BY season_id";

            string assistCountQuery = "SELECT matches.season_id AS season_id, COUNT(*) AS assist_count " +
                                      "FROM goals " +
                                      "INNER JOIN matches ON matches.id = match_id " +
                                      "WHERE assist_player_id <> -1 GROUP BY season_id";

            string penaltyMinutesCountQuery = "SELECT matches.season_id AS season_id, COALESCE(SUM(penalty_type.minutes), 0) AS penalty_minutes " +
                                              "FROM penalties " +
                                              "INNER JOIN matches ON matches.id = match_id " +
                                              "INNER JOIN penalty_type ON penalty_type.code = penalty_type_id " +
                                              "GROUP BY season_id";

            string seasonStatsQuery = "SELECT w.name AS winner, winner_id, c.name AS competition_name, seasons.id, competition_id, seasons.name, seasons.info, qualification_count, " +
                                      "qualification_rounds, group_count, play_off_rounds, play_off_best_of, play_off_started, points_for_W, points_for_OW, points_for_T, points_for_OL, points_for_L, " +
                                      "IFNULL(match_count, 0) AS match_count, " +
                                      "IFNULL(team_count, 0) AS team_count, " +
                                      "IFNULL(player_count, 0) AS player_count, " +
                                      "IFNULL(goal_count, 0) AS goal_count, " +
                                      "IFNULL(assist_count, 0) AS assist_count, " +
                                      "IFNULL(penalty_minutes, 0) AS penalty_minutes " +
                                      "FROM seasons " +
                                      "INNER JOIN competitions AS c ON c.id = competition_id " +
                                      "INNER JOIN team AS w ON w.id = winner_id ";

            seasonStatsQuery += "LEFT JOIN (" + matchCountQuery + ") AS m ON m.season_id = seasons.id " +
                                "LEFT JOIN (" + teamCountQuery + ") AS t ON t.season_id = seasons.id " +
                                "LEFT JOIN (" + playerCountQuery + ") AS p ON p.season_id = seasons.id " +
                                "LEFT JOIN (" + goalCountQuery + ") AS g ON g.season_id = seasons.id " +
                                "LEFT JOIN (" + assistCountQuery + ") AS a ON a.season_id = seasons.id " +
                                "LEFT JOIN (" + penaltyMinutesCountQuery + ") AS pm ON pm.season_id = seasons.id";

            if (SportsData.IsCompetitionSet())
            {
                seasonStatsQuery += " WHERE competition_id = " + SportsData.COMPETITION.ID;
            }

            cmd.CommandText = seasonStatsQuery;

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                Entities = new ObservableCollection<Season>();

                foreach (DataRow row in dataTable.Rows)
                {
                    Competition c = new();
                    c.Name = row["competition_name"].ToString();
                    c.ID = int.Parse(row["competition_id"].ToString());

                    Season s = new()
                    {
                        ID = int.Parse(row["id"].ToString()),
                        Competition = c,
                        Name = row["name"].ToString(),
                        Info = row["info"].ToString(),
                        QualificationCount = int.Parse(row["qualification_count"].ToString()),
                        QualificationRounds = int.Parse(row["qualification_rounds"].ToString()),
                        GroupCount = int.Parse(row["group_count"].ToString()),
                        PlayOffRounds = int.Parse(row["play_off_rounds"].ToString()),
                        PlayOffBestOf = int.Parse(row["play_off_best_of"].ToString()),
                        WinnerName = row["winner"].ToString(),
                        WinnerID = int.Parse(row["winner_id"].ToString()),
                        PlayOffStarted = Convert.ToBoolean(int.Parse(row["play_off_started"].ToString())),
                        PointsForWin = int.Parse(row["points_for_W"].ToString()),
                        PointsForOTWin = int.Parse(row["points_for_OW"].ToString()),
                        PointsForTie = int.Parse(row["points_for_T"].ToString()),
                        PointsForOTLoss = int.Parse(row["points_for_OL"].ToString()),
                        PointsForLoss = int.Parse(row["points_for_L"].ToString())
                    };
                    string[] imgPath = System.IO.Directory.GetFiles(SportsData.SeasonLogosPath, SportsData.SPORT.Name + row["id"].ToString() + ".*");
                    if (imgPath.Length != 0)
                    {
                        s.ImagePath = imgPath.First();
                    }

                    //s.Stats = new SeasonStats(s);
                    s.Stats = new SeasonStats(int.Parse(row["match_count"].ToString()),
                                              int.Parse(row["team_count"].ToString()),
                                              int.Parse(row["player_count"].ToString()),
                                              int.Parse(row["goal_count"].ToString()),
                                              int.Parse(row["assist_count"].ToString()),
                                              int.Parse(row["penalty_minutes"].ToString()));
                    Entities.Add(s);
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