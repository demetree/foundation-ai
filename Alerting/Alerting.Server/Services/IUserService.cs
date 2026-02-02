//
// Escalation Service Implementation
//
// Processes escalation rules and resolves on-call targets.
//
using Foundation.Security.Database;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Alerting.Server.Services
{
    public class User
    {
        public string accountName { get; set; }

        public string firstName { get; set; }

        public string middleName { get; set; }

        public string lastName { get; set; }

        public DateTime? dateOfBirth { get; set; }

        public string emailAddress { get; set; }

        public string cellPhoneNumber { get; set; }

        public string phoneNumber { get; set; }

        public string phoneExtension { get; set; }

        public string description { get; set; }

        public byte[] image { get; set; }

        public Guid? securityOrganizationGuid { get; set; }

        public Guid? securityDepartmentGuid { get; set; }

        public Guid? securityTeamGuid { get; set; }

        public Guid objectGuid { get; set; }
    }


    public class Team
    {
        public string name { get; set; }

        public string description { get; set; }

        public Guid objectGuid { get; set; }

        //public List<User> Users { get; set; } = new List<User>();
    }


    public interface IUserService
    {
        Task<List<User>> GetUsersAsync(Guid tenantGuid, CancellationToken cancellationToken = default);

        Task<User> GetUserAsync(Guid tenantGuid, Guid userGuid, CancellationToken cancellationToken = default);

        Task<List<Team>> GetTeamsAsync(Guid tenantGuid, CancellationToken cancellationToken = default);

        Task<Team> GetTeamAsync(Guid tenantGuid, Guid teamGuid, CancellationToken cancellationToken = default);

        Task<List<User>> GetTeamUsersAsync(Guid tenantGuid, Guid teamGuid, CancellationToken cancellationToken = default);
    }
}