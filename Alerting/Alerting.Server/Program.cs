using Alerting.Server.Controllers;
using Alerting.Server.Services;
using Alerting.Server.Services.Notifications;
using Foundation.Auditor.Controllers.WebAPI;
using Foundation.Auditor.Database;
using Foundation.Extensions;
using Foundation.Alerting.Controllers.WebAPI;
using Foundation.Alerting.Database;
using Foundation.Security.Configuration;
using Foundation.Security.Controllers.WebAPI;
using Foundation.Security.Database;
using Foundation.Security.OIDC;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Foundation.Configuration;
using static Foundation.StartupBasics;


namespace Foundation.Alerting
{
    public static class Constants
    {
        public const int ONE_GIGABYTE_IN_BYTES = 1073741824;


        public const string UNKNOWN = "Unknown";

    }

    public class Program
    {
        private const string LOG_DIRECTORY = "Log";
        private const string LOG_FILENAME = "Startup";


        private static async Task Main(string[] args)
        {
            IConfigurationRoot config = GetConfiguration();

            Logger logger = SetupLogger(config);

            logger.LogSystem("Alerting is starting.");

            //
            // Configure the system auditor
            //
            Foundation.StartupBasics.ConfigureAuditor();

            logger.LogInformation("Auditor is configured.");


            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            try
            {
                // Configure Kestrel to allow synchronous I/O - needed for Excel export
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.AllowSynchronousIO = true; // Enable synchronous I/O
                });


                //// Configure IIS to allow synchronous IO for when we run behind IIS. - needed for Excel export
                builder.Services.Configure<IISServerOptions>(options =>
                {
                    options.AllowSynchronousIO = true;
                });


                //
                // Add the logger to the pipeline
                //
                builder.Services.AddLogging(builder =>
                {
                    builder.AddProvider(logger);
                });

                //
                // Add HSTS
                //
                builder.Services.AddHsts(options =>
                {
                    options.Preload = true;
                    options.IncludeSubDomains = true;
                    options.MaxAge = TimeSpan.FromDays(365);
                    //options.ExcludedHosts.Add("example.com");
                });


                //
                // Add HTTP client factory
                //
                builder.Services.AddHttpClient();

                //
                // Configure the foundation services
                //
                BuildFoundationServices(builder, logger);


