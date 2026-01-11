using Foundation.Security.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Foundation.Security.Controllers.WebAPI
{
    public partial class NewUserController : SecureWebAPIController
    {
        private SecurityContext _securityDb;
        private SecurityUser updatedUser;

        public NewUserController(SecurityContext securityDb) : base("Security", "SecurityUser")
        {
            _securityDb = securityDb;
        }

        public class SubmitUserRequest
        {
            public string Token { get; set; }
            public SecurityUser UpdatedUser { get; set; }

        }

        [Route("api/User/GetUserByToken")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserByToken([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Token is required.");

            try
            {
                // Restore Base64 from URL-safe version
                string base64 = token.Replace('-', '+').Replace('_', '/');
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }

                // Try decoding it
                try
                {
                    Convert.FromBase64String(base64);
                }
                catch (FormatException)
                {
                    return BadRequest("Invalid token format.");
                }

                // Look for the token entry
                var tokenEntry = await _securityDb.SecurityUserPasswordResetTokens
                    .FirstOrDefaultAsync(t => t.token == token && t.active && !t.deleted);

                if (tokenEntry == null)
                    return NotFound("Token not found or expired.");

                // Now get the user
                var user = await _securityDb.SecurityUsers
                    .Where(u => u.id == tokenEntry.securityUserId)
                    .Select(u => new
                    {
                        u.id,
                        u.accountName,
                        u.firstName,
                        u.middleName,
                        u.lastName,
                        u.dateOfBirth,
                        u.emailAddress,
                        u.cellPhoneNumber,
                        u.phoneNumber,
                        u.image
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                    return NotFound("Associated user not found.");

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }


        [Route("api/User/ActivateUser")]
        [HttpPut]
        [AllowAnonymous]
        public async Task<IActionResult> ActivateUser([FromBody] SubmitUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest("Token is required.");
            }

            if (request.UpdatedUser == null)
            {
                return BadRequest("User information is required.");
            }


            string token = request.Token;
            updatedUser = request.UpdatedUser;

            try
            {
                // Restore Base64 from URL-safe version
                string base64 = token.Replace('-', '+').Replace('_', '/');
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }

                try
                {
                    Convert.FromBase64String(base64);
                }
                catch (FormatException)
                {
                    return BadRequest("Invalid token format.");
                }

                //
                // Look for active and non expired system initiated tokens
                //
                var tokenEntry = await _securityDb.SecurityUserPasswordResetTokens
                    .FirstOrDefaultAsync(t =>
                        t.token == token &&
                        t.systemInitiated == true &&
                        t.active &&
                        !t.deleted &&
                        !t.completed &&
                        t.expiry > DateTime.UtcNow);

                if (tokenEntry == null)
                {
                    await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Attempt to activate user with invalid token was received.  Token provided is {token}", false);
                    return NotFound("Token not found, expired, or already completed.");
                }

                var user = await _securityDb.SecurityUsers.FirstOrDefaultAsync(u => u.id == tokenEntry.securityUserId);

                if (user == null)
                {
                    return NotFound("Associated user not found.");
                }

                // Update user information - Note that we do not allow the account name to be updated
                user.firstName = updatedUser.firstName;
                user.middleName = updatedUser.middleName;
                user.lastName = updatedUser.lastName;
                user.dateOfBirth = updatedUser.dateOfBirth;
                user.emailAddress = updatedUser.emailAddress;
                user.cellPhoneNumber = updatedUser.cellPhoneNumber;
                user.phoneNumber = updatedUser.phoneNumber;


                //
                // Image must be less than one megabyte
                //
                if (updatedUser.image != null && updatedUser.image.Length < 1024768)
                {
                    user.image = updatedUser.image;
                }
                else
                {
                    user.image = null;                
                }

                user.canLogin = true;
                user.password = SecurityLogic.SecurePasswordHasher.Hash(updatedUser.password);
                user.mustChangePassword = false;
                user.failedLoginCount = 0;

                //
                // Update token to mark as completed
                //
                tokenEntry.completed = true;
                tokenEntry.comments = $"User registration completed at {DateTime.UtcNow.ToString("s")}.";

                // Create event log
                var userEvent = new SecurityUserEvent
                {
                    securityUserId = user.id,
                    securityUserEventTypeId = (int)SecurityLogic.SecurityUserEventTypes.SystemInitiatedPasswordResetCompleted,
                    timeStamp = DateTime.UtcNow,
                    comments = "User info submitted and canLogin enabled via SubmitUser API.",
                    active = true,
                    deleted = false
                };


                _securityDb.SecurityUserEvents.Add(userEvent);

                await _securityDb.SaveChangesAsync();

                await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.ConfirmationGranted, $"Attempt to activate user with token was received and accepted.  User activated is {user.accountName} with guid of {user.objectGuid}.  Token provided is {token}", true);

                return Ok(new
                {
                    message = "User updated successfully.",
                    userId = user.id
                });
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.Error, $"Caught error trying to activate user.", false, "", "", "", ex);

                return StatusCode(500, "An error occurred while submitting the activation request for user." + ex.Message);
            }
        }
    }
}

