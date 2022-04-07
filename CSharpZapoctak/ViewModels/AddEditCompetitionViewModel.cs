using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Others;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace CSharpZapoctak.ViewModels
{
    public class AddEditCompetitionViewModel : TemplateEditViewModel<Competition>
    {
        public Competition Competition
        {
            get => Entity;
            set
            {
                Entity = value;
                OnPropertyChanged();
            }
        }

        public Visibility HeaderVisibility { get; set; }

        public AddEditCompetitionViewModel(NavigationStore navigationStore)
        {
            NavigateBackCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new CompetitionViewModel(navigationStore)));
            GetImageFolderPath();

            if (!SportsData.IsCompetitionSet())
            {
                Competition = SportsData.COMPETITION;
                HeaderVisibility = Visibility.Visible;
            }
            else
            {
                Competition = SportsData.COMPETITION;
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