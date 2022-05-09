using SportsManager.ViewModels;

namespace SportsManager.Models
{
    /// <summary>
    /// Class for representing a penalty reason.
    /// </summary>
    public class PenaltyReason : NotifyPropertyChanged
    {
        private string name = "";
        /// <summary>
        /// Name of the penalty reason.
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
        /// Code of the penalty reason.
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