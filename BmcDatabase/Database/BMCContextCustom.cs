using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using static Foundation.DatabaseUtility.DBContextExtensions;

namespace Foundation.BMC.Database
{

    /// <summary>
    /// 
    /// This is an extension class to the BMC Context that is separated from the main BMCContext.cs so that it won't be clobbered if the EF Core Power tools
    /// are used to rebuild the BMC entity classes
    /// 
    /// </summary>
    public partial class BMCContext : DbContext
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

        public BMCContext()
        {
            this.ChangeTracker.LazyLoadingEnabled = false;
        }

        public BMCContext(int commandTimeoutSeconds)
        {
            this.commandTimeoutSeconds = commandTimeoutSeconds;
        }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");

                // Check for a custom or standard environment variable
                if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development" ||
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    builder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
                }

                IConfigurationRoot configuration = builder.Build();
                var connectionString = configuration.GetConnectionString("BMC");
                optionsBuilder.UseSqlServer(connectionString)
                              .UseLazyLoadingProxies(false);

                if (connectionString != null)
                {
                    optionsBuilder.UseSqlServer(connectionString, opts => opts.CommandTimeout(commandTimeoutSeconds))
                                  .UseLazyLoadingProxies(false);
                }
                else
                {
                    throw new Exception("Unable to read BMC connection string from the appSettings.json file");
                }
            }

            return;
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
    }
}
