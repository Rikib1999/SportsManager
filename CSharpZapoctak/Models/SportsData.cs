using CSharpZapoctak.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CSharpZapoctak
{
    public struct Sport
    {
        public string Name { get; set; }

        public string FormattedName => char.ToUpper(Name[0]) + Name[1..].Replace('_', '-');

        public Brush Color { get; set; }

        public string Image => SportsData.ResourcesPath + "/" + Name + ".png";

        public string DatabaseTables { get; set; }
    }

    public interface IEntity
    {
        public int ID { get; set; }
    }

    /// <summary>
    /// Static data for database and current settings.
    /// </summary>
    public static class SportsData
    {
        #region Database connection strings
        public static readonly string server = "localhost";
        public static readonly string UID = "root";
        public static readonly string password = "";
        public static readonly string commonDatabaseName = "sports_manager";
        public static string ConnectionStringNoDatabase => "SERVER=" + server + ";UID=" + UID + ";PASSWORD=" + password + ";";
        public static string ConnectionStringSport => "SERVER=" + server + ";DATABASE=" + SPORT.Name + ";UID=" + UID + ";PASSWORD=" + password + "; convert zero datetime=True";
        public static string ConnectionStringCommon => "SERVER=" + server + ";DATABASE=" + commonDatabaseName + ";UID=" + UID + ";PASSWORD=" + password + "; convert zero datetime=True";
        #endregion

        #region Application folder and file paths
        public static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SportsManager";
        public static readonly string ImagesPath = AppDataPath + "/Images";
        public static readonly string ResourcesPath = "/Resources";
        public static readonly string CompetitionLogosPath = AppDataPath + "/Images/Competition_Logos";
        public static readonly string SeasonLogosPath = AppDataPath + "/Images/Season_Logos";
        public static readonly string TeamLogosPath = AppDataPath + "/Images/Team_Logos";
        public static readonly string PlayerPhotosPath = AppDataPath + "/Images/Player_Photos";
        public static readonly string PythonOCRPath = AppDataPath + "/GamesheetOCR.exe";
        #endregion

        /// <summary>
        /// Constant for not missing ID.
        /// </summary>
        public static readonly int NOID = -1;

        /// <summary>
        /// List of available sports.
        /// </summary>
        public static readonly List<Sport> SportsList = new()
        {
            //new() { Name = "football", Color = Brushes.LightGreen, DatabaseTables = Properties.Resources.football_tables },
            new() { Name = "ice_hockey", Color = Brushes.AliceBlue, DatabaseTables = Properties.Resources.ice_hockey_tables },
            //new() { Name = "tennis", Color = Brushes.Coral, DatabaseTables = Properties.Resources.tennis_tables },
            new() { Name = "floorball", Color = Brushes.CornflowerBlue, DatabaseTables = Properties.Resources.floorball_tables }
        };

        /// <summary>
        /// Currently selected sport.
        /// </summary>
        public static Sport SPORT { get; private set; } = new Sport { Name = "" };

        /// <summary>
        /// Currently selected competition.
        /// </summary>
        public static Competition COMPETITION { get; private set; } = new();

        /// <summary>
        /// Currently selected season.
        /// </summary>
        public static Season SEASON { get; private set; } = new();

        /// <summary>
        /// Collection of all the countries from database.
        /// </summary>
        public static ObservableCollection<Country> Countries { get; private set; } = new();

        /// <summary>
        /// Returns true if a competition is selected.
        /// </summary>
        public static bool IsCompetitionSet()
        {
            return COMPETITION.ID != NOID;
        }

        /// <summary>
        /// Returns true if a season is selected.
        /// </summary>
        public static bool IsSeasonSet()
        {
            return SEASON.ID != NOID;
        }

        /// <summary>
        /// Sets the sport/competition/season as currently selected.
        /// </summary>
        /// <param name="parameter">Sport, Competition or Season instance.</param>
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

        /// <summary>
        /// Sets the sport as currently selected. Resets competition and season.
        /// </summary>
        /// <param name="s">Sport to select.</param>
        public static void Set(Sport s)
        {
            SPORT = s;
            COMPETITION = new Competition();
            SEASON = new Season();
        }

        /// <summary>
        /// Sets the competition as currently selected. Resets season.
        /// </summary>
        /// <param name="c">Competition to select.</param>
        public static void Set(Competition c)
        {
            if (COMPETITION.ID != c.ID)
            {
                SEASON = new Season();
            }
            COMPETITION = c;
        }

        /// <summary>
        /// Sets the season as currently selected. Selects seasons competition too.
        /// </summary>
        /// <param name="s">Season to select.</param>
        public static void Set(Season s)
        {
            SEASON = s;
            if (!IsCompetitionSet())
            {
                MySqlConnection connection = new(ConnectionStringSport);
                MySqlCommand cmd = new("SELECT id , name , info FROM competitions WHERE id = '" + s.Competition.ID + "'", connection);

                try
                {
                    connection.Open();
                    DataTable dataTable = new();
                    dataTable.Load(cmd.ExecuteReader());

                    Competition c = new()
                    {
                        ID = int.Parse(dataTable.Rows[0]["id"].ToString()),
                        Name = dataTable.Rows[0]["name"].ToString(),
                        Info = dataTable.Rows[0]["info"].ToString()
                    };
                    string[] imgPath = System.IO.Directory.GetFiles(CompetitionLogosPath, SPORT.Name + dataTable.Rows[0]["id"].ToString() + ".*");
                    if (imgPath.Length != 0)
                    {
                        c.ImagePath = imgPath.First();
                    }
                    COMPETITION = c;
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

        /// <summary>
        /// Loads all penalty reasons from database.
        /// </summary>
        /// <returns>All penalty reasons.</returns>
        public static ObservableCollection<PenaltyReason> LoadPenaltyReasons()
        {
            ObservableCollection<PenaltyReason> PenaltyReasons = new();

            MySqlConnection connection = new(ConnectionStringSport);
            MySqlCommand cmd = new("SELECT code, name FROM penalty_reason", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow row in dataTable.Rows)
                {
                    PenaltyReason pr = new()
                    {
                        Code = row["code"].ToString(),
                        Name = row["name"].ToString()
                    };

                    PenaltyReasons.Add(pr);
                }
            }
            catch (Exception)
            {
                _ = MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        /// <summary>
        /// Loads all penalty types from database.
        /// </summary>
        /// <returns>All penalty types.</returns>
        public static ObservableCollection<PenaltyType> LoadPenaltyTypes()
        {
            ObservableCollection<PenaltyType> PenaltyTypes = new();

            MySqlConnection connection = new(ConnectionStringSport);
            MySqlCommand cmd = new("SELECT code, name, minutes FROM penalty_type", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow row in dataTable.Rows)
                {
                    PenaltyType pt = new()
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
                _ = MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        /// <summary>
        /// Loads all countries from database.
        /// </summary>
        public static void LoadCountries()
        {
            MySqlConnection connection = new(ConnectionStringCommon);
            MySqlCommand cmd = new("SELECT code_two , name , code_three FROM country", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow cntry in dataTable.Rows)
                {
                    Country c = new()
                    {
                        Name = cntry["name"].ToString(),
                        CodeTwo = cntry["code_two"].ToString(),
                        CodeThree = cntry["code_three"].ToString()
                    };
                    Countries.Add(c);
                }
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
