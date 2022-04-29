using SportsManager.Others;
using SportsManager.Stores;
using SportsManager.ViewModels;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SportsManager
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
                _ = Directory.CreateDirectory(SportsData.AppDataPath);
            }
            if (!Directory.Exists(SportsData.ImagesPath))
            {
                _ = Directory.CreateDirectory(SportsData.ImagesPath);
            }
            if (!Directory.Exists(SportsData.CompetitionLogosPath))
            {
                _ = Directory.CreateDirectory(SportsData.CompetitionLogosPath);
            }
            if (!Directory.Exists(SportsData.SeasonLogosPath))
            {
                _ = Directory.CreateDirectory(SportsData.SeasonLogosPath);
            }
            if (!Directory.Exists(SportsData.TeamLogosPath))
            {
                _ = Directory.CreateDirectory(SportsData.TeamLogosPath);
            }
            if (!Directory.Exists(SportsData.PlayerPhotosPath))
            {
                _ = Directory.CreateDirectory(SportsData.PlayerPhotosPath);
            }
            if (!File.Exists(SportsData.PythonOCRPath))
            {
                FileStream f = File.Create(SportsData.PythonOCRPath);
                f.Write(SportsManager.Properties.Resources.GamesheetOCR);
                f.Close();
                f.Dispose();
            }

            //check if all databases exists and if not create them
            DatabaseHandler.EnsureDatabases();
            //check if all tables exists and if not create them
            DatabaseHandler.EnsureTables();

            _ = Task.Run(() => SportsData.LoadCountries());

            NavigationStore navigationStore = new();

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