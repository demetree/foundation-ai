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
        }
    }
}