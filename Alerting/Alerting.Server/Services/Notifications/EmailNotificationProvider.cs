//
// Email Notification Provider
//
// Sends email notifications via SendGrid using the existing SendGridEmailService.
//
using System;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Services;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Services.Notifications
{
    /// <summary>
    /// Sends notifications via email using SendGrid.
    /// Wraps the existing Foundation SendGridEmailService.
    /// </summary>
    public class EmailNotificationProvider : INotificationProvider
    {
        private readonly ILogger<EmailNotificationProvider> _logger;

        // NotificationChannelType ID for Email (from database seed data)
        public int ChannelTypeId => 1; // Email

        public EmailNotificationProvider(ILogger<EmailNotificationProvider> logger)
        {
            _logger = logger;
        }

        public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
        {
            NotificationLogger.Debug($"EmailNotificationProvider.SendAsync - User: {request.UserObjectGuid}, Email: {request.UserEmail ?? "null"}");

            if (string.IsNullOrWhiteSpace(request.UserEmail))
            {
                NotificationLogger.Warning($"Cannot send email notification to user {request.UserObjectGuid}: No email address configured");
                _logger.LogWarning("Cannot send email notification to user {UserGuid}: No email address configured",
                    request.UserObjectGuid);
                return NotificationResult.Failed("No email address configured for user");
            }

            try
            {
                NotificationLogger.Debug($"Building email content for incident {request.Incident.IncidentKey}");
                var subject = BuildSubject(request);
                var body = BuildHtmlBody(request);

                NotificationLogger.Debug($"Email subject: {subject}");
                NotificationLogger.Debug($"Email body length: {body.Length} characters");
                NotificationLogger.Debug($"Calling SendGridEmailService.SendEmailAsync to {request.UserEmail}");

                var success = await SendGridEmailService.SendEmailAsync(
                    senderEmail: null,      // Use config default
                    senderName: "Alerting System",
                    toEmail: request.UserEmail,
                    subject: subject,
                    body: body,
                    ccEmails: null,
                    includeSignature: false,
                    bodyIsHtml: true
                );

                if (success)
                {
                    NotificationLogger.Info($"Email sent successfully to {request.UserEmail} for incident {request.Incident.IncidentKey}");
                    _logger.LogInformation("Email notification sent successfully to {Email} for incident {IncidentKey}",
                        request.UserEmail, request.Incident.IncidentKey);
                    return NotificationResult.Succeeded();
                }
                else
                {
                    NotificationLogger.Error($"SendGrid returned failure for email to {request.UserEmail} - incident {request.Incident.IncidentKey}");
                    _logger.LogError("Failed to send email notification to {Email} for incident {IncidentKey}",
                        request.UserEmail, request.Incident.IncidentKey);
                    return NotificationResult.Failed("SendGrid delivery failed");
                }
            }
            catch (Exception ex)
            {
                NotificationLogger.Exception($"Exception sending email to {request.UserEmail} for incident {request.Incident.IncidentKey}", ex);
                _logger.LogError(ex, "Exception sending email notification to {Email} for incident {IncidentKey}",
                    request.UserEmail, request.Incident.IncidentKey);
                return NotificationResult.Failed(ex.Message);
            }
        }

        private string BuildSubject(NotificationRequest request)
        {
            return $"[{request.Incident.SeverityName}] {request.Incident.Title}";
        }

        private string BuildHtmlBody(NotificationRequest request)
        {
            var severityColor = GetSeverityColor(request.Incident.SeverityId);
            var createdAtLocal = request.Incident.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, {severityColor} 0%, #1a1a2e 100%); color: white; padding: 24px; }}
        .severity-badge {{ display: inline-block; background: rgba(255,255,255,0.2); padding: 4px 12px; border-radius: 4px; font-size: 12px; font-weight: 600; text-transform: uppercase; margin-bottom: 8px; }}
        .title {{ font-size: 20px; font-weight: 600; margin: 0; }}
        .content {{ padding: 24px; }}
        .meta {{ color: #666; font-size: 14px; margin-bottom: 16px; }}
        .meta-item {{ margin-bottom: 8px; }}
        .meta-label {{ font-weight: 600; color: #333; }}
        .description {{ background: #f8f9fa; padding: 16px; border-radius: 6px; border-left: 4px solid {severityColor}; margin-top: 16px; }}
        .actions {{ padding: 24px; background: #f8f9fa; text-align: center; }}
        .btn {{ display: inline-block; background: #0d6efd; color: white; padding: 12px 24px; border-radius: 6px; text-decoration: none; font-weight: 600; margin: 0 8px; }}
        .btn-secondary {{ background: #6c757d; }}
        .footer {{ padding: 16px 24px; text-align: center; font-size: 12px; color: #999; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <span class='severity-badge'>{request.Incident.SeverityName}</span>
            <h1 class='title'>{System.Net.WebUtility.HtmlEncode(request.Incident.Title)}</h1>
        </div>
        <div class='content'>
            <div class='meta'>
                <div class='meta-item'><span class='meta-label'>Incident Key:</span> {request.Incident.IncidentKey}</div>
                <div class='meta-item'><span class='meta-label'>Service:</span> {System.Net.WebUtility.HtmlEncode(request.Incident.ServiceName ?? "Unknown")}</div>
                <div class='meta-item'><span class='meta-label'>Status:</span> {request.Incident.StatusName}</div>
                <div class='meta-item'><span class='meta-label'>Created:</span> {createdAtLocal}</div>
            </div>
            {(string.IsNullOrEmpty(request.Incident.Description) ? "" : $"<div class='description'>{System.Net.WebUtility.HtmlEncode(request.Incident.Description)}</div>")}
        </div>
        <div class='actions'>
            <a href='#' class='btn'>Acknowledge</a>
            <a href='#' class='btn btn-secondary'>View Incident</a>
        </div>
        <div class='footer'>
            You are receiving this because you are on-call for this service.
        </div>
    </div>
</body>
</html>";
        }

        private string GetSeverityColor(int severityId)
        {
            return severityId switch
            {
                1 => "#dc3545", // Critical - Red
                2 => "#fd7e14", // High - Orange
                3 => "#ffc107", // Medium - Amber
                4 => "#0dcaf0", // Low - Cyan
                _ => "#6c757d"  // Default - Gray
            };
        }
    }
}
