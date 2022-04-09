using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace CSharpZapoctak.Others
{
    public static class DatabaseHandler
    {
        /// <summary>
        /// Creates SQL WHERE statement based on selected competition and season.
        /// </summary>
        /// <param name="startWithWhere">Start query with WHERE. Else start with AND.</param>
        /// <param name="seasonID">Name of season ID column.</param>
        /// <param name="competitionID">Name of competition ID column.</param>
        /// <returns>String of WHERE statement. With WHERE or without it as a concatenation beginning with AND.</returns>
        public static string WhereSeasonCompetitionQuery(bool startWithWhere, string seasonID, string competitionID)
        {
            string query = "";

            if (startWithWhere)
            {
                if (SportsData.IsSeasonSet() || SportsData.IsCompetitionSet()) { query += " WHERE"; }
                if (SportsData.IsSeasonSet()) { query += " " + seasonID + " = " + SportsData.SEASON.ID; }
                if (SportsData.IsSeasonSet() && SportsData.IsCompetitionSet()) { query += " AND"; }
            }
            else
            {
                if (SportsData.IsSeasonSet()) { query += " AND " + seasonID + " = " + SportsData.SEASON.ID; }
                if (SportsData.IsSeasonSet() || SportsData.IsCompetitionSet()) { query += " AND"; }
            }

            if (SportsData.IsCompetitionSet()) { query += " " + competitionID + " = " + SportsData.COMPETITION.ID; }

            return query;
        }

        public static void CreateDatabase(string command)
        {

        }

        public static bool CheckDatabase(string databaseName)
        {
            return false;
        }

        public static void EnsureDatabases()
        {
            MySqlConnection connection = new(SportsData.ConnectionStringNoDatabase);
            MySqlCommand cmd = new("CREATE DATABASE IF NOT EXISTS `sports_manager`;" +
                                   "CREATE DATABASE IF NOT EXISTS `floorball`;" +
                                   "CREATE DATABASE IF NOT EXISTS `ice_hockey`;"
                                   , connection);

            try
            {
                connection.Open();
                _ = cmd.ExecuteNonQuery();
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