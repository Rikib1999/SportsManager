using CSharpZapoctak.Stores;
using CSharpZapoctak.ViewModels;
using System;

namespace CSharpZapoctak.Commands
{
    public class NavigateCommand<TViewModel> : CommandBase where TViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly Func<TViewModel> _createViewModel;

        public NavigateCommand(NavigationStore navigationStore, Func<TViewModel> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }

        public override void Execute(object parameter)
        {
            SportsData.Set(parameter);
            _navigationStore.CurrentViewModel = _createViewModel();
        }
    }
}
