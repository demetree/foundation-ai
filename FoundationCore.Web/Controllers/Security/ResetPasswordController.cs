using Foundation.Auditor;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace Foundation.Security.Controllers.WebAPI
{
    public partial class ResetPasswordController : SecureWebAPIController
    {
        public const string GENERIC_CONFIRMATION_MESSAGE = "If the account exists, a password reset email will be sent.";

        private SecurityContext _securityDb;

        public class SetPasswordRequest
        {
            public string Token { get; set; }
            public string Password { get; set; }
        }

        public class TokenCheckRequest
        {
            public string Token { get; set; }
        }

        public class SendResetEmailRequest
        {
            public string AccountName { get; set; }
        }

        public ResetPasswordController(SecurityContext securityDb) : base("Security", "SecurityUser")
        {
            _securityDb = securityDb;
        }


        [Route("api/User/SendPasswordResetEmail")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SendPasswordResetEmail([FromBody] SendResetEmailRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.AccountName))
            {
                return BadRequest();
            }

            StartAuditEventClock();

            //
            // Find user by account name - must be active and not deleted
            //
            var securityUser = await (from su in _securityDb.SecurityUsers
                                      where su.accountName == request.AccountName &&
                                      su.active == true &&
                                      su.deleted == false
                                      select su)
                                      .FirstOrDefaultAsync(cancellationToken);

            if (securityUser == null)
            {
                await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Attempt to initiate reset password for invalid user.  User identifier provided is {request.AccountName}", false);

                await Task.Delay(1000);
                return Ok(new
                {
                    message = GENERIC_CONFIRMATION_MESSAGE
                });
            }

            byte[] randomBytes = new byte[128];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            //
            // Createa URL-safe Base64 string
            //
            string token = Convert.ToBase64String(randomBytes)
                              .Replace('+', '-') // URL safe
                              .Replace('/', '_') // URL safe
                              .TrimEnd('=');

            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            string resetUrl = $"{baseUrl}/reset-password/{token}";


         
            try
            {
                // Send email with reset URL
                await SendPasswordResetEmailViaSendGridAsync(securityUser.accountName, securityUser.firstName, resetUrl);

                int passwordResetTokenHours = Foundation.Configuration.GetIntegerConfigurationSetting("PasswordResetTokenHours", 1);

                // Insert into SecurityUserPasswordResetToken table
                var resetTokenEntry = new SecurityUserPasswordResetToken
                {
                    securityUserId = securityUser.id,
                    token = token,
                    timeStamp = DateTime.UtcNow,
                    expiry = DateTime.UtcNow.AddHours(passwordResetTokenHours),     // Set expiry duration to the time configured in the settings
                    systemInitiated = false,                                        // this is user initiated
                    completed = false,
                    comments = $"Reset password request created via '/api/User/SendPasswordResetEmail' at {DateTime.UtcNow.ToString("s")}.",
                    active = true,
                    deleted = false
                };

                _securityDb.SecurityUserPasswordResetTokens.Add(resetTokenEntry);

                //
                // Insert SecurityUserEvent log
                //
                SecurityUserEvent emailSentEvent = new SecurityUserEvent
                {
                    securityUserId = securityUser.id,
                    securityUserEventTypeId = (int)SecurityLogic.SecurityUserEventTypes.UserInitiatedPasswordResetRequest,
                    timeStamp = DateTime.UtcNow,
                    comments = $"Reset link sent with token {token} at {DateTime.UtcNow}",
                    active = true,
                    deleted = false
                };

                _securityDb.SecurityUserEvents.Add(emailSentEvent);
                await _securityDb.SaveChangesAsync(cancellationToken);

                //
                // Add a regular audit event
                //
                await CreateAuditEventAsync(AuditEngine.AuditType.ConfirmationRequested, $"Password reset email sent for user {securityUser.accountName} with guid of {securityUser.objectGuid}", securityUser.id.ToString());

                return Ok(new
                {
                    message = GENERIC_CONFIRMATION_MESSAGE
                });
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.Error, $"Caught error trying to start password reset process for user.", false, "", "", "", ex);

                return StatusCode(500);
            }
        }


        private async Task<bool> SendPasswordResetEmailViaSendGridAsync(string toEmail, string greetingName, string resetUrl)
        {
            try
            {
                string apiKey = Foundation.Configuration.GetStringConfigurationSetting("SendGridAPIKey", null);
                string emailFromAccount = Foundation.Configuration.GetStringConfigurationSetting("EmailFromAddress", "donotreply@notconfigured.com");
                string emailDisplayName = Foundation.Configuration.GetStringConfigurationSetting("EmailDisplayName", "donotreply@notconfigured.com");
                string emailSignature = Foundation.Configuration.GetStringConfigurationSetting("EmailSignature", "donotreply@notconfigured.com");
                string companyName = Foundation.Configuration.GetStringConfigurationSetting("CompanyName", "Not Configured");
                int passwordResetTokenHours = Foundation.Configuration.GetIntegerConfigurationSetting("PasswordResetTokenHours", 1);

                if (string.IsNullOrEmpty(apiKey) == true)
                {
                    throw new Exception("Unable to send email because API key for SendGrid is not found in the appSettings");
                }


                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(emailFromAccount, emailDisplayName);
                var to = new EmailAddress(toEmail, greetingName);
                var subject = $"{companyName} Password Reset Request";

                var htmlContent = $@"
<html>
  <body style=""font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;"">
    <table width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"">
      <tr>
        <td style=""background-color: #004aad; color: white; padding: 20px; text-align: center;"">
          <h2 style=""margin: 0;"">{companyName} Password Reset Request</h2>
        </td>
      </tr>
      <tr>
        <td style=""padding: 30px;"">
          <p style=""font-size: 16px; color: #333;"">Dear {greetingName},</p>
          <p style=""font-size: 16px; color: #333;"">
            A password reset has been requested for your account.
          </p>
          <p style=""font-size: 16px; color: #333;"">
            To reset your password, please click the button below:
          </p>
          <p style=""text-align: center; margin: 30px 0;"">
            <a href=""{resetUrl}"" style=""background-color: #004aad; color: white; text-decoration: none; padding: 12px 24px; font-size: 16px; border-radius: 5px; display: inline-block;"">
              Reset Your Password
            </a>
          </p>
          <p style=""font-size: 14px; color: #666;"">
            If the button above doesn't work, copy and paste the following link into your browser:
          </p>
          <p style=""font-size: 14px; color: #666; word-break: break-word;"">
            <a href=""{resetUrl}"" style=""color: #004aad;"">{resetUrl}</a>
          </p>
          <p style=""font-size: 14px; color: #999; margin-top: 40px;"">
            This link will expire in {passwordResetTokenHours} hour{(passwordResetTokenHours > 1 ? "s" : "")}.
          </p>
          <p style=""font-size: 14px; color: #333;"">
            If you didn't request this reset, you can safely ignore this email.
          </p>
          <p style=""font-size: 14px; color: #333;"">
            Best regards,<br />
            <strong>{emailSignature}</strong>
          </p>
        </td>
      </tr>
      <tr>
        <td style=""background-color: #f4f4f4; text-align: center; padding: 15px; font-size: 12px; color: #999;"">
          © 2025 {companyName}. All rights reserved.
        </td>
      </tr>
    </table>
  </body>
</html>";

                var replyTo = new EmailAddress("support@k2research.ca", "K2 Research Support");

                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
                msg.ReplyTo = replyTo;

                var response = await client.SendEmailAsync(msg);

                if ((int)response.StatusCode >= 400)
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    throw new Exception($"SendGrid API Error: {(int)response.StatusCode} - {errorBody}");
                }

                return true;
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.Error, $"Caught error trying to send password reset email through SendGrid to email address {toEmail}.", false, "", "", "", ex);
                return false;
            }
        }


        [Route("api/User/SetPassword")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SetSecurityUserPassword([FromBody] SetPasswordRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || 
                string.IsNullOrWhiteSpace(request.Token) || 
                request.Token.Length > 300 || 
                string.IsNullOrWhiteSpace(request.Password))
            {
                await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Attempt to set password for user received with invalid input data", false);

                return BadRequest();
            }

            StartAuditEventClock();

            try
            {
                string token = request.Token.Trim();

                // Restore original Base64 format by replacing characters and adding padding
                string base64 = token.Replace('-', '+').Replace('_', '/');
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }

                try
                {
                    byte[] tokenBytes = Convert.FromBase64String(base64);

                    // Use tokenBytes to check against a stored token in DB, for example
                }
                catch (FormatException)
                {
                    await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Attempt to reset password with token received with a badly formatted token.  Toeken received is {token}", false);

                    return BadRequest();
                }

                var tokenEntry = await (from suprt in _securityDb.SecurityUserPasswordResetTokens
                                        where suprt.token == token
                                        select suprt)
                                        .FirstOrDefaultAsync(cancellationToken);

                if (tokenEntry == null)
                {
                    await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Attempt to reset password with reset token received for a token that does not exist.  Token received is {token}", false);

                    return BadRequest();
                }

                if (tokenEntry.completed == true)
                {
                    await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Attempt to reset password with reset token received for a token that has already been used.  Token received is {token}", false);

                    return BadRequest();
                }

                if (tokenEntry.expiry < DateTime.UtcNow)
                {
                    await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Attempt to reset password with reset token received for a token that has expired.  Token received is {token}", false);

                    return BadRequest();
                }

                string password = request.Password?.Trim();

                if (!PasswordCheck.IsValidPassword(password, 8, 4, true, true, true, true))
                {
                    await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.Error, $"Attempt to reset password with reset token received with a password that is not sufficiently complex.  Token received is {token}", false);

                    return BadRequest("Password must be at least 8 characters long and contain uppercase, lowercase, digit, and special character.");
                }


                // Get the corresponding user
                var securityUser = await (from su in _securityDb.SecurityUsers
                                          where su.id == tokenEntry.securityUserId &&
                                          su.active == true &&
                                          su.deleted == false
                                          select su)
                                          .FirstOrDefaultAsync(cancellationToken);


                if (securityUser == null)
                {
                    await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Attempt to reset password for invalid user.  Token provided is {request.Token}", false);

                    return BadRequest();
                }

                if (securityUser.password == SecurityLogic.SecurePasswordHasher.Hash(request.Password))
                {
                    await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.Error, $"Attempt to reset password with reset token received with a password that is currently in use.  Token received is {token}", false);

                    return BadRequest("Can not use previously used password");
                }

                // Update the password
                securityUser.password = SecurityLogic.SecurePasswordHasher.Hash(request.Password);

                //set the canLogin to true
                securityUser.canLogin = true;

                // reset the failed login count to 0 to ensure that the user can login immediately after the reset.
                securityUser.failedLoginCount = 0;

                // Mark token as completed
                tokenEntry.completed = true;

               
                //
                // Log the Security User event
                //
                var passwordResetEvent = new SecurityUserEvent
                {
                    securityUserId = securityUser.id,
                    securityUserEventTypeId = (int)SecurityLogic.SecurityUserEventTypes.UserInitiatedPasswordResetCompleted,
                    timeStamp = DateTime.UtcNow,
                    comments = "User successfully reset password via token.",
                    active = true,
                    deleted = false
                };

                _securityDb.SecurityUserEvents.Add(passwordResetEvent);
                
                await _securityDb.SaveChangesAsync(cancellationToken);

                //
                // Also log as a system update event
                //
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.ConfirmationGranted,
                    $"SecurityUser password reset for user {securityUser.accountName} via token {token}",
                    securityUser.id.ToString()
                );

                return Ok(new
                {
                    message = "Password reset successfully.",
                    securityUserId = securityUser.id
                });
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.Error, $"Caught error trying to set user password.", false, "", "", "", ex);

                return StatusCode(500);
            }
        }
        

        [Route("api/User/IsTokenInvalid")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> IsTokenInvalid([FromBody] TokenCheckRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Token))
            {
                await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Attempt to check status of reset token received without valid request information", false);

                return BadRequest();
            }

            try
            {
                string token = request.Token.Trim();


                if (token.Length > 300)     // semi arbitrary size check just to filter out ridiculous input.  Max real values seen in db is 172 characters.
                {
                    token = token.Substring(0, 300);
                }


                // Restore original Base64 format by replacing characters and adding padding
                string base64 = token.Replace('-', '+').Replace('_', '/');
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }

                try
                {
                    byte[] tokenBytes = Convert.FromBase64String(base64);
                    // tokenBytes can be used if needed
                }
                catch (FormatException)
                {
                    await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Attempt to check status of reset token received with a badly formatted token.  Toeken received is {token}", false);

                    return BadRequest();
                }

                var tokenEntry = await (from suprt in _securityDb.SecurityUserPasswordResetTokens
                                        where suprt.token == token
                                        select suprt)
                                        .FirstOrDefaultAsync(cancellationToken);
                    
                if (tokenEntry == null)
                {
                    await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Attempt to check status of reset token received for a token that does not exist.  Token received is {token}", false);

                    return BadRequest();
                }

                bool isInvalidToken = tokenEntry.completed == true|| 
                                      tokenEntry.expiry < DateTime.UtcNow ||
                                      tokenEntry.active == false ||
                                      tokenEntry.deleted == true;

                await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.Search, $"Attempt to check status of reset token received, and returned valid state of {isInvalidToken} for token {token}", true);

                return Ok(new { isInvalidToken });
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.Error, $"Caught error trying to validate token.", false, "", "", "", ex);

                return StatusCode(500, "An error occurred while checking the token.");
            }
        }
    }
}

