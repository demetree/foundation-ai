using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Foundation.Security.Database
{

    /// <summry>
    /// 
    /// This is an extension class to the Security Context that is separated from the main SecurityContext.cs so that it won't be clobbered if the EF Core Power tools
    /// are used to rebuild the Security classes
    /// 
    /// </summary>
    public partial class SecurityContext : DbContext
    {
        int commandTimeoutSeconds = 30;

        public SecurityContext()
        {

        }

        public SecurityContext(int commandTimeoutSeconds)
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
                var connectionString = configuration.GetConnectionString("Security");
                optionsBuilder.UseSqlServer(connectionString)
                              .UseLazyLoadingProxies(false);

                if (connectionString != null)
                {
                    optionsBuilder.UseSqlServer(connectionString, opts => opts.CommandTimeout(commandTimeoutSeconds))
                                  .UseLazyLoadingProxies(false);
                }
                else
                {
                    throw new Exception("Unable to read Security connection string from the appSettings.json file");
                }
            }

            return;
        }

        public bool DoesDatabaseStoreDateWithTimeZone()
        {
            //
            // SQL Server DateTime fields do not store the UTC flag on the date, but SQLite does.
            // This affects the way that the resultant DateTime objects are 'kinded', and that varies between reads from SQL Server and SQLite.
            // This means that SQL Server DATETIME fields don't store their original time zone nor the UTC identifier, and come back with a local date kind.
            // SQLite stores UTC dates as ISO date strings, including the Z suffix, so they are read into local time, after converting from the UTC stored time if there is a Z suffix.
            // 
            // We want all date/time fields to serialize out as UTC dates, so we need to adjust them to be that, and this function helps determine what to do for the serialization.
            //

            if (this.Database.GetDbConnection().GetType().ToString().Contains("SQLite") == true)
            {
                //
                // For SQLite, and any other databases that store dates with the time zone (ie. in ISO 8601 string format, like SQLite), the date that comes back is of 'kind' local, and the time is already ajusted from the UTC raw storage format.
                // For these, do a direct conversion back into Universal time so that it serializes out with the UTC time and Z suffix.
                //
                return true;
            }
            else
            {
                //
                // This is for SQL Server, or other databases that don't retain the 'Z' UTC identifier (or any other time zone code) in the date, meaning that the date always come back in a format that is understood by the EF to be a local date time.  In this case, change the date to be of kind 'UTC' because that is what we want it to be.
                //
                return false;
            }
        }
    }
}
