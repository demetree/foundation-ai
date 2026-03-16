using Foundation.Community.Database;
using Foundation.Extensions;
using Foundation.Security.Configuration;
using Foundation.Security.Database;
using Foundation.Auditor.Database;
using Foundation.Security.OIDC;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Foundation.Configuration;
using static Foundation.StartupBasics;
using Foundation.Web.Services;
using Foundation.Community.Controllers.WebAPI;
using Foundation.Community.Middleware;


namespace Foundation.Community
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

            //
            // Initialize log error notifications EARLY - before DI
            //
            Foundation.Web.Services.LogErrorNotificationExtensions.InitializeFromConfiguration(config);

            logger.LogSystem("Community CMS is starting.");

            //
            // Configure the system auditor
            //
            Foundation.StartupBasics.ConfigureAuditor();

            logger.LogInformation("Auditor is configured.");


            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            try
            {
                // Configure Kestrel to allow synchronous I/O
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.AllowSynchronousIO = true;
                });

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
                // Enable local IndexedDB-backed session cache for per-request validation
                //
                builder.Services.AddSessionCache();
                builder.Services.AddAuditBuffer();

                //
                // Add memory cache for tenant resolution and scoped tenant context
                //
                builder.Services.AddMemoryCache();
                builder.Services.AddScoped<TenantContext>();


                //
                // Add the Community Database context
                //
                builder.Services.AddDbContext<CommunityContext>(options =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("Community"))
                           .UseLazyLoadingProxies(false);
                });


                // Configure Kestrel server options
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.Limits.MaxConcurrentConnections = null;
                    options.Limits.MaxConcurrentUpgradedConnections = null;
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
                // Allow this system to be monitored
                //
                Foundation.Web.Utility.StartupBasics.AddSystemHealthControllers(controllers);


                //
                // Custom Community controllers
                //
                controllers.Add(typeof(Controllers.PublicContentController));


                //
                // Code generated controllers for Community module
                //
                controllers.Add(typeof(AnnouncementsController));
                controllers.Add(typeof(AnnouncementChangeHistoriesController));
                controllers.Add(typeof(ContactSubmissionsController));
                controllers.Add(typeof(DocumentDownloadsController));
                controllers.Add(typeof(GalleryAlbumsController));
                controllers.Add(typeof(GalleryImagesController));
                controllers.Add(typeof(MediaAssetsController));
                controllers.Add(typeof(MenuItemsController));
                controllers.Add(typeof(MenusController));
                controllers.Add(typeof(PageChangeHistoriesController));
                controllers.Add(typeof(PagesController));
                controllers.Add(typeof(PostCategoriesController));
                controllers.Add(typeof(PostChangeHistoriesController));
                controllers.Add(typeof(PostTagAssignmentsController));
                controllers.Add(typeof(PostTagsController));
                controllers.Add(typeof(PostsController));
                controllers.Add(typeof(SiteSettingsController));


                logger.LogInformation("Controllers have been configured.");

                //
                // Constrain this host to just use the listed controllers
                //
                builder.Services.AddMvcCore().UseSpecificControllers(controllers.ToArray());

                builder.Services.AddControllers(options =>
                {
                    options.ReturnHttpNotAcceptable = false;
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
                    options.JsonSerializerOptions.Converters.Add(new Foundation.JSON.UtcDateTimeConverter());
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });


                //
                // Add Swagger
                //
                builder.Services.AddEndpointsApiExplorer();

                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Community CMS Server", Version = "v1" });
                });


                WebApplication app = builder.Build();


                //
                // Database schema validation
                //
                try
                {
                    await ValidateCommunitySchema(logger);
                }
                catch (Exception ex)
                {
                    logger.LogError("Community database schema validation failed. " + ex.Message);
                    logger.LogError(ex.StackTrace);
                }

                logger.LogInformation("Schema validation complete.");


                //
                // Configure the HTTP request pipeline
                //
                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseSession();
                app.UseDefaultFiles();
                app.UseStaticFiles();
                app.UseRouting();

                //
                // Tenant resolution from Host header — resolves SecurityTenant.hostName
                // and populates the scoped TenantContext for downstream use.
                //
                app.UseMiddleware<TenantResolutionMiddleware>();

                //
                // CORS configuration
                //
                if (app.Environment.IsDevelopment())
                {
                    app.UseCors(policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
                }
                else
                {
                    app.UseCors(policy =>
                    {
                        policy.WithOrigins("https://localhost:5902")
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    });
                }


                //
                // Enable authentication and authorization middleware
                //
                app.UseAuthentication();
                app.UseAuthorization();


                //
                // Security middleware: Content Security Policy (CSP)
                //
                app.Use(async (context, next) =>
                {
                    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                    context.Response.Headers.Append("X-Frame-Options", "DENY");
                    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

                    await next();
                });


                app.MapControllers();

                //
                // Fall back to SPA for the public site
                //
                app.MapFallbackToFile("/index.html");


                logger.LogSystem("Community CMS has started.");

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                logger.LogException($"Community CMS is exiting because of exception {ex.Message}", ex);
            }
            finally
            {
                Logger.TerminateApplicationLogging();
            }
        }


        private static async Task ValidateCommunitySchema(Logger logger)
        {
            logger.LogInformation("About to validate Community database schema.");

            try
            {
                await using CommunityContext validationContext = new CommunityContext();

                DatabaseSchemaValidator<CommunityContext> schemaValidator = new DatabaseSchemaValidator<CommunityContext>(validationContext, logger);

                DatabaseSchemaValidator<CommunityContext>.DatabaseSchemaValidatorResult schemaValidationResult = await schemaValidator.ValidateSchemaAsync("Community").ConfigureAwait(false);

                if (schemaValidationResult.IsValid == false)
                {
                    logger.LogCritical("Community database schema validation failed:");
                    foreach (var mismatch in schemaValidationResult.Mismatches)
                    {
                        logger.LogCritical(mismatch);
                    }

                    throw new InvalidOperationException("Community database schema is out of sync with EF context.");
                }
                else
                {
                    logger.LogCritical("Community database schema validation passed.");
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An error occurred during Community database schema validation.");
                throw;
            }
        }


        private static Logger SetupLogger(IConfigurationRoot config)
        {
            Logger logger = new Logger();

            //
            // Set the common logger
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
                logLevel = "Information";
            }

            Logger.LogLevels logLevelToUse;

            if (Logger.LogLevelFromString(logLevel, out logLevelToUse) == false)
            {
                logLevelToUse = Logger.LogLevels.Information;
            }

            logger.Level = logLevelToUse;

            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "";

            logger.SetDirectory(Path.Combine(currentPath, LOG_DIRECTORY));
            logger.SetFileName(LOG_FILENAME);

            return logger;
        }
    }
}
