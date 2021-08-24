using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class GroupsScheduleViewModel : ViewModelBase
    {
        public class MatchStats : IStats
        {
            public string Overview { get; set; }
            public string HomeScore { get; set; }
            public string AwayScore { get; set; }
            public string Datetime { get; set; }
        }

        private readonly NavigationStore ns;

        private readonly object roundsLock = new object();

        private ObservableCollection<Round> rounds;
        public ObservableCollection<Round> Rounds
        {
            get { return rounds; }
            set
            {
                rounds = value;
                OnPropertyChanged();
            }
        }

        private ICommand deleteRoundCommand;
        public ICommand DeleteRoundCommand
        {
            get
            {
                if (deleteRoundCommand == null)
                {
                    deleteRoundCommand = new RelayCommand(param => DeleteRound((Round)param));
                }
                return deleteRoundCommand;
            }
        }

        private ICommand matchDetailCommand;
        public ICommand MatchDetailCommand
        {
            get
            {
                if (matchDetailCommand == null)
                {
                    matchDetailCommand = new RelayCommand(param => MatchDetail((Match)param));
                }
                return matchDetailCommand;
            }
        }

        private ICommand addMatchCommand;
        public ICommand AddMatchCommand
        {
            get
            {
                if (addMatchCommand == null)
                {
                    addMatchCommand = new RelayCommand(param => AddMatch());
                }
                return addMatchCommand;
            }
        }

        private ICommand addRoundCommand;
        public ICommand AddRoundCommand
        {
            get
            {
                if (addRoundCommand == null)
                {
                    addRoundCommand = new RelayCommand(param => AddRound());
                }
                return addRoundCommand;
            }
        }

        public GroupsScheduleViewModel(NavigationStore navigationStore)
        {
            ns = navigationStore;
            LoadRounds();
        }

        private void LoadRounds()
        {
            Rounds = new ObservableCollection<Round>();

            lock (roundsLock)
            {
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT id, season_id, name FROM rounds", connection);

                try
                {
                    connection.Open();
                    DataTable dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());

                    foreach (DataRow row in dataTable.Rows)
                    {
                        Round r = new Round
                        {
                            id = int.Parse(row["id"].ToString()),
                            Name = row["name"].ToString(),
                            SeasonID = int.Parse(row["season_id"].ToString()),
                            Matches = new ObservableCollection<Match>()
                        };

                        cmd = new MySqlCommand("SELECT id, season_id, name FROM rounds", connection);
                        cmd = new MySqlCommand("SELECT h.name AS home_name, a.name AS away_name, s.competition_id AS competition_id, " +
                                                "matches.id, datetime, home_score, away_score, overtime, shootout, forfeit FROM matches " +
                                                "INNER JOIN seasons AS s ON s.id = matches.season_id", connection);
                        if (SportsData.sport.name == "tennis")
                        {
                            cmd.CommandText += " INNER JOIN player AS h ON h.id = matches.home_competitor";
                            cmd.CommandText += " INNER JOIN player AS a ON a.id = matches.away_competitor";
                        }
                        else
                        {
                            cmd.CommandText += " INNER JOIN team AS h ON h.id = matches.home_competitor";
                            cmd.CommandText += " INNER JOIN team AS a ON a.id = matches.away_competitor";
                        }
                        if (SportsData.competition.Name != "" && SportsData.competition.id != (int)EntityState.NotSelected && SportsData.competition.id != (int)EntityState.AddNew)
                        {
                            cmd.CommandText += " WHERE competition_id = " + SportsData.competition.id;
                            if (SportsData.season.Name != "" && SportsData.season.id != (int)EntityState.NotSelected && SportsData.season.id != (int)EntityState.AddNew)
                            {
                                cmd.CommandText += " AND season_id = " + SportsData.season.id;
                            }
                        }

                        DataTable dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());

                        //load matches
                        foreach (DataRow dtRow in dt.Rows)
                        {
                            Team home = new Team();
                            home.Name = dtRow["home_name"].ToString();
                            Team away = new Team();
                            away.Name = dtRow["away_name"].ToString();

                            Match m = new Match
                            {
                                id = int.Parse(dtRow["id"].ToString()),
                                Datetime = DateTime.Parse(dtRow["datetime"].ToString()),
                                HomeTeam = home,
                                AwayTeam = away,
                                HomeScore = int.Parse(dtRow["home_score"].ToString()),
                                AwayScore = int.Parse(dtRow["away_score"].ToString()),
                                Overtime = Convert.ToBoolean(int.Parse(dtRow["overtime"].ToString())),
                                Shootout = Convert.ToBoolean(int.Parse(dtRow["shootout"].ToString())),
                                Forfeit = Convert.ToBoolean(int.Parse(dtRow["forfeit"].ToString()))
                            };

                            MatchStats mStats = new MatchStats
                            {
                                Overview = m.Overview(),
                                HomeScore = m.HomeTeam.Name + "   " + m.HomeScore,
                                AwayScore = m.AwayScore + "   " + m.AwayTeam.Name
                            };
                            m.Stats = mStats;

                            r.Matches.Add(m);
                        }
                        Rounds.Add(r);
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
        }

        private void DeleteRound(Round r)
        {
            //rename all rounds after!!!
            //TODO: delete round from DB
            //TODO: delete matches from DB
            //MessageBox.Show("Really delete<<<<???.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            Rounds.Remove(r);

            //TODO: delete all player and goalie match enlistment from DB!!!
        }

        private void MatchDetail(Match m)
        {
            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new MatchViewModel(ns, m))).Execute(null);
        }

        private void AddMatch()
        {
            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new AddMatchViewModel(ns, this))).Execute(null);
        }

        private void AddRound()
        {
            Round r = new Round();
            r.SeasonID = SportsData.season.id;
            r.Name = "Round " + (Rounds.Count + 1);
            r.Matches = new ObservableCollection<Match>();

            /*
            for (int i = 0; i < 5; i++)
            {
                Match m = new Match();
                m.Datetime = DateTime.Now;
                m.HomeTeam = new Team { Name = "Sahy Flamingos" };
                m.AwayTeam = new Team { Name = "FBC TREX Batorove Kosihy" };
                m.HomeScore = 5;
                m.AwayScore = 4;
                m.Overtime = true;
                MatchStats ms = new MatchStats();
                ms.Overview = m.Overview();
                ms.HomeScore = m.HomeTeam.Name + "     " + m.HomeScore;
                ms.AwayScore = m.AwayScore + "     " + m.AwayTeam.Name;
                ms.Datetime = m.Datetime.ToString("g");
                m.Stats = ms;
                r.Matches.Add(m);
            }
            */

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("INSERT INTO rounds(season_id, name) VALUES ('" + r.SeasonID + "', '" + r.Name + "')", connection);

            try
            {
                connection.Open();
                cmd.ExecuteNonQuery();
                r.id = (int)cmd.LastInsertedId;

                Rounds.Add(r);
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