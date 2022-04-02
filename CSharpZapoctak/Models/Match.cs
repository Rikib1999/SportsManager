using CSharpZapoctak.ViewModels;
using System;

namespace CSharpZapoctak.Models
{
    class Match : NotifyPropertyChanged
    {
        public int id = (int)EntityState.NotSelected;

        public int serieNumber;

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

        private Competition competition;
        public Competition Competition
        {
            get { return competition; }
            set
            {
                competition = value;
                OnPropertyChanged();
            }
        }

        private Season season;
        public Season Season
        {
            get { return season; }
            set
            {
                season = value;
                OnPropertyChanged();
            }
        }

        private bool played;
        public bool Played
        {
            get { return played; }
            set
            {
                played = value;
                OnPropertyChanged();
            }
        }

        private int periods;
        public int Periods
        {
            get { return periods; }
            set
            {
                periods = value;
                OnPropertyChanged();
            }
        }

        private int periodDuration;
        public int PeriodDuration
        {
            get { return periodDuration; }
            set
            {
                periodDuration = value;
                OnPropertyChanged();
            }
        }

        private DateTime datetime;
        public DateTime Datetime
        {
            get { return datetime; }
            set
            {
                datetime = value;
                OnPropertyChanged();
            }
        }

        public string DatetimeToString
        {
            get { return Datetime.ToString("g"); }
        }

        private Team homeTeam;
        public Team HomeTeam
        {
            get { return homeTeam; }
            set
            {
                homeTeam = value;
                OnPropertyChanged();
            }
        }

        private Team awayTeam;
        public Team AwayTeam
        {
            get { return awayTeam; }
            set
            {
                awayTeam = value;
                OnPropertyChanged();
            }
        }

        private int homeScore;
        public int HomeScore
        {
            get { return homeScore; }
            set
            {
                homeScore = value;
                OnPropertyChanged();
            }
        }

        private int awayScore;
        public int AwayScore
        {
            get { return awayScore; }
            set
            {
                awayScore = value;
                OnPropertyChanged();
            }
        }

        private bool overtime;
        public bool Overtime
        {
            get { return overtime; }
            set
            {
                overtime = value;
                OnPropertyChanged();
            }
        }

        private bool shootout;
        public bool Shootout
        {
            get { return shootout; }
            set
            {
                shootout = value;
                OnPropertyChanged();
            }
        }

        private bool forfeit;
        public bool Forfeit
        {
            get { return forfeit; }
            set
            {
                forfeit = value;
                OnPropertyChanged();
            }
        }

        public string Score()
        {
            string score = HomeScore + " : " + AwayScore;
            if (Overtime) { score += " ot"; }
            if (Shootout) { score += " so"; }
            if (Forfeit) { score += " ff"; }
            return score;
        }

        public string Overview()
        {
            return Datetime.ToString("g") + " " + HomeTeam.Name + " " + Score() + " " + AwayTeam.Name;
        }

        public string ResultType()
        {
            if (Overtime) { return " ot"; }
            if (Shootout) { return " so"; }
            if (Forfeit) { return " ff"; }
            return "";
        }
    }
}