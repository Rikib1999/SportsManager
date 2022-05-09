using SportsManager.Stores;

namespace SportsManager.ViewModels
{
    /// <summary>
    /// Viewmodel for the main window.
    /// </summary>
    public class MainViewModel : NotifyPropertyChanged
    {
        private readonly NavigationStore _navigationStore;

        /// <summary>
        /// The instance of current viewmodel.
        /// </summary>
        public NotifyPropertyChanged CurrentViewModel => _navigationStore.CurrentViewModel;

        /// <summary>
        /// Instantiates new MainViewModel.
        /// </summary>
        /// <param name="navigationStore">Current instance of NavigationStore.</param>
        public MainViewModel(NavigationStore navigationStore)
        {
            _navigationStore = navigationStore;

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        /// <summary>
        /// Notifies on change of current viewmodel.
        /// </summary>
        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}