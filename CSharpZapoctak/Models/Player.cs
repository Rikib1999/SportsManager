using CSharpZapoctak.ViewModels;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace CSharpZapoctak.Models
{
    public class Player : NotifyPropertyChanged
    {
        public int id = SportsData.NO_ID;

        private string firstName = "";
        public string FirstName
        {
            get { return firstName; }
            set
            {
                firstName = value;
                OnPropertyChanged();
            }
        }

        private string lastName = "";
        public string LastName
        {
            get { return lastName; }
            set
            {
                lastName = value;
                OnPropertyChanged();
            }
        }

        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }

        private DateTime birthdate = DateTime.Now;
        public DateTime Birthdate
        {
            get { return birthdate; }
            set
            {
                birthdate = value;
                OnPropertyChanged();
            }
        }

        private string gender;
        public string Gender
        {
            get { return gender; }
            set
            {
                gender = value;
                OnPropertyChanged();
            }
        }

        //in centimeters
        private int height;
        public int Height
        {
            get { return height; }
            set
            {
                height = value;
                OnPropertyChanged();
            }
        }

        //in kilograms
        private int weight;
        public int Weight
        {
            get { return weight; }
            set
            {
                weight = value;
                OnPropertyChanged();
            }
        }

        private string playsWith;
        public string PlaysWith
        {
            get { return playsWith; }
            set
            {
                playsWith = value;
                OnPropertyChanged();
            }
        }

        private Country citizenship;
        public Country Citizenship
        {
            get { return citizenship; }
            set
            {
                citizenship = value;
                OnPropertyChanged();
            }
        }

        private string birthplaceCity;
        public string BirthplaceCity
        {
            get { return birthplaceCity; }
            set
            {
                birthplaceCity = value;
                OnPropertyChanged();
            }
        }

        private Country birthplaceCountry;
        public Country BirthplaceCountry
        {
            get { return birthplaceCountry; }
            set
            {
                birthplaceCountry = value;
                OnPropertyChanged();
            }
        }

        private bool status;
        public bool Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        public string StatusText { get { return Status ? "active" : "inactive"; } }


        private string info;
        public string Info
        {
            get { return info; }
            set
            {
                info = value;
                OnPropertyChanged();
            }
        }

        private string photoPath;
        public string PhotoPath
        {
            get { return photoPath; }
            set
            {
                photoPath = value;
                OnPropertyChanged();
            }
        }

        private IStats stats;
        public IStats Stats
        {
            get { return stats; }
            set
            {
                stats = value;
                OnPropertyChanged();
            }
        }
    }

    public class PlayerInMatchStats : NotifyPropertyChanged
    {
        #region Properties
        private DateTime datetime;
        public DateTime Datetime
        {
            get { return datetime; }
            set
            {
                datetime = value;
                OnPropertyChanged();
            }
        }

        private int goals = 0;
        public int Goals
        {
            get { return goals; }
            set
            {
                goals = value;
                OnPropertyChanged();
            }
        }

        private int assists = 0;
        public int Assists
        {
            get { return assists; }
            set
            {
                assists = value;
                OnPropertyChanged();
            }
        }

        private int points = 0;
        public int Points
        {
            get { return points; }
            set
            {
                points = value;
                OnPropertyChanged();
            }
        }

        private int penaltyMinutes = 0;
        public int PenaltyMinutes
        {
            get { return penaltyMinutes; }
            set
            {
                penaltyMinutes = value;
                OnPropertyChanged();
            }
        }
        #endregion
    }

    public static class PlayerInMatchStatsLoader
    {
        public static void LoadPlayerInMatchStats(ObservableCollection<PlayerInMatchStats> stats, string[] datetimeXLabels, Player player)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.SPORT.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT m.match_id, matches.datetime AS datetime, IFNULL(g_count, 0) AS goal_count, IFNULL(a_count, 0) AS assist_count, IFNULL(p_count, 0) AS penalty_count " +
                                                "FROM player_matches AS m " +

                                                "INNER JOIN matches ON matches.id = m.match_id " +

                                                "LEFT JOIN " +
                                                "(SELECT g.match_id AS g_match_id, COUNT(g.player_id) AS g_count FROM goals AS g " +
                                                "WHERE g.player_id = " + player.id + " " +
                                                "GROUP BY g.match_id) " +
                                                "AS g_table ON g_table.g_match_id = m.match_id " +

                                                "LEFT JOIN " +
                                                "(SELECT a.match_id AS a_match_id, COUNT(a.assist_player_id) AS a_count FROM goals AS a " +
                                                "WHERE a.assist_player_id = " + player.id + " " +
                                                "GROUP BY a.match_id) " +
                                                "AS a_table ON a_table.a_match_id = m.match_id " +

                                                "LEFT JOIN " +
                                                "(SELECT p.match_id AS p_match_id, COALESCE(SUM(p_type.minutes), 0) AS p_count FROM penalties AS p " +
                                                " INNER JOIN penalty_type AS p_type ON p_type.code = p.penalty_type_id " +
                                                "WHERE p.player_id = " + player.id + " " +
                                                "GROUP BY p.match_id) " +
                                                "AS p_table ON p_table.p_match_id = m.match_id " +

                                                "WHERE m.player_id = " + player.id + " " +
                                                "GROUP BY m.match_id", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    PlayerInMatchStats p = new PlayerInMatchStats
                    {
                        Datetime = DateTime.Parse(row["datetime"].ToString()),
                        Goals = int.Parse(row["goal_count"].ToString()),
                        Assists = int.Parse(row["assist_count"].ToString()),
                        PenaltyMinutes = int.Parse(row["penalty_count"].ToString()),
                        Points = int.Parse(row["goal_count"].ToString()) + int.Parse(row["assist_count"].ToString()),
                    };

                    stats.Add(p);
                }

                stats.OrderBy(x => x.Datetime);
                datetimeXLabels = stats.Select(x => x.Datetime.ToString("d. M. yyyy")).ToArray();
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
