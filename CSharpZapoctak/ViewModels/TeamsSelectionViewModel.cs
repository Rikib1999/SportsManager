using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Others;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class TeamsSelectionViewModel : ViewModelBase
    {
        public class TeamStats : TeamTableStats
        {
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

            private string dateOfCreation;
            public string DateOfCreation
            {
                get { return dateOfCreation; }
                set
                {
                    dateOfCreation = value;
                    OnPropertyChanged();
                }
            }

            private string status;
            public string Status
            {
                get { return status; }
                set
                {
                    status = value;
                    OnPropertyChanged();
                }
            }

            public TeamStats(Team t, string status)
            {
                Status = status;
                DateOfCreation = t.DateOfCreation.ToShortDateString();
                CalculateStats(t.id).Await();
            }

            public new async Task CalculateStats(int teamID)
            {
                List<Task> tasks = new List<Task>();
                tasks.Add(Task.Run(() => CountMatches(teamID)));
                tasks.Add(Task.Run(() => CountGoals(teamID)));
                tasks.Add(Task.Run(() => CountAssists(teamID)));
                tasks.Add(Task.Run(() => CountGoalsAgainst(teamID)));
                tasks.Add(Task.Run(() => CountPenaltyMinutes(teamID)));
                await Task.WhenAll(tasks);
                GoalDifference = Goals - GoalsAgainst;
            }

            private async Task CountAssists(int teamID)
            {
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM goals " +
                                                    "INNER JOIN matches AS m ON m.id = match_id " +
                                                    "WHERE assist_player_id <> -1 AND team_id = " + teamID, connection);
                if (SportsData.season.id > 0) { cmd.CommandText += " AND m.season_id = " + SportsData.season.id; }
                try
                {
                    connection.Open();
                    Assists = (int)(long)await cmd.ExecuteScalarAsync();
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

        public ICommand NavigateTeamCommand { get; set; }

        private ICommand checkNavigateTeamCommand;
        public ICommand CheckNavigateTeamCommand
        {
            get
            {
                if (checkNavigateTeamCommand == null)
                {
                    checkNavigateTeamCommand = new RelayCommand(param => CheckNavigateTeam());
                }
                return checkNavigateTeamCommand;
            }
        }

        public Team SelectedTeam { get; set; }

        public ObservableCollection<Team> Teams { get; set; }

        public TeamsSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateTeamCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new TeamViewModel(navigationStore, SelectedTeam)));
            SelectedTeam = null;

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT team_id, t.name AS team_name, t.status AS status, t.country AS country, t.date_of_creation AS date_of_creation, " +
                                                "season_id, s.competition_id AS competition_id " +
                                                "FROM team_enlistment " +
                                                "INNER JOIN team AS t ON t.id = team_id " +
                                                "INNER JOIN seasons AS s ON s.id = season_id", connection);
            cmd.CommandText += " WHERE team_id <> -1";
            if (SportsData.competition.id != (int)EntityState.NotSelected && SportsData.competition.id != (int)EntityState.AddNew)
            {
                cmd.CommandText += " AND competition_id = " + SportsData.competition.id;
                if (SportsData.season.id != (int)EntityState.NotSelected && SportsData.season.id != (int)EntityState.AddNew)
                {
                    cmd.CommandText += " AND season_id = " + SportsData.season.id;
                }
            }
            cmd.CommandText += " GROUP BY team_id";

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                Teams = new ObservableCollection<Team>();

                //logo      name      status      country     dateOfCreation      GP  W   WO  T   LO  L   G   GA  GD  A   PM  
                foreach (DataRow row in dataTable.Rows)
                {
                    Team t = new Team
                    {
                        id = int.Parse(row["team_id"].ToString()),
                        Name = row["team_name"].ToString(),
                        Status = bool.Parse(row["status"].ToString()),
                        Country = new Country { CodeTwo = row["country"].ToString() },
                        DateOfCreation = DateTime.Parse(row["date_of_creation"].ToString())
                    };

                    string[] imgPath = System.IO.Directory.GetFiles(SportsData.TeamLogosPath, SportsData.sport.name + t.id + ".*");
                    if (imgPath.Length != 0)
                    {
                        t.LogoPath = imgPath.First();
                    }

                    string status = "inactive";
                    if (t.Status) { status = "active"; }

                    t.Stats = new TeamStats(t, status);

                    Teams.Add(t);
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

        private void CheckNavigateTeam()
        {
            if (SelectedTeam != null)
            {
                NavigateTeamCommand.Execute(null);
            }
        }
    }
}