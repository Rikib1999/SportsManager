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
    /// Viewmodel for adding new competition or editing an existing one.
    /// </summary>
    public class AddEditCompetitionViewModel : TemplateEditViewModel<Competition>
    {
        /// <summary>
        /// Current competition instance.
        /// </summary>
        public Competition Competition
        {
            get => Entity;
            set
            {
                Entity = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Visibility of the header for adding a competition.
        /// </summary>
        public Visibility HeaderVisibility { get; set; }

        /// <summary>
        /// Instantiates new AddEditCompetitionViewModel.
        /// </summary>
        /// <param name="navigationStore">Current instance of NavigationStore.</param>
        public AddEditCompetitionViewModel(NavigationStore navigationStore)
        {
            Competition = SportsData.COMPETITION;
            NavigateBackCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new CompetitionViewModel(navigationStore)));
            GetImageFolderPath();

            if (!SportsData.IsCompetitionSet())
            {
                HeaderVisibility = Visibility.Visible;
            }
            else
            {
                HeaderVisibility = Visibility.Collapsed;

                if (!string.IsNullOrWhiteSpace(Competition.ImagePath))
                {
                    Bitmap = ImageHandler.ImageToBitmap(Competition.ImagePath);
                }
                else
                {
                    Competition.ImagePath = "";
                }
            }
        }

        /// <summary>
        /// Saves the current competition into database.
        /// </summary>
        protected override void Save()
        {
            if (string.IsNullOrWhiteSpace(Competition.Name))
            {
                _ = MessageBox.Show("You must provide a name for your competition.", "Name missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!SportsData.IsCompetitionSet())
            {
                //INSERT
                MySqlConnection connection = new(SportsData.ConnectionStringSport);
                MySqlCommand cmd = new("INSERT INTO competitions(name ,info) VALUES ('" + Competition.Name + "', '" + Competition.Info + "')", connection);

                try
                {
                    connection.Open();
                    _ = cmd.ExecuteNonQuery();
                    Competition.ID = (int)cmd.LastInsertedId;

                    SaveImage();

                    NavigateBackCommand.Execute(Competition);
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
            else
            {
                //UPDATE
                MySqlConnection connection = new(SportsData.ConnectionStringSport);
                MySqlCommand cmd = new("UPDATE competitions SET name = '" + Competition.Name + "', info = '" + Competition.Info + "' WHERE id = " + Competition.ID, connection);

                try
                {
                    //execute querry
                    connection.Open();
                    _ = cmd.ExecuteNonQuery();

                    //save logo
                    UpdateImage();

                    NavigateBackCommand.Execute(Competition);
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
}