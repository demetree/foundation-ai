//
// VolunteerReminderWorker.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Background service that sends automated reminders to volunteers
// about upcoming assignments (24h and 48h before start).
//
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Foundation.Security;
using Foundation.Security.Database;
using Foundation.Scheduler.Database;

namespace Foundation.Scheduler.Services
{
    /// <summary>
    /// Background worker that runs every hour, finds upcoming volunteer assignments
    /// within configured reminder windows, and sends email/SMS reminders.
    ///
    /// NOTE: Once the 'reminderSentDateTime' column is added to EventResourceAssignment,
    /// enable the deduplication check (marked with TODO below) to prevent duplicate sends.
    /// Until then, reminders are not sent to avoid repeated notifications.
    /// </summary>
    public class VolunteerReminderWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VolunteerReminderWorker> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);

        // Reminder windows: send reminders when event starts within these windows
        private static readonly int[] ReminderHoursBeforeEvent = { 24, 48 };

        public VolunteerReminderWorker(
            IServiceProvider serviceProvider,
            ILogger<VolunteerReminderWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("VolunteerReminderWorker starting with {Interval}h interval", _interval.TotalHours);

            // Wait 30s at startup to let the rest of the app initialize
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in VolunteerReminderWorker");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("VolunteerReminderWorker stopping");
        }

        private async Task ProcessRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<VolunteerNotificationService>();

            var now = DateTime.UtcNow;
            int totalSent = 0;

            foreach (var window in ReminderHoursBeforeEvent)
            {
                var windowStart = now;
                var windowEnd = now.AddHours(window);

                // Find volunteer assignments starting within this window
                var assignments = await db.EventResourceAssignments
                    .Include(a => a.scheduledEvent)
                    .Include(a => a.resource)
                    .Where(a =>
                        a.isVolunteer &&
                        a.active &&
                        !a.deleted &&
                        a.scheduledEvent != null &&
                        a.scheduledEvent.active &&
                        !a.scheduledEvent.deleted &&
                        a.scheduledEvent.startDateTime >= windowStart &&
                        a.scheduledEvent.startDateTime <= windowEnd &&
                        a.reminderSentDateTime == null)
                    .ToListAsync();
                if (assignments.Count == 0) continue;

                _logger.LogInformation("VolunteerReminderWorker: Found {Count} assignments needing {Window}h reminder",
                    assignments.Count, window);

                foreach (var assignment in assignments)
                {
                    try
                    {
                        // Resolve volunteer contact info via VolunteerProfile.linkedUserGuid
                        var volunteerProfile = await db.VolunteerProfiles
                            .Include(vp => vp.resource)
                            .FirstOrDefaultAsync(vp => vp.resourceId == assignment.resourceId);

                        if (volunteerProfile == null) continue;

                        // Get email/phone from the volunteer's linked SecurityUser
                        string email = null;
                        string phone = null;
                        string name = volunteerProfile.resource?.name ?? "Volunteer";

                        if (volunteerProfile.linkedUserGuid.HasValue)
                        {
                            var securityContext = new SecurityContext();
                            var secUser = await securityContext.SecurityUsers
                                .FirstOrDefaultAsync(u => u.objectGuid == volunteerProfile.linkedUserGuid.Value);

                            email = secUser?.emailAddress;
                            phone = secUser?.cellPhoneNumber;
                        }

                        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
                        {
                            _logger.LogDebug("VolunteerReminderWorker: No contact info for volunteer profile {Id}, skipping",
                                volunteerProfile.id);
                            continue;
                        }

                        var hoursUntil = window == 24 ? "tomorrow" : "in 2 days";

                        await notificationService.SendReminderAsync(
                            email, phone, name,
                            assignment.scheduledEvent.name,
                            assignment.scheduledEvent.startDateTime,
                            assignment.scheduledEvent.location,
                            hoursUntil
                        );

                        // Mark reminder as sent
                        assignment.reminderSentDateTime = DateTime.UtcNow;
                        totalSent++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "VolunteerReminderWorker: Error processing reminder for assignment {Id}",
                            assignment.id);
                    }
                }

                await db.SaveChangesAsync();
            }

            if (totalSent > 0)
            {
                _logger.LogInformation("VolunteerReminderWorker: Sent {Count} reminders", totalSent);
            }
        }
    }
}
