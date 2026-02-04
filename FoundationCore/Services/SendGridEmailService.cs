using System.IO;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;


namespace Foundation.Services
{
    public static class SendGridEmailService
    {
        // Logging Setup
        private const string LOG_DIRECTORY = "Log";
        private const string LOG_FILENAME = "SendGridEmailService";
        private static readonly Logger _logger = new Logger();
        public static Logger Logger { get { return _logger; } }


        public static async Task<bool> SendEmailAsync(string senderEmail,
                                                      string senderName,
                                                      string toEmail,
                                                      string subject,
                                                      string body,
                                                      List<string> ccEmails = null,
                                                      bool includeSignature = true,
                                                      bool bodyIsHtml = false)
        {
            InitializeLogger();

            try
            {
                //
                // Get the email configuration parameters from the configuration
                //
                string apiKey = Foundation.Configuration.GetStringConfigurationSetting("SendGridAPIKey", null);
                string configEmailFrom = Foundation.Configuration.GetStringConfigurationSetting("EmailFromAddress", "donotreply@notconfigured.com");
                
                string displayName = senderName;

                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new Exception("SendGrid API key is missing.");
                }

                SendGridClient client = new SendGridClient(apiKey);
                EmailAddress from;
                EmailAddress replyTo;

                if (string.IsNullOrEmpty(senderEmail) == true)
                {
                    from = new EmailAddress(configEmailFrom, displayName);
                    replyTo = new EmailAddress(configEmailFrom, senderName);
                }
                else
                {
                    from = new EmailAddress(senderEmail, displayName);
                    replyTo = new EmailAddress(senderEmail, senderName);
                }

                EmailAddress to = new EmailAddress(toEmail);

                // Check if the incoming body needs to be converted to HTML
                string htmlBody = null;

                if (bodyIsHtml == true)
                {
                    htmlBody = body;
                }
                else
                {
                    htmlBody = ConvertPlainTextToHtml(body);
                }

                string htmlContent =
                    $"<div style='font-family: Arial, sans-serif; font-size: 14px;'>{htmlBody}</div>";

                if (includeSignature == true)
                {
                    string formattedSignature = WebUtility.HtmlEncode(senderName ?? string.Empty);
                    htmlContent += $@"
                        <p style='font-family: Arial, sans-serif; font-size: 14px; margin-top: 16px;'>
                        Best regards,<br/>
                        <strong>{formattedSignature}</strong>
                        </p>";
                }

                SendGridMessage msg = new SendGridMessage
                {
                    From = from,
                    Subject = subject,
                    HtmlContent = htmlContent,
                    ReplyTo = replyTo
                };

                Personalization personalization = new Personalization
                {
                    Tos = new List<EmailAddress> { to }
                };

                if (ccEmails != null && ccEmails.Any())
                {
                    personalization.Ccs = ccEmails
                        .Where(cc => string.IsNullOrWhiteSpace(cc) == false)
                        .Select(cc => new EmailAddress(cc))
                        .ToList();
                }

                msg.Personalizations = new List<Personalization> { personalization };

                Response response = await client.SendEmailAsync(msg);

                if ((int)response.StatusCode >= 400)
                {
                    string errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError($"SendGrid send failed with status {(int)response.StatusCode} ({response.StatusCode}). Body: {errorBody}");
                    throw new Exception($"SendGrid API Error: {(int)response.StatusCode} - {errorBody}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogException("SendGrid send threw an exception.", ex);
                return false;
            }
        }

        public static async Task<bool> SendEmailWithAttachmentAsync(string senderEmail,
                                                                    string senderName,
                                                                    string toEmail,
                                                                    string subject,
                                                                    string body,
                                                                    string attachmentFileName,
                                                                    string attachmentBase64Data,
                                                                    string attachmentMimeType = "attachment/pdf",
                                                                    List<string> ccEmails = null,
                                                                    bool includeSignature = true,
                                                                    bool bodyIsHtml = false)
        {
            InitializeLogger();
            string apiKey = Foundation.Configuration.GetStringConfigurationSetting("SendGridAPIKey", null);
            string configEmailFrom = Foundation.Configuration.GetStringConfigurationSetting("EmailFromAddress", "donotreply@notconfigured.com");

            string displayName = senderName;

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("SendGrid API key is missing.");
            }

            SendGridClient client = new SendGridClient(apiKey);
            EmailAddress from;
            EmailAddress replyTo;

            if (string.IsNullOrEmpty(senderEmail) == true)
            {
                from = new EmailAddress(configEmailFrom, displayName);
                replyTo = new EmailAddress(configEmailFrom, senderName);
            }
            else
            {
                from = new EmailAddress(senderEmail, displayName);
                replyTo = new EmailAddress(senderEmail, senderName);
            }

            EmailAddress to = new EmailAddress(toEmail);

            // Check if the incoming body needs to be converted to HTML
            string htmlBody = null;

            if (bodyIsHtml == true)
            {
                htmlBody = body;
            }
            else
            {
                htmlBody = ConvertPlainTextToHtml(body);
            }
            
            string htmlContent = $"<div style='font-family: Arial, sans-serif; font-size: 14px;'>{htmlBody}</div>";

            if (includeSignature)
            {
                string formattedSignature = WebUtility.HtmlEncode(senderName ?? string.Empty);
                htmlContent += $@"
                    <p style='font-family: Arial, sans-serif; font-size: 14px; margin-top: 16px;'>
                    Best regards,<br/>
                    <strong>{formattedSignature}</strong>
                    </p>";
            }

            SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
            msg.ReplyTo = replyTo;

            // Add CC emails if provided
            if (ccEmails != null)
            {
                foreach (string ccEmail in ccEmails)
                {
                    if (string.IsNullOrWhiteSpace(ccEmail) == false)
                    {
                        msg.AddCc(new EmailAddress(ccEmail.Trim()));
                    }
                }
            }

            // Add attachment if available
            if (string.IsNullOrEmpty(attachmentBase64Data) == false)
            {
                msg.AddAttachment(attachmentFileName,
                                  attachmentBase64Data,
                                  attachmentMimeType);
            }

            Response response = await client.SendEmailAsync(msg);

            if ((int)response.StatusCode >= 400)
            {
                string errorBody = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid API Error: {(int)response.StatusCode} - {errorBody}");
            }

            return true;
        }

