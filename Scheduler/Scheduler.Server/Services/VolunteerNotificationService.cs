//
// VolunteerNotificationService.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Sends templated email and SMS notifications to volunteers.
// Uses Foundation SendGridEmailService for email and TwilioSmsService for SMS.
//
using System;
using System.Threading.Tasks;
using Foundation.Services;
using Microsoft.Extensions.Logging;

namespace Foundation.Scheduler.Services
{
    /// <summary>
    /// Sends templated notifications (email/SMS) to volunteers for assignments,
    /// reminders, confirmations, and thank-you messages.
    /// </summary>
    public class VolunteerNotificationService
    {
        private readonly ILogger<VolunteerNotificationService> _logger;

        // Configuration keys
        private const string CONFIG_SENDER_EMAIL = "VolunteerHub:SenderEmail";
        private const string CONFIG_SENDER_NAME = "VolunteerHub:SenderName";
        private const string CONFIG_ORG_NAME = "VolunteerHub:OrganizationName";

        public VolunteerNotificationService(ILogger<VolunteerNotificationService> logger)
        {
            _logger = logger;
        }


        // ─────── Assignment Notification ───────

        /// <summary>
        /// Notify volunteer of a new assignment.
        /// </summary>
        public async Task SendAssignmentNotificationAsync(
            string volunteerEmail, string volunteerPhone, string volunteerName,
            string eventName, DateTime eventDate, string eventLocation)
        {
            var orgName = Foundation.Configuration.GetStringConfigurationSetting(CONFIG_ORG_NAME, "Our Organization");
            var subject = $"You've been assigned to: {eventName}";

            var body = BuildHtmlWrapper($@"
                <h2 style='color: #4F46E5; margin: 0 0 8px 0;'>Assignment Notification</h2>
                <p>Hi {System.Net.WebUtility.HtmlEncode(volunteerName)},</p>
                <p>You've been assigned to the following event:</p>
                <table style='border-collapse: collapse; width: 100%; margin: 12px 0;'>
                    <tr><td style='padding: 8px; font-weight: 600; color: #6B7280;'>Event</td>
                        <td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(eventName)}</td></tr>
                    <tr style='background: #F3F4F6;'><td style='padding: 8px; font-weight: 600; color: #6B7280;'>Date</td>
                        <td style='padding: 8px;'>{eventDate:dddd, MMMM d, yyyy 'at' h:mm tt}</td></tr>
                    <tr><td style='padding: 8px; font-weight: 600; color: #6B7280;'>Location</td>
                        <td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(eventLocation ?? "TBD")}</td></tr>
                </table>
                <p>Log in to your Volunteer Hub to accept or view details.</p>
                <p style='color: #9CA3AF; font-size: 12px;'>— {System.Net.WebUtility.HtmlEncode(orgName)}</p>");

            await SendEmailAsync(volunteerEmail, volunteerName, subject, body);

            if (!string.IsNullOrWhiteSpace(volunteerPhone))
            {
                var smsBody = $"[{orgName}] You've been assigned to {eventName} on {eventDate:MMM d} at {eventDate:h:mm tt}. Log in to your Volunteer Hub for details.";
                await SendSmsAsync(volunteerPhone, smsBody);
            }
        }


        // ─────── Reminder ───────

