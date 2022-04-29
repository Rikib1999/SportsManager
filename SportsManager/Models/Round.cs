using SportsManager.Commands;
using SportsManager.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SportsManager.Models
{
    public class Round : NotifyPropertyChanged
    {
        public int ID { get; set; } = SportsData.NOID;

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

        private void SetRoundVisibility()
        {
            RoundVisibility = RoundVisibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private ObservableCollection<Match> matches;

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
