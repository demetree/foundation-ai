using Foundation.Auditor.Database;
using Foundation.BMC.Controllers.WebAPI;
using Foundation.BMC.Database;
using Foundation.Extensions;
using Foundation.Security.Configuration;
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
using Foundation.Web.Services;
using Foundation.AI;
using Foundation.AI.Embed;
using Foundation.AI.Inference;
using Foundation.AI.Rag;
using Foundation.AI.VectorStore;
using Foundation.AI.VectorStore.Zvec;
using BMC.AI;
using Foundation.BMC.Services;
using BMC.Rebrickable.Sync;
using BMC.BrickSet.Sync;
using BMC.BrickLink.Sync;
using BMC.BrickEconomy.Sync;
using BMC.BrickOwl.Sync;


namespace Foundation.BMC
{
    public static class BmcConstants
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
            // Reads from "LogErrorNotification" section in appsettings.json
            //
            Foundation.Web.Services.LogErrorNotificationExtensions.InitializeFromConfiguration(config);

            logger.LogSystem("BMC is starting.");

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


                // Configure IIS to allow synchronous IO
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
                // In-memory cache for custom controllers (e.g. PartsCatalogController)
                //
                builder.Services.AddMemoryCache();


                //
                // Parts Universe precomputation service (background startup task)
                //
                builder.Services.AddSingleton<PartsUniverseService>();
                builder.Services.AddHostedService(sp => sp.GetRequiredService<PartsUniverseService>());

                //
                // Set Explorer precomputation service (background startup task)
                //
                builder.Services.AddSingleton<SetExplorerService>();
                builder.Services.AddHostedService(sp => sp.GetRequiredService<SetExplorerService>());

                //
                // Minifig Gallery precomputation service (background startup task)
                //
                builder.Services.AddSingleton<MinifigGalleryService>();
                builder.Services.AddHostedService(sp => sp.GetRequiredService<MinifigGalleryService>());

                //
                // Data import worker — auto-bootstraps on first run (empty database),
                // then checks LDraw + Rebrickable for upstream changes on an hourly basis.
                //
                builder.Services.AddHostedService<DataImportWorker>();


                //
                // Rebrickable sync service — manages bidirectional sync with Rebrickable API
                //
                builder.Services.AddSingleton<global::BMC.Rebrickable.Sync.IRebrickableActivityBroadcaster, RebrickableActivityBroadcaster>();
                builder.Services.AddScoped<RebrickableSyncService>();
                builder.Services.AddHostedService<Foundation.BMC.Services.RebrickablePeriodicPullService>();
                builder.Services.AddHostedService<Foundation.BMC.Services.RebrickableSyncQueueProcessor>();

                //
                // BrickSet sync service — manages set enrichment and user collection sync with BrickSet API
                //
                builder.Services.AddScoped<BrickSetSyncService>();

                //
                // BrickLink sync service — secondary market pricing via OAuth-authenticated API
                //
                builder.Services.AddScoped<BrickLinkSyncService>();

                //
                // BrickEconomy sync service — AI-powered set valuation with daily quota tracking
                //
                builder.Services.AddScoped<BrickEconomySyncService>();

