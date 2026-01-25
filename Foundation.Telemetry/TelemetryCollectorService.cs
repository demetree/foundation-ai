using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Foundation.Services;
using Foundation.Telemetry.Telemetry.Database;

namespace Foundation.Telemetry
{
    /// <summary>
    /// Service that collects telemetry data from monitored applications at regular intervals.
    /// Uses RecurringJob for scheduling and IMonitoredApplicationService for fetching health data.
    /// </summary>
    public class TelemetryCollectorService
    {
        private const string TELEMETRY_COLLECTOR_JOB = "TELEMETRY_COLLECTOR";
        private const string TELEMETRY_PURGE_JOB = "TELEMETRY_PURGE";

        private readonly IMonitoredApplicationService _monitoredAppService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TelemetryCollectorService> _logger;
        private readonly TelemetryConfiguration _config;
        private readonly List<TelemetryApplicationConfiguration> _applicationConfigs;
        private readonly Func<TelemetryContext> _contextFactory;

        private static TelemetryCollectorService _instance;
        public static TelemetryCollectorService Instance => _instance;

        public TelemetryCollectorService(
            IMonitoredApplicationService monitoredAppService,
            IConfiguration configuration,
            ILogger<TelemetryCollectorService> logger,
            Func<TelemetryContext> contextFactory = null)
        {
            _monitoredAppService = monitoredAppService;
            _configuration = configuration;
            _logger = logger;
            _contextFactory = contextFactory ?? (() => new TelemetryContext());

            // Load configuration
            _config = new TelemetryConfiguration();
            configuration.GetSection("Telemetry").Bind(_config);

            // Load application configurations
            _applicationConfigs = new List<TelemetryApplicationConfiguration>();
            configuration.GetSection("Telemetry:Applications").Bind(_applicationConfigs);

            _instance = this;
        }

        /// <summary>
        /// Initialize the telemetry collector service.
        /// Seeds applications from config and starts the collection job.
        /// </summary>
        public void Initialize()
        {
            if (!_config.Enabled)
            {
                _logger.LogInformation("Telemetry collection is disabled");
                return;
            }

            _logger.LogInformation("Initializing Telemetry Collector Service...");

            // Seed applications from configuration
            SeedApplicationsFromConfig();

            // Start the recurring collection job
            var cronExpression = GetCronExpressionForInterval(_config.CollectionIntervalMinutes);
            RecurringJob.AddOrUpdate(TELEMETRY_COLLECTOR_JOB, () => CollectTelemetryAsync(), cronExpression);

            _logger.LogInformation($"Telemetry collection started with interval: {_config.CollectionIntervalMinutes} minute(s)");

            // Start the purge job (daily at configured time)
            var purgeHour = _config.PurgeRunTimeUtc.Hours;
            var purgeCron = $"0 {purgeHour} * * *"; // Every day at the specified hour
            RecurringJob.AddOrUpdate(TELEMETRY_PURGE_JOB, () => PurgeTelemetryAsync(), purgeCron);

            _logger.LogInformation($"Telemetry purge scheduled for {purgeHour}:00 UTC, retention: {_config.RetentionDays} days");
        }

