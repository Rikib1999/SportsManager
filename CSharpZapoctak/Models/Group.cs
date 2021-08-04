using CSharpZapoctak.ViewModels;
using System.Collections.ObjectModel;

namespace CSharpZapoctak.Models
{
    class Group : ViewModelBase
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

        private int seasonID;
        public int SeasonID
        {
            get { return seasonID; }
            set
            {
                seasonID = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Team> teams;
        public ObservableCollection<Team> Teams
        {
            get { return teams; }
            set
            {
                teams = value;
                OnPropertyChanged();
            }
        }
    }
}