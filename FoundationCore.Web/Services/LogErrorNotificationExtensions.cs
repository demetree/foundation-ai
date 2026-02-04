//
// Log Error Notification Extensions
//
// DI extension methods for integrating LogErrorNotificationConsumer
// with ASP.NET Core dependency injection.
//
// AI Generated - Feb 2026
//
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Foundation;
using Foundation.Security;
using Foundation.Services;
using Foundation.Web.Services.Alerting;

namespace Foundation.Web.Services
{
    /// <summary>
    /// 
    /// Extension methods for adding log error notification to DI.
    /// 
    /// </summary>
    public static class LogErrorNotificationExtensions
    {
        /// <summary>
        /// 
        /// Initializes log error notification early in startup (before DI builds).
        /// Reads configuration from the "LogErrorNotification" section of appsettings.json.
        /// Supports both email and alerting channels.
        /// 
        /// </summary>
        /// <param name="configuration">Application configuration (e.g., from GetConfiguration()).</param>
        /// <returns>The initialized consumer instance.</returns>
        public static LogErrorNotificationConsumer InitializeFromConfiguration(IConfiguration configuration)
        {
            //
            // Read options from configuration
            //
            LogErrorNotificationOptions options = new LogErrorNotificationOptions();
            configuration.GetSection("LogErrorNotification").Bind(options);


            if (options.Enabled == false) 
            {
                return null;
            }

            //
            // Set up email sender delegate if configured
            //
            Func<string, string, Task<bool>> emailSender = null;

            if (options.EnableEmail == true && options.NotificationEmails?.Any() == true)
            {
                emailSender = (subject, body) =>
                    SendGridEmailService.SendEmailToMultipleRecipientsAsync(senderEmail: options.EmailFromAddress,
                                                                             senderName: options.EmailFromName,
                                                                             toEmails: options.NotificationEmails,
                                                                             subject: subject,
                                                                             body: body,
                                                                             includeSignature: false,
                                                                             bodyIsHtml: false);
            }

            //
            // Set up alerting delegate if configured (using standalone HTTP client)
            // Reads from "Alerting" section and attempts to get API key from SystemSettings
            //
            Func<string, string, Task> alertSender = null;

            if (options.EnableAlerting == true)
            {
                // Read Alerting configuration
                string alertingBaseUrl = configuration.GetValue<string>("Alerting:BaseUrl");
                string alertingApiKey = configuration.GetValue<string>("Alerting:ApiKey");  // Optional in config
                string serviceName = configuration.GetValue<string>("Alerting:ServiceName");

                if (!string.IsNullOrEmpty(alertingBaseUrl))
                {
                    alertSender = async (title, description) =>
                    {
                        try
                        {
                            // Try to get API key: first from config, then from SystemSettings
                            string apiKey = alertingApiKey;

                            if (string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(serviceName))
                            {
                                try
                                {
                                    string settingName = $"Alerting:Integration:{serviceName}:ApiKey";
                                    apiKey = await SystemSettings.GetSystemSettingAsync(settingName, null).ConfigureAwait(false);
                                }
                                catch
                                {
                                    // Database may not be available at early startup - that's okay
                                }
                            }

                            if (string.IsNullOrEmpty(apiKey))
                            {
                                Console.WriteLine("[LogErrorNotification] Alerting API key not available. Skipping alert.");
                                return;
                            }

                            // Make direct HTTP call to Alerting API
                            await RaiseAlertDirectAsync(alertingBaseUrl, apiKey, options, title, description).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[LogErrorNotification] Failed to raise alert: {ex.Message}");
                        }
                    };

                    Console.WriteLine($"[LogErrorNotification] Alerting configured for {alertingBaseUrl}");
                }
                else
                {
                    Console.WriteLine("[LogErrorNotification] Alerting is enabled but Alerting:BaseUrl is not configured. Alerting notifications will be skipped.");
                }
            }

            //
            // Initialize the consumer
            //
            return LogErrorNotificationConsumer.Initialize(options, emailSender, alertSender);
        }


