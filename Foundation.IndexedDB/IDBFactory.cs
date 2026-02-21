using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;


namespace Foundation.IndexedDB
{
    public class IDBFactory
    {
        private readonly string _basePath;


        public IDBFactory(string basePath = null)
        {
            _basePath = basePath ?? Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "";
        }

        /// <summary>
        /// 
        /// Opens the named database, creating it if it doesn't exist. If the version is higher than the existing version (or no version exists),
        /// the upgrade action is invoked. Use the upgrade event action handler to create object stores and indexes as needed.
        /// 
        /// </summary>
        /// <param name="name">The name of the database (must be a valid file name).</param>
        /// <param name="version">The target version (optional; defaults to current or 1 if new).</param>
        /// <returns>An IDBOpenDBRequest that resolves to the opened database.</returns>
        public async Task<IDBOpenDBRequest> OpenAsync(string name, 
                                                      uint? version = null,
                                                      Action<IDBDatabase, uint, uint> upgradeNeededHandler = null)
        {
            if (string.IsNullOrWhiteSpace(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("Invalid database name.", nameof(name));
            }

            IDBOpenDBRequest openDbRequest = new IDBOpenDBRequest();

            try
            {
                // Build the path to the backing SQLite file
                string dbPath = Path.Combine(_basePath, "IndexedDB", $"{name}.sqlite");

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath));


                // Configure EF with SQLite; enable logging if needed (e.g., via constructor injection in future).
                DbContextOptionsBuilder<IDBContext> optionsBuilder = new DbContextOptionsBuilder<IDBContext>().UseSqlite($"Data Source={dbPath}")
                                                                                                              .UseLazyLoadingProxies(false);
                      
                // This turns on debug logging to the console.
                      // .EnableSensitiveDataLogging()
                      //.LogTo((message) => Console.WriteLine(message), Microsoft.Extensions.Logging.LogLevel.Debug);


                //
                // Turn on write ahead logging in SQLite
                //
                optionsBuilder.AddInterceptors(new Utility.SqliteWALModeInterceptor());


                IDBContext context = new IDBContext(optionsBuilder.Options);

                await context.Database.EnsureCreatedAsync().ConfigureAwait(false);  // Creates tables if missing.

                // Retrieve current version from metadata (default to 0 if absent).
                string currentVersionString = await context.Metadata.Where(m => m.Key == "version")
                                                                    .Select(m => m.Value)
                                                                    .FirstOrDefaultAsync().ConfigureAwait(false) ?? "0";

                uint currentVersion = uint.Parse(currentVersionString);
                uint targetVersion = version ?? (currentVersion == 0 ? 1 : currentVersion);

                if (targetVersion < currentVersion)
                {
                    throw new InvalidOperationException("Cannot downgrade database version.");
                }

                //
                // Note that the ownership and disposal of the context is handled by the database class.
                //
                IDBDatabase db = new IDBDatabase(context, name);

                //
                // Invoke the upgrade handler if needed.  This will both fire the first time the database is needed, and also any time an upgrade is needed.
                //
                if (targetVersion > currentVersion)
                {
                    // Directly invoke the handler if provided
                    upgradeNeededHandler?.Invoke(db, currentVersion, targetVersion);

                    await db.UpdateMetaAsync("version", targetVersion.ToString()).ConfigureAwait(false);
                }

                await db.LoadConfigsAsync().ConfigureAwait(false);  // Load store configurations after potential upgrade.

                openDbRequest.SetResult(db);
            }
            catch (Exception ex)
            {
                openDbRequest.SetError(ex);
            }

            return openDbRequest;
        }


        /// <summary>
        /// 
        /// Delete the named database.
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IDBRequest<object> DeleteDatabase(string name)
        {
            IDBRequest<object> request = new IDBRequest<object>();

            string dbPath = Path.Combine(_basePath, "IndexedDB", $"{name}.sqlite");

            if (System.IO.File.Exists(dbPath) == true)
            {
                try
                {
                    //
                    // Clear the SQLite connection pool for this database before deleting files.
                    //
                    // EF Core's SQLite provider pools connections by default, so even after disposing 
                    // the IDBContext, the underlying file handle may still be held by a pooled connection.
                    // Creating a temporary connection with the same connection string and calling ClearPool 
                    // forces all pooled connections for this file to be fully closed and released.
                    //
                    string connectionString = $"Data Source={dbPath}";
                    using (SqliteConnection tempConnection = new SqliteConnection(connectionString))
                    {
                        SqliteConnection.ClearPool(tempConnection);
                    }

                    System.IO.File.Delete(dbPath);

                    //
                    // Clean up WAL mode sidecar files
                    //
                    string walPath = dbPath + "-wal";
                    string shmPath = dbPath + "-shm";

                    if (System.IO.File.Exists(walPath) == true)
                    {
                        System.IO.File.Delete(walPath);
                    }

                    if (System.IO.File.Exists(shmPath) == true)
                    {
                        System.IO.File.Delete(shmPath);
                    }
                }
                catch (Exception ex)
                {
                    request.SetError(ex);
                    return request;
                }
            }

            request.SetResult(null);

            return request;
        }
   }
}