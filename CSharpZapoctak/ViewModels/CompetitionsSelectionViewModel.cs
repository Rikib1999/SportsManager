using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class CompetitionsSelectionViewModel : ViewModelBase
    {
        public ICommand NavigateCompetitionCommand { get; }
        public ObservableCollection<Competition> Competitions { get; set; }

        public CompetitionsSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateCompetitionCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new CompetitionViewModel(navigationStore)));

            SportsData.competition = new Competition();
            SportsData.season = new Season();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT id , name , info FROM competitions", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                Competitions = new ObservableCollection<Competition>();
                Competition c = new Competition
                {
                    id = (int)EntityState.AddNew,
                    Name = "ADD NEW",
                    LogoPath = SportsData.ResourcesPath + "/add_icon.png"
                };
                Competitions.Add(c);

                foreach (DataRow compet in dataTable.Rows)
                {
                    c = new Competition
                    {
                        id = int.Parse(compet["id"].ToString()),
                        Name = compet["name"].ToString(),
                        Info = compet["info"].ToString()
                    };
                    string[] imgPath = System.IO.Directory.GetFiles(SportsData.CompetitionLogosPath, SportsData.sport.name + compet["id"].ToString() + ".*");
                    if (imgPath.Length != 0)
                    {
                        c.LogoPath = imgPath.First();
                    }
                    Competitions.Add(c);
                }
            }
            catch (System.Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
