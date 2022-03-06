using CSharpZapoctak.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace CSharpZapoctak
{
    static class Extensions
    {
        public static void Sort<T>(this ObservableCollection<T> collection) where T : IComparable
        {
            List<T> sorted = collection.OrderBy(x => x).ToList();
            for (int i = 0; i < sorted.Count(); i++)
                collection.Move(collection.IndexOf(sorted[i]), i);
        }
    }

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
    /// Static data for database and current settings.
    /// </summary>
    public static class SportsData
    {
        public static string server = "localhost";
        public static string UID = "root";
        public static string password = "";

        public static string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SportsManager";
        public static string ImagesPath = AppDataPath + "/Images";
        public static string ResourcesPath = "/Resources";
        public static string CompetitionLogosPath = AppDataPath + "/Images/Competition_Logos";
        public static string SeasonLogosPath = AppDataPath + "/Images/Season_Logos";
        public static string TeamLogosPath = AppDataPath + "/Images/Team_Logos";
        public static string PlayerPhotosPath = AppDataPath + "/Images/Player_Photos";
        public static string PythonOCRPath = AppDataPath + "/GamesheetOCR.py";

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

        public static ObservableCollection<PenaltyReason> LoadPenaltyReasons()
        {
            ObservableCollection<PenaltyReason> PenaltyReasons = new ObservableCollection<PenaltyReason>();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT code, name FROM penalty_reason", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow row in dataTable.Rows)
                {
                    PenaltyReason pr = new PenaltyReason
                    {
                        Code = row["code"].ToString(),
                        Name = row["name"].ToString()
                    };

                    PenaltyReasons.Add(pr);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return PenaltyReasons;
        }

        public static ObservableCollection<PenaltyType> LoadPenaltyTypes()
        {
            ObservableCollection<PenaltyType> PenaltyTypes = new ObservableCollection<PenaltyType>();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT code, name, minutes FROM penalty_type", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow row in dataTable.Rows)
                {
                    PenaltyType pt = new PenaltyType
                    {
                        Code = row["code"].ToString(),
                        Name = row["name"].ToString(),
                        Minutes = int.Parse(row["minutes"].ToString())
                    };

                    PenaltyTypes.Add(pt);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return PenaltyTypes;
        }
    }
}
