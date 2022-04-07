using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Others;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    public class TemplateBracketScheduleViewModel : NotifyPropertyChanged
    {
        protected NavigationStore Ns { get; set; }
        protected ConstructorInfo Constructor { get; set; }
        public bool IsEnabled { get; protected set; } = true;

        #region Commands
        private ICommand exportBracketCommand;
        public ICommand ExportBracketCommand
        {
            get
            {
                if (exportBracketCommand == null)
                {
                    exportBracketCommand = new RelayCommand(param => Exports.ExportControlToImage((FrameworkElement)param));
                }
                return exportBracketCommand;
            }
        }

        private ICommand matchDetailCommand;
        public ICommand MatchDetailCommand
        {
            get
            {
                if (matchDetailCommand == null)
                {
                    matchDetailCommand = new RelayCommand(param => NavigateMatchDetail((Match)param));
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
                    addMatchCommand = new RelayCommand(param => AddMatch((Serie)param));
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
                    removeFirstTeamFromSerieCommand = new RelayCommand(param => RemoveTeamFromSerie((Serie)param, 1));
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
                    addFirstTeamToSerieCommand = new RelayCommand(param => AddFirstTeamToSerie((Serie)param));
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
                    removeSecondTeamFromSerieCommand = new RelayCommand(param => RemoveTeamFromSerie((Serie)param, 2));
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
                    addSecondTeamToSerieCommand = new RelayCommand(param => AddSecondTeamToSerie((Serie)param));
                }
                return addSecondTeamToSerieCommand;
            }
        }
        #endregion

        #region Data
        private ObservableCollection<Team> notSelectedTeams;
        public ObservableCollection<Team> NotSelectedTeams
        {
            get => notSelectedTeams;
            set
            {
                notSelectedTeams = value;
                OnPropertyChanged();
            }
        }

        private Bracket currentBracket;
        public Bracket CurrentBracket
        {
            get => currentBracket;
            set
            {
                currentBracket = value;
                OnPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// Navigates to the provided match detail.
        /// </summary>
        /// <param name="m"></param>
        protected void NavigateMatchDetail(Match m)
        {
            new NavigateCommand<SportViewModel>(Ns, () => new SportViewModel(Ns, new MatchViewModel(Ns, m, (NotifyPropertyChanged)Constructor.Invoke(new object[] { Ns })))).Execute(null);
        }

        /// <summary>
        /// Adds match to the serie.
        /// </summary>
        /// <param name="s">Serie instance.</param>
        protected void AddMatch(Serie s)
        {
            (int, int) roundIndex = CurrentBracket.GetSerieRoundIndex(s);

            int matchNumberInSerie = s.Matches.Count(x => x.Played) + 1;

            new NavigateCommand<SportViewModel>(Ns, () => new SportViewModel(Ns, new AddMatchViewModel(Ns, (NotifyPropertyChanged)Constructor.Invoke(new object[] { Ns }), CurrentBracket.ID, roundIndex.Item2, roundIndex.Item1, matchNumberInSerie, s.FirstTeam, s.SecondTeam))).Execute(null);
        }

        /// <summary>
        /// Adds first team to serie.
        /// </summary>
        /// <param name="param">Serie instance.</param>
        protected void AddFirstTeamToSerie(Serie s)
        {
            (int, int) roundIndex = CurrentBracket.GetSerieRoundIndex(s);

            if (s.FirstSelectedTeam == null || !NotSelectedTeams.Contains(s.FirstSelectedTeam)) { return; }

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd;

            if (s.Matches.Count == 0)
            {
                //INSERT
                cmd = new("INSERT INTO matches(season_id, played, qualification_id, bracket_index, round, serie_match_number, home_competitor, away_competitor, bracket_first_team) " +
                                       "VALUES (" + SportsData.SEASON.ID + ", 0, " + CurrentBracket.ID + ", " + roundIndex.Item2 + "," +
                                       " " + roundIndex.Item1 + ", -1, " + s.FirstSelectedTeam.ID + ", -1, " + s.FirstSelectedTeam.ID + ")", connection);
            }
            else
            {
                //UPDATE
                if (s.Matches[0].HomeTeam.ID == SportsData.NOID)
                {
                    //home
                    cmd = new("UPDATE matches SET home_competitor = " + s.FirstSelectedTeam.ID + ", bracket_first_team = " + s.FirstSelectedTeam.ID + " WHERE id = " + s.Matches[0].ID, connection);
                    s.Matches[0].HomeTeam = s.FirstSelectedTeam;
                }
                else
                {
                    //away
                    cmd = new("UPDATE matches SET away_competitor = " + s.FirstSelectedTeam.ID + ", bracket_first_team = " + s.FirstSelectedTeam.ID + " WHERE id = " + s.Matches[0].ID, connection);
                    s.Matches[0].AwayTeam = s.FirstSelectedTeam;
                }
            }

            int matchID = SportsData.NOID;
            try
            {
                connection.Open();
                _ = cmd.ExecuteNonQuery();
                matchID = (int)cmd.LastInsertedId;
                connection.Close();
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

            s.FirstTeam = s.FirstSelectedTeam;
            s.FirstSelectedTeam = new Team();
            s.Winner = new Team();
            s.RemoveFirstTeamVisibility = Visibility.Visible;
            s.RemoveSecondTeamVisibility = Visibility.Visible;
            _ = NotSelectedTeams.Remove(s.FirstTeam);

            if (s.Matches.Count == 0)
            {
                Match m = new() { ID = matchID, Played = false, HomeTeam = s.FirstTeam, AwayTeam = new Team(), SerieNumber = -1 };
                s.InsertMatch(m, s.FirstTeam.ID, 1);
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

        /// <summary>
        /// Adds second team to serie.
        /// </summary>
        /// <param name="param">Serie instance.</param>
        protected void AddSecondTeamToSerie(Serie s)
        {
            (int, int) roundIndex = CurrentBracket.GetSerieRoundIndex(s);

            if (s.SecondSelectedTeam == null || !NotSelectedTeams.Contains(s.SecondSelectedTeam)) { return; }

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd;

            if (s.Matches.Count == 0)
            {
                //INSERT
                cmd = new("INSERT INTO matches(season_id, played, qualification_id, bracket_index, round, serie_match_number, home_competitor, away_competitor, bracket_first_team) " + "VALUES (" + SportsData.SEASON.ID + ", 0, " + CurrentBracket.ID + ", " + roundIndex.Item2 + "," + " " + roundIndex.Item1 + ", -1, -1, " + s.SecondSelectedTeam.ID + ", -1)", connection);
            }
            else
            {
                //UPDATE
                if (s.Matches[0].HomeTeam.ID == SportsData.NOID)
                {
                    //home
                    cmd = new("UPDATE matches SET home_competitor = " + s.SecondSelectedTeam.ID + " WHERE id = " + s.Matches[0].ID, connection);
                    s.Matches[0].HomeTeam = s.SecondSelectedTeam;
                }
                else
                {
                    //away
                    cmd = new("UPDATE matches SET away_competitor = " + s.SecondSelectedTeam.ID + " WHERE id = " + s.Matches[0].ID, connection);
                    s.Matches[0].AwayTeam = s.SecondSelectedTeam;
                }
            }

            int matchID = SportsData.NOID;
            try
            {
                connection.Open();
                _ = cmd.ExecuteNonQuery();
                matchID = (int)cmd.LastInsertedId;
                connection.Close();
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

            s.SecondTeam = s.SecondSelectedTeam;
            s.SecondSelectedTeam = new Team();
            s.Winner = new Team();
            s.RemoveFirstTeamVisibility = Visibility.Visible;
            s.RemoveSecondTeamVisibility = Visibility.Visible;
            _ = NotSelectedTeams.Remove(s.SecondTeam);

            if (s.Matches.Count == 0)
            {
                Match m = new() { ID = matchID, Played = false, AwayTeam = s.SecondTeam, HomeTeam = new Team(), SerieNumber = -1 };
                s.InsertMatch(m, SportsData.NOID, 1);
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

        /// <summary>
        /// Removes first or second team from serie.
        /// </summary>
        /// <param name="param">Serie instance.</param>
        /// <param name="teamNumber">1 or 2 (first or second team)</param>
        protected void RemoveTeamFromSerie(Serie s, int teamNumber)
        {
            if (teamNumber is not (1 or 2)) { return; }

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd;

            if (s.Matches[0].HomeTeam.ID == SportsData.NOID || s.Matches[0].AwayTeam.ID == SportsData.NOID)
            {
                //DELETE
                cmd = new("DELETE FROM matches WHERE id = " + s.Matches[0].ID, connection);
                s.Matches.RemoveAt(0);
            }
            else
            {
                //UPDATE
                if ((teamNumber == 1 && s.Matches[0].HomeTeam.ID == s.FirstTeam.ID) || (teamNumber == 2 && s.Matches[0].HomeTeam.ID == s.SecondTeam.ID))
                {
                    //delete home
                    cmd = new("UPDATE matches SET home_competitor = -1 WHERE id = " + s.Matches[0].ID, connection);
                    s.Matches[0].HomeTeam = new Team();
                }
                else
                {
                    //delete away
                    cmd = new("UPDATE matches SET away_competitor = -1 WHERE id = " + s.Matches[0].ID, connection);
                    s.Matches[0].AwayTeam = new Team();
                }
            }

            try
            {
                connection.Open();
                _ = cmd.ExecuteNonQuery();
                connection.Close();
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

            if (teamNumber == 1)
            {
                NotSelectedTeams.Add(s.FirstTeam);
                s.FirstTeam = new Team();
            }
            else
            {
                NotSelectedTeams.Add(s.SecondTeam);
                s.SecondTeam = new Team();
            }

            (int, int) roundIndex = CurrentBracket.GetSerieRoundIndex(s);
            CurrentBracket.ResetSeriesAdvanced(roundIndex.Item1, roundIndex.Item2, teamNumber);
            CurrentBracket.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, teamNumber, -1);
            CurrentBracket.PrepareSeries();
        }
    }
}