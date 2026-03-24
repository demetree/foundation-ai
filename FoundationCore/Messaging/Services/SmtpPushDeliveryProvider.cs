using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// SMTP Push Delivery Provider - delivers notifications via email using System.Net.Mail.
    /// 
    /// Configuration is read from appsettings.json under the "Messaging:Smtp" section:
    /// 
    ///   "Messaging": {
    ///     "Smtp": {
    ///       "Host": "smtp.example.com",
    ///       "Port": 587,
    ///       "UseSsl": true,
    ///       "Username": "notifications@example.com",
    ///       "Password": "...",
    ///       "FromAddress": "notifications@example.com",
    ///       "FromName": "Catalyst Messaging",
    ///       "Enabled": true
    ///     }
    ///   }
    /// 
    /// AI-developed as part of Foundation.Messaging Phase 3B, March 2026.
    /// 
    /// </summary>
    public class SmtpPushDeliveryProvider : IPushDeliveryProvider
    {
        private readonly string _host;
        private readonly int _port;
        private readonly bool _useSsl;
        private readonly string _username;
        private readonly string _password;
        private readonly string _fromAddress;
        private readonly string _fromName;
        private readonly bool _enabled;


        public string ProviderId => "email";
        public string DisplayName => "Email (SMTP)";
        public bool IsEnabled => _enabled;


        public SmtpPushDeliveryProvider(IConfiguration configuration)
        {
            IConfigurationSection smtp = configuration?.GetSection("Messaging:Smtp");

            _host = smtp?.GetValue<string>("Host") ?? "";
            _port = smtp?.GetValue<int>("Port") ?? 587;
            _useSsl = smtp?.GetValue<bool>("UseSsl") ?? true;
            _username = smtp?.GetValue<string>("Username") ?? "";
            _password = smtp?.GetValue<string>("Password") ?? "";
            _fromAddress = smtp?.GetValue<string>("FromAddress") ?? "";
            _fromName = smtp?.GetValue<string>("FromName") ?? "Catalyst Messaging";
            _enabled = smtp?.GetValue<bool>("Enabled") ?? false;
        }


        /// <summary>
        /// Delivers a notification via SMTP email.
        /// </summary>
        public async Task<PushDeliveryResult> DeliverAsync(PushDeliveryRequest request, CancellationToken cancellationToken = default)
        {
            if (_enabled == false)
            {
                return PushDeliveryResult.Failed(ProviderId, "SMTP provider is not enabled.");
            }

            if (string.IsNullOrWhiteSpace(_host))
            {
                return PushDeliveryResult.Failed(ProviderId, "SMTP host is not configured.");
            }

            if (string.IsNullOrWhiteSpace(request?.Destination))
            {
                return PushDeliveryResult.Failed(ProviderId, "No email destination specified.");
            }

            try
            {
                using (SmtpClient client = new SmtpClient(_host, _port))
                {
                    client.EnableSsl = _useSsl;

                    if (string.IsNullOrWhiteSpace(_username) == false)
                    {
                        client.Credentials = new NetworkCredential(_username, _password);
                    }

                    using (MailMessage message = new MailMessage())
                    {
                        message.From = new MailAddress(_fromAddress, _fromName);
                        message.To.Add(new MailAddress(request.Destination));

                        //
                        // Build subject line
                        //
                        if (string.IsNullOrWhiteSpace(request.Subject) == false)
                        {
                            message.Subject = request.Subject;
                        }
                        else if (string.IsNullOrWhiteSpace(request.ConversationName) == false)
                        {
                            message.Subject = $"New message in {request.ConversationName}";
                        }
                        else
                        {
                            message.Subject = "New Notification";
                        }

                        //
                        // Build body — prefer HTML, fall back to plain text
                        //
                        if (string.IsNullOrWhiteSpace(request.HtmlBody) == false)
                        {
                            message.Body = request.HtmlBody;
                            message.IsBodyHtml = true;

                            //
                            // Add a plain-text alternative view for email clients that don't render HTML
                            //
                            if (string.IsNullOrWhiteSpace(request.Body) == false)
                            {
                                message.AlternateViews.Add(
                                    AlternateView.CreateAlternateViewFromString(request.Body, null, "text/plain"));
                            }
                        }
                        else
                        {
                            message.Body = request.Body ?? "You have a new notification.";
                            message.IsBodyHtml = false;
                        }

                        //
                        // Add custom headers for tracking
                        //
                        if (request.ConversationId.HasValue)
                        {
                            message.Headers.Add("X-Messaging-ConversationId", request.ConversationId.Value.ToString());
                        }

                        if (request.MessageId.HasValue)
                        {
                            message.Headers.Add("X-Messaging-MessageId", request.MessageId.Value.ToString());
                        }

                        message.Headers.Add("X-Messaging-TenantGuid", request.TenantGuid.ToString());

                        await client.SendMailAsync(message, cancellationToken);
                    }
                }

                return PushDeliveryResult.Succeeded(ProviderId);
            }
            catch (SmtpException ex)
            {
                return PushDeliveryResult.Failed(ProviderId, $"SMTP error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return PushDeliveryResult.Failed(ProviderId, $"Email delivery failed: {ex.Message}");
            }
        }


        /// <summary>
        /// Validates that the destination is a well-formed email address.
        /// </summary>
        public bool ValidateDestination(string destination)
        {
            if (string.IsNullOrWhiteSpace(destination))
            {
                return false;
            }

            try
            {
                MailAddress addr = new MailAddress(destination);
                return addr.Address == destination.Trim();
            }
            catch
            {
                return false;
            }
        }
    }
}
