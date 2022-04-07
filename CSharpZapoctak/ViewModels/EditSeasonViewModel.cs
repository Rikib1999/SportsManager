using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Others;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace CSharpZapoctak.ViewModels
{
    public class EditSeasonViewModel : TemplateEditViewModel<Season>
    {
        public Season Season
        {
            get => Entity;
            set
            {
                Entity = value;
                OnPropertyChanged();
            }
        }

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