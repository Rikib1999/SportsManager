using CSharpZapoctak.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace CSharpZapoctak.Models
{
    class Serie : ViewModelBase
    {
        private Team firstTeam;
        public Team FirstTeam
        {
            get { return firstTeam; }
            set
            {
                firstTeam = value;
                OnPropertyChanged();
            }
        }

        private Team secondTeam;
        public Team SecondTeam
        {
            get { return secondTeam; }
            set
            {
                secondTeam = value;
                OnPropertyChanged();
            }
        }

        private Team firstSelectedTeam;
        public Team FirstSelectedTeam
        {
            get { return firstSelectedTeam; }
            set
            {
                firstSelectedTeam = value;
                OnPropertyChanged();
            }
        }

        private Team secondSelectedTeam;
        public Team SecondSelectedTeam
        {
            get { return secondSelectedTeam; }
            set
            {
                secondSelectedTeam = value;
                OnPropertyChanged();
            }
        }

        private bool firstIsEnabled = true;
        public bool FirstIsEnabled
        {
            get { return firstIsEnabled; }
            set
            {
                firstIsEnabled = value;
                OnPropertyChanged();
            }
        }

        private int firstLock;
        public int FirstLock
        {
            get { return firstLock; }
            set
            {
                firstLock = value;
                if (firstLock == 0)
                {
                    FirstIsEnabled = true;
                }
                else
                {
                    FirstIsEnabled = false;
                }
            }
        }

        private Visibility firstSelectedVisibility = Visibility.Collapsed;
        public Visibility FirstSelectedVisibility
        {
            get { return firstSelectedVisibility; }
            set
            {
                firstSelectedVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility firstNotSelectedVisibility = Visibility.Visible;
        public Visibility FirstNotSelectedVisibility
        {
            get { return firstNotSelectedVisibility; }
            set
            {
                firstNotSelectedVisibility = value;
                OnPropertyChanged();
            }
        }

        private bool secondIsEnabled = true;
        public bool SecondIsEnabled
        {
            get { return secondIsEnabled; }
            set
            {
                secondIsEnabled = value;
                OnPropertyChanged();
            }
        }

        private int secondLock;
        public int SecondLock
        {
            get { return secondLock; }
            set
            {
                secondLock = value;
                if (secondLock == 0)
                {
                    SecondIsEnabled = true;
                }
                else
                {
                    SecondIsEnabled = false;
                }
            }
        }

        private Visibility secondSelectedVisibility = Visibility.Collapsed;
        public Visibility SecondSelectedVisibility
        {
            get { return secondSelectedVisibility; }
            set
            {
                secondSelectedVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility secondNotSelectedVisibility = Visibility.Visible;
        public Visibility SecondNotSelectedVisibility
        {
            get { return secondNotSelectedVisibility; }
            set
            {
                secondNotSelectedVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility preLineVisibility = Visibility.Visible;
        public Visibility PreLineVisibility
        {
            get { return preLineVisibility; }
            set
            {
                preLineVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility postLineVisibility = Visibility.Visible;
        public Visibility PostLineVisibility
        {
            get { return postLineVisibility; }
            set
            {
                postLineVisibility = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<List<Match>> matches;
        public ObservableCollection<List<Match>> Matches
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
