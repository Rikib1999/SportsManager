using SportsManager.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace SportsManager.Models
{
    /// <summary>
    /// Class for representing a teams score in a match of a serie.
    /// </summary>
    public class TeamScoreInSerieMatch : NotifyPropertyChanged
    {
        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="score">Teams score.</param>
        /// <param name="match">Match instance.</param>
        public TeamScoreInSerieMatch(int score, Match match)
        {
            Value = score;
            Match = match;
        }

        private int value;
        /// <summary>
        /// Score of the team in given match.
        /// </summary>
        public int Value
        {
            get => value;
            set
            {
                this.value = value;
                OnPropertyChanged();
            }
        }

        private Match match;
        /// <summary>
        /// Match instance of the serie.
        /// </summary>
        public Match Match
        {
            get => match;
            set
            {
                match = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Class for representing one serie of a bracket.
    /// </summary>
    public class Serie : NotifyPropertyChanged
    {
        private Team firstTeam = new() { ID = SportsData.NOID, Name = "-- no team --" };
        /// <summary>
        /// Instance of the first team of the bracket. Default value is a new Team object with ID = -1 and Name = "-- no team --".
        /// </summary>
        public Team FirstTeam
        {
            get => firstTeam;
            set
            {
                firstTeam = value;
                if (value.ID != SportsData.NOID || !value.SavedInDatabase)
                {
                    FirstSelectedVisibility = Visibility.Visible;
                    FirstNotSelectedVisibility = Visibility.Collapsed;
                }
                else
                {
                    FirstSelectedVisibility = Visibility.Collapsed;
                    FirstNotSelectedVisibility = Visibility.Visible;
                }
                OnPropertyChanged();
            }
        }

        private Team secondTeam = new() { ID = SportsData.NOID, Name = "-- no team --" };
        /// <summary>
        /// Instance of the second team of the bracket. Default value is a new Team object with ID = -1 and Name = "-- no team --".
        /// </summary>
        public Team SecondTeam
        {
            get => secondTeam;
            set
            {
                secondTeam = value;
                if (value.ID != SportsData.NOID || !value.SavedInDatabase)
                {
                    SecondSelectedVisibility = Visibility.Visible;
                    SecondNotSelectedVisibility = Visibility.Collapsed;
                }
                else
                {
                    SecondSelectedVisibility = Visibility.Collapsed;
                    SecondNotSelectedVisibility = Visibility.Visible;
                }
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Match> matches = new();
        /// <summary>
        /// Collection of all matches played in the serie.
        /// </summary>
        public ObservableCollection<Match> Matches
        {
            get => matches;
            set
            {
                matches = value;
                OnPropertyChanged();
            }
        }

        private Visibility preLineVisibility = Visibility.Visible;
        /// <summary>
        /// Visibility of a line conecting previous series in bracket.
        /// </summary>
        public Visibility PreLineVisibility
        {
            get => preLineVisibility;
            set
            {
                preLineVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility postLineVisibility = Visibility.Visible;
        /// <summary>
        /// Visibility for a line connecting following serie in bracket.
        /// </summary>
        public Visibility PostLineVisibility
        {
            get => postLineVisibility;
            set
            {
                postLineVisibility = value;
                OnPropertyChanged();
            }
        }

        #region Setting matches
        /// <summary>
        /// Instance of a winner team of the serie.
        /// </summary>
        public Team Winner { get; set; } = new() { ID = SportsData.NOID, Name = "-- no team --" };

        private ObservableCollection<TeamScoreInSerieMatch> firstScore = new();
        /// <summary>
        /// Collection of first team scores in the serie matches.
        /// </summary>
        public ObservableCollection<TeamScoreInSerieMatch> FirstScore
        {
            get => firstScore;
            set
            {
                firstScore = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TeamScoreInSerieMatch> secondScore = new();
        /// <summary>
        /// Collection of second team scores in the serie matches.
        /// </summary>
        public ObservableCollection<TeamScoreInSerieMatch> SecondScore
        {
            get => secondScore;
            set
            {
                secondScore = value;
                OnPropertyChanged();
            }
        }

        private Visibility addMatchVisibility = Visibility.Collapsed;
        /// <summary>
        /// Visibility of the button for adding a new match to the serie.
        /// </summary>
        public Visibility AddMatchVisibility
        {
            get => addMatchVisibility;
            set
            {
                addMatchVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility removeFirstTeamVisibility = Visibility.Visible;
        /// <summary>
        /// Visibility of the button for removing the first team from the serie.
        /// </summary>
        public Visibility RemoveFirstTeamVisibility
        {
            get => removeFirstTeamVisibility;
            set
            {
                removeFirstTeamVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility removeSecondTeamVisibility = Visibility.Visible;
        /// <summary>
        /// Visibility of the button for removing the second team from the serie.
        /// </summary>
        public Visibility RemoveSecondTeamVisibility
        {
            get => removeSecondTeamVisibility;
            set
            {
                removeSecondTeamVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility addFirstTeamVisibility = Visibility.Visible;
        /// <summary>
        /// Visibility of the button for adding the first team to the serie.
        /// </summary>
        public Visibility AddFirstTeamVisibility
        {
            get => addFirstTeamVisibility;
            set
            {
                addFirstTeamVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility addSecondTeamVisibility = Visibility.Visible;
        /// <summary>
        /// Visibility of the button for adding the second team to the serie.
        /// </summary>
        public Visibility AddSecondTeamVisibility
        {
            get => addSecondTeamVisibility;
            set
            {
                addSecondTeamVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Inserts a new match to the serie. Sets teams and scores from the match and calculates the winner of the serie.
        /// </summary>
        /// <param name="m">New match instance.</param>
        /// <param name="firstTeamID">Identification number of the first team of the serie.</param>
        /// <param name="firstToWin">Number of wins in the serie needed for winning the serie.</param>
        public void InsertMatch(Match m, int firstTeamID, int firstToWin)
        {
            //set teams
            if (FirstTeam.ID == SportsData.NOID || SecondTeam.ID == SportsData.NOID)
            {
                if (m.HomeTeam.ID == firstTeamID)
                {
                    FirstTeam = m.HomeTeam;
                    SecondTeam = m.AwayTeam;
                }
                else
                {
                    FirstTeam = m.AwayTeam;
                    SecondTeam = m.HomeTeam;
                }
            }

            //add match
            if (Matches.Count == 0 || Matches[^1].SerieNumber < m.SerieNumber)
            {
                Matches.Add(m);
                if (m.Played)
                {
                    if (m.HomeTeam.ID == FirstTeam.ID)
                    {
                        FirstScore.Add(new TeamScoreInSerieMatch(m.HomeScore, m));
                        SecondScore.Add(new TeamScoreInSerieMatch(m.AwayScore, m));
                    }
                    else
                    {
                        FirstScore.Add(new TeamScoreInSerieMatch(m.AwayScore, m));
                        SecondScore.Add(new TeamScoreInSerieMatch(m.HomeScore, m));
                    }
                }
            }
            else
            {
                int matchCount = Matches.Count;
                for (int i = 0; i < matchCount; i++)
                {
                    if (Matches[i].SerieNumber > m.SerieNumber)
                    {
                        Matches.Insert(i, m);

                        int insertAt = i;
                        if (Matches.Any(x => !x.Played) && insertAt != 0) { insertAt--; }

                        if (m.Played)
                        {
                            if (m.HomeTeam.ID == FirstTeam.ID)
                            {
                                FirstScore.Insert(insertAt, new TeamScoreInSerieMatch(m.HomeScore, m));
                                SecondScore.Insert(insertAt, new TeamScoreInSerieMatch(m.AwayScore, m));
                            }
                            else
                            {
                                FirstScore.Insert(insertAt, new TeamScoreInSerieMatch(m.AwayScore, m));
                                SecondScore.Insert(insertAt, new TeamScoreInSerieMatch(m.HomeScore, m));
                            }
                        }
                        break;
                    }
                }
            }

            //check winner
            int firstWins = 0;
            int secondWins = 0;
            foreach (Match match in matches)
            {
                if (match.HomeScore > match.AwayScore)
                {
                    if (match.HomeTeam.ID == FirstTeam.ID) { firstWins++; } else { secondWins++; }
                }
                else if (match.HomeScore < match.AwayScore)
                {
                    if (match.HomeTeam.ID == FirstTeam.ID) { secondWins++; } else { firstWins++; }
                }
            }
            if (firstWins >= firstToWin)
            {
                Winner = FirstTeam;
            }
            else if (secondWins >= firstToWin)
            {
                Winner = SecondTeam;
            }
        }
        #endregion

        #region Setting teams
        private Team firstSelectedTeam;
        /// <summary>
        /// First selected team for adding to the serie.
        /// </summary>
        public Team FirstSelectedTeam
        {
            get => firstSelectedTeam;
            set
            {
                firstSelectedTeam = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Second selected team for adding to the serie.
        /// </summary>
        private Team secondSelectedTeam;
        public Team SecondSelectedTeam
        {
            get => secondSelectedTeam;
            set
            {
                secondSelectedTeam = value;
                OnPropertyChanged();
            }
        }

        private bool firstIsEnabled = true;
        /// <summary>
        /// True if the first team is not selected, otherwise false.
        /// </summary>
        public bool FirstIsEnabled
        {
            get => firstIsEnabled;
            set
            {
                firstIsEnabled = value;
                AddFirstTeamVisibility = firstIsEnabled ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

        private int firstLock;
        /// <summary>
        /// Number of locks at the first team. If zero, FirstIsEnabled sets to true.
        /// </summary>
        public int FirstLock
        {
            get => firstLock;
            set
            {
                firstLock = value;
                FirstIsEnabled = firstLock == 0;
            }
        }

        private Visibility firstSelectedVisibility = Visibility.Collapsed;
        /// <summary>
        /// Visibility of the selected first team. Visible if team is selected.
        /// </summary>
        public Visibility FirstSelectedVisibility
        {
            get => firstSelectedVisibility;
            set
            {
                firstSelectedVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility firstNotSelectedVisibility = Visibility.Visible;
        /// <summary>
        /// Visibility of the not selected first team. Visible if team is not selected.
        /// </summary>
        public Visibility FirstNotSelectedVisibility
        {
            get => firstNotSelectedVisibility;
            set
            {
                firstNotSelectedVisibility = value;
                OnPropertyChanged();
            }
        }

        private bool secondIsEnabled = true;
        /// <summary>
        /// True if the second team is not selected, otherwise false.
        /// </summary>
        public bool SecondIsEnabled
        {
            get => secondIsEnabled;
            set
            {
                secondIsEnabled = value;
                AddSecondTeamVisibility = secondIsEnabled ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

        private int secondLock;
        /// <summary>
        /// Number of locks at the second team. If zero, SecondIsEnabled sets to true.
        /// </summary>
        public int SecondLock
        {
            get => secondLock;
            set
            {
                secondLock = value;
                SecondIsEnabled = secondLock == 0;
            }
        }

        private Visibility secondSelectedVisibility = Visibility.Collapsed;
        /// <summary>
        /// Visibility of the selected second team. Visible if team is selected.
        /// </summary>
        public Visibility SecondSelectedVisibility
        {
            get => secondSelectedVisibility;
            set
            {
                secondSelectedVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility secondNotSelectedVisibility = Visibility.Visible;
        /// <summary>
        /// Visibility of the not selected second team. Visible if team is not selected.
        /// </summary>
        public Visibility SecondNotSelectedVisibility
        {
            get => secondNotSelectedVisibility;
            set
            {
                secondNotSelectedVisibility = value;
                OnPropertyChanged();
            }
        }
        #endregion
    }
}