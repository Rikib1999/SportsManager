using CSharpZapoctak.ViewModels;

namespace CSharpZapoctak.Models
{
    class Match : ViewModelBase
    {
        public int id = (int)EntityState.NotSelected;

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
    }
}
