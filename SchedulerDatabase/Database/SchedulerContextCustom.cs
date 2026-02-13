using System;
using System.Configuration;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static Foundation.DatabaseUtility.DBContextExtensions;

namespace Foundation.Scheduler.Database;


/// <summry>
/// 
/// This is an extension class to the Scheduler Context that is separated from the main SchedulerContext.cs so that it won't be clobbered if the EF Core Power tools
/// are used to rebuild the Auditor classes
/// 
/// </summary>

public partial class SchedulerContext : DbContext
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


    public SchedulerContext()
    {
        this.ChangeTracker.LazyLoadingEnabled = false;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.AddInterceptors(UtcDateTimeInterceptor.Instance);

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
            string connectionString = configuration.GetConnectionString("Scheduler");
            optionsBuilder.UseSqlServer(connectionString)
                          .UseLazyLoadingProxies(false)
                          .AddInterceptors(UtcDateTimeInterceptor.Instance);
        }

        return;
    }

    // Custom constructor for SQL logging with custom ILogger to write to
    public SchedulerContext(ILogger logger, LogLevel level)
        : base(CreateOptionsWithDebugLogging(logger, level))
    {
    }


    private static DbContextOptions<SchedulerContext> CreateOptionsWithDebugLogging(ILogger logger, LogLevel level)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SchedulerContext>();

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
        var connectionString = configuration.GetConnectionString("Scheduler");

        optionsBuilder.UseSqlServer(connectionString)
                      .EnableSensitiveDataLogging()
                      .LogTo((message) => logger.LogInformation(message), level);       // all messages logged to our logger at the info level

        return optionsBuilder.Options;
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