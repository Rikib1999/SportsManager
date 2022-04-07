using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace CSharpZapoctak.ViewModels
{
    public class GoaliesSelectionViewModel : TemplateSelectionDataGridViewModel<Player>
    {
        public GoaliesSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateEntityCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new PlayerViewModel(navigationStore, SelectedEntity)));
            LoadData();
        }

        protected override void LoadData()
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT p.* " +
                                   "FROM goalie_matches " +
                                   "INNER JOIN player AS p ON p.id = player_id " +
                                   "INNER JOIN matches AS m ON m.id = match_id " +
                                   "INNER JOIN seasons AS s ON s.id = m.season_id ", connection);
            cmd.CommandText += " WHERE player_id <> -1";
            if (SportsData.IsCompetitionSet())
            {
                cmd.CommandText += " AND s.competition_id = " + SportsData.COMPETITION.ID;
                if (SportsData.IsSeasonSet())
                {
                    cmd.CommandText += " AND m.season_id = " + SportsData.SEASON.ID;
                }
            }
            cmd.CommandText += " GROUP BY player_id";

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                Entities = new ObservableCollection<Player>();

                foreach (DataRow row in dataTable.Rows)
                {
                    Player p = new()
                    {
                        ID = int.Parse(row["id"].ToString()),
                        FirstName = row["first_name"].ToString(),
                        LastName = row["last_name"].ToString(),
                        Birthdate = DateTime.Parse(row["birthdate"].ToString()),
                        Gender = row["gender"].ToString(),
                        Height = int.Parse(row["height"].ToString()),
                        Weight = int.Parse(row["weight"].ToString()),
                        PlaysWith = row["plays_with"].ToString(),
                        Citizenship = new Country { CodeTwo = row["citizenship"].ToString() },
                        BirthplaceCity = row["birthplace_city"].ToString(),
                        BirthplaceCountry = new Country { CodeTwo = row["birthplace_country"].ToString() },
                        Status = Convert.ToBoolean(int.Parse(row["status"].ToString())),
                        Info = row["info"].ToString()
                    };

                    string[] imgPath = System.IO.Directory.GetFiles(SportsData.PlayerPhotosPath, SportsData.SPORT.Name + p.ID + ".*");
                    p.ImagePath = imgPath.Length != 0
                        ? imgPath.First()
                        : p.Gender == "M" ? SportsData.ResourcesPath + "\\male.png" : SportsData.ResourcesPath + "\\female.png";

                    p.Stats = new GoalieStats(p, SportsData.SEASON.ID, SportsData.COMPETITION.ID);

                    Entities.Add(p);
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