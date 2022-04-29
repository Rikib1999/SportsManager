using System;

namespace SportsManager.Models
{
    public class Team : Competition
    {
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

        private bool status = true;
        public bool Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        public string StatusText => Status ? "active" : "inactive";

        private Country country;
        public Country Country
        {
            get => country;
            set
            {
                country = value;
                OnPropertyChanged();
            }
        }

        private DateTime dateOfCreation = DateTime.Today;
        public DateTime DateOfCreation
        {
            get => dateOfCreation;
            set
            {
                dateOfCreation = value;
                OnPropertyChanged();
            }
        }

        public bool SavedInDatabase { get; set; } = true;

        public Team()
        {
            ID = SportsData.NOID;
            Name = "";
            Info = "";
            ImagePath = "";
            Status = true;
            Country = null;
            DateOfCreation = DateTime.Today;
        }

        public Team(Team t)
        {
            ID = t.ID;
            Name = t.Name;
            Info = t.Info;
            ImagePath = t.ImagePath;
            Status = t.Status;
            Country = t.Country;
            DateOfCreation = t.DateOfCreation;
        }
    }
}
