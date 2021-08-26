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
    class PlayerInRoster : ViewModelBase
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

        private int? number = null;
        public int? Number
        {
            get { return number; }
            set
            {
                number = value;
                OnPropertyChanged();
            }
        }

        private bool present;
        public bool Present
        {
            get { return present; }
            set
            {
                present = value;

                ObservableCollection<PlayerInRoster> n = new ObservableCollection<PlayerInRoster>();
                foreach (PlayerInRoster p in vm.HomePlayers)
                {
                    if (p.Present)
                    {
                        n.Add(p);
                    }
                }
                foreach (Period p in vm.Periods)
                {
                    p.HomeRoster = n;
                }
                vm.HomeRoster = n;

                n = new ObservableCollection<PlayerInRoster>();
                foreach (PlayerInRoster p in vm.AwayPlayers)
                {
                    if (p.Present)
                    {
                        n.Add(p);
                    }
                }
                foreach (Period p in vm.Periods)
                {
                    p.AwayRoster = n;
                }
                vm.AwayRoster = n;

                foreach (Period p in vm.Periods)
                {
                    for (int i = p.Goals.Count - 1; i >= 0; i--)
                    {
                        if (!p.Goals[i].Scorer.Present || (p.Goals[i].Assist != null && !p.Goals[i].Assist.Present))
                        {
                            p.Goals.RemoveAt(i);
                        }
                    }
                    for (int i = p.Penalties.Count - 1; i >= 0; i--)
                    {
                        if (!p.Penalties[i].Player.Present)
                        {
                            p.Penalties.RemoveAt(i);
                        }
                    }
                    for (int i = p.PenaltyShots.Count - 1; i >= 0; i--)
                    {
                        if (!p.PenaltyShots[i].Player.Present)
                        {
                            p.PenaltyShots.RemoveAt(i);
                        }
                    }
                }

                OnPropertyChanged();
            }
        }

        public AddMatchViewModel vm;
    }

    class Period : ViewModelBase
    {
        public Period(int n, ObservableCollection<PlayerInRoster> h, ObservableCollection<PlayerInRoster> a)
        {
            Number = n;
            HomeRoster = h;
            AwayRoster = a;
            Goals = new ObservableCollection<Goal>();
            NewGoal = new Goal();
            GoalsRoster = new ObservableCollection<PlayerInRoster>();
            Penalties = new ObservableCollection<Penalty>();
            NewPenalty = new Penalty();
            PenaltyRoster = new ObservableCollection<PlayerInRoster>();
            PenaltyShots = new ObservableCollection<PenaltyShot>();
            NewPenaltyShot = new PenaltyShot();
            PenaltyShotRoster = new ObservableCollection<PlayerInRoster>();
        }

        public string Name
        {
            get { return "Period " + Number; }
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

        private ObservableCollection<PlayerInRoster> homeRoster;
        public ObservableCollection<PlayerInRoster> HomeRoster
        {
            get { return homeRoster; }
            set
            {
                homeRoster = value;
                if (GoalSide == "Home") { GoalsRoster = value; }
                if (PenaltySide == "Home") { PenaltyRoster = value; }
                if (PenaltyShotSide == "Home") { PenaltyShotRoster = value; }
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> awayRoster;
        public ObservableCollection<PlayerInRoster> AwayRoster
        {
            get { return awayRoster; }
            set
            {
                awayRoster = value;
                if (GoalSide == "Away") { GoalsRoster = value; }
                if (PenaltySide == "Away") { PenaltyRoster = value; }
                if (PenaltyShotSide == "Away") { PenaltyShotRoster = value; }
                OnPropertyChanged();
            }
        }

        #region Goals
        private ObservableCollection<Goal> goals;
        public ObservableCollection<Goal> Goals
        {
            get { return goals; }
            set
            {
                goals = value;
                OnPropertyChanged();
            }
        }

        private Goal newGoal;
        public Goal NewGoal
        {
            get { return newGoal; }
            set
            {
                newGoal = value;
                OnPropertyChanged();
            }
        }

        private string goalSide;
        public string GoalSide
        {
            get { return goalSide; }
            set
            {
                if (goalSide != value)
                {
                    goalSide = value;
                    NewGoal.Side = value;
                    NewGoal.Scorer = null;
                    NewGoal.Assist = null;
                    if (value == "Home")
                    {
                        GoalsRoster = HomeRoster;
                    }
                    else
                    {
                        GoalsRoster = AwayRoster;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<PlayerInRoster> goalsRoster;
        public ObservableCollection<PlayerInRoster> GoalsRoster
        {
            get { return goalsRoster; }
            set
            {
                goalsRoster = value;
                OnPropertyChanged();
            }
        }

        private ICommand addGoalCommand;
        public ICommand AddGoalCommand
        {
            get
            {
                if (addGoalCommand == null)
                {
                    addGoalCommand = new RelayCommand(param => AddGoal());
                }
                return addGoalCommand;
            }
        }

        private void AddGoal()
        {
            if (string.IsNullOrWhiteSpace(NewGoal.Side))
            {
                MessageBox.Show("Please select side.", "Side not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewGoal.Scorer == null)
            {
                MessageBox.Show("Please select scorer.", "Scorer not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewGoal.Scorer == NewGoal.Assist)
            {
                MessageBox.Show("Goal and assist can not be made by the same player.", "Goal and assist error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewGoal.PenaltyShot && NewGoal.OwnGoal)
            {
                MessageBox.Show("Own goal can not be scored on penalty shot.", "Own goal on penalty shot", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewGoal.PenaltyShot || NewGoal.OwnGoal)
            {
                NewGoal.Assist = new PlayerInRoster();
            }

            Goals.Add(NewGoal);
            NewGoal = new Goal();
            GoalSide = null;
            Goals.OrderBy(x => x.Minute).ThenBy(x => x.Second);
        }
        #endregion

        #region Penalties
        private ObservableCollection<Penalty> penalties;
        public ObservableCollection<Penalty> Penalties
        {
            get { return penalties; }
            set
            {
                penalties = value;
                OnPropertyChanged();
            }
        }

        private Penalty newPenalty;
        public Penalty NewPenalty
        {
            get { return newPenalty; }
            set
            {
                newPenalty = value;
                OnPropertyChanged();
            }
        }

        private string penaltySide;
        public string PenaltySide
        {
            get { return penaltySide; }
            set
            {
                if (penaltySide != value)
                {
                    penaltySide = value;
                    NewPenalty.Side = value;
                    NewPenalty.Player = null;
                    if (value == "Home")
                    {
                        PenaltyRoster = HomeRoster;
                    }
                    else
                    {
                        PenaltyRoster = AwayRoster;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<PlayerInRoster> penaltyRoster;
        public ObservableCollection<PlayerInRoster> PenaltyRoster
        {
            get { return penaltyRoster; }
            set
            {
                penaltyRoster = value;
                OnPropertyChanged();
            }
        }

        private ICommand addPenaltyCommand;
        public ICommand AddPenaltyCommand
        {
            get
            {
                if (addPenaltyCommand == null)
                {
                    addPenaltyCommand = new RelayCommand(param => AddPenalty());
                }
                return addPenaltyCommand;
            }
        }

        private void AddPenalty()
        {
            if (string.IsNullOrWhiteSpace(NewPenalty.Side))
            {
                MessageBox.Show("Please select side.", "Side not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewPenalty.Player == null)
            {
                MessageBox.Show("Please select player.", "Player not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewPenalty.PenaltyReason == null || NewPenalty.PenaltyType == null)
            {
                MessageBox.Show("Please select penalty reason and type.", "Penalty not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Penalties.Add(NewPenalty);
            NewPenalty = new Penalty();
            PenaltySide = null;
            Penalties.OrderBy(x => x.Minute).ThenBy(x => x.Second);
        }
        #endregion

        #region PenaltyShots
        private ObservableCollection<PenaltyShot> penaltyShots;
        public ObservableCollection<PenaltyShot> PenaltyShots
        {
            get { return penaltyShots; }
            set
            {
                penaltyShots = value;
                OnPropertyChanged();
            }
        }

        private PenaltyShot newPenaltyShot;
        public PenaltyShot NewPenaltyShot
        {
            get { return newPenaltyShot; }
            set
            {
                newPenaltyShot = value;
                OnPropertyChanged();
            }
        }

        private string penaltyShotSide;
        public string PenaltyShotSide
        {
            get { return penaltyShotSide; }
            set
            {
                if (penaltyShotSide != value)
                {
                    penaltyShotSide = value;
                    NewPenaltyShot.Side = value;
                    NewPenaltyShot.Player = null;
                    if (value == "Home")
                    {
                        PenaltyShotRoster = HomeRoster;
                    }
                    else
                    {
                        PenaltyShotRoster = AwayRoster;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<PlayerInRoster> penaltyShotRoster;
        public ObservableCollection<PlayerInRoster> PenaltyShotRoster
        {
            get { return penaltyShotRoster; }
            set
            {
                penaltyShotRoster = value;
                OnPropertyChanged();
            }
        }

        private ICommand addPenaltyShotCommand;
        public ICommand AddPenaltyShotCommand
        {
            get
            {
                if (addPenaltyShotCommand == null)
                {
                    addPenaltyShotCommand = new RelayCommand(param => AddPenaltyShot());
                }
                return addPenaltyShotCommand;
            }
        }

        private void AddPenaltyShot()
        {
            if (string.IsNullOrWhiteSpace(NewPenaltyShot.Side))
            {
                MessageBox.Show("Please select side.", "Side not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewPenaltyShot.Player == null)
            {
                MessageBox.Show("Please select player.", "Player not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PenaltyShots.Add(NewPenaltyShot);
            NewPenaltyShot = new PenaltyShot();
            PenaltyShotSide = null;
            PenaltyShots.OrderBy(x => x.Minute).ThenBy(x => x.Second);
        }
        #endregion
    }

    class Stat : ViewModelBase
    {
        private int minute;
        public int Minute
        {
            get { return minute; }
            set
            {
                minute = value;
                OnPropertyChanged();
            }
        }

        private int second;
        public int Second
        {
            get { return second; }
            set
            {
                second = value;
                OnPropertyChanged();
            }
        }

        public string Time
        {
            get { return Minute + ":" + Second; }
        }

        private string side;
        public string Side
        {
            get { return side; }
            set
            {
                side = value;
                OnPropertyChanged();
            }
        }
    }

    class Goal : Stat
    {
        private PlayerInRoster scorer = new PlayerInRoster();
        public PlayerInRoster Scorer
        {
            get { return scorer; }
            set
            {
                scorer = value;
                OnPropertyChanged();
            }
        }

        private PlayerInRoster assist = new PlayerInRoster();
        public PlayerInRoster Assist
        {
            get { return assist; }
            set
            {
                assist = value;
                OnPropertyChanged();
            }
        }

        private bool penaltyShot;
        public bool PenaltyShot
        {
            get { return penaltyShot; }
            set
            {
                penaltyShot = value;
                OnPropertyChanged();
            }
        }

        private bool ownGoal;
        public bool OwnGoal
        {
            get { return ownGoal; }
            set
            {
                ownGoal = value;
                OnPropertyChanged();
            }
        }

        public string Type
        {
            get
            {
                string s = "normal";
                if (PenaltyShot) { s = "penalty shot"; }
                if (OwnGoal) { s = "own goal"; }
                return s;
            }
        }
    }

    class Penalty : Stat
    {
        private PlayerInRoster player = new PlayerInRoster();
        public PlayerInRoster Player
        {
            get { return player; }
            set
            {
                player = value;
                OnPropertyChanged();
            }
        }

        private PenaltyReason penaltyReason;
        public PenaltyReason PenaltyReason
        {
            get { return penaltyReason; }
            set
            {
                penaltyReason = value;
                OnPropertyChanged();
            }
        }

        private PenaltyType penaltyType;
        public PenaltyType PenaltyType
        {
            get { return penaltyType; }
            set
            {
                penaltyType = value;
                OnPropertyChanged();
            }
        }
    }

    class PenaltyShot : Stat
    {
        private PlayerInRoster player = new PlayerInRoster();
        public PlayerInRoster Player
        {
            get { return player; }
            set
            {
                player = value;
                OnPropertyChanged();
            }
        }

        private bool wasGoal;
        public bool WasGoal
        {
            get { return wasGoal; }
            set
            {
                wasGoal = value;
                OnPropertyChanged();
            }
        }
    }

    class AddMatchViewModel : ViewModelBase
    {
        #region Properties
        private DateTime matchDate = DateTime.Now;
        public DateTime MatchDate
        {
            get { return matchDate; }
            set
            {
                matchDate = value;
                OnPropertyChanged();
            }
        }

        private int matchTimeHours;
        public int MatchTimeHours
        {
            get { return matchTimeHours; }
            set
            {
                matchTimeHours = value;
                OnPropertyChanged();
            }
        }

        private int matchTimeMinutes;
        public int MatchTimeMinutes
        {
            get { return matchTimeMinutes; }
            set
            {
                matchTimeMinutes = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Team> availableTeamsHome;
        public ObservableCollection<Team> AvailableTeamsHome
        {
            get { return availableTeamsHome; }
            set
            {
                availableTeamsHome = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Team> availableTeamsAway;
        public ObservableCollection<Team> AvailableTeamsAway
        {
            get { return availableTeamsAway; }
            set
            {
                availableTeamsAway = value;
                OnPropertyChanged();
            }
        }

        private Team homeTeam;
        public Team HomeTeam
        {
            get { return homeTeam; }
            set
            {
                if (homeTeam != null)
                {
                    AvailableTeamsAway.Add(homeTeam);
                }
                homeTeam = value;
                AvailableTeamsAway.Remove(homeTeam);
                LoadRoster("home");

                foreach (Period p in Periods)
                {
                    for (int i = p.Goals.Count - 1; i >= 0; i--)
                    {
                        if (p.Goals[i].Side == "Home")
                        {
                            p.Goals.RemoveAt(i);
                        }
                    }
                    for (int i = p.Penalties.Count - 1; i >= 0; i--)
                    {
                        if (p.Penalties[i].Side == "Home")
                        {
                            p.Penalties.RemoveAt(i);
                        }
                    }
                    for (int i = p.PenaltyShots.Count - 1; i >= 0; i--)
                    {
                        if (p.PenaltyShots[i].Side == "Home")
                        {
                            p.PenaltyShots.RemoveAt(i);
                        }
                    }
                }
                //TODO:
                OnPropertyChanged();
            }
        }

        private Team awayTeam;
        public Team AwayTeam
        {
            get { return awayTeam; }
            set
            {
                if (awayTeam != null)
                {
                    AvailableTeamsHome.Add(awayTeam);
                }
                awayTeam = value;
                AvailableTeamsHome.Remove(awayTeam);
                LoadRoster("away");

                foreach (Period p in Periods)
                {
                    for (int i = p.Goals.Count - 1; i >= 0; i--)
                    {
                        if (p.Goals[i].Side == "Away")
                        {
                            p.Goals.RemoveAt(i);
                        }
                    }
                    for (int i = p.Penalties.Count - 1; i >= 0; i--)
                    {
                        if (p.Penalties[i].Side == "Away")
                        {
                            p.Penalties.RemoveAt(i);
                        }
                    }
                    for (int i = p.PenaltyShots.Count - 1; i >= 0; i--)
                    {
                        if (p.PenaltyShots[i].Side == "Away")
                        {
                            p.PenaltyShots.RemoveAt(i);
                        }
                    }
                }
                //TODO:
                OnPropertyChanged();
            }
        }

        private bool played;
        public bool Played
        {
            get { return played; }
            set
            {
                played = value;
                OnPropertyChanged();
            }
        }

        private bool forfeit;
        public bool Forfeit
        {
            get { return forfeit; }
            set
            {
                forfeit = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> homePlayers = new ObservableCollection<PlayerInRoster>();
        public ObservableCollection<PlayerInRoster> HomePlayers
        {
            get { return homePlayers; }
            set
            {
                homePlayers = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> awayPlayers = new ObservableCollection<PlayerInRoster>();
        public ObservableCollection<PlayerInRoster> AwayPlayers
        {
            get { return awayPlayers; }
            set
            {
                awayPlayers = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> homeRoster;
        public ObservableCollection<PlayerInRoster> HomeRoster
        {
            get { return homeRoster; }
            set
            {
                homeRoster = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> awayRoster;
        public ObservableCollection<PlayerInRoster> AwayRoster
        {
            get { return awayRoster; }
            set
            {
                awayRoster = value;
                OnPropertyChanged();
            }
        }

        private int periodCount;
        public int PeriodCount
        {
            get { return periodCount; }
            set
            {
                if (value == Periods.Count)
                {
                    periodCount = value;
                }
                else
                {
                    int dif = value - periodCount;
                    if (dif < 0)
                    {
                        for (int i = 0; i > dif; i--)
                        {
                            Periods.RemoveAt(Periods.Count - 1);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < dif; i++)
                        {
                            Periods.Add(new Period(Periods.Count + 1, HomeRoster, AwayRoster));
                        }
                    }
                    periodCount = value;
                }
                OnPropertyChanged();
            }
        }

        private int periodDuration;
        public int PeriodDuration
        {
            get { return periodDuration; }
            set
            {
                periodDuration = value;
                OnPropertyChanged();
            }
        }

        private bool overtime;
        public bool Overtime
        {
            get { return overtime; }
            set
            {
                overtime = value;
                OnPropertyChanged();
            }
        }

        private bool shootout;
        public bool Shootout
        {
            get { return shootout; }
            set
            {
                shootout = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Period> periods;
        public ObservableCollection<Period> Periods
        {
            get { return periods; }
            set
            {
                periods = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> sides;
        public ObservableCollection<string> Sides
        {
            get { return sides; }
            set
            {
                sides = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PenaltyReason> PenaltyReasons { get; private set; }

        public ObservableCollection<PenaltyType> PenaltyTypes { get; private set; }
        #endregion

        public AddMatchViewModel(NavigationStore navigationStore, ViewModelBase scheduleToReturnVM)
        {
            Periods = new ObservableCollection<Period>();
            LoadTeams();
            LoadSides();
            LoadPenaltyReasons();
            LoadPenaltyTypes();
        }

        #region Loading
        private void LoadSides()
        {
            Sides = new ObservableCollection<string>();
            Sides.Add("Home");
            Sides.Add("Away");
        }

        private void LoadTeams()
        {
            AvailableTeamsHome = new ObservableCollection<Team>();
            AvailableTeamsAway = new ObservableCollection<Team>();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT team_id, t.name AS team_name, season_id " +
                                                "FROM team_enlistment " +
                                                "INNER JOIN team AS t ON t.id = team_id " +
                                                "WHERE season_id = " + SportsData.season.id, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow tm in dataTable.Rows)
                {
                    Team t = new Team
                    {
                        id = int.Parse(tm["team_id"].ToString()),
                        Name = tm["team_name"].ToString()
                    };

                    AvailableTeamsHome.Add(t);
                    AvailableTeamsAway.Add(t);
                }
            }
            catch (Exception)
            {
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

        private void LoadRoster(string side)
        {
            ObservableCollection<PlayerInRoster> roster = new ObservableCollection<PlayerInRoster>(); ;
            int teamID;
            if (side == "home")
            {
                teamID = HomeTeam.id;
            }
            else
            {
                teamID = AwayTeam.id;
            }

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT player_id, number, pos.name AS position_name, p.first_name AS player_first_name, p.last_name AS player_last_name " +
                                                "FROM player_enlistment " +
                                                "INNER JOIN player AS p ON p.id = player_id " +
                                                "INNER JOIN position AS pos ON pos.code = position_code " +
                                                "WHERE season_id = " + SportsData.season.id + " AND team_id = " + teamID, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow row in dataTable.Rows)
                {
                    PlayerInRoster p = new PlayerInRoster
                    {
                        id = int.Parse(row["player_id"].ToString()),
                        Name = row["player_first_name"].ToString() + " " + row["player_last_name"].ToString(),
                        Number = int.Parse(row["number"].ToString()),
                        Position = row["position_name"].ToString(),
                        vm = this
                    };

                    roster.Add(p);
                }

                if (side == "home")
                {
                    HomePlayers = roster;
                    foreach (Period p in Periods)
                    {
                        p.HomeRoster = new ObservableCollection<PlayerInRoster>();
                    }
                }
                else
                {
                    AwayPlayers = roster;
                    foreach (Period p in Periods)
                    {
                        p.AwayRoster = new ObservableCollection<PlayerInRoster>();
                    }
                }
            }
            catch (Exception)
            {
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

        private void LoadPenaltyReasons()
        {
            PenaltyReasons = new ObservableCollection<PenaltyReason>();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT code, name FROM penalty_reason", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow row in dataTable.Rows)
                {
                    PenaltyReason pr = new PenaltyReason
                    {
                        Code = row["code"].ToString(),
                        Name = row["name"].ToString()
                    };

                    PenaltyReasons.Add(pr);
                }
            }
            catch (Exception)
            {
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

        private void LoadPenaltyTypes()
        {
            PenaltyTypes = new ObservableCollection<PenaltyType>();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT code, name, minutes FROM penalty_type", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow row in dataTable.Rows)
                {
                    PenaltyType pt = new PenaltyType
                    {
                        Code = row["code"].ToString(),
                        Name = row["name"].ToString(),
                        Minutes = int.Parse(row["minutes"].ToString())
                    };

                    PenaltyTypes.Add(pt);
                }
            }
            catch (Exception)
            {
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
        #endregion
    }
}