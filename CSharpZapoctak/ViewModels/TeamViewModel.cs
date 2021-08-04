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

        //           (competition_name)  (season_id)(season_name)
        private Dictionary<string, Dictionary<int, (string, PlayerEnlistment)>> competitionEnlistments;
        public Dictionary<string, Dictionary<int, (string, PlayerEnlistment)>> CompetitionEnlistments
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
            CompetitionEnlistments = new Dictionary<string, Dictionary<int, (string, PlayerEnlistment)>>();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT season_id, s.name AS season_name, c.name AS competition_name FROM team_enlistment " +
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
                    if (!CompetitionEnlistments.ContainsKey(row["competition_name"].ToString()))
                    {
                        CompetitionEnlistments.Add(row["competition_name"].ToString(), new Dictionary<int, (string, PlayerEnlistment)>());
                    }
                    CompetitionEnlistments[row["competition_name"].ToString()].Add(int.Parse(row["season_id"].ToString()), (row["season_name"].ToString(), new PlayerEnlistment()));
                }
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