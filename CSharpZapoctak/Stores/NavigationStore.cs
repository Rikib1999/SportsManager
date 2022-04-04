using CSharpZapoctak.ViewModels;
using System;

namespace CSharpZapoctak.Stores
{
    public class NavigationStore
    {
        public event Action CurrentViewModelChanged;

        private NotifyPropertyChanged currentViewModel;

        public NotifyPropertyChanged CurrentViewModel
        {
            get => currentViewModel;
            set
            {
                currentViewModel = value;
                OnCurrentViewModelChanged();
            }
        }

        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }
}