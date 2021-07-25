using CSharpZapoctak.ViewModels;

namespace CSharpZapoctak.Models
{
    public class Country : ViewModelBase
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

        private string codeTwo;

        public string CodeTwo
        {
            get { return codeTwo; }
            set
            {
                codeTwo = value;
                OnPropertyChanged();
            }
        }

        private string codeThree;

        public string CodeThree
        {
            get { return codeThree; }
            set
            {
                codeThree = value;
                OnPropertyChanged();
            }
        }
    }
}