        /// <summary>
        /// Seed TelemetryApplication table from appsettings.json configuration.
        /// Creates new records for apps not already in the database.
        /// </summary>
        private void SeedApplicationsFromConfig()
        {
            if (_applicationConfigs == null || !_applicationConfigs.Any())
            {
                _logger.LogWarning("No Telemetry:Applications configured in appsettings.json");
                return;
            }

            try
            {
                using (var context = _contextFactory())
                {
                    foreach (var appConfig in _applicationConfigs)
                    {
                        if (string.IsNullOrWhiteSpace(appConfig.Name))
                            continue;

                        var existingApp = context.TelemetryApplications
                            .FirstOrDefault(a => a.name == appConfig.Name);

                        if (existingApp == null)
                        {
                            var newApp = new TelemetryApplication
                            {
                                name = appConfig.Name,
                                url = appConfig.Url,
                                isSelf = appConfig.IsSelf,
                                firstSeen = DateTime.UtcNow
                            };
                            context.TelemetryApplications.Add(newApp);
                            _logger.LogInformation($"Seeding new TelemetryApplication: {appConfig.Name}");
                        }
                        else
                        {
                            // Update URL if changed
                            if (existingApp.url != appConfig.Url)
                            {
                                existingApp.url = appConfig.Url;
                                _logger.LogInformation($"Updated URL for TelemetryApplication: {appConfig.Name}");
                            }
                        }
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to seed TelemetryApplications from config");
            }
        }

        /// <summary>
        /// Main collection method invoked by RecurringJob.
        /// Collects health status from all monitored applications.
        /// </summary>
        public async void CollectTelemetryAsync()
        {
            _logger.LogDebug("Starting telemetry collection run...");

            TelemetryCollectionRun collectionRun = null;
            int applicationsPolled = 0;
            int applicationsSucceeded = 0;
            var errors = new List<string>();

            try
            {
                using (var context = _contextFactory())
                {
                    // Create collection run record
                    collectionRun = new TelemetryCollectionRun
                    {
                        startTime = DateTime.UtcNow
                    };
                    context.TelemetryCollectionRuns.Add(collectionRun);
                    await context.SaveChangesAsync();

                    // Get all registered applications from the database
                    var applications = await context.TelemetryApplications.ToListAsync();
                    applicationsPolled = applications.Count;

                    // Fetch health status from each application using MonitoredApplicationService
                    var statuses = await _monitoredAppService.GetAllApplicationStatusesAsync();

                    foreach (var app in applications)
                    {
                        try
                        {
                            var status = statuses.FirstOrDefault(s =>
                                s.Name.Equals(app.name, StringComparison.OrdinalIgnoreCase) ||
                                s.Url.Equals(app.url, StringComparison.OrdinalIgnoreCase));

                            var snapshot = CreateSnapshotFromStatus(app, collectionRun, status);
                            context.TelemetrySnapshots.Add(snapshot);

                            // Update lastSeen if online
                            if (snapshot.isOnline)
                            {
                                app.lastSeen = DateTime.UtcNow;
                                applicationsSucceeded++;
                            }

                            // Parse HealthData JSON to populate child records
                            if (status?.HealthData != null)
                            {
                                PopulateChildRecordsFromHealthData(context, snapshot, status.HealthData);
                            }
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"{app.name}: {ex.Message}");
                            _logger.LogWarning(ex, $"Failed to collect telemetry for {app.name}");
                        }
                    }

                    // Complete the collection run
                    collectionRun.endTime = DateTime.UtcNow;
                    collectionRun.applicationsPolled = applicationsPolled;
                    collectionRun.applicationsSucceeded = applicationsSucceeded;
                    if (errors.Any())
                    {
                        collectionRun.errorMessage = string.Join("; ", errors);
                    }

                    await context.SaveChangesAsync();
                }

                _logger.LogDebug($"Telemetry collection complete: {applicationsSucceeded}/{applicationsPolled} succeeded");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Telemetry collection run failed");
            }
        }

        /// <summary>
        /// Creates a TelemetrySnapshot from a MonitoredApplicationStatus.
        /// </summary>
        private TelemetrySnapshot CreateSnapshotFromStatus(
            TelemetryApplication app,
            TelemetryCollectionRun run,
            MonitoredApplicationStatus status)
        {
            var snapshot = new TelemetrySnapshot
            {
                telemetryApplicationId = app.id,
                telemetryCollectionRunId = run.id,
                collectedAt = DateTime.UtcNow,
                isOnline = status?.IsAvailable ?? false
            };

            // Parse health data JSON if available
            if (status?.HealthData != null)
            {
                try
                {
                    var healthJson = status.HealthData as JsonElement? ?? 
                        JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(status.HealthData));

                    // Store full JSON for extensibility
                    snapshot.statusJson = healthJson.GetRawText();

                    // Extract specific metrics from the status JSON
                    // ASP.NET Core serializes JSON with camelCase by default
                    if (healthJson.TryGetProperty("application", out var application))
                    {
                        // Uptime is nested under application.uptime.totalSeconds
                        if (application.TryGetProperty("uptime", out var uptime))
                        {
                            if (uptime.TryGetProperty("totalSeconds", out var totalSeconds))
                                snapshot.uptimeSeconds = (long)totalSeconds.GetDouble();
                        }

                        // Memory is nested under application.memory
                        if (application.TryGetProperty("memory", out var memory))
                        {
                            if (memory.TryGetProperty("workingSetMB", out var workingSet))
                                snapshot.memoryWorkingSetMB = workingSet.GetDouble();
                            if (memory.TryGetProperty("gcHeapMB", out var gcHeap))
                                snapshot.memoryGcHeapMB = gcHeap.GetDouble();
                        }

                        // Environment info is nested under application.environment
                        if (application.TryGetProperty("environment", out var environment))
                        {
                            if (environment.TryGetProperty("machineName", out var machine))
                                snapshot.machineName = machine.GetString();
                            if (environment.TryGetProperty("dotNetVersion", out var dotnet))
                                snapshot.dotNetVersion = dotnet.GetString();
                        }
                    }

                    // ThreadPool is at the root level
                    if (healthJson.TryGetProperty("threadPool", out var threadPool))
                    {
                        if (threadPool.TryGetProperty("workerThreads", out var workerThreads))
                        {
                            if (workerThreads.TryGetProperty("available", out var available))
                                snapshot.threadPoolWorkerThreads = available.GetInt32();
                        }
                        if (threadPool.TryGetProperty("completionPortThreads", out var completionPortThreads))
                        {
                            if (completionPortThreads.TryGetProperty("available", out var available))
                                snapshot.threadPoolCompletionPortThreads = available.GetInt32();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse health data JSON for {AppName}", app.name);
                }
            }

            return snapshot;
        }

        /// <summary>
        /// Populate child records (database health, disk health, sessions) from the health data JSON.
        /// </summary>
        private void PopulateChildRecordsFromHealthData(TelemetryContext context, TelemetrySnapshot snapshot, object healthData)
        {
            try
            {
                var healthJson = healthData as JsonElement? ??
                    JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(healthData));

                // Database health records
                if (healthJson.TryGetProperty("databases", out var databases) && databases.ValueKind == JsonValueKind.Array)
                {
                    foreach (var db in databases.EnumerateArray())
                    {
                        var dbHealth = new TelemetryDatabaseHealth
                        {
                            databaseName = db.TryGetProperty("name", out var n) ? n.GetString() : "Unknown",
                            isConnected = db.TryGetProperty("isConnected", out var c) && c.GetBoolean(),
                            status = db.TryGetProperty("status", out var s) ? s.GetString() : null,
                            server = db.TryGetProperty("server", out var srv) ? srv.GetString() : null,
                            provider = db.TryGetProperty("provider", out var p) ? p.GetString() : null,
                            errorMessage = db.TryGetProperty("errorMessage", out var e) ? e.GetString() : null
                        };
                        snapshot.TelemetryDatabaseHealths.Add(dbHealth);
                    }
                }

                // Disk health records
                if (healthJson.TryGetProperty("disks", out var disks) && disks.ValueKind == JsonValueKind.Array)
                {
                    foreach (var disk in disks.EnumerateArray())
                    {
                        var diskHealth = new TelemetryDiskHealth
                        {
                            driveName = disk.TryGetProperty("name", out var n) ? n.GetString() : "Unknown",
                            driveLabel = disk.TryGetProperty("label", out var l) ? l.GetString() : null,
                            totalGB = disk.TryGetProperty("totalGB", out var t) ? t.GetDouble() : 0,
                            freeGB = disk.TryGetProperty("freeGB", out var f) ? f.GetDouble() : 0,
                            freePercent = disk.TryGetProperty("freePercent", out var fp) ? fp.GetDouble() : 0,
                            status = disk.TryGetProperty("status", out var s) ? s.GetString() : null,
                            isApplicationDrive = disk.TryGetProperty("isApplicationDrive", out var ia) && ia.GetBoolean()
                        };
                        snapshot.TelemetryDiskHealths.Add(diskHealth);
                    }
                }

                // Session snapshot
                if (healthJson.TryGetProperty("sessions", out var sessions))
                {
                    var sessionSnapshot = new TelemetrySessionSnapshot
                    {
                        activeSessionCount = sessions.TryGetProperty("activeCount", out var ac) ? ac.GetInt32() : 0,
                        expiredSessionCount = sessions.TryGetProperty("expiredCount", out var ec) ? ec.GetInt32() : 0
                    };
                    if (sessions.TryGetProperty("oldestStart", out var os) && os.ValueKind != JsonValueKind.Null)
                        sessionSnapshot.oldestSessionStart = os.GetDateTime();
                    if (sessions.TryGetProperty("newestStart", out var ns) && ns.ValueKind != JsonValueKind.Null)
                        sessionSnapshot.newestSessionStart = ns.GetDateTime();
                    
                    snapshot.TelemetrySessionSnapshots.Add(sessionSnapshot);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to populate child records from health data");
            }
        }

        /// <summary>
        /// Purge old telemetry data based on retention policy.
        /// </summary>
        public async void PurgeTelemetryAsync()
        {
            _logger.LogInformation($"Starting telemetry purge (retention: {_config.RetentionDays} days)...");

            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-_config.RetentionDays);

                using (var context = _contextFactory())
                {
                    // Delete old collection runs (cascades to snapshots and children)
                    var oldRuns = await context.TelemetryCollectionRuns
                        .Where(r => r.startTime < cutoffDate)
                        .ToListAsync();

                    if (oldRuns.Any())
                    {
                        context.TelemetryCollectionRuns.RemoveRange(oldRuns);
                        await context.SaveChangesAsync();
                        _logger.LogInformation($"Purged {oldRuns.Count} old collection runs");
                    }
                    else
                    {
                        _logger.LogDebug("No telemetry data to purge");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Telemetry purge failed");
            }
        }

        /// <summary>
        /// Manually trigger a telemetry collection run.
        /// </summary>
        public bool TriggerCollection()
        {
            return RecurringJob.TriggerJob(TELEMETRY_COLLECTOR_JOB);
        }

        /// <summary>
        /// Convert collection interval in minutes to a cron expression.
        /// </summary>
        private string GetCronExpressionForInterval(int minutes)
        {
            if (minutes <= 0) minutes = 1;

            if (minutes == 1)
            {
                return RecurringJob.CRON_MINUTELY; // Every minute
            }
            else if (minutes < 60)
            {
                return $"*/{minutes} * * * *"; // Every X minutes
            }
            else
            {
                var hours = minutes / 60;
                return $"0 */{hours} * * *"; // Every X hours
            }
        }

        /// <summary>
        /// Stop the telemetry collector.
        /// </summary>
        public void Stop()
        {
            RecurringJob.RemoveIfExists(TELEMETRY_COLLECTOR_JOB);
            RecurringJob.RemoveIfExists(TELEMETRY_PURGE_JOB);
            _logger.LogInformation("Telemetry collector stopped");
        }
    }
}
