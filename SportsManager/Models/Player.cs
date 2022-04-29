using SportsManager.Others;
using SportsManager.ViewModels;
using System;

namespace SportsManager.Models
{
    public class Player : NotifyPropertyChanged, IHasImage, IEntity
    {
        public int ID { get; set; } = SportsData.NOID;

        private string firstName = "";
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
        public string LastName
        {
            get => lastName;
            set
            {
                lastName = value;
                OnPropertyChanged();
            }
        }

        public string FullName => FirstName + " " + LastName;

        private DateTime birthdate = DateTime.Now;
        public DateTime Birthdate
        {
            get => birthdate;
            set
            {
                birthdate = value;
                OnPropertyChanged();
            }
        }

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

        //in centimeters
        private int height;
        public int Height
        {
            get => height;
            set
            {
                height = value;
                OnPropertyChanged();
            }
        }

        //in kilograms
        private int weight;
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

        private string info;
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