                //
                // Brick Owl sync service — marketplace catalog, cross-platform ID mapping
                //
                builder.Services.AddScoped<BrickOwlSyncService>();


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
                // Add the BMC Database context
                //
                builder.Services.AddDbContext<BMCContext>(options =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("BMC"))
                           .UseLazyLoadingProxies(false);
                });


                //
                // Configure Foundation.AI services (embedding, vector store, inference, RAG)
                //
                builder.Services.AddFoundationAI(ai =>
                {
                    // Embedding provider — uses Ollama's OpenAI-compatible /v1/embeddings endpoint
                    var embeddingSection = builder.Configuration.GetSection("AI:Embedding");
                    ai.Services.AddOpenAiEmbedding(c =>
                    {
                        c.Endpoint = embeddingSection.GetValue<string>("Endpoint") ?? "http://localhost:11434/v1/embeddings";
                        c.Model = embeddingSection.GetValue<string>("Model") ?? "nomic-embed-text";
                        c.Dimension = embeddingSection.GetValue<int>("Dimension");
                        c.ApiKey = embeddingSection.GetValue<string>("ApiKey") ?? "ollama";
                    });

                    // Vector store — Zvec local file-based store
                    string vectorBasePath = builder.Configuration.GetValue<string>("AI:VectorStore:BasePath") ?? "./ai-data/vectors";
                    ai.Services.AddVectorStore(sp => new ZvecVectorStore(new ZvecVectorStoreConfig
                    {
                        BasePath = vectorBasePath
                    }));

                    // Inference provider — OpenAI-compatible (supports OpenAI, Azure, Ollama)
                    var inferenceSection = builder.Configuration.GetSection("AI:Inference");
                    ai.Services.AddOpenAiInference(c =>
                    {
                        c.Endpoint = inferenceSection.GetValue<string>("Endpoint") ?? "http://localhost:11434/v1/chat/completions";
                        c.ApiKey = inferenceSection.GetValue<string>("ApiKey") ?? "ollama";
                        c.Model = inferenceSection.GetValue<string>("Model") ?? "qwen3:30b-a3b";
                    });

                    // RAG orchestration — ties embed + vectorstore + inference together
                    ai.Services.AddRag();
                });

                //
                // Register BMC AI services (BmcSearchIndex + BmcAiService)
                //
                builder.Services.AddBmcAI();


                //
                // Add SignalR for streaming AI chat
                //
                builder.Services.AddSignalR(options =>
                {
                    // Manual generator sends large base64 payloads (PDF can be 10+ MB)
                    options.MaximumReceiveMessageSize = 50 * 1024 * 1024;  // 50 MB

                    // Manual generation can take several minutes for large models
                    options.ClientTimeoutInterval = TimeSpan.FromMinutes(10);
                    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                });


                // Configure Kestrel server options
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.Limits.MaxConcurrentConnections = null;
                    options.Limits.MaxConcurrentUpgradedConnections = null;
                    options.Limits.MaxRequestBodySize = BmcConstants.ONE_GIGABYTE_IN_BYTES;
                });

                //
                // Add CORS
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
                // Custom BMC controllers will be added here as they are created
                //
                controllers.Add(typeof(LDrawController));
                controllers.Add(typeof(CollectionController));
                controllers.Add(typeof(PartsCatalogController));
                controllers.Add(typeof(ProfileController));
                controllers.Add(typeof(AiController));
                controllers.Add(typeof(PartsUniverseController));
                controllers.Add(typeof(SetExplorerController));
                controllers.Add(typeof(MinifigGalleryController));
                controllers.Add(typeof(PartRendererController));
                controllers.Add(typeof(ManualGeneratorController));
                controllers.Add(typeof(PublicShowcaseController));
                controllers.Add(typeof(RebrickableSyncController));
                controllers.Add(typeof(BrickSetSyncController));
                controllers.Add(typeof(BrickLinkSyncController));
                controllers.Add(typeof(BrickEconomySyncController));
                controllers.Add(typeof(BrickOwlSyncController));
                controllers.Add(typeof(UserSetListController));
                controllers.Add(typeof(MySetsController));
                controllers.Add(typeof(MyPartListsController));
                controllers.Add(typeof(MyLostPartsController));
                controllers.Add(typeof(BrickbergController));

                //
                // Start of code generated controller list for BMC module
                //
                controllers.Add(typeof(AchievementsController));
                controllers.Add(typeof(AchievementCategoriesController));
                controllers.Add(typeof(ActivityEventsController));
                controllers.Add(typeof(ActivityEventTypesController));
                controllers.Add(typeof(ApiKeysController));
                controllers.Add(typeof(ApiRequestLogsController));
                controllers.Add(typeof(BrickCategoriesController));
                controllers.Add(typeof(BrickColoursController));
                controllers.Add(typeof(BrickConnectionsController));
                controllers.Add(typeof(BrickElementsController));
                controllers.Add(typeof(BrickPartsController));
                controllers.Add(typeof(BrickPartChangeHistoriesController));
                controllers.Add(typeof(BrickSetSetReviewsController));
                controllers.Add(typeof(BrickSetTransactionsController));
                controllers.Add(typeof(BrickSetUserLinksController));
                controllers.Add(typeof(BrickPartColoursController));
                controllers.Add(typeof(BrickPartConnectorsController));
                controllers.Add(typeof(BrickPartRelationshipsController));
                controllers.Add(typeof(BuildChallengesController));
                controllers.Add(typeof(BuildChallengeChangeHistoriesController));
                controllers.Add(typeof(BuildChallengeEntriesController));
                controllers.Add(typeof(BuildManualsController));
                controllers.Add(typeof(BuildManualChangeHistoriesController));
                controllers.Add(typeof(BuildManualPagesController));
                controllers.Add(typeof(BuildManualStepsController));
                controllers.Add(typeof(BuildStepAnnotationsController));
                controllers.Add(typeof(BuildStepAnnotationTypesController));
                controllers.Add(typeof(BuildStepPartsController));
                controllers.Add(typeof(ColourFinishesController));
                controllers.Add(typeof(ConnectorTypesController));
                controllers.Add(typeof(ConnectorTypeCompatibilitiesController));
                controllers.Add(typeof(ContentReportsController));
                controllers.Add(typeof(ContentReportReasonsController));
                controllers.Add(typeof(ExportFormatsController));
                controllers.Add(typeof(LegoMinifigsController));
                controllers.Add(typeof(LegoSetsController));
                controllers.Add(typeof(LegoSetMinifigsController));
                controllers.Add(typeof(LegoSetPartsController));
                controllers.Add(typeof(LegoSetSubsetsController));
                controllers.Add(typeof(LegoThemesController));
                controllers.Add(typeof(MocCommentsController));
                controllers.Add(typeof(MocFavouritesController));
                controllers.Add(typeof(MocLikesController));
                controllers.Add(typeof(ModelBuildStepsController));
                controllers.Add(typeof(ModelDocumentsController));
                controllers.Add(typeof(ModelDocumentChangeHistoriesController));
                controllers.Add(typeof(ModelStepPartsController));
                controllers.Add(typeof(ModelSubFilesController));
                controllers.Add(typeof(ModerationActionsController));
                controllers.Add(typeof(PartSubFileReferencesController));
                controllers.Add(typeof(PartTypesController));
                controllers.Add(typeof(PendingRegistrationsController));
                controllers.Add(typeof(PlacedBricksController));
                controllers.Add(typeof(PlacedBrickChangeHistoriesController));
                controllers.Add(typeof(PlatformAnnouncementsController));
                controllers.Add(typeof(ProjectsController));
                controllers.Add(typeof(ProjectCameraPresetsController));
                controllers.Add(typeof(ProjectChangeHistoriesController));
                controllers.Add(typeof(ProjectExportsController));
                controllers.Add(typeof(ProjectReferenceImagesController));
                controllers.Add(typeof(ProjectRendersController));
                controllers.Add(typeof(ProjectTagsController));
                controllers.Add(typeof(ProjectTagAssignmentsController));
                controllers.Add(typeof(PublishedMocsController));
                controllers.Add(typeof(PublishedMocChangeHistoriesController));
                controllers.Add(typeof(PublishedMocImagesController));
                controllers.Add(typeof(RebrickableSyncQueuesController));
                controllers.Add(typeof(RebrickableTransactionsController));
                controllers.Add(typeof(RebrickableUserLinksController));
                controllers.Add(typeof(RenderPresetsController));
                controllers.Add(typeof(SharedInstructionsController));
                controllers.Add(typeof(SharedInstructionChangeHistoriesController));
                controllers.Add(typeof(SubmodelsController));
                controllers.Add(typeof(SubmodelChangeHistoriesController));
                controllers.Add(typeof(SubmodelPlacedBricksController));
                controllers.Add(typeof(UserAchievementsController));
                controllers.Add(typeof(UserBadgesController));
                controllers.Add(typeof(UserBadgeAssignmentsController));
                controllers.Add(typeof(UserCollectionsController));
                controllers.Add(typeof(UserCollectionChangeHistoriesController));
                controllers.Add(typeof(UserCollectionPartsController));
                controllers.Add(typeof(UserCollectionSetImportsController));
                controllers.Add(typeof(UserFollowsController));
                controllers.Add(typeof(UserLostPartsController));
                controllers.Add(typeof(UserPartListsController));
                controllers.Add(typeof(UserPartListChangeHistoriesController));
                controllers.Add(typeof(UserPartListItemsController));
                controllers.Add(typeof(UserProfilesController));
                controllers.Add(typeof(UserProfileChangeHistoriesController));
                controllers.Add(typeof(UserProfileLinksController));
                controllers.Add(typeof(UserProfileLinkTypesController));
                controllers.Add(typeof(UserProfilePreferredThemesController));
                controllers.Add(typeof(UserProfileStatsController));
                controllers.Add(typeof(UserSetListsController));
                controllers.Add(typeof(UserSetListChangeHistoriesController));
                controllers.Add(typeof(UserSetListItemsController));
                controllers.Add(typeof(UserSetOwnershipsController));
                controllers.Add(typeof(UserWishlistItemsController));
                //
                // End of code generated controller list for BMC module
                //


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
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });


                //
                // Add Swagger
                //
                builder.Services.AddEndpointsApiExplorer();

                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BMC Server", Version = "v1" });
               
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
                    new Foundation.Services.DbContextHealthProvider<BMCContext>("BMC"));

                //
                // Authenticated Users Provider (for System Health dashboard)
                //
                builder.Services.AddSingleton<Foundation.Services.IAuthenticatedUsersProvider,
                    Foundation.Services.SecurityContextAuthenticatedUsersProvider>();

                //
                // IP Geolocation Services (for login attempt geographic visualization)
                //
                builder.Services.AddSingleton<Foundation.Services.IIpGeolocationService, Foundation.Services.IpApiGeolocationService>();
                builder.Services.AddScoped<Foundation.Services.IpAddressLocationManager>();
                builder.Services.AddHostedService<Foundation.Services.IpAddressLocationWorker>();

                //
                // Configurations
                //
                builder.Services.Configure<AppSettings>(builder.Configuration);


                logger.LogInformation("About to build web application.");


                var app = builder.Build();

                logger.LogInformation("Completed building web application.");

                //
                // Configure sessions
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
                        c.DocumentTitle = "Swagger UI - BMC";
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BMC Server V1");
                        c.OAuthClientId(OidcApplicationManager.SWAGGER_CLIENT_ID);
                        c.EnablePersistAuthorization();
                    });

                    IdentityModelEventSource.ShowPII = true;
                }
                else
                {
                    // Production error handling
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
                    
                    app.UseHsts();
                }

                app.UseHttpsRedirection();

                app.UseAuthentication();
                app.UseAuthorization();


                //
                // Setup CORS based on the environment
                //
                if (app.Environment.IsDevelopment())
                {
                    app.UseCors(builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    );
                }
                else
                {
                    string allowedDomains = Configuration.GetStringConfigurationSetting("AllowedCORSDomains", "https:////k2research.ca;");

                    app.UseCors(builder => builder
                        .WithOrigins(allowedDomains.Split(","))
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .WithHeaders("Content-Type", "Accept", "Authorization")
                        .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                        .SetPreflightMaxAge(TimeSpan.FromMinutes(5))
                    );
                }


                //
                // Add Content Security Policy Header
                //
                app.Use(async (context, next) =>
                {
                    try
                    {
                        string cspPolicy;

                        if (app.Environment.IsDevelopment())
                        {
                            cspPolicy = "default-src 'self'; " +
                                        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                                        "style-src 'self' 'unsafe-inline'; " +
                                        "img-src 'self' data: blob: https://cdn.rebrickable.com; " +
                                        "connect-src 'self' https://cdn.rebrickable.com;";
                        }
                        else
                        {
                            cspPolicy = "default-src 'self'; " +
                                        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                                        "style-src 'self' 'unsafe-inline'; " +
                                        "img-src 'self' data: blob: https://cdn.rebrickable.com; " +
                                        "connect-src 'self' https://cdn.rebrickable.com;";
                        }

                        context.Response.Headers["Content-Security-Policy"] = cspPolicy;
                        await next();
                    }
                    catch (Exception ex)
                    {
                        logger.LogException("Error adding CSP header", ex);
                    }
                });



                app.MapControllers();

                app.MapHub<AiChatHub>("/AiChatSignal");
                app.MapHub<ManualGeneratorHub>("/ManualGeneratorSignal");
                app.MapHub<RebrickableHub>("/RebrickableSyncSignal");

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
                    logger.LogCritical(ex, "An error occurred during creating BMC client applications in OIDC database.");

                    throw;
                }

                //
                // Validate the database schemas
                //
                await ValidateBmcSchema(logger).ConfigureAwait(false);

                await ValidateSecuritySchema(logger).ConfigureAwait(false);

                await ValidateAuditorSchema(logger).ConfigureAwait(false);


                //
                // Log database statistics for startup diagnostics
                //
                using (SecurityContext securityContext = new SecurityContext())
                using (AuditorContext audtiorContext = new AuditorContext())
                using (BMCContext bmcContext = new BMCContext())

                {
                    LogDatabaseStatistics(logger,
                        ("Security", securityContext),
                        ("Auditor", audtiorContext),
                        ("BMC", bmcContext)
                    );
                }


                //
                // Run BMC
                //
                logger.LogSystem("About to run BMC web server.");

                app.Run();
            }
            catch (Exception ex)
            {
                logger.LogException($"BMC is exiting because of exception {ex.Message}", ex);
            }
            finally
            {
                Logger.TerminateApplicationLogging();
            }
        }


        private static async Task ValidateBmcSchema(Logger logger)
        {
            logger.LogInformation("About to validate BMC database schema.");

            try
            {
                await using BMCContext validationContext = new BMCContext();

                DatabaseSchemaValidator<BMCContext> schemaValidator = new DatabaseSchemaValidator<BMCContext>(validationContext, logger);

                DatabaseSchemaValidator<BMCContext>.DatabaseSchemaValidatorResult schemaValidationResult = await schemaValidator.ValidateSchemaAsync("BMC").ConfigureAwait(false);

                if (schemaValidationResult.IsValid == false)
                {
                    logger.LogCritical("BMC database schema validation failed:");
                    foreach (var mismatch in schemaValidationResult.Mismatches)
                    {
                        logger.LogCritical(mismatch);
                    }

                    throw new InvalidOperationException("BMC database schema is out of sync with EF context.");
                }
                else
                {
                    logger.LogCritical("BMC database schema validation passed.");
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An error occurred during BMC database schema validation.");
                throw;
            }

            logger.LogInformation("Completed validation of BMC database schema.");
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
                await using Foundation.Auditor.Database.AuditorContext validationContext = new Foundation.Auditor.Database.AuditorContext();

                DatabaseSchemaValidator<Foundation.Auditor.Database.AuditorContext> schemaValidator = new DatabaseSchemaValidator<Foundation.Auditor.Database.AuditorContext>(validationContext, foundationLogger);

                DatabaseSchemaValidator<Foundation.Auditor.Database.AuditorContext>.DatabaseSchemaValidatorResult schemaValidationResult = await schemaValidator.ValidateSchemaAsync("Auditor").ConfigureAwait(false);

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

            Logger.SetCommonLogger(logger);

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
