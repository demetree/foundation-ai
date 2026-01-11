using Foundation.CodeGeneration;
using System;
using System.Data;


namespace Foundation
{
    /* 
     * 
     * The intention of this class is to provide tools to create a database from scratch, if possible, when starting a new system instance.  
     * 
     * It will use the connection string provided 
     *
     */
    public class DatabaseBuilder
    {
        public static bool CreateDatabase(DatabaseGenerator.DatabaseType databaseType, string connectionStringOrFileName, DatabaseGenerator.Database databaseDefinition)
        {
            //
            // Each database type is handled differently from a creation perspective.  Start wth SQLite, then add support for others as needs arise.
            //
            if (databaseType != DatabaseGenerator.DatabaseType.SQLite)
            {
                throw new Exception("Unhandled database type.");
            }

            //
            // Create the SQLite database if it does not already exist.
            //
            return CreateSQLiteDatabase(connectionStringOrFileName, databaseDefinition);
        }


        //
        // This will create the SQLite database and setup the tables in it if it does not already exist.
        //
        private static bool CreateSQLiteDatabase(string connectionStringOrFileName, DatabaseGenerator.Database databaseDefinition)
        {
            bool databaseCreated = false;
            //
            // Step 1 - Determine the file name from the connection string, and see if it exists, and then log the status to the debug console.
            //
            string SQLiteFile = null;
            string connectionString = null;

            if (connectionStringOrFileName != null && connectionStringOrFileName.Contains("data source=") == true)
            {
                connectionString = connectionStringOrFileName;

                string[] connectionStringData = connectionStringOrFileName.Split(';');

                foreach (string connectionStringValue in connectionStringData)
                {
                    if (connectionStringValue.StartsWith("data source=") == true)
                    {
                        SQLiteFile = connectionStringValue.Substring("data source=".Length);

                        break;
                    }
                }
            }
            else
            {
                SQLiteFile = connectionStringOrFileName;
                connectionString = "data source=" + connectionStringOrFileName;
            }


            if (SQLiteFile == null)
            {
                throw new Exception("Cannot find 'data source=' tag in connection string of " + connectionStringOrFileName);
            }


            if (SQLiteFile.Contains("|DataDirectory|") == true)
            {
                string dataDirectoryValue = Foundation.StartupBasics.GetDataDirectory();

                SQLiteFile = SQLiteFile.Replace("|DataDirectory|", dataDirectoryValue);

                if (System.IO.File.Exists(SQLiteFile) == true)
                {
                    System.Diagnostics.Debug.WriteLine("SQLite file '" + SQLiteFile + "' exists.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("SQLite file '" + SQLiteFile + "' does not exist.");
                }
            }


            if (connectionString.Contains("|DataDirectory|") == true)
            {
                string dataDirectoryValue = Foundation.StartupBasics.GetDataDirectory();

                connectionString = connectionString.Replace("|DataDirectory|", dataDirectoryValue);
            }


            //
            // Now we have the file name.  Open the connection using the connection string.  This will create the SQLite file if it doesn't exist, but it won't create missig directories.  The directory struture must exist or this will fail.
            //
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);

            connection.Open();

            //
            // Check for the existence of any tables.  If none exist, then create the schema.  This does not version check or try to make any adjustments.  If any tables exist, then it assumes they are current.
            //
            string tableCheckSQL = "SELECT * FROM sqlite_master WHERE type='table'";
            int tableCount = 0;

            //DataSet ds = new DataSet();

            //Microsoft.Data.Sqlite.SqliteDataAdapter da = new Microsoft.Data.Sqlite.SqliteDataAdapter(tableCheckSQL, connection);

            //da.Fill(ds);
            //DataTable dt = ds.Tables[0];

            //tableCount = dt.Rows.Count;

            using var cmd = new Microsoft.Data.Sqlite.SqliteCommand(tableCheckSQL, connection);
            using var reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);

            tableCount = dt.Rows.Count;

            //
            // If we have no tables, then we have an empty database.  Create the schema.
            //
            if (tableCount == 0)
            {
                // We don't want th attach line or any comments
                databaseDefinition.disableDatabaseFileCreation = true;  // we don't want the initialization to attach/create a database because the file already exists
                databaseDefinition.disableComments = true;              // no need for comments in this mode since no one will ever read them.
                databaseDefinition.disableSchemaName = true;            // we don't want the database prefix on the scripts because we didn't start with an attach that gave it one.

                string schemaCreationSQL = databaseDefinition.CreateSQL(DatabaseGenerator.DatabaseType.SQLite);


                Microsoft.Data.Sqlite.SqliteCommand createCmd = connection.CreateCommand();
                createCmd.CommandText = schemaCreationSQL;
                createCmd.ExecuteNonQuery();

                databaseCreated = true;
            }

            connection.Close();

            return databaseCreated;
        }

        public static void CreateSecurityConfiguration(DatabaseGenerator.DatabaseType databaseType, string moduleName, string securityConnectionString, DatabaseGenerator.Database databaseDefinition)
        {
            //
            // Each database type is handled differently from a creation perspective.  Start wth SQLite, then add support for others as needs arise.
            //
            if (databaseType != DatabaseGenerator.DatabaseType.SQLite)
            {
                throw new Exception("Unhandled database type.");
            }

            //
            // Create the security configuration in a SQLite security database.
            //
            CreateSQLiteSecurityConfiguration(securityConnectionString, moduleName, databaseDefinition);

            return;
        }

        private static void CreateSQLiteSecurityConfiguration(string securityConnectionString, string moduleName, DatabaseGenerator.Database databaseDefinition)
        {
            string SQLiteFile = null;

            string[] connectionStringData = securityConnectionString.Split(';');

            foreach (string connectionStringValue in connectionStringData)
            {
                if (connectionStringValue.StartsWith("data source=") == true)
                {
                    SQLiteFile = connectionStringValue.Substring("data source=".Length);
                    break;
                }
            }


            if (SQLiteFile == null)
            {
                throw new Exception("Cannot find 'data source=' tag in connection string of " + securityConnectionString);
            }


            if (SQLiteFile.Contains("|DataDirectory|") == true)
            {
                string dataDirectoryValue = Foundation.StartupBasics.GetDataDirectory();

                SQLiteFile = SQLiteFile.Replace("|DataDirectory|", dataDirectoryValue);

                if (System.IO.File.Exists(SQLiteFile) == false)
                {
                    throw new Exception("SQLite file '" + SQLiteFile + "' does not exist.");
                }
            }


            //
            // Now we have the file name.  Open the connection using the connection string.  This will create the SQLite file if it doesn't exist.
            //
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(securityConnectionString);

            connection.Open();


            // no need for comments in this mode since no one will ever read them.
            // we don't want the database prefix on the scripts because we didn't start with an attach that gave it one.
            databaseDefinition.disableSchemaName = true;
            databaseDefinition.disableComments = true;

            string schemaCreationSQL = databaseDefinition.CreateSecurityConfigurationSQL(moduleName, DatabaseGenerator.DatabaseType.SQLite, true, null, true);


            Microsoft.Data.Sqlite.SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = schemaCreationSQL;
            cmd.ExecuteNonQuery();

            connection.Close();
        }
    }
}
