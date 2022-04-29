using SportsManager.Commands;
using SportsManager.Models;
using SportsManager.Stores;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SportsManager.ViewModels
{
    public class CompetitionsSelectionViewModel : NotifyPropertyChanged
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
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT id , name , info FROM competitions", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                Competitions = new ObservableCollection<Competition>();

                //competition placeholder for add new competition button
                Competition c = new()
                {
                    ID = SportsData.NOID,
                    Name = "ADD NEW",
                    ImagePath = SportsData.ResourcesPath + "/add_icon.png"
                };
                Competitions.Add(c);

                foreach (DataRow compet in dataTable.Rows)
                {
                    c = new Competition
                    {
                        ID = int.Parse(compet["id"].ToString()),
                        Name = compet["name"].ToString(),
                        Info = compet["info"].ToString()
                    };
                    string[] imgPath = System.IO.Directory.GetFiles(SportsData.CompetitionLogosPath, SportsData.SPORT.Name + compet["id"].ToString() + ".*");
                    if (imgPath.Length != 0)
                    {
                        c.ImagePath = imgPath.First();
                    }
                    Competitions.Add(c);
                }
            }
            catch (System.Exception)
            {
                _ = MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Navigates to view for editing competition or adding new competition.
        /// </summary>
        /// <param name="c">Selected competition.</param>
        private void CheckNavigateCompetition(Competition c)
        {
            if (c.ID == SportsData.NOID)
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
