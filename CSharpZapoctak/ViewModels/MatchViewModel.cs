using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class MatchEvent : ViewModelBase, IComparable
    {
        private int period;
        public int Period
        {
            get { return period; }
            set
            {
                period = value;
                OnPropertyChanged();
            }
        }

        private int timeInSeconds;
        public int TimeInSeconds
        {
            get { return timeInSeconds; }
            set
            {
                timeInSeconds = value;
                OnPropertyChanged();
            }
        }

        public int orderInMatch;

        public string Time
        {
            get { return (timeInSeconds / 60) + ":" + (timeInSeconds % 60).ToString("00"); }
        }

        private string homeEvent = "";
        public string HomeEvent
        {
            get { return homeEvent; }
            set
            {
                homeEvent = value;
                OnPropertyChanged();
            }
        }

        private string awayEvent = "";
        public string AwayEvent
        {
            get { return awayEvent; }
            set
            {
                awayEvent = value;
                OnPropertyChanged();
            }
        }

        public int CompareTo(object obj)
        {
            int otherOrder = ((MatchEvent)obj).orderInMatch;
            if (orderInMatch < otherOrder)
            {
                return -1;
            }
            else if (orderInMatch == otherOrder)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }

    class PeriodEvents : ViewModelBase
    {
        private int period;
        public int Period
        {
            get { return period; }
            set
            {
                period = value;
                OnPropertyChanged();
            }
        }

        private string periodName;
        public string PeriodName
        {
            get { return periodName; }
            set
            {
                periodName = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<MatchEvent> events = new ObservableCollection<MatchEvent>();
        public ObservableCollection<MatchEvent> Events
        {
            get { return events; }
            set
            {
                events = value;
                OnPropertyChanged();
            }
        }
    }

    class MatchViewModel : ViewModelBase
    {
        public Competition CurrentSeason { get; set; }

        public ICommand NavigateEditCompetitionCommand { get; }

        private Match match;
        public Match Match
        {
            get { return match; }
            set
            {
                match = value;
                OnPropertyChanged();
            }
        }

        private string periodScores;
        public string PeriodScores
        {
            get { return periodScores; }
            set
            {
                periodScores = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> homeRoster;
        public ObservableCollection<string> HomeRoster
        {
            get { return homeRoster; }
            set
            {
                homeRoster = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> awayRoster;
        public ObservableCollection<string> AwayRoster
        {
            get { return awayRoster; }
            set
            {
                awayRoster = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PenaltyReason> penaltyReasons;
        public ObservableCollection<PenaltyReason> PenaltyReasons
        {
            get { return penaltyReasons; }
            set
            {
                penaltyReasons = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PenaltyType> penaltyTypes;
        public ObservableCollection<PenaltyType> PenaltyTypes
        {
            get { return penaltyTypes; }
            set
            {
                penaltyTypes = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PeriodEvents> periodEvents;
        public ObservableCollection<PeriodEvents> PeriodEvents
        {
            get { return periodEvents; }
            set
            {
                periodEvents = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> shootoutHome;
        public ObservableCollection<string> ShootoutHome
        {
            get { return shootoutHome; }
            set
            {
                shootoutHome = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> shootoutAway;
        public ObservableCollection<string> ShootoutAway
        {
            get { return shootoutAway; }
            set
            {
                shootoutAway = value;
                OnPropertyChanged();
            }
        }

        private Visibility shootoutVisibility = Visibility.Collapsed;
        public Visibility ShootoutVisibility
        {
            get { return shootoutVisibility; }
            set
            {
                shootoutVisibility = value;
                OnPropertyChanged();
            }
        }

        public string GameType
        {
            get
            {
                if (Match.Forfeit)
                {
                    return "ff";
                }
                if (Match.Shootout)
                {
                    return "so";
                }
                if (Match.Overtime)
                {
                    return "ot";
                }
                return "";
            }
        }

        public string HomeScore
        {
            get
            {
                if (Match.Played)
                {
                    return Match.HomeScore.ToString();
                }
                return "-";
            }
        }

        public string AwayScore
        {
            get
            {
                if (Match.Played)
                {
                    return Match.AwayScore.ToString();
                }
                return "-";
            }
        }

        private ICommand editCommand;
        public ICommand EditCommand
        {
            get
            {
                if (editCommand == null)
                {
                    editCommand = new RelayCommand(param => Edit((Match)param));
                }
                return editCommand;
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
        public ViewModelBase scheduleToReturnVM;

        public MatchViewModel(NavigationStore navigationStore, Match m, ViewModelBase scheduleToReturnVM)
        {
            this.scheduleToReturnVM = scheduleToReturnVM;
            ns = navigationStore;
            PenaltyReasons = SportsData.LoadPenaltyReasons();
            PenaltyTypes = SportsData.LoadPenaltyTypes();
            LoadMatch(m);
            LoadPeriodScore();
            LoadRoster("Home");
            LoadRoster("Away");
            LoadEvents();
            LoadShootout();
        }

        private void LoadMatch(Match m)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT matches.id, season_id, datetime, played, periods, period_duration, home_competitor, away_competitor, home_score, away_score, overtime, shootout, forfeit, " +
                                                "ht.name AS home_name, at.name AS away_name " +
                                                "FROM matches " +
                                                "INNER JOIN team AS ht ON ht.id = home_competitor " +
                                                "INNER JOIN team AS at ON at.id = away_competitor " +
                                                "WHERE matches.id = " + m.id, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                Match = new Match
                {
                    id = int.Parse(dataTable.Rows[0]["id"].ToString()),
                    Season = new Season { id = int.Parse(dataTable.Rows[0]["season_id"].ToString()) },
                    Datetime = DateTime.Parse(dataTable.Rows[0]["datetime"].ToString()),
                    Played = Convert.ToBoolean(int.Parse(dataTable.Rows[0]["played"].ToString())),
                    Periods = int.Parse(dataTable.Rows[0]["periods"].ToString()),
                    PeriodDuration = int.Parse(dataTable.Rows[0]["period_duration"].ToString()),
                    HomeTeam = new Team { id = int.Parse(dataTable.Rows[0]["home_competitor"].ToString()), Name = dataTable.Rows[0]["home_name"].ToString() },
                    AwayTeam = new Team { id = int.Parse(dataTable.Rows[0]["away_competitor"].ToString()), Name = dataTable.Rows[0]["away_name"].ToString() },
                    HomeScore = int.Parse(dataTable.Rows[0]["home_score"].ToString()),
                    AwayScore = int.Parse(dataTable.Rows[0]["away_score"].ToString()),
                    Overtime = Convert.ToBoolean(int.Parse(dataTable.Rows[0]["overtime"].ToString())),
                    Shootout = Convert.ToBoolean(int.Parse(dataTable.Rows[0]["shootout"].ToString())),
                    Forfeit = Convert.ToBoolean(int.Parse(dataTable.Rows[0]["forfeit"].ToString()))
                };

                string[] imgPath = System.IO.Directory.GetFiles(SportsData.TeamLogosPath, SportsData.sport.name + Match.HomeTeam.id + ".*");
                if (imgPath.Length != 0)
                {
                    Match.HomeTeam.LogoPath = imgPath.First();
                }
                imgPath = System.IO.Directory.GetFiles(SportsData.TeamLogosPath, SportsData.sport.name + Match.AwayTeam.id + ".*");
                if (imgPath.Length != 0)
                {
                    Match.AwayTeam.LogoPath = imgPath.First();
                }

                connection.Close();
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

        private void LoadPeriodScore()
        {
            PeriodEvents = new ObservableCollection<PeriodEvents>();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM period_score WHERE match_id = " + Match.id, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                PeriodScores = "(";
                
                foreach (DataRow row in dataTable.Rows)
                {
                    PeriodEvents pe = new PeriodEvents();
                    pe.Period = int.Parse(row["period"].ToString());
                    if (pe.Period > 100)
                    {
                        pe.PeriodName = "Overtime";
                    }
                    else
                    {
                        pe.PeriodName = "Period " + row["period"].ToString();
                    }
                    PeriodEvents.Add(pe);

                    PeriodScores += row["home_score"].ToString() + ":" + row["away_score"].ToString() + ", ";
                }

                if (dataTable.Rows.Count > 0)
                {
                    PeriodScores = PeriodScores.Remove(PeriodScores.Length - 2);
                }
                PeriodScores += ")";

                if (dataTable.Rows.Count == 0)
                {
                    PeriodScores = "";
                }

                connection.Close();
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
            ObservableCollection<string> roster = new ObservableCollection<string>(); ;
            int teamID;
            if (side == "Home")
            {
                teamID = Match.HomeTeam.id;
            }
            else
            {
                teamID = Match.AwayTeam.id;
            }

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT e.number AS player_number, pos.code AS position_code, p.first_name AS player_first_name, p.last_name AS player_last_name " +
                                                "FROM player_matches " +
                                                "INNER JOIN player AS p ON p.id = player_matches.player_id " +
                                                "INNER JOIN player_enlistment AS e ON e.player_id = player_matches.player_id " +
                                                "INNER JOIN position AS pos ON pos.code = e.position_code " +
                                                "WHERE match_id = " + Match.id + " AND player_matches.team_id = " + teamID, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow row in dataTable.Rows)
                {
                    roster.Add(row["player_number"].ToString() + "# " + row["player_first_name"].ToString() + " " + row["player_last_name"].ToString() + " (" + row["position_code"].ToString() + ")");
                }

                if (side == "Home")
                {
                    HomeRoster = roster;
                }
                else
                {
                    AwayRoster = roster;
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

        private void LoadEvents()
        {
            //load penalties
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT p.first_name AS player_first_name, p.last_name AS player_last_name, period, period_seconds, order_in_match, team_id, penalty_type_id, penalty_reason_id " +
                                                "FROM penalties " +
                                                "INNER JOIN player AS p ON p.id = player_id " +
                                                "WHERE match_id = " + Match.id, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    MatchEvent e = new MatchEvent
                    {
                        TimeInSeconds = int.Parse(row["period_seconds"].ToString()),
                        orderInMatch = int.Parse(row["order_in_match"].ToString()),
                        Period = int.Parse(row["period"].ToString())
                    };

                    if (int.Parse(row["team_id"].ToString()) == Match.HomeTeam.id)
                    {
                        e.HomeEvent = PenaltyTypes.First(x => x.Code == row["penalty_type_id"].ToString()).Name + " - " + row["player_first_name"].ToString().Substring(0, 1) + ". " + row["player_last_name"].ToString() + " (" + PenaltyReasons.First(x => x.Code == row["penalty_reason_id"].ToString()).Name + ")";
                    }
                    else
                    {
                        e.AwayEvent = PenaltyTypes.First(x => x.Code == row["penalty_type_id"].ToString()).Name + " - " + row["player_first_name"].ToString().Substring(0, 1) + ". " + row["player_last_name"].ToString() + " (" + PenaltyReasons.First(x => x.Code == row["penalty_reason_id"].ToString()).Name + ")";
                    }

                    PeriodEvents.First(x => x.Period == int.Parse(row["period"].ToString())).Events.Add(e);
                }

                //load goals
                cmd = new MySqlCommand("SELECT p.first_name AS player_first_name, p.last_name AS player_last_name, " +
                                       "assist_player_id, a.first_name AS assist_first_name, a.last_name AS assist_last_name, " +
                                       "period, period_seconds, order_in_match, team_id, own_goal, empty_net, penalty_shot " +
                                       "FROM goals " +
                                       "INNER JOIN player AS p ON p.id = player_id " +
                                       "INNER JOIN player AS a ON a.id = assist_player_id " +
                                       "WHERE match_id = " + Match.id, connection);

                dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    MatchEvent e = new MatchEvent
                    {
                        TimeInSeconds = int.Parse(row["period_seconds"].ToString()),
                        orderInMatch = int.Parse(row["order_in_match"].ToString()),
                        Period = int.Parse(row["period"].ToString())
                    };

                    string s = "GOAL";
                    if (Convert.ToBoolean(int.Parse(row["own_goal"].ToString())))
                    {
                        s += " (OG)";
                    }
                    if (Convert.ToBoolean(int.Parse(row["empty_net"].ToString())))
                    {
                        s += " (EN)";
                    }
                    if (Convert.ToBoolean(int.Parse(row["penalty_shot"].ToString())))
                    {
                        s += " (PS)";
                    }

                    if (int.Parse(row["team_id"].ToString()) == Match.HomeTeam.id)
                    {
                        e.HomeEvent = s + " - " + row["player_first_name"].ToString().Substring(0, 1) + ". " + row["player_last_name"].ToString();
                        if (int.Parse(row["assist_player_id"].ToString()) != -1)
                        {
                            e.HomeEvent += " (" + row["assist_first_name"].ToString().Substring(0, 1) + ". " + row["assist_last_name"].ToString() + ")";
                        }
                    }
                    else
                    {
                        e.AwayEvent = s + " - " + row["player_first_name"].ToString().Substring(0, 1) + ". " + row["player_last_name"].ToString();
                        if (int.Parse(row["assist_player_id"].ToString()) != -1)
                        {
                            e.AwayEvent += " (" + row["assist_first_name"].ToString().Substring(0, 1) + ". " + row["assist_last_name"].ToString() + ")";
                        }
                    }
                    PeriodEvents.First(x => x.Period == int.Parse(row["period"].ToString())).Events.Add(e);
                }

                //load time-outs
                cmd = new MySqlCommand("SELECT * FROM time_outs WHERE match_id = " + Match.id, connection);

                dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    MatchEvent e = new MatchEvent
                    {
                        TimeInSeconds = int.Parse(row["period_seconds"].ToString()),
                        orderInMatch = int.Parse(row["order_in_match"].ToString()),
                        Period = int.Parse(row["period"].ToString())
                    };

                    if (int.Parse(row["team_id"].ToString()) == Match.HomeTeam.id)
                    {
                        e.HomeEvent = "Time-out";
                    }
                    else
                    {
                        e.AwayEvent = "Time-out";
                    }

                    PeriodEvents.First(x => x.Period == int.Parse(row["period"].ToString())).Events.Add(e);
                }

                //load penalty shots
                cmd = new MySqlCommand("SELECT p.first_name AS player_first_name, p.last_name AS player_last_name, " +
                                       "period, period_seconds, order_in_match, team_id, was_goal " +
                                       "FROM penalty_shots " +
                                       "INNER JOIN player AS p ON p.id = player_id " +
                                       "WHERE match_id = " + Match.id, connection);

                dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    if (int.Parse(row["was_goal"].ToString()) == 1)
                    {
                        MatchEvent e = new MatchEvent
                        {
                            TimeInSeconds = int.Parse(row["period_seconds"].ToString()),
                            orderInMatch = int.Parse(row["order_in_match"].ToString()),
                            Period = int.Parse(row["period"].ToString())
                        };

                        if (int.Parse(row["team_id"].ToString()) == Match.HomeTeam.id)
                        {
                            e.HomeEvent = "Not converted penalty shot - " + row["player_first_name"].ToString().Substring(0, 1) + ". " + row["player_last_name"].ToString();
                        }
                        else
                        {
                            e.AwayEvent = "Not converted penalty shot - " + row["player_first_name"].ToString().Substring(0, 1) + ". " + row["player_last_name"].ToString();
                        }
                        PeriodEvents.First(x => x.Period == int.Parse(row["period"].ToString())).Events.Add(e);
                    }
                }

                //load shifts
                cmd = new MySqlCommand("SELECT p.first_name AS player_first_name, p.last_name AS player_last_name, " +
                                       "period, period_seconds, order_in_match, team_id, end_period_seconds, end_order_in_match " +
                                       "FROM shifts " +
                                       "INNER JOIN player AS p ON p.id = player_id " +
                                       "WHERE match_id = " + Match.id, connection);

                dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    MatchEvent e = new MatchEvent
                    {
                        TimeInSeconds = int.Parse(row["period_seconds"].ToString()),
                        orderInMatch = int.Parse(row["order_in_match"].ToString()),
                        Period = int.Parse(row["period"].ToString())
                    };

                    if (int.Parse(row["team_id"].ToString()) == Match.HomeTeam.id)
                    {
                        e.HomeEvent = "Goaltender in - " + row["player_first_name"].ToString().Substring(0, 1) + ". " + row["player_last_name"].ToString();
                    }
                    else
                    {
                        e.AwayEvent = "Goaltender in - " + row["player_first_name"].ToString().Substring(0, 1) + ". " + row["player_last_name"].ToString();
                    }
                    PeriodEvents.First(x => x.Period == int.Parse(row["period"].ToString())).Events.Add(e);

                    e = new MatchEvent
                    {
                        TimeInSeconds = int.Parse(row["end_period_seconds"].ToString()),
                        orderInMatch = int.Parse(row["end_order_in_match"].ToString()),
                        Period = int.Parse(row["period"].ToString())
                    };

                    if (int.Parse(row["team_id"].ToString()) == Match.HomeTeam.id)
                    {
                        e.HomeEvent = "Goaltender out - " + row["player_first_name"].ToString().Substring(0, 1) + ". " + row["player_last_name"].ToString();
                    }
                    else
                    {
                        e.AwayEvent = "Goaltender out - " + row["player_first_name"].ToString().Substring(0, 1) + ". " + row["player_last_name"].ToString();
                    }
                    PeriodEvents.First(x => x.Period == int.Parse(row["period"].ToString())).Events.Add(e);
                }

                foreach (PeriodEvents pe in PeriodEvents)
                {
                    pe.Events.Sort();
                }

                connection.Close();
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

        private void LoadShootout()
        {
            ShootoutHome = new ObservableCollection<string>();
            ShootoutAway = new ObservableCollection<string>();

            if (Match.Shootout)
            {
                ShootoutVisibility = Visibility.Visible;

                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT p.first_name AS player_first_name, p.last_name AS player_last_name, " +
                                                    "team_id, was_goal " +
                                                    "FROM shootout_shots " +
                                                    "INNER JOIN player AS p ON p.id = player_id " +
                                                    "WHERE match_id = " + Match.id + " " +
                                                    "ORDER BY number", connection);

                try
                {
                    connection.Open();
                    DataTable dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());

                    foreach (DataRow row in dataTable.Rows)
                    {
                        string s = "GOAL";
                        if (int.Parse(row["was_goal"].ToString()) == 0) { s = "MISS"; }

                        if (int.Parse(row["team_id"].ToString()) == Match.HomeTeam.id)
                        {
                            ShootoutHome.Add(s + " - " + row["player_first_name"].ToString().Substring(0, 1) + ". " + row["player_last_name"].ToString());
                            ShootoutAway.Add("");
                        }
                        else
                        {
                            ShootoutHome.Add("");
                            ShootoutAway.Add(s + " - " + row["player_first_name"].ToString().Substring(0, 1) + ". " + row["player_last_name"].ToString());
                        }
                    }
                    connection.Close();
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
        }

        private void Edit(Match param)
        {
            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new AddMatchViewModel(ns, Match))).Execute(null);
        }

        private void Delete()
        {
            MessageBoxResult msgResult = MessageBox.Show("Do you really want to delete this match?.", "Delete match", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (msgResult == MessageBoxResult.Yes)
            {
                //delete match from DB
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlTransaction transaction = null;
                MySqlCommand cmd = null;
                string querry = "DELETE FROM matches WHERE id = " + Match.id;

                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    cmd = new MySqlCommand(querry, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();

                    //delete all player/goalie match enlistments and all stats
                    List<string> databases = new List<string> { "player_matches", "goalie_matches", "penalties", "goals", "penalty_shots", "shutouts", "shifts", "shootout_shots", "time_outs", "period_score", "game_state" };
                    foreach (string db in databases)
                    {
                        querry = "DELETE FROM " + db + " WHERE match_id = " + Match.id;
                        cmd = new MySqlCommand(querry, connection);
                        cmd.Transaction = transaction;
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    connection.Close();

                    ScheduleViewModel scheduleViewModel = new ScheduleViewModel(ns);
                    switch (scheduleToReturnVM)
                    {
                        //TODO: add qualification and play-off schedules
                        case MatchesSelectionViewModel:
                            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new MatchesSelectionViewModel(ns))).Execute(null);
                            break;
                        case GroupsScheduleViewModel:
                            scheduleViewModel.CurrentViewModel = new GroupsScheduleViewModel(ns);
                            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, scheduleViewModel)).Execute(null);
                            break;
                        default:
                            break;
                    }
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
    }
}