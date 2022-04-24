using CSharpZapoctak.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CSharpZapoctak.Models
{
    public class TeamScoreInSerieMatch : NotifyPropertyChanged
    {
        public TeamScoreInSerieMatch(int score, Match match)
        {
            Value = score;
            Match = match;
        }

        private int value;
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

    public class Serie : NotifyPropertyChanged
    {
        private Team firstTeam = new() { ID = SportsData.NOID, Name = "-- no team --" };
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
        public Team Winner { get; set; } = new() { ID = SportsData.NOID, Name = "-- no team --" };

        private ObservableCollection<TeamScoreInSerieMatch> firstScore = new();
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
        public Visibility AddSecondTeamVisibility
        {
            get => addSecondTeamVisibility;
            set
            {
                addSecondTeamVisibility = value;
                OnPropertyChanged();
            }
        }

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
        public Team FirstSelectedTeam
        {
            get => firstSelectedTeam;
            set
            {
                firstSelectedTeam = value;
                OnPropertyChanged();
            }
        }

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