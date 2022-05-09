using System;

namespace SportsManager.Models
{
    /// <summary>
    /// Class for representing a season entity.
    /// </summary>
    public class Season : Competition
    {
        private Competition competition;
        /// <summary>
        /// Instance of the competition for which this season is for.
        /// </summary>
        public Competition Competition
        {
            get => competition;
            set
            {
                competition = value;
                OnPropertyChanged();
            }
        }

        private IStats stats;
        /// <summary>
        /// Statistics of the season represented in an object.
        /// </summary>
        public IStats Stats
        {
            get => stats;
            set
            {
                stats = value;
                OnPropertyChanged();
            }
        }

        private int qualificationCount;
        /// <summary>
        /// Number of qualification brackets in the season.
        /// </summary>
        public int QualificationCount
        {
            get => qualificationCount;
            set
            {
                qualificationCount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Returns the seasons format.
        /// </summary>
        public string Format => GetFormat();

        private int qualificationRounds;
        /// <summary>
        /// Number of rounds of all qualification brackets of the season.
        /// </summary>
        public int QualificationRounds
        {
            get => qualificationRounds;
            set
            {
                qualificationRounds = value;
                OnPropertyChanged();
            }
        }

        private int groupCount;
        /// <summary>
        /// Number of groups in the group stage of the season.
        /// </summary>
        public int GroupCount
        {
            get => groupCount;
            set
            {
                groupCount = value;
                OnPropertyChanged();
            }
        }

        private int playOffRounds;
        /// <summary>
        /// Number of rounds in the play-off bracket.
        /// </summary>
        public int PlayOffRounds
        {
            get => playOffRounds;
            set
            {
                playOffRounds = value;
                OnPropertyChanged();
            }
        }

        private int playOffBestOf;
        /// <summary>
        /// Maximal number of matches played in a play-off serie of the season.
        /// </summary>
        public int PlayOffBestOf
        {
            get => playOffBestOf;
            set
            {
                playOffBestOf = value;
                OnPropertyChanged();
            }
        }

        private bool playOffStarted;
        /// <summary>
        /// Returns true if the play-off has already started, otherwise false.
        /// </summary>
        public bool PlayOffStarted
        {
            get => playOffStarted;
            set
            {
                playOffStarted = value;
                OnPropertyChanged();
            }
        }

        private string winnerName;
        /// <summary>
        /// Name of the winner of the season.
        /// </summary>
        public string WinnerName
        {
            get => winnerName;
            set
            {
                winnerName = value;
                OnPropertyChanged();
            }
        }

        private int winnerID;
        /// <summary>
        /// Identification number of the winner of the season.
        /// </summary>
        public int WinnerID
        {
            get => winnerID;
            set
            {
                winnerID = value;
                OnPropertyChanged();
            }
        }

        private int? pointsForWin;
        /// <summary>
        /// Number of points a team recieves for a win in regular time in the group stage of the season.
        /// </summary>
        public int? PointsForWin
        {
            get => pointsForWin;
            set
            {
                pointsForWin = value;
                OnPropertyChanged();
            }
        }

        private int? pointsForOTWin;
        /// <summary>
        /// Number of points a team recieves for an overtime win in the group stage of the season.
        /// </summary>
        public int? PointsForOTWin
        {
            get => pointsForOTWin;
            set
            {
                pointsForOTWin = value;
                OnPropertyChanged();
            }
        }

        private int? pointsForTie;
        /// <summary>
        /// Number of points a team recieves for a tie in the group stage of the season.
        /// </summary>
        public int? PointsForTie
        {
            get => pointsForTie;
            set
            {
                pointsForTie = value;
                OnPropertyChanged();
            }
        }

        private int? pointsForLoss;
        /// <summary>
        /// Number of points a team recieves for a loss in regular time in the group stage of the season.
        /// </summary>
        public int? PointsForLoss
        {
            get => pointsForLoss;
            set
            {
                pointsForLoss = value;
                OnPropertyChanged();
            }
        }

        private int? pointsForOTLoss;
        /// <summary>
        /// Number of points a team recieves for an overtime loss in the group stage of the season.
        /// </summary>
        public int? PointsForOTLoss
        {
            get => pointsForOTLoss;
            set
            {
                pointsForOTLoss = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Returns the number of maximal number of competitors in the given round.
        /// </summary>
        /// <param name="rounds">Index of a round.</param>
        /// <returns>The number of maximal number of competitors in the given round.</returns>
        public static int RoundOf(int rounds)
        {
            return (int)Math.Pow(2, rounds - 1);
        }

        /// <summary>
        /// Returns the formatted season format.
        /// </summary>
        /// <returns>Name of the season format.</returns>
        public string GetFormat()
        {
            string format = "";

            if (QualificationCount > 0)
            {
                format += QualificationCount + "x Qualification (round of " + RoundOf(QualificationRounds) + ")";
                if (PlayOffRounds > 0)
                {
                    format += " + ";
                }
            }
            if (GroupCount > 0)
            {
                format += GroupCount + " Group";
                if (GroupCount > 1)
                {
                    format += "s";
                }
                if (PlayOffRounds > 0)
                {
                    format += " + ";
                }
            }
            if (PlayOffRounds > 0)
            {
                format += "Play-off (round of " + RoundOf(PlayOffRounds) + ", best of " + PlayOffBestOf + ")";
            }

            return format;
        }

        /// <summary>
        /// Checks if the season has a qualification part.
        /// </summary>
        /// <returns>True if season has qualififcation part, otherwise false.</returns>
        public bool HasQualification()
        {
            return QualificationCount > 0;
        }

        /// <summary>
        /// Checks if the season has a group stage.
        /// </summary>
        /// <returns>True if season has group stage, otherwise false.</returns>
        public bool HasGroups()
        {
            return GroupCount > 0;
        }

        /// <summary>
        /// Checks if the season has a play-off.
        /// </summary>
        /// <returns>True if season has play-off, otherwise false.</returns>
        public bool HasPlayOff()
        {
            return PlayOffRounds > 0;
        }
    }
}