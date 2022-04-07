using CSharpZapoctak.ViewModels;

namespace CSharpZapoctak.Models
{
    public class PenaltyType : NotifyPropertyChanged
    {
        private string name = "";
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string code;
        public string Code
        {
            get => code;
            set
            {
                code = value;
                OnPropertyChanged();
            }
        }

        private int minutes;
        public int Minutes
        {
            get => minutes;
            set
            {
                minutes = value;
                OnPropertyChanged();
            }
        }

        public string FullName => Name + " (" + Code + ")";
    }
}