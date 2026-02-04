//
// SecurityUsersController.AdminActions.cs
//
// Partial class file containing admin-only user management actions:
// - Password reset (send email, set password)
// - Account locking/unlocking
// - User creation with password
//
// Merged from AdminUserActionsController.cs to eliminate code duplication
// and consolidate all user management in one controller.
//
// Each action logs a SecurityUserEvent for audit trail.
//
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Foundation.Auditor;
using Foundation.Security.Database;

using SendGrid;
using SendGrid.Helpers.Mail;


namespace Foundation.Security.Controllers.WebAPI
{
    public partial class SecurityUsersController
    {
        //
        // Request Models for Admin Actions
        //
        public class AdminSetPasswordRequest
        {
            public string Password { get; set; }
        }

        public class AdminCreateUserRequest
        {
            [Required] public string AccountName { get; set; }
            [Required] public string Password { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
            public string EmailAddress { get; set; }
            public string CellPhoneNumber { get; set; }
            public string PhoneNumber { get; set; }
            public string PhoneExtension { get; set; }
            public string Description { get; set; }
            public int? SecurityUserTitleId { get; set; }
            public int? ReportsToSecurityUserId { get; set; }
            public int? SecurityTenantId { get; set; }
            public int? SecurityOrganizationId { get; set; }
            public int? SecurityDepartmentId { get; set; }
            public int? SecurityTeamId { get; set; }
            public int ReadPermissionLevel { get; set; } = 0;
            public int WritePermissionLevel { get; set; } = 0;
            public bool MustChangePassword { get; set; } = false;
            public bool TwoFactorSendByEmail { get; set; } = false;
            public bool TwoFactorSendBySMS { get; set; } = false;
        }


        #region Admin User Actions

        /// <summary>
        /// 
        /// Sends a password reset email to the specified user (admin-initiated).
        /// 
        /// </summary>
        [Route("api/Admin/User/{id}/SendPasswordReset")]
        [HttpPost]
        public async Task<IActionResult> AdminSendPasswordReset(long id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            //
            // Verify admin privileges
            //
            if (await UserCanAdministerAsync(cancellationToken) == false)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Non-admin user attempted to send password reset for user id {id}", false);

                return Forbid();
            }

            try
            {
                //
                // Get the target user
                //
                SecurityUser targetUser = await (from su in _context.SecurityUsers
                                                  where su.id == id &&
                                                  su.deleted == false
                                                  select su)
                                                  .FirstOrDefaultAsync(cancellationToken);

                if (targetUser == null)
                {
                    return NotFound("User not found.");
                }

                //
                // Generate reset token
                //
                byte[] randomBytes = new byte[128];

                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomBytes);
                }

                string token = Convert.ToBase64String(randomBytes)
                                  .Replace('+', '-')
                                  .Replace('/', '_')
                                  .TrimEnd('=');

                string baseUrl = $"{Request.Scheme}://{Request.Host}";
                string resetUrl = $"{baseUrl}/reset-password/{token}";

                //
                // Send email
                //
                await SendPasswordResetEmailViaSendGridAsync(targetUser.emailAddress, targetUser.firstName, resetUrl);

                int passwordResetTokenHours = Foundation.Configuration.GetIntegerConfigurationSetting("PasswordResetTokenHours", 1);

                //
                // Create reset token record
                //
                SecurityUserPasswordResetToken resetTokenEntry = new SecurityUserPasswordResetToken
                {
                    securityUserId = targetUser.id,
                    token = token,
                    timeStamp = DateTime.UtcNow,
                    expiry = DateTime.UtcNow.AddHours(passwordResetTokenHours),
                    systemInitiated = true,
                    completed = false,
                    comments = $"Admin-initiated password reset via '/api/Admin/User/{id}/SendPasswordReset' at {DateTime.UtcNow.ToString("s")}.",
                    active = true,
                    deleted = false
                };

                _context.SecurityUserPasswordResetTokens.Add(resetTokenEntry);

