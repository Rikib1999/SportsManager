using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Others;
using CSharpZapoctak.Stores;
using LiveCharts;
using LiveCharts.Wpf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CSharpZapoctak.ViewModels
{
    public class CompetitionRecord<T> : NotifyPropertyChanged where T : IStats
    {
        private Competition competition;
        public Competition Competition
        {
            get => competition;
            set
            {
                competition = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<T> stats;
        public ObservableCollection<T> Stats
        {
            get => stats;
            set
            {
                stats = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<SeasonRecord<T>> seasons = new();
        public ObservableCollection<SeasonRecord<T>> Seasons
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

    public class SeasonRecord<T> : NotifyPropertyChanged where T : IStats
    {
        private Season season;
        public Season Season
        {
            get => season;
            set
            {
                season = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<T> stats;
        public ObservableCollection<T> Stats
        {
            get => stats;
            set
            {
                stats = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Match> matches = new();
        public ObservableCollection<Match> Matches
        {
            get => matches;
            set
            {
                matches = value;
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
    }

    public class PlayerViewModel : NotifyPropertyChanged
    {
        private Player player;
        public Player Player
        {
            get => player;
            set
            {
                player = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<CompetitionRecord<PlayerStats>> competitions = new();
        public ObservableCollection<CompetitionRecord<PlayerStats>> Competitions
        {
            get => competitions;
            set
            {
                competitions = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<CompetitionRecord<GoalieStats>> competitionsAsGoalie = new();
        public ObservableCollection<CompetitionRecord<GoalieStats>> CompetitionsAsGoalie
        {
            get => competitionsAsGoalie;
            set
            {
                competitionsAsGoalie = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInMatchStats> playerInMatchStats = new();
        public ObservableCollection<PlayerInMatchStats> PlayerInMatchStats
        {
            get => playerInMatchStats;
            set
            {
                playerInMatchStats = value;
                OnPropertyChanged();
            }
        }

        public Func<double, string> AxisFormatterScatter { get; set; } = value => value.ToString("N2");

        public string[] DatetimeXLabels { get; set; }

        private SeriesCollection playerInMatchStatsSeries = new();
        public SeriesCollection PlayerInMatchStatsSeries
        {
            get => playerInMatchStatsSeries;
            set
            {
                playerInMatchStatsSeries = value;
                OnPropertyChanged();
            }
        }

        private SeriesCollection playerInMatchStatsSumSeries = new();
        public SeriesCollection PlayerInMatchStatsSumSeries
        {
            get => playerInMatchStatsSumSeries;
            set
            {
                playerInMatchStatsSumSeries = value;
                OnPropertyChanged();
            }
        }

        private SeriesCollection playerInMatchStatsAverageSeries = new();
        public SeriesCollection PlayerInMatchStatsAverageSeries
        {
            get => playerInMatchStatsAverageSeries;
            set
            {
                playerInMatchStatsAverageSeries = value;
                OnPropertyChanged();
            }
        }

        private ICommand exportChartCommand;
        public ICommand ExportChartCommand
        {
            get
            {
                if (exportChartCommand == null)
                {
                    exportChartCommand = new RelayCommand(param => Exports.ExportControlToImage((FrameworkElement)param));
                }
                return exportChartCommand;
            }
        }

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

        public Match SelectedMatch { get; set; }

        public ICommand NavigateEditCommand { get; }

        public PlayerViewModel(NavigationStore ns, Player p)
        {
            NavigateMatchCommand = new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new MatchViewModel(ns, SelectedMatch, new PlayerViewModel(ns, p))));
            NavigateEditCommand = new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new EditPlayerViewModel(ns, p)));
            Player = p;
            if (string.IsNullOrEmpty(p.ImagePath)) { p.ImagePath = p.Gender == "M" ? SportsData.ResourcesPath + "\\male.png" : SportsData.ResourcesPath + "\\female.png"; }
            LoadCompetitions();
            LoadCompetitionsAsGoalie();
            PlayerInMatchStatsLoader.LoadPlayerInMatchStats(PlayerInMatchStats, DatetimeXLabels, player);
            LoadPlayerInMatchStatsSeries();
            LoadPlayerInMatchStatsSumSeries();
            LoadPlayerInMatchStatsAverageSeries();
        }

        private void LoadCompetitions()
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT s.competition_id AS competition_id, c.name AS competition_name " +
                                                "FROM player_enlistment " +
                                                "INNER JOIN seasons AS s ON s.id = season_id " +
                                                "INNER JOIN competitions AS c ON c.id = s.competition_id " +
                                                "WHERE player_id = " + Player.ID + " GROUP BY s.competition_id", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    Competition c = new() { ID = int.Parse(row["competition_id"].ToString()), Name = row["competition_name"].ToString() };

                    CompetitionRecord<PlayerStats> cr = new()
                    {
                        Competition = c,
                        Stats = new ObservableCollection<PlayerStats> { new PlayerStats(Player, SportsData.NOID, c.ID) }
                    };

                    //load seasons
                    cmd = new MySqlCommand("SELECT s.id AS season_id, s.name AS season_name " +
                                                "FROM player_enlistment " +
                                                "INNER JOIN seasons AS s ON s.id = season_id " +
                                                "INNER JOIN competitions AS c ON c.id = s.competition_id " +
                                                "WHERE player_id = " + Player.ID + " AND s.competition_id = " + c.ID, connection);

                    DataTable seasonTable = new();
                    seasonTable.Load(cmd.ExecuteReader());

                    foreach (DataRow ssn in seasonTable.Rows)
                    {
                        Season s = new() { ID = int.Parse(ssn["season_id"].ToString()), Name = ssn["season_name"].ToString() };

                        SeasonRecord<PlayerStats> sr = new()
                        {
                            Season = s,
                            Stats = new ObservableCollection<PlayerStats> { new PlayerStats(Player, s.ID, c.ID) }
                        };

                        //load matches
                        cmd = new MySqlCommand("SELECT h.name AS h_name, a.name AS a_name, " +
                                               "m.id AS match_id, m.datetime AS match_datetime, m.home_score AS h_score, m.away_score AS a_score, " +
                                               "m.overtime AS match_overtime, m.shootout AS match_shootout, m.forfeit AS match_forfeit, " +
                                               "m.qualification_id AS match_qualification_id, m.serie_match_number AS match_serie_match_number " +
                                               "FROM player_matches " +
                                               "INNER JOIN matches AS m ON m.id = match_id " +
                                               "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                               "INNER JOIN competitions AS c ON c.id = s.competition_id " +
                                               "INNER JOIN team AS h ON h.id = m.home_competitor " +
                                               "INNER JOIN team AS a ON a.id = m.away_competitor " +
                                               "WHERE player_id = " + Player.ID + " AND s.competition_id = " + c.ID + " AND m.season_id = " + s.ID + " " +
                                               "ORDER BY m.datetime DESC", connection);

                        DataTable matchTable = new();
                        matchTable.Load(cmd.ExecuteReader());

                        foreach (DataRow mtch in matchTable.Rows)
                        {
                            Team home = new();
                            home.Name = mtch["h_name"].ToString();
                            Team away = new();
                            away.Name = mtch["a_name"].ToString();

                            Match m = new()
                            {
                                ID = int.Parse(mtch["match_id"].ToString()),
                                Competition = c,
                                Season = s,
                                Datetime = DateTime.Parse(mtch["match_datetime"].ToString()),
                                HomeTeam = home,
                                AwayTeam = away,
                                HomeScore = int.Parse(mtch["h_score"].ToString()),
                                AwayScore = int.Parse(mtch["a_score"].ToString()),
                                Overtime = Convert.ToBoolean(int.Parse(mtch["match_overtime"].ToString())),
                                Shootout = Convert.ToBoolean(int.Parse(mtch["match_shootout"].ToString())),
                                Forfeit = Convert.ToBoolean(int.Parse(mtch["match_forfeit"].ToString()))
                            };

                            string partOfSeason = "Play-off";
                            if (int.Parse(mtch["match_serie_match_number"].ToString()) < 1)
                            {
                                partOfSeason = "Group";
                            }
                            else if (int.Parse(mtch["match_qualification_id"].ToString()) > 0)
                            {
                                partOfSeason = "Qualification";
                            }

                            m.Stats = new MatchStats(m);
                            ((MatchStats)m.Stats).PartOfSeason = partOfSeason;
                            ((MatchStats)m.Stats).Score = m.Score();

                            sr.Matches.Add(m);
                        }

                        cr.Seasons.Add(sr);
                    }

                    Competitions.Add(cr);
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

        private void LoadCompetitionsAsGoalie()
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT s.competition_id AS competition_id, c.name AS competition_name " +
                                                "FROM goalie_matches " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "INNER JOIN competitions AS c ON c.id = s.competition_id " +
                                                "WHERE player_id = " + Player.ID + " GROUP BY competition_id", connection);
            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    Competition c = new() { ID = int.Parse(row["competition_id"].ToString()), Name = row["competition_name"].ToString() };

                    CompetitionRecord<GoalieStats> cr = new()
                    {
                        Competition = c,
                        Stats = new ObservableCollection<GoalieStats> { new GoalieStats(Player, SportsData.NOID, c.ID) }
                    };

                    //load seasons
                    cmd = new MySqlCommand("SELECT s.id AS season_id, s.name AS season_name " +
                                                "FROM goalie_matches " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE player_id = " + Player.ID + " GROUP BY season_id", connection);

                    DataTable seasonTable = new();
                    seasonTable.Load(cmd.ExecuteReader());

                    foreach (DataRow ssn in seasonTable.Rows)
                    {
                        Season s = new() { ID = int.Parse(ssn["season_id"].ToString()), Name = ssn["season_name"].ToString() };

                        SeasonRecord<GoalieStats> sr = new()
                        {
                            Season = s,
                            Stats = new ObservableCollection<GoalieStats> { new GoalieStats(Player, s.ID, c.ID) }
                        };

                        //load matches
                        cmd = new MySqlCommand("SELECT h.name AS h_name, a.name AS a_name, " +
                                               "m.id AS match_id, m.datetime AS match_datetime, m.home_score AS h_score, m.away_score AS a_score, " +
                                               "m.overtime AS match_overtime, m.shootout AS match_shootout, m.forfeit AS match_forfeit, " +
                                               "m.qualification_id AS match_qualification_id, m.serie_match_number AS match_serie_match_number " +
                                               "FROM goalie_matches " +
                                               "INNER JOIN matches AS m ON m.id = match_id " +
                                               "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                               "INNER JOIN competitions AS c ON c.id = s.competition_id " +
                                               "INNER JOIN team AS h ON h.id = m.home_competitor " +
                                               "INNER JOIN team AS a ON a.id = m.away_competitor " +
                                               "WHERE player_id = " + Player.ID + " AND s.competition_id = " + c.ID + " AND m.season_id = " + s.ID + " " +
                                               "ORDER BY m.datetime DESC", connection);

                        DataTable matchTable = new();
                        matchTable.Load(cmd.ExecuteReader());

                        foreach (DataRow mtch in matchTable.Rows)
                        {
                            Team home = new();
                            home.Name = mtch["h_name"].ToString();
                            Team away = new();
                            away.Name = mtch["a_name"].ToString();

                            Match m = new()
                            {
                                ID = int.Parse(mtch["match_id"].ToString()),
                                Competition = c,
                                Season = s,
                                Datetime = DateTime.Parse(mtch["match_datetime"].ToString()),
                                HomeTeam = home,
                                AwayTeam = away,
                                HomeScore = int.Parse(mtch["h_score"].ToString()),
                                AwayScore = int.Parse(mtch["a_score"].ToString()),
                                Overtime = Convert.ToBoolean(int.Parse(mtch["match_overtime"].ToString())),
                                Shootout = Convert.ToBoolean(int.Parse(mtch["match_shootout"].ToString())),
                                Forfeit = Convert.ToBoolean(int.Parse(mtch["match_forfeit"].ToString()))
                            };

                            string partOfSeason = "Play-off";
                            if (int.Parse(mtch["match_serie_match_number"].ToString()) < 1)
                            {
                                partOfSeason = "Group";
                            }
                            else if (int.Parse(mtch["match_qualification_id"].ToString()) > 0)
                            {
                                partOfSeason = "Qualification";
                            }

                            m.Stats = new MatchStats(m);
                            ((MatchStats)m.Stats).PartOfSeason = partOfSeason;
                            ((MatchStats)m.Stats).Score = m.Score();

                            sr.Matches.Add(m);
                        }

                        cr.Seasons.Add(sr);
                    }

                    CompetitionsAsGoalie.Add(cr);
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

        private void LoadPlayerInMatchStatsSeries()
        {
            //goals per match
            ChartValues<int> goals = new();

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                goals.Add(PlayerInMatchStats[i].Goals);
            }

            PlayerInMatchStatsSeries.Add(new LineSeries
            {
                Values = goals,
                Fill = Brushes.Transparent,
                Title = "Goals",
                LabelPoint = value => "Goals: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });

            //assists per match
            ChartValues<int> assists = new();

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                assists.Add(PlayerInMatchStats[i].Assists);
            }

            PlayerInMatchStatsSeries.Add(new LineSeries
            {
                Values = assists,
                Fill = Brushes.Transparent,
                Title = "Assists",
                LabelPoint = value => "Assists: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });

            //penalty minutes per match
            ChartValues<int> penalties = new();

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                penalties.Add(PlayerInMatchStats[i].PenaltyMinutes);
            }

            PlayerInMatchStatsSeries.Add(new LineSeries
            {
                Values = penalties,
                Fill = Brushes.Transparent,
                Title = "Penalty minutes",
                LabelPoint = value => "Penalty minutes: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });
        }

        private void LoadPlayerInMatchStatsSumSeries()
        {
            //goals until match
            ChartValues<int> goals = new();
            int sum = 0;

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                sum += PlayerInMatchStats[i].Goals;
                goals.Add(sum);
            }

            PlayerInMatchStatsSumSeries.Add(new LineSeries
            {
                Values = goals,
                Fill = Brushes.Transparent,
                Title = "Goals",
                LabelPoint = value => "Goals: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });

            //assists until match
            ChartValues<int> assists = new();
            sum = 0;

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                sum += PlayerInMatchStats[i].Assists;
                assists.Add(sum);
            }

            PlayerInMatchStatsSumSeries.Add(new LineSeries
            {
                Values = assists,
                Fill = Brushes.Transparent,
                Title = "Assists",
                LabelPoint = value => "Assists: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });

            //penalty minutes until match
            ChartValues<int> penalties = new();
            sum = 0;

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                sum += PlayerInMatchStats[i].PenaltyMinutes;
                penalties.Add(sum);
            }

            PlayerInMatchStatsSumSeries.Add(new LineSeries
            {
                Values = penalties,
                Fill = Brushes.Transparent,
                Title = "Penalty minutes",
                LabelPoint = value => "Penalty minutes: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });
        }

        private void LoadPlayerInMatchStatsAverageSeries()
        {
            //goals average until match
            ChartValues<double> goals = new();
            int sum = 0;

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                sum += PlayerInMatchStats[i].Goals;
                goals.Add(Math.Round(sum / (double)(i + 1), 2));
            }

            PlayerInMatchStatsAverageSeries.Add(new LineSeries
            {
                Values = goals,
                Fill = Brushes.Transparent,
                Title = "Goals",
                LabelPoint = value => "Goals: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });

            //assists average until match
            ChartValues<double> assists = new();
            sum = 0;

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                sum += PlayerInMatchStats[i].Assists;
                assists.Add(Math.Round(sum / (double)(i + 1), 2));
            }

            PlayerInMatchStatsAverageSeries.Add(new LineSeries
            {
                Values = assists,
                Fill = Brushes.Transparent,
                Title = "Assists",
                LabelPoint = value => "Assists: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });

            //penalty minutes average until match
            ChartValues<double> penalties = new();
            sum = 0;

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                sum += PlayerInMatchStats[i].PenaltyMinutes;
                penalties.Add(Math.Round(sum / (double)(i + 1), 2));
            }

            PlayerInMatchStatsAverageSeries.Add(new LineSeries
            {
                Values = penalties,
                Fill = Brushes.Transparent,
                Title = "Penalty minutes",
                LabelPoint = value => "Penalty minutes: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });
        }
    }
}