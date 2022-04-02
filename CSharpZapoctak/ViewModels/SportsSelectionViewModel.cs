using CSharpZapoctak.Commands;
using CSharpZapoctak.Stores;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    public class SportsSelectionViewModel : NotifyPropertyChanged
    {
        /*
        private Sport football = new Sport { name = "football" };
        public Sport Football
        {
            get { return football; }
            set { football = value; }
        }
        */

        private Sport iceHockey = new Sport { name = "ice_hockey" };
        public Sport IceHockey
        {
            get { return iceHockey; }
            set { iceHockey = value; }
        }
        
        /*
        private Sport tennis = new Sport { name = "tennis" };
        public Sport Tennis
        {
            get { return tennis; }
            set { tennis = value; }
        }
        */

        private Sport floorball = new Sport { name = "floorball" };
        public Sport Floorball
        {
            get { return floorball; }
            set { floorball = value; }
        }

        public ICommand NavigateSportCommand { get; }

        public SportsSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateSportCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new CompetitionsSelectionViewModel(navigationStore)));
        }
    }
}