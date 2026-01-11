using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Runtime.CompilerServices;


namespace Foundation.Scheduler.DatabaseUtility
{
    /// <summary>
    /// 
    /// The purpose of this is to add an interface to the .Net Core DBContext class to make it compatible with a structure from the Foundation EF library so that
    /// We can use standard code generation between the platforms.
    /// 
    /// </summary>
    public static class DBContextExtensions
    {
        public class DbContextConfiguration
        {
            private readonly DbContext _context;

            public DbContextConfiguration(DbContext context)
            {
                _context = context;
            }

            private bool _autoDetectChangesEnabled;
            public bool AutoDetectChangesEnabled
            {
                get => _autoDetectChangesEnabled;
                set
                {
                    _autoDetectChangesEnabled = value;
                    _context.ChangeTracker.AutoDetectChangesEnabled = value;
                }
            }

            //
            /// <summary>
            /// This does nothing but expose a property for compatibility purposes
            /// </summary>
            //
            private bool _validateOnSaveEnabled;
            public bool ValidateOnSaveEnabled
            {
                get => _validateOnSaveEnabled;
                set
                {
                    _validateOnSaveEnabled = value;
                }
            }
        }
    }



    public static class DatabaseExtensions
    {
        // Backing store for CommandTimeout values
        private static readonly ConditionalWeakTable<DatabaseFacade, CommandTimeoutStore> _commandTimeouts
            = new ConditionalWeakTable<DatabaseFacade, CommandTimeoutStore>();

        public static int? GetCommandTimeout(this DatabaseFacade database)
        {
            return _commandTimeouts.TryGetValue(database, out var store) ? store.CommandTimeout : null;
        }

        public static void SetCommandTimeout(this DatabaseFacade database, int? timeout)
        {
            var store = _commandTimeouts.GetOrCreateValue(database);
            store.CommandTimeout = timeout;

            // Apply the timeout if the database provider supports it
            if (database.IsSqlServer())
            {
                database.SetCommandTimeout(timeout);
            }
        }


        /// <summary>
        /// This puts a EF Framework execute sql command method onto the .Net Core EF model
        /// </summary>
        /// <param name="database"></param>
        /// <param name="sql"></param>
        public static void ExecuteSqlCommand(this DatabaseFacade database, string sql)
        {

            database.ExecuteSqlRaw(sql);

            return;
        }



        // Helper class to store the command timeout for each DatabaseFacade instance
        private class CommandTimeoutStore
        {
            public int? CommandTimeout { get; set; }
        }
    }
}