        /// <summary>
        /// Send an upcoming event reminder.
        /// </summary>
        public async Task SendReminderAsync(
            string volunteerEmail, string volunteerPhone, string volunteerName,
            string eventName, DateTime eventDate, string eventLocation, string hoursUntil)
        {
            var orgName = Foundation.Configuration.GetStringConfigurationSetting(CONFIG_ORG_NAME, "Our Organization");
            var subject = $"Reminder: {eventName} is {hoursUntil}";

            var body = BuildHtmlWrapper($@"
                <h2 style='color: #F59E0B; margin: 0 0 8px 0;'>⏰ Event Reminder</h2>
                <p>Hi {System.Net.WebUtility.HtmlEncode(volunteerName)},</p>
                <p>This is a friendly reminder that you have an upcoming event <strong>{hoursUntil}</strong>:</p>
                <table style='border-collapse: collapse; width: 100%; margin: 12px 0;'>
                    <tr><td style='padding: 8px; font-weight: 600; color: #6B7280;'>Event</td>
                        <td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(eventName)}</td></tr>
                    <tr style='background: #F3F4F6;'><td style='padding: 8px; font-weight: 600; color: #6B7280;'>Date</td>
                        <td style='padding: 8px;'>{eventDate:dddd, MMMM d, yyyy 'at' h:mm tt}</td></tr>
                    <tr><td style='padding: 8px; font-weight: 600; color: #6B7280;'>Location</td>
                        <td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(eventLocation ?? "TBD")}</td></tr>
                </table>
                <p style='color: #9CA3AF; font-size: 12px;'>— {System.Net.WebUtility.HtmlEncode(orgName)}</p>");

            await SendEmailAsync(volunteerEmail, volunteerName, subject, body);

