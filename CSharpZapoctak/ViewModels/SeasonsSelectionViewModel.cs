using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    public class SeasonsSelectionViewModel : TemplateSelectionDataGridViewModel<Season>
    {
        public ICommand NavigateAddSeasonCommand { get; set; }

        private ICommand checkNavigateEntityCommand;
        public new ICommand CheckNavigateEntityCommand
        {
            get
            {
                if (checkNavigateEntityCommand == null)
                {
                    checkNavigateEntityCommand = new RelayCommand(param => CheckNavigateEntity(SelectedEntity));
                }
                return checkNavigateEntityCommand;
            }
        }

        public SeasonsSelectionViewModel(NavigationStore navigationStore)
        {
            NavigateAddSeasonCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new AddSeasonViewModel(navigationStore)));
            NavigateEntityCommand = new NavigateCommand<SportViewModel>(navigationStore, () => new SportViewModel(navigationStore, new SeasonViewModel(navigationStore)));
            LoadData();
        }

        protected override void LoadData()
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT w.name AS winner, winner_id, c.name AS competition_name, seasons.id, competition_id, seasons.name, seasons.info, qualification_count, " +
                                                "qualification_rounds, group_count, play_off_rounds, play_off_best_of, play_off_started, points_for_W, points_for_OW, points_for_T, points_for_OL, points_for_L " +
                                                "FROM seasons " +
                                                "INNER JOIN competitions AS c ON c.id = seasons.competition_id " +
                                                "INNER JOIN team AS w ON w.id = seasons.winner_id", connection);
            if (SportsData.IsCompetitionSet())
            {
                cmd.CommandText += " WHERE competition_id = " + SportsData.COMPETITION.ID;
            }

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                Entities = new ObservableCollection<Season>();

                foreach (DataRow row in dataTable.Rows)
                {
                    Competition c = new();
                    c.Name = row["competition_name"].ToString();
                    c.ID = int.Parse(row["competition_id"].ToString());

                    Season s = new()
                    {
                        ID = int.Parse(row["id"].ToString()),
                        Competition = c,
                        Name = row["name"].ToString(),
                        Info = row["info"].ToString(),
                        QualificationCount = int.Parse(row["qualification_count"].ToString()),
                        QualificationRounds = int.Parse(row["qualification_rounds"].ToString()),
                        GroupCount = int.Parse(row["group_count"].ToString()),
                        PlayOffRounds = int.Parse(row["play_off_rounds"].ToString()),
                        PlayOffBestOf = int.Parse(row["play_off_best_of"].ToString()),
                        WinnerName = row["winner"].ToString(),
                        WinnerID = int.Parse(row["winner_id"].ToString()),
                        PlayOffStarted = Convert.ToBoolean(int.Parse(row["play_off_started"].ToString())),
                        PointsForWin = int.Parse(row["points_for_W"].ToString()),
                        PointsForOTWin = int.Parse(row["points_for_OW"].ToString()),
                        PointsForTie = int.Parse(row["points_for_T"].ToString()),
                        PointsForOTLoss = int.Parse(row["points_for_OL"].ToString()),
                        PointsForLoss = int.Parse(row["points_for_L"].ToString())
                    };
                    string[] imgPath = System.IO.Directory.GetFiles(SportsData.SeasonLogosPath, SportsData.SPORT.Name + row["id"].ToString() + ".*");
                    if (imgPath.Length != 0)
                    {
                        s.ImagePath = imgPath.First();
                    }

                    s.Stats = new SeasonStats(s); ;
                    Entities.Add(s);
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