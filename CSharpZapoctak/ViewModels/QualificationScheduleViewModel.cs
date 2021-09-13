using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class QualificationScheduleViewModel : ViewModelBase
    {
        private readonly NavigationStore ns;

        #region Commands
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
                    addMatchCommand = new RelayCommand(param => AddMatch(((Round)param).id));
                }
                return addMatchCommand;
            }
        }

        private ICommand removeFirstTeamFromSerieCommand;
        public ICommand RemoveFirstTeamFromSerieCommand
        {
            get
            {
                if (removeFirstTeamFromSerieCommand == null)
                {
                    removeFirstTeamFromSerieCommand = new RelayCommand(param => RemoveFirstTeamFromSerie(param));
                }
                return removeFirstTeamFromSerieCommand;
            }
        }

        private ICommand addFirstTeamToSerieCommand;
        public ICommand AddFirstTeamToSerieCommand
        {
            get
            {
                if (addFirstTeamToSerieCommand == null)
                {
                    addFirstTeamToSerieCommand = new RelayCommand(param => AddFirstTeamToSerie(param));
                }
                return addFirstTeamToSerieCommand;
            }
        }

        private ICommand removeSecondTeamFromSerieCommand;
        public ICommand RemoveSecondTeamFromSerieCommand
        {
            get
            {
                if (removeSecondTeamFromSerieCommand == null)
                {
                    removeSecondTeamFromSerieCommand = new RelayCommand(param => RemoveSecondTeamFromSerie(param));
                }
                return removeSecondTeamFromSerieCommand;
            }
        }

        private ICommand addSecondTeamToSerieCommand;
        public ICommand AddSecondTeamToSerieCommand
        {
            get
            {
                if (addSecondTeamToSerieCommand == null)
                {
                    addSecondTeamToSerieCommand = new RelayCommand(param => AddSecondTeamToSerie(param));
                }
                return addSecondTeamToSerieCommand;
            }
        }
        #endregion

        private ObservableCollection<Team> notSelectedTeams;
        public ObservableCollection<Team> NotSelectedTeams
        {
            get { return notSelectedTeams; }
            set
            {
                notSelectedTeams = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Bracket> brackets;
        public ObservableCollection<Bracket> Brackets
        {
            get { return brackets; }
            set
            {
                brackets = value;
                OnPropertyChanged();
            }
        }

        public QualificationScheduleViewModel(NavigationStore navigationStore)
        {
            ns = navigationStore;
            LoadBrackets();
            LoadNotSelectedTeams();
        }

        private void LoadNotSelectedTeams()
        {
            NotSelectedTeams = new ObservableCollection<Team>();
            //only name and id of team
        }

        private void LoadBrackets()
        {
            Brackets = new ObservableCollection<Bracket>();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + "; convert zero datetime=True";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT id, season_id, name FROM brackets WHERE season_id = " + SportsData.season.id + " AND type = 'QF' ORDER BY name", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    Bracket b = new Bracket(int.Parse(row["id"].ToString()), row["name"].ToString(), int.Parse(row["season_id"].ToString()), SportsData.season.QualificationRounds);

                    cmd = new MySqlCommand("SELECT h.name AS home_name, a.name AS away_name, home_competitor, away_competitor, " +
                                            "matches.id, played, datetime, home_score, away_score, bracket_index, round, serie_match_number, bracket_first_team " +
                                            "FROM matches", connection);
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
                    cmd.CommandText += " WHERE qualification_id = " + b.id + " ORDER BY round, bracket_index";

                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    //load matches
                    foreach (DataRow dtRow in dt.Rows)
                    {
                        Team home = new Team();
                        home.Name = dtRow["home_name"].ToString();
                        home.id = int.Parse(dtRow["home_competitor"].ToString());
                        Team away = new Team();
                        away.Name = dtRow["away_name"].ToString();
                        away.id = int.Parse(dtRow["away_competitor"].ToString());

                        Match m = new Match
                        {
                            id = int.Parse(dtRow["id"].ToString()),
                            Datetime = DateTime.Parse(dtRow["datetime"].ToString()),
                            Played = Convert.ToBoolean(int.Parse(dtRow["played"].ToString())),
                            HomeTeam = home,
                            AwayTeam = away,
                            HomeScore = int.Parse(dtRow["home_score"].ToString()),
                            AwayScore = int.Parse(dtRow["away_score"].ToString()),
                            serieNumber = int.Parse(dtRow["serie_match_number"].ToString())
                        };

                        int round = int.Parse(dtRow["round"].ToString());
                        int index = int.Parse(dtRow["bracket_index"].ToString());
                        int firstTeamID = int.Parse(dtRow["bracket_first_team"].ToString());

                        b.Series[round][index].InsertMatch(m, firstTeamID, 1);

                        if (firstTeamID != -1)
                        {
                            b.IsEnabledTreeAfterInsertionAt(round, index, 1, 1);
                        }
                        if (home.id != -1 && away.id != -1)
                        {
                            b.IsEnabledTreeAfterInsertionAt(round, index, 2, 1);
                        }
                        else if (firstTeamID == -1)
                        {
                            b.IsEnabledTreeAfterInsertionAt(round, index, 2, 1);
                        }
                    }
                    b.PrepareSeries();
                    Brackets.Add(b);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to connect to databse."+e.Message + e.StackTrace, "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void MatchDetail(Match m)
        {
            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new MatchViewModel(ns, m, new QualificationScheduleViewModel(ns)))).Execute(null);
        }
        //delete match in serie, shift serie numbers
        private void AddMatch(int id)
        {
            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new AddMatchViewModel(ns, new QualificationScheduleViewModel(ns), -1, -1, id, -1))).Execute(null);
            //TODO: indexes rounds etc, allow only set teams
        }

        private void RemoveFirstTeamFromSerie(object param)
        {
            IList serieAndBracket = (IList)param;
            Serie s = (Serie)serieAndBracket[0];
            Bracket b = (Bracket)serieAndBracket[1];

            NotSelectedTeams.Add(s.FirstTeam);
            s.FirstTeam = new Team();

            (int, int) roundIndex = b.GetSerieRoundIndex(s);
            b.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 1, -1);
        }

        private void AddFirstTeamToSerie(object param)
        {
            IList serieAndBracket = (IList)param;
            Serie s = (Serie)serieAndBracket[0];
            Bracket b = (Bracket)serieAndBracket[1];

            if (s.FirstSelectedTeam == null || !NotSelectedTeams.Contains(s.FirstSelectedTeam))
            {
                return;
            }
            s.FirstTeam = s.FirstSelectedTeam;
            s.FirstSelectedTeam = new Team();
            NotSelectedTeams.Remove(s.FirstTeam);

            (int, int) roundIndex = b.GetSerieRoundIndex(s);
            b.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 1, 1);
        }

        private void RemoveSecondTeamFromSerie(object param)
        {
            IList serieAndBracket = (IList)param;
            Serie s = (Serie)serieAndBracket[0];
            Bracket b = (Bracket)serieAndBracket[1];

            NotSelectedTeams.Add(s.SecondTeam);
            s.SecondTeam = new Team();

            (int, int) roundIndex = b.GetSerieRoundIndex(s);
            b.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 2, -1);
        }

        private void AddSecondTeamToSerie(object param)
        {
            IList serieAndBracket = (IList)param;
            Serie s = (Serie)serieAndBracket[0];
            Bracket b = (Bracket)serieAndBracket[1];

            if (s.SecondSelectedTeam == null || !NotSelectedTeams.Contains(s.SecondSelectedTeam))
            {
                return;
            }
            s.SecondTeam = s.SecondSelectedTeam;
            s.SecondSelectedTeam = new Team();
            NotSelectedTeams.Remove(s.SecondTeam);

            (int, int) roundIndex = b.GetSerieRoundIndex(s);
            b.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 2, 1);
        }
    }
}