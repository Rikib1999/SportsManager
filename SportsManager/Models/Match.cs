using SportsManager.ViewModels;
using System;

namespace SportsManager.Models
{
    /// <summary>
    /// Class for representing a match with its basic details.
    /// </summary>
    public class Match : NotifyPropertyChanged, IEntity
    {
        /// <summary>
        /// Identification number of the match.
        /// </summary>
        public int ID { get; set; } = SportsData.NOID;

        /// <summary>
        /// Number of the match in the serie. In what order was it played.
        /// </summary>
        public int SerieNumber { get; set; }

        private IStats stats;
        /// <summary>
        /// Statistics of the match. For example number of goals etc.
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

        private Competition competition;
        /// <summary>
        /// Instance of the competition in which the current match was played.
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

        private Season season;
        /// <summary>
        /// Instance of the season in which the current match was played.
        /// </summary>
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
        /// <summary>
        /// If the match was already played or is only scheduled.
        /// </summary>
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
        /// <summary>
        /// Number of periods in the match.
        /// </summary>
        public int Periods
        {
            get => periods;
            set
            {
                periods = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Duration of each period of the match in minutes.
        /// </summary>
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
        /// <summary>
        /// Date and time of the start of the match.
        /// </summary>
        public DateTime Datetime
        {
            get => datetime;
            set
            {
                datetime = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Returns date and time of the start of the match in format "DD. MM. YYYY HH:mm".
        /// </summary>
        public string DatetimeToString => Datetime.ToString("g");

        private Team homeTeam;
        /// <summary>
        /// Instance of the home team.
        /// </summary>
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
        /// <summary>
        /// Instance of the away team.
        /// </summary>
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
        /// <summary>
        /// Home team score.
        /// </summary>
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
        /// <summary>
        /// Away team score.
        /// </summary>
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
        /// <summary>
        /// Wheter overtime was played or not.
        /// </summary>
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
        /// <summary>
        /// Wheter shootout was played or not.
        /// </summary>
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
        /// <summary>
        /// Wheter the match was forfeited or not.
        /// </summary>
        public bool Forfeit
        {
            get => forfeit;
            set
            {
                forfeit = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Returns the score of the match in format "(home score) : (away score) (overtime/shootout/forfeit)".
        /// </summary>
        public string Score
        {
            get
            {
                string score = HomeScore + " : " + AwayScore;
                if (Overtime) { score += " ot"; }
                if (Shootout) { score += " so"; }
                if (Forfeit) { score += " ff"; }
                return score;
            }
        }

        /// <summary>
        /// Returns the match overview in format "DD. MM. YYYY HH:mm (home team name) (home score) : (away score) (away team name) (overtime/shootout/forfeit)".
        /// </summary>
        /// <returns>Match overview in format "DD. MM. YYYY HH:mm (home team name) (home score) : (away score) (away team name) (overtime/shootout/forfeit)".</returns>
        public string Overview()
        {
            return Datetime.ToString("g") + " " + HomeTeam.Name + " " + Score + " " + AwayTeam.Name;
        }

        /// <summary>
        /// Returns the result type abbreviation.
        /// </summary>
        /// <returns>For overtime " ot", for shootout " so", for forfeit " ff" and empty string for normal result "".</returns>
        public string ResultType()
        {
            return Overtime ? " ot" : Shootout ? " so" : Forfeit ? " ff" : "";
        }
    }
}