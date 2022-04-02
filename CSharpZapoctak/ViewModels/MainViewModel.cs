using CSharpZapoctak.Stores;

namespace CSharpZapoctak.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private readonly NavigationStore _navigationStore;

        public NotifyPropertyChanged CurrentViewModel => _navigationStore.CurrentViewModel;

        public MainViewModel(NavigationStore navigationStore)
        {
            _navigationStore = navigationStore;

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