        /// <summary>
        /// 
        /// Initializes log error notification with full control.
        /// Use this if you want to provide custom email and alerting delegates.
        /// 
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="alertingService">Optional alerting service (for DI scenarios).</param>
        /// <returns>The initialized consumer instance.</returns>
        public static LogErrorNotificationConsumer InitializeFromConfiguration(IConfiguration configuration,
                                                                                 IAlertingIntegrationService alertingService)
        {
            //
            // Read options from configuration
            //
            LogErrorNotificationOptions options = new LogErrorNotificationOptions();
            configuration.GetSection("LogErrorNotification").Bind(options);

            //
            // Set up email sender delegate if configured
            //
            Func<string, string, Task<bool>> emailSender = null;

            if (options.EnableEmail == true && options.NotificationEmails?.Any() == true)
            {
                emailSender = (subject, body) =>
                    SendGridEmailService.SendEmailToMultipleRecipientsAsync(senderEmail: options.EmailFromAddress,
                                                                             senderName: options.EmailFromName,
                                                                             toEmails: options.NotificationEmails,
                                                                             subject: subject,
                                                                             body: body,
                                                                             includeSignature: false,
                                                                             bodyIsHtml: false);
            }

            //
            // Set up alerting delegate using the provided DI service
            //
            Func<string, string, Task> alertSender = null;

            if (options.EnableAlerting == true && alertingService?.IsRegistered == true)
            {
                alertSender = async (title, description) =>
                {
                    try
                    {
                        await alertingService.RaiseIncidentAsync(new RaiseIncidentRequest
                        {
                            Severity = options.AlertingSeverity,
                            Title = title,
                            Description = description,
                            DeduplicationKey = $"{options.SystemName}-log-errors",
                            Source = options.SystemName
                        }).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[LogErrorNotification] Failed to raise alert: {ex.Message}");
                    }
                };
            }

            //
            // Initialize the consumer
            //
            return LogErrorNotificationConsumer.Initialize(options, emailSender, alertSender);
        }


