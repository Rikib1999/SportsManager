﻿using SportsManager.Commands;
using SportsManager.Models;
using SportsManager.Others;
using SportsManager.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;

namespace SportsManager.ViewModels
{
    public class CompetitionViewModel : TemplateEntityViewModel
    {
        public Competition Competition { get; set; }

        public CompetitionViewModel(NavigationStore navigationStore)
        {
            Competition = SportsData.COMPETITION;
            NavigateEditCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new AddEditCompetitionViewModel(navigationStore)));
            NavigateBackCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new CompetitionsSelectionViewModel(navigationStore)));

            if (!string.IsNullOrWhiteSpace(Competition.ImagePath) && Competition.ImagePath != SportsData.ResourcesPath + "/add_icon.png")
            {
                Bitmap = ImageHandler.ImageToBitmap(Competition.ImagePath);
            }
            else
            {
                Competition.ImagePath = "";
            }
        }

        protected override void Delete()
        {
            MessageBoxResult msgResult = MessageBox.Show("Do you really want to delete this competition? All seasons and matches will be deleted.", "Delete competition", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (msgResult == MessageBoxResult.Yes)
            {
                //delete competition from DB
                MySqlConnection connection = new(SportsData.ConnectionStringSport);
                MySqlTransaction transaction = null;
                MySqlCommand cmd = new("DELETE FROM competitions WHERE id = " + Competition.ID, connection);

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
                                               "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                               "WHERE s.competition_id = " + Competition.ID, connection)
                        {
                            Transaction = transaction
                        };
                        _ = cmd.ExecuteNonQuery();
                    }

                    //delete rounds, groups and brackets
                    databases = new List<string> { "rounds", "groups", "brackets" };
                    foreach (string db in databases)
                    {
                        cmd = new MySqlCommand("DELETE " + db + ".* FROM " + db + " " +
                                               "INNER JOIN seasons AS s ON s.id = season_id " +
                                               "WHERE s.competition_id = " + Competition.ID, connection)
                        {
                            Transaction = transaction
                        };
                        _ = cmd.ExecuteNonQuery();
                    }

                    //delete matches
                    cmd = new MySqlCommand("DELETE matches.* FROM matches " +
                                           "INNER JOIN seasons AS s ON s.id = season_id " +
                                           "WHERE s.competition_id = " + Competition.ID, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();

                    //get all team ids
                    cmd = new MySqlCommand("SELECT team_id FROM team_enlistment " +
                                           "INNER JOIN seasons AS s ON s.id = season_id " +
                                           "WHERE s.competition_id = " + Competition.ID, connection)
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
                        cmd = new MySqlCommand("DELETE team_enlistment.* FROM team_enlistment " +
                                               "INNER JOIN seasons AS s ON s.id = season_id " +
                                               "WHERE s.competition_id = " + Competition.ID, connection)
                        {
                            Transaction = transaction
                        };
                        _ = cmd.ExecuteNonQuery();

                        foreach (int teamID in teams)
                        {
                            //count if there is any team enlistment
                            cmd = new MySqlCommand("SELECT COUNT(*) FROM team_enlistment WHERE team_id = " + teamID, connection)
                            {
                                Transaction = transaction
                            };

                            if ((int)(long)cmd.ExecuteScalar() == 0)
                            {
                                //delete team
                                cmd = new MySqlCommand("DELETE FROM team WHERE id = " + teamID, connection)
                                {
                                    Transaction = transaction
                                };
                                _ = cmd.ExecuteNonQuery();

                                //if there is logo in the database then delete it
                                //get previous logo
                                previousImgPath = Directory.GetFiles(SportsData.TeamLogosPath, SportsData.SPORT.Name + teamID + ".*");
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
                        }
                    }

                    //get all player ids
                    cmd = new MySqlCommand("SELECT player_id FROM player_enlistment " +
                                           "INNER JOIN seasons AS s ON s.id = season_id " +
                                           "WHERE s.competition_id = " + Competition.ID + " GROUP BY player_id", connection)
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
                        cmd = new MySqlCommand("DELETE player_enlistment.* FROM player_enlistment " +
                                               "INNER JOIN seasons AS s ON s.id = season_id " +
                                               "WHERE s.competition_id = " + Competition.ID, connection)
                        {
                            Transaction = transaction
                        };
                        _ = cmd.ExecuteNonQuery();

                        foreach (int playerID in players)
                        {
                            //count if there is any player enlistment
                            cmd = new MySqlCommand("SELECT COUNT(*) FROM player_enlistment WHERE player_id = " + playerID, connection)
                            {
                                Transaction = transaction
                            };

                            if ((int)(long)cmd.ExecuteScalar() == 0)
                            {
                                //delete player
                                cmd = new MySqlCommand("DELETE FROM player WHERE id = " + playerID, connection)
                                {
                                    Transaction = transaction
                                };
                                _ = cmd.ExecuteNonQuery();
                            }

                            previousImgPath = Directory.GetFiles(SportsData.PlayerPhotosPath, SportsData.SPORT.Name + playerID + ".*");
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
                    }

                    //get all season ids
                    cmd = new MySqlCommand("SELECT id FROM seasons " +
                                           "WHERE competition_id = " + Competition.ID, connection)
                    {
                        Transaction = transaction
                    };
                    dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());

                    List<int> seasons = new();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        seasons.Add(int.Parse(row["id"].ToString()));
                    }

                    //delete seasons
                    cmd = new MySqlCommand("DELETE FROM seasons " +
                                           "WHERE competition_id = " + Competition.ID, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();

                    //DELETE COMPETITION LOGO
                    //if there is logo in the database then delete it
                    //get previous logo
                    previousImgPath = Directory.GetFiles(SportsData.CompetitionLogosPath, SportsData.SPORT.Name + Competition.ID + ".*");
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

                    //DELETE SEASON LOGOS
                    foreach (int seasonID in seasons)
                    {
                        //if there is logo in the database then delete it
                        //get previous logo
                        previousImgPath = Directory.GetFiles(SportsData.SeasonLogosPath, SportsData.SPORT.Name + seasonID + ".*");
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
                    NavigateBackCommand.Execute(new Competition());
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