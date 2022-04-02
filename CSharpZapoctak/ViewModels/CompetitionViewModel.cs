using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CSharpZapoctak.ViewModels
{
    class CompetitionViewModel : NotifyPropertyChanged
    {
        private BitmapImage bitmap;
        public BitmapImage Bitmap
        {
            get { return bitmap; }
            set
            {
                bitmap = value;
                OnPropertyChanged();
            }
        }

        private ICommand deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new RelayCommand(param => Delete());
                }
                return deleteCommand;
            }
        }

        public NavigationStore ns;

        public Competition CurrentCompetition { get; set; }

        public ICommand NavigateAddEditCompetitionCommand { get; }

        public CompetitionViewModel(NavigationStore navigationStore)
        {
            NavigateAddEditCompetitionCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new AddEditCompetitionViewModel(navigationStore)));

            ns = navigationStore;
            CurrentCompetition = SportsData.competition;

            if (!string.IsNullOrWhiteSpace(CurrentCompetition.LogoPath) && CurrentCompetition.LogoPath != SportsData.ResourcesPath + "/add_icon.png")
            {
                MemoryStream ms = new MemoryStream();
                byte[] arrbytFileContent = File.ReadAllBytes(CurrentCompetition.LogoPath);
                ms.Write(arrbytFileContent, 0, arrbytFileContent.Length);
                ms.Position = 0;

                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                Bitmap = bitmap;
                GC.Collect();
            }
            else
            {
                CurrentCompetition.LogoPath = "";
            }
        }

        private void Delete()
        {
            MessageBoxResult msgResult = MessageBox.Show("Do you really want to delete this competition? All seasons and matches will be deleted.", "Delete competition", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (msgResult == MessageBoxResult.Yes)
            {
                //delete competition from DB
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlTransaction transaction = null;
                MySqlCommand cmd = new MySqlCommand("DELETE FROM competitions WHERE id = " + CurrentCompetition.id, connection);

                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();

                    //delete all player/goalie match enlistments and all stats
                    List<string> databases = new List<string> { "player_matches", "goalie_matches", "penalties", "goals", "penalty_shots", "shutouts", "shifts", "shootout_shots", "time_outs", "period_score", "game_state" };
                    foreach (string db in databases)
                    {
                        cmd = new MySqlCommand("DELETE " + db + ".* FROM " + db + " " +
                                               "INNER JOIN matches AS m ON m.id = match_id " +
                                               "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                               "WHERE s.competition_id = " + CurrentCompetition.id, connection);
                        cmd.Transaction = transaction;
                        cmd.ExecuteNonQuery();
                    }

                    //delete rounds, groups and brackets
                    databases = new List<string> { "rounds", "groups", "brackets" };
                    foreach (string db in databases)
                    {
                        cmd = new MySqlCommand("DELETE " + db + ".* FROM " + db + " " +
                                               "INNER JOIN seasons AS s ON s.id = season_id " +
                                               "WHERE s.competition_id = " + CurrentCompetition.id, connection);
                        cmd.Transaction = transaction;
                        cmd.ExecuteNonQuery();
                    }

                    //delete matches
                    cmd = new MySqlCommand("DELETE matches.* FROM matches " +
                                           "INNER JOIN seasons AS s ON s.id = season_id " +
                                           "WHERE s.competition_id = " + CurrentCompetition.id, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();

                    //get all team ids
                    cmd = new MySqlCommand("SELECT team_id FROM team_enlistment " +
                                           "INNER JOIN seasons AS s ON s.id = season_id " +
                                           "WHERE s.competition_id = " + CurrentCompetition.id, connection);
                    cmd.Transaction = transaction;
                    DataTable dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());

                    List<int> teams = new List<int>();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        teams.Add(int.Parse(row["team_id"].ToString()));
                    }

                    if (teams.Count > 0)
                    {
                        //delete team enlistments
                        cmd = new MySqlCommand("DELETE team_enlistment.* FROM team_enlistment " +
                                               "INNER JOIN seasons AS s ON s.id = season_id " +
                                               "WHERE s.competition_id = " + CurrentCompetition.id, connection);
                        cmd.Transaction = transaction;
                        cmd.ExecuteNonQuery();

                        foreach (int teamID in teams)
                        {
                            //count if there is any team enlistment
                            cmd = new MySqlCommand("SELECT COUNT(*) FROM team_enlistment WHERE team_id = " + teamID, connection);
                            cmd.Transaction = transaction;

                            if ((int)(long)cmd.ExecuteScalar() == 0)
                            {
                                //delete team
                                cmd = new MySqlCommand("DELETE FROM team WHERE id = " + teamID, connection);
                                cmd.Transaction = transaction;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    //get all player ids
                    cmd = new MySqlCommand("SELECT player_id FROM player_enlistment " +
                                           "INNER JOIN seasons AS s ON s.id = season_id " +
                                           "WHERE s.competition_id = " + CurrentCompetition.id + " GROUP BY player_id", connection);
                    cmd.Transaction = transaction;
                    dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());

                    List<int> players = new List<int>();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        players.Add(int.Parse(row["player_id"].ToString()));
                    }

                    if (players.Count > 0)
                    {
                        //delete player enlistments
                        cmd = new MySqlCommand("DELETE player_enlistment.* FROM player_enlistment " +
                                               "INNER JOIN seasons AS s ON s.id = season_id " +
                                               "WHERE s.competition_id = " + CurrentCompetition.id, connection);
                        cmd.Transaction = transaction;
                        cmd.ExecuteNonQuery();

                        foreach (int playerID in players)
                        {
                            //count if there is any player enlistment
                            cmd = new MySqlCommand("SELECT COUNT(*) FROM player_enlistment WHERE player_id = " + playerID, connection);
                            cmd.Transaction = transaction;

                            if ((int)(long)cmd.ExecuteScalar() == 0)
                            {
                                //delete player
                                cmd = new MySqlCommand("DELETE FROM player WHERE id = " + playerID, connection);
                                cmd.Transaction = transaction;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    //get all season ids
                    cmd = new MySqlCommand("SELECT id FROM seasons " +
                                           "WHERE competition_id = " + CurrentCompetition.id, connection);
                    cmd.Transaction = transaction;
                    dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());

                    List<int> seasons = new List<int>();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        seasons.Add(int.Parse(row["id"].ToString()));
                    }

                    //delete seasons
                    cmd = new MySqlCommand("DELETE FROM seasons " +
                                           "WHERE competition_id = " + CurrentCompetition.id, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();

                    //DELETE COMPETITION LOGO
                    //if there is logo in the database then delete it
                    //get previous logo
                    string[] previousImgPath = Directory.GetFiles(SportsData.CompetitionLogosPath, SportsData.sport.name + CurrentCompetition.id + ".*");
                    string previousFilePath = "";
                    //if it exists
                    if (previousImgPath.Length != 0)
                    {
                        previousFilePath = previousImgPath.First();
                    }
                    //delete photo
                    if (previousFilePath != "")
                    {
                        GC.Collect();
                        File.Delete(previousFilePath);
                    }

                    //DELETE SEASON LOGOS
                    foreach (int seasonID in seasons)
                    {
                        //if there is logo in the database then delete it
                        //get previous logo
                        previousImgPath = Directory.GetFiles(SportsData.SeasonLogosPath, SportsData.sport.name + seasonID + ".*");
                        previousFilePath = "";
                        //if it exists
                        if (previousImgPath.Length != 0)
                        {
                            previousFilePath = previousImgPath.First();
                        }
                        //delete photo
                        if (previousFilePath != "")
                        {
                            File.Delete(previousFilePath);
                        }
                    }

                    //DELETE TEAM LOGOS
                    foreach (int teamID in teams)
                    {
                        //if there is logo in the database then delete it
                        //get previous logo
                        previousImgPath = Directory.GetFiles(SportsData.TeamLogosPath, SportsData.sport.name + teamID + ".*");
                        previousFilePath = "";
                        //if it exists
                        if (previousImgPath.Length != 0)
                        {
                            previousFilePath = previousImgPath.First();
                        }
                        //delete logo
                        if (previousFilePath != "")
                        {
                            File.Delete(previousFilePath);
                        }
                    }

                    //DELETE PLAYER PHOTOS
                    //if there is photo in the database then delete it
                    //get previous photo
                    foreach (int playerID in players)
                    {
                        previousImgPath = Directory.GetFiles(SportsData.PlayerPhotosPath, SportsData.sport.name + playerID + ".*");
                        previousFilePath = "";
                        //if it exists
                        if (previousImgPath.Length != 0)
                        {
                            previousFilePath = previousImgPath.First();
                        }
                        //delete photo
                        if (previousFilePath != "")
                        {
                            File.Delete(previousFilePath);
                        }
                    }

                    transaction.Commit();
                    connection.Close();

                    //switch view
                    SportsData.competition.id = (int)EntityState.NotSelected;
                    SportsData.season.id = (int)EntityState.NotSelected;
                    new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new CompetitionsSelectionViewModel(ns))).Execute(null);
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    MessageBox.Show("Unable to connect to databse." + e.Message + e.StackTrace, "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }
    }
}