                //
                // Log the event
                //
                SecurityUserEvent userEvent = new SecurityUserEvent
                {
                    securityUserId = targetUser.id,
                    securityUserEventTypeId = (int)SecurityLogic.SecurityUserEventTypes.SystemInitiatedPasswordResetRequest,
                    timeStamp = DateTime.UtcNow,
                    comments = $"Admin-initiated password reset email sent by user {GetSecurityUser()?.accountName ?? "Unknown"}.",
                    active = true,
                    deleted = false
                };

                _context.SecurityUserEvents.Add(userEvent);
                await _context.SaveChangesAsync(cancellationToken);

                await CreateAuditEventAsync(AuditEngine.AuditType.ConfirmationRequested, $"Admin-initiated password reset email sent for user {targetUser.accountName}", targetUser.id.ToString());

                return Ok(new { message = "Password reset email sent successfully." });
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Error sending admin-initiated password reset.", false, "", "", "", ex);

                return StatusCode(500, "An error occurred while sending the password reset email.");
            }
        }


        /// <summary>
        /// 
        /// Sets a temporary password for the specified user (admin-initiated).
        /// 
        /// </summary>
        [Route("api/Admin/User/{id}/SetPassword")]
        [HttpPost]
        public async Task<IActionResult> AdminSetPassword(long id, [FromBody] AdminSetPasswordRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            //
            // Verify admin privileges
            //
            if (await UserCanAdministerAsync(cancellationToken) == false)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Non-admin user attempted to set password for user id {id}", false);

                return Forbid();
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Password is required.");
            }

            try
            {
                //
                // Get the target user
                //
                SecurityUser targetUser = await (from su in _context.SecurityUsers
                                                  where su.id == id &&
                                                  su.deleted == false
                                                  select su)
                                                  .FirstOrDefaultAsync(cancellationToken);

                if (targetUser == null)
                {
                    return NotFound("User not found.");
                }

                //
                // Validate password complexity
                //
                if (PasswordCheck.IsValidPassword(request.Password, 8, 4, true, true, true, true) == false)
                {
                    return BadRequest("Password must be at least 8 characters long and contain uppercase, lowercase, digit, and special character.");
                }

                //
                // Set the new password
                //
                targetUser.password = SecurityLogic.SecurePasswordHasher.Hash(request.Password);
                targetUser.mustChangePassword = true;
                targetUser.failedLoginCount = 0;

                //
                // Log the event
                //
                SecurityUserEvent userEvent = new SecurityUserEvent
                {
                    securityUserId = targetUser.id,
                    securityUserEventTypeId = (int)SecurityLogic.SecurityUserEventTypes.AdminInitiatedPasswordSet,
                    timeStamp = DateTime.UtcNow,
                    comments = $"Password set by admin user {GetSecurityUser()?.accountName ?? "Unknown"}. User must change password on next login.",
                    active = true,
                    deleted = false
                };

                _context.SecurityUserEvents.Add(userEvent);
                await _context.SaveChangesAsync(cancellationToken);

                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, $"Admin set password for user {targetUser.accountName}", targetUser.id.ToString());

                return Ok(new { message = "Password set successfully. User will be required to change password on next login." });
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Error setting password for user.", false, "", "", "", ex);

                return StatusCode(500, "An error occurred while setting the password.");
            }
        }


        /// <summary>
        /// 
        /// Creates a new user with initial password (admin-initiated).
        /// Password is hashed and stored securely. User is created with canLogin = true.
        /// 
        /// </summary>
        [Route("api/Admin/CreateUser")]
        [HttpPost]
        public async Task<IActionResult> AdminCreateUser([FromBody] AdminCreateUserRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            //
            // Verify admin privileges
            //
            if (await UserCanAdministerAsync(cancellationToken) == false)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Non-admin user attempted to create a new user", false);

                return Forbid();
            }

            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            if (string.IsNullOrWhiteSpace(request.AccountName))
            {
                return BadRequest("Account name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Password is required.");
            }

            try
            {
                //
                // Check if account name already exists
                //
                bool accountExists = await (from su in _context.SecurityUsers
                                            where su.accountName == request.AccountName &&
                                            su.deleted == false
                                            select su).AnyAsync(cancellationToken);

                if (accountExists)
                {
                    return BadRequest("An account with this name already exists.");
                }

                //
                // Validate password complexity
                //
                if (PasswordCheck.IsValidPassword(request.Password, 8, 4, true, true, true, true) == false)
                {
                    return BadRequest("Password must be at least 8 characters long and contain uppercase, lowercase, digit, and special character.");
                }

                //
                // Create the new user
                //
                SecurityUser newUser = new SecurityUser
                {
                    accountName = request.AccountName,
                    activeDirectoryAccount = false,
                    password = SecurityLogic.SecurePasswordHasher.Hash(request.Password),
                    canLogin = true,
                    mustChangePassword = request.MustChangePassword,
                    firstName = request.FirstName,
                    middleName = request.MiddleName,
                    lastName = request.LastName,
                    emailAddress = request.EmailAddress,
                    cellPhoneNumber = request.CellPhoneNumber,
                    phoneNumber = request.PhoneNumber,
                    phoneExtension = request.PhoneExtension,
                    description = request.Description,
                    securityUserTitleId = request.SecurityUserTitleId,
                    reportsToSecurityUserId = request.ReportsToSecurityUserId,
                    securityTenantId = request.SecurityTenantId,
                    securityOrganizationId = request.SecurityOrganizationId,
                    securityDepartmentId = request.SecurityDepartmentId,
                    securityTeamId = request.SecurityTeamId,
                    readPermissionLevel = request.ReadPermissionLevel,
                    writePermissionLevel = request.WritePermissionLevel,
                    twoFactorSendByEmail = request.TwoFactorSendByEmail,
                    twoFactorSendBySMS = request.TwoFactorSendBySMS,
                    failedLoginCount = 0,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.SecurityUsers.Add(newUser);
                await _context.SaveChangesAsync(cancellationToken);

                //
                // Log the user creation event
                //
                SecurityUserEvent userEvent = new SecurityUserEvent
                {
                    securityUserId = newUser.id,
                    securityUserEventTypeId = (int)SecurityLogic.SecurityUserEventTypes.AdminInitiatedPasswordSet,
                    timeStamp = DateTime.UtcNow,
                    comments = $"User created by admin {GetSecurityUser()?.accountName ?? "Unknown"} via Admin/CreateUser endpoint.",
                    active = true,
                    deleted = false
                };

                _context.SecurityUserEvents.Add(userEvent);
                await _context.SaveChangesAsync(cancellationToken);

                //
                // Validate data visibility hierarchy and create mapping table entries
                // Uses existing methods from the main SecurityUsersController
                //
                ValidateDataVisibilityDefaultsForUser(newUser);
                await CreateOrUpdateUserDataVisibilityTablesAsync(newUser, cancellationToken);

                //
                // Clear security caches
                //
                SecurityFramework.ClearSecurityCaches();
                SecurityLogic.ClearSecurityCaches();

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, $"Admin created new user {newUser.accountName}", newUser.id.ToString());

                //
                // Clear password before returning
                //
                newUser.password = null;

                return Ok(SecurityUser.CreateAnonymous(newUser));
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Error creating new user.", false, "", "", "", ex);

                return StatusCode(500, "An error occurred while creating the user: " + ex.Message);
            }
        }


        /// <summary>
        /// 
        /// Locks the specified user account (sets active = false).
        /// 
        /// </summary>
        [Route("api/Admin/User/{id}/Lock")]
        [HttpPost]
        public async Task<IActionResult> AdminLockAccount(long id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            //
            // Verify admin privileges
            //
            if (await UserCanAdministerAsync(cancellationToken) == false)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Non-admin user attempted to lock account for user id {id}", false);

                return Forbid();
            }

            try
            {
                //
                // Get the target user
                //
                SecurityUser targetUser = await (from su in _context.SecurityUsers
                                                  where su.id == id &&
                                                  su.deleted == false
                                                  select su)
                                                  .FirstOrDefaultAsync(cancellationToken);

                if (targetUser == null)
                {
                    return NotFound("User not found.");
                }

                //
                // Lock the account
                //
                targetUser.active = false;

                //
                // Log the event
                //
                SecurityUserEvent userEvent = new SecurityUserEvent
                {
                    securityUserId = targetUser.id,
                    securityUserEventTypeId = (int)SecurityLogic.SecurityUserEventTypes.AdminActionLockAccount,
                    timeStamp = DateTime.UtcNow,
                    comments = $"Account locked by admin user {GetSecurityUser()?.accountName ?? "Unknown"}.",
                    active = true,
                    deleted = false
                };

                _context.SecurityUserEvents.Add(userEvent);
                await _context.SaveChangesAsync(cancellationToken);

                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, $"Admin locked account for user {targetUser.accountName}", targetUser.id.ToString());

                return Ok(new { message = "Account locked successfully." });
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Error locking user account.", false, "", "", "", ex);

                return StatusCode(500, "An error occurred while locking the account.");
            }
        }


        /// <summary>
        /// 
        /// Unlocks the specified user account (sets active = true, resets failed login count).
        /// 
        /// </summary>
        [Route("api/Admin/User/{id}/Unlock")]
        [HttpPost]
        public async Task<IActionResult> AdminUnlockAccount(long id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            //
            // Verify admin privileges
            //
            if (await UserCanAdministerAsync(cancellationToken) == false)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Non-admin user attempted to unlock account for user id {id}", false);

                return Forbid();
            }

            try
            {
                //
                // Get the target user
                //
                SecurityUser targetUser = await (from su in _context.SecurityUsers
                                                  where su.id == id &&
                                                  su.deleted == false
                                                  select su)
                                                  .FirstOrDefaultAsync(cancellationToken);

                if (targetUser == null)
                {
                    return NotFound("User not found.");
                }

                //
                // Unlock the account
                //
                targetUser.active = true;
                targetUser.failedLoginCount = 0;

                //
                // Log the event
                //
                SecurityUserEvent userEvent = new SecurityUserEvent
                {
                    securityUserId = targetUser.id,
                    securityUserEventTypeId = (int)SecurityLogic.SecurityUserEventTypes.AccountUnlocked,
                    timeStamp = DateTime.UtcNow,
                    comments = $"Account unlocked by admin user {GetSecurityUser()?.accountName ?? "Unknown"}.",
                    active = true,
                    deleted = false
                };

                _context.SecurityUserEvents.Add(userEvent);
                await _context.SaveChangesAsync(cancellationToken);

                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, $"Admin unlocked account for user {targetUser.accountName}", targetUser.id.ToString());

                return Ok(new { message = "Account unlocked successfully." });
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Error unlocking user account.", false, "", "", "", ex);

                return StatusCode(500, "An error occurred while unlocking the account.");
            }
        }

        #endregion


        #region SendGrid Email Helpers

        //
        // Helper method to send password reset email via SendGrid
        //
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
                var subject = $"{companyName} Password Reset Request (Admin Initiated)";

                var htmlContent = $@"
<html>
  <body style=""font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;"">
    <table width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"">
      <tr>
        <td style=""background-color: #004aad; color: white; padding: 20px; text-align: center;"">
          <h2 style=""margin: 0;"">{companyName} Password Reset</h2>
        </td>
      </tr>
      <tr>
        <td style=""padding: 30px;"">
          <p style=""font-size: 16px; color: #333;"">Dear {greetingName},</p>
          <p style=""font-size: 16px; color: #333;"">
            An administrator has requested a password reset for your account.
          </p>
          <p style=""font-size: 16px; color: #333;"">
            To set a new password, please click the button below:
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
            Best regards,<br />
            <strong>{emailSignature}</strong>
          </p>
        </td>
      </tr>
      <tr>
        <td style=""background-color: #f4f4f4; text-align: center; padding: 15px; font-size: 12px; color: #999;"">
          © {DateTime.UtcNow.Year} {companyName}. All rights reserved.
        </td>
      </tr>
    </table>
  </body>
</html>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);

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
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Caught error trying to send admin-initiated password reset email through SendGrid to email address {toEmail}.", false, "", "", "", ex);

                return false;
            }
        }

        #endregion
    }
}
