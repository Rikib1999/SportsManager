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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class QualificationScheduleViewModel : ViewModelBase
    {
        private readonly NavigationStore ns;
        public bool IsEnabled { get; private set; } = true;

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
                    addMatchCommand = new RelayCommand(param => AddMatch(param));
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

        private ICommand nextBracketCommand;
        public ICommand NextBracketCommand
        {
            get
            {
                if (nextBracketCommand == null)
                {
                    nextBracketCommand = new RelayCommand(param => NextBracket());
                }
                return nextBracketCommand;
            }
        }

        private ICommand previousBracketCommand;
        public ICommand PreviousBracketCommand
        {
            get
            {
                if (previousBracketCommand == null)
                {
                    previousBracketCommand = new RelayCommand(param => PreviousBracket());
                }
                return previousBracketCommand;
            }
        }
        #endregion

        #region Data
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

        private Bracket currentBracket;
        public Bracket CurrentBracket
        {
            get { return currentBracket; }
            set
            {
                currentBracket = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public QualificationScheduleViewModel(NavigationStore navigationStore)
        {
            ns = navigationStore;
            if (SportsData.season.PlayOffStarted || SportsData.season.WinnerID != -1) { IsEnabled = false; }
            LoadNotSelectedTeams();
            LoadBrackets();
        }

        private void LoadNotSelectedTeams()
        {
            NotSelectedTeams = new ObservableCollection<Team>();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT home_competitor, away_competitor " +
                                                "FROM matches " +
                                                "WHERE season_id = " + SportsData.season.id + " AND qualification_id <> -1", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                List<int> selectedTeams = new List<int>();
                foreach (DataRow tm in dataTable.Rows)
                {
                    int h = int.Parse(tm["home_competitor"].ToString());
                    int a = int.Parse(tm["away_competitor"].ToString());

                    if (h != -1 && !selectedTeams.Contains(h))
                    {
                        selectedTeams.Add(h);
                    }
                    if (a != -1 && !selectedTeams.Contains(a))
                    {
                        selectedTeams.Add(a);
                    }
                }


                cmd = new MySqlCommand("SELECT team_id, t.name AS team_name FROM team_enlistment " +
                                                "INNER JOIN team AS t ON t.id = team_id " +
                                                "WHERE season_id = " + SportsData.season.id, connection);
                dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow tm in dataTable.Rows)
                {
                    Team t = new Team
                    {
                        id = int.Parse(tm["team_id"].ToString()),
                        Name = tm["team_name"].ToString(),
                    };

                    if (t.id != -1 && !selectedTeams.Contains(t.id))
                    {
                        NotSelectedTeams.Add(t);
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

        private async Task<Bracket> LoadBracket(DataRow row)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + "; convert zero datetime=True";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT h.name AS home_name, a.name AS away_name, home_competitor, away_competitor, " +
                                    "matches.id, played, datetime, home_score, away_score, bracket_index, round, serie_match_number, bracket_first_team " +
                                    "FROM matches", connection);

            connection.Open();

            Bracket b = new Bracket(int.Parse(row["id"].ToString()), row["name"].ToString(), int.Parse(row["season_id"].ToString()), SportsData.season.QualificationRounds);

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
            cmd.CommandText += " WHERE qualification_id = " + b.id + " AND season_id = " + SportsData.season.id + " ORDER BY round, bracket_index";

            DataTable dt = new DataTable();
            dt.Load(await cmd.ExecuteReaderAsync());

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
            return b;
        }

        public void LoadBrackets()
        {
            Brackets = new ObservableCollection<Bracket>();
            for (int i = 0; i < SportsData.season.QualificationCount; i++)
            {
                Brackets.Add(null);
            }

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + "; convert zero datetime=True";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT id, season_id, name FROM brackets WHERE season_id = " + SportsData.season.id + " ORDER BY name", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    int index = int.Parse(row["name"].ToString().Remove(0, 8)) - 1;
                    Brackets[index] = Task.Run(() => LoadBracket(row)).Result;
                }

                CurrentBracket = Brackets.First();
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

        private void MatchDetail(Match m)
        {
            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new MatchViewModel(ns, m, new QualificationScheduleViewModel(ns)))).Execute(null);
        }

        private void AddMatch(object param)
        {
            IList serieAndBracket = (IList)param;
            Serie s = (Serie)serieAndBracket[0];
            //Bracket b = (Bracket)serieAndBracket[1];
            (int, int) roundIndex = CurrentBracket.GetSerieRoundIndex(s);

            int matchNumber = s.Matches.Count(x => x.Played) + 1;

            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new AddMatchViewModel(ns, new QualificationScheduleViewModel(ns), CurrentBracket.id, roundIndex.Item2, roundIndex.Item1, matchNumber, s.FirstTeam, s.SecondTeam))).Execute(null);
        }

        private void RemoveFirstTeamFromSerie(object param)
        {
            IList serieAndBracket = (IList)param;
            Serie s = (Serie)serieAndBracket[0];
            //Bracket b = (Bracket)serieAndBracket[1];

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd;

            if (s.Matches[0].HomeTeam.id == -1 || s.Matches[0].AwayTeam.id == -1)
            {
                //DELETE
                cmd = new MySqlCommand("DELETE FROM matches WHERE id = " + s.Matches[0].id, connection);
                s.Matches.RemoveAt(0);
            }
            else
            {
                //UPDATE
                if (s.Matches[0].HomeTeam.id == s.FirstTeam.id)
                {
                    //delete home
                    cmd = new MySqlCommand("UPDATE matches SET home_competitor = -1 WHERE id = " + s.Matches[0].id, connection);
                    s.Matches[0].HomeTeam = new Team { id = -1 };
                }
                else
                {
                    //delete away
                    cmd = new MySqlCommand("UPDATE matches SET away_competitor = -1 WHERE id = " + s.Matches[0].id, connection);
                    s.Matches[0].AwayTeam = new Team { id = -1 };
                }
            }

            try
            {
                connection.Open();
                cmd.ExecuteNonQuery();
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

            NotSelectedTeams.Add(s.FirstTeam);
            s.FirstTeam = new Team();

            (int, int) roundIndex = CurrentBracket.GetSerieRoundIndex(s);
            CurrentBracket.ResetSeriesAdvanced(roundIndex.Item1, roundIndex.Item2, 1);
            CurrentBracket.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 1, -1);
            CurrentBracket.PrepareSeries();
        }

        private void AddFirstTeamToSerie(object param)
        {
            IList serieAndBracket = (IList)param;
            Serie s = (Serie)serieAndBracket[0];
            //Bracket b = (Bracket)serieAndBracket[1];
            (int, int) roundIndex = CurrentBracket.GetSerieRoundIndex(s);

            if (s.FirstSelectedTeam == null || !NotSelectedTeams.Contains(s.FirstSelectedTeam))
            {
                return;
            }

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd;

            if (s.Matches.Count == 0)
            {
                //INSERT
                cmd = new MySqlCommand("INSERT INTO matches(season_id, played, qualification_id, bracket_index, round, serie_match_number, home_competitor, away_competitor, bracket_first_team) " +
                                       "VALUES (" + SportsData.season.id + ", 0, " + CurrentBracket.id + ", " + roundIndex.Item2 + "," +
                                       " " + roundIndex.Item1 + ", -1, " + s.FirstSelectedTeam.id + ", -1, " + s.FirstSelectedTeam.id + ")", connection);
            }
            else
            {
                //UPDATE
                if (s.Matches[0].HomeTeam.id == -1)
                {
                    //home
                    cmd = new MySqlCommand("UPDATE matches SET home_competitor = " + s.FirstSelectedTeam.id + ", bracket_first_team = " + s.FirstSelectedTeam.id + " WHERE id = " + s.Matches[0].id, connection);
                    s.Matches[0].HomeTeam = s.FirstSelectedTeam;
                }
                else
                {
                    //away
                    cmd = new MySqlCommand("UPDATE matches SET away_competitor = " + s.FirstSelectedTeam.id + ", bracket_first_team = " + s.FirstSelectedTeam.id + " WHERE id = " + s.Matches[0].id, connection);
                    s.Matches[0].AwayTeam = s.FirstSelectedTeam;
                }
            }

            int matchID = -1;
            try
            {
                connection.Open();
                cmd.ExecuteNonQuery();
                matchID = (int)cmd.LastInsertedId;
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

            s.FirstTeam = s.FirstSelectedTeam;
            s.FirstSelectedTeam = new Team();
            s.winner = new Team { id = -1 };
            s.RemoveFirstTeamVisibility = Visibility.Visible;
            s.RemoveSecondTeamVisibility = Visibility.Visible;
            NotSelectedTeams.Remove(s.FirstTeam);

            if (s.Matches.Count == 0)
            {
                Match m = new Match { id = matchID, Played = false, HomeTeam = s.FirstTeam, AwayTeam = new Team { id = -1 }, serieNumber = -1 };
                s.InsertMatch(m, s.FirstTeam.id, 1);
            }
            if (roundIndex.Item1 < CurrentBracket.Series.Count - 1)
            {
                int newPosition = 2;
                if (roundIndex.Item2 % 2 == 0) { newPosition = 1; }
                CurrentBracket.ResetSeriesAdvanced(roundIndex.Item1 + 1, roundIndex.Item2 / 2, newPosition);
            }

            CurrentBracket.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 1, 1);
            CurrentBracket.PrepareSeries();
        }

        private void RemoveSecondTeamFromSerie(object param)
        {
            IList serieAndBracket = (IList)param;
            Serie s = (Serie)serieAndBracket[0];
            //Bracket b = (Bracket)serieAndBracket[1];

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd;

            if (s.Matches[0].HomeTeam.id == -1 || s.Matches[0].AwayTeam.id == -1)
            {
                //DELETE
                cmd = new MySqlCommand("DELETE FROM matches WHERE id = " + s.Matches[0].id, connection);
                s.Matches.RemoveAt(0);
            }
            else
            {
                //UPDATE
                if (s.Matches[0].HomeTeam.id == s.SecondTeam.id)
                {
                    //delete home
                    cmd = new MySqlCommand("UPDATE matches SET home_competitor = -1 WHERE id = " + s.Matches[0].id, connection);
                    s.Matches[0].HomeTeam = new Team { id = -1 };
                }
                else
                {
                    //delete away
                    cmd = new MySqlCommand("UPDATE matches SET away_competitor = -1 WHERE id = " + s.Matches[0].id, connection);
                    s.Matches[0].AwayTeam = new Team { id = -1 };
                }
            }

            try
            {
                connection.Open();
                cmd.ExecuteNonQuery();
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

            NotSelectedTeams.Add(s.SecondTeam);
            s.SecondTeam = new Team();

            (int, int) roundIndex = CurrentBracket.GetSerieRoundIndex(s);
            CurrentBracket.ResetSeriesAdvanced(roundIndex.Item1, roundIndex.Item2, 2);
            CurrentBracket.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 2, -1);
            CurrentBracket.PrepareSeries();
        }

        private void AddSecondTeamToSerie(object param)
        {
            IList serieAndBracket = (IList)param;
            Serie s = (Serie)serieAndBracket[0];
            //Bracket b = (Bracket)serieAndBracket[1];

            (int, int) roundIndex = CurrentBracket.GetSerieRoundIndex(s);

            if (s.SecondSelectedTeam == null || !NotSelectedTeams.Contains(s.SecondSelectedTeam))
            {
                return;
            }

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd;

            if (s.Matches.Count == 0)
            {
                //INSERT
                cmd = new MySqlCommand("INSERT INTO matches(season_id, played, qualification_id, bracket_index, round, serie_match_number, home_competitor, away_competitor, bracket_first_team) " +
                                       "VALUES (" + SportsData.season.id + ", 0, " + CurrentBracket.id + ", " + roundIndex.Item2 + "," +
                                       " " + roundIndex.Item1 + ", -1, -1, " + s.SecondSelectedTeam.id + ", -1)", connection);
            }
            else
            {
                //UPDATE
                if (s.Matches[0].HomeTeam.id == -1)
                {
                    //home
                    cmd = new MySqlCommand("UPDATE matches SET home_competitor = " + s.SecondSelectedTeam.id + " WHERE id = " + s.Matches[0].id, connection);
                    s.Matches[0].HomeTeam = s.SecondSelectedTeam;
                }
                else
                {
                    //away
                    cmd = new MySqlCommand("UPDATE matches SET away_competitor = " + s.SecondSelectedTeam.id + " WHERE id = " + s.Matches[0].id, connection);
                    s.Matches[0].AwayTeam = s.SecondSelectedTeam;
                }
            }

            int matchID = -1;
            try
            {
                connection.Open();
                cmd.ExecuteNonQuery();
                matchID = (int)cmd.LastInsertedId;
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

            s.SecondTeam = s.SecondSelectedTeam;
            s.SecondSelectedTeam = new Team();
            s.winner = new Team { id = -1 };
            s.RemoveFirstTeamVisibility = Visibility.Visible;
            s.RemoveSecondTeamVisibility = Visibility.Visible;
            NotSelectedTeams.Remove(s.SecondTeam);

            if (s.Matches.Count == 0)
            {
                Match m = new Match { id = matchID, Played = false, AwayTeam = s.SecondTeam, HomeTeam = new Team { id = -1 }, serieNumber = -1 };
                s.InsertMatch(m, -1, 1);
            }
            if (roundIndex.Item1 < CurrentBracket.Series.Count - 1)
            {
                int newPosition = 2;
                if (roundIndex.Item2 % 2 == 0) { newPosition = 1; }
                CurrentBracket.ResetSeriesAdvanced(roundIndex.Item1 + 1, roundIndex.Item2 / 2, newPosition);
            }

            CurrentBracket.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 2, 1);
            CurrentBracket.PrepareSeries();
        }

        private void NextBracket()
        {
            if (Brackets.Count == 0) { return; }

            int index = Brackets.IndexOf(CurrentBracket);
            if (Brackets.Count - 1 == index)
            {
                index = 0;
            }
            else
            {
                index++;
            }

            CurrentBracket = Brackets[index];
        }

        private void PreviousBracket()
        {
            if (Brackets.Count == 0) { return; }

            int index = Brackets.IndexOf(CurrentBracket);
            if (0 == index)
            {
                index = Brackets.Count - 1;
            }
            else
            {
                index--;
            }

            CurrentBracket = Brackets[index];
        }
    }
}