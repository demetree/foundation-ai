using System;
using System.Configuration;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static Foundation.DatabaseUtility.DBContextExtensions;

namespace Foundation.Community.Database
{

    /// <summary>
    /// 
    /// This is an extension class to the BMC Context that is separated from the main BMCContext.cs so that it won't be clobbered if the EF Core Power tools
    /// are used to rebuild the BMC entity classes
    /// 
    /// </summary>
    public partial class CommunityContext : DbContext
    {
        public DbContextConfiguration _configuration;
        public DbContextConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = new DbContextConfiguration(this);
                }

                return _configuration;
            }
        }

        int commandTimeoutSeconds = 30;

        public CommunityContext()
        {
            this.ChangeTracker.LazyLoadingEnabled = false;
        }

        public CommunityContext(int commandTimeoutSeconds)
        {
            this.commandTimeoutSeconds = commandTimeoutSeconds;
            this.ChangeTracker.LazyLoadingEnabled = false;
        }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationBuilder builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");

                // Check for a custom or standard environment variable
                if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development" ||
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    builder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
                }

                IConfigurationRoot configuration = builder.Build();
                string connectionString = configuration.GetConnectionString("Community");
                optionsBuilder.UseSqlServer(connectionString)
                              .UseLazyLoadingProxies(false);

                if (connectionString != null)
                {
                    optionsBuilder.UseSqlServer(connectionString, opts => opts.CommandTimeout(commandTimeoutSeconds))
                                  .UseLazyLoadingProxies(false);
                }
                else
                {
                    throw new Exception("Unable to read Community connection string from the appSettings.json file");
                }
            }

            return;
        }



        // Custom constructor for SQL logging with custom ILogger to write to
        public CommunityContext(ILogger logger, LogLevel level)
            : base(CreateOptionsWithDebugLogging(logger, level))
        {
        }



        private static DbContextOptions<CommunityContext> CreateOptionsWithDebugLogging(ILogger logger, LogLevel level)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CommunityContext>();

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                    .AddJsonFile("appsettings.json");

            // Check for a custom or standard environment variable
            if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development" ||
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                builder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            }

            builder.Build();

            IConfigurationRoot configuration = builder.Build();
            var connectionString = configuration.GetConnectionString("Community");

            optionsBuilder.UseSqlServer(connectionString)
                          .EnableSensitiveDataLogging()
                          .LogTo((message) => logger.LogInformation(message), level);       // all messages logged to our logger at the info level

            return optionsBuilder.Options;
        }


        public bool DoesDatabaseStoreDateWithTimeZone()
        {
            if (this.Database.GetDbConnection().GetType().ToString().Contains("SQLite") == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        /// <summary>
        /// Partial OnModelCreating extension for manually-managed entities.
        /// Called by EF Core alongside the auto-generated OnModelCreating.
        /// </summary>
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
        }
    }
}
