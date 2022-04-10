using MySql.Data.MySqlClient;
using System;

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

        /// <summary>
        /// Checks if required tables exists and if not creates them.
        /// </summary>
        public static void EnsureTables()
        {
            //check common tables
            MySqlConnection connection = new(SportsData.ConnectionStringCommon);
            MySqlCommand cmd = new(Properties.Resources.sports_manager_tables, connection);

            try
            {
                connection.Open();
                _ = cmd.ExecuteNonQuery();
            }
            catch (Exception) { }
            finally
            {
                connection.Close();
            }

            //check tables for each sport
            foreach (Sport s in SportsData.SportsList)
            {
                SportsData.Set(s);
                connection = new(SportsData.ConnectionStringSport);
                cmd = new(s.DatabaseTables, connection);

                try
                {
                    connection.Open();
                    _ = cmd.ExecuteNonQuery();
                }
                catch (Exception) { }
                finally
                {
                    connection.Close();
                }
            }

            SportsData.Set(new Sport { Name = "" });
        }

        /// <summary>
        /// Checks if required databases exists and if not creates them.
        /// </summary>
        public static void EnsureDatabases()
        {
            string command = "CREATE DATABASE IF NOT EXISTS `" + SportsData.commonDatabaseName + "`;";
            foreach (Sport s in SportsData.SportsList)
            {
                command += "CREATE DATABASE IF NOT EXISTS `" + s.Name + "`;";
            }

            MySqlConnection connection = new(SportsData.ConnectionStringNoDatabase);
            MySqlCommand cmd = new(command, connection);

            try
            {
                connection.Open();
                _ = cmd.ExecuteNonQuery();
            }
            catch (Exception) { }
            finally
            {
                connection.Close();
            }
        }
    }
}