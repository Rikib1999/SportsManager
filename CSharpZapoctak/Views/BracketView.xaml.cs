using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;

namespace CSharpZapoctak.Views
{
    /// <summary>
    /// Interaction logic for BracketView.xaml
    /// </summary>
    public partial class BracketView : UserControl
    {
        DataTable dataTable = new DataTable();
        
        DockPanel dockPanel = new DockPanel();
        List<TextBlock> homeCompetitors = new List<TextBlock>();
        List<TextBlock> awayCompetitors = new List<TextBlock>();
        List<TextBlock> homeCompetitorsScores = new List<TextBlock>();
        List<TextBlock> awayCompetitorsScores = new List<TextBlock>();

        public BracketView(string partOfSeason, int rounds)
        {
            InitializeComponent();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);

            string querry;
            if (SportsData.sport.name == "tennis")
            {
                querry = @"SELECT a.first_name AS home_first_name
                                    ,a.last_name AS home_first_name
	                                ,matches.home_score
                                    ,matches.away_score
                                    , b.first_name AS away_first_name
                                    ,b.last_name AS away_first_name
                                    FROM matches WHERE season_id = " + SportsData.season.id + " AND part_of_season = '" + partOfSeason + @"'
                                    INNER JOIN player as a ON a.id = matches.home_competitor 
                                    INNER JOIN player as b ON b.id = matches.away_competitor";
            }
            else
            {
                querry = @"SELECT a.name AS home_competitor
	                                ,matches.home_score
                                    ,matches.away_score
                                    , b.name AS away_competitor
                                    FROM matches WHERE season_id = " + SportsData.season.id + " AND part_of_season = '" + partOfSeason + @"'
                                    INNER JOIN team as a ON a.id = matches.home_competitor 
                                    INNER JOIN team as b ON b.id = matches.away_competitor";
            }

            MySqlCommand cmd = new MySqlCommand(querry, connection);

            connection.Open();
            dataTable.Load(cmd.ExecuteReader());
            connection.Close();

            int matches_count = (int)Math.Pow(2, rounds);

            for (int i = 0; i < matches_count; i++)
            {
                homeCompetitors.Add(new TextBlock());
                awayCompetitors.Add(new TextBlock());
                homeCompetitorsScores.Add(new TextBlock());
                awayCompetitorsScores.Add(new TextBlock());
            }

            if (SportsData.sport.name == "tennis")
            {
                foreach (DataRow match in dataTable.Rows)
                {
                    homeCompetitors[int.Parse(match["bracket_index"].ToString())].Text = match["home_first_name"].ToString() + match["home_last_name"].ToString();
                    homeCompetitors[int.Parse(match["bracket_index"].ToString())].Text = match["away_first_name"].ToString() + match["away_last_name"].ToString();
                    homeCompetitorsScores[int.Parse(match["bracket_index"].ToString())].Text = match["home_score"].ToString();
                    awayCompetitorsScores[int.Parse(match["bracket_index"].ToString())].Text = match["away_score"].ToString();
                }
            }
            else
            {
                foreach (DataRow match in dataTable.Rows)
                {
                    homeCompetitors[int.Parse(match["bracket_index"].ToString())].Text = match["home_competitor"].ToString();
                    homeCompetitors[int.Parse(match["bracket_index"].ToString())].Text = match["away_competitor"].ToString();
                    homeCompetitorsScores[int.Parse(match["bracket_index"].ToString())].Text = match["home_score"].ToString();
                    awayCompetitorsScores[int.Parse(match["bracket_index"].ToString())].Text = match["away_score"].ToString();
                }
            }

            AddChild(dockPanel);
            dockPanel.LastChildFill = false;

            for (int i = rounds; i > 0 ; i--)
            {
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Vertical;

                for (int j = matches_count - 1; j >= 0; j--)
                {
                    Grid g = new Grid();
                    g.ColumnDefinitions.Add(new ColumnDefinition()/*.Width(45)*/);
                    g.ColumnDefinitions.Add(new ColumnDefinition());
                    g.RowDefinitions.Add(new RowDefinition());
                    g.RowDefinitions.Add(new RowDefinition());

                    Grid.SetColumn(homeCompetitors[j], 0);
                    Grid.SetRow(homeCompetitors[j], 0);
                    g.Children.Add(homeCompetitors[j]);

                    Grid.SetColumn(awayCompetitors[j], 0);
                    Grid.SetRow(awayCompetitors[j], 1);
                    g.Children.Add(awayCompetitors[j]);

                    Grid.SetColumn(homeCompetitorsScores[j], 1);
                    Grid.SetRow(homeCompetitorsScores[j], 0);
                    g.Children.Add(homeCompetitorsScores[j]);

                    Grid.SetColumn(awayCompetitorsScores[j], 1);
                    Grid.SetRow(awayCompetitorsScores[j], 1);
                    g.Children.Add(awayCompetitorsScores[j]);

                    sp.Children.Add(g);
                }

                DockPanel.SetDock(sp, Dock.Right);
                dockPanel.Children.Add(sp);
            }
        }
    }
}
