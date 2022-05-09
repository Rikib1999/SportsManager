using SportsManager.ViewModels;

namespace SportsManager.Models
{
    /// <summary>
    /// Class for representing a penalty type.
    /// </summary>
    public class PenaltyType : NotifyPropertyChanged
    {
        private string name = "";
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
        /// Code of the penalty type.
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

        private int minutes;
        /// <summary>
        /// Penalty minutes given to a player.
        /// </summary>
        public int Minutes
        {
            get => minutes;
            set
            {
                minutes = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Returns penalty name with its code in format "name (code)".
        /// </summary>
        public string FullName => Name + " (" + Code + ")";
    }
}