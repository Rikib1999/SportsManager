using SportsManager.ViewModels;

namespace SportsManager.Models
{
    /// <summary>
    /// Class for representing a country of a world.
    /// </summary>
    public class Country : NotifyPropertyChanged
    {
        private string name = "";
        /// <summary>
        /// Full name of the country in English.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string codeTwo;
        /// <summary>
        /// International abbreviation of the country of two characters.
        /// </summary>
        public string CodeTwo
        {
            get => codeTwo;
            set
            {
                codeTwo = value;
                OnPropertyChanged();
            }
        }

        private string codeThree;
        /// <summary>
        /// International abbreviation of the country of three characters.
        /// </summary>
        public string CodeThree
        {
            get => codeThree;
            set
            {
                codeThree = value;
                OnPropertyChanged();
            }
        }
    }
}
