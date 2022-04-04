using CSharpZapoctak.Stores;
using CSharpZapoctak.ViewModels;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace CSharpZapoctak
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //create folders for images, if they do not exist
            if (!Directory.Exists(SportsData.AppDataPath))
            {
                Directory.CreateDirectory(SportsData.AppDataPath);
            }
            if (!Directory.Exists(SportsData.ImagesPath))
            {
                Directory.CreateDirectory(SportsData.ImagesPath);
            }
            if (!Directory.Exists(SportsData.CompetitionLogosPath))
            {
                Directory.CreateDirectory(SportsData.CompetitionLogosPath);
            }
            if (!Directory.Exists(SportsData.SeasonLogosPath))
            {
                Directory.CreateDirectory(SportsData.SeasonLogosPath);
            }
            if (!Directory.Exists(SportsData.TeamLogosPath))
            {
                Directory.CreateDirectory(SportsData.TeamLogosPath);
            }
            if (!Directory.Exists(SportsData.PlayerPhotosPath))
            {
                Directory.CreateDirectory(SportsData.PlayerPhotosPath);
            }
            if (!File.Exists(SportsData.PythonOCRPath))
            {
                FileStream f = File.Create(SportsData.PythonOCRPath);
                f.Write(CSharpZapoctak.Properties.Resources.GamesheetOCR);
                f.Close();
                f.Dispose();
            }

            Task.Run(() => SportsData.LoadCountries());

            NavigationStore navigationStore = new NavigationStore();

            //starts with SportsSelectionView
            navigationStore.CurrentViewModel = new SportsSelectionViewModel(navigationStore);

            MainWindow = new MainWindow()
            {
                DataContext = new MainViewModel(navigationStore)
            };
            MainWindow.Show();

            base.OnStartup(e);
        }
    }
}