using CSharpZapoctak.Stores;
using System.Windows;

namespace CSharpZapoctak.ViewModels
{
    class ScheduleViewModel : NotifyPropertyChanged
    {
        private NavigationStore ns;

        public NotifyPropertyChanged CurrentViewModel { get; set; }

        private bool qualififcationSet = false;
        public bool QualificationSet
        {
            get { return qualififcationSet; }
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

        private bool groupsSet = false;
        public bool GroupsSet
        {
            get { return groupsSet; }
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

        private bool playOffSet = false;
        public bool PlayOffSet
        {
            get { return playOffSet; }
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
            get { return qualificationVisibility; }
            set
            {
                qualificationVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility groupsVisibility = Visibility.Collapsed;
        public Visibility GroupsVisibility
        {
            get { return groupsVisibility; }
            set
            {
                groupsVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility playOffVisibility = Visibility.Collapsed;
        public Visibility PlayOffVisibility
        {
            get { return playOffVisibility; }
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