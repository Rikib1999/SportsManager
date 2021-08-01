using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CSharpZapoctak.ViewModels
{
    class AddEditCompetitionViewModel : ViewModelBase
    {
        private Competition currentCompetition;
        public Competition CurrentCompetition
        {
            get { return currentCompetition; }
            set
            {
                currentCompetition = value;
                OnPropertyChanged();
            }
        }

        public Visibility HeaderVisibility { get; set; }

        private BitmapImage bitmap;
        public BitmapImage Bitmap
        {
            get { return bitmap; }
            set
            {
                bitmap = value;
                OnPropertyChanged();
            }
        }

        private ICommand loadImageCommand;
        public ICommand LoadImageCommand
        {
            get
            {
                if (loadImageCommand == null)
                {
                    loadImageCommand = new RelayCommand(param => LoadImage());
                }
                return loadImageCommand;
            }
        }

        private ICommand saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                {
                    saveCommand = new RelayCommand(param => Save());
                }
                return saveCommand;
            }
        }

        public AddEditCompetitionViewModel(NavigationStore navigationStore)
        {
            if (SportsData.competition.id == (int)EntityState.AddNew)
            {
                CurrentCompetition = new Competition();
                CurrentCompetition.id = (int)EntityState.AddNew;
                HeaderVisibility = Visibility.Visible;
            }
            else
            {
                CurrentCompetition = SportsData.competition;
                HeaderVisibility = Visibility.Collapsed;

                if (CurrentCompetition.LogoPath != null)
                {
                    MemoryStream ms = new MemoryStream();
                    byte[] arrbytFileContent = File.ReadAllBytes(CurrentCompetition.LogoPath);
                    ms.Write(arrbytFileContent, 0, arrbytFileContent.Length);
                    ms.Position = 0;

                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    Bitmap = bitmap;
                }
            }
            //TO DO: SportsData.season = new Season(); needed? maybe not
        }

        private void LoadImage()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.DefaultExt = ".png";
            open.Filter = "Pictures (*.jpg;*.png)|*.jpg;*.png";

            if (open.ShowDialog() == true)
            {
                CurrentCompetition.LogoPath = open.FileName;
                
                MemoryStream ms = new MemoryStream();
                byte[] arrbytFileContent = File.ReadAllBytes(CurrentCompetition.LogoPath);
                ms.Write(arrbytFileContent, 0, arrbytFileContent.Length);
                ms.Position = 0;

                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                Bitmap = bitmap;
                GC.Collect();
            }
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(CurrentCompetition.Name))
            {
                return;
            }
            if (CurrentCompetition.id == (int)EntityState.AddNew)
            {
                //INSERT
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("INSERT INTO competitions(name ,info) VALUES ('" + CurrentCompetition.Name + "', '" + CurrentCompetition.Info + "')", connection);

                try
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    currentCompetition.id = (int)cmd.LastInsertedId;

                    if (string.IsNullOrWhiteSpace(CurrentCompetition.LogoPath))
                    {
                        return;
                    }
                    string filePath = SportsData.CompetitionLogosPath + "/" + SportsData.sport.name + currentCompetition.id + Path.GetExtension(CurrentCompetition.LogoPath);
                    File.Copy(CurrentCompetition.LogoPath, filePath);
                    CurrentCompetition.LogoPath = filePath;
                    
                    //reset
                    CurrentCompetition = new Competition();
                    CurrentCompetition.id = (int)EntityState.AddNew;
                    Bitmap = null;
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                //UPDATE
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("UPDATE competitions SET name = '" + CurrentCompetition.Name + "', info = '" + CurrentCompetition.Info + "' WHERE id = " + CurrentCompetition.id, connection);

                try
                {   //execute querry
                    connection.Open();
                    cmd.ExecuteNonQuery();

                    //SAVE LOGO
                    //if logo is not selected return
                    if (string.IsNullOrWhiteSpace(CurrentCompetition.LogoPath))
                    {
                        return;
                    }
                    //get current logo
                    string[] imgPath = Directory.GetFiles(SportsData.CompetitionLogosPath, SportsData.sport.name + currentCompetition.id + ".*");
                    string filePath = "";
                    //if logo existed
                    if (imgPath.Length != 0)
                    {
                        filePath = imgPath.First();
                    }
                    //if logo did not exist declare its path
                    if (string.IsNullOrWhiteSpace(filePath))
                    {
                        filePath = SportsData.CompetitionLogosPath + @"\" + SportsData.sport.name + currentCompetition.id + Path.GetExtension(CurrentCompetition.LogoPath);
                    }
                    //if logo had changeg
                    if (CurrentCompetition.LogoPath != filePath)
                    {
                        GC.Collect();
                        File.Delete(filePath);
                        filePath = Path.ChangeExtension(filePath, Path.GetExtension(CurrentCompetition.LogoPath));
                        File.Copy(CurrentCompetition.LogoPath, filePath);
                        CurrentCompetition.LogoPath = filePath;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}