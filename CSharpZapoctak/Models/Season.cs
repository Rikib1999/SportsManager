using System;

namespace CSharpZapoctak.Models
{
    public class Season : Competition
    {
        private Competition competition = null;
        public Competition Competition
        {
            get { return competition; }
            set
            {
                competition = value; OnPropertyChanged();
            }
        }

        private IStats stats;
        public IStats Stats
        {
            get { return stats; }
            set
            {
                stats = value;
                OnPropertyChanged();
            }
        }

        private int qualificationCount;
        public int QualificationCount
        {
            get { return qualificationCount; }
            set
            {
                qualificationCount = value;
                OnPropertyChanged();
            }
        }

        private int qualificationRounds;
        public int QualificationRounds
        {
            get { return qualificationRounds; }
            set
            {
                qualificationRounds = value;
                OnPropertyChanged();
            }
        }

        private int groupCount;
        public int GroupCount
        {
            get { return groupCount; }
            set
            {
                groupCount = value;
                OnPropertyChanged();
            }
        }

        private int playOffRounds;
        public int PlayOffRounds
        {
            get { return playOffRounds; }
            set
            {
                playOffRounds = value;
                OnPropertyChanged();
            }
        }

        private int playOffBestOf;
        public int PlayOffBestOf
        {
            get { return playOffBestOf; }
            set
            {
                playOffBestOf = value;
                OnPropertyChanged();
            }
        }

        private string winner;
        public string Winner
        {
            get { return winner; }
            set
            {
                winner = value;
                OnPropertyChanged();
            }
        }

        private int? pointsForWin;
        public int? PointsForWin
        {
            get { return pointsForWin; }
            set
            {
                pointsForWin = value;
                OnPropertyChanged();
            }
        }

        private int? pointsForOTWin;
        public int? PointsForOTWin
        {
            get { return pointsForOTWin; }
            set
            {
                pointsForOTWin = value;
                OnPropertyChanged();
            }
        }

        private int? pointsForTie;
        public int? PointsForTie
        {
            get { return pointsForTie; }
            set
            {
                pointsForTie = value;
                OnPropertyChanged();
            }
        }

        private int? pointsForLoss;
        public int? PointsForLoss
        {
            get { return pointsForLoss; }
            set
            {
                pointsForLoss = value;
                OnPropertyChanged();
            }
        }

        private int? pointsForOTLoss;
        public int? PointsForOTLoss
        {
            get { return pointsForOTLoss; }
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