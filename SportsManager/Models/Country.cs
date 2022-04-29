using SportsManager.ViewModels;

namespace SportsManager.Models
{
    public class Country : NotifyPropertyChanged
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

        private string codeTwo;
        public string CodeTwo
        {
            get => codeTwo;
            set
            {
                codeTwo = value;
                OnPropertyChanged();
            }
        }

        private string codeThree;
        public string CodeThree
        {
            get => codeThree;
            set
            {
                codeThree = value;
                OnPropertyChanged();
            }
        }
    }
}
