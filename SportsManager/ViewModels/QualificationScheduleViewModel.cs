using SportsManager.Commands;
using SportsManager.Models;
using SportsManager.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SportsManager.ViewModels
{
    /// <summary>
    /// Viewmodel for qualification schedule.
    /// </summary>
    public class QualificationScheduleViewModel : TemplateBracketScheduleViewModel
    {
        #region Commands
        private ICommand nextBracketCommand;
        /// <summary>
        /// When executed, switches to the next bracket.
        /// </summary>
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
        /// <summary>
        /// When executed, switches to the previous bracket.
        /// </summary>
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
        private ObservableCollection<Bracket> brackets;
        /// <summary>
        /// Collection of all qualification brackets of the current season.
        /// </summary>
        public ObservableCollection<Bracket> Brackets
        {
            get => brackets;
            set
            {
                brackets = value;
                OnPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// Instantiates new PlayOffScheduleViewModel.
        /// </summary>
        /// <param name="navigationStore">Current instance of the NavigationStore.</param>
        public QualificationScheduleViewModel(NavigationStore navigationStore)
        {
            Ns = navigationStore;
            Constructor = GetType().GetConstructor(new Type[] { typeof(NavigationStore) });

            if (SportsData.SEASON.PlayOffStarted || SportsData.SEASON.WinnerID != SportsData.NOID) { IsEnabled = false; }

            LoadNotSelectedTeams();
            LoadBrackets();
        }

        /// <summary>
        /// Loads all teams enlisted in the season that are not placed in any qualification.
        /// </summary>
        private void LoadNotSelectedTeams()
        {
            NotSelectedTeams = new ObservableCollection<Team>();

            //first, fetch all enlisted teams from the season
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT home_competitor, away_competitor " +
                                                "FROM matches " +
                                                "WHERE season_id = " + SportsData.SEASON.ID + " AND qualification_id <> -1", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                List<int> selectedTeams = new();
                foreach (DataRow tm in dataTable.Rows)
                {
                    int h = int.Parse(tm["home_competitor"].ToString());
                    int a = int.Parse(tm["away_competitor"].ToString());

                    if (h != SportsData.NOID && !selectedTeams.Contains(h))
                    {
                        selectedTeams.Add(h);
                    }
                    if (a != SportsData.NOID && !selectedTeams.Contains(a))
                    {
                        selectedTeams.Add(a);
                    }
                }

                //then fetch all teams that are placed in qualification
                cmd = new("SELECT team_id, t.name AS team_name FROM team_enlistment " + "INNER JOIN team AS t ON t.id = team_id " + "WHERE season_id = " + SportsData.SEASON.ID, connection);
                dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow tm in dataTable.Rows)
                {
                    Team t = new()
                    {
                        ID = int.Parse(tm["team_id"].ToString()),
                        Name = tm["team_name"].ToString(),
                    };

                    //populate NotSelectedTeams with teams that are not placed in qualification
                    if (t.ID != SportsData.NOID && !selectedTeams.Contains(t.ID))
                    {
                        NotSelectedTeams.Add(t);
                    }
                }
            }
            catch (Exception)
            {
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

        /// <summary>
        /// Fetches data from database and creates bracket instance with all matches.
        /// </summary>
        /// <param name="row">Bracket data from query.</param>
        /// <returns>Task that returns one bracket.</returns>
        private async Task<Bracket> LoadBracket(DataRow row)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT h.name AS home_name, a.name AS away_name, home_competitor, away_competitor, " +
                                    "matches.id, played, datetime, home_score, away_score, bracket_index, round, serie_match_number, bracket_first_team " +
                                    "FROM matches", connection);

            try
            {
                connection.Open();

                Bracket b = new(int.Parse(row["id"].ToString()), row["name"].ToString(), int.Parse(row["season_id"].ToString()), SportsData.SEASON.QualificationRounds);

                if (SportsData.SPORT.Name == "tennis")
                {
                    cmd.CommandText += " INNER JOIN player AS h ON h.id = matches.home_competitor";
                    cmd.CommandText += " INNER JOIN player AS a ON a.id = matches.away_competitor";
                }
                else
                {
                    cmd.CommandText += " INNER JOIN team AS h ON h.id = matches.home_competitor";
                    cmd.CommandText += " INNER JOIN team AS a ON a.id = matches.away_competitor";
                }
                cmd.CommandText += " WHERE qualification_id = " + b.ID + " AND season_id = " + SportsData.SEASON.ID + " ORDER BY round, bracket_index";

                DataTable dt = new();
                dt.Load(await cmd.ExecuteReaderAsync());

                //load matches
                foreach (DataRow dtRow in dt.Rows)
                {
                    Team home = new();
                    home.Name = dtRow["home_name"].ToString();
                    home.ID = int.Parse(dtRow["home_competitor"].ToString());
                    Team away = new();
                    away.Name = dtRow["away_name"].ToString();
                    away.ID = int.Parse(dtRow["away_competitor"].ToString());

                    Match m = new()
                    {
                        ID = int.Parse(dtRow["id"].ToString()),
                        Datetime = DateTime.Parse(dtRow["datetime"].ToString()),
                        Played = Convert.ToBoolean(int.Parse(dtRow["played"].ToString())),
                        HomeTeam = home,
                        AwayTeam = away,
                        HomeScore = int.Parse(dtRow["home_score"].ToString()),
                        AwayScore = int.Parse(dtRow["away_score"].ToString()),
                        SerieNumber = int.Parse(dtRow["serie_match_number"].ToString())
                    };

                    int round = int.Parse(dtRow["round"].ToString());
                    int index = int.Parse(dtRow["bracket_index"].ToString());
                    int firstTeamID = int.Parse(dtRow["bracket_first_team"].ToString());

                    b.Series[round][index].InsertMatch(m, firstTeamID, 1);

                    //blocks matches after and before current match
                    if (firstTeamID != SportsData.NOID)
                    {
                        b.IsEnabledTreeAfterInsertionAt(round, index, 1, 1);
                    }
                    if (home.ID != SportsData.NOID && away.ID != SportsData.NOID)
                    {
                        b.IsEnabledTreeAfterInsertionAt(round, index, 2, 1);
                    }
                    else if (firstTeamID == SportsData.NOID)
                    {
                        b.IsEnabledTreeAfterInsertionAt(round, index, 2, 1);
                    }
                }
                b.PrepareSeries();
                return b;
            }
            catch (Exception)
            {
                _ = MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return new Bracket(SportsData.NOID, "", SportsData.NOID, 1);
        }

        /// <summary>
        /// Loads all brackets asynchronously.
        /// </summary>
        public void LoadBrackets()
        {
            Brackets = new ObservableCollection<Bracket>();
            for (int i = 0; i < SportsData.SEASON.QualificationCount; i++)
            {
                Brackets.Add(null);
            }

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT id, season_id, name FROM brackets WHERE season_id = " + SportsData.SEASON.ID + " ORDER BY name", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
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

        /// <summary>
        /// Switch to next bracket.
        /// </summary>
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

        /// <summary>
        /// Switch to previous bracket.
        /// </summary>
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