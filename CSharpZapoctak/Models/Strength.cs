using CSharpZapoctak.ViewModels;

namespace CSharpZapoctak.Models
{
    class Strength : ViewModelBase
    {
        public int id;

        private string situation = "";
        public string Situation
        {
            get { return situation; }
            set
            {
                situation = value;
                OnPropertyChanged();
            }
        }

        private string advantage;
        public string Advantage
        {
            get { return advantage; }
            set
            {
                advantage = value;
                OnPropertyChanged();
            }
        }
    }
}
