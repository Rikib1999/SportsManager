using SportsManager.Commands;
using SportsManager.Models;
using SportsManager.Others;
using SportsManager.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;

namespace SportsManager.ViewModels
{
    /// <summary>
    /// Viewmodel for viewing all matches and theirs statistics in one sortable table with filtering. Serves for navigating to a match detail viewmodel.
    /// </summary>
    public class MatchesSelectionViewModel : TemplateSelectionDataGridViewModel<Match>
    {
        /// <summary>
        /// Instantiates new MatchesSelectionViewModel.
        /// </summary>
        /// <param name="navigationStore">Current instance of NavigationStore.</param>
        public MatchesSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateEntityCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new MatchViewModel(navigationStore, SelectedEntity, new MatchesSelectionViewModel(navigationStore))));
            LoadData();
        }

        /// <summary>
        /// Loads the matches with their statistics from the database.
        /// </summary>
        protected override void LoadData()
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("", connection);

            string goalCountQuery = "SELECT match_id, COUNT(*) AS goal_count " +
                                    "FROM goals " +
                                    "INNER JOIN matches ON matches.id = match_id " +
                                    "INNER JOIN seasons ON seasons.id = matches.season_id";
            goalCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(true, "matches.season_id", "seasons.competition_id");
            goalCountQuery += " GROUP BY match_id";

            string assistCountQuery = "SELECT match_id, COUNT(*) AS assist_count " +
                                      "FROM goals " +
                                      "INNER JOIN matches ON matches.id = match_id " +
                                      "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                      "WHERE assist_player_id <> -1";
            assistCountQuery += DatabaseHandler.WhereSeasonCompetitionQuery(false, "matches.season_id", "seasons.competition_id");
            assistCountQuery += " GROUP BY match_id";

            string penaltyMinutesQuery = "SELECT match_id, COALESCE(SUM(penalty_type.minutes), 0) AS penalty_minutes " +
                                         "FROM penalties " +
                                         "INNER JOIN matches ON matches.id = match_id " +
                                         "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                         "INNER JOIN penalty_type ON penalty_type.code = penalty_type_id";
            penaltyMinutesQuery += DatabaseHandler.WhereSeasonCompetitionQuery(true, "matches.season_id", "seasons.competition_id");
            penaltyMinutesQuery += " GROUP BY match_id";

            string matchStatsQuery = "SELECT h.name AS home_name, a.name AS away_name, s.competition_id AS competition_id, s.name AS season_name, c.name AS competition_name, " +
                                            "matches.id, datetime, home_score, away_score, overtime, shootout, forfeit, qualification_id, serie_match_number, " +
                                            "IFNULL(goal_count, 0) AS goal_count, " +
                                            "IFNULL(assist_count, 0) AS assist_count, " +
                                            "IFNULL(penalty_minutes, 0) AS penalty_minutes " +
                                      "FROM matches " +
                                      "INNER JOIN seasons AS s ON s.id = matches.season_id " +
                                      "INNER JOIN competitions AS c ON c.id = competition_id " +
                                      "INNER JOIN team AS h ON h.id = matches.home_competitor " +
                                      "INNER JOIN team AS a ON a.id = matches.away_competitor ";

            matchStatsQuery += "LEFT JOIN (" + goalCountQuery + ") AS g ON g.match_id = matches.id " +
                               "LEFT JOIN (" + assistCountQuery + ") AS a ON a.match_id = matches.id " +
                               "LEFT JOIN (" + penaltyMinutesQuery + ") AS pm ON pm.match_id = matches.id";

            matchStatsQuery += " WHERE matches.played = 1";
            if (SportsData.IsCompetitionSet())
            {
                matchStatsQuery += " AND s.competition_id = " + SportsData.COMPETITION.ID;
                if (SportsData.IsSeasonSet())
                {
                    matchStatsQuery += " AND season_id = " + SportsData.SEASON.ID;
                }
            }
            matchStatsQuery += " GROUP BY matches.id" +
                               " ORDER BY matches.datetime DESC";

            cmd.CommandText = matchStatsQuery;

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                Entities = new ObservableCollection<Match>();

                foreach (DataRow row in dataTable.Rows)
                {
                    Competition c = new();
                    c.Name = row["competition_name"].ToString();
                    Season s = new();
                    s.Name = row["season_name"].ToString();
                    Team home = new();
                    home.Name = row["home_name"].ToString();
                    Team away = new();
                    away.Name = row["away_name"].ToString();

                    Match m = new()
                    {
                        ID = int.Parse(row["id"].ToString()),
                        Competition = c,
                        Season = s,
                        Datetime = DateTime.Parse(row["datetime"].ToString()),
                        HomeTeam = home,
                        AwayTeam = away,
                        HomeScore = int.Parse(row["home_score"].ToString()),
                        AwayScore = int.Parse(row["away_score"].ToString()),
                        Overtime = Convert.ToBoolean(int.Parse(row["overtime"].ToString())),
                        Shootout = Convert.ToBoolean(int.Parse(row["shootout"].ToString())),
                        Forfeit = Convert.ToBoolean(int.Parse(row["forfeit"].ToString()))
                    };

                    string partOfSeason = "Play-off";
                    if (int.Parse(row["serie_match_number"].ToString()) < 1)
                    {
                        partOfSeason = "Group";
                    }
                    else if (int.Parse(row["qualification_id"].ToString()) > 0)
                    {
                        partOfSeason = "Qualification";
                    }

                    //m.Stats = new MatchStats(m);
                    //((MatchStats)m.Stats).PartOfSeason = partOfSeason;
                    //((MatchStats)m.Stats).Score = m.Score();
                    m.Stats = new MatchStats(int.Parse(row["goal_count"].ToString()),
                                             int.Parse(row["assist_count"].ToString()),
                                             int.Parse(row["penalty_minutes"].ToString()),
                                             partOfSeason);

                    Entities.Add(m);
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