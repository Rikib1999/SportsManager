using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using CSharpZapoctak.ViewModels;
using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;
using System.IO;
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

            //automatically starts mysql
            if (Process.GetProcessesByName("mysqld").Length == 0)
            {
                //Process.Start("C:/xampp/mysql/bin/mysqld.exe");
            }

            LoadCountries();

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
        protected override void OnExit(ExitEventArgs e)
        {
            //Process.GetProcessesByName("mysqld")[0].Kill();

            base.OnExit(e);
        }

        private void LoadCountries()
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=sports_manager;UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT code_two , name , code_three FROM country", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow cntry in dataTable.Rows)
                {
                    Country c = new Country
                    {
                        Name = cntry["name"].ToString(),
                        CodeTwo = cntry["code_two"].ToString(),
                        CodeThree = cntry["code_three"].ToString()
                    };
                    SportsData.countries.Add(c);
                }
            }
            catch (System.Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
