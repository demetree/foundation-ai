using Foundation.Auditor.Database;
using Foundation.Security.Database;
using Foundation.Security.OIDC;
using Foundation.Security.OIDC.TokenValidators;
using Foundation.Security.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Validation.AspNetCore;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using static OpenIddict.Abstractions.OpenIddictConstants;


namespace Foundation
{
    public class StartupBasics
    {
        /// <summary>
        /// This builds up a Web Application to register the Foundation database contexts, and setup Foundation's OIDC authentication.
        /// 
        /// Application using still need to ensure that the Security authentication controller will respond to /connect/token.  Be mindful of this if using selective controllers.
        /// 
        /// </summary>
        /// <param name="builder"></param>
        public static void BuildFoundationServices(WebApplicationBuilder builder, Logger logger)
        {
            //
            // Add the Foundation Security Database context
            //
            builder.Services.AddDbContext<SecurityContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Security"))
                       .UseLazyLoadingProxies(false)
                       .AddInterceptors(UtcDateTimeInterceptor.Instance)
                       .UseOpenIddict();   // Add the OIDC tables into the Security context.
            });


            //
            // Add the Foundation Auditor context
            //
            builder.Services.AddDbContext<AuditorContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Auditor"))
                       .UseLazyLoadingProxies(false)
                       .AddInterceptors(UtcDateTimeInterceptor.Instance);
            });


            //
            // Add Http Context Accessor, and the custom UserId accessor
            //
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IUserIdAccessor, UserIdAccessor>();

            //
            // Add the Foundation User verification Security service
            //
            builder.Services.AddScoped<IUserService, UserService>();


            //
            // Configure OpenIddict periodic pruning of orphaned authorizations/tokens from the database.
            //
            builder.Services.AddQuartz(options =>
            {
                options.UseSimpleTypeLoader();
                options.UseInMemoryStore();
            });

            // Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
            builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);


            //
            // Configure OpenId
            //
            builder.Services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                           .UseDbContext<SecurityContext>();

                    options.UseQuartz();
                })
                .AddServer(options =>
                {
                    options.SetTokenEndpointUris("connect/token");
                    options.AllowPasswordFlow()
                           .AllowCustomFlow(Constants.ExtensionGrantType)
                           .AllowRefreshTokenFlow();


                    // Set token lifetimes
                    options.Configure(config =>
                    {
                        config.AccessTokenLifetime = TimeSpan.FromHours(1);  // Access tokens live for 1 hour
                        config.RefreshTokenLifetime = TimeSpan.FromDays(7);  // Refresh tokens live for 7 days
                    });


                    options.RegisterScopes(
                        Scopes.Profile,             //  We want basic profile information
                        Scopes.Email,               //  we want email
                        Scopes.Phone,               //  Phone couldn't hurt
                        Scopes.Roles);              //  We want roles  - the only ones of interest will come from the foundation security database

                    if (builder.Environment.IsDevelopment())
                    {
                        options.AddDevelopmentEncryptionCertificate()
                               .AddDevelopmentSigningCertificate();
                    }
                    else
                    {
                        // 
                        // See https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html
                        //

                        string currentPath = builder.Environment.ContentRootPath;               // This should work under both IIS and running directly

                        string certificateFile = System.IO.Path.Combine(currentPath, builder.Configuration["OIDC:Certificate"]);
                        string keyFile = System.IO.Path.Combine(currentPath, builder.Configuration["OIDC:Key"]);


                        if (string.IsNullOrWhiteSpace(certificateFile))
                        {
                            //
                            // Not ideal - this is for dev only,  Not persisted across instances.
                            //
                            options.AddEphemeralEncryptionKey()
                                   .AddEphemeralSigningKey();
                        }
                        else
                        {
                            logger.LogSystem($"Using OIDC certificate file of {certificateFile}");

                            //
                            // Load certificate and key from PEM files
                            //
                            // Note that PEM is being used for the cross platform support for all .Net's possibly hosting operating systems.
                            //
                            X509Certificate2 certificate = X509Certificate2.CreateFromPem(File.ReadAllText(certificateFile).AsSpan(), 
                                                                                          File.ReadAllText(keyFile).AsSpan());


                            options.AddEncryptionCertificate(certificate)
                                   .AddSigningCertificate(certificate);

                        }
                    }

                    var aspNetCoreConfig = options.UseAspNetCore()
                           .EnableTokenEndpointPassthrough();

                    //
                    // Disable HSTS requirement in development environments only
                    //
                    if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development" ||
                        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    {
                        aspNetCoreConfig.DisableTransportSecurityRequirement();      // Require this to run in http only mode to allow OpenID to work
                    }
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });

            //
            // Add external authentication providers
            //
            builder.Services.AddExternalLoginValidators(options =>
            {
                options.AddValidator<GoogleTokenValidator>("google");
                options.AddValidator<FacebookTokenValidator>("facebook");
                options.AddValidator<TwitterTokenValidator>("twitter");
                options.AddValidator<MicrosoftTokenValidator>("microsoft");
            });


            //
            // add OpenID authentication
            //
            builder.Services.AddAuthentication(o =>
            {
                o.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                o.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            });
        }


        public static string GetDataDirectory()
        {
            string output = null;

            string dataDirectoryConfigurationSetting = Foundation.Configuration.GetStringConfigurationSetting("DataDirectory", null);

            if (dataDirectoryConfigurationSetting != null)
            {
                output = dataDirectoryConfigurationSetting;
            }
            else
            {
                //
                // This DataDirectory property is used in SQLite connection strings to allow a path to be assigned.
                //
                // For example:  <add name="K2JobProEntities" connectionString="data source=|DataDirectory|\K2JobPro\K2JobPro.db;BinaryGUID=false;" providerName="System.Data.SQLite" />
                //
                string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string appDataPath = System.IO.Path.Combine(rootDirectory, "App_Data");
                output = appDataPath;
            }

            return output;
        }


        public static void SetStartupConfiguration()
        {
            //
            // This is so that I can use the |DataDirectory| option in SQL connection strings like this: <add name="AuditorContext" connectionString="data source=|DataDirectory|\Auditor\Auditor.db" providerName="System.Data.SQLite" />
            //
            //
            // It will default to an App_Data folder under the application root, unless there is a DataDirectory app config setting in the configuration
            //
            string dataDirectoryConfigurationSetting = Foundation.Configuration.GetStringConfigurationSetting("DataDirectory", null);

            if (dataDirectoryConfigurationSetting != null)
            {
                AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectoryConfigurationSetting);
            }
            else
            {
                //
                // This DataDirectory property is used in SQLite connection strings to allow a path to be assigned.
                //
                // For example:  <add name="K2JobProEntities" connectionString="data source=|DataDirectory|\K2JobPro\K2JobPro.db;BinaryGUID=false;" providerName="System.Data.SQLite" />
                //
                string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string appDataPath = System.IO.Path.Combine(rootDirectory, "App_Data");
                AppDomain.CurrentDomain.SetData("DataDirectory", appDataPath);
            }
        }


        public static void SystemIntegrityCheck()
        {
            //
            // Add things here as necessary to make sure that the system is in a state to operate properly
            //
            Foundation.Security.SecurityLogic.IntegrityCheckPrivilegeTable();
            Foundation.Security.SecurityLogic.IntegrityCheckEntityDataTokenEventTypeTable();
            Foundation.Security.SecurityLogic.IntegrityCheckSecurityUserEventTypeTable();

            Foundation.Auditor.AuditEngine.IntegrityCheckAuditTypeTable();
            Foundation.Auditor.AuditEngine.IntegrityCheckAuditAccessTypeTable();
        }


        public static void ConfigureAuditor()
        {
            //
            // Should be configured before starting any logging in a mode other than in process.
            //
            string auditorMode = Foundation.Configuration.GetStringConfigurationSetting("AuditorMode", Auditor.AuditEngine.AuditorMode.DispatchToBackgroundImmediately.ToString());

            Foundation.Auditor.AuditEngine.Instance.SetAuditorMode(auditorMode);

            int auditorAutoPurgeDays = Foundation.Configuration.GetIntegerConfigurationSetting("AuditorAutoPurgeDays", 0);

            if (auditorAutoPurgeDays > 0)
            {
                Foundation.Auditor.AuditEngine.Instance.EnableAutoPurge(auditorAutoPurgeDays);
            }
            else
            {
                Foundation.Auditor.AuditEngine.Instance.DisableAutoPurge();
            }


            //
            // Configure LoginAttempt auto-purge.
            // Default to 90 days retention if not specified (0 disables purge).
            //
            int loginAttemptAutoPurgeDays = Foundation.Configuration.GetIntegerConfigurationSetting("LoginAttemptAutoPurgeDays", 90);

            if (loginAttemptAutoPurgeDays > 0)
            {
                Foundation.Security.SecurityLogic.EnableLoginAttemptAutoPurge(loginAttemptAutoPurgeDays);
            }
            else
            {
                Foundation.Security.SecurityLogic.DisableLoginAttemptAutoPurge();
            }
        }


        /// <summary>
        /// 
        /// Logs database table statistics (row counts, sizes) for each provided module context.
        /// Supports SQL Server, SQLite, MySQL, and PostgreSQL with provider-specific queries.
        /// Uses fast approximate counts where available - does not scan tables.
        /// 
        /// Call this during application startup to log database state for diagnostics.
        /// 
        /// </summary>
        /// <param name="logger">The Foundation logger to write output to.</param>
        /// <param name="contexts">One or more tuples of (ModuleName, DbContext) to log statistics for.</param>
        public static void LogDatabaseStatistics(Logger logger, params (string ModuleName, DbContext Context)[] contexts)
        {
            if (logger == null || contexts == null || contexts.Length == 0)
            {
                return;
            }

            foreach ((string ModuleName, DbContext Context) moduleContext in contexts)
            {
                try
                {
                    //
                    // Get the database connection and provider info
                    //
                    var connection = moduleContext.Context.Database.GetDbConnection();
                    string databaseName = connection.Database;
                    string providerName = moduleContext.Context.Database.ProviderName ?? "";

                    //
                    // Determine the database provider and get the appropriate query
                    //
                    string query = null;
                    bool includesSize = true;

                    if (providerName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
                    {
                        //
                        // SQL Server - uses sys.partitions for fast approximate counts
                        //
                        query = @"
                            SELECT 
                                t.name                                      AS TableName,
                                p.rows                                      AS RowCount,
                                SUM(a.used_pages) * 8 / 1024.0              AS UsedSizeMB
                            FROM 
                                sys.tables t
                                INNER JOIN sys.partitions p 
                                    ON t.object_id = p.object_id
                                INNER JOIN sys.allocation_units a 
                                    ON p.partition_id = a.container_id
                            WHERE 
                                t.is_ms_shipped = 0
                                AND p.index_id IN (0,1)
                            GROUP BY 
                                t.object_id,
                                t.name,
                                p.rows
                            ORDER BY 
                                t.name";
                    }
                    else if (providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
                    {
                        //
                        // SQLite - no built-in size statistics, use COUNT(*) for each table
                        // Note: This is slower than metadata queries but SQLite doesn't maintain statistics
                        //
                        query = @"
                            SELECT 
                                name AS TableName 
                            FROM 
                                sqlite_master 
                            WHERE 
                                type = 'table' 
                                AND name NOT LIKE 'sqlite_%'
                            ORDER BY 
                                name";

                        includesSize = false;
                    }
                    else if (providerName.Contains("MySql", StringComparison.OrdinalIgnoreCase) ||
                             providerName.Contains("Pomelo", StringComparison.OrdinalIgnoreCase))
                    {
                        //
                        // MySQL / MariaDB - uses information_schema
                        //
                        query = @"
                            SELECT 
                                TABLE_NAME                                  AS TableName,
                                TABLE_ROWS                                  AS RowCount,
                                (DATA_LENGTH + INDEX_LENGTH) / 1024 / 1024  AS UsedSizeMB
                            FROM 
                                information_schema.TABLES
                            WHERE 
                                TABLE_SCHEMA = DATABASE()
                                AND TABLE_TYPE = 'BASE TABLE'
                            ORDER BY 
                                TABLE_NAME";
                    }
                    else if (providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
                             providerName.Contains("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                    {
                        //
                        // PostgreSQL - uses pg_stat_user_tables and pg_total_relation_size
                        //
                        query = @"
                            SELECT 
                                relname                                     AS TableName,
                                n_live_tup                                  AS RowCount,
                                pg_total_relation_size(relid) / 1024.0 / 1024.0 AS UsedSizeMB
                            FROM 
                                pg_stat_user_tables
                            ORDER BY 
                                relname";
                    }
                    else
                    {
                        //
                        // Unknown provider - skip with a message
                        //
                        logger.LogSystem($"[{moduleContext.ModuleName}] Database statistics not supported for provider: {providerName}");
                        continue;
                    }

                    //
                    // Log the header
                    //
                    string providerShortName = GetProviderShortName(providerName);
                    logger.LogSystem($"[{moduleContext.ModuleName} Database: {databaseName} ({providerShortName})]");

                    //
                    // Execute the query and collect statistics
                    //
                    List<TableStatistic> statistics = new List<TableStatistic>();

                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    if (includesSize == true)
                    {
                        //
                        // Standard path for providers that support size metadata
                        //
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = query;

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    TableStatistic stat = new TableStatistic();

                                    stat.TableName = reader.GetString(0);
                                    stat.RowCount = Convert.ToInt64(reader.GetValue(1) ?? 0);
                                    stat.UsedSizeMB = Convert.ToDecimal(reader.GetValue(2) ?? 0);

                                    statistics.Add(stat);
                                }
                            }
                        }
                    }
                    else
                    {
                        //
                        // SQLite path - get table names first, then count each
                        //
                        List<string> tableNames = new List<string>();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = query;

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    tableNames.Add(reader.GetString(0));
                                }
                            }
                        }

                        foreach (string tableName in tableNames)
                        {
                            using (var countCommand = connection.CreateCommand())
                            {
                                //
                                // Use quotes to handle table names that might be keywords
                                //
                                countCommand.CommandText = $"SELECT COUNT(*) FROM \"{tableName}\"";

                                object result = countCommand.ExecuteScalar();

                                TableStatistic stat = new TableStatistic();

                                stat.TableName = tableName;
                                stat.RowCount = Convert.ToInt64(result ?? 0);
                                stat.UsedSizeMB = 0;

                                statistics.Add(stat);
                            }
                        }
                    }

                    //
                    // Log each table's statistics
                    //
                    long totalRows = 0;
                    decimal totalSizeMB = 0;

                    foreach (TableStatistic stat in statistics)
                    {
                        if (includesSize == true)
                        {
                            logger.LogSystem($"  {stat.TableName,-40} {stat.RowCount,12:N0} rows ({stat.UsedSizeMB,8:N1} MB)");
                        }
                        else
                        {
                            logger.LogSystem($"  {stat.TableName,-40} {stat.RowCount,12:N0} rows");
                        }

                        totalRows += stat.RowCount;
                        totalSizeMB += stat.UsedSizeMB;
                    }

                    //
                    // Log the totals for this module
                    //
                    if (includesSize == true)
                    {
                        logger.LogSystem($"  {"Total:",-40} {statistics.Count,12:N0} tables, {totalRows:N0} rows, {totalSizeMB:N1} MB");
                    }
                    else
                    {
                        logger.LogSystem($"  {"Total:",-40} {statistics.Count,12:N0} tables, {totalRows:N0} rows");
                    }

                    logger.LogSystem("");
                }
                catch (Exception ex)
                {
                    logger.LogSystem($"[{moduleContext.ModuleName}] Error getting database statistics: {ex.Message}");
                }
            }
        }


        /// <summary>
        /// 
        /// Gets a short, friendly name for the database provider.
        /// 
        /// </summary>
        private static string GetProviderShortName(string providerName)
        {
            if (providerName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                return "SQL Server";
            }
            else if (providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                return "SQLite";
            }
            else if (providerName.Contains("MySql", StringComparison.OrdinalIgnoreCase) ||
                     providerName.Contains("Pomelo", StringComparison.OrdinalIgnoreCase))
            {
                return "MySQL";
            }
            else if (providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
                     providerName.Contains("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                return "PostgreSQL";
            }
            else
            {
                return providerName;
            }
        }


        /// <summary>
        /// 
        /// Helper class to hold table statistics from the database query.
        /// 
        /// </summary>
        private class TableStatistic
        {
            public string TableName { get; set; }
            public long RowCount { get; set; }
            public decimal UsedSizeMB { get; set; }
        }
    }
}