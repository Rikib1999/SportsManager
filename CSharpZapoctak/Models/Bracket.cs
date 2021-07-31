using CSharpZapoctak.ViewModels;
using System;
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
                    if(i == rounds - 1)
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
    }
}
