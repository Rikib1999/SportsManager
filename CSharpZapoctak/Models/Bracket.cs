using CSharpZapoctak.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CSharpZapoctak.Models
{
    class Bracket : NotifyPropertyChanged
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

            CreateBracket(rounds);
        }

        public Bracket(int rounds)
        {
            CreateBracket(rounds);
        }

        private void CreateBracket(int rounds)
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
                //if has winner
                if (Series[round][index].winner.id != -1)
                {
                    return true;
                }
                //if there are not winners before
                if (round != 0 && (Series[round - 1][index * 2].winner.id == -1 || Series[round - 1][(index * 2) + 1].winner.id == -1))
                {
                    return false;
                }
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

        public void PrepareSeries()
        {
            //for each round
            for (int r = 0; r < Series.Count; r++)
            {
                //for each serie in round
                for (int i = 0; i < Series[r].Count; i++)
                {
                    //if there is no competitor in next match and this match was already played and is closed or its a BYE match
                    if (r != Series.Count - 1 && !IsThereNextCompetitor(r, i) && HasWinner(r, i))
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

                    if (Series[r][i].winner.id == -1 && Series[r][i].FirstTeam.id != -1 && Series[r][i].SecondTeam.id != -1)
                    {
                        Series[r][i].AddMatchVisibility = Visibility.Visible;
                    }
                }
            }

            CollapseRemoveButton(Series.Count - 1, 0);
        }

        private void CollapseRemoveButton(int round, int index)
        {
            if (round == -1) { return; }

            if (Series[round][index].Matches.Count(x => x.Played) > 0)
            {
                CollapseAllRemoveButton(round, index);
            }
            else if (Series[round][index].Matches.Count > 0)
            {
                if (Series[round][index].Matches[0].HomeTeam.id == -1)
                {
                    Series[round][index].RemoveFirstTeamVisibility = Visibility.Collapsed;
                    CollapseRemoveButton(round - 1, index * 2);
                    CollapseRemoveButton(round - 1, (index * 2) + 1);
                }
                else
                {
                    CollapseAllRemoveButton(round - 1, index * 2);
                }

                if (Series[round][index].Matches[0].AwayTeam.id == -1)
                {
                    Series[round][index].RemoveSecondTeamVisibility = Visibility.Collapsed;
                    CollapseRemoveButton(round - 1, index * 2);
                    CollapseRemoveButton(round - 1, (index * 2) + 1);
                }
                else
                {
                    CollapseAllRemoveButton(round - 1, (index * 2) + 1);
                }
            }
            else
            {
                Series[round][index].RemoveFirstTeamVisibility = Visibility.Collapsed;
                Series[round][index].RemoveSecondTeamVisibility = Visibility.Collapsed;
                CollapseRemoveButton(round - 1, index * 2);
                CollapseRemoveButton(round - 1, (index * 2) + 1);
            }
        }

        private void CollapseAllRemoveButton(int round, int index)
        {
            if (round == -1) { return; }

            Series[round][index].RemoveFirstTeamVisibility = Visibility.Collapsed;
            Series[round][index].RemoveSecondTeamVisibility = Visibility.Collapsed;
            CollapseAllRemoveButton(round - 1, index * 2);
            CollapseAllRemoveButton(round - 1, (index * 2) + 1);
        }

        //sets advancing team of the serie after serie modification
        public void ResetSeriesAdvanced(int round, int index, int position)
        {
            if (round == Series.Count) { return; }

            if (position == 1)
            {
                Series[round][index].FirstTeam = new Team { id = -1 };
            }
            else
            {
                Series[round][index].SecondTeam = new Team { id = -1 };
            }
            Series[round][index].winner = new Team { id = -1 };
            Series[round][index].RemoveFirstTeamVisibility = Visibility.Visible;
            Series[round][index].RemoveSecondTeamVisibility = Visibility.Visible;
            Series[round][index].AddMatchVisibility = Visibility.Collapsed;

            int newPosition = 2;
            if (index % 2 == 0) { newPosition = 1; }
            ResetSeriesAdvanced(round + 1, index / 2, newPosition);
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
