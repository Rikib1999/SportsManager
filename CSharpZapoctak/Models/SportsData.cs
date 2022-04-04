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

    public interface IStats { };

    /// <summary>
    /// Static data for database and current settings.
    /// </summary>
    public static class SportsData
    {
        public static readonly string server = "localhost";
        public static readonly string UID = "root";
        public static readonly string password = "";

        public static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SportsManager";
        public static readonly string ImagesPath = AppDataPath + "/Images";
        public static readonly string ResourcesPath = "/Resources";
        public static readonly string CompetitionLogosPath = AppDataPath + "/Images/Competition_Logos";
        public static readonly string SeasonLogosPath = AppDataPath + "/Images/Season_Logos";
        public static readonly string TeamLogosPath = AppDataPath + "/Images/Team_Logos";
        public static readonly string PlayerPhotosPath = AppDataPath + "/Images/Player_Photos";
        public static readonly string PythonOCRPath = AppDataPath + "/GamesheetOCR.py";

        public static readonly int NO_ID = -1;

        public static Sport SPORT { get; private set; } = new Sport { name = "" };
        public static Competition COMPETITION { get; private set; } = new Competition();
        public static Season SEASON { get; private set; } = new Season();

        public static ObservableCollection<Country> countries = new ObservableCollection<Country>();

        public static bool IsCompetitionSet()
        {
            return COMPETITION.id != NO_ID;
        }

        public static bool IsSeasonSet()
        {
            return SEASON.id != NO_ID;
        }

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
            SPORT = s;
            COMPETITION = new Competition();
            SEASON = new Season();
        }

        public static void Set(Competition c)
        {
            if (COMPETITION.id != c.id)
            {
                SEASON = new Season();
            }
            COMPETITION = c;
        }

        public static void Set(Season s)
        {
            SEASON = s;
            if (!IsCompetitionSet())
            {
                string connectionString = "SERVER=" + server + ";DATABASE=" + SPORT.name + ";UID=" + UID + ";PASSWORD=" + password + ";";
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
                    string[] imgPath = System.IO.Directory.GetFiles(CompetitionLogosPath, SPORT.name + dataTable.Rows[0]["id"].ToString() + ".*");
                    if (imgPath.Length != 0)
                    {
                        c.LogoPath = imgPath.First();
                    }
                    COMPETITION = c;
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

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.SPORT.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
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

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.SPORT.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
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

        public static void LoadCountries()
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=sports_manager;UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT code_two , name , code_three FROM country", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow cntry in dataTable.Rows)
                {
                    Country c = new Country
                    {
                        Name = cntry["name"].ToString(),
                        CodeTwo = cntry["code_two"].ToString(),
                        CodeThree = cntry["code_three"].ToString()
                    };
                    SportsData.countries.Add(c);
                }
            }
            catch (System.Exception)
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
