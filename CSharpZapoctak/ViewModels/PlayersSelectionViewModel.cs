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
    class PlayersSelectionViewModel : ViewModelBase
    {
        #region Commands
        public ICommand NavigatePlayerCommand { get; set; }

        private ICommand checkNavigatePlayerCommand;
        public ICommand CheckNavigatePlayerCommand
        {
            get
            {
                if (checkNavigatePlayerCommand == null)
                {
                    checkNavigatePlayerCommand = new RelayCommand(param => CheckNavigatePlayer());
                }
                return checkNavigatePlayerCommand;
            }
        }

        private ICommand exportPDFCommand;
        public ICommand ExportPDFCommand
        {
            get
            {
                if (exportPDFCommand == null)
                {
                    exportPDFCommand = new RelayCommand(param => Export((System.Windows.Controls.DataGrid)param, "PDF"));
                }
                return exportPDFCommand;
            }
        }

        private ICommand exportXLSXCommand;
        public ICommand ExportXLSXCommand
        {
            get
            {
                if (exportXLSXCommand == null)
                {
                    exportXLSXCommand = new RelayCommand(param => Export((System.Windows.Controls.DataGrid)param, "XLSX"));
                }
                return exportXLSXCommand;
            }
        }
        #endregion

        #region Visibilities
        private bool showPhoto = true;
        public bool ShowPhoto
        {
            get { return showPhoto; }
            set
            {
                showPhoto = value;
                PhotoVisibility = showPhoto ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

        private bool showInfo = true;
        public bool ShowInfo
        {
            get { return showInfo; }
            set
            {
                showInfo = value;
                InfoVisibility = showInfo ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

        private bool showStats = true;
        public bool ShowStats
        {
            get { return showStats; }
            set
            {
                showStats = value;
                StatsVisibility = showStats ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

        private Visibility photoVisibility = Visibility.Visible;
        public Visibility PhotoVisibility
        {
            get { return photoVisibility; }
            set
            {
                photoVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility infoVisibility = Visibility.Visible;
        public Visibility InfoVisibility
        {
            get { return infoVisibility; }
            set
            {
                infoVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility statsVisibility = Visibility.Visible;
        public Visibility StatsVisibility
        {
            get { return statsVisibility; }
            set
            {
                statsVisibility = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public Player SelectedPlayer { get; set; }

        public ObservableCollection<Player> Players { get; set; }

        public PlayersSelectionViewModel(NavigationStore navigationStore)
        {
            NavigatePlayerCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new PlayerViewModel(navigationStore, SelectedPlayer)));
            SelectedPlayer = null;

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("", connection);

            ////////////////////////
            //string query = "SELECT p.* " +
            //               "FROM player_enlistment " +
            //               "INNER JOIN player AS p ON p.id = player_id " +
            //               "INNER JOIN seasons AS s ON s.id = season_id";
            //query += " WHERE player_id <> -1";
            //if (SportsData.competition.id != (int)EntityState.NotSelected && SportsData.competition.id != (int)EntityState.AddNew)
            //{
            //    query += " AND competition_id = " + SportsData.competition.id;
            //    if (SportsData.season.id != (int)EntityState.NotSelected && SportsData.season.id != (int)EntityState.AddNew)
            //    {
            //        query += " AND season_id = " + SportsData.season.id;
            //    }
            //}
            //query += " GROUP BY player_id";
            //
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            
            string matchCountQuery = "SELECT player_id, COUNT(*) AS match_count " +
                                    "FROM player_matches " +
                                    "INNER JOIN matches ON matches.id = match_id " +
                                    "INNER JOIN seasons ON seasons.id = matches.season_id";
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { matchCountQuery += " WHERE"; }
            if (SportsData.season.id > 0) { matchCountQuery += " matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 && SportsData.competition.id > 0) { matchCountQuery += " AND"; }
            if (SportsData.competition.id > 0) { matchCountQuery += " seasons.competition_id = " + SportsData.competition.id; }
            matchCountQuery += " GROUP BY player_id";

            string goalCountQuery = "SELECT player_id, COUNT(*) AS goal_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id";
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { goalCountQuery += " WHERE"; }
            if (SportsData.season.id > 0) { goalCountQuery += " matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 && SportsData.competition.id > 0) { goalCountQuery += " AND"; }
            if (SportsData.competition.id > 0) { goalCountQuery += " seasons.competition_id = " + SportsData.competition.id; }
            goalCountQuery += " GROUP BY player_id";

            string assistCountQuery = "SELECT assist_player_id, COUNT(*) AS assist_count " +
                                "FROM goals " +
                                "INNER JOIN matches ON matches.id = match_id " +
                                "INNER JOIN seasons ON seasons.id = matches.season_id";
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { assistCountQuery += " WHERE"; }
            if (SportsData.season.id > 0) { assistCountQuery += " matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 && SportsData.competition.id > 0) { assistCountQuery += " AND"; }
            if (SportsData.competition.id > 0) { assistCountQuery += " seasons.competition_id = " + SportsData.competition.id; }
            assistCountQuery += " GROUP BY assist_player_id";

            string penaltyMinutesQuery = "SELECT player_id, COALESCE(SUM(penalty_type.minutes), 0) AS penalty_minutes " +
                                                "FROM penalties " +
                                                "INNER JOIN matches ON matches.id = match_id " +
                                                "INNER JOIN seasons ON seasons.id = matches.season_id " +
                                                "INNER JOIN penalty_type ON penalty_type.code = penalty_type_id";
            if (SportsData.season.id > 0 || SportsData.competition.id > 0) { penaltyMinutesQuery += " WHERE"; }
            if (SportsData.season.id > 0) { penaltyMinutesQuery += " matches.season_id = " + SportsData.season.id; }
            if (SportsData.season.id > 0 && SportsData.competition.id > 0) { penaltyMinutesQuery += " AND"; }
            if (SportsData.competition.id > 0) { penaltyMinutesQuery += " seasons.competition_id = " + SportsData.competition.id; }
            penaltyMinutesQuery += " GROUP BY player_id";

            string playerStatsQuery = "SELECT p.*, s.competition_id, IFNULL(match_count, 0) AS match_count, IFNULL(goal_count, 0) AS goal_count, IFNULL(assist_count, 0) AS assist_count, IFNULL(penalty_minutes, 0) AS penalty_minutes " +
                          "FROM player_enlistment " +
                          "RIGHT JOIN player AS p ON p.id = player_id " +
                          "INNER JOIN seasons AS s ON s.id = season_id";

            playerStatsQuery += " LEFT JOIN (" + matchCountQuery + ") AS m ON m.player_id = p.id " +
                                "LEFT JOIN (" + goalCountQuery + ") AS g ON g.player_id = p.id " +
                                "LEFT JOIN (" + assistCountQuery + ") AS a ON a.assist_player_id = p.id " +
                                "LEFT JOIN (" + penaltyMinutesQuery + ") AS pm ON pm.player_id = p.id";

            playerStatsQuery += " WHERE p.id <> -1";
            if (SportsData.competition.id != (int)EntityState.NotSelected && SportsData.competition.id != (int)EntityState.AddNew)
            {
                playerStatsQuery += " AND s.competition_id = " + SportsData.competition.id;
                if (SportsData.season.id != (int)EntityState.NotSelected && SportsData.season.id != (int)EntityState.AddNew)
                {
                    playerStatsQuery += " AND season_id = " + SportsData.season.id;
                }
            }
            playerStatsQuery += " GROUP BY p.id";

            cmd.CommandText = playerStatsQuery;
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                Players = new ObservableCollection<Player>();

                foreach (DataRow row in dataTable.Rows)
                {
                    Player p = new Player
                    {
                        id = int.Parse(row["id"].ToString()),
                        FirstName = row["first_name"].ToString(),
                        LastName = row["last_name"].ToString(),
                        Birthdate = DateTime.Parse(row["birthdate"].ToString()),
                        Gender = row["gender"].ToString(),
                        Height = int.Parse(row["height"].ToString()),
                        Weight = int.Parse(row["weight"].ToString()),
                        PlaysWith = row["plays_with"].ToString(),
                        Citizenship = SportsData.countries.Where(x => x.CodeTwo == row["citizenship"].ToString()).First(),
                        BirthplaceCity = row["birthplace_city"].ToString(),
                        BirthplaceCountry = SportsData.countries.Where(x => x.CodeTwo == row["birthplace_country"].ToString()).First(),
                        Status = Convert.ToBoolean(int.Parse(row["status"].ToString())),
                        Info = row["info"].ToString()
                    };

                    string[] imgPath = System.IO.Directory.GetFiles(SportsData.PlayerPhotosPath, SportsData.sport.name + p.id + ".*");
                    if (imgPath.Length != 0)
                    {
                        p.PhotoPath = imgPath.First();
                    }
                    else
                    {
                        p.PhotoPath = p.Gender == "M" ? SportsData.ResourcesPath + "\\male.png" : SportsData.ResourcesPath + "\\female.png";
                    }

                    //p.Stats = new PlayerStats(p, SportsData.season.id, SportsData.competition.id);
                    p.Stats = new PlayerStats(int.Parse(row["match_count"].ToString()), int.Parse(row["goal_count"].ToString()), int.Parse(row["assist_count"].ToString()), int.Parse(row["penalty_minutes"].ToString()));

                    Players.Add(p);
                }
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

        private void CheckNavigatePlayer()
        {
            if (SelectedPlayer != null)
            {
                NavigatePlayerCommand.Execute(null);
            }
        }

        private void Export(System.Windows.Controls.DataGrid dataGrid, string format)
        {
            //p.Items;
            //create excel file
            Microsoft.Office.Interop.Excel.Application excelApplication = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel._Workbook excelWorkbook;
            excelWorkbook = excelApplication.Workbooks.Add();
            Microsoft.Office.Interop.Excel.Worksheet gamesheet = (Microsoft.Office.Interop.Excel.Worksheet)excelApplication.ActiveSheet;
            //Microsoft.Office.Interop.Excel.Worksheet gamesheet = (Microsoft.Office.Interop.Excel.Worksheet)excelWorkbook.Worksheets[1];

            //match info
            //gamesheet.Range["G" + 8].Value = SportsData.competition.Name;
            //gamesheet.Range["J" + 9].Value = SportsData.season.Name;
            //
            //if (serieMatchNumber < 1)
            //{
            //    gamesheet.Range["J" + 10].Value = "Group";
            //}
            //else if (qualificationID > 0)
            //{
            //    gamesheet.Range["J" + 10].Value = "Qualification";
            //}
            //else
            //{
            //    gamesheet.Range["J" + 10].Value = "Play-off";
            //}
            //
            ////teams
            //gamesheet.Range["A" + 3].Value = HomeTeam.Name;
            //gamesheet.Range["G" + 3].Value = AwayTeam.Name;
            //
            ////datetime
            //gamesheet.Range["C" + 1].Value = MatchDateTime.ToString("d");
            //gamesheet.Range["I" + 1].Value = MatchDateTime.ToString("HH:mm");
            //
            ////rosters
            //int row;
            //for (int i = 0; i < HomePlayers.Count; i++)
            //{
            //    row = i + 18;
            //    //number
            //    gamesheet.Range["A" + row].Value = HomePlayers[i].Number;
            //    //name
            //    gamesheet.Range["C" + row].Value = HomePlayers[i].Name;
            //}
            //for (int i = 0; i < AwayPlayers.Count; i++)
            //{
            //    row = i + 18;
            //    //number
            //    gamesheet.Range["L" + row].Value = AwayPlayers[i].Number;
            //    //name
            //    gamesheet.Range["N" + row].Value = AwayPlayers[i].Name;
            //}
            //
            ////select path
            //string gamesheetPath = "";
            //
            //SaveFileDialog saveFileDialog = new SaveFileDialog();
            //switch (format)
            //{
            //    case "PDF":
            //        saveFileDialog.Filter = "PDF Files | *.pdf";
            //        saveFileDialog.DefaultExt = "pdf";
            //        break;
            //    case "XLSX":
            //        saveFileDialog.Filter = "XLSX | *.xlsx";
            //        saveFileDialog.DefaultExt = "xlsx";
            //        break;
            //    default:
            //        break;
            //}
            //saveFileDialog.FileName = "gamesheet_" + MatchDateTime.ToString("yyyy_MM_dd_HH_mm") + "_" + HomeTeam.Name + "_vs_" + AwayTeam.Name;
            //
            //bool? result = saveFileDialog.ShowDialog();
            //if (result.ToString() != string.Empty)
            //{
            //    gamesheetPath = saveFileDialog.FileName;
            //
            //    switch (format)
            //    {
            //        case "PDF":
            //            //export to pdf
            //            try
            //            {
            //                excelWorkbook.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF, gamesheetPath);
            //            }
            //            catch (Exception) { }
            //            break;
            //        case "XLSX":
            //            excelWorkbook.SaveCopyAs(gamesheetPath);
            //            break;
            //        default:
            //            break;
            //    }
            //}
            //
            //excelWorkbook.Close(false);
        }
    }
}