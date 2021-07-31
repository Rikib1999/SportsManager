using CSharpZapoctak.ViewModels;

namespace CSharpZapoctak.Models
{
    class Match : ViewModelBase
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
    }
}