        /// <summary>
        /// 
        /// Makes a direct HTTP call to the Alerting API without requiring DI.
        /// 
        /// </summary>
        private static async Task RaiseAlertDirectAsync(string baseUrl,
                                                         string apiKey,
                                                         LogErrorNotificationOptions options,
                                                         string title,
                                                         string description)
        {
            using HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(30);

            var payload = new
            {
                severity = options.AlertingSeverity ?? "High",
                title = title,
                description = description,
                deduplicationKey = $"{options.SystemName}-log-errors",
                source = options.SystemName
            };

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/alerts/v1/enqueue");

            request.Headers.Add("X-Api-Key", apiKey);
            request.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload),
                                                 System.Text.Encoding.UTF8,
                                                 "application/json");

            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Console.WriteLine($"[LogErrorNotification] Alerting API returned {response.StatusCode}: {content}");
            }
        }


        /// <summary>
        /// 
        /// Adds the LogErrorNotificationConsumer to the service collection.
        /// Reads configuration from the "LogErrorNotification" section.
        /// 
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddLogErrorNotification(this IServiceCollection services,
                                                                  IConfiguration configuration)
        {
            //
            // Bind configuration
            //
            services.Configure<LogErrorNotificationOptions>(configuration.GetSection("LogErrorNotification"));

            //
            // Register as singleton - the consumer must persist for the lifetime of the app
            //
            services.AddSingleton<LogErrorNotificationConsumer>(sp =>
            {
                LogErrorNotificationOptions options = sp.GetRequiredService<IOptions<LogErrorNotificationOptions>>().Value;

                //
                // Set up email sender delegate if configured
                //
                Func<string, string, Task<bool>> emailSender = null;

                if (options.EnableEmail == true && options.NotificationEmails?.Any() == true)
                {
                    emailSender = (subject, body) =>
                        SendGridEmailService.SendEmailToMultipleRecipientsAsync(senderEmail: options.EmailFromAddress,
                                                                                 senderName: options.EmailFromName,
                                                                                 toEmails: options.NotificationEmails,
                                                                                 subject: subject,
                                                                                 body: body,
                                                                                 includeSignature: false,
                                                                                 bodyIsHtml: false);
                }

                //
                // Set up alerting delegate if configured
                //
                Func<string, string, Task> alertSender = null;

                if (options.EnableAlerting == true)
                {
                    IAlertingIntegrationService alertingService = sp.GetService<IAlertingIntegrationService>();

                    if (alertingService?.IsRegistered == true)
                    {
                        alertSender = async (title, description) =>
                        {
                            try
                            {
                                await alertingService.RaiseIncidentAsync(new RaiseIncidentRequest
                                {
                                    Severity = options.AlertingSeverity,
                                    Title = title,
                                    Description = description,
                                    DeduplicationKey = $"{options.SystemName}-log-errors",
                                    Source = options.SystemName
                                }).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                // Don't write to the log here because it could risk endless loops
                                Console.WriteLine($"[LogErrorNotification] Failed to raise alert: {ex.Message}");
                            }
                        };
                    }
                    else
                    {
                        // Don't write to the log here because it could risk endless loops
                        Console.WriteLine("[LogErrorNotification] Alerting is enabled but IAlertingIntegrationService is not registered. Alerting notifications will be skipped.");
                    }
                }

                //
                // Create the consumer
                //
                LogErrorNotificationConsumer consumer = new LogErrorNotificationConsumer(options: options,
                                                                                          sendEmailAsync: emailSender,
                                                                                          raiseAlertAsync: alertSender);

                //
                // Register with ALL loggers globally to receive log entries from every logger in the system
                //
                Logger.AddGlobalConsumer(consumer);

                Logger.GetCommonLogger().LogInformation($"[LogErrorNotification] Initialized for {options.SystemName}. " +
                                      $"Email: {options.EnableEmail}, " +
                                      $"Alerting: {options.EnableAlerting}, " +
                                      $"BatchWindow: {options.BatchWindowMinutes}min");

                return consumer;
            });

            return services;
        }


        /// <summary>
        /// 
        /// Adds the LogErrorNotificationConsumer with programmatic configuration.
        /// 
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">Action to configure options.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddLogErrorNotification(this IServiceCollection services,
                                                                  Action<LogErrorNotificationOptions> configure)
        {
            //
            // Configure options via action
            //
            services.Configure(configure);

            //
            // Register as singleton - same logic as config-based version
            //
            services.AddSingleton<LogErrorNotificationConsumer>(sp =>
            {
                LogErrorNotificationOptions options = sp.GetRequiredService<IOptions<LogErrorNotificationOptions>>().Value;

                //
                // Set up email sender delegate if configured
                //
                Func<string, string, Task<bool>> emailSender = null;

                if (options.EnableEmail == true && options.NotificationEmails?.Any() == true)
                {
                    emailSender = (subject, body) =>
                        SendGridEmailService.SendEmailToMultipleRecipientsAsync(senderEmail: options.EmailFromAddress,
                                                                                 senderName: options.EmailFromName,
                                                                                 toEmails: options.NotificationEmails,
                                                                                 subject: subject,
                                                                                 body: body,
                                                                                 includeSignature: false,
                                                                                 bodyIsHtml: false);
                }

                //
                // Set up alerting delegate if configured
                //
                Func<string, string, Task> alertSender = null;

                if (options.EnableAlerting == true)
                {
                    IAlertingIntegrationService alertingService = sp.GetService<IAlertingIntegrationService>();

                    if (alertingService?.IsRegistered == true)
                    {
                        alertSender = async (title, description) =>
                        {
                            try
                            {
                                await alertingService.RaiseIncidentAsync(new RaiseIncidentRequest
                                {
                                    Severity = options.AlertingSeverity,
                                    Title = title,
                                    Description = description,
                                    DeduplicationKey = $"{options.SystemName}-log-errors",
                                    Source = options.SystemName
                                }).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[LogErrorNotification] Failed to raise alert: {ex.Message}");
                            }
                        };
                    }
                }

                //
                // Create the consumer
                //
                LogErrorNotificationConsumer consumer = new LogErrorNotificationConsumer(options: options,
                                                                                          sendEmailAsync: emailSender,
                                                                                          raiseAlertAsync: alertSender);

                //
                // Register with ALL loggers globally
                //
                Logger.AddGlobalConsumer(consumer);

                Logger.GetCommonLogger().LogInformation($"[LogErrorNotification] Initialized for {options.SystemName}.");

                return consumer;
            });

            return services;
        }
    }
}
