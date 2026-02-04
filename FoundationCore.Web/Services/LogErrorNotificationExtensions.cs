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
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Foundation;
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
                Logger logger = Logger.GetCommonLogger();

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
                    else
                    {
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
                // Register with the common logger to receive log entries
                //
                logger.AddConsumer(consumer);

                logger.LogInformation($"[LogErrorNotification] Initialized for {options.SystemName}. " +
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
                Logger logger = Logger.GetCommonLogger();

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
                // Register with the common logger
                //
                logger.AddConsumer(consumer);

                logger.LogInformation($"[LogErrorNotification] Initialized for {options.SystemName}.");

                return consumer;
            });

            return services;
        }
    }
}
