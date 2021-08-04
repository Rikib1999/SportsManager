using CSharpZapoctak.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace CSharpZapoctak
{
    public struct Sport
    {
        public string name;
    }

    public enum EntityState
    {
        AddNew = -1,
        NotSelected = -2
    }

    public interface IStats { };

    /// <summary>
    /// static data for database and current settings
    /// </summary>
    public static class SportsData
    {
        public static string server = "localhost";
        public static string UID = "root";
        public static string password = "";

        public static string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/SportsManager";
        public static string ImagesPath = AppDataPath + "/Images";
        public static string ResourcesPath = "/Resources";
        public static string CompetitionLogosPath = AppDataPath + "/Images/Competition_Logos";
        public static string SeasonLogosPath = AppDataPath + "/Images/Season_Logos";
        public static string TeamLogosPath = AppDataPath + "/Images/Team_Logos";
        public static string PlayerPhotosPath = AppDataPath + "/Images/Player_Photos";

        public static Sport sport = new Sport { name = "" };
        public static Competition competition = new Competition();
        public static Season season = new Season();

        public static ObservableCollection<Country> countries = new ObservableCollection<Country>();

        public static void Set(object parameter)
        {
            switch (parameter)
            {
                case Sport s:
                    Set(s);
                    break;
                case Season s:
                    Set(s);
                    break;
                case Competition c:
                    Set(c);
                    break;
                default:
                    break;
            }
        }

        public static void Set(Sport s)
        {
            sport = s;
            competition = new Competition { id = (int)EntityState.NotSelected };
            season = new Season { id = (int)EntityState.NotSelected };
        }

        public static void Set(Competition c)
        {
            if (competition.id != c.id)
            {
                season = new Season { id = (int)EntityState.NotSelected };
            }
            competition = c;
        }

        public static void Set(Season s)
        {
            season = s;
            if (competition.id == (int)EntityState.NotSelected)
            {
                string connectionString = "SERVER=" + server + ";DATABASE=" + sport.name + ";UID=" + UID + ";PASSWORD=" + password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT id , name , info FROM competitions WHERE id = '" + s.Competition.id + "'", connection);

                try
                {
                    connection.Open();
                    DataTable dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());

                    Competition c = new Competition
                    {
                        id = int.Parse(dataTable.Rows[0]["id"].ToString()),
                        Name = dataTable.Rows[0]["name"].ToString(),
                        Info = dataTable.Rows[0]["info"].ToString()
                    };
                    string[] imgPath = System.IO.Directory.GetFiles(CompetitionLogosPath, sport.name + dataTable.Rows[0]["id"].ToString() + ".*");
                    if (imgPath.Length != 0)
                    {
                        c.LogoPath = imgPath.First();
                    }
                    competition = c;
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
