using CSharpZapoctak.ViewModels;

namespace CSharpZapoctak.Models
{
    public class PenaltyReason : NotifyPropertyChanged
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
    }
}