using SportsManager.Commands;
using SportsManager.Models;
using SportsManager.Others;
using SportsManager.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace SportsManager.ViewModels
{
    /// <summary>
    /// Viewmodel for editing a season.
    /// </summary>
    public class EditSeasonViewModel : TemplateEditViewModel<Season>
    {
        /// <summary>
        /// Current season insatnce.
        /// </summary>
        public Season Season
        {
            get => Entity;
            set
            {
                Entity = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Instantiates new EditSeasonViewModel.
        /// </summary>
        /// <param name="navigationStore">Current instance of NavigationStore.</param>
        public EditSeasonViewModel(NavigationStore navigationStore)
        {
            Season = SportsData.SEASON;
            NavigateBackCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new SeasonViewModel(navigationStore)));
            GetImageFolderPath();

            if (!string.IsNullOrWhiteSpace(Season.ImagePath))
            {
                Bitmap = ImageHandler.ImageToBitmap(Season.ImagePath);
            }
            else
            {
                Season.ImagePath = "";
            }
        }

        /// <summary>
        /// Saves the season into database.
        /// </summary>
        protected override void Save()
        {
            if (string.IsNullOrWhiteSpace(Season.Name))
            {
                _ = MessageBox.Show("You must provide a name for your season.", "Name missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //UPDATE
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("UPDATE seasons SET name = '" + Season.Name + "', info = '" + Season.Info + "' WHERE id = " + Season.ID, connection);

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