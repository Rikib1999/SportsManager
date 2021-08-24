using CSharpZapoctak.ViewModels;

namespace CSharpZapoctak.Models
{
    public class PenaltyType : ViewModelBase
    {
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

        private string code;
        public string Code
        {
            get { return code; }
            set
            {
                code = value;
                OnPropertyChanged();
            }
        }

        private int minutes;
        public int Minutes
        {
            get { return minutes; }
            set
            {
                minutes = value;
                OnPropertyChanged();
            }
        }

        public string FullName
        {
            get { return Name + " (" + Code + ")"; }
        }
    }
}