using SportsManager.ViewModels;

namespace SportsManager.Models
{
    public class Strength : NotifyPropertyChanged
    {
        public int ID { get; set; }

        private string situation = "";
        public string Situation
        {
            get => situation;
            set
            {
                situation = value;
                OnPropertyChanged();
            }
        }

        private string advantage;
        public string Advantage
        {
            get => advantage;
            set
            {
                advantage = value;
                OnPropertyChanged();
            }
        }
    }
}
