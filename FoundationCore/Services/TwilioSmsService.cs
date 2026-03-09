using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;


namespace Foundation.Services
{
    /// <summary>
    /// Provides SMS sending capabilities via the Twilio API.
    /// Mirrors the SendGridEmailService pattern — static class using Foundation.Configuration.
    ///
    /// Required configuration keys:
    ///   TwilioAccountSid   — Twilio account SID
    ///   TwilioAuthToken    — Twilio auth token
    ///   TwilioFromNumber   — Twilio sender phone number (E.164 format)
    /// </summary>
    public static class TwilioSmsService
    {
        // Logging Setup
        private const string LOG_DIRECTORY = "Log";
        private const string LOG_FILENAME = "TwilioSmsService";
        private static readonly Logger _logger = new Logger();
        public static Logger Logger { get { return _logger; } }

        private static bool _initialized;


        /// <summary>
        /// Send an SMS message to a single phone number.
        /// </summary>
        /// <param name="toPhoneNumber">Recipient phone in E.164 format (e.g. +15551234567).</param>
        /// <param name="messageBody">The SMS body text.</param>
        /// <returns>Tuple: (success, messageSid, errorMessage).</returns>
        public static async Task<(bool Success, string MessageSid, string Error)> SendSmsAsync(
            string toPhoneNumber,
            string messageBody)
        {
            InitializeLogger();

            try
            {
                if (string.IsNullOrWhiteSpace(toPhoneNumber))
                {
                    _logger.LogWarning("TwilioSmsService: Cannot send SMS — no phone number provided.");
                    return (false, null, "No phone number provided");
                }

                string fromNumber = Foundation.Configuration.GetStringConfigurationSetting("TwilioFromNumber", null);
                if (string.IsNullOrEmpty(fromNumber))
                {
                    _logger.LogWarning("TwilioSmsService: TwilioFromNumber not configured.");
                    return (false, null, "TwilioFromNumber not configured");
                }

                EnsureInitialized();
                if (!_initialized)
                {
                    return (false, null, "Twilio not initialized — credentials missing");
                }

                _logger.LogInformation($"TwilioSmsService: Sending SMS to {MaskPhoneNumber(toPhoneNumber)} ({messageBody.Length} chars)");

                var message = await MessageResource.CreateAsync(
                    to: new PhoneNumber(toPhoneNumber),
                    from: new PhoneNumber(fromNumber),
                    body: messageBody
                );

                bool success = message.Status != MessageResource.StatusEnum.Failed &&
                               message.Status != MessageResource.StatusEnum.Undelivered;

                if (success)
                {
                    _logger.LogInformation($"TwilioSmsService: SMS sent successfully to {MaskPhoneNumber(toPhoneNumber)} (SID: {message.Sid})");
                    return (true, message.Sid, null);
                }
                else
                {
                    _logger.LogError($"TwilioSmsService: SMS failed to {MaskPhoneNumber(toPhoneNumber)}: {message.ErrorMessage}");
                    return (false, null, message.ErrorMessage ?? "SMS delivery failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogException($"TwilioSmsService: Exception sending SMS to {MaskPhoneNumber(toPhoneNumber)}", ex);
                return (false, null, ex.Message);
            }
        }


        /// <summary>
        /// Ensures the Twilio client is initialized with credentials from Foundation.Configuration.
        /// </summary>
        private static void EnsureInitialized()
        {
            if (_initialized) return;

            string accountSid = Foundation.Configuration.GetStringConfigurationSetting("TwilioAccountSid", null);
            string authToken = Foundation.Configuration.GetStringConfigurationSetting("TwilioAuthToken", null);

            if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
            {
                _logger.LogWarning("TwilioSmsService: Twilio credentials not configured — SMS disabled.");
                return;
            }

            TwilioClient.Init(accountSid, authToken);
            _initialized = true;
        }


        /// <summary>
        /// Mask phone number for logging — show last 4 digits only.
        /// </summary>
        public static string MaskPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 4)
                return "****";
            return new string('*', phone.Length - 4) + phone.Substring(phone.Length - 4);
        }


        private static void InitializeLogger()
        {
            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? string.Empty;
            _logger.SetDirectory(Path.Combine(currentPath, LOG_DIRECTORY));
            _logger.SetFileName(LOG_FILENAME);
            _logger.Level = Logger.LogLevels.Information;
        }
    }
}
