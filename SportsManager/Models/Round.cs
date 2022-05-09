using SportsManager.Commands;
using SportsManager.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SportsManager.Models
{
    /// <summary>
    /// Class for representing a round of matches in a group stage of a season.
    /// </summary>
    public class Round : NotifyPropertyChanged
    {
        /// <summary>
        /// Identification number of the round.
        /// </summary>
        public int ID { get; set; } = SportsData.NOID;

        /// <summary>
        /// Name of the round.
        /// </summary>
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

        private int seasonID;
        /// <summary>
        /// Identification number of the season that the round is part of.
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

        private Visibility roundVisibility = Visibility.Visible;
        /// <summary>
        /// Visibility of the round in the view.
        /// </summary>
        public Visibility RoundVisibility
        {
            get => roundVisibility;
            set
            {
                roundVisibility = value;
                OnPropertyChanged();
            }
        }

        private ICommand setRoundVisibilityCommand;
        /// <summary>
        /// Command for switching the round visibility.
        /// </summary>
        public ICommand SetRoundVisibilityCommand
        {
            get
            {
                if (setRoundVisibilityCommand == null)
                {
                    setRoundVisibilityCommand = new RelayCommand(param => SetRoundVisibility());
                }
                return setRoundVisibilityCommand;
            }
        }

        /// <summary>
        /// Switches the round visibility.
        /// </summary>
        private void SetRoundVisibility()
        {
            RoundVisibility = RoundVisibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private ObservableCollection<Match> matches;
        /// <summary>
        /// Collection of all matches played in the round.
        /// </summary>
        public ObservableCollection<Match> Matches
        {
            get => matches;
            set
            {
                matches = value;
                OnPropertyChanged();
            }
        }
    }
}
