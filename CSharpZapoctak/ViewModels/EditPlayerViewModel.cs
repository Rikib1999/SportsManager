using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CSharpZapoctak.ViewModels
{
    class EditPlayerViewModel : ViewModelBase
    {
        private Player player;
        public Player Player
        {
            get { return player; }
            set
            {
                player = value;
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

        private string newPlaysWith;
        public string NewPlaysWith
        {
            get { return newPlaysWith; }
            set
            {
                newPlaysWith = value;
                OnPropertyChanged();
            }
        }

        private string newGender;
        public string NewGender
        {
            get { return newGender; }
            set
            {
                newGender = value;
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

        public ObservableCollection<string> PlaysWith { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> Genders { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<Country> Countries { get; } = new ObservableCollection<Country>();

        public NavigationStore ns;

        public EditPlayerViewModel(NavigationStore navigationStore, Player p)
        {
            Player = p;
            Countries = SportsData.countries;
            ns = navigationStore;

            Player.Citizenship = SportsData.countries.Where(x => x.CodeTwo == Player.Citizenship.CodeTwo).First();
            Player.BirthplaceCountry = SportsData.countries.Where(x => x.CodeTwo == Player.BirthplaceCountry.CodeTwo).First();
            Countries = SportsData.countries;
            LoadGenders();
            LoadPlaysWith();

            NewGender = Player.Gender == "M" ? "Male" : "Female";
            NewPlaysWith = Player.PlaysWith == "R" ? "Right" : "Left";

            if (!string.IsNullOrWhiteSpace(Player.PhotoPath))
            {
                if (Player.PhotoPath != SportsData.ResourcesPath + "\\male.png" && Player.PhotoPath != SportsData.ResourcesPath + "\\female.png")
                {
                    MemoryStream ms = new MemoryStream();
                    byte[] arrbytFileContent = File.ReadAllBytes(Player.PhotoPath);
                    ms.Write(arrbytFileContent, 0, arrbytFileContent.Length);
                    ms.Position = 0;

                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    Bitmap = bitmap;
                }
            }
        }

        private void LoadGenders()
        {
            Genders = new ObservableCollection<string>();
            Genders.Add("Male");
            Genders.Add("Female");
        }

        private void LoadPlaysWith()
        {
            PlaysWith = new ObservableCollection<string>();
            PlaysWith.Add("Right");
            PlaysWith.Add("Left");
        }

        private void LoadImage()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.DefaultExt = ".png";
            open.Filter = "Pictures (*.jpg;*.png)|*.jpg;*.png";

            if (open.ShowDialog() == true)
            {
                Player.PhotoPath = open.FileName;

                MemoryStream ms = new MemoryStream();
                byte[] arrbytFileContent = File.ReadAllBytes(Player.PhotoPath);
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
            if (string.IsNullOrWhiteSpace(Player.FirstName) || string.IsNullOrWhiteSpace(Player.LastName))
            {
                MessageBox.Show("Player name can not be empty.", "No name", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Player.Gender = NewGender == "Male" ? "M" : "F";
            Player.PlaysWith = NewPlaysWith == "Right" ? "R" : "L";

            //UPDATE
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("UPDATE player " +
                                                "SET first_name = '" + Player.FirstName + "', " +
                                                "last_name = '" + Player.LastName + "', " +
                                                "birthdate = '" + Player.Birthdate.ToString("yyyy-MM-dd H:mm:ss") + "', " +
                                                "gender = '" + Player.Gender + "', " +
                                                "height = " + Player.Height + ", " +
                                                "weight = " + Player.Weight + ", " +
                                                "plays_with = '" + Player.PlaysWith + "', " +
                                                "citizenship = '" + Player.Citizenship.CodeTwo + "', " +
                                                "birthplace_city = '" + Player.BirthplaceCity + "', " +
                                                "birthplace_country = '" + Player.BirthplaceCountry.CodeTwo + "', " +
                                                "status = '" + Convert.ToInt32(Player.Status) + "', " +
                                                "info = '" + Player.Info + "' " +
                                                "WHERE id = " + Player.id, connection);

            try
            {   //execute querry
                connection.Open();
                cmd.ExecuteNonQuery();

                //SAVE LOGO
                //if logo is not selected return
                if (string.IsNullOrWhiteSpace(Player.PhotoPath))
                {
                    return;
                }
                //get current logo
                string[] imgPath = Directory.GetFiles(SportsData.PlayerPhotosPath, SportsData.sport.name + Player.id + ".*");
                string filePath = "";
                //if logo existed
                if (imgPath.Length != 0)
                {
                    filePath = imgPath.First();
                }
                //if logo did not exist declare its path
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    filePath = SportsData.PlayerPhotosPath + @"\" + SportsData.sport.name + Player.id + Path.GetExtension(Player.PhotoPath);
                }
                //if logo had changeg
                if (Player.PhotoPath != filePath)
                {
                    GC.Collect();
                    File.Delete(filePath);
                    filePath = Path.ChangeExtension(filePath, Path.GetExtension(Player.PhotoPath));
                    File.Copy(Player.PhotoPath, filePath);
                    Player.PhotoPath = filePath;
                }

                new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new PlayerViewModel(ns, Player))).Execute(null);
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