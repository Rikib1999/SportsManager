using CSharpZapoctak.ViewModels;
using System.Collections.ObjectModel;

namespace CSharpZapoctak.Models
{
    public class Group : NotifyPropertyChanged
    {
        public int ID { get; set; } = SportsData.NOID;

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

        private int seasonID;
        public int SeasonID
        {
            get => seasonID;
            set
            {
                seasonID = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Team> teams;
        public ObservableCollection<Team> Teams
        {
            get => teams;
            set
            {
                teams = value;
                OnPropertyChanged();
            }
        }
    }
}