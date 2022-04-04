using CSharpZapoctak.Commands;
using CSharpZapoctak.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.Models
{
    class Round : NotifyPropertyChanged
    {
        public int id = SportsData.NO_ID;

        private string name = "";
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private int seasonID;
        public int SeasonID
        {
            get { return seasonID; }
            set
            {
                seasonID = value;
                OnPropertyChanged();
            }
        }

        private Visibility roundVisibility = Visibility.Visible;
        public Visibility RoundVisibility
        {
            get { return roundVisibility; }
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
            if (RoundVisibility == Visibility.Visible)
            {
                RoundVisibility = Visibility.Collapsed;
            }
            else
            {
                RoundVisibility = Visibility.Visible;
            }
        }

        private ObservableCollection<Match> matches;

        public ObservableCollection<Match> Matches
        {
            get { return matches; }
            set
            {
                matches = value;
                OnPropertyChanged();
            }
        }
    }
}
