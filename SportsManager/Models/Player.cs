using SportsManager.Others;
using SportsManager.ViewModels;
using System;

namespace SportsManager.Models
{
    /// <summary>
    /// Class for representing a player.
    /// </summary>
    public class Player : NotifyPropertyChanged, IHasImage, IEntity
    {
        /// <summary>
        /// Identification number of the player.
        /// </summary>
        public int ID { get; set; } = SportsData.NOID;

        private string firstName = "";
        /// <summary>
        /// First name of the player.
        /// </summary>
        public string FirstName
        {
            get => firstName;
            set
            {
                firstName = value;
                OnPropertyChanged();
            }
        }

        private string lastName = "";
        /// <summary>
        /// Last name of the player.
        /// </summary>
        public string LastName
        {
            get => lastName;
            set
            {
                lastName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Returns full name of the player (first name last name).
        /// </summary>
        public string FullName => FirstName + " " + LastName;

        private DateTime birthdate = DateTime.Now;
        /// <summary>
        /// Birthdate of the player.
        /// </summary>
        public DateTime Birthdate
        {
            get => birthdate;
            set
            {
                birthdate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gender of the player. M for males and F for females.
        /// </summary>
        private string gender;
        public string Gender
        {
            get => gender;
            set
            {
                gender = value;
                OnPropertyChanged();
            }
        }

        private int height;
        /// <summary>
        /// Height of the player in centimeters.
        /// </summary>
        public int Height
        {
            get => height;
            set
            {
                height = value;
                OnPropertyChanged();
            }
        }

        private int weight;
        /// <summary>
        /// Weight of the player in kilograms.
        /// </summary>
        public int Weight
        {
            get => weight;
            set
            {
                weight = value;
                OnPropertyChanged();
            }
        }

        private string playsWith;
        /// <summary>
        /// Side the player plays with. R for right-handed or L for left-handed.
        /// </summary>
        public string PlaysWith
        {
            get => playsWith;
            set
            {
                playsWith = value;
                OnPropertyChanged();
            }
        }

        private Country citizenship;
        /// <summary>
        /// Instance of the country object of which the player is citizen.
        /// </summary>
        public Country Citizenship
        {
            get => citizenship;
            set
            {
                citizenship = value;
                OnPropertyChanged();
            }
        }

        private string birthplaceCity;
        /// <summary>
        /// Name of the birthplace city of the player.
        /// </summary>
        public string BirthplaceCity
        {
            get => birthplaceCity;
            set
            {
                birthplaceCity = value;
                OnPropertyChanged();
            }
        }

        private Country birthplaceCountry;
        /// <summary>
        /// Instance of the country object in which the player was born.
        /// </summary>
        public Country BirthplaceCountry
        {
            get => birthplaceCountry;
            set
            {
                birthplaceCountry = value;
                OnPropertyChanged();
            }
        }

        private bool status;
        /// <summary>
        /// True if the player is active and false if not.
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
        /// Returns players status. For true returns "active" and for false returns "inactive".
        /// </summary>
        public string StatusText => Status ? "active" : "inactive";

        private string info;
        /// <summary>
        /// Additional information about the player.
        /// </summary>
        public string Info
        {
            get => info;
            set
            {
                info = value;
                OnPropertyChanged();
            }
        }

        private string imagePath;
        /// <summary>
        /// Local path to the image of the players photo.
        /// </summary>
        public string ImagePath
        {
            get => imagePath;
            set
            {
                imagePath = value;
                OnPropertyChanged();
            }
        }

        private IStats stats;
        /// <summary>
        /// Statistics of the player represented in an object.
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
    }
}
