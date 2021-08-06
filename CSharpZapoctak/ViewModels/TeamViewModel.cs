using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class PlayerEnlistment : ViewModelBase
    {
        public int id;

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string position;
        public string Position
        {
            get { return position; }
            set
            {
                position = value;
                OnPropertyChanged();
            }
        }

        private int number;
        public int Number
        {
            get { return number; }
            set
            {
                number = value;
                OnPropertyChanged();
            }
        }
    }

    class PlayerEnlistmentDictionary : ViewModelBase
    {
        public int SeasonID { get; set; }

        private Tuple<string, PlayerList> season;
        public Tuple<string, PlayerList> Season
        {
            get { return season; }
            set
            {
                season = value;
                OnPropertyChanged();
            }
        }
    }

    class CompetitionDictionary : ViewModelBase
    {
        public string CompetitionName { get; set; }

        private SeasonDictionary seasonDictionary;
        public SeasonDictionary SeasonDictionary
        {
            get { return seasonDictionary; }
            set
            {
                seasonDictionary = value;
                OnPropertyChanged();
            }
        }
    }

    class SeasonDictionary : ViewModelBase
    {
        private ObservableCollection<PlayerEnlistmentDictionary> seasons = new ObservableCollection<PlayerEnlistmentDictionary>();
        public ObservableCollection<PlayerEnlistmentDictionary> Seasons
        {
            get { return seasons; }
            set
            {
                seasons = value;
                OnPropertyChanged();
            }
        }

        private Visibility competitionVisibility = Visibility.Collapsed;
        public Visibility CompetitionVisibility
        {
            get { return competitionVisibility; }
            set
            {
                competitionVisibility = value;
                OnPropertyChanged();
            }
        }


        private ICommand setCompetitionVisibilityCommand;
        public ICommand SetCompetitionVisibilityCommand
        {
            get
            {
                if (setCompetitionVisibilityCommand == null)
                {
                    setCompetitionVisibilityCommand = new RelayCommand(param => SetCompetitionVisibility());
                }
                return setCompetitionVisibilityCommand;
            }
        }

        private void SetCompetitionVisibility()
        {
            if (CompetitionVisibility == Visibility.Visible)
            {
                CompetitionVisibility = Visibility.Collapsed;
            }
            else
            {
                CompetitionVisibility = Visibility.Visible;
            }
        }
    }

    class PlayerList : ViewModelBase
    {
        private ObservableCollection<PlayerEnlistment> players = new ObservableCollection<PlayerEnlistment>();
        public ObservableCollection<PlayerEnlistment> Players
        {
            get { return players; }
            set
            {
                players = value;
                OnPropertyChanged();
            }
        }

        private Visibility seasonVisibility = Visibility.Collapsed;
        public Visibility SeasonVisibility
        {
            get { return seasonVisibility; }
            set
            {
                seasonVisibility = value;
                OnPropertyChanged();
            }
        }


        private ICommand setSeasonVisibilityCommand;
        public ICommand SetSeasonVisibilityCommand
        {
            get
            {
                if (setSeasonVisibilityCommand == null)
                {
                    setSeasonVisibilityCommand = new RelayCommand(param => SetSeasonVisibility());
                }
                return setSeasonVisibilityCommand;
            }
        }

        private void SetSeasonVisibility()
        {
            if (SeasonVisibility == Visibility.Visible)
            {
                SeasonVisibility = Visibility.Collapsed;
            }
            else
            {
                SeasonVisibility = Visibility.Visible;
            }
        }

        private Player selectedPlayer;
        public Player SelectedPlayer
        {
            get { return selectedPlayer; }
            set
            {
                selectedPlayer = value;
                OnPropertyChanged();
            }
        }

        private int? selectedNumber = null;
        public int? SelectedNumber
        {
            get { return selectedNumber; }
            set
            {
                selectedNumber = value;
                OnPropertyChanged();
            }
        }

        private Position selectedPosition;
        public Position SelectedPosition
        {
            get { return selectedPosition; }
            set
            {
                selectedPosition = value;
                OnPropertyChanged();
            }
        }

        private ICommand addPlayerCommand;
        public ICommand AddPlayerCommand
        {
            get
            {
                if (addPlayerCommand == null)
                {
                    addPlayerCommand = new RelayCommand(param => AddPlayer(param));
                }
                return addPlayerCommand;
            }
        }

        private void AddPlayer(object param)
        {
            IList teamAndSeasonID = param as IList;
            int teamID = ((Team)teamAndSeasonID[0]).id;
            int seasonID = (int)teamAndSeasonID[1];

            if (SelectedPlayer == null || SelectedPosition == null || SelectedNumber == null)
            {
                MessageBox.Show("Please fill in all the fields", "Empty fields", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (Players.Any(x => x.id == SelectedPlayer.id))
            {
                MessageBox.Show("Selected player is already enlisted.", "Player already enlisted", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (Players.Any(x => x.Number == SelectedNumber))
            {
                MessageBox.Show("Number " + SelectedNumber + " is already taken.", "Number is taken", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                Players.Add(new PlayerEnlistment
                {
                    id = SelectedPlayer.id,
                    Name = SelectedPlayer.FullName,
                    Position = SelectedPosition.Name,
                    Number = (int)SelectedNumber
                });

                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("INSERT INTO player_enlistment(player_id, team_id, season_id, number, position_code) " +
                                                    "VALUES (" + SelectedPlayer.id + ", " + teamID + ", " + seasonID + ", " + SelectedNumber + ", '" + SelectedPosition.Code + "')", connection);

                try
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
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
    }

    class TeamViewModel : ViewModelBase
    {
        public Team CurrentTeam { get; set; }

        public ICommand NavigateEditTeamCommand { get; }

        private ObservableCollection<CompetitionDictionary> competitionEnlistments;
        public ObservableCollection<CompetitionDictionary> CompetitionEnlistments
        {
            get { return competitionEnlistments; }
            set
            {
                competitionEnlistments = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Position> Positions { get; } = new ObservableCollection<Position>();

        public ObservableCollection<Player> Players { get; } = new ObservableCollection<Player>();

        public TeamViewModel(NavigationStore navigationStore, Team t)
        {
            NavigateEditTeamCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new EditTeamViewModel(navigationStore, CurrentTeam)));
            CurrentTeam = t;
            CurrentTeam.Country = SportsData.countries.Where(x => x.CodeTwo == CurrentTeam.Country.CodeTwo).First();
            LoadTeamInfo();
            LoadEnlistments();
            LoadPositions();
            LoadPlayers();
        }

        private void LoadTeamInfo()
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT info FROM team WHERE id = " + CurrentTeam.id, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                CurrentTeam.Info = dataTable.Rows[0]["info"].ToString();
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

        private void LoadEnlistments()
        {
            CompetitionEnlistments = new ObservableCollection<CompetitionDictionary>();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT player_id, season_id, number, p.first_name AS player_first_name, p.last_name AS player_last_name, pos.name AS position, s.name AS season_name, c.name AS competition_name " +
                                                "FROM player_enlistment " +
                                                "INNER JOIN player AS p ON p.id = player_id " +
                                                "INNER JOIN position AS pos ON pos.code = position_code " +
                                                "INNER JOIN seasons AS s ON s.id = season_id " +
                                                "INNER JOIN competitions AS c ON c.id = s.competition_id " +
                                                "WHERE team_id = " + CurrentTeam.id, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    string competition = row["competition_name"].ToString();
                    int seasonID = int.Parse(row["season_id"].ToString());

                    if (!CompetitionEnlistments.Any(x => x.CompetitionName == competition))
                    {
                        CompetitionEnlistments.Add(new CompetitionDictionary { CompetitionName = competition, SeasonDictionary = new SeasonDictionary() });
                    }
                    if (!CompetitionEnlistments.Where(x => x.CompetitionName == competition).First().SeasonDictionary.Seasons.Any(x => x.SeasonID == seasonID))
                    {
                        CompetitionEnlistments.Where(x => x.CompetitionName == competition).First().SeasonDictionary.Seasons.Add(new PlayerEnlistmentDictionary { SeasonID = seasonID, Season = Tuple.Create(row["season_name"].ToString(), new PlayerList()) });
                    }
                    CompetitionEnlistments.Where(x => x.CompetitionName == competition).First().SeasonDictionary.Seasons.Where(x => x.SeasonID == seasonID).First().Season.Item2.Players.Add(new PlayerEnlistment
                    {
                        id = int.Parse(row["player_id"].ToString()),
                        Name = row["player_first_name"].ToString() + " " + row["player_last_name"].ToString(),
                        Number = int.Parse(row["number"].ToString()),
                        Position = row["position"].ToString()
                    }); ;
                }

                //load seasons where no players are enlisted
                cmd = new MySqlCommand("SELECT season_id, s.name AS season_name, c.name AS competition_name " +
                                                "FROM team_enlistment " +
                                                "INNER JOIN seasons AS s ON s.id = season_id " +
                                                "INNER JOIN competitions AS c ON c.id = s.competition_id " +
                                                "WHERE team_id = " + CurrentTeam.id, connection);

                dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    string competition = row["competition_name"].ToString();
                    int seasonID = int.Parse(row["season_id"].ToString());

                    if (!CompetitionEnlistments.Any(x => x.CompetitionName == competition))
                    {
                        CompetitionEnlistments.Add(new CompetitionDictionary { CompetitionName = competition, SeasonDictionary = new SeasonDictionary() });
                    }
                    if (!CompetitionEnlistments.Where(x => x.CompetitionName == competition).First().SeasonDictionary.Seasons.Any(x => x.SeasonID == seasonID))
                    {
                        CompetitionEnlistments.Where(x => x.CompetitionName == competition).First().SeasonDictionary.Seasons.Add(new PlayerEnlistmentDictionary { SeasonID = seasonID, Season = Tuple.Create(row["season_name"].ToString(), new PlayerList()) });
                    }
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

        private void LoadPositions()
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT code, name FROM position", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    Positions.Add(new Position { Name = row["name"].ToString(), Code = row["code"].ToString() });
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

        private void LoadPlayers()
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT id, first_name, last_name FROM player", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    Players.Add(new Player { id = int.Parse(row["id"].ToString()), FirstName = row["first_name"].ToString(), LastName = row["last_name"].ToString() });
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
    }
}