            if (!string.IsNullOrWhiteSpace(volunteerPhone))
            {
                var smsBody = $"[{orgName}] Reminder: {eventName} is {hoursUntil} ({eventDate:MMM d, h:mm tt}). Location: {eventLocation ?? "TBD"}";
                await SendSmsAsync(volunteerPhone, smsBody);
            }
        }


        // ─────── Confirmation ───────

        /// <summary>
        /// Confirm a volunteer's sign-up for an opportunity.
        /// </summary>
        public async Task SendSignUpConfirmationAsync(
            string volunteerEmail, string volunteerPhone, string volunteerName,
            string eventName, DateTime eventDate)
        {
            var orgName = Foundation.Configuration.GetStringConfigurationSetting(CONFIG_ORG_NAME, "Our Organization");
            var subject = $"Confirmed: You're signed up for {eventName}";

            var body = BuildHtmlWrapper($@"
                <h2 style='color: #10B981; margin: 0 0 8px 0;'>✅ Sign-Up Confirmed</h2>
                <p>Hi {System.Net.WebUtility.HtmlEncode(volunteerName)},</p>
                <p>Your sign-up for <strong>{System.Net.WebUtility.HtmlEncode(eventName)}</strong> on <strong>{eventDate:MMMM d, yyyy}</strong> has been confirmed!</p>
                <p>We look forward to seeing you there.</p>
                <p style='color: #9CA3AF; font-size: 12px;'>— {System.Net.WebUtility.HtmlEncode(orgName)}</p>");

            await SendEmailAsync(volunteerEmail, volunteerName, subject, body);

            if (!string.IsNullOrWhiteSpace(volunteerPhone))
            {
                var smsBody = $"[{orgName}] Confirmed: You're signed up for {eventName} on {eventDate:MMM d}. Thank you!";
                await SendSmsAsync(volunteerPhone, smsBody);
            }
        }


        // ─────── Thank You (Post-Event) ───────

        /// <summary>
        /// Send a post-event thank you with hours summary.
        /// </summary>
        public async Task SendThankYouAsync(
            string volunteerEmail, string volunteerName,
            string eventName, DateTime eventDate, float hoursServed)
        {
            var orgName = Foundation.Configuration.GetStringConfigurationSetting(CONFIG_ORG_NAME, "Our Organization");
            var subject = $"Thank you for volunteering at {eventName}!";

            var body = BuildHtmlWrapper($@"
                <h2 style='color: #4F46E5; margin: 0 0 8px 0;'>🎉 Thank You!</h2>
                <p>Hi {System.Net.WebUtility.HtmlEncode(volunteerName)},</p>
                <p>Thank you for volunteering at <strong>{System.Net.WebUtility.HtmlEncode(eventName)}</strong> on {eventDate:MMMM d, yyyy}!</p>
                <p>You contributed <strong style='color: #4F46E5; font-size: 1.1em;'>{hoursServed:F1} hours</strong> of service.</p>
                <p>Your dedication makes a real difference. We hope to see you again soon!</p>
                <p style='color: #9CA3AF; font-size: 12px;'>— {System.Net.WebUtility.HtmlEncode(orgName)}</p>");

            await SendEmailAsync(volunteerEmail, volunteerName, subject, body);
        }


        // ─────── Welcome (Post-Registration Approval) ───────

        /// <summary>
        /// Send a welcome email after registration approval.
        /// </summary>
        public async Task SendWelcomeAsync(
            string volunteerEmail, string volunteerName)
        {
            var orgName = Foundation.Configuration.GetStringConfigurationSetting(CONFIG_ORG_NAME, "Our Organization");
            var subject = $"Welcome to {orgName}'s volunteer team!";

            var body = BuildHtmlWrapper($@"
                <h2 style='color: #4F46E5; margin: 0 0 8px 0;'>🙌 Welcome!</h2>
                <p>Hi {System.Net.WebUtility.HtmlEncode(volunteerName)},</p>
                <p>Your volunteer registration with <strong>{System.Net.WebUtility.HtmlEncode(orgName)}</strong> has been approved!</p>
                <p>You can now log in to the Volunteer Hub to:</p>
                <ul>
                    <li>View and sign up for opportunities</li>
                    <li>See your schedule and assignments</li>
                    <li>Track your volunteer hours</li>
                    <li>Update your profile and preferences</li>
                </ul>
                <p>To log in, visit the Volunteer Hub and enter your email address to receive a one-time code.</p>
                <p style='color: #9CA3AF; font-size: 12px;'>— {System.Net.WebUtility.HtmlEncode(orgName)}</p>");

            await SendEmailAsync(volunteerEmail, volunteerName, subject, body);
        }


        // ─────── Helpers ───────

        private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning("VolunteerNotificationService: Cannot send email — no email address for {Name}", toName);
                return;
            }

            var senderEmail = Foundation.Configuration.GetStringConfigurationSetting(CONFIG_SENDER_EMAIL, null);
            var senderName = Foundation.Configuration.GetStringConfigurationSetting(CONFIG_SENDER_NAME, "Volunteer Hub");

            try
            {
                var result = await SendGridEmailService.SendEmailAsync(
                    senderEmail, senderName, toEmail, subject, htmlBody, bodyIsHtml: true, includeSignature: false);

                if (result)
                {
                    _logger.LogInformation("VolunteerNotificationService: Email sent to {Email} — {Subject}", toEmail, subject);
                }
                else
                {
                    _logger.LogWarning("VolunteerNotificationService: Email failed to {Email} — {Subject}", toEmail, subject);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VolunteerNotificationService: Exception sending email to {Email}", toEmail);
            }
        }

        private async Task SendSmsAsync(string toPhone, string messageBody)
        {
            try
            {
                var (success, sid, error) = await TwilioSmsService.SendSmsAsync(toPhone, messageBody);

                if (success)
                {
                    _logger.LogInformation("VolunteerNotificationService: SMS sent to {Phone} (SID: {Sid})",
                        TwilioSmsService.MaskPhoneNumber(toPhone), sid);
                }
                else
                {
                    _logger.LogWarning("VolunteerNotificationService: SMS failed to {Phone}: {Error}",
                        TwilioSmsService.MaskPhoneNumber(toPhone), error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VolunteerNotificationService: Exception sending SMS to {Phone}",
                    TwilioSmsService.MaskPhoneNumber(toPhone));
            }
        }

        private string BuildHtmlWrapper(string innerContent)
        {
            return $@"
            <div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; max-width: 560px; margin: 0 auto; padding: 24px; background: #FFFFFF; border-radius: 8px; border: 1px solid #E5E7EB;'>
                {innerContent}
            </div>";
        }
    }
}
