using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class ScheduleViewModel : ViewModelBase
    {
        private NavigationStore ns;

        public ViewModelBase CurrentViewModel { get; set; }

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
                    //CurrentViewModel = new PlayOffScheduleViewModel(navigationStore);
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

            if (SportsData.season.HasQualification())
            {
                QualificationVisibility = Visibility.Visible;
                QualificationSet = true;
                CurrentViewModel = new QualificationScheduleViewModel(navigationStore);
            }
            if (SportsData.season.HasGroups())
            {
                GroupsVisibility = Visibility.Visible;
                if (!QualificationSet) { GroupsSet = true; }
                CurrentViewModel = new GroupsScheduleViewModel(navigationStore);
            }
            if (SportsData.season.HasPlayOff())
            {
                PlayOffVisibility = Visibility.Visible;
                if (!QualificationSet && !GroupsSet) { PlayOffSet = true; }
                //CurrentViewModel = new PlayOffScheduleViewModel(navigationStore);
            }
        }
    }
}