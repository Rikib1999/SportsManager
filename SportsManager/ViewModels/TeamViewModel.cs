﻿using SportsManager.Commands;
using SportsManager.Models;
using SportsManager.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SportsManager.ViewModels
{
    public class PlayerEnlistment : NotifyPropertyChanged
    {
        public int ID { get; set; }

        public ObservableCollection<PlayerEnlistment> Parent { get; set; }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private Position position;
        public Position Position
        {
            get => position;
            set
            {
                position = value;
                OnPropertyChanged();
            }
        }

        private int number;
        public int Number
        {
            get => number;
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

            MySqlTransaction transaction = null;
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT COUNT(*) FROM player_matches " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "WHERE player_id = " + ID + " AND team_id = " + t.ID + " AND m.season_id = " + seasonID, connection);
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;
                if ((int)(long)cmd.ExecuteScalar() == 0)
                {
                    cmd = new MySqlCommand("DELETE FROM player_enlistment WHERE player_id = " + ID + " AND team_id = " + t.ID + " AND season_id = " + seasonID, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();

                    _ = Parent.Remove(this);

                    //if there are no more enlistments for the player delete player from database
                    cmd = new MySqlCommand("SELECT COUNT(*) FROM player_enlistment WHERE player_id = " + ID, connection)
                    {
                        Transaction = transaction
                    };
                    if ((int)(long)cmd.ExecuteScalar() == 0)
                    {
                        cmd = new MySqlCommand("DELETE FROM player WHERE id = " + ID, connection)
                        {
                            Transaction = transaction
                        };
                        _ = cmd.ExecuteNonQuery();

                        string[] previousImgPath = Directory.GetFiles(SportsData.PlayerPhotosPath, SportsData.SPORT.Name + ID + ".*");
                        string previousFilePath = "";
                        //if it exists
                        if (previousImgPath.Length != 0)
                        {
                            previousFilePath = previousImgPath.First();
                        }
                        //delete photo
                        if (!string.IsNullOrWhiteSpace(previousFilePath))
                        {
                            File.Delete(previousFilePath);
                        }
                    }
                }
                else
                {
                    _ = MessageBox.Show("Unable to remove player enlistment because the player has played matches already. First remove player from all matches he played in given season for given team.", "Unable to remove player", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                transaction.Commit();
                connection.Close();
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

    public class CompetitionRecord : NotifyPropertyChanged
    {
        public int CompetitionID { get; set; }

        public string CompetitionName { get; set; }

        private ObservableCollection<SeasonRecord> seasons = new();
        public ObservableCollection<SeasonRecord> Seasons
        {
            get => seasons;
            set
            {
                seasons = value;
                OnPropertyChanged();
            }
        }

        private Visibility competitionVisibility = Visibility.Collapsed;
        public Visibility CompetitionVisibility
        {
            get => competitionVisibility;
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
            CompetitionVisibility = CompetitionVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public class SeasonRecord : NotifyPropertyChanged
    {
        public int SeasonID { get; set; }

        public string SeasonName { get; set; }

        private ObservableCollection<PlayerEnlistment> players = new();
        public ObservableCollection<PlayerEnlistment> Players
        {
            get => players;
            set
            {
                players = value;
                OnPropertyChanged();
            }
        }

        private Visibility seasonVisibility = Visibility.Collapsed;
        public Visibility SeasonVisibility
        {
            get => seasonVisibility;
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
            SeasonVisibility = SeasonVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        #region Player enlistment

        #region Properties
        private PlayerEnlistment editedPlayer;
        public PlayerEnlistment EditedPlayer
        {
            get => editedPlayer;
            set
            {
                if (value != null)
                {
                    editedPlayer = value;
                    EditedNumber = editedPlayer.Number;
                    EditedPosition = editedPlayer.Position;
                    OnPropertyChanged();
                }
            }
        }

        private Player selectedPlayer;
        public Player SelectedPlayer
        {
            get => selectedPlayer;
            set
            {
                selectedPlayer = value;
                OnPropertyChanged();
            }
        }

        private int? selectedNumber;
        public int? SelectedNumber
        {
            get => selectedNumber;
            set
            {
                selectedNumber = value;
                OnPropertyChanged();
            }
        }

        private Position selectedPosition;
        public Position SelectedPosition
        {
            get => selectedPosition;
            set
            {
                selectedPosition = value;
                OnPropertyChanged();
            }
        }

        private int? editeddNumber;
        public int? EditedNumber
        {
            get => editeddNumber;
            set
            {
                editeddNumber = value;
                OnPropertyChanged();
            }
        }

        private Position editedPosition;
        public Position EditedPosition
        {
            get => editedPosition;
            set
            {
                editedPosition = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region New player properties
        private string newFirstName;
        public string NewFirstName
        {
            get => newFirstName;
            set
            {
                newFirstName = value;
                OnPropertyChanged();
            }
        }

        private string newLastName;
        public string NewLastName
        {
            get => newLastName;
            set
            {
                newLastName = value;
                OnPropertyChanged();
            }
        }

        private DateTime? newBirthdate;
        public DateTime? NewBirthdate
        {
            get => newBirthdate;
            set
            {
                newBirthdate = value;
                OnPropertyChanged();
            }
        }

        private string newGender;
        public string NewGender
        {
            get => newGender;
            set
            {
                newGender = value;
                OnPropertyChanged();
            }
        }

        private int? newHeight;
        public int? NewHeight
        {
            get => newHeight;
            set
            {
                newHeight = value;
                OnPropertyChanged();
            }
        }

        private int? newWeight;
        public int? NewWeight
        {
            get => newWeight;
            set
            {
                newWeight = value;
                OnPropertyChanged();
            }
        }

        private string newPlaysWith;
        public string NewPlaysWith
        {
            get => newPlaysWith;
            set
            {
                newPlaysWith = value;
                OnPropertyChanged();
            }
        }

        private Country newCitizenship;
        public Country NewCitizenship
        {
            get => newCitizenship;
            set
            {
                newCitizenship = value;
                OnPropertyChanged();
            }
        }

        private string newBirthplaceCity;
        public string NewBirthplaceCity
        {
            get => newBirthplaceCity;
            set
            {
                newBirthplaceCity = value;
                OnPropertyChanged();
            }
        }

        private Country newBirthplaceCountry;
        public Country NewBirthplaceCountry
        {
            get => newBirthplaceCountry;
            set
            {
                newBirthplaceCountry = value;
                OnPropertyChanged();
            }
        }

        private string newInfo;
        public string NewInfo
        {
            get => newInfo;
            set
            {
                newInfo = value;
                OnPropertyChanged();
            }
        }

        private string newStatus = "";
        public string NewStatus
        {
            get => newStatus;
            set
            {
                newStatus = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
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
        #endregion

        #region Methods
        private void AddPlayer(object param)
        {
            IList teamAndSeasonID = param as IList;
            int teamID = ((Team)teamAndSeasonID[0]).ID;
            int seasonID = (int)teamAndSeasonID[1];

            if (SelectedPlayer == null || SelectedPosition == null || SelectedNumber == null)
            {
                _ = MessageBox.Show("Please fill in all the fields", "Empty fields", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (Players.Any(x => x.ID == SelectedPlayer.ID))
            {
                _ = MessageBox.Show("Selected player is already enlisted.", "Player already enlisted", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (Players.Any(x => x.Number == SelectedNumber))
            {
                _ = MessageBox.Show("Number " + SelectedNumber + " is already taken.", "Number is taken", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                Players.Add(new PlayerEnlistment
                {
                    ID = SelectedPlayer.ID,
                    Parent = Players,
                    Name = SelectedPlayer.FullName,
                    Position = SelectedPosition,
                    Number = (int)SelectedNumber
                });

                MySqlConnection connection = new(SportsData.ConnectionStringSport);
                MySqlCommand cmd = new("INSERT INTO player_enlistment(player_id, team_id, season_id, number, position_code) " +
                                                    "VALUES (" + SelectedPlayer.ID + ", " + teamID + ", " + seasonID + ", " + SelectedNumber + ", '" + SelectedPosition.Code + "')", connection);

                try
                {
                    connection.Open();
                    _ = cmd.ExecuteNonQuery();

                    SelectedNumber = null;
                    SelectedPlayer = null;
                    SelectedPosition = null;
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

        private void EditPlayer(object param)
        {
            IList teamIDSeasonIDcomboBox = param as IList;
            int teamID = ((Team)teamIDSeasonIDcomboBox[0]).ID;
            int seasonID = (int)teamIDSeasonIDcomboBox[1];
            ComboBox comboBox = (ComboBox)teamIDSeasonIDcomboBox[2];

            if (EditedPlayer == null || EditedPosition == null || EditedNumber == null)
            {
                _ = MessageBox.Show("Please fill in all the fields", "Empty fields", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (EditedNumber != EditedPlayer.Number && Players.Any(x => x.Number == EditedNumber))
            {
                _ = MessageBox.Show("Number " + EditedNumber + " is already taken.", "Number is taken", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MySqlConnection connection = new(SportsData.ConnectionStringSport);
                MySqlCommand cmd = new("UPDATE player_enlistment SET number = " + EditedNumber + ", position_code = '" + EditedPosition.Code + "' " +
                                                    "WHERE player_id = " + EditedPlayer.ID + " AND team_id = " + teamID + " AND season_id = " + seasonID, connection);
                try
                {
                    connection.Open();
                    _ = cmd.ExecuteNonQuery();

                    EditedPlayer.Number = (int)EditedNumber;
                    EditedPlayer.Position = EditedPosition;

                    EditedPlayer = new PlayerEnlistment();
                    comboBox.SelectedIndex = -1;
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

        private void AddNewPlayer(object param)
        {
            IList teamSeasonIDViewModel = param as IList;
            int teamID = ((Team)teamSeasonIDViewModel[0]).ID;
            int seasonID = (int)teamSeasonIDViewModel[1];

            if (string.IsNullOrWhiteSpace(NewFirstName) || string.IsNullOrWhiteSpace(NewLastName) || NewBirthdate == null
                || string.IsNullOrWhiteSpace(NewGender) || NewHeight == null || NewWeight == null || string.IsNullOrWhiteSpace(NewPlaysWith)
                || NewCitizenship == null || string.IsNullOrWhiteSpace(NewBirthplaceCity) || NewBirthplaceCountry == null
                || string.IsNullOrWhiteSpace(NewStatus))
            {
                _ = MessageBox.Show("Please fill in all the fields", "Empty fields", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (Players.Any(x => x.Number == SelectedNumber))
            {
                _ = MessageBox.Show("Number " + SelectedNumber + " is already taken.", "Number is taken", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MySqlConnection connection = new(SportsData.ConnectionStringSport);
                //MySqlCommand cmd = new MySqlCommand(s, connection);
                MySqlCommand cmd = new("INSERT INTO player(first_name, last_name, birthdate, gender, height, weight, plays_with, citizenship, birthplace_city, birthplace_country, status, info) " +
                                                    "VALUES ('" + NewFirstName + "', '" + NewLastName + "', '" + ((DateTime)NewBirthdate).ToString("yyyy-MM-dd H:mm:ss") + "', '" + gender + "'," +
                                                    " " + NewHeight + ", " + NewWeight + ", '" + playsWith + "', '" + NewCitizenship.CodeTwo + "', '" + NewBirthplaceCity + "', " +
                                                    "'" + NewBirthplaceCountry.CodeTwo + "'" +
                                                    ", " + Convert.ToInt32(status) + ", '" + NewInfo + "')", connection);
                try
                {
                    connection.Open();
                    _ = cmd.ExecuteNonQuery();
                    newID = (int)cmd.LastInsertedId;

                    ((TeamViewModel)teamSeasonIDViewModel[2]).Players.Add(new Player { ID = newID, FirstName = NewFirstName, LastName = NewLastName });

                    Players.Add(new PlayerEnlistment
                    {
                        ID = newID,
                        Parent = Players,
                        Name = NewFirstName + " " + NewLastName,
                        Position = SelectedPosition,
                        Number = (int)SelectedNumber
                    });

                    //insert player_enlistment
                    cmd = new MySqlCommand("INSERT INTO player_enlistment(player_id, team_id, season_id, number, position_code) " +
                                                        "VALUES (" + newID + ", " + teamID + ", " + seasonID + ", " + SelectedNumber + ", '" + SelectedPosition.Code + "')", connection);

                    _ = cmd.ExecuteNonQuery();

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
                    _ = MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        #endregion

        #endregion
    }

    public class TeamViewModel : NotifyPropertyChanged
    {
        public ICommand NavigateEditTeamCommand { get; }

        #region Data
        public Team CurrentTeam { get; set; }

        private ObservableCollection<CompetitionRecord> competitionEnlistments;
        public ObservableCollection<CompetitionRecord> CompetitionEnlistments
        {
            get => competitionEnlistments;
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
        #endregion

        public TeamViewModel(NavigationStore navigationStore, Team t)
        {
            NavigateEditTeamCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new EditTeamViewModel(navigationStore, CurrentTeam)));
            CurrentTeam = t;
            CurrentTeam.Country = SportsData.Countries.First(x => x.CodeTwo == CurrentTeam.Country.CodeTwo);
            Countries = SportsData.Countries;
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
            Statuses = new ObservableCollection<string>
            {
                "Active",
                "Inactive"
            };
        }

        private void LoadGenders()
        {
            Genders = new ObservableCollection<string>
            {
                "Male",
                "Female"
            };
        }

        private void LoadPlaysWith()
        {
            PlaysWith = new ObservableCollection<string>
            {
                "Right",
                "Left"
            };
        }

        private void LoadTeamInfo()
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT info FROM team WHERE id = " + CurrentTeam.ID, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                CurrentTeam.Info = dataTable.Rows[0]["info"].ToString();
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

        private void LoadEnlistments()
        {
            CompetitionEnlistments = new ObservableCollection<CompetitionRecord>();

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT player_id, season_id, number, p.first_name AS player_first_name, p.last_name AS player_last_name, pos.name AS position, s.name AS season_name, c.name AS competition_name, c.id AS competition_id " +
                                                "FROM player_enlistment " +
                                                "INNER JOIN player AS p ON p.id = player_id " +
                                                "INNER JOIN position AS pos ON pos.code = position_code " +
                                                "INNER JOIN seasons AS s ON s.id = season_id " +
                                                "INNER JOIN competitions AS c ON c.id = s.competition_id " +
                                                "WHERE team_id = " + CurrentTeam.ID + " ORDER BY number", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    int competitionID = int.Parse(row["competition_id"].ToString());
                    string competitionName = row["competition_name"].ToString();
                    int seasonID = int.Parse(row["season_id"].ToString());
                    string seasonName = row["season_name"].ToString();

                    if (!CompetitionEnlistments.Any(x => x.CompetitionID == competitionID))
                    {
                        CompetitionEnlistments.Add(new CompetitionRecord { CompetitionID = competitionID, CompetitionName = competitionName, Seasons = new() });
                    }
                    CompetitionRecord competitionRecord = CompetitionEnlistments.First(x => x.CompetitionID == competitionID);

                    if (!competitionRecord.Seasons.Any(x => x.SeasonID == seasonID))
                    {
                        competitionRecord.Seasons.Add(new SeasonRecord { SeasonID = seasonID, SeasonName = seasonName, Players = new() });
                    }
                    SeasonRecord seasonRecord = competitionRecord.Seasons.First(x => x.SeasonID == seasonID);

                    seasonRecord.Players.Add(new PlayerEnlistment
                    {
                        ID = int.Parse(row["player_id"].ToString()),
                        Parent = seasonRecord.Players,
                        Name = row["player_first_name"].ToString() + " " + row["player_last_name"].ToString(),
                        Number = int.Parse(row["number"].ToString()),
                        Position = Positions.First(x => x.Name == row["position"].ToString())
                    }); ;
                }

                //load seasons where no players are enlisted
                cmd = new MySqlCommand("SELECT season_id, s.name AS season_name, c.name AS competition_name, c.id AS competition_id " +
                                                "FROM team_enlistment " +
                                                "INNER JOIN seasons AS s ON s.id = season_id " +
                                                "INNER JOIN competitions AS c ON c.id = s.competition_id " +
                                                "WHERE team_id = " + CurrentTeam.ID, connection);

                dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    int competitionID = int.Parse(row["competition_id"].ToString());
                    string competitionName = row["competition_name"].ToString();
                    int seasonID = int.Parse(row["season_id"].ToString());
                    string seasonName = row["season_name"].ToString();

                    if (!CompetitionEnlistments.Any(x => x.CompetitionID == competitionID))
                    {
                        CompetitionEnlistments.Add(new CompetitionRecord { CompetitionID = competitionID, CompetitionName = competitionName, Seasons = new() });
                    }
                    CompetitionRecord competitionRecord = CompetitionEnlistments.First(x => x.CompetitionID == competitionID);

                    if (!competitionRecord.Seasons.Any(x => x.SeasonID == seasonID))
                    {
                        competitionRecord.Seasons.Add(new SeasonRecord { SeasonID = seasonID, SeasonName = seasonName, Players = new() });
                    }
                }
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

        private void LoadPositions()
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT code, name FROM position", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    Positions.Add(new Position { Name = row["name"].ToString(), Code = row["code"].ToString() });
                }
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

        private void LoadPlayers()
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT id, first_name, last_name FROM player", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    if (int.Parse(row["id"].ToString()) != SportsData.NOID)
                    {
                        Players.Add(new Player { ID = int.Parse(row["id"].ToString()), FirstName = row["first_name"].ToString(), LastName = row["last_name"].ToString() });
                    }
                }
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
        #endregion
    }
}