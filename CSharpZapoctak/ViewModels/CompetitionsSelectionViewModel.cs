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
    class CompetitionsSelectionViewModel : NotifyPropertyChanged
    {
        public ICommand NavigateCompetitionCommand { get; }
        public ICommand NavigateAddCompetitionCommand { get; }

        private ICommand checkNavigateCompetitionCommand;
        public ICommand CheckNavigateCompetitionCommand
        {
            get
            {
                if (checkNavigateCompetitionCommand == null)
                {
                    checkNavigateCompetitionCommand = new RelayCommand(param => CheckNavigateCompetition((Competition)param));
                }
                return checkNavigateCompetitionCommand;
            }
        }

        public ObservableCollection<Competition> Competitions { get; set; }

        public CompetitionsSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateCompetitionCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new CompetitionViewModel(navigationStore)));
            NavigateAddCompetitionCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new AddEditCompetitionViewModel(navigationStore)));

            //reset competition and season
            SportsData.Set(SportsData.SPORT);

            //load competitions list
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.SPORT.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT id , name , info FROM competitions", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                Competitions = new ObservableCollection<Competition>();

                //competition placeholder for add new competition button
                Competition c = new Competition
                {
                    id = SportsData.NO_ID,
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
                    string[] imgPath = System.IO.Directory.GetFiles(SportsData.CompetitionLogosPath, SportsData.SPORT.name + compet["id"].ToString() + ".*");
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
            finally
            {
                connection.Close();
            }
        }

        private void CheckNavigateCompetition(Competition c)
        {
            if (c.id == SportsData.NO_ID)
            {
                NavigateAddCompetitionCommand.Execute(new Competition());
            }
            else
            {
                NavigateCompetitionCommand.Execute(c);
            }
        }
    }
}
