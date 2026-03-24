using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace Foundation.Messaging.Database
{

    /// <summary>
    /// 
    /// Custom partial of the MessagingContext that handles connection string resolution and schema configuration.
    /// 
    /// This follows the SecurityContextCustom pattern — the context reads its connection string from
    /// appsettings.json using the "Messaging" key, falling back to "DefaultConnection" if not present.
    /// 
    /// The SchemaName property allows each module to specify its own schema (e.g., "Catalyst", "Basecamp").
    /// 
    /// </summary>
    public partial class MessagingContext : DbContext
    {
        int commandTimeoutSeconds = 60;


        /// <summary>
        /// The database schema name to use for the messaging tables.
        /// Set by the module at startup (e.g., "Catalyst", "Basecamp", "Scheduler").
        /// </summary>
        public static string SchemaName { get; set; } = "dbo";


        public MessagingContext()
        {

        }

        public MessagingContext(int commandTimeoutSeconds)
        {
            this.commandTimeoutSeconds = commandTimeoutSeconds;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var builder = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("appsettings.json");

                // Check for a custom or standard environment variable
                if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development" ||
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    builder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
                }

                IConfigurationRoot configuration = builder.Build();

                //
                // Try "Messaging" key first, fall back to "DefaultConnection"
                //
                var connectionString = configuration.GetConnectionString("Messaging")
                                    ?? configuration.GetConnectionString("DefaultConnection");

                if (connectionString != null)
                {
                    optionsBuilder.UseSqlServer(connectionString, opts => opts.CommandTimeout(commandTimeoutSeconds))
                                  .UseLazyLoadingProxies(false);
                }
                else
                {
                    throw new Exception("Unable to read Messaging or DefaultConnection connection string from the appSettings.json file");
                }
            }

            return;
        }
    }
}
