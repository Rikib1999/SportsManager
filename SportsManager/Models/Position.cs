using SportsManager.ViewModels;

namespace SportsManager.Models
{
    /// <summary>
    /// Class for representing a players position. For example "left-wing", "goaltender" or "defenseman".
    /// </summary>
    public class Position : NotifyPropertyChanged
    {
        private string name = "";
        /// <summary>
        /// Name of the position.
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

        private string code;
        /// <summary>
        /// Abbreviation of the position.
        /// </summary>
        public string Code
        {
            get => code;
            set
            {
                code = value;
                OnPropertyChanged();
            }
        }
    }
}
