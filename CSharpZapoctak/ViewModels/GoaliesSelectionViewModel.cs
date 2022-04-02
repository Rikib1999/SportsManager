using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Others;
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
    class GoaliesSelectionViewModel : NotifyPropertyChanged
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
                    exportPDFCommand = new RelayCommand(param => Exports.ExportTable((System.Windows.Controls.DataGrid)param, "PDF", ExportTop));
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
                    exportXLSXCommand = new RelayCommand(param => Exports.ExportTable((System.Windows.Controls.DataGrid)param, "XLSX", ExportTop));
                }
                return exportXLSXCommand;
            }
        }
        #endregion

        private int? exportTop;
        public int? ExportTop
        {
            get { return exportTop; }
            set
            {
                exportTop = value;
                OnPropertyChanged();
            }
        }

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

        public GoaliesSelectionViewModel(NavigationStore navigationStore)
        {
            NavigatePlayerCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new PlayerViewModel(navigationStore, SelectedPlayer)));
            SelectedPlayer = null;

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT p.* " +
                                                "FROM goalie_matches " +
                                                "INNER JOIN player AS p ON p.id = player_id " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id ", connection);
            cmd.CommandText += " WHERE player_id <> -1";
            if (SportsData.competition.id != (int)EntityState.NotSelected && SportsData.competition.id != (int)EntityState.AddNew)
            {
                cmd.CommandText += " AND s.competition_id = " + SportsData.competition.id;
                if (SportsData.season.id != (int)EntityState.NotSelected && SportsData.season.id != (int)EntityState.AddNew)
                {
                    cmd.CommandText += " AND m.season_id = " + SportsData.season.id;
                }
            }
            cmd.CommandText += " GROUP BY player_id";

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
                        Citizenship = new Country { CodeTwo = row["citizenship"].ToString() },
                        BirthplaceCity = row["birthplace_city"].ToString(),
                        BirthplaceCountry = new Country { CodeTwo = row["birthplace_country"].ToString() },
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

                    p.Stats = new GoalieStats(p, SportsData.season.id, SportsData.competition.id);

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
    }
}