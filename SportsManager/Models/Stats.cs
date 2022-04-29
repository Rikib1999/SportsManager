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
    public interface IStats { }

    public class SeasonStats : NotifyPropertyChanged, IStats
    {
        #region Properties
        private int matches;
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

        public SeasonStats(Season s)
        {
            CalculateStats(s.ID).Await();
        }

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

    public class TeamStats : NotifyPropertyChanged, IStats
    {
        #region Properties
        private int gamesPlayed;
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

    public class MatchStats : NotifyPropertyChanged, IStats
    {
        #region Properties
        private string partOfSeason = "";
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

        public MatchStats(int goals, int assists, int penaltyMinutes, string partOfSeason)
        {
            Goals = goals;
            Assists = assists;
            PenaltyMinutes = penaltyMinutes;
            PartOfSeason = partOfSeason;
        }

        public MatchStats(Match m)
        {
            CalculateStats(m.ID).Await();
        }

        public async Task CalculateStats(int matchID)
        {
            List<Task> tasks = new();
            tasks.Add(Task.Run(() => CountGoals(matchID)));
            tasks.Add(Task.Run(() => CountAssists(matchID)));
            tasks.Add(Task.Run(() => CountPenalties(matchID)));
            await Task.WhenAll(tasks);
        }

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

    public class PlayerStats : NotifyPropertyChanged, IStats
    {
        #region Properties
        private int gamesPlayed;
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
        public int Goals
        {
            get => goals;
            set
            {
                goals = value;
                OnPropertyChanged();
            }
        }

        public float GoalsPerGame => GamesPlayed != 0 ? (float)Math.Round(Goals / (float)GamesPlayed, 2) : float.NaN;

        private int assists;
        public int Assists
        {
            get => assists;
            set
            {
                assists = value;
                OnPropertyChanged();
            }
        }

        public float AssistsPerGame => GamesPlayed != 0 ? (float)Math.Round(Assists / (float)GamesPlayed, 2) : float.NaN;

        private int ppGoals;
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
        public int EvGoals
        {
            get => evGoals;
            set
            {
                evGoals = value;
                OnPropertyChanged();
            }
        }

        public int Points => Goals + Assists;

        private int penaltyMinutes;
        public int PenaltyMinutes
        {
            get => penaltyMinutes;
            set
            {
                penaltyMinutes = value;
                OnPropertyChanged();
            }
        }

        public string PenaltyMinutesPerGame => GamesPlayed != 0 ? (PenaltyMinutes / GamesPlayed) + ":" + (PenaltyMinutes * 60 / GamesPlayed % 60).ToString("00") : "0:00";

        private int penaltyShots;
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
        public int PenaltyShotGoals
        {
            get => penaltyShotGoals;
            set
            {
                penaltyShotGoals = value;
                OnPropertyChanged();
            }
        }

        public float PenaltyShotsPercentage => PenaltyShots != 0 ? (float)Math.Round(PenaltyShotGoals / (float)PenaltyShots * 100, 2) : float.NaN;

        private int emptyNetGoals;
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

        public PlayerStats(Player player, int seasonID, int competitionID)
        {
            CalculateStats(player.ID, seasonID, competitionID).Await();
        }

        public async Task CalculateStats(int playerID, int seasonID, int competitionID)
        {
            List<Task> tasks = new();
            tasks.Add(Task.Run(() => CountGamesPlayed(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountGoals(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountAssists(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountPenaltyMinutes(playerID, seasonID, competitionID)));
            await Task.WhenAll(tasks);
        }

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

    public class GoalieStats : NotifyPropertyChanged, IStats
    {
        #region Properties
        private int gamesPlayed;
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
        public int TimeInGame
        {
            get => timeInGame;
            set
            {
                timeInGame = value;
                OnPropertyChanged();
            }
        }

        public string TimeInGameText => (TimeInGame / 60) + ":" + (TimeInGame % 60).ToString("00");

        public string TimeInGamePerGame => GamesPlayed != 0 ? (TimeInGame / GamesPlayed / 60) + ":" + (TimeInGame / GamesPlayed % 60).ToString("00") : "0:00";

        private int goalsAgainst;
        public int GoalsAgainst
        {
            get => goalsAgainst;
            set
            {
                goalsAgainst = value;
                OnPropertyChanged();
            }
        }

        public float GoalsAgainstPerGame => GamesPlayed != 0 ? (float)Math.Round(GoalsAgainst / (float)GamesPlayed, 2) : 0;

        public string TimeInGamePerGoalAgainst => GoalsAgainst != 0 ? (TimeInGame / GoalsAgainst / 60) + ":" + (TimeInGame / GoalsAgainst % 60).ToString("00") : "no goal against";

        private int penaltyShotsAgainst;
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
        public int PenaltyShotGoalsAgainst
        {
            get => penaltyShotGoalsAgainst;
            set
            {
                penaltyShotGoalsAgainst = value;
                OnPropertyChanged();
            }
        }

        public float PenaltyShotsPercentage => PenaltyShotsAgainst != 0 ? (float)Math.Round((PenaltyShotsAgainst - PenaltyShotGoalsAgainst) / (float)PenaltyShotsAgainst * 100, 2) : float.NaN;

        private int shootoutShotsAgainst;
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
        public int ShootoutShotGoalsAgainst
        {
            get => shootoutShotGoalsAgainst;
            set
            {
                shootoutShotGoalsAgainst = value;
                OnPropertyChanged();
            }
        }

        public float ShootoutShotsPercentage => ShootoutShotsAgainst != 0 ? (float)Math.Round((ShootoutShotsAgainst - ShootoutShotGoalsAgainst) / (float)ShootoutShotsAgainst * 100, 2) : float.NaN;
        #endregion

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

        public GoalieStats(Player player, int seasonID, int competitionID)
        {
            CalculateStats(player.ID, seasonID, competitionID).Await();
        }

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
    /// Queries player stats by match.
    /// </summary>
    public class PlayerInMatchStats : NotifyPropertyChanged
    {
        #region Properties
        private DateTime datetime;
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

    public static class PlayerInMatchStatsLoader
    {
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