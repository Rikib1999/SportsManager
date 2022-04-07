using System;

namespace CSharpZapoctak.Models
{
    public class Season : Competition
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

        private IStats stats;
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
        public int QualificationCount
        {
            get => qualificationCount;
            set
            {
                qualificationCount = value;
                OnPropertyChanged();
            }
        }

        private int qualificationRounds;
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
        public int? PointsForOTLoss
        {
            get => pointsForOTLoss;
            set
            {
                pointsForOTLoss = value;
                OnPropertyChanged();
            }
        }

        public int RoundOf(int rounds)
        {
            return (int)Math.Pow(2, rounds - 1);
        }

        public string Format()
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

        public bool HasQualification()
        {
            return QualificationCount > 0;
        }

        public bool HasGroups()
        {
            return GroupCount > 0;
        }

        public bool HasPlayOff()
        {
            return PlayOffRounds > 0;
        }
    }
}