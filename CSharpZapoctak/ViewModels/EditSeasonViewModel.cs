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
    class EditSeasonViewModel : NotifyPropertyChanged
    {
        private Season currentSeason;
        public Season CurrentSeason
        {
            get { return currentSeason; }
            set
            {
                currentSeason = value;
                OnPropertyChanged();
            }
        }

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

        private ICommand removeImageCommand;
        public ICommand RemoveImageCommand
        {
            get
            {
                if (removeImageCommand == null)
                {
                    removeImageCommand = new RelayCommand(param => RemoveImage());
                }
                return removeImageCommand;
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

        public NavigationStore ns;
        public EditSeasonViewModel(NavigationStore navigationStore)
        {
            CurrentSeason = SportsData.season;
            ns = navigationStore;

            if (!string.IsNullOrWhiteSpace(CurrentSeason.LogoPath))
            {
                MemoryStream ms = new MemoryStream();
                byte[] arrbytFileContent = File.ReadAllBytes(CurrentSeason.LogoPath);
                ms.Write(arrbytFileContent, 0, arrbytFileContent.Length);
                ms.Position = 0;

                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                Bitmap = bitmap;
            }
            else
            {
                CurrentSeason.LogoPath = "";
            }
        }

        private void LoadImage()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.DefaultExt = ".png";
            open.Filter = "Pictures (*.jpg;*.png)|*.jpg;*.png";

            if (open.ShowDialog() == true)
            {
                CurrentSeason.LogoPath = open.FileName;

                MemoryStream ms = new MemoryStream();
                byte[] arrbytFileContent = File.ReadAllBytes(CurrentSeason.LogoPath);
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

        private void RemoveImage()
        {
            CurrentSeason.LogoPath = "";
            Bitmap = new BitmapImage();
            GC.Collect();
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(CurrentSeason.Name))
            {
                return;
            }
            //UPDATE
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("UPDATE seasons SET name = '" + CurrentSeason.Name + "', info = '" + CurrentSeason.Info + "' WHERE id = " + CurrentSeason.id, connection);

            try
            {   //execute querry
                connection.Open();
                cmd.ExecuteNonQuery();

                //SAVE LOGO
                //if logo is not selected return
                if (string.IsNullOrWhiteSpace(CurrentSeason.LogoPath))
                {
                    //if there is logo in the database then delete it
                    //get previous logo
                    string[] previousImgPath = Directory.GetFiles(SportsData.SeasonLogosPath, SportsData.sport.name + CurrentSeason.id + ".*");
                    string previousFilePath = "";
                    //if it exists
                    if (previousImgPath.Length != 0)
                    {
                        previousFilePath = previousImgPath.First();
                    }
                    //delete logo
                    if (!string.IsNullOrWhiteSpace(previousFilePath))
                    {
                        GC.Collect();
                        File.Delete(previousFilePath);
                    }
                }
                else
                {
                    //get current logo
                    string[] imgPath = Directory.GetFiles(SportsData.SeasonLogosPath, SportsData.sport.name + currentSeason.id + ".*");
                    string filePath = "";
                    //if logo existed
                    if (imgPath.Length != 0)
                    {
                        filePath = imgPath.First();
                    }
                    //if logo did not exist declare its path
                    if (string.IsNullOrWhiteSpace(filePath))
                    {
                        filePath = SportsData.SeasonLogosPath + @"\" + SportsData.sport.name + currentSeason.id + Path.GetExtension(CurrentSeason.LogoPath);
                    }
                    //if logo had changeg
                    if (CurrentSeason.LogoPath != filePath)
                    {
                        GC.Collect();
                        File.Delete(filePath);
                        filePath = Path.ChangeExtension(filePath, Path.GetExtension(CurrentSeason.LogoPath));
                        File.Copy(CurrentSeason.LogoPath, filePath);
                        CurrentSeason.LogoPath = filePath;
                    }
                }

                new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new SeasonViewModel(ns))).Execute(null);
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