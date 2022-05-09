using SportsManager.ViewModels;

namespace SportsManager.Models
{
    /// <summary>
    /// Class for representing a situation of strength of both teams.
    /// </summary>
    public class Strength : NotifyPropertyChanged
    {
        /// <summary>
        /// Identification number of strength situation.
        /// </summary>
        public int ID { get; set; }

        private string situation = "";
        /// <summary>
        /// Situation name, description. For example "5 v 4".
        /// </summary>
        public string Situation
        {
            get => situation;
            set
            {
                situation = value;
                OnPropertyChanged();
            }
        }

        private string advantage;
        /// <summary>
        /// Code of the advantage from current strength from the view of the first team. "EV" = equal strength, "PP" = power play, "SH" = short-handed.
        /// </summary>
        public string Advantage
        {
            get => advantage;
            set
            {
                advantage = value;
                OnPropertyChanged();
            }
        }
    }
}
