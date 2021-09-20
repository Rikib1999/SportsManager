using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class PlayerEnlistment : ViewModelBase
    {
        public int id;

        public ObservableCollection<PlayerEnlistment> parent;

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

        private Position position;
        public Position Position
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

        private ICommand deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new RelayCommand(param => Delete(param));
                }
                return deleteCommand;
            }
        }

        private void Delete(object param)
        {
            IList teamAndSerie = (IList)param;
            Team t = (Team)teamAndSerie[0];
            int seasonID = (int)teamAndSerie[1];

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlTransaction transaction = null;
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM player_matches " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "WHERE player_id = " + id + " AND team_id = " + t.id + " AND m.season_id = " + seasonID, connection);
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;
                if ((int)(long)cmd.ExecuteScalar() == 0)
                {
                    cmd = new MySqlCommand("DELETE FROM player_enlistment WHERE player_id = " + id + " AND team_id = " + t.id + " AND season_id = " + seasonID, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();

                    parent.Remove(this);

                    //if there are no more enlistments for the player delete player from database
                    cmd = new MySqlCommand("SELECT COUNT(*) FROM player_enlistment WHERE player_id = " + id, connection);
                    cmd.Transaction = transaction;
                    if ((int)(long)cmd.ExecuteScalar() == 0)
                    {
                        cmd = new MySqlCommand("DELETE FROM player WHERE id = " + id, connection);
                        cmd.Transaction = transaction;
                        cmd.ExecuteNonQuery();

                        string[] previousImgPath = Directory.GetFiles(SportsData.PlayerPhotosPath, SportsData.sport.name + id + ".*");
                        string previousFilePath = "";
                        //if it exists
                        if (previousImgPath.Length != 0)
                        {
                            previousFilePath = previousImgPath.First();
                        }
                        //delete photo
                        File.Delete(previousFilePath);
                    }
                }
                else
                {
                    MessageBox.Show("Unable to remove player enlistment because the player has played matches already. First remove player from all matches he played in given season for given team.", "Unable to remove player", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                transaction.Commit();
                connection.Close();
            }
            catch (Exception)
            {
                transaction.Rollback();
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private PlayerEnlistment editedPlayer;
        public PlayerEnlistment EditedPlayer
        {
            get { return editedPlayer; }
            set
            {
                editedPlayer = value;
                EditedNumber = editedPlayer.Number;
                EditedPosition = editedPlayer.Position;
                OnPropertyChanged();
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

        private int? editeddNumber = null;
        public int? EditedNumber
        {
            get { return editeddNumber; }
            set
            {
                editeddNumber = value;
                OnPropertyChanged();
            }
        }

        private Position editedPosition;
        public Position EditedPosition
        {
            get { return editedPosition; }
            set
            {
                editedPosition = value;
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
                    parent = Players,
                    Name = SelectedPlayer.FullName,
                    Position = SelectedPosition,
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

                    SelectedNumber = null;
                    SelectedPlayer = null;
                    SelectedPosition = null;
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

        private ICommand editPlayerCommand;
        public ICommand EditPlayerCommand
        {
            get
            {
                if (editPlayerCommand == null)
                {
                    editPlayerCommand = new RelayCommand(param => EditPlayer(param));
                }
                return editPlayerCommand;
            }
        }

        private void EditPlayer(object param)
        {
            IList teamAndSeasonID = param as IList;
            int teamID = ((Team)teamAndSeasonID[0]).id;
            int seasonID = (int)teamAndSeasonID[1];

            if (EditedPlayer == null || EditedPosition == null || EditedNumber == null)
            {
                MessageBox.Show("Please fill in all the fields", "Empty fields", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (EditedNumber != EditedPlayer.Number && Players.Any(x => x.Number == EditedNumber))
            {
                MessageBox.Show("Number " + EditedNumber + " is already taken.", "Number is taken", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("UPDATE player_enlistment SET number = " + EditedNumber + ", position_code = '" + EditedPosition.Code + "' " +
                                                    "WHERE player_id = " +EditedPlayer.id + " AND team_id = " + teamID + " AND season_id = " + seasonID, connection);
                try
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();

                    EditedPlayer.Number = (int)EditedNumber;
                    EditedPlayer.Position = EditedPosition;

                    EditedPlayer = new PlayerEnlistment();
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

        private string newFirstName;
        public string NewFirstName
        {
            get { return newFirstName; }
            set
            {
                newFirstName = value;
                OnPropertyChanged();
            }
        }

        private string newLastName;
        public string NewLastName
        {
            get { return newLastName; }
            set
            {
                newLastName = value;
                OnPropertyChanged();
            }
        }

        private DateTime? newBirthdate;
        public DateTime? NewBirthdate
        {
            get { return newBirthdate; }
            set
            {
                newBirthdate = value;
                OnPropertyChanged();
            }
        }

        private string newGender;
        public string NewGender
        {
            get { return newGender; }
            set
            {
                newGender = value;
                OnPropertyChanged();
            }
        }

        private int? newHeight = null;
        public int? NewHeight
        {
            get { return newHeight; }
            set
            {
                newHeight = value;
                OnPropertyChanged();
            }
        }

        private int? newWeight = null;
        public int? NewWeight
        {
            get { return newWeight; }
            set
            {
                newWeight = value;
                OnPropertyChanged();
            }
        }

        private string newPlaysWith;
        public string NewPlaysWith
        {
            get { return newPlaysWith; }
            set
            {
                newPlaysWith = value;
                OnPropertyChanged();
            }
        }

        private Country newCitizenship;
        public Country NewCitizenship
        {
            get { return newCitizenship; }
            set
            {
                newCitizenship = value;
                OnPropertyChanged();
            }
        }

        private string newBirthplaceCity;
        public string NewBirthplaceCity
        {
            get { return newBirthplaceCity; }
            set
            {
                newBirthplaceCity = value;
                OnPropertyChanged();
            }
        }

        private Country newBirthplaceCountry;
        public Country NewBirthplaceCountry
        {
            get { return newBirthplaceCountry; }
            set
            {
                newBirthplaceCountry = value;
                OnPropertyChanged();
            }
        }

        private string newInfo;
        public string NewInfo
        {
            get { return newInfo; }
            set
            {
                newInfo = value;
                OnPropertyChanged();
            }
        }

        private string newStatus = "";
        public string NewStatus
        {
            get { return newStatus; }
            set
            {
                newStatus = value;
                OnPropertyChanged();
            }
        }

        private ICommand addNewPlayerCommand;
        public ICommand AddNewPlayerCommand
        {
            get
            {
                if (addNewPlayerCommand == null)
                {
                    addNewPlayerCommand = new RelayCommand(param => AddNewPlayer(param));
                }
                return addNewPlayerCommand;
            }
        }

        private void AddNewPlayer(object param)
        {
            //TODO:
            IList teamSeasonIDViewModel = param as IList;
            int teamID = ((Team)teamSeasonIDViewModel[0]).id;
            int seasonID = (int)teamSeasonIDViewModel[1];

            if (string.IsNullOrWhiteSpace(NewFirstName) || string.IsNullOrWhiteSpace(NewLastName) || NewBirthdate == null
                || string.IsNullOrWhiteSpace(NewGender) || NewHeight == null || NewWeight == null || string.IsNullOrWhiteSpace(NewPlaysWith)
                || NewCitizenship == null || string.IsNullOrWhiteSpace(NewBirthplaceCity) || NewBirthplaceCountry == null
                || string.IsNullOrWhiteSpace(NewStatus))
            {
                MessageBox.Show("Please fill in all the fields", "Empty fields", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (Players.Any(x => x.Number == SelectedNumber))
            {
                MessageBox.Show("Number " + SelectedNumber + " is already taken.", "Number is taken", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                string gender = "M";
                if (NewGender == "Female") { gender = "F"; }
                string playsWith = "R";
                if (NewPlaysWith == "Left") { playsWith = "L"; }
                bool status = true;
                if (NewStatus == "Inactive") { status = false; }
                int newID = -2;

                //insert new player
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                //MySqlCommand cmd = new MySqlCommand(s, connection);
                MySqlCommand cmd = new MySqlCommand("INSERT INTO player(first_name, last_name, birthdate, gender, height, weight, plays_with, citizenship, birthplace_city, birthplace_country, status, info) " +
                                                    "VALUES ('" + NewFirstName + "', '" + NewLastName + "', '" + ((DateTime)NewBirthdate).ToString("yyyy-MM-dd H:mm:ss") + "', '" + gender + "'," +
                                                    " " + NewHeight + ", " + NewWeight + ", '" + playsWith + "', '" + NewCitizenship.CodeTwo + "', '" + NewBirthplaceCity + "', " +
                                                    "'" + NewBirthplaceCountry.CodeTwo + "'" +
                                                    ", " + Convert.ToInt32(status) + ", '" + NewInfo + "')", connection);
                try
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    newID = (int)cmd.LastInsertedId;

                    ((TeamViewModel)teamSeasonIDViewModel[2]).Players.Add(new Player { id = newID, FirstName = NewFirstName, LastName = NewLastName });

                    Players.Add(new PlayerEnlistment
                    {
                        id = newID,
                        parent = Players,
                        Name = NewFirstName + " " + NewLastName,
                        Position = SelectedPosition,
                        Number = (int)SelectedNumber
                    });

                    //insert player_enlistment
                    cmd = new MySqlCommand("INSERT INTO player_enlistment(player_id, team_id, season_id, number, position_code) " +
                                                        "VALUES (" + newID + ", " + teamID + ", " + seasonID + ", " + SelectedNumber + ", '" + SelectedPosition.Code + "')", connection);

                    cmd.ExecuteNonQuery();

                    SelectedNumber = null;
                    SelectedPlayer = null;
                    SelectedPosition = null;

                    NewFirstName = null;
                    NewLastName = null;
                    NewBirthdate = null;
                    NewGender = null;
                    NewHeight = null;
                    NewWeight = null;
                    NewPlaysWith = null;
                    NewCitizenship = null;
                    NewBirthplaceCity = null;
                    NewBirthplaceCountry = null;
                    NewStatus = null;
                    NewInfo = null;
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

        public ObservableCollection<string> Statuses { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> PlaysWith { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> Genders { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<Country> Countries { get; } = new ObservableCollection<Country>();

        public ObservableCollection<Player> Players { get; } = new ObservableCollection<Player>();

        public TeamViewModel(NavigationStore navigationStore, Team t)
        {
            NavigateEditTeamCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new EditTeamViewModel(navigationStore, CurrentTeam)));
            CurrentTeam = t;
            CurrentTeam.Country = SportsData.countries.Where(x => x.CodeTwo == CurrentTeam.Country.CodeTwo).First();
            Countries = SportsData.countries;
            LoadStatuses();
            LoadGenders();
            LoadPlaysWith();
            LoadTeamInfo();
            LoadPositions();
            LoadEnlistments();
            LoadPlayers();
        }

        #region Loading
        private void LoadStatuses()
        {
            Statuses = new ObservableCollection<string>();
            Statuses.Add("Active");
            Statuses.Add("Inactive");
        }

        private void LoadGenders()
        {
            Genders = new ObservableCollection<string>();
            Genders.Add("Male");
            Genders.Add("Female");
        }

        private void LoadPlaysWith()
        {
            PlaysWith = new ObservableCollection<string>();
            PlaysWith.Add("Right");
            PlaysWith.Add("Left");
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
                                                "WHERE team_id = " + CurrentTeam.id + " ORDER BY number", connection);

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
                        parent = CompetitionEnlistments.Where(x => x.CompetitionName == competition).First().SeasonDictionary.Seasons.Where(x => x.SeasonID == seasonID).First().Season.Item2.Players,
                        Name = row["player_first_name"].ToString() + " " + row["player_last_name"].ToString(),
                        Number = int.Parse(row["number"].ToString()),
                        Position = Positions.First(x => x.Name == row["position"].ToString())
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
                    if (int.Parse(row["id"].ToString()) != -1)
                    {
                        Players.Add(new Player { id = int.Parse(row["id"].ToString()), FirstName = row["first_name"].ToString(), LastName = row["last_name"].ToString() });
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
        #endregion
    }
}