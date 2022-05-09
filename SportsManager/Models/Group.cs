using SportsManager.ViewModels;
using System.Collections.ObjectModel;

namespace SportsManager.Models
{
    /// <summary>
    /// Class for representing a group of teams in the group stage of the season.
    /// </summary>
    public class Group : NotifyPropertyChanged
    {
        /// <summary>
        /// Identification number of the group.
        /// </summary>
        public int ID { get; set; } = SportsData.NOID;

        private string name = "";
        /// <summary>
        /// Name of the group.
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

        private int seasonID;
        /// <summary>
        /// Identification number of the season in which the group is.
        /// </summary>
        public int SeasonID
        {
            get => seasonID;
            set
            {
                seasonID = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Team> teams;
        /// <summary>
        /// Collection of all teams that belongs to this group.
        /// </summary>
        public ObservableCollection<Team> Teams
        {
            get => teams;
            set
            {
                teams = value;
                OnPropertyChanged();
            }
        }
    }
}