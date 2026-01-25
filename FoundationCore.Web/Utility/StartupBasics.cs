using Foundation.Auditor.Controllers.WebAPI;
using Foundation.Security.Controllers.WebAPI;
using System;
using System.Collections.Generic;

namespace Foundation.Web.Utility
{
    public class StartupBasics
    {

        public static void AddFoundationEssentialWebAPIControllers(List<Type> controllers)
        {
            controllers.Add(typeof(AuthorizationController));           // Need this to authenticate
            controllers.Add(typeof(UserSettingsController));            // Foundation security user settings
            controllers.Add(typeof(UserFiltersController));             // Foundation security user filters
            controllers.Add(typeof(ResetPasswordController));
            controllers.Add(typeof(NewUserController));

            //
            // Utility Controllers
            //
            controllers.Add(typeof(Foundation.Controllers.WebAPI.LogViewerController));  // Log file viewer
        }



        public static void AddSecurityWebAPIControllers(List<Type> controllers)
        {
            //
            // Custom for Foundation Admin user listing custom actions
            //
            controllers.Add(typeof(AdminUserActionsController));             // Admin-only user management actions
            controllers.Add(typeof(SessionsController));                 // Session state management
            controllers.Add(typeof(UserSessionsController));                 // Session state management

            //
            // These are the Data Controllers for the security module
            //
            controllers.Add(typeof(EntityDataTokensController));
            controllers.Add(typeof(EntityDataTokenEventsController));
            controllers.Add(typeof(EntityDataTokenEventTypesController));
            controllers.Add(typeof(LoginAttemptsController));
            controllers.Add(typeof(ModulesController));
            controllers.Add(typeof(ModuleSecurityRolesController));
            controllers.Add(typeof(OAUTHTokensController));
            controllers.Add(typeof(PrivilegesController));
            controllers.Add(typeof(SecurityDepartmentsController));
            controllers.Add(typeof(SecurityDepartmentUsersController));
            controllers.Add(typeof(SecurityGroupsController));
            controllers.Add(typeof(SecurityGroupSecurityRolesController));
            controllers.Add(typeof(SecurityOrganizationsController));
            controllers.Add(typeof(SecurityOrganizationUsersController));
            controllers.Add(typeof(SecurityRolesController));
            controllers.Add(typeof(SecurityTeamsController));
            controllers.Add(typeof(SecurityTeamUsersController));
            controllers.Add(typeof(SecurityTenantsController));
            controllers.Add(typeof(SecurityTenantUsersController));
            controllers.Add(typeof(SecurityUsersController));
            controllers.Add(typeof(SecurityUserEventsController));
            controllers.Add(typeof(SecurityUserEventTypesController));
            controllers.Add(typeof(SecurityUserPasswordResetTokensController));
            controllers.Add(typeof(SecurityUserSecurityGroupsController));
            controllers.Add(typeof(SecurityUserSecurityRolesController));
            controllers.Add(typeof(SecurityUserTitlesController));
            controllers.Add(typeof(SystemSettingsController));
        }

        /// <summary>
        /// 
        /// This adds the foundation system health controller
        /// 
        /// </summary>
        /// <param name="controllers"></param>
        public static void AddSystemHealthController(List<Type> controllers)
        {
            controllers.Add(typeof(Foundation.Controllers.WebAPI.SystemHealthController));  // System Health end poitns
        }


        /// <summary>
        /// 
        /// This adds the foundation system health controller
        /// 
        /// </summary>
        /// <param name="controllers"></param>
        public static void AddMonitoredApplicationsController(List<Type> controllers)
        {
            controllers.Add(typeof(Foundation.Controllers.WebAPI.MonitoredApplicationsController));  // System Health end poitns
        }


        public static void AddAuditorWebAPIControllers(List<Type> controllers)
        {
            //
            // This is the Log file viewing controller.  It depends on Auditor security
            //
            controllers.Add(typeof(Foundation.Controllers.WebAPI.LogViewerController));  // Log file viewer

            //
            // These are the Data Controllers for the auditor module
            //
            controllers.Add(typeof(AuditAccessTypesController));
            controllers.Add(typeof(AuditEventsController));
            controllers.Add(typeof(AuditEventEntityStatesController));
            controllers.Add(typeof(AuditEventErrorMessagesController));
            controllers.Add(typeof(AuditHostSystemsController));
            controllers.Add(typeof(AuditModulesController));
            controllers.Add(typeof(AuditModuleEntitiesController));
            controllers.Add(typeof(AuditPlanBsController));
            controllers.Add(typeof(AuditResourcesController));
            controllers.Add(typeof(AuditSessionsController));
            controllers.Add(typeof(AuditSourcesController));
            controllers.Add(typeof(AuditTypesController));
            controllers.Add(typeof(AuditUsersController));
            controllers.Add(typeof(AuditUserAgentsController));
            controllers.Add(typeof(ExternalCommunicationsController));
            controllers.Add(typeof(ExternalCommunicationRecipientsController));
        }
    }
}
