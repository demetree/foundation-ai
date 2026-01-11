import {Injectable} from '@angular/core';
import {EntityDataTokenService} from  './entity-data-token.service';
import {EntityDataTokenEventService} from  './entity-data-token-event.service';
import {EntityDataTokenEventTypeService} from  './entity-data-token-event-type.service';
import {LoginAttemptService} from  './login-attempt.service';
import {ModuleService} from  './module.service';
import {ModuleSecurityRoleService} from  './module-security-role.service';
import {OAUTHTokenService} from  './o-a-u-t-h-token.service';
import {PrivilegeService} from  './privilege.service';
import {SecurityDepartmentService} from  './security-department.service';
import {SecurityDepartmentUserService} from  './security-department-user.service';
import {SecurityGroupService} from  './security-group.service';
import {SecurityGroupSecurityRoleService} from  './security-group-security-role.service';
import {SecurityOrganizationService} from  './security-organization.service';
import {SecurityOrganizationUserService} from  './security-organization-user.service';
import {SecurityRoleService} from  './security-role.service';
import {SecurityTeamService} from  './security-team.service';
import {SecurityTeamUserService} from  './security-team-user.service';
import {SecurityTenantService} from  './security-tenant.service';
import {SecurityTenantUserService} from  './security-tenant-user.service';
import {SecurityUserService} from  './security-user.service';
import {SecurityUserEventService} from  './security-user-event.service';
import {SecurityUserEventTypeService} from  './security-user-event-type.service';
import {SecurityUserPasswordResetTokenService} from  './security-user-password-reset-token.service';
import {SecurityUserSecurityGroupService} from  './security-user-security-group.service';
import {SecurityUserSecurityRoleService} from  './security-user-security-role.service';
import {SecurityUserTitleService} from  './security-user-title.service';
import {SystemSettingService} from  './system-setting.service';


@Injectable({
  providedIn: 'root'
})
export class SecurityDataServiceManagerService  {

    constructor(public entityDataTokenService: EntityDataTokenService
              , public entityDataTokenEventService: EntityDataTokenEventService
              , public entityDataTokenEventTypeService: EntityDataTokenEventTypeService
              , public loginAttemptService: LoginAttemptService
              , public moduleService: ModuleService
              , public moduleSecurityRoleService: ModuleSecurityRoleService
              , public oAUTHTokenService: OAUTHTokenService
              , public privilegeService: PrivilegeService
              , public securityDepartmentService: SecurityDepartmentService
              , public securityDepartmentUserService: SecurityDepartmentUserService
              , public securityGroupService: SecurityGroupService
              , public securityGroupSecurityRoleService: SecurityGroupSecurityRoleService
              , public securityOrganizationService: SecurityOrganizationService
              , public securityOrganizationUserService: SecurityOrganizationUserService
              , public securityRoleService: SecurityRoleService
              , public securityTeamService: SecurityTeamService
              , public securityTeamUserService: SecurityTeamUserService
              , public securityTenantService: SecurityTenantService
              , public securityTenantUserService: SecurityTenantUserService
              , public securityUserService: SecurityUserService
              , public securityUserEventService: SecurityUserEventService
              , public securityUserEventTypeService: SecurityUserEventTypeService
              , public securityUserPasswordResetTokenService: SecurityUserPasswordResetTokenService
              , public securityUserSecurityGroupService: SecurityUserSecurityGroupService
              , public securityUserSecurityRoleService: SecurityUserSecurityRoleService
              , public securityUserTitleService: SecurityUserTitleService
              , public systemSettingService: SystemSettingService
) { }  


    public ClearAllCaches() {

        this.entityDataTokenService.ClearAllCaches();
        this.entityDataTokenEventService.ClearAllCaches();
        this.entityDataTokenEventTypeService.ClearAllCaches();
        this.loginAttemptService.ClearAllCaches();
        this.moduleService.ClearAllCaches();
        this.moduleSecurityRoleService.ClearAllCaches();
        this.oAUTHTokenService.ClearAllCaches();
        this.privilegeService.ClearAllCaches();
        this.securityDepartmentService.ClearAllCaches();
        this.securityDepartmentUserService.ClearAllCaches();
        this.securityGroupService.ClearAllCaches();
        this.securityGroupSecurityRoleService.ClearAllCaches();
        this.securityOrganizationService.ClearAllCaches();
        this.securityOrganizationUserService.ClearAllCaches();
        this.securityRoleService.ClearAllCaches();
        this.securityTeamService.ClearAllCaches();
        this.securityTeamUserService.ClearAllCaches();
        this.securityTenantService.ClearAllCaches();
        this.securityTenantUserService.ClearAllCaches();
        this.securityUserService.ClearAllCaches();
        this.securityUserEventService.ClearAllCaches();
        this.securityUserEventTypeService.ClearAllCaches();
        this.securityUserPasswordResetTokenService.ClearAllCaches();
        this.securityUserSecurityGroupService.ClearAllCaches();
        this.securityUserSecurityRoleService.ClearAllCaches();
        this.securityUserTitleService.ClearAllCaches();
        this.systemSettingService.ClearAllCaches();
    }
}