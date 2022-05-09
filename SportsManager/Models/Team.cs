using System;

namespace SportsManager.Models
{
    /// <summary>
    /// Class for representing a team object.
    /// </summary>
    public class Team : Competition
    {
        private IStats stats;
        /// <summary>
        /// Statistics of the team represented in an object.
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

        private bool status = true;
        /// <summary>
        /// True if the team is active and false if not.
        /// </summary>
        public bool Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Returns teams status. For true returns "active" and for false returns "inactive".
        /// </summary>
        public string StatusText => Status ? "active" : "inactive";

        /// <summary>
        /// Instance of a country from which the team originates.
        /// </summary>
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

        /// <summary>
        /// Date of the creation of the team.
        /// </summary>
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

        /// <summary>
        /// True if team is already saved in database, otherwise false.
        /// </summary>
        public bool SavedInDatabase { get; set; } = true;

        /// <summary>
        /// Creates new default instance of a team.
        /// </summary>
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

        /// <summary>
        /// Creates a deep copy of a team.
        /// </summary>
        /// <param name="t">Team instance to copy.</param>
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
