using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Others;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    public class MatchesSelectionViewModel : NotifyPropertyChanged
    {
        #region Commands
        public ICommand NavigateMatchCommand { get; set; }

        private ICommand checkNavigateMatchCommand;
        public ICommand CheckNavigateMatchCommand
        {
            get
            {
                if (checkNavigateMatchCommand == null)
                {
                    checkNavigateMatchCommand = new RelayCommand(param => CheckNavigateMatch());
                }
                return checkNavigateMatchCommand;
            }
        }

        private ICommand exportPDFCommand;
        public ICommand ExportPDFCommand
        {
            get
            {
                if (exportPDFCommand == null)
                {
                    exportPDFCommand = new RelayCommand(param => Exports.ExportTable((System.Windows.Controls.DataGrid)param, "PDF", ExportTop));
                }
                return exportPDFCommand;
            }
        }

        private ICommand exportXLSXCommand;
        public ICommand ExportXLSXCommand
        {
            get
            {
                if (exportXLSXCommand == null)
                {
                    exportXLSXCommand = new RelayCommand(param => Exports.ExportTable((System.Windows.Controls.DataGrid)param, "XLSX", ExportTop));
                }
                return exportXLSXCommand;
            }
        }
        #endregion

        private int? exportTop;
        public int? ExportTop
        {
            get => exportTop;
            set
            {
                exportTop = value;
                OnPropertyChanged();
            }
        }

        #region Visibilities
        private bool showInfo = true;
        public bool ShowInfo
        {
            get => showInfo;
            set
            {
                showInfo = value;
                InfoVisibility = showInfo ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

        private bool showStats = true;
        public bool ShowStats
        {
            get => showStats;
            set
            {
                showStats = value;
                StatsVisibility = showStats ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

        private Visibility infoVisibility = Visibility.Visible;
        public Visibility InfoVisibility
        {
            get => infoVisibility;
            set
            {
                infoVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility statsVisibility = Visibility.Visible;
        public Visibility StatsVisibility
        {
            get => statsVisibility;
            set
            {
                statsVisibility = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public Match SelectedMatch { get; set; }

        public ObservableCollection<Match> Matches { get; set; }

        public MatchesSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateMatchCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new MatchViewModel(navigationStore, SelectedMatch, new MatchesSelectionViewModel(navigationStore))));
            SelectedMatch = null;

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT h.name AS home_name, a.name AS away_name, s.competition_id AS competition_id, s.name AS season_name, c.name AS competition_name, " +
                                                "matches.id, datetime, home_score, away_score, overtime, shootout, forfeit, qualification_id, serie_match_number FROM matches " +
                                                "INNER JOIN seasons AS s ON s.id = matches.season_id " +
                                                "INNER JOIN competitions AS c ON c.id = competition_id " +
                                                "INNER JOIN team AS h ON h.id = matches.home_competitor " +
                                                "INNER JOIN team AS a ON a.id = matches.away_competitor " +
                                                "WHERE matches.played = 1", connection);

            if (SportsData.IsCompetitionSet())
            {
                cmd.CommandText += " AND competition_id = " + SportsData.COMPETITION.ID;
                if (SportsData.IsSeasonSet())
                {
                    cmd.CommandText += " AND season_id = " + SportsData.SEASON.ID;
                }
            }
            cmd.CommandText += " ORDER BY matches.datetime DESC";

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                Matches = new ObservableCollection<Match>();

                foreach (DataRow row in dataTable.Rows)
                {
                    Competition c = new();
                    c.Name = row["competition_name"].ToString();
                    Season s = new();
                    s.Name = row["season_name"].ToString();
                    Team home = new();
                    home.Name = row["home_name"].ToString();
                    Team away = new();
                    away.Name = row["away_name"].ToString();

                    Match m = new()
                    {
                        ID = int.Parse(row["id"].ToString()),
                        Competition = c,
                        Season = s,
                        Datetime = DateTime.Parse(row["datetime"].ToString()),
                        HomeTeam = home,
                        AwayTeam = away,
                        HomeScore = int.Parse(row["home_score"].ToString()),
                        AwayScore = int.Parse(row["away_score"].ToString()),
                        Overtime = Convert.ToBoolean(int.Parse(row["overtime"].ToString())),
                        Shootout = Convert.ToBoolean(int.Parse(row["shootout"].ToString())),
                        Forfeit = Convert.ToBoolean(int.Parse(row["forfeit"].ToString()))
                    };

                    string partOfSeason = "Play-off";
                    if (int.Parse(row["serie_match_number"].ToString()) < 1)
                    {
                        partOfSeason = "Group";
                    }
                    else if (int.Parse(row["qualification_id"].ToString()) > 0)
                    {
                        partOfSeason = "Qualification";
                    }

                    m.Stats = new MatchStats(m);
                    ((MatchStats)m.Stats).PartOfSeason = partOfSeason;
                    ((MatchStats)m.Stats).Score = m.Score();

                    Matches.Add(m);
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

        private void CheckNavigateMatch()
        {
            if (SelectedMatch != null)
            {
                NavigateMatchCommand.Execute(SelectedMatch);
            }
        }
    }
}