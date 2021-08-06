using CSharpZapoctak.ViewModels;
using System;

namespace CSharpZapoctak.Models
{
    public class Player : ViewModelBase
    {
        public int id = (int)EntityState.NotSelected;

        private string firstName = "";
        public string FirstName
        {
            get { return firstName; }
            set
            {
                firstName = value;
                OnPropertyChanged();
            }
        }

        private string lastName = "";
        public string LastName
        {
            get { return lastName; }
            set
            {
                lastName = value;
                OnPropertyChanged();
            }
        }

        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }

        private DateTime birthdate = DateTime.Now;
        public DateTime Birthdate
        {
            get { return birthdate; }
            set
            {
                birthdate = value;
                OnPropertyChanged();
            }
        }

        private string gender;
        public string Gender
        {
            get { return gender; }
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
            get { return height; }
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
            get { return weight; }
            set
            {
                weight = value;
                OnPropertyChanged();
            }
        }

        private string playsWith;
        public string PlaysWith
        {
            get { return playsWith; }
            set
            {
                playsWith = value;
                OnPropertyChanged();
            }
        }

        private Country citizenship;
        public Country Citizenship
        {
            get { return citizenship; }
            set
            {
                citizenship = value;
                OnPropertyChanged();
            }
        }

        private string birthplaceCity;
        public string BirthplaceCity
        {
            get { return birthplaceCity; }
            set
            {
                birthplaceCity = value;
                OnPropertyChanged();
            }
        }

        private Country birthplaceCountry;
        public Country BirthplaceCountry
        {
            get { return birthplaceCountry; }
            set
            {
                birthplaceCountry = value;
                OnPropertyChanged();
            }
        }

        private bool status;
        public bool Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        private string info;
        public string Info
        {
            get { return info; }
            set
            {
                info = value;
                OnPropertyChanged();
            }
        }

        private string photoPath;
        public string PhotoPath
        {
            get { return photoPath; }
            set
            {
                photoPath = value;
                OnPropertyChanged();
            }
        }
    }
}
