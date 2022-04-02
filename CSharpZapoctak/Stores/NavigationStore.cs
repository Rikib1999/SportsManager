using CSharpZapoctak.ViewModels;
using System;

namespace CSharpZapoctak.Stores
{
    public class NavigationStore
    {
        public event Action CurrentViewModelChanged;

        private NotifyPropertyChanged _currentViewModel;

        public NotifyPropertyChanged CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnCurrentViewModelChanged();
            }
        }

        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }
}
