using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class PlayerEnlistment
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public int Number { get; set; }
    }

    class TeamViewModel : ViewModelBase
    {
        public Team CurrentTeam { get; set; }

        public ICommand NavigateEditTeamCommand { get; }

        //           (competition_name)  (season_id)(season_name)  (players info)
        private Dictionary<string, Dictionary<int, Tuple<string, List<PlayerEnlistment>>>> competitionEnlistments;
        public Dictionary<string, Dictionary<int, Tuple<string, List<PlayerEnlistment>>>> CompetitionEnlistments
        {
            get { return competitionEnlistments; }
            set { competitionEnlistments = value; }
        }

        public TeamViewModel(NavigationStore navigationStore, Team t)
        {
            NavigateEditTeamCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new EditTeamViewModel(navigationStore, CurrentTeam)));
            CurrentTeam = t;
            CurrentTeam.Country = SportsData.countries.Where(x => x.CodeTwo == CurrentTeam.Country.CodeTwo).First();
            LoadTeamInfo();
            LoadEnlistments();
        }

        private void LoadTeamInfo()
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT info FROM team WHERE id = " + CurrentTeam.id, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                CurrentTeam.Info = dataTable.Rows[0]["info"].ToString();
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

        private void LoadEnlistments()
        {
            CompetitionEnlistments = new Dictionary<string, Dictionary<int, Tuple<string, List<PlayerEnlistment>>>>();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT season_id, number, p.first_name AS player_first_name, p.last_name AS player_last_name, pos.name AS position, s.name AS season_name, c.name AS competition_name FROM player_enlistment " +
                                                "INNER JOIN player AS p ON p.id = player_id " +
                                                "INNER JOIN position AS pos ON pos.code = position_code " +
                                                "INNER JOIN seasons AS s ON s.id = season_id " +
                                                "INNER JOIN competitions AS c ON c.id = s.competition_id " +
                                                "WHERE team_id = " + CurrentTeam.id, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    string competition = row["competition_name"].ToString();
                    int seasonID = int.Parse(row["season_id"].ToString());

                    if (!CompetitionEnlistments.ContainsKey(competition))
                    {
                        CompetitionEnlistments.Add(competition, new Dictionary<int, Tuple<string, List<PlayerEnlistment>>>());
                    }
                    if (!CompetitionEnlistments[competition].ContainsKey(seasonID))
                    {
                        CompetitionEnlistments[competition].Add(seasonID, Tuple.Create(row["season_name"].ToString(), new List<PlayerEnlistment>()));
                    }
                    CompetitionEnlistments[competition][seasonID].Item2.Add(new PlayerEnlistment
                    {
                        Name = row["player_first_name"].ToString() + " " + row["player_last_name"].ToString(),
                        Number = int.Parse(row["number"].ToString()),
                        Position = row["position"].ToString()
                    });
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to connect to databse.\n" + e.Message, "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}