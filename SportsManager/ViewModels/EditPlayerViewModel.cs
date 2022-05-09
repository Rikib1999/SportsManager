using SportsManager.Commands;
using SportsManager.Models;
using SportsManager.Others;
using SportsManager.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace SportsManager.ViewModels
{
    /// <summary>
    /// Viewmodel for editing the player.
    /// </summary>
    public class EditPlayerViewModel : TemplateEditViewModel<Player>
    {
        /// <summary>
        /// Current player.
        /// </summary>
        public Player Player
        {
            get => Entity;
            set
            {
                Entity = value;
                OnPropertyChanged();
            }
        }

        private string newPlaysWith;
        /// <summary>
        /// Right ot left.
        /// </summary>
        public string NewPlaysWith
        {
            get => newPlaysWith;
            set
            {
                newPlaysWith = value;
                OnPropertyChanged();
            }
        }

        private string newGender;
        /// <summary>
        /// Players gender. M for males and F for females.
        /// </summary>
        public string NewGender
        {
            get => newGender;
            set
            {
                newGender = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Collection of options for choosing playing side.
        /// </summary>
        public ObservableCollection<string> PlaysWith { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Collection of genders.
        /// </summary>
        public ObservableCollection<string> Genders { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Collection of countries of the world.
        /// </summary>
        public ObservableCollection<Country> Countries { get; } = new ObservableCollection<Country>();

        /// <summary>
        /// Instantiates new EditPlayerViewModel.
        /// </summary>
        /// <param name="navigationStore">Current instance of NavigationStore.</param>
        /// <param name="p">Player for editing.</param>
        public EditPlayerViewModel(NavigationStore navigationStore, Player p)
        {
            Player = p;
            NavigateBackCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new PlayerViewModel(navigationStore, Player)));
            GetImageFolderPath();
            Countries = SportsData.Countries;
            LoadGenders();
            LoadPlaysWith();

            Player.Citizenship = SportsData.Countries.First(x => x.CodeTwo == Player.Citizenship.CodeTwo);
            Player.BirthplaceCountry = SportsData.Countries.First(x => x.CodeTwo == Player.BirthplaceCountry.CodeTwo);

            NewGender = Player.Gender == "M" ? "Male" : "Female";
            NewPlaysWith = Player.PlaysWith == "R" ? "Right" : "Left";

            if (!string.IsNullOrWhiteSpace(Player.ImagePath))
            {
                if (Player.ImagePath != SportsData.ResourcesPath + "\\male.png" && Player.ImagePath != SportsData.ResourcesPath + "\\female.png")
                {
                    Bitmap = ImageHandler.ImageToBitmap(Player.ImagePath);
                }
                else
                {
                    Player.ImagePath = "";
                }
            }
        }

        /// <summary>
        /// Populates the collection of genders.
        /// </summary>
        private void LoadGenders()
        {
            Genders = new();
            Genders.Add("Male");
            Genders.Add("Female");
        }

        /// <summary>
        /// Populates the collection of playing sides.
        /// </summary>
        private void LoadPlaysWith()
        {
            PlaysWith = new();
            PlaysWith.Add("Right");
            PlaysWith.Add("Left");
        }

        /// <summary>
        /// Saves the player into database.
        /// </summary>
        protected override void Save()
        {
            if (string.IsNullOrWhiteSpace(Player.FirstName) || string.IsNullOrWhiteSpace(Player.LastName))
            {
                _ = MessageBox.Show("Player name can not be empty.", "No name", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Player.Gender = NewGender == "Male" ? "M" : "F";
            Player.PlaysWith = NewPlaysWith == "Right" ? "R" : "L";

            //UPDATE
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("UPDATE player " +
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
                                   "WHERE id = " + Player.ID, connection);

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