using CSharpZapoctak.ViewModels;

namespace CSharpZapoctak.Models
{
    public class Competition : ViewModelBase
    {
        public int id = (int)EntityState.NotSelected;

        private string name = "";

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string info;

        public string Info
        {
            get { return info; }
            set
            {
                info = value;
                OnPropertyChanged();
            }
        }

        private string logoPath;

        public string LogoPath
        {
            get { return logoPath; }
            set
            {
                logoPath = value;
                OnPropertyChanged();
            }
        }
    }
}
