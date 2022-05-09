using SportsManager.Others;
using SportsManager.ViewModels;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SportsManager.Models
{
    /// <summary>
    /// Interface for classes representing statistics.
    /// </summary>
    public interface IStats { }

    /// <summary>
    /// Class representing statistics of a season.
    /// </summary>
    public class SeasonStats : NotifyPropertyChanged, IStats
    {
        #region Properties
        private int matches;
        /// <summary>
        /// Number of played matches in the season.
        /// </summary>
        public int Matches
        {
            get => matches;
            set
            {
                matches = value;
                OnPropertyChanged();
            }
        }

        private int teams;
        /// <summary>
        /// Number of teams in the season.
        /// </summary>
        public int Teams
        {
            get => teams;
            set
            {
                teams = value;
                OnPropertyChanged();
            }
        }

        private int players;
        /// <summary>
        /// Number of players in the season.
        /// </summary>
        public int Players
        {
            get => players;
            set
            {
                players = value;
                OnPropertyChanged();
            }
        }

        private int goals;
        /// <summary>
        /// Number of scored goals in the season.
        /// </summary>
        public int Goals
        {
            get => goals;
            set
            {
                goals = value;
                OnPropertyChanged();
            }
        }

        private float goalsPerGame;
        /// <summary>
        /// Number of goals per match in the season.
        /// </summary>
        public float GoalsPerGame
        {
            get => goalsPerGame;
            set
            {
                goalsPerGame = value;
                OnPropertyChanged();
            }
        }

        private int assists;
        /// <summary>
        /// Number of assists in the season.
        /// </summary>
        public int Assists
        {
            get => assists;
            set
            {
                assists = value;
                OnPropertyChanged();
            }
        }

        private float assistsPerGame;
        /// <summary>
        /// Number of assists per match in the season.
        /// </summary>
        public float AssistsPerGame
        {
            get => assistsPerGame;
            set
            {
                assistsPerGame = value;
                OnPropertyChanged();
            }
        }

        private int penaltyMinutes;
        /// <summary>
        /// Number of penalty minutes in the season.
        /// </summary>
        public int PenaltyMinutes
        {
            get => penaltyMinutes;
            set
            {
                penaltyMinutes = value;
                OnPropertyChanged();
            }
        }

        private float penaltyMinutesPerGame;
        /// <summary>
        /// Number of penalty minutes per match in the season.
        /// </summary>
        public float PenaltyMinutesPerGame
        {
            get => penaltyMinutesPerGame;
            set
            {
                penaltyMinutesPerGame = value;
                OnPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// Loads all stats of a season asynchronously by category.
        /// </summary>
        /// <param name="s">Season to calculate stats for.</param>
        public SeasonStats(Season s)
        {
            CalculateStats(s.ID).Await();
        }

        /// <summary>
        /// Stores and calculates stats for a season.
        /// </summary>
        /// <param name="matches">Number of played matches in the season.</param>
        /// <param name="teams">Number of teams in the season.</param>
        /// <param name="players">Number of players in the season</param>
        /// <param name="goals">Number of goals in the season</param>
        /// <param name="assists">Number of assists in the season</param>
        /// <param name="penaltyMinutes">Number of penalty minutes in the season</param>
        public SeasonStats(int matches, int teams, int players, int goals, int assists, int penaltyMinutes)
        {
            Matches = matches;
            Teams = teams;
            Players = players;
            Goals = goals;
            Assists = assists;
            PenaltyMinutes = penaltyMinutes;
            if (Matches > 0)
            {
                GoalsPerGame = (float)Math.Round(Goals / (float)Matches, 2);
                AssistsPerGame = (float)Math.Round(Assists / (float)Matches, 2);
                PenaltyMinutesPerGame = (float)Math.Round(PenaltyMinutes / (float)Matches, 2);
            }
        }

        /// <summary>
        /// Loads season stats asynchronously.
        /// </summary>
        /// <param name="seasonID">Identification number of season to load stats for.</param>
        /// <returns>The task calculating the stats.</returns>
        public async Task CalculateStats(int seasonID)
        {
            List<Task> tasks = new();
            //await matches so they are available for ...PerGame calculations
            await Task.Run(() => CountMatches(seasonID));
            tasks.Add(Task.Run(() => CountTeams(seasonID)));
            tasks.Add(Task.Run(() => CountPlayers(seasonID)));
            tasks.Add(Task.Run(() => CountGoals(seasonID)));
            tasks.Add(Task.Run(() => CountAssists(seasonID)));
            tasks.Add(Task.Run(() => CountPenalties(seasonID)));
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Calculate number of played matches in the season asynchronously.
        /// </summary>
        /// <param name="seasonID">Identification number of season to load stats for.</param>
        /// <returns>Task calculating the stats.</returns>
        private async Task CountMatches(int seasonID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) FROM matches WHERE season_id = " + seasonID, connection);
            try
            {
                connection.Open();
                Matches = (int)(long)await cmd.ExecuteScalarAsync();
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

        /// <summary>
        /// Calculate number of teams in the season asynchronously.
        /// </summary>
        /// <param name="seasonID">Identification number of season to load stats for.</param>
        /// <returns>Task calculating the stats.</returns>
        private async Task CountTeams(int seasonID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) FROM team_enlistment WHERE season_id = " + seasonID, connection);
            try
            {
                connection.Open();
                Teams = (int)(long)await cmd.ExecuteScalarAsync();
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

        /// <summary>
        /// Calculate number of players in the season asynchronously.
        /// </summary>
        /// <param name="seasonID">Identification number of season to load stats for.</param>
        /// <returns>Task calculating the stats.</returns>
        private async Task CountPlayers(int seasonID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(DISTINCT player_id) FROM player_enlistment WHERE season_id = " + seasonID, connection);
            try
            {
                connection.Open();
                Players = (int)(long)await cmd.ExecuteScalarAsync();
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

        /// <summary>
        /// Calculate number of scored goals in the season asynchronously.
        /// </summary>
        /// <param name="seasonID">Identification number of season to load stats for.</param>
        /// <returns>Task calculating the stats.</returns>
        private async Task CountGoals(int seasonID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) FROM goals " +
                                   "INNER JOIN matches AS m ON m.id = match_id " +
                                   "WHERE m.season_id = " + seasonID, connection);
            try
            {
                connection.Open();
                Goals = (int)(long)await cmd.ExecuteScalarAsync();
                if (Matches != 0) { GoalsPerGame = (float)Math.Round(Goals / (float)Matches, 2); }
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

        /// <summary>
        /// Calculate number of assists in the season asynchronously.
        /// </summary>
        /// <param name="seasonID">Identification number of season to load stats for.</param>
        /// <returns>Task calculating the stats.</returns>
        private async Task CountAssists(int seasonID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) FROM goals " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "WHERE assist_player_id <> -1 AND m.season_id = " + seasonID, connection);
            try
            {
                connection.Open();
                Assists = (int)(long)await cmd.ExecuteScalarAsync();
                if (Matches != 0) { AssistsPerGame = (float)Math.Round(Assists / (float)Matches, 2); }
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

        /// <summary>
        /// Calculate number of penalty minutes in the season asynchronously.
        /// </summary>
        /// <param name="seasonID">Identification number of season to load stats for.</param>
        /// <returns>Task calculating the stats.</returns>
        private async Task CountPenalties(int seasonID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) FROM penalties " +
                                   "INNER JOIN matches AS m ON m.id = match_id " +
                                   "WHERE m.season_id = " + seasonID, connection);
            try
            {
                connection.Open();
                PenaltyMinutes = (int)(long)await cmd.ExecuteScalarAsync();
                if (Matches != 0) { PenaltyMinutesPerGame = (float)Math.Round(PenaltyMinutes / (float)Matches, 2); }
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

    /// <summary>
    /// Class representing statistics of a team.
    /// </summary>
    public class TeamStats : NotifyPropertyChanged, IStats
    {
        #region Properties
        private int gamesPlayed;
        /// <summary>
        /// Number of played matches.
        /// </summary>
        public int GamesPlayed
        {
            get => gamesPlayed;
            set
            {
                gamesPlayed = value;
                OnPropertyChanged();
            }
        }

        private int wins;
        /// <summary>
        /// Number of won matches.
        /// </summary>
        public int Wins
        {
            get => wins;
            set
            {
                wins = value;
                OnPropertyChanged();
            }
        }

        private int winsOT;
        /// <summary>
        /// Number of won matches in overtime.
        /// </summary>
        public int WinsOT
        {
            get => winsOT;
            set
            {
                winsOT = value;
                OnPropertyChanged();
            }
        }

        private int ties;
        /// <summary>
        /// Number of tied matches.
        /// </summary>
        public int Ties
        {
            get => ties;
            set
            {
                ties = value;
                OnPropertyChanged();
            }
        }

        private int lossesOT;
        /// <summary>
        /// Number of lost matches in overtime.
        /// </summary>
        public int LossesOT
        {
            get => lossesOT;
            set
            {
                lossesOT = value;
                OnPropertyChanged();
            }
        }

        private int losses;
        /// <summary>
        /// Number of lost matches.
        /// </summary>
        public int Losses
        {
            get => losses;
            set
            {
                losses = value;
                OnPropertyChanged();
            }
        }

        private int goals;
        /// <summary>
        /// Number of scored goals.
        /// </summary>
        public int Goals
        {
            get => goals;
            set
            {
                goals = value;
                OnPropertyChanged();
            }
        }

        private int assists;
        /// <summary>
        /// Number of assists.
        /// </summary>
        public int Assists
        {
            get => assists;
            set
            {
                assists = value;
                OnPropertyChanged();
            }
        }

        private int goalsAgainst;
        /// <summary>
        /// Number of goals against.
        /// </summary>
        public int GoalsAgainst
        {
            get => goalsAgainst;
            set
            {
                goalsAgainst = value;
                OnPropertyChanged();
            }
        }

        private int goalDifference;
        /// <summary>
        /// The goal difference. Scored goals minus goals against.
        /// </summary>
        public int GoalDifference
        {
            get => goalDifference;
            set
            {
                goalDifference = value;
                OnPropertyChanged();
            }
        }

        private int penaltyMinutes;
        /// <summary>
        /// Number of penalty minutes recieved.
        /// </summary>
        public int PenaltyMinutes
        {
            get => penaltyMinutes;
            set
            {
                penaltyMinutes = value;
                OnPropertyChanged();
            }
        }

        private int points;
        /// <summary>
        /// Number of points recieved for match results.
        /// </summary>
        public int Points
        {
            get => points;
            set
            {
                points = value;
                OnPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// Stores all stats in a single object.
        /// </summary>
        /// <param name="gamesPlayed">Number of played games.</param>
        /// <param name="goals">Number of scored goals.</param>
        /// <param name="goalsAgainst">Number of goals against.</param>
        /// <param name="assists">Number of assists.</param>
        /// <param name="penaltyMinutes">Number of recieved penalty minutes.</param>
        /// <param name="wins">Number of won matches.</param>
        /// <param name="winsOT">Number of won matches in overtime.</param>
        /// <param name="ties">Number of tied matches.</param>
        /// <param name="lossesOT">Number of lost matches in overtime.</param>
        /// <param name="losses">Number of lost matches.</param>
        public TeamStats(int gamesPlayed, int goals, int goalsAgainst, int assists, int penaltyMinutes, int wins, int winsOT, int ties, int lossesOT, int losses)
        {
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

    /// <summary>
    /// Class representing statistics of a match.
    /// </summary>
    public class MatchStats : NotifyPropertyChanged, IStats
    {
        #region Properties
        private string partOfSeason = "";
        /// <summary>
        /// Name of the part of season the match was played in.
        /// </summary>
        public string PartOfSeason
        {
            get => partOfSeason;
            set
            {
                partOfSeason = value;
                OnPropertyChanged();
            }
        }

        private int goals;
        /// <summary>
        /// Number of goals in the match.
        /// </summary>
        public int Goals
        {
            get => goals;
            set
            {
                goals = value;
                OnPropertyChanged();
            }
        }

        private int assists;
        /// <summary>
        /// Number of assists in the match.
        /// </summary>
        public int Assists
        {
            get => assists;
            set
            {
                assists = value;
                OnPropertyChanged();
            }
        }

        private int penaltyMinutes;
        /// <summary>
        /// Number of penalty minutes in the match.
        /// </summary>
        public int PenaltyMinutes
        {
            get => penaltyMinutes;
            set
            {
                penaltyMinutes = value;
                OnPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// Stores match statistics in a single object.
        /// </summary>
        /// <param name="goals">Number of goals in the match.</param>
        /// <param name="assists">Number of assists in the match.</param>
        /// <param name="penaltyMinutes">Number of penalty minutes in the match.</param>
        /// <param name="partOfSeason">Name of the part of season the match was played in.</param>
        public MatchStats(int goals, int assists, int penaltyMinutes, string partOfSeason)
        {
            Goals = goals;
            Assists = assists;
            PenaltyMinutes = penaltyMinutes;
            PartOfSeason = partOfSeason;
        }

        /// <summary>
        /// Loads all stats of a match asynchronously by category.
        /// </summary>
        /// <param name="m">Match to calculate stats for.</param>
        public MatchStats(Match m)
        {
            CalculateStats(m.ID).Await();
        }

        /// <summary>
        /// Loads match stats asynchronously.
        /// </summary>
        /// <param name="matchID">Identification number of match to load stats for.</param>
        /// <returns>The task calculating the stats.</returns>
        public async Task CalculateStats(int matchID)
        {
            List<Task> tasks = new();
            tasks.Add(Task.Run(() => CountGoals(matchID)));
            tasks.Add(Task.Run(() => CountAssists(matchID)));
            tasks.Add(Task.Run(() => CountPenalties(matchID)));
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Calculate number of scored goals in the match asynchronously.
        /// </summary>
        /// <param name="matchID">Identification number of match to load stats for.</param>
        /// <returns>Task calculating the stats.</returns>
        private async Task CountGoals(int matchID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) FROM goals " +
                                                "WHERE match_id = " + matchID, connection);
            try
            {
                connection.Open();
                Goals = (int)(long)await cmd.ExecuteScalarAsync();
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

        /// <summary>
        /// Calculate number of assists in the match asynchronously.
        /// </summary>
        /// <param name="matchID">Identification number of match to load stats for.</param>
        /// <returns>Task calculating the stats.</returns>
        private async Task CountAssists(int matchID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) FROM goals " +
                                                "WHERE assist_player_id <> -1 AND match_id = " + matchID, connection);
            try
            {
                connection.Open();
                Assists = (int)(long)await cmd.ExecuteScalarAsync();
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

        /// <summary>
        /// Calculate number of penalty minutes in the match asynchronously.
        /// </summary>
        /// <param name="matchID">Identification number of match to load stats for.</param>
        /// <returns>Task calculating the stats.</returns>
        private async Task CountPenalties(int matchID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COALESCE(SUM(penalty_type.minutes), 0) FROM penalties " +
                                         "INNER JOIN penalty_type ON penalty_type.code = penalty_type_id " +
                                         "WHERE match_id = " + matchID, connection);
            try
            {
                connection.Open();
                PenaltyMinutes = (int)(decimal)await cmd.ExecuteScalarAsync();
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

    /// <summary>
    /// Class representing statistics of a player.
    /// </summary>
    public class PlayerStats : NotifyPropertyChanged, IStats
    {
        #region Properties
        private int gamesPlayed;
        /// <summary>
        /// Number of games played.
        /// </summary>
        public int GamesPlayed
        {
            get => gamesPlayed;
            set
            {
                gamesPlayed = value;
                OnPropertyChanged();
            }
        }

        private int goals;
        /// <summary>
        /// Number of goals scored.
        /// </summary>
        public int Goals
        {
            get => goals;
            set
            {
                goals = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Number of goals scored per game.
        /// </summary>
        public float GoalsPerGame => GamesPlayed != 0 ? (float)Math.Round(Goals / (float)GamesPlayed, 2) : float.NaN;

        private int assists;
        /// <summary>
        /// Number of assists.
        /// </summary>
        public int Assists
        {
            get => assists;
            set
            {
                assists = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Number of assists per game.
        /// </summary>
        public float AssistsPerGame => GamesPlayed != 0 ? (float)Math.Round(Assists / (float)GamesPlayed, 2) : float.NaN;

        private int ppGoals;
        /// <summary>
        /// Number of power play goals scored.
        /// </summary>
        public int PpGoals
        {
            get => ppGoals;
            set
            {
                ppGoals = value;
                OnPropertyChanged();
            }
        }

        private int shGoals;
        /// <summary>
        /// Number of short-handed goals scored.
        /// </summary>
        public int ShGoals
        {
            get => shGoals;
            set
            {
                shGoals = value;
                OnPropertyChanged();
            }
        }

        private int evGoals;
        /// <summary>
        /// Number of even strength goals scored.
        /// </summary>
        public int EvGoals
        {
            get => evGoals;
            set
            {
                evGoals = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Number of goals and assists.
        /// </summary>
        public int Points => Goals + Assists;

        private int penaltyMinutes;
        /// <summary>
        /// Number of penalty minutes recieved.
        /// </summary>
        public int PenaltyMinutes
        {
            get => penaltyMinutes;
            set
            {
                penaltyMinutes = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Number of penalty minutes recieved per game.
        /// </summary>
        public string PenaltyMinutesPerGame => GamesPlayed != 0 ? (PenaltyMinutes / GamesPlayed) + ":" + (PenaltyMinutes * 60 / GamesPlayed % 60).ToString("00") : "0:00";

        private int penaltyShots;
        /// <summary>
        /// Number of penalty shot attempts.
        /// </summary>
        public int PenaltyShots
        {
            get => penaltyShots;
            set
            {
                penaltyShots = value;
                OnPropertyChanged();
            }
        }

        private int penaltyShotGoals;
        /// <summary>
        /// Number of penalty shot goals.
        /// </summary>
        public int PenaltyShotGoals
        {
            get => penaltyShotGoals;
            set
            {
                penaltyShotGoals = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Percentage of successfull penalty shot attempts.
        /// </summary>
        public float PenaltyShotsPercentage => PenaltyShots != 0 ? (float)Math.Round(PenaltyShotGoals / (float)PenaltyShots * 100, 2) : float.NaN;

        private int emptyNetGoals;
        /// <summary>
        /// Number of goals scored into an empty net.
        /// </summary>
        public int EmptyNetGoals
        {
            get => emptyNetGoals;
            set
            {
                emptyNetGoals = value;
                OnPropertyChanged();
            }
        }

        private int delayedPenaltyGoals;
        /// <summary>
        /// Number of goals scored during delayed penalty.
        /// </summary>
        public int DelayedPenaltyGoals
        {
            get => delayedPenaltyGoals;
            set
            {
                delayedPenaltyGoals = value;
                OnPropertyChanged();
            }
        }

        private int ownGoals;
        /// <summary>
        /// Number of own goals scored.
        /// </summary>
        public int OwnGoals
        {
            get => ownGoals;
            set
            {
                ownGoals = value;
                OnPropertyChanged();
            }
        }

        private int gameWinningGoals;
        /// <summary>
        /// Number of winning goals scored.
        /// </summary>
        public int GameWinningGoals
        {
            get => gameWinningGoals;
            set
            {
                gameWinningGoals = value;
                OnPropertyChanged();
            }
        }

        private int goalsWithoutAssist;
        /// <summary>
        /// Number of own goals scored without assist.
        /// </summary>
        public int GoalsWithoutAssist
        {
            get => goalsWithoutAssist;
            set
            {
                goalsWithoutAssist = value;
                OnPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// Stores player statistics in a single object.
        /// </summary>
        /// <param name="gamesPlayed">Number of games played.</param>
        /// <param name="goals">Number of scored goals.</param>
        /// <param name="assists">Number of assists.</param>
        /// <param name="ppGoals">Number of power play goals scored.</param>
        /// <param name="shGoals">Number of goals scored while short-handed.</param>
        /// <param name="evGoals">Number of even strength goals scored.</param>
        /// <param name="penaltyMinutes">Number of penalty minutes recieved.</param>
        /// <param name="penaltyShots">Number of penlaty shot attempts.</param>
        /// <param name="penaltyShotGoals">Number of penalty shot goals.</param>
        /// <param name="emptyNetGoals">Number of empty net goals scored.</param>
        /// <param name="delayedPenaltyGoals">Number of goals scored during delayed penalty.</param>
        /// <param name="ownGoals">Number of own goals scored.</param>
        /// <param name="gameWinningGoals">Number of winning goals scored.</param>
        /// <param name="goalsWithoutAssist">Number of scored goals without assist.</param>
        public PlayerStats(int gamesPlayed, int goals, int assists, int ppGoals, int shGoals, int evGoals, int penaltyMinutes, int penaltyShots, int penaltyShotGoals,
                           int emptyNetGoals, int delayedPenaltyGoals, int ownGoals, int gameWinningGoals, int goalsWithoutAssist)
        {
            GamesPlayed = gamesPlayed;
            Goals = goals;
            Assists = assists;
            PpGoals = ppGoals;
            ShGoals = shGoals;
            EvGoals = evGoals;
            PenaltyMinutes = penaltyMinutes;
            PenaltyShots = penaltyShots;
            PenaltyShotGoals = penaltyShotGoals;
            EmptyNetGoals = emptyNetGoals;
            DelayedPenaltyGoals = delayedPenaltyGoals;
            OwnGoals = ownGoals;
            GameWinningGoals = gameWinningGoals;
            GoalsWithoutAssist = goalsWithoutAssist;
        }

        /// <summary>
        /// Loads all stats of a player asynchronously by category.
        /// </summary>
        /// <param name="player">Player for which the stats will be loaded.</param>
        /// <param name="seasonID">Identification number of season from witch the stats will be loaded.</param>
        /// <param name="competitionID">Identification number of competition from witch the stats will be loaded.</param>
        public PlayerStats(Player player, int seasonID, int competitionID)
        {
            CalculateStats(player.ID, seasonID, competitionID).Await();
        }

        /// <summary>
        /// Loads match stats asynchronously.
        /// </summary>
        /// <param name="playerID">Identification number of player for which the stats will be loaded.</param>
        /// <param name="seasonID">Identification number of season from witch the stats will be loaded.</param>
        /// <param name="competitionID">Identification number of competition from witch the stats will be loaded.</param>
        /// <returns>The task calculating the stats.</returns>
        public async Task CalculateStats(int playerID, int seasonID, int competitionID)
        {
            List<Task> tasks = new();
            tasks.Add(Task.Run(() => CountGamesPlayed(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountGoals(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountAssists(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountPenaltyMinutes(playerID, seasonID, competitionID)));
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Calculate number of played games asynchronously.
        /// </summary>
        /// <param name="playerID">Identification number of player for which the stats will be loaded.</param>
        /// <param name="seasonID">Identification number of season from witch the stats will be loaded.</param>
        /// <param name="competitionID">Identification number of competition from witch the stats will be loaded.</param>
        /// <returns>The task calculating the stats.</returns>
        public async Task CountGamesPlayed(int playerID, int seasonID, int competitionID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) " +
                                                "FROM player_matches " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE player_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                GamesPlayed = (int)(long)await cmd.ExecuteScalarAsync();
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

        /// <summary>
        /// Calculate number of scored goals asynchronously.
        /// </summary>
        /// <param name="playerID">Identification number of player for which the stats will be loaded.</param>
        /// <param name="seasonID">Identification number of season from witch the stats will be loaded.</param>
        /// <param name="competitionID">Identification number of competition from witch the stats will be loaded.</param>
        /// <returns>The task calculating the stats.</returns>
        public async Task CountGoals(int playerID, int seasonID, int competitionID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) " +
                                                "FROM goals " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE own_goal = 0 AND player_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                Goals = (int)(long)await cmd.ExecuteScalarAsync();
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

        /// <summary>
        /// Calculate number of assists asynchronously.
        /// </summary>
        /// <param name="playerID">Identification number of player for which the stats will be loaded.</param>
        /// <param name="seasonID">Identification number of season from witch the stats will be loaded.</param>
        /// <param name="competitionID">Identification number of competition from witch the stats will be loaded.</param>
        /// <returns>The task calculating the stats.</returns>
        public async Task CountAssists(int playerID, int seasonID, int competitionID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) " +
                                                "FROM goals " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE assist_player_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                Assists = (int)(long)await cmd.ExecuteScalarAsync();
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

        /// <summary>
        /// Calculate number of recieved penalty minutes asynchronously.
        /// </summary>
        /// <param name="playerID">Identification number of player for which the stats will be loaded.</param>
        /// <param name="seasonID">Identification number of season from witch the stats will be loaded.</param>
        /// <param name="competitionID">Identification number of competition from witch the stats will be loaded.</param>
        /// <returns>The task calculating the stats.</returns>
        public async Task CountPenaltyMinutes(int playerID, int seasonID, int competitionID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COALESCE(SUM(p.minutes), 0) " +
                                                "FROM penalties " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "INNER JOIN penalty_type AS p ON p.code = penalty_type_id " +
                                                "WHERE player_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                PenaltyMinutes = Convert.ToInt32(await cmd.ExecuteScalarAsync());
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

    /// <summary>
    /// Class representing statistics of a goaltender.
    /// </summary>
    public class GoalieStats : NotifyPropertyChanged, IStats
    {
        #region Properties
        private int gamesPlayed;
        /// <summary>
        /// Number of played games.
        /// </summary>
        public int GamesPlayed
        {
            get => gamesPlayed;
            set
            {
                gamesPlayed = value;
                OnPropertyChanged();
            }
        }

        private int shutouts;
        /// <summary>
        /// Number of shutouts (full games without recieving a goal).
        /// </summary>
        public int Shutouts
        {
            get => shutouts;
            set
            {
                shutouts = value;
                OnPropertyChanged();
            }
        }

        private int timeInGame;
        /// <summary>
        /// Time spent in game in seconds.
        /// </summary>
        public int TimeInGame
        {
            get => timeInGame;
            set
            {
                timeInGame = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Time spent in game in format "mm:ss".
        /// </summary>
        public string TimeInGameText => (TimeInGame / 60) + ":" + (TimeInGame % 60).ToString("00");

        public string TimeInGamePerGame => GamesPlayed != 0 ? (TimeInGame / GamesPlayed / 60) + ":" + (TimeInGame / GamesPlayed % 60).ToString("00") : "0:00";

        private int goalsAgainst;
        /// <summary>
        /// Number of goals against.
        /// </summary>
        public int GoalsAgainst
        {
            get => goalsAgainst;
            set
            {
                goalsAgainst = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Number of goals against per game.
        /// </summary>
        public float GoalsAgainstPerGame => GamesPlayed != 0 ? (float)Math.Round(GoalsAgainst / (float)GamesPlayed, 2) : 0;

        /// <summary>
        /// Time spent in game per goal against in format "mm:ss".
        /// </summary>
        /// <remarks>If no goals against, returns "no goals against".</remarks>
        public string TimeInGamePerGoalAgainst => GoalsAgainst != 0 ? (TimeInGame / GoalsAgainst / 60) + ":" + (TimeInGame / GoalsAgainst % 60).ToString("00") : "no goal against";

        private int penaltyShotsAgainst;
        /// <summary>
        /// Number of penalty shot attempts against.
        /// </summary>
        public int PenaltyShotsAgainst
        {
            get => penaltyShotsAgainst;
            set
            {
                penaltyShotsAgainst = value;
                OnPropertyChanged();
            }
        }

        private int penaltyShotGoalsAgainst;
        /// <summary>
        /// Number of penalty shot goals against.
        /// </summary>
        public int PenaltyShotGoalsAgainst
        {
            get => penaltyShotGoalsAgainst;
            set
            {
                penaltyShotGoalsAgainst = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Percentage of successfully saved penalty shot attempts.
        /// </summary>
        /// <remarks>If no penalty shots against, returns NaN.</remarks>
        public float PenaltyShotsPercentage => PenaltyShotsAgainst != 0 ? (float)Math.Round((PenaltyShotsAgainst - PenaltyShotGoalsAgainst) / (float)PenaltyShotsAgainst * 100, 2) : float.NaN;

        private int shootoutShotsAgainst;
        /// <summary>
        /// Number of shootout shot attempts against.
        /// </summary>
        public int ShootoutShotsAgainst
        {
            get => shootoutShotsAgainst;
            set
            {
                shootoutShotsAgainst = value;
                OnPropertyChanged();
            }
        }

        private int shootoutShotGoalsAgainst;
        /// <summary>
        /// Number of shootout shot goals against.
        /// </summary>
        public int ShootoutShotGoalsAgainst
        {
            get => shootoutShotGoalsAgainst;
            set
            {
                shootoutShotGoalsAgainst = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Percentage of successfully saved shootout shot attempts.
        /// </summary>
        /// <remarks>If no shootout shots against, returns NaN.</remarks>
        public float ShootoutShotsPercentage => ShootoutShotsAgainst != 0 ? (float)Math.Round((ShootoutShotsAgainst - ShootoutShotGoalsAgainst) / (float)ShootoutShotsAgainst * 100, 2) : float.NaN;
        #endregion

        /// <summary>
        /// Stores goaltender statistics in a single object.
        /// </summary>
        /// <param name="gamesPlayed">Number of games played.</param>
        /// <param name="goalsAgainst">Number of goals against.</param>
        /// <param name="shutouts">Number of shutouts.</param>
        /// <param name="timeInGame">Time spent in game in seconds.</param>
        /// <param name="penaltyShots">Number of penalty shot attempts against.</param>
        /// <param name="penaltyShotGoalsAgainst">Number of penalty shot goals against.</param>
        /// <param name="shootoutShots">Number of shootout shot attempts against.</param>
        /// <param name="shootoutShotGoalsAgainst">Number of shootout shot goals against.</param>
        public GoalieStats(int gamesPlayed, int goalsAgainst, int shutouts, int timeInGame, int penaltyShots, int penaltyShotGoalsAgainst, int shootoutShots, int shootoutShotGoalsAgainst)
        {
            GamesPlayed = gamesPlayed;
            GoalsAgainst = goalsAgainst;
            Shutouts = shutouts;
            TimeInGame = timeInGame;
            PenaltyShotsAgainst = penaltyShots;
            PenaltyShotGoalsAgainst = penaltyShotGoalsAgainst;
            ShootoutShotsAgainst = shootoutShots;
            ShootoutShotGoalsAgainst = shootoutShotGoalsAgainst;
        }

        /// <summary>
        /// Loads all stats of a goaltender asynchronously by category.
        /// </summary>
        /// <param name="player">Player for which the stats will be loaded.</param>
        /// <param name="seasonID">Identification number of season from witch the stats will be loaded.</param>
        /// <param name="competitionID">Identification number of competition from witch the stats will be loaded.</param>
        public GoalieStats(Player player, int seasonID, int competitionID)
        {
            CalculateStats(player.ID, seasonID, competitionID).Await();
        }

        /// <summary>
        /// Loads goaltender stats asynchronously.
        /// </summary>
        /// <param name="playerID">Identification number of player for which the stats will be loaded.</param>
        /// <param name="seasonID">Identification number of season from witch the stats will be loaded.</param>
        /// <param name="competitionID">Identification number of competition from witch the stats will be loaded.</param>
        /// <returns>The task calculating the stats.</returns>
        public async Task CalculateStats(int playerID, int seasonID, int competitionID)
        {
            List<Task> tasks = new();
            tasks.Add(Task.Run(() => CountGamesPlayed(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountGoalsAgainst(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountShutouts(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountTimeInGame(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountPenaltyShots(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountPenaltyShotGoalsAgainst(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountShootoutShots(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountShootoutShotGoalsAgainst(playerID, seasonID, competitionID)));
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Calculate number of played games asynchronously.
        /// </summary>
        /// <param name="playerID">Identification number of player for which the stats will be loaded.</param>
        /// <param name="seasonID">Identification number of season from witch the stats will be loaded.</param>
        /// <param name="competitionID">Identification number of competition from witch the stats will be loaded.</param>
        /// <returns>The task calculating the stats.</returns>
        public async Task CountGamesPlayed(int playerID, int seasonID, int competitionID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) " +
                                                "FROM goalie_matches " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE player_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                GamesPlayed = (int)(long)await cmd.ExecuteScalarAsync();
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

        /// <summary>
        /// Calculate number of goals against asynchronously.
        /// </summary>
        /// <param name="playerID">Identification number of player for which the stats will be loaded.</param>
        /// <param name="seasonID">Identification number of season from witch the stats will be loaded.</param>
        /// <param name="competitionID">Identification number of competition from witch the stats will be loaded.</param>
        /// <returns>The task calculating the stats.</returns>
        public async Task CountGoalsAgainst(int playerID, int seasonID, int competitionID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) " +
                                                "FROM goals " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE goalie_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                GoalsAgainst = (int)(long)await cmd.ExecuteScalarAsync();
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

        /// <summary>
        /// Calculate number of shutouts asynchronously.
        /// </summary>
        /// <param name="playerID">Identification number of player for which the stats will be loaded.</param>
        /// <param name="seasonID">Identification number of season from witch the stats will be loaded.</param>
        /// <param name="competitionID">Identification number of competition from witch the stats will be loaded.</param>
        /// <returns>The task calculating the stats.</returns>
        public async Task CountShutouts(int playerID, int seasonID, int competitionID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) " +
                                                "FROM shutouts " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE goalie_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                Shutouts = (int)(long)await cmd.ExecuteScalarAsync();
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

        /// <summary>
        /// Calculate time in game asynchronously.
        /// </summary>
        /// <param name="playerID">Identification number of player for which the stats will be loaded.</param>
        /// <param name="seasonID">Identification number of season from witch the stats will be loaded.</param>
        /// <param name="competitionID">Identification number of competition from witch the stats will be loaded.</param>
        /// <returns>The task calculating the stats.</returns>
        public async Task CountTimeInGame(int playerID, int seasonID, int competitionID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COALESCE(SUM(duration), 0) " +
                                                "FROM shifts " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE player_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                TimeInGame = Convert.ToInt32(await cmd.ExecuteScalarAsync());
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

        /// <summary>
        /// Calculate number of penalty shot attempts against asynchronously.
        /// </summary>
        /// <param name="playerID">Identification number of player for which the stats will be loaded.</param>
        /// <param name="seasonID">Identification number of season from witch the stats will be loaded.</param>
        /// <param name="competitionID">Identification number of competition from witch the stats will be loaded.</param>
        /// <returns>The task calculating the stats.</returns>
        public async Task CountPenaltyShots(int playerID, int seasonID, int competitionID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) " +
                                                "FROM penalty_shots " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE goalie_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                PenaltyShotsAgainst = (int)(long)await cmd.ExecuteScalarAsync();
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

        /// <summary>
        /// Calculate number of penalty shot goals against asynchronously.
        /// </summary>
        /// <param name="playerID">Identification number of player for which the stats will be loaded.</param>
        /// <param name="seasonID">Identification number of season from witch the stats will be loaded.</param>
        /// <param name="competitionID">Identification number of competition from witch the stats will be loaded.</param>
        /// <returns>The task calculating the stats.</returns>
        public async Task CountPenaltyShotGoalsAgainst(int playerID, int seasonID, int competitionID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) " +
                                                "FROM penalty_shots " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE was_goal = 1 AND goalie_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                PenaltyShotGoalsAgainst = (int)(long)await cmd.ExecuteScalarAsync();
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

        /// <summary>
        /// Calculate number of shootout shot attempts against asynchronously.
        /// </summary>
        /// <param name="playerID">Identification number of player for which the stats will be loaded.</param>
        /// <param name="seasonID">Identification number of season from witch the stats will be loaded.</param>
        /// <param name="competitionID">Identification number of competition from witch the stats will be loaded.</param>
        /// <returns>The task calculating the stats.</returns>
        public async Task CountShootoutShots(int playerID, int seasonID, int competitionID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) " +
                                                "FROM shootout_shots " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE goalie_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                ShootoutShotsAgainst = (int)(long)await cmd.ExecuteScalarAsync();
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

        /// <summary>
        /// Calculate number of shootout shot goals against asynchronously.
        /// </summary>
        /// <param name="playerID">Identification number of player for which the stats will be loaded.</param>
        /// <param name="seasonID">Identification number of season from witch the stats will be loaded.</param>
        /// <param name="competitionID">Identification number of competition from witch the stats will be loaded.</param>
        /// <returns>The task calculating the stats.</returns>
        public async Task CountShootoutShotGoalsAgainst(int playerID, int seasonID, int competitionID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) " +
                                                "FROM shootout_shots " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE was_goal = 1 AND goalie_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                ShootoutShotGoalsAgainst = (int)(long)await cmd.ExecuteScalarAsync();
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

    /// <summary>
    /// Class representing statistics of a player in one match.
    /// </summary>
    public class PlayerInMatchStats : NotifyPropertyChanged
    {
        #region Properties
        private DateTime datetime;
        /// <summary>
        /// The date on which the match was played.
        /// </summary>
        public DateTime Datetime
        {
            get => datetime;
            set
            {
                datetime = value;
                OnPropertyChanged();
            }
        }

        private int goals;
        /// <summary>
        /// Number of goals scored in the match.
        /// </summary>
        public int Goals
        {
            get => goals;
            set
            {
                goals = value;
                OnPropertyChanged();
            }
        }

        private int assists;
        /// <summary>
        /// Number of assists in the match.
        /// </summary>
        public int Assists
        {
            get => assists;
            set
            {
                assists = value;
                OnPropertyChanged();
            }
        }

        private int points;
        /// <summary>
        /// Number of scored points in the match. Goals plus assists.
        /// </summary>
        public int Points
        {
            get => points;
            set
            {
                points = value;
                OnPropertyChanged();
            }
        }

        private int penaltyMinutes;
        /// <summary>
        /// Number of recieved penalty minutes in the match.
        /// </summary>
        public int PenaltyMinutes
        {
            get => penaltyMinutes;
            set
            {
                penaltyMinutes = value;
                OnPropertyChanged();
            }
        }
        #endregion
    }

    /// <summary>
    /// Queries player statistics by match.
    /// </summary>
    public static class PlayerInMatchStatsLoader
    {
        /// <summary>
        /// Loads player statistics by matches he played in.
        /// </summary>
        /// <param name="stats">Empty collection of PlayerInMatchStats.</param>
        /// <param name="datetimeXLabels">Labels for the x axis of the graph consisting of matches dates.</param>
        /// <param name="player">Player for which the stats will be loaded.</param>
        public static void LoadPlayerInMatchStats(ObservableCollection<PlayerInMatchStats> stats, out string[] datetimeXLabels, Player player)
        {
            datetimeXLabels = null;
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT m.match_id, matches.datetime AS datetime, IFNULL(g_count, 0) AS goal_count, IFNULL(a_count, 0) AS assist_count, IFNULL(p_count, 0) AS penalty_count " +
                                                "FROM player_matches AS m " +

                                                "INNER JOIN matches ON matches.id = m.match_id " +

                                                "LEFT JOIN " +
                                                "(SELECT g.match_id AS g_match_id, COUNT(g.player_id) AS g_count FROM goals AS g " +
                                                "WHERE g.own_goal = 0 AND g.player_id = " + player.ID + " " +
                                                "GROUP BY g.match_id) " +
                                                "AS g_table ON g_table.g_match_id = m.match_id " +

                                                "LEFT JOIN " +
                                                "(SELECT a.match_id AS a_match_id, COUNT(a.assist_player_id) AS a_count FROM goals AS a " +
                                                "WHERE a.assist_player_id = " + player.ID + " " +
                                                "GROUP BY a.match_id) " +
                                                "AS a_table ON a_table.a_match_id = m.match_id " +

                                                "LEFT JOIN " +
                                                "(SELECT p.match_id AS p_match_id, COALESCE(SUM(p_type.minutes), 0) AS p_count FROM penalties AS p " +
                                                " INNER JOIN penalty_type AS p_type ON p_type.code = p.penalty_type_id " +
                                                "WHERE p.player_id = " + player.ID + " " +
                                                "GROUP BY p.match_id) " +
                                                "AS p_table ON p_table.p_match_id = m.match_id " +

                                                "WHERE m.player_id = " + player.ID + " " +
                                                "GROUP BY m.match_id", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    PlayerInMatchStats p = new()
                    {
                        Datetime = DateTime.Parse(row["datetime"].ToString()),
                        Goals = int.Parse(row["goal_count"].ToString()),
                        Assists = int.Parse(row["assist_count"].ToString()),
                        PenaltyMinutes = int.Parse(row["penalty_count"].ToString()),
                        Points = int.Parse(row["goal_count"].ToString()) + int.Parse(row["assist_count"].ToString()),
                    };

                    stats.Add(p);
                }

                _ = stats.OrderBy(x => x.Datetime);
                datetimeXLabels = stats.Select(x => x.Datetime.ToString("d. M. yyyy")).ToArray();
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