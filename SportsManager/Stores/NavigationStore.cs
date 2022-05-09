using SportsManager.ViewModels;
using System;

namespace SportsManager.Stores
{
    /// <summary>
    /// Class for storing and updating the current viewmodel.
    /// </summary>
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