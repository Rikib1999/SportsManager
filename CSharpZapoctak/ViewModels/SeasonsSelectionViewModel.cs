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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class SeasonsSelectionViewModel : ViewModelBase
    {
        public class SeasonStats : ViewModelBase, IStats
        {
            #region Properties
            private string format = "";
            public string Format
            {
                get { return format; }
                set
                {
                    format = value;
                    OnPropertyChanged();
                }
            }

            private int matches = 0;
            public int Matches
            {
                get { return matches; }
                set
                {
                    matches = value;
                    OnPropertyChanged();
                }
            }

            private int teams = 0;
            public int Teams
            {
                get { return teams; }
                set
                {
                    teams = value;
                    OnPropertyChanged();
                }
            }

            private int players = 0;
            public int Players
            {
                get { return players; }
                set
                {
                    players = value;
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

            private float goalsPerGame = 0;
            public float GoalsPerGame
            {
                get { return goalsPerGame; }
                set
                {
                    goalsPerGame = value;
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

            private float assistsPerGame = 0;
            public float AssistsPerGame
            {
                get { return assistsPerGame; }
                set
                {
                    assistsPerGame = value;
                    OnPropertyChanged();
                }
            }

            private int penalties = 0;
            public int Penalties
            {
                get { return penalties; }
                set
                {
                    penalties = value;
                    OnPropertyChanged();
                }
            }

            private float penaltiesPerGame = 0;
            public float PenaltiesPerGame
            {
                get { return penaltiesPerGame; }
                set
                {
                    penaltiesPerGame = value;
                    OnPropertyChanged();
                }
            }
            #endregion

            public SeasonStats(Season s)
            {
                Format = s.Format();
                CalculateStats(s.id).Await();
            }

            /*TODO:
            public SeasonStats(ObservableCollection<Season> seasons)
            {
                foreach (Season s in seasons)
                {
                    Matches += ((SeasonStats)s.Stats).Matches;
                    Teams += ((SeasonStats)s.Stats).Teams;
                    Players += ((SeasonStats)s.Stats).Players;
                    Goals += ((SeasonStats)s.Stats).Goals;
                    Assists += ((SeasonStats)s.Stats).Assists;
                    Penalties += ((SeasonStats)s.Stats).Penalties;
                }

                if (Matches != 0)
                {
                    GoalsPerGame = (float)Math.Round((float)Goals / (float)Matches, 2);
                    AssistsPerGame = (float)Math.Round((float)Assists / (float)Matches, 2);
                    PenaltiesPerGame = (float)Math.Round((float)Penalties / (float)Matches, 2);
                }
            }*/

            public async Task CalculateStats(int seasonID)
            {
                List<Task> tasks = new List<Task>();
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
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM matches WHERE season_id = " + seasonID, connection);
                try
                {
                    connection.Open();
                    Matches = (int)(long)await cmd.ExecuteScalarAsync();
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

            private async Task CountTeams(int seasonID)
            {
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM team_enlistment WHERE season_id = " + seasonID, connection);
                try
                {
                    connection.Open();
                    Teams = (int)(long)await cmd.ExecuteScalarAsync();
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

            private async Task CountPlayers(int seasonID)
            {
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(DISTINCT player_id) FROM player_enlistment WHERE season_id = " + seasonID, connection);
                try
                {
                    connection.Open();
                    Players = (int)(long)await cmd.ExecuteScalarAsync();
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

            private async Task CountGoals(int seasonID)
            {
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM goals " +
                                                    "INNER JOIN matches AS m ON m.id = match_id " +
                                                    "WHERE m.season_id = " + seasonID, connection);
                try
                {
                    connection.Open();
                    Goals = (int)(long)await cmd.ExecuteScalarAsync();
                    if (Matches != 0) { GoalsPerGame = (float)Math.Round((float)Goals / (float)Matches, 2); }
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

            private async Task CountAssists(int seasonID)
            {
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM goals " +
                                                    "INNER JOIN matches AS m ON m.id = match_id " +
                                                    "WHERE assist_player_id <> -1 AND m.season_id = " + seasonID, connection);
                try
                {
                    connection.Open();
                    Assists = (int)(long)await cmd.ExecuteScalarAsync();
                    if (Matches != 0) { AssistsPerGame = (float)Math.Round((float)Assists / (float)Matches, 2); }
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

            private async Task CountPenalties(int seasonID)
            {
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM penalties " +
                                                    "INNER JOIN matches AS m ON m.id = match_id " +
                                                    "WHERE m.season_id = " + seasonID, connection);
                try
                {
                    connection.Open();
                    Penalties = (int)(long)await cmd.ExecuteScalarAsync();
                    if (Matches != 0) { PenaltiesPerGame = (float)Math.Round((float)Penalties / (float)Matches, 2); }
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

        public ICommand NavigateAddSeasonCommand { get; set; }

        public ICommand NavigateSeasonCommand { get; set; }

        private ICommand checkNavigateSeasonCommand;
        public ICommand CheckNavigateSeasonCommand
        {
            get
            {
                if (checkNavigateSeasonCommand == null)
                {
                    checkNavigateSeasonCommand = new RelayCommand(param => CheckNavigateSeason());
                }
                return checkNavigateSeasonCommand;
            }
        }

        public Season SelectedSeason { get; set; }

        public ObservableCollection<Season> Seasons { get; set; }

        public SeasonsSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateAddSeasonCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new AddSeasonViewModel(navigationStore)));
            NavigateSeasonCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new SeasonViewModel(navigationStore)));
            SelectedSeason = null;

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT w.name AS winner, c.name AS competition_name, seasons.id, competition_id, seasons.name, seasons.info, qualification_count, " +
                                                "qualification_rounds, group_count, play_off_rounds, play_off_best_of, play_off_started, points_for_W, points_for_OW, points_for_T, points_for_OL, points_for_L " +
                                                "FROM seasons " +
                                                "INNER JOIN competitions AS c ON c.id = seasons.competition_id", connection);
            if (SportsData.sport.name == "tennis")
            {
                cmd.CommandText += " INNER JOIN player AS w ON w.id = seasons.winner_id";
            }
            else
            {
                cmd.CommandText += " INNER JOIN team AS w ON w.id = seasons.winner_id";
            }
            if (SportsData.competition.Name != "" && SportsData.competition.id != (int)EntityState.NotSelected && SportsData.competition.id != (int)EntityState.AddNew)
            {
                cmd.CommandText += " WHERE competition_id = " + SportsData.competition.id;
            }

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                Seasons = new ObservableCollection<Season>();

                //name  format  winner  #matches    #teams      #players    goals   goals/g     assists     assists/g   penalties   penalties/g
                //name  format  winner  #matches    #teams      #players    goals   goals/g     assists     assists/g   yellow cards   red cards
                //name  format  winner  #matches    #players    #sets       service%            breaks      ...
                foreach (DataRow row in dataTable.Rows)
                {
                    Competition c = new Competition();
                    c.Name = row["competition_name"].ToString();
                    c.id = int.Parse(row["competition_id"].ToString());

                    Season s = new Season
                    {
                        id = int.Parse(row["id"].ToString()),
                        Competition = c,
                        Name = row["name"].ToString(),
                        Info = row["info"].ToString(),
                        QualificationCount = int.Parse(row["qualification_count"].ToString()),
                        QualificationRounds = int.Parse(row["qualification_rounds"].ToString()),
                        GroupCount = int.Parse(row["group_count"].ToString()),
                        PlayOffRounds = int.Parse(row["play_off_rounds"].ToString()),
                        PlayOffBestOf = int.Parse(row["play_off_best_of"].ToString()),
                        Winner = row["winner"].ToString(),
                        PlayOffStarted = Convert.ToBoolean(int.Parse(dataTable.Rows[0]["play_off_started"].ToString())),
                        PointsForWin = int.Parse(row["points_for_W"].ToString()),
                        PointsForOTWin = int.Parse(row["points_for_OW"].ToString()),
                        PointsForTie = int.Parse(row["points_for_T"].ToString()),
                        PointsForOTLoss = int.Parse(row["points_for_OL"].ToString()),
                        PointsForLoss = int.Parse(row["points_for_L"].ToString())
                    };
                    string[] imgPath = System.IO.Directory.GetFiles(SportsData.SeasonLogosPath, SportsData.sport.name + row["id"].ToString() + ".*");
                    if (imgPath.Length != 0)
                    {
                        s.LogoPath = imgPath.First();
                    }

                    s.Stats = new SeasonStats(s); ;
                    Seasons.Add(s);
                }
                /*
                Season all = new Season
                {
                    id = (int)EntityState.NotSelected,
                    Name = "All (" + Seasons.Count + ")",
                };
                all.Stats = new SeasonStats(Seasons);
                Seasons.Add(all);*/
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

        private void CheckNavigateSeason()
        {
            if (SelectedSeason != null)
            {
                NavigateSeasonCommand.Execute(SelectedSeason);
            }
        }
    }
}