        public static async Task<bool> SendEmailToMultipleRecipientsAsync(string senderEmail,
                                                                          string senderName,
                                                                          List<string> toEmails,
                                                                          string subject,
                                                                          string body,
                                                                          bool includeSignature = true,
                                                                          bool bodyIsHtml = false)
        {
            InitializeLogger();

            try
            {
                //
                // Get the email configuration parameters from the configuration
                //
                string apiKey = Foundation.Configuration.GetStringConfigurationSetting("SendGridAPIKey", null);
                string configEmailFrom = Foundation.Configuration.GetStringConfigurationSetting("EmailFromAddress", "donotreply@notconfigured.com");

                string displayName = senderName;

                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new Exception("SendGrid API key is missing.");
                }

                SendGridClient client = new SendGridClient(apiKey);
                EmailAddress from;
                EmailAddress replyTo;

                if (string.IsNullOrEmpty(senderEmail) == true)
                {
                    from = new EmailAddress(configEmailFrom, displayName);
                    replyTo = new EmailAddress(configEmailFrom, senderName);
                }
                else
                {
                    from = new EmailAddress(senderEmail, displayName);
                    replyTo = new EmailAddress(senderEmail, senderName);
                }

                List<EmailAddress> toEmailAddresses = new List<EmailAddress>();

                foreach (string emailAddress in toEmails)
                {
                    if (string.IsNullOrWhiteSpace(emailAddress) == false)
                    {
                        toEmailAddresses.Add(new EmailAddress(emailAddress));
                    }
                }

                // Check if the incoming body needs to be converted to HTML
                string htmlBody = null;

                if (bodyIsHtml == true)
                {
                    htmlBody = body;
                }
                else
                {
                    htmlBody = ConvertPlainTextToHtml(body);
                }

                string htmlContent =
                    $"<div style='font-family: Arial, sans-serif; font-size: 14px;'>{htmlBody}</div>";

                if (includeSignature)
                {
                    string formattedSignature = WebUtility.HtmlEncode(senderName ?? string.Empty);
                    htmlContent += $@"
                        <p style='font-family: Arial, sans-serif; font-size: 14px; margin-top: 16px;'>
                        Best regards,<br/>
                        <strong>{formattedSignature}</strong>
                        </p>";
                }

                SendGridMessage msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, toEmailAddresses, subject, null, htmlContent);
                msg.ReplyTo = replyTo;

                Response response = await client.SendEmailAsync(msg);

                if ((int)response.StatusCode >= 400)
                {
                    string errorBody = await response.Body.ReadAsStringAsync();
                    throw new Exception($"SendGrid API Error: {(int)response.StatusCode} - {errorBody}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogException("SendGrid multi-recipient send threw an exception.", ex);
                return false;
            }
        }

        private static void InitializeLogger()
        {
            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? string.Empty;
            _logger.SetDirectory(Path.Combine(currentPath, LOG_DIRECTORY));
            _logger.SetFileName(LOG_FILENAME);
            _logger.Level = Logger.LogLevels.Information;
        }

        private static string ConvertPlainTextToHtml(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            return WebUtility.HtmlEncode(content)
                             .Replace("\r\n", "\n")
                             .Replace("\n", "<br/>");
        }
    }
}