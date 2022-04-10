using CSharpZapoctak.Stores;
using System.Windows;

namespace CSharpZapoctak.ViewModels
{
    class ScheduleViewModel : NotifyPropertyChanged
    {
        private readonly NavigationStore ns;

        public NotifyPropertyChanged CurrentViewModel { get; set; }

        private bool qualififcationSet;
        public bool QualificationSet
        {
            get => qualififcationSet;
            set
            {
                qualififcationSet = value;
                if (value)
                {
                    GroupsSet = false;
                    PlayOffSet = false;
                    CurrentViewModel = new QualificationScheduleViewModel(ns);
                }
                OnPropertyChanged();
            }
        }

        private bool groupsSet;
        public bool GroupsSet
        {
            get => groupsSet;
            set
            {
                groupsSet = value;
                if (value)
                {
                    QualificationSet = false;
                    PlayOffSet = false;
                    CurrentViewModel = new GroupsScheduleViewModel(ns);
                }
                OnPropertyChanged();
            }
        }

        private bool playOffSet;
        public bool PlayOffSet
        {
            get => playOffSet;
            set
            {
                playOffSet = value;
                if (value)
                {
                    QualificationSet = false;
                    GroupsSet = false;
                    CurrentViewModel = new PlayOffScheduleViewModel(ns);
                }
                OnPropertyChanged();
            }
        }

        private Visibility qualificationVisibility = Visibility.Collapsed;
        public Visibility QualificationVisibility
        {
            get => qualificationVisibility;
            set
            {
                qualificationVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility groupsVisibility = Visibility.Collapsed;
        public Visibility GroupsVisibility
        {
            get => groupsVisibility;
            set
            {
                groupsVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility playOffVisibility = Visibility.Collapsed;
        public Visibility PlayOffVisibility
        {
            get => playOffVisibility;
            set
            {
                playOffVisibility = value;
                OnPropertyChanged();
            }
        }

        public ScheduleViewModel(NavigationStore navigationStore)
        {
            ns = navigationStore;

            if (SportsData.SEASON.HasQualification())
            {
                QualificationVisibility = Visibility.Visible;
                QualificationSet = true;
            }
            if (SportsData.SEASON.HasGroups())
            {
                GroupsVisibility = Visibility.Visible;
                GroupsSet = true;
            }
            if (SportsData.SEASON.HasPlayOff())
            {
                PlayOffVisibility = Visibility.Visible;
                if (!QualificationSet && !GroupsSet)
                {
                    PlayOffSet = true;
                }
            }
        }
    }
}