using SportsManager.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace SportsManager.Models
{
    /// <summary>
    /// Bracket implementation. Used in qualification and play-off.
    /// </summary>
    public class Bracket : NotifyPropertyChanged
    {
        /// <summary>
        /// Identification number of bracket.
        /// </summary>
        public int ID { get; set; } = SportsData.NOID;

        private string name = "";
        /// <summary>
        /// Brackets name.
        /// </summary>
        /// <example>Qualification 1, Play-off</example>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private int seasonID;
        /// <summary>
        /// Identification number of the season in which the current bracket is.
        /// </summary>
        public int SeasonID
        {
            get => seasonID;
            set
            {
                seasonID = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<List<Serie>> series;
        /// <summary>
        /// Collection of rounds, each item is one round, each round consists of 2^n series, last item is final.
        /// </summary>
        public ObservableCollection<List<Serie>> Series
        {
            get => series;
            set
            {
                series = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Instantiates new bracket with provided data.
        /// </summary>
        /// <param name="id">Identification number of the bracket.</param>
        /// <param name="name">Name of the bracket.</param>
        /// <param name="seasonID">Identification number of the season in which the bracket is.</param>
        /// <param name="rounds">Number of rounds of the bracket.</param>
        public Bracket(int id, string name, int seasonID, int rounds)
        {
            ID = id;
            Name = name;
            SeasonID = seasonID;

            CreateBracket(rounds);
        }

        /// <summary>
        /// Instantiates new bracket without identification number and name.
        /// </summary>
        /// <param name="rounds">Number of rounds of the bracket.</param>
        public Bracket(int rounds)
        {
            CreateBracket(rounds);
        }

        /// <summary>
        /// Creates bracket from rounds. For each round, series are created.
        /// </summary>
        /// <param name="rounds">Number of rounds of the bracket.</param>
        private void CreateBracket(int rounds)
        {
            Series = new ObservableCollection<List<Serie>>();
            int pow = 1;
            for (int i = 0; i < rounds; i++)
            {
                List<Serie> round = new();
                for (int j = 0; j < pow; j++)
                {
                    Serie s = new();
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

        /// <summary>
        /// Returns the position of the serie in current bracket.
        /// </summary>
        /// <param name="s">Instance of the serie.</param>
        /// <returns>Tuple of round index and serie index in that round, in that order.
        /// If bracket does not contain given serie, returns (-1, -1).</returns>
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
        /// <summary>
        /// Checks wheter there is a competitor in the following serie or not.
        /// </summary>
        /// <param name="round">Current round index.</param>
        /// <param name="index">Index of the serie in the round.</param>
        /// <returns>True if there is a competitor in the following serie.</returns>
        public bool IsThereNextCompetitor(int round, int index)
        {
            if (round < Series.Count - 1)
            {
                if (index % 2 == 0)
                {
                    if (Series[round + 1][index / 2].FirstTeam.ID == SportsData.NOID)
                    {
                        return false;
                    }
                }
                else
                {
                    if (Series[round + 1][index / 2].SecondTeam.ID == SportsData.NOID)
                    {
                        return false;
                    }
                }
                return true;
            }
            return true;
        }

        /// <summary>
        /// Checks if the serie in provided round at provided index has a winner.
        /// </summary>
        /// <param name="round">Index of round of the serie.</param>
        /// <param name="index">Index of the serie in the round.</param>
        /// <returns>True if there is a winner.</returns>
        public bool HasWinner(int round, int index)
        {
            if (round < Series.Count)
            {
                //if has winner
                if (Series[round][index].Winner.ID != SportsData.NOID)
                {
                    return true;
                }
                //if there are not winners before
                if (round != 0 && (Series[round - 1][index * 2].Winner.ID == SportsData.NOID || Series[round - 1][(index * 2) + 1].Winner.ID == SportsData.NOID))
                {
                    return false;
                }
                if (Series[round][index].FirstTeam.ID != SportsData.NOID && Series[round][index].SecondTeam.ID == SportsData.NOID)
                {
                    Series[round][index].Winner = Series[round][index].FirstTeam;
                }
                if (Series[round][index].FirstTeam.ID == SportsData.NOID && Series[round][index].SecondTeam.ID != SportsData.NOID)
                {
                    Series[round][index].Winner = Series[round][index].SecondTeam;
                }

                return Series[round][index].Winner.ID != SportsData.NOID;
            }
            return false;
        }

        /// <summary>
        /// Seeds winners of the series into the following ones.
        /// </summary>
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
                            Series[r + 1][i / 2].FirstTeam = Series[r][i].Winner;
                        }
                        else
                        {
                            Series[r + 1][i / 2].SecondTeam = Series[r][i].Winner;
                        }
                    }

                    if (Series[r][i].Winner.ID == SportsData.NOID && Series[r][i].FirstTeam.ID != SportsData.NOID && Series[r][i].SecondTeam.ID != SportsData.NOID)
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

            if (Series[round][index].Matches.Any(x => x.Played))
            {
                CollapseAllRemoveButton(round, index);
            }
            else if (Series[round][index].Matches.Count > 0)
            {
                if (Series[round][index].Matches[0].HomeTeam.ID == SportsData.NOID)
                {
                    Series[round][index].RemoveFirstTeamVisibility = Visibility.Collapsed;
                    CollapseRemoveButton(round - 1, index * 2);
                    CollapseRemoveButton(round - 1, (index * 2) + 1);
                }
                else
                {
                    CollapseAllRemoveButton(round - 1, index * 2);
                }

                if (Series[round][index].Matches[0].AwayTeam.ID == SportsData.NOID)
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

        /// <summary>
        /// Resets the advancing team of the serie after serie modification.
        /// </summary>
        /// <param name="round">Index of the round of the serie.</param>
        /// <param name="index">Index of the serie in the round.</param>
        /// <param name="position">1 for first team or 2 for second team of the serie.</param>
        public void ResetSeriesAdvanced(int round, int index, int position)
        {
            if (round == Series.Count) { return; }

            if (position == 1)
            {
                Series[round][index].FirstTeam = new Team();
            }
            else
            {
                Series[round][index].SecondTeam = new Team();
            }
            Series[round][index].Winner = new Team();
            Series[round][index].RemoveFirstTeamVisibility = Visibility.Visible;
            Series[round][index].RemoveSecondTeamVisibility = Visibility.Visible;
            Series[round][index].AddMatchVisibility = Visibility.Collapsed;

            int newPosition = 2;
            if (index % 2 == 0) { newPosition = 1; }
            ResetSeriesAdvanced(round + 1, index / 2, newPosition);
        }
        #endregion

        #region Setting teams
        /// <summary>
        /// Recursively checks bracket branches from the provided position and locks or unlocks them for modification after insertion or deletion of a team to the provided position.
        /// </summary>
        /// <param name="round">Index of the round of the serie.</param>
        /// <param name="index">Index of the serie in the round.</param>
        /// <param name="position">1 for first team or 2 for second team of the serie.</param>
        /// <param name="lockChange">1 if team was inserted or -1 if team was removed.</param>
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
