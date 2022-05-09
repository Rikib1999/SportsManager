using System.ComponentModel;

namespace SportsManager.ViewModels
{
    /// <summary>
    /// Implementation of the INotifyPropertyChanged interface.
    /// </summary>
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        /// <summary>
        /// Event for handling the property change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies that the property has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}