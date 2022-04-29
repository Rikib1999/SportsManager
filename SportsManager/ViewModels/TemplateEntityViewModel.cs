using SportsManager.Commands;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SportsManager.ViewModels
{
    public class TemplateEntityViewModel : NotifyPropertyChanged
    {
        private BitmapImage bitmap;
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

        public ICommand NavigateEditCommand { get; set; }

        public ICommand NavigateBackCommand { get; set; }

        protected virtual void Delete() { }
    }
}