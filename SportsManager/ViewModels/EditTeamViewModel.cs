using SportsManager.Commands;
using SportsManager.Models;
using SportsManager.Others;
using SportsManager.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace SportsManager.ViewModels
{
    /// <summary>
    /// Viewmodel for editing a team.
    /// </summary>
    public class EditTeamViewModel : TemplateEditViewModel<Team>
    {
        /// <summary>
        /// Current team instance.
        /// </summary>
        public Team Team
        {
            get => Entity;
            set
            {
                Entity = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Collection of all the countries of the world.
        /// </summary>
        private ObservableCollection<Country> countries;
        public ObservableCollection<Country> Countries
        {
            get => countries;
            set
            {
                countries = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Instantiates new EditTeamViewModel.
        /// </summary>
        /// <param name="navigationStore">Current instance of NavigationStore.</param>
        /// <param name="t">Team for editing.</param>
        public EditTeamViewModel(NavigationStore navigationStore, Team t)
        {
            Team = t;
            NavigateBackCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new TeamViewModel(navigationStore, Team)));
            GetImageFolderPath();
            Countries = SportsData.Countries;

            if (!string.IsNullOrWhiteSpace(Team.ImagePath))
            {
                Bitmap = ImageHandler.ImageToBitmap(Team.ImagePath);
            }
            else
            {
                Team.ImagePath = "";
            }
        }

        /// <summary>
        /// Saves the team into database.
        /// </summary>
        protected override void Save()
        {
            if (string.IsNullOrWhiteSpace(Team.Name))
            {
                _ = MessageBox.Show("You must provide a name for your team.", "Name missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //UPDATE
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("UPDATE team SET name = '" + Team.Name + "', info = '" + Team.Info + "', status = '" + Convert.ToInt32(Team.Status) + "', " +
                "country = '" + Team.Country.CodeTwo + "', date_of_creation = '" + Team.DateOfCreation.ToString("yyyy-MM-dd H:mm:ss") + "' " +
                "WHERE id = " + Team.ID, connection);

            try
            {   //execute querry
                connection.Open();
                _ = cmd.ExecuteNonQuery();

                //save logo
                UpdateImage();

                NavigateBackCommand.Execute(null);
            }
            catch (Exception)
            {
                _ = MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}