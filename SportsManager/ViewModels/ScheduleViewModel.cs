using SportsManager.Stores;
using System.Windows;

namespace SportsManager.ViewModels
{
    /// <summary>
    /// Viewmodel for seasons schedule containing viewmodels for qualification, group stage and play-off.
    /// </summary>
    class ScheduleViewModel : NotifyPropertyChanged
    {
        /// <summary>
        /// Current instance of the NavigationStore.
        /// </summary>
        private readonly NavigationStore ns;

        /// <summary>
        /// Currently selected viewmodel of the schedule for specific part of the season.
        /// </summary>
        public NotifyPropertyChanged CurrentViewModel { get; set; }

        private bool qualififcationSet;
        /// <summary>
        /// Sets the qualification schedule viewmodel. Returns true if qualification is selected.
        /// </summary>
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
        /// <summary>
        /// Sets the group stage schedule viewmodel. Returns true if group stage is selected.
        /// </summary>
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
        /// <summary>
        /// Sets the play-off schedule viewmodel. Returns true if play-off is selected.
        /// </summary>
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
        /// <summary>
        /// Qualification radio button visibility.
        /// </summary>
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
        /// <summary>
        /// Group stage radio button visibility.
        /// </summary>
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
        /// <summary>
        /// Play-off radio button visibility.
        /// </summary>
        public Visibility PlayOffVisibility
        {
            get => playOffVisibility;
            set
            {
                playOffVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Instantiates a new ScheduleViewModel.
        /// </summary>
        /// <param name="navigationStore">Current instance of the NavigationStore.</param>
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