                //
                // Add the Alerting Database context
                //
                builder.Services.AddDbContext<AlertingContext>(options =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("Alerting"))
                           .UseLazyLoadingProxies(false)
                           .AddInterceptors(UtcDateTimeInterceptor.Instance);

                });




                // Configure Kestrel server options
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.Limits.MaxConcurrentConnections = null;                 // null for no limit
                    options.Limits.MaxConcurrentUpgradedConnections = null;         // null for no limit

                    //
                    // Set a very large request body size to allow for the uploading of binary data for manual software releases.
                    //
                    options.Limits.MaxRequestBodySize = Constants.ONE_GIGABYTE_IN_BYTES;
                });

                //
                // Add CORS - Will be configured below.
                //
                builder.Services.AddCors();

                //
                // Add session support that is needed for Foundation modules
                //
                builder.Services.AddMvc(configuration =>
                    configuration.EnableEndpointRouting = false
                ).AddSessionStateTempDataProvider();

                builder.Services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(30);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                        ? CookieSecurePolicy.None
                        : CookieSecurePolicy.Always;
                });


                logger.LogInformation("Web Host Builder Services have been configured.");

                // use the extensions from Damian Hickey to reference needed controllers only
                List<Type> controllers = new List<Type>();

                // 
                // Add the essential Foundation controllers for basic user login and config features
                //
                Foundation.Web.Utility.StartupBasics.AddFoundationEssentialWebAPIControllers(controllers);

                //
                // Allow this sytem to be monitored and the system-health UI to work
                //
                Foundation.Web.Utility.StartupBasics.AddSystemHealthControllers(controllers);
                Foundation.Web.Utility.StartupBasics.AddMonitoredApplicationsController(controllers);


                //
                // Custom Alerting controllers
                //
                controllers.Add(typeof(AlertsController));
                controllers.Add(typeof(DashboardController));
                controllers.Add(typeof(IncidentManagementController));
                controllers.Add(typeof(IntegrationManagementController));
                controllers.Add(typeof(NotificationFlightControlController));
                controllers.Add(typeof(UsersController));
                controllers.Add(typeof(IntegrationRegistrationController));
                controllers.Add(typeof(NotificationAuditController));

                //
                // End of Alerting custom controllers
                //

                //
                // Start of code generated controller list for Alerting module
                //
                controllers.Add(typeof(EscalationPoliciesController));
                controllers.Add(typeof(EscalationPolicyChangeHistoriesController));
                controllers.Add(typeof(EscalationRulesController));
                controllers.Add(typeof(EscalationRuleChangeHistoriesController));
                controllers.Add(typeof(IncidentsController));
                controllers.Add(typeof(IncidentChangeHistoriesController));
                controllers.Add(typeof(IncidentEventTypesController));
                controllers.Add(typeof(IncidentNotesController));
                controllers.Add(typeof(IncidentNoteChangeHistoriesController));
                controllers.Add(typeof(IncidentNotificationsController));
                controllers.Add(typeof(IncidentStatusTypesController));
                controllers.Add(typeof(IncidentTimelineEventsController));
                controllers.Add(typeof(IntegrationsController));
                controllers.Add(typeof(IntegrationCallbackIncidentEventTypesController));
                controllers.Add(typeof(IntegrationCallbackIncidentEventTypeChangeHistoriesController));
                controllers.Add(typeof(IntegrationChangeHistoriesController));
                controllers.Add(typeof(NotificationChannelTypesController));
                controllers.Add(typeof(NotificationDeliveryAttemptsController));
                controllers.Add(typeof(OnCallSchedulesController));
                controllers.Add(typeof(OnCallScheduleChangeHistoriesController));
                controllers.Add(typeof(ScheduleLayersController));
                controllers.Add(typeof(ScheduleLayerChangeHistoriesController));
                controllers.Add(typeof(ScheduleLayerMembersController));
                controllers.Add(typeof(ScheduleLayerMemberChangeHistoriesController));
                controllers.Add(typeof(ScheduleOverridesController));
                controllers.Add(typeof(ScheduleOverrideChangeHistoriesController));
                controllers.Add(typeof(ScheduleOverrideTypesController));
                controllers.Add(typeof(ServicesController));
                controllers.Add(typeof(ServiceChangeHistoriesController));
                controllers.Add(typeof(SeverityTypesController));
                controllers.Add(typeof(UserNotificationChannelPreferencesController));
                controllers.Add(typeof(UserNotificationChannelPreferenceChangeHistoriesController));
                controllers.Add(typeof(UserNotificationPreferencesController));
                controllers.Add(typeof(UserNotificationPreferenceChangeHistoriesController));
                controllers.Add(typeof(WebhookDeliveryAttemptsController));
                //
                // End of code generated controller list for Alerting module
                //


                logger.LogInformation("Controllers have been configured.");

                //
                // Constrain this host to just use the dashboard controllers listed, not all the controllers in all the assemblies.  We want to hide the Foundation Security and Auditor controllers from outside access.
                //
                builder.Services.AddMvcCore().UseSpecificControllers(controllers.ToArray());

                builder.Services.AddControllers(options =>
                {

                    // Suppress automatic output formatter validation for specific content types
                    // Prevents ASP.NET Core from throwing an error when no output formatter is found for the requested content type.
                    options.ReturnHttpNotAcceptable = false;
                })
                // Add JSON Options to the controllers to allow the JSON serialization of NAN values, and UTC formatting for dates.
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
                    options.JsonSerializerOptions.Converters.Add(new Foundation.JSON.UtcDateTimeConverter());
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;      // Don't serialize circular references
                });


                //
                // Add Swagger.  Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                //
                builder.Services.AddEndpointsApiExplorer();

                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = OidcApplicationManager.ALERTING_SERVER_NAME, Version = "v1" });
               
                    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            Password = new OpenApiOAuthFlow
                            {
                                TokenUrl = new Uri("/connect/token", UriKind.Relative)
                            }
                        }
                    });

                    // Add Global Security Requirement
                    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("oauth2", doc)] = new List<string> 
                        { 
                            "openid", "profile", "email", "roles", "phone", "address" 
                        }
                    });
                });

                builder.Services.AddSingleton<Foundation.Logger>(logger);

                //
                // Monitored Application Service (for multi-app health monitoring)
                //
                builder.Services.AddSingleton<Foundation.Services.IMonitoredApplicationService, Foundation.Services.MonitoredApplicationService>();

                //
                // Database Health Providers (for System Health dashboard)
                //
                builder.Services.AddSingleton<Foundation.Services.IDatabaseHealthProvider>(
                    new Foundation.Services.DbContextHealthProvider<SecurityContext>("Security"));
                builder.Services.AddSingleton<Foundation.Services.IDatabaseHealthProvider>(
                    new Foundation.Services.DbContextHealthProvider<AuditorContext>("Auditor"));
                builder.Services.AddSingleton<Foundation.Services.IDatabaseHealthProvider>(
                    new Foundation.Services.DbContextHealthProvider<AlertingContext>("Alerting"));

                //
                // Authenticated Users Provider (for System Health dashboard)
                //
                builder.Services.AddSingleton<Foundation.Services.IAuthenticatedUsersProvider,
                    Foundation.Services.SecurityContextAuthenticatedUsersProvider>();

                //
                // Application Metrics Provider (for System Health dashboard)
                //
                builder.Services.AddSingleton<Foundation.Services.IApplicationMetricsProvider,
                    global::Alerting.Server.Services.AlertingMetricsProvider>();

                //
                // Alerting Services
                //
                builder.Services.AddScoped<IAlertingService, AlertingService>();
                builder.Services.AddScoped<IDashboardService, DashboardService>();
                builder.Services.AddScoped<IEscalationService, EscalationService>();
                builder.Services.AddScoped<INotificationFlightControlService, NotificationFlightControlService>();
                builder.Services.AddScoped<INotificationAuditService, NotificationAuditService>();
                builder.Services.AddScoped<IUserService, UserService>();

                //
                // Notification Delivery Services
                //
                builder.Services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
                builder.Services.AddScoped<INotificationProvider, EmailNotificationProvider>();
                builder.Services.AddScoped<INotificationProvider, SmsNotificationProvider>();
                builder.Services.AddScoped<INotificationProvider, VoiceCallNotificationProvider>();
                builder.Services.AddScoped<INotificationProvider, TeamsNotificationProvider>();
                builder.Services.AddScoped<INotificationProvider, PushNotificationProvider>();

                //
                // Notification Engine Configuration
                //
                var notificationEngineSection = builder.Configuration.GetSection(NotificationEngineOptions.SectionName);
                builder.Services.Configure<NotificationEngineOptions>(notificationEngineSection);
                
                // Configure the static NotificationLogger with settings from appsettings.json
                var notificationEngineOptions = notificationEngineSection.Get<NotificationEngineOptions>() ?? new NotificationEngineOptions();
                NotificationLogger.Configure(notificationEngineOptions);
                logger.LogInformation($"Notification engine configured: LogLevel={notificationEngineOptions.LogLevel}, " +
                    $"EscalationInterval={notificationEngineOptions.EscalationWorkerIntervalSeconds}s, " +
                    $"RetryInterval={notificationEngineOptions.RetryWorkerIntervalSeconds}s");

                //
                // Background Workers
                //
                builder.Services.AddHostedService<EscalationWorker>();
                builder.Services.AddHostedService<NotificationRetryWorker>();

                //
                // Configurations
                //
                builder.Services.Configure<AppSettings>(builder.Configuration);


                logger.LogInformation("About to build web application.");


                var app = builder.Build();

                logger.LogInformation("Completed building web application.");

                //
                // Configure sessions.  This is necessary for the Foundation to operate.
                //
                app.UseSession();

                //
                // Configure the request pipeline
                //
                app.UseDefaultFiles();
                app.UseStaticFiles();

                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();

                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        c.DocumentTitle = "Swagger UI - Alerting";
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{OidcApplicationManager.ALERTING_SERVER_NAME} V1");
                        c.OAuthClientId(OidcApplicationManager.SWAGGER_CLIENT_ID);

                        //
                        // .NET 10 / Swashbuckle 10.x requires these settings for OAuth2 password flow
                        //
                        c.EnablePersistAuthorization();           // Persist the token across requests
                    });

                    IdentityModelEventSource.ShowPII = true;
                }
                else
                {
                    // Production error handling middleware - logs exceptions and returns a generic error response
                    app.UseExceptionHandler(errorApp =>
                    {
                        errorApp.Run(async context =>
                        {
                            var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                            if (exceptionFeature?.Error != null)
                            {
                                logger.LogException($"Unhandled exception: {exceptionFeature.Error.Message}", exceptionFeature.Error);
                            }

                            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync("{\"error\": \"An unexpected error occurred. Please try again later.\"}");
                        });
                    });                    
                    
                    // See https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                app.UseHttpsRedirection();

                app.UseAuthentication();
                app.UseAuthorization();


                //
                // Setup CORS based on the environment.  Production is hardened.
                //
                if (app.Environment.IsDevelopment())
                {
                    //
                    // This is for development only
                    //
                    app.UseCors(builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    );
                }
                else
                {
                    //
                    // This is for production
                    //
                    string allowedDomains = Configuration.GetStringConfigurationSetting("AllowedCORSDomains", "https:////k2research.ca;");

                    // Note that since bearer tokens are used, we do not need to use .AllowCredentials
                    app.UseCors(builder => builder
                        .WithOrigins(allowedDomains.Split(","))
                        .SetIsOriginAllowedToAllowWildcardSubdomains()      // To allow other apps under the root domain
                        .WithHeaders("Content-Type", "Accept", "Authorization")
                        .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                        .SetPreflightMaxAge(TimeSpan.FromMinutes(5))
                    );
                }


                //
                // Add Content Security Policy Header based on environment
                //
                app.Use(async (context, next) =>
                {
                    string cspPolicy;

                    if (app.Environment.IsDevelopment())
                    {
                        // More permissive policy for development, allowing API calls, inline scripts, and styles
                        cspPolicy = "default-src 'self'; " +
                                    "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                                    "style-src 'self' 'unsafe-inline'; " +
                                    "img-src 'self' data: blob: ; " +
                                    "connect-src 'self';";
                    }
                    else
                    {
                        // Stricter policy for production, ensuring API calls to GitHub and the software registration API are allowed
                        cspPolicy = "default-src 'self'; " +
                                    "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                                    "style-src 'self' 'unsafe-inline'; " +
                                    "img-src 'self' data: blob: ; " +
                                    "connect-src 'self' ;";
                    }

                    context.Response.Headers["Content-Security-Policy"] = cspPolicy;

                    await next();
                });



                app.MapControllers();

                app.MapFallbackToFile("/index.html");


                //
                // Add Applications to Security Module's OIDC Database
                //
                using IServiceScope scope = app.Services.CreateScope();
                try
                {
                    await OidcApplicationManager.RegisterClientApplicationsAsync(scope.ServiceProvider);
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "An error occurred during creating Alerting client applications in OIDC database.");

                    throw;
                }

                //
                // Validate the database schemas are correct before we start.  These will throw if there is a problem.
                //
                await ValidateAlertingSchema(logger).ConfigureAwait(false);

                await ValidateSecuritySchema(logger).ConfigureAwait(false);

                await ValidateAuditorSchema(logger).ConfigureAwait(false);


                //
                // Log database statistics for startup diagnostics
                //
                using (var securityContext = new SecurityContext())
                using (var auditorContext = new AuditorContext())
                using (var alertingContext = new AlertingContext())
                {
                    LogDatabaseStatistics(logger,
                        ("Security", securityContext),
                        ("Auditor", auditorContext),
                        ("Alerting", alertingContext)
                    );
                }

                //
                // Run Alerting
                //
                logger.LogSystem("About to run Alerting web server.");
                app.Run();

            }
            catch (Exception ex)
            {
                logger.LogException($"Alerting is exiting because of exception {ex.Message}", ex);
            }
            finally
            {
                Logger.TerminateApplicationLogging();
            }
        }


        private static async Task ValidateAlertingSchema(Logger logger)
        {
            logger.LogInformation("About to validate Alerting database schema.");

            try
            {
                await using AlertingContext validationContext = new AlertingContext();

                DatabaseSchemaValidator<AlertingContext> schemaValidator = new DatabaseSchemaValidator<AlertingContext>(validationContext, logger);

                DatabaseSchemaValidator<AlertingContext>.DatabaseSchemaValidatorResult schemaValidationResult = await schemaValidator.ValidateSchemaAsync("Alerting").ConfigureAwait(false);

                if (schemaValidationResult.IsValid == false)
                {
                    logger.LogCritical("Alerting database schema validation failed:");
                    foreach (var mismatch in schemaValidationResult.Mismatches)
                    {
                        logger.LogCritical(mismatch);
                    }

                    throw new InvalidOperationException("Alerting database schema is out of sync with EF context.");
                }
                else
                {
                    logger.LogCritical("Alerting database schema validation passed.");
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An error occurred during Alerting database schema validation.");
                throw;
            }


            logger.LogInformation("Completed validation of Alerting database schema.");
        }


        private static async Task ValidateSecuritySchema(Logger foundationLogger)
        {
            foundationLogger.LogInformation("About to validate Security database schema.");

            try
            {
                await using SecurityContext validationContext = new SecurityContext();

                DatabaseSchemaValidator<SecurityContext> schemaValidator = new DatabaseSchemaValidator<SecurityContext>(validationContext, foundationLogger);

                DatabaseSchemaValidator<SecurityContext>.DatabaseSchemaValidatorResult schemaValidationResult = await schemaValidator.ValidateSchemaAsync("Security").ConfigureAwait(false);

                if (schemaValidationResult.IsValid == false)
                {
                    foundationLogger.LogCritical("Security database schema validation failed:");
                    foreach (var mismatch in schemaValidationResult.Mismatches)
                    {
                        foundationLogger.LogCritical(mismatch);
                    }

                    throw new InvalidOperationException("Security database schema is out of sync with EF context.");
                }
                else
                {
                    foundationLogger.LogCritical("Security database schema validation passed.");
                }
            }
            catch (Exception ex)
            {
                foundationLogger.LogCritical(ex, "An error occurred during Security database schema validation.");
                throw;
            }


            foundationLogger.LogInformation("Completed validation of Security database schema.");
        }


        private static async Task ValidateAuditorSchema(Logger foundationLogger)
        {
            foundationLogger.LogInformation("About to validate Auditor database schema.");

            try
            {
                await using AuditorContext validationContext = new AuditorContext();

                DatabaseSchemaValidator<AuditorContext> schemaValidator = new DatabaseSchemaValidator<AuditorContext>(validationContext, foundationLogger);

                DatabaseSchemaValidator<AuditorContext>.DatabaseSchemaValidatorResult schemaValidationResult = await schemaValidator.ValidateSchemaAsync("Auditor").ConfigureAwait(false);

                if (schemaValidationResult.IsValid == false)
                {
                    foundationLogger.LogCritical("Auditor database schema validation failed:");
                    foreach (var mismatch in schemaValidationResult.Mismatches)
                    {
                        foundationLogger.LogCritical(mismatch);
                    }

                    throw new InvalidOperationException("Auditor database schema is out of sync with EF context.");
                }
                else
                {
                    foundationLogger.LogCritical("Auditor database schema validation passed.");
                }
            }
            catch (Exception ex)
            {
                foundationLogger.LogCritical(ex, "An error occurred during Auditor database schema validation.");
                throw;
            }

            foundationLogger.LogInformation("Completed validation of Auditor database schema.");
        }


        private static Logger SetupLogger(IConfigurationRoot config)
        {
            Logger logger = new Logger();

            //
            // Set the common logger to the Basecamp Launcher logger
            //
            Logger.SetCommonLogger(logger);


            //
            // Get the log level from the settings
            //
            string logLevel;
            try
            {
                logLevel = config.GetValue<string>("Logging:LogLevel:Default");
            }
            catch
            {
                logLevel = "Information";        // default to information logging if the config file doesn't say otherwise.
            }

            Logger.LogLevels logLevelToUse;

            // Try to convert the log level string to a log level.  If it fails, use the default information.
            if (Logger.LogLevelFromString(logLevel, out logLevelToUse) == false)
            {
                logLevelToUse = Logger.LogLevels.Information;
            }

            logger.Level = logLevelToUse;



            // start off by setting the default log information
            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "";

            logger.SetDirectory(Path.Combine(currentPath, LOG_DIRECTORY));
            logger.SetFileName(LOG_FILENAME);

            return logger;
        }
    }
}
