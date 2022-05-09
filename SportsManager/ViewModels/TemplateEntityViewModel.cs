using SportsManager.Commands;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SportsManager.ViewModels
{
    /// <summary>
    /// Template viewmodel an entity detail.
    /// </summary>
    public class TemplateEntityViewModel : NotifyPropertyChanged
    {
        private BitmapImage bitmap;
        /// <summary>
        /// Bitmap of current logo or photo image of the entity.
        /// </summary>
        public BitmapImage Bitmap
        {
            get => bitmap;
            set
            {
                bitmap = value;
                OnPropertyChanged();
            }
        }

        private ICommand deleteCommand;
        /// <summary>
        /// Command that deletes current entity from database after executing it.
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new RelayCommand(param => Delete());
                }
                return deleteCommand;
            }
        }

        /// <summary>
        /// Command that navigates to viewmodel for editing the entity after executing it.
        /// </summary>
        public ICommand NavigateEditCommand { get; set; }

        /// <summary>
        /// Command that navigates to the previous viewmodel after executing it when the entity is deleted.
        /// </summary>
        public ICommand NavigateBackCommand { get; set; }

        /// <summary>
        /// Virtual method for deleting the entity from database.
        /// </summary>
        protected virtual void Delete() { }
    }
}