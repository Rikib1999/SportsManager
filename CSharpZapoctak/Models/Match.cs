using CSharpZapoctak.ViewModels;
using System;

namespace CSharpZapoctak.Models
{
    public class Match : NotifyPropertyChanged, IEntity
    {
        public int ID { get; set; } = SportsData.NOID;

        public int SerieNumber { get; set; }

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

        private Season season;
        public Season Season
        {
            get => season;
            set
            {
                season = value;
                OnPropertyChanged();
            }
        }

        private bool played;
        public bool Played
        {
            get => played;
            set
            {
                played = value;
                OnPropertyChanged();
            }
        }

        private int periods;
        public int Periods
        {
            get => periods;
            set
            {
                periods = value;
                OnPropertyChanged();
            }
        }

        private int periodDuration;
        public int PeriodDuration
        {
            get => periodDuration;
            set
            {
                periodDuration = value;
                OnPropertyChanged();
            }
        }

        private DateTime datetime;
        public DateTime Datetime
        {
            get => datetime;
            set
            {
                datetime = value;
                OnPropertyChanged();
            }
        }

        public string DatetimeToString => Datetime.ToString("g");

        private Team homeTeam;
        public Team HomeTeam
        {
            get => homeTeam;
            set
            {
                homeTeam = value;
                OnPropertyChanged();
            }
        }

        private Team awayTeam;
        public Team AwayTeam
        {
            get => awayTeam;
            set
            {
                awayTeam = value;
                OnPropertyChanged();
            }
        }

        private int homeScore;
        public int HomeScore
        {
            get => homeScore;
            set
            {
                homeScore = value;
                OnPropertyChanged();
            }
        }

        private int awayScore;
        public int AwayScore
        {
            get => awayScore;
            set
            {
                awayScore = value;
                OnPropertyChanged();
            }
        }

        private bool overtime;
        public bool Overtime
        {
            get => overtime;
            set
            {
                overtime = value;
                OnPropertyChanged();
            }
        }

        private bool shootout;
        public bool Shootout
        {
            get => shootout;
            set
            {
                shootout = value;
                OnPropertyChanged();
            }
        }

        private bool forfeit;
        public bool Forfeit
        {
            get => forfeit;
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
            return Overtime ? " ot" : Shootout ? " so" : Forfeit ? " ff" : "";
        }
    }
}