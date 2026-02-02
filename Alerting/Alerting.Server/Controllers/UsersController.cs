//
// Users Controller
//
// REST API for getting users and teams for the Alerting module.
// Sources data from the Security database with module-specific constraints.
//
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Alerting.Server.Services;
using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Controllers
{
    /// <summary>
    /// API for getting users and teams that are applicable for the Alerting module.
    /// Users must have a role associated with the Alerting module.
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : SecureWebAPIController
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger) 
            : base("Alerting", "Users")
        {
            _userService = userService;
            _logger = logger;
        }

        #region Response DTOs

        /// <summary>
        /// User DTO for API responses - matches frontend expectations
        /// </summary>
        public class UserDto
        {
            public Guid ObjectGuid { get; set; }
            public string AccountName { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
            public string DisplayName { get; set; }
            public string EmailAddress { get; set; }
            public string CellPhoneNumber { get; set; }
            public string PhoneNumber { get; set; }
            public Guid? TeamGuid { get; set; }
        }

        /// <summary>
        /// Team DTO for API responses
        /// </summary>
        public class TeamDto
        {
            public Guid ObjectGuid { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        #endregion

        #region Mapping Methods

        private static UserDto MapToDto(User user)
        {
            var displayName = string.IsNullOrWhiteSpace(user.firstName) && string.IsNullOrWhiteSpace(user.lastName)
                ? user.accountName
                : $"{user.firstName} {user.lastName}".Trim();

            return new UserDto
            {
                ObjectGuid = user.objectGuid,
                AccountName = user.accountName,
                FirstName = user.firstName,
                MiddleName = user.middleName,
                LastName = user.lastName,
                DisplayName = displayName,
                EmailAddress = user.emailAddress,
                CellPhoneNumber = user.cellPhoneNumber,
                PhoneNumber = user.phoneNumber,
                TeamGuid = user.securityTeamGuid
            };
        }

        private static TeamDto MapToDto(Team team)
        {
            return new TeamDto
            {
                ObjectGuid = team.objectGuid,
                Name = team.name,
                Description = team.description
            };
        }

        #endregion

        #region User Endpoints

        /// <summary>
        /// Get all users applicable for the Alerting module in the current tenant.
        /// </summary>
        /// <returns>List of users with Alerting module access.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            if (!await UserCanReadAsync())
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            if (securityUser == null)
            {
                return Unauthorized();
            }

            Guid tenantGuid;
            try
            {
                tenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception)
            {
                return Problem("Your user account is not configured with a tenant.");
            }

            try
            {
                var users = await _userService.GetUsersAsync(tenantGuid, cancellationToken);
                var dtos = users.ConvertAll(MapToDto);

                _logger.LogDebug("Retrieved {Count} users for tenant {TenantGuid}", dtos.Count, tenantGuid);
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users for tenant {TenantGuid}", tenantGuid);
                return StatusCode(500, "An error occurred while retrieving users");
            }
        }

        /// <summary>
        /// Get a specific user by their object GUID.
        /// </summary>
        /// <param name="userGuid">The user's object GUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The user details or 404 if not found.</returns>
        [HttpGet("{userGuid:guid}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUser(Guid userGuid, CancellationToken cancellationToken)
        {
            if (!await UserCanReadAsync())
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            if (securityUser == null)
            {
                return Unauthorized();
            }

            Guid tenantGuid;
            try
            {
                tenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception)
            {
                return Problem("Your user account is not configured with a tenant.");
            }

            try
            {
                var user = await _userService.GetUserAsync(tenantGuid, userGuid, cancellationToken);
                if (user == null)
                {
                    return NotFound();
                }

                return Ok(MapToDto(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserGuid} for tenant {TenantGuid}", userGuid, tenantGuid);
                return StatusCode(500, "An error occurred while retrieving the user");
            }
        }

        #endregion

        #region Team Endpoints

        /// <summary>
        /// Get all teams in the current tenant.
        /// </summary>
        /// <returns>List of teams.</returns>
        [HttpGet("~/api/teams")]
        [ProducesResponseType(typeof(List<TeamDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetTeams(CancellationToken cancellationToken)
        {
            if (!await UserCanReadAsync())
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            if (securityUser == null)
            {
                return Unauthorized();
            }

            Guid tenantGuid;
            try
            {
                tenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception)
            {
                return Problem("Your user account is not configured with a tenant.");
            }

            try
            {
                var teams = await _userService.GetTeamsAsync(tenantGuid, cancellationToken);
                var dtos = teams.ConvertAll(MapToDto);

                _logger.LogDebug("Retrieved {Count} teams for tenant {TenantGuid}", dtos.Count, tenantGuid);
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teams for tenant {TenantGuid}", tenantGuid);
                return StatusCode(500, "An error occurred while retrieving teams");
            }
        }

        /// <summary>
        /// Get a specific team by its object GUID.
        /// </summary>
        /// <param name="teamGuid">The team's object GUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The team details or 404 if not found.</returns>
        [HttpGet("~/api/teams/{teamGuid:guid}")]
        [ProducesResponseType(typeof(TeamDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTeam(Guid teamGuid, CancellationToken cancellationToken)
        {
            if (!await UserCanReadAsync())
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            if (securityUser == null)
            {
                return Unauthorized();
            }

            Guid tenantGuid;
            try
            {
                tenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception)
            {
                return Problem("Your user account is not configured with a tenant.");
            }

            try
            {
                var team = await _userService.GetTeamAsync(tenantGuid, teamGuid, cancellationToken);
                if (team == null)
                {
                    return NotFound();
                }

                return Ok(MapToDto(team));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving team {TeamGuid} for tenant {TenantGuid}", teamGuid, tenantGuid);
                return StatusCode(500, "An error occurred while retrieving the team");
            }
        }

        /// <summary>
        /// Get all users that belong to a specific team.
        /// </summary>
        /// <param name="teamGuid">The team's object GUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of users in the team.</returns>
        [HttpGet("~/api/teams/{teamGuid:guid}/users")]
        [ProducesResponseType(typeof(List<UserDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetTeamUsers(Guid teamGuid, CancellationToken cancellationToken)
        {
            if (!await UserCanReadAsync())
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            if (securityUser == null)
            {
                return Unauthorized();
            }

            Guid tenantGuid;
            try
            {
                tenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception)
            {
                return Problem("Your user account is not configured with a tenant.");
            }

            try
            {
                var users = await _userService.GetTeamUsersAsync(tenantGuid, teamGuid, cancellationToken);
                var dtos = users.ConvertAll(MapToDto);

                _logger.LogDebug("Retrieved {Count} users for team {TeamGuid}", dtos.Count, teamGuid);
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users for team {TeamGuid}", teamGuid);
                return StatusCode(500, "An error occurred while retrieving team users");
            }
        }

        #endregion
    }
}
