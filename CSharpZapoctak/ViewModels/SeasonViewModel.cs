using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Others;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;

namespace CSharpZapoctak.ViewModels
{
    public class SeasonViewModel : TemplateEntityViewModel
    {
        public Season Season { get; set; }

        public SeasonViewModel(NavigationStore navigationStore)
        {
            Season = SportsData.SEASON;
            NavigateEditCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new EditSeasonViewModel(navigationStore)));
            NavigateBackCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new SeasonsSelectionViewModel(navigationStore)));

            if (!string.IsNullOrWhiteSpace(Season.ImagePath))
            {
                Bitmap = ImageHandler.ImageToBitmap(Season.ImagePath);
            }
            else
            {
                Season.ImagePath = "";
            }
        }

        protected override void Delete()
        {
            MessageBoxResult msgResult = MessageBox.Show("Do you really want to delete this season? All matches will be deleted.", "Delete season", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (msgResult == MessageBoxResult.Yes)
            {
                //delete season from DB
                MySqlConnection connection = new(SportsData.ConnectionStringSport);
                MySqlTransaction transaction = null;
                MySqlCommand cmd = new("DELETE FROM seasons WHERE id = " + Season.ID, connection);

                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    cmd.Transaction = transaction;
                    _ = cmd.ExecuteNonQuery();

                    //delete all player/goalie match enlistments and all stats
                    List<string> databases = new() { "player_matches", "goalie_matches", "penalties", "goals", "penalty_shots", "shutouts", "shifts", "shootout_shots", "time_outs", "period_score", "game_state" };
                    foreach (string db in databases)
                    {
                        cmd = new MySqlCommand("DELETE " + db + ".* FROM " + db + " " +
                                               "INNER JOIN matches AS m ON m.id = match_id " +
                                               "WHERE m.season_id = " + Season.ID, connection)
                        {
                            Transaction = transaction
                        };
                        _ = cmd.ExecuteNonQuery();
                    }

                    //delete rounds, groups and brackets
                    databases = new List<string> { "rounds", "groups", "brackets" };
                    foreach (string db in databases)
                    {
                        cmd = new MySqlCommand("DELETE FROM " + db + " WHERE season_id = " + Season.ID, connection)
                        {
                            Transaction = transaction
                        };
                        _ = cmd.ExecuteNonQuery();
                    }

                    //delete matches
                    cmd = new MySqlCommand("DELETE FROM matches WHERE season_id = " + Season.ID, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();

                    //get all team ids
                    cmd = new MySqlCommand("SELECT team_id FROM team_enlistment WHERE season_id = " + Season.ID, connection)
                    {
                        Transaction = transaction
                    };
                    DataTable dataTable = new();
                    dataTable.Load(cmd.ExecuteReader());

                    List<int> teams = new();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        teams.Add(int.Parse(row["team_id"].ToString()));
                    }

                    string[] previousImgPath;
                    string previousFilePath;
                    GC.Collect();

                    if (teams.Count > 0)
                    {
                        //delete team enlistments
                        cmd = new MySqlCommand("DELETE FROM team_enlistment WHERE season_id = " + Season.ID, connection)
                        {
                            Transaction = transaction
                        };
                        _ = cmd.ExecuteNonQuery();

                        for (int i = teams.Count - 1; i >= 0; i--)
                        {
                            //count if there is any team enlistment
                            cmd = new MySqlCommand("SELECT COUNT(*) FROM team_enlistment WHERE team_id = " + teams[i], connection)
                            {
                                Transaction = transaction
                            };

                            if ((int)(long)cmd.ExecuteScalar() == 0)
                            {
                                //delete team
                                cmd = new MySqlCommand("DELETE FROM team WHERE id = " + teams[i], connection)
                                {
                                    Transaction = transaction
                                };
                                _ = cmd.ExecuteNonQuery();

                                //if there is logo in the database then delete it
                                //get previous logo
                                previousImgPath = Directory.GetFiles(SportsData.TeamLogosPath, SportsData.SPORT.Name + teams[i] + ".*");
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
                            else
                            {
                                teams.RemoveAt(i);
                            }
                        }
                    }

                    //get all player ids
                    cmd = new MySqlCommand("SELECT player_id FROM player_enlistment WHERE season_id = " + Season.ID + " GROUP BY player_id", connection)
                    {
                        Transaction = transaction
                    };
                    dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());

                    List<int> players = new();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        players.Add(int.Parse(row["player_id"].ToString()));
                    }

                    if (players.Count > 0)
                    {
                        //delete player enlistments
                        cmd = new MySqlCommand("DELETE FROM player_enlistment WHERE season_id = " + Season.ID, connection)
                        {
                            Transaction = transaction
                        };
                        _ = cmd.ExecuteNonQuery();

                        for (int i = players.Count - 1; i >= 0; i--)
                        {
                            //count if there is any player enlistment
                            cmd = new MySqlCommand("SELECT COUNT(*) FROM player_enlistment WHERE player_id = " + players[i], connection)
                            {
                                Transaction = transaction
                            };

                            if ((int)(long)cmd.ExecuteScalar() == 0)
                            {
                                //delete player
                                cmd = new MySqlCommand("DELETE FROM player WHERE id = " + players[i], connection)
                                {
                                    Transaction = transaction
                                };
                                _ = cmd.ExecuteNonQuery();

                                previousImgPath = Directory.GetFiles(SportsData.PlayerPhotosPath, SportsData.SPORT.Name + players[i] + ".*");
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
                            else
                            {
                                players.RemoveAt(i);
                            }
                        }
                    }

                    //DELETE SEASON LOGO
                    //if there is logo in the database then delete it
                    //get previous logo
                    previousImgPath = Directory.GetFiles(SportsData.SeasonLogosPath, SportsData.SPORT.Name + Season.ID + ".*");
                    previousFilePath = "";
                    //if it exists
                    if (previousImgPath.Length != 0)
                    {
                        previousFilePath = previousImgPath.First();
                    }
                    //delete photo
                    if (previousFilePath != "")
                    {
                        if (Bitmap.StreamSource != null) { Bitmap.StreamSource.Dispose(); }
                        GC.Collect();
                        File.Delete(previousFilePath);
                    }

                    transaction.Commit();
                    connection.Close();

                    //switch view
                    NavigateBackCommand.Execute(new Season());
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    _ = MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
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