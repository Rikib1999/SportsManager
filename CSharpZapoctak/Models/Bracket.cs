using CSharpZapoctak.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace CSharpZapoctak.Models
{
    class Bracket : ViewModelBase
    {
        public int id = (int)EntityState.NotSelected;

        private string name = "";
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private int seasonID;
        public int SeasonID
        {
            get { return seasonID; }
            set
            {
                seasonID = value;
                OnPropertyChanged();
            }
        }

        //collection of rounds, each item is one round, each round consists of 2^n series, last item is final
        private ObservableCollection<List<Serie>> series;
        public ObservableCollection<List<Serie>> Series
        {
            get { return series; }
            set
            {
                series = value;
                OnPropertyChanged();
            }
        }

        public Bracket(int id, string name, int seasonID, int rounds)
        {
            this.id = id;
            Name = name;
            SeasonID = seasonID;

            Series = new ObservableCollection<List<Serie>>();
            int pow = 1;
            for (int i = 0; i < rounds; i++)
            {
                List<Serie> round = new List<Serie>();
                for (int j = 0; j < pow; j++)
                {
                    Serie s = new Serie();
                    round.Add(s);
                    if (i == 0)
                    {
                        s.PostLineVisibility = Visibility.Collapsed;
                    }
                    if (i == rounds - 1)
                    {
                        s.PreLineVisibility = Visibility.Collapsed;
                    }
                }
                Series.Insert(0, round);
                pow *= 2;
            }
        }

        public Bracket(int rounds)
        {
            Series = new ObservableCollection<List<Serie>>();
            int pow = 1;
            for (int i = 0; i < rounds; i++)
            {
                List<Serie> round = new List<Serie>();
                for (int j = 0; j < pow; j++)
                {
                    Serie s = new Serie();
                    round.Add(s);
                    if (i == 0)
                    {
                        s.PostLineVisibility = Visibility.Collapsed;
                    }
                    if (i == rounds - 1)
                    {
                        s.PreLineVisibility = Visibility.Collapsed;
                    }
                }
                Series.Insert(0, round);
                pow *= 2;
            }
        }

        public (int, int) GetSerieRoundIndex(Serie s)
        {
            for (int i = 0; i < Series.Count; i++)
            {
                for (int j = 0; j < Series[i].Count; j++)
                {
                    if (Series[i][j] == s)
                    {
                        return (i, j);
                    }
                }
            }
            return (-1, -1);
        }

        #region Setting matches
        public bool IsThereNextCompetitor(int round, int index)
        {
            if (round < Series.Count - 1)
            {
                if (index % 2 == 0)
                {
                    if (Series[round + 1][index / 2].FirstTeam.id == -1)
                    {
                        return false;
                    }
                }
                else
                {
                    if (Series[round + 1][index / 2].SecondTeam.id == -1)
                    {
                        return false;
                    }
                }
                return true;
            }
            return true;
        }

        public bool HasWinner(int round, int index)
        {
            if (round < Series.Count)
            {
                if (Series[round][index].FirstTeam.id != -1 && Series[round][index].SecondTeam.id == -1)
                {
                    Series[round][index].winner = Series[round][index].FirstTeam;
                }
                if (Series[round][index].FirstTeam.id == -1 && Series[round][index].SecondTeam.id != -1)
                {
                    Series[round][index].winner = Series[round][index].SecondTeam;
                }

                return Series[round][index].winner.id != -1;
            }
            return false;
        }

        public void LockSeriesBefore(int round, int index)
        {
            if (round == -1) { return; }

            Series[round][index].IsEnabled = false;

            LockSeriesBefore(round - 1, index * 2);
            LockSeriesBefore(round - 1, (index * 2) + 1);
        }

        public void LockSeriesAfter(int round, int index)
        {
            if (round == Series.Count) { return; }

            Series[round][index].IsEnabled = false;

            LockSeriesAfter(round + 1, index / 2);
        }

        public void PrepareSeries()
        {
            SeedCompetitors();
            FindSeriesToLock(Series.Count - 1, 0);
        }

        public void SeedCompetitors()
        {
            //for each round except last one (there is no match after final)
            for (int r = 0; r < Series.Count - 1; r++)
            {
                //for each serie in round
                for (int i = 0; i < Series[r].Count; i++)
                {
                    //if there is no competitor in next match and this match was already played and is closed or its a BYE match
                    if (!IsThereNextCompetitor(r, i) && HasWinner(r, i))
                    {
                        //advance winner to the next match
                        if (i % 2 == 0)
                        {
                            Series[r + 1][i / 2].FirstTeam = Series[r][i].winner;
                        }
                        else
                        {
                            Series[r + 1][i / 2].SecondTeam = Series[r][i].winner;
                        }
                    }
                }
            }
        }

        public void FindSeriesToLock(int round, int index)
        {
            if (round == -1) { return; }

            if (Series[round][index].Matches.Count > 0)
            {
                if (Series[round][index].FirstTeam.id != -1 && Series[round][index].SecondTeam.id == -1)
                {
                    LockSeriesBefore(round - 1, index * 2);
                }
                else if (Series[round][index].FirstTeam.id == -1 && Series[round][index].SecondTeam.id != -1)
                {
                    LockSeriesBefore(round - 1, (index * 2) + 1);
                }
                else
                {
                    LockSeriesBefore(round - 1, index * 2);
                    LockSeriesBefore(round - 1, (index * 2) + 1);
                    LockSeriesAfter(round + 1, index / 2);
                }
            }
            else
            {
                FindSeriesToLock(round - 1, index * 2);
                FindSeriesToLock(round - 1, (index * 2) + 1);
            }
        }
        #endregion

        #region Setting teams
        public void IsEnabledTreeAfterInsertionAt(int round, int index, int position, int lockChange)
        {
            if (round != 0)
            {
                int newIndex = index * 2;
                if (position == 2) { newIndex++; }
                IsEnabledPreviousTree(round - 1, newIndex, lockChange);
            }

            if (round < Series.Count - 1)
            {
                int newPosition = 2;
                if (index % 2 == 0) { newPosition = 1; }
                IsEnabledNextCompetitor(round + 1, index / 2, newPosition, lockChange);
            }
        }

        private void IsEnabledPreviousTree(int round, int index, int lockChange)
        {
            Series[round][index].FirstLock += lockChange;
            Series[round][index].SecondLock += lockChange;

            if (round != 0)
            {
                IsEnabledPreviousTree(round - 1, index * 2, lockChange);
                IsEnabledPreviousTree(round - 1, (index * 2) + 1, lockChange);
            }
        }

        private void IsEnabledNextCompetitor(int round, int index, int position, int lockChange)
        {
            if (position == 1)
            {
                Series[round][index].FirstLock += lockChange;
            }
            else
            {
                Series[round][index].SecondLock += lockChange;
            }

            if (round < Series.Count - 1)
            {
                int newPosition = 2;
                if (index % 2 == 0) { newPosition = 1; }
                IsEnabledNextCompetitor(round + 1, index / 2, newPosition, lockChange);
            }
        }
        #endregion
    }
}
