using Foundation.Auditor.Controllers.WebAPI;
using Foundation.Auditor.Database;
using Foundation.Extensions;
using Foundation.Scheduler.Controllers.WebAPI;
using Foundation.Scheduler.Database;
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


namespace Foundation.Scheduler
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

            logger.LogSystem("Scheduler is starting.");

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

                // Register Donor Journey Calculator
                builder.Services.AddScoped<Foundation.Scheduler.Services.DonorJourneyCalculator>();

                //
                // Add the Scheduler Database context
                //
                builder.Services.AddDbContext<SchedulerContext>(options =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("Scheduler"))
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
                // Add the Foundation controllers.
                //
                Foundation.Web.Utility.StartupBasics.AddFoundationEssentialWebAPIControllers(controllers);
                Foundation.Web.Utility.StartupBasics.AddSecurityWebAPIControllers(controllers);
                Foundation.Web.Utility.StartupBasics.AddAuditorWebAPIControllers(controllers);

                //
                // Custom Scheduler controllers
                //
                controllers.Add(typeof(DataController));                    // For Excel export
                controllers.Add(typeof(TenantProfileController));           // For profile access with auto creation
                //
                // End of Scheduler custom controllers
                //

                //
                // Start of code generated controller list for Scheduler module
                //
                controllers.Add(typeof(AppealsController));
                controllers.Add(typeof(AppealChangeHistoriesController));
                controllers.Add(typeof(AssignmentRolesController));
                controllers.Add(typeof(AssignmentRoleQualificationRequirementsController));
                controllers.Add(typeof(AssignmentRoleQualificationRequirementChangeHistoriesController));
                controllers.Add(typeof(AssignmentStatusesController));
                controllers.Add(typeof(AttributeDefinitionsController));
                controllers.Add(typeof(AttributeDefinitionChangeHistoriesController));
                controllers.Add(typeof(AttributeDefinitionEntitiesController));
                controllers.Add(typeof(AttributeDefinitionTypesController));
                controllers.Add(typeof(BatchesController));
                controllers.Add(typeof(BatchChangeHistoriesController));
                controllers.Add(typeof(BatchStatusesController));
                controllers.Add(typeof(BookingSourceTypesController));
                controllers.Add(typeof(CalendarsController));
                controllers.Add(typeof(CalendarChangeHistoriesController));
                controllers.Add(typeof(CampaignsController));
                controllers.Add(typeof(CampaignChangeHistoriesController));
                controllers.Add(typeof(ChargeStatusesController));
                controllers.Add(typeof(ChargeTypesController));
                controllers.Add(typeof(ChargeTypeChangeHistoriesController));
                controllers.Add(typeof(ClientsController));
                controllers.Add(typeof(ClientChangeHistoriesController));
                controllers.Add(typeof(ClientContactsController));
                controllers.Add(typeof(ClientContactChangeHistoriesController));
                controllers.Add(typeof(ClientTypesController));
                controllers.Add(typeof(ConstituentsController));
                controllers.Add(typeof(ConstituentChangeHistoriesController));
                controllers.Add(typeof(ConstituentJourneyStagesController));
                controllers.Add(typeof(ConstituentJourneyStageChangeHistoriesController));
                controllers.Add(typeof(ContactsController));
                controllers.Add(typeof(ContactChangeHistoriesController));
                controllers.Add(typeof(ContactContactsController));
                controllers.Add(typeof(ContactContactChangeHistoriesController));
                controllers.Add(typeof(ContactInteractionsController));
                controllers.Add(typeof(ContactInteractionChangeHistoriesController));
                controllers.Add(typeof(ContactMethodsController));
                controllers.Add(typeof(ContactTagsController));
                controllers.Add(typeof(ContactTagChangeHistoriesController));
                controllers.Add(typeof(ContactTypesController));
                controllers.Add(typeof(CountriesController));
                controllers.Add(typeof(CrewsController));
                controllers.Add(typeof(CrewChangeHistoriesController));
                controllers.Add(typeof(CrewMembersController));
                controllers.Add(typeof(CrewMemberChangeHistoriesController));
                controllers.Add(typeof(CurrenciesController));
                controllers.Add(typeof(DependencyTypesController));
                controllers.Add(typeof(EventCalendarsController));
                controllers.Add(typeof(EventChargesController));
                controllers.Add(typeof(EventChargeChangeHistoriesController));
                controllers.Add(typeof(EventResourceAssignmentsController));
                controllers.Add(typeof(EventResourceAssignmentChangeHistoriesController));
                controllers.Add(typeof(EventStatusesController));
                controllers.Add(typeof(FundsController));
                controllers.Add(typeof(FundChangeHistoriesController));
                controllers.Add(typeof(GiftsController));
                controllers.Add(typeof(GiftChangeHistoriesController));
                controllers.Add(typeof(HouseholdsController));
                controllers.Add(typeof(HouseholdChangeHistoriesController));
                controllers.Add(typeof(IconsController));
                controllers.Add(typeof(InteractionTypesController));
                controllers.Add(typeof(NotificationSubscriptionsController));
                controllers.Add(typeof(NotificationSubscriptionChangeHistoriesController));
                controllers.Add(typeof(NotificationTypesController));
                controllers.Add(typeof(OfficesController));
                controllers.Add(typeof(OfficeChangeHistoriesController));
                controllers.Add(typeof(OfficeContactsController));
                controllers.Add(typeof(OfficeContactChangeHistoriesController));
                controllers.Add(typeof(OfficeTypesController));
                controllers.Add(typeof(PaymentTypesController));
                controllers.Add(typeof(PledgesController));
                controllers.Add(typeof(PledgeChangeHistoriesController));
                controllers.Add(typeof(PrioritiesController));
                controllers.Add(typeof(QualificationsController));
                controllers.Add(typeof(RateSheetsController));
                controllers.Add(typeof(RateSheetChangeHistoriesController));
                controllers.Add(typeof(RateTypesController));
                controllers.Add(typeof(ReceiptTypesController));
                controllers.Add(typeof(RecurrenceExceptionsController));
                controllers.Add(typeof(RecurrenceExceptionChangeHistoriesController));
                controllers.Add(typeof(RecurrenceFrequenciesController));
                controllers.Add(typeof(RecurrenceRulesController));
                controllers.Add(typeof(RecurrenceRuleChangeHistoriesController));
                controllers.Add(typeof(RelationshipTypesController));
                controllers.Add(typeof(ResourcesController));
                controllers.Add(typeof(ResourceAvailabilitiesController));
                controllers.Add(typeof(ResourceAvailabilityChangeHistoriesController));
                controllers.Add(typeof(ResourceChangeHistoriesController));
                controllers.Add(typeof(ResourceContactsController));
                controllers.Add(typeof(ResourceContactChangeHistoriesController));
                controllers.Add(typeof(ResourceQualificationsController));
                controllers.Add(typeof(ResourceQualificationChangeHistoriesController));
                controllers.Add(typeof(ResourceShiftsController));
                controllers.Add(typeof(ResourceShiftChangeHistoriesController));
                controllers.Add(typeof(ResourceTypesController));
                controllers.Add(typeof(SalutationsController));
                controllers.Add(typeof(ScheduledEventsController));
                controllers.Add(typeof(ScheduledEventChangeHistoriesController));
                controllers.Add(typeof(ScheduledEventDependenciesController));
                controllers.Add(typeof(ScheduledEventDependencyChangeHistoriesController));
                controllers.Add(typeof(ScheduledEventQualificationRequirementsController));
                controllers.Add(typeof(ScheduledEventQualificationRequirementChangeHistoriesController));
                controllers.Add(typeof(ScheduledEventTemplatesController));
                controllers.Add(typeof(ScheduledEventTemplateChangeHistoriesController));
                controllers.Add(typeof(ScheduledEventTemplateChargesController));
                controllers.Add(typeof(ScheduledEventTemplateChargeChangeHistoriesController));
                controllers.Add(typeof(ScheduledEventTemplateQualificationRequirementsController));
                controllers.Add(typeof(ScheduledEventTemplateQualificationRequirementChangeHistoriesController));
                controllers.Add(typeof(SchedulingTargetsController));
                controllers.Add(typeof(SchedulingTargetAddressesController));
                controllers.Add(typeof(SchedulingTargetAddressChangeHistoriesController));
                controllers.Add(typeof(SchedulingTargetChangeHistoriesController));
                controllers.Add(typeof(SchedulingTargetContactsController));
                controllers.Add(typeof(SchedulingTargetContactChangeHistoriesController));
                controllers.Add(typeof(SchedulingTargetQualificationRequirementsController));
                controllers.Add(typeof(SchedulingTargetQualificationRequirementChangeHistoriesController));
                controllers.Add(typeof(SchedulingTargetTypesController));
                controllers.Add(typeof(ShiftPatternsController));
                controllers.Add(typeof(ShiftPatternChangeHistoriesController));
                controllers.Add(typeof(ShiftPatternDaysController));
                controllers.Add(typeof(ShiftPatternDayChangeHistoriesController));
                controllers.Add(typeof(SoftCreditsController));
                controllers.Add(typeof(SoftCreditChangeHistoriesController));
                controllers.Add(typeof(StateProvincesController));
                controllers.Add(typeof(TagsController));
                controllers.Add(typeof(TenantProfilesController));
                controllers.Add(typeof(TenantProfileChangeHistoriesController));
                controllers.Add(typeof(TimeZonesController));
                controllers.Add(typeof(TributesController));
                controllers.Add(typeof(TributeChangeHistoriesController));
                controllers.Add(typeof(TributeTypesController));
                //
                // End of code generated controller list for Scheduler module
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
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = OidcApplicationManager.SCHEDULER_SERVER_NAME, Version = "v1" });
               
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
                        c.DocumentTitle = "Swagger UI - Scheduler";
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{OidcApplicationManager.SCHEDULER_SERVER_NAME} V1");
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
                    //app.UseExceptionHandler("/Home/Error");       Add an error handler to deal with this in prod

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
                    logger.LogCritical(ex, "An error occurred during creating Foundation client applications in OIDC database.");

                    throw;
                }

                //
                // Validate the database schemas are correct before we start.  These will throw if there is a problem.
                //
                await ValidateSchedulerSchema(logger).ConfigureAwait(false);

                await ValidateSecuritySchema(logger).ConfigureAwait(false);

                await ValidateAuditorSchema(logger).ConfigureAwait(false);


                //
                // Run Foundation
                //
                logger.LogSystem("About to run Scheduler web server.");
                app.Run();

            }
            catch (Exception ex)
            {
                logger.LogException($"Scheduler is exiting because of exception {ex.Message}", ex);
            }
            finally
            {
                Logger.TerminateApplicationLogging();
            }
        }


        private static async Task ValidateSchedulerSchema(Logger logger)
        {
            logger.LogInformation("About to validate Scheduler database schema.");

            try
            {
                await using SchedulerContext validationContext = new SchedulerContext();

                DatabaseSchemaValidator<SchedulerContext> schemaValidator = new DatabaseSchemaValidator<SchedulerContext>(validationContext, logger);

                DatabaseSchemaValidator<SchedulerContext>.DatabaseSchemaValidatorResult schemaValidationResult = await schemaValidator.ValidateSchemaAsync("Scheduler").ConfigureAwait(false);

                if (schemaValidationResult.IsValid == false)
                {
                    logger.LogCritical("Scheduler database schema validation failed:");
                    foreach (var mismatch in schemaValidationResult.Mismatches)
                    {
                        logger.LogCritical(mismatch);
                    }

                    throw new InvalidOperationException("Scheduler database schema is out of sync with EF context.");
                }
                else
                {
                    logger.LogCritical("Scheduler database schema validation passed.");
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An error occurred during Scheduler database schema validation.");
                throw;
            }


            logger.LogInformation("Completed validation of Scheduler database schema.");
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
