using System;

namespace CSharpZapoctak.Models
{
    public class Team : Competition
    {
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

        private bool status = true;
        public bool Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        private Country country;
        public Country Country
        {
            get { return country; }
            set
            {
                country = value;
                OnPropertyChanged();
            }
        }

        private DateTime dateOfCreation = DateTime.Today;
        public DateTime DateOfCreation
        {
            get { return dateOfCreation; }
            set
            {
                dateOfCreation = value;
                OnPropertyChanged();
            }
        }

        public Team()
        {
            id = (int)EntityState.NotSelected;
            Name = "";
            Info = "";
            LogoPath = "";
            Status = true;
            Country = null;
            DateOfCreation = DateTime.Today;
        }
        public Team(Team t)
        {
            id = t.id;
            Name = t.Name;
            Info = t.Info;
            LogoPath = t.LogoPath;
            Status = t.Status;
            Country = t.Country;
            DateOfCreation = t.DateOfCreation;
        }
    }
}
