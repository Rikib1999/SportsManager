﻿using CSharpZapoctak.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CSharpZapoctak.Models
{
    class Serie : ViewModelBase
    {
        private Team firstTeam = new Team { id = -1, Name = "-- no team --" };
        public Team FirstTeam
        {
            get { return firstTeam; }
            set
            {
                firstTeam = value;
                if (value.id != -1)
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

        private Team secondTeam = new Team { id = -1, Name = "-- no team --" };
        public Team SecondTeam
        {
            get { return secondTeam; }
            set
            {
                secondTeam = value;
                if (value.id != -1)
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


        private ObservableCollection<Match> matches = new ObservableCollection<Match>();
        public ObservableCollection<Match> Matches
        {
            get { return matches; }
            set
            {
                matches = value;
                OnPropertyChanged();
            }
        }

        public Visibility preLineVisibility = Visibility.Visible;
        public Visibility PreLineVisibility
        {
            get { return preLineVisibility; }
            set
            {
                preLineVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility postLineVisibility = Visibility.Visible;
        public Visibility PostLineVisibility
        {
            get { return postLineVisibility; }
            set
            {
                postLineVisibility = value;
                OnPropertyChanged();
            }
        }

        #region Setting matches
        public Team winner = new Team { id = -1, Name = "-- no team --" };

        public ObservableCollection<int> firstScore = new ObservableCollection<int>();
        public ObservableCollection<int> FirstScore
        {
            get { return firstScore; }
            set
            {
                firstScore = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<int> secondScore = new ObservableCollection<int>();
        public ObservableCollection<int> SecondScore
        {
            get { return secondScore; }
            set
            {
                secondScore = value;
                OnPropertyChanged();
            }
        }

        public Visibility addMatchVisibility = Visibility.Collapsed;
        public Visibility AddMatchVisibility
        {
            get { return addMatchVisibility; }
            set
            {
                addMatchVisibility = value;
                OnPropertyChanged();
            }
        }
        
        public Visibility removeTeamVisibility = Visibility.Visible;
        public Visibility RemoveTeamVisibility
        {
            get { return removeTeamVisibility; }
            set
            {
                removeTeamVisibility = value;
                OnPropertyChanged();
            }
        }

        public void InsertMatch(Match m, int firstTeam, int firstToWin)
        {
            if (FirstTeam.id == -1 || SecondTeam.id == -1)
            {
                if (m.HomeTeam.id == firstTeam)
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

            if (Matches.Count == 0 || Matches[Matches.Count - 1].serieNumber < m.serieNumber)
            {
                Matches.Add(m);
                if (m.Played)
                {
                    if (m.HomeTeam.id == FirstTeam.id)
                    {
                        FirstScore.Add(m.HomeScore);
                        SecondScore.Add(m.AwayScore);
                    }
                    else
                    {
                        ////////add them after all match insertions
                        FirstScore.Add(m.AwayScore);
                        SecondScore.Add(m.HomeScore);
                    }
                }
            }
            else
            {
                for (int i = 0; i < Matches.Count; i++)
                {
                    if (Matches[i].serieNumber > m.serieNumber)
                    {
                        Matches.Insert(i, m);

                        if (m.HomeTeam.id == FirstTeam.id)
                        {
                            FirstScore.Insert(i, m.HomeScore);
                            SecondScore.Insert(i, m.AwayScore);
                        }
                        else
                        {///////////////////////////
                            FirstScore.Insert(i, m.AwayScore);
                            SecondScore.Insert(i, m.HomeScore);
                        }
                    }
                }
            }

            if (m.Played)
            {
                RemoveTeamVisibility = Visibility.Collapsed;
            }

            int firstWins = 0;
            int secondWins = 0;
            foreach (Match match in matches)
            {
                if (match.HomeScore > match.AwayScore)
                {
                    if (match.HomeTeam.id == FirstTeam.id) { firstWins++; } else { secondWins++; }
                }
                else if (match.HomeScore < match.AwayScore)
                {
                    if (match.HomeTeam.id == FirstTeam.id) { secondWins++; } else { firstWins++; }
                }
            }
            if (firstWins >= firstToWin)
            {
                winner = FirstTeam;
            }
            else if (secondWins >= firstToWin)
            {
                winner = SecondTeam;
            }
        }
        #endregion

        #region Setting teams
        private Team firstSelectedTeam;
        public Team FirstSelectedTeam
        {
            get { return firstSelectedTeam; }
            set
            {
                firstSelectedTeam = value;
                OnPropertyChanged();
            }
        }

        private Team secondSelectedTeam;
        public Team SecondSelectedTeam
        {
            get { return secondSelectedTeam; }
            set
            {
                secondSelectedTeam = value;
                OnPropertyChanged();
            }
        }

        private bool firstIsEnabled = true;
        public bool FirstIsEnabled
        {
            get { return firstIsEnabled; }
            set
            {
                firstIsEnabled = value;
                OnPropertyChanged();
            }
        }

        private int firstLock;
        public int FirstLock
        {
            get { return firstLock; }
            set
            {
                firstLock = value;
                if (firstLock == 0)
                {
                    FirstIsEnabled = true;
                }
                else
                {
                    FirstIsEnabled = false;
                }
            }
        }

        private Visibility firstSelectedVisibility = Visibility.Collapsed;
        public Visibility FirstSelectedVisibility
        {
            get { return firstSelectedVisibility; }
            set
            {
                firstSelectedVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility firstNotSelectedVisibility = Visibility.Visible;
        public Visibility FirstNotSelectedVisibility
        {
            get { return firstNotSelectedVisibility; }
            set
            {
                firstNotSelectedVisibility = value;
                OnPropertyChanged();
            }
        }

        private bool secondIsEnabled = true;
        public bool SecondIsEnabled
        {
            get { return secondIsEnabled; }
            set
            {
                secondIsEnabled = value;
                OnPropertyChanged();
            }
        }

        private int secondLock;
        public int SecondLock
        {
            get { return secondLock; }
            set
            {
                secondLock = value;
                if (secondLock == 0)
                {
                    SecondIsEnabled = true;
                }
                else
                {
                    SecondIsEnabled = false;
                }
            }
        }

        private Visibility secondSelectedVisibility = Visibility.Collapsed;
        public Visibility SecondSelectedVisibility
        {
            get { return secondSelectedVisibility; }
            set
            {
                secondSelectedVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility secondNotSelectedVisibility = Visibility.Visible;
        public Visibility SecondNotSelectedVisibility
        {
            get { return secondNotSelectedVisibility; }
            set
            {
                secondNotSelectedVisibility = value;
                OnPropertyChanged();
            }
        }
        #endregion
    }
}
