import { NgModule, Injectable } from '@angular/core';
import { RouterModule, Routes, DefaultUrlSerializer, UrlSerializer, UrlTree, TitleStrategy } from '@angular/router';


import { UnsavedChangesGuard } from './guards/unsaved-changes.guard';

import { LoginComponent } from './components/login/login.component';
import { AuthCallbackComponent } from './components/auth-callback/auth-callback.component';

//
// Custom components
//
import { NotFoundComponent } from './components/not-found/not-found.component';
import { AppTitleService } from './services/app-title.service';
import { AuthService } from './services/auth.service';
import { AuthGuard } from './services/auth-guard';
import { Utilities } from './services/utilities';
import { OverviewComponent } from './components/overview/overview.component'
import { NewUserComponent } from './components/new-user/new-user.component';
import { ResetPasswordComponent } from './components/reset-password/reset-password.component';
import { UserCustomListingComponent } from './components/user-custom/user-custom-listing/user-custom-listing.component';
import { UserCustomDetailComponent } from './components/user-custom/user-custom-detail/user-custom-detail.component';


//
// Beginning of imports for Security Data Components
//
import { EntityDataTokenListingComponent } from './security-data-components/entity-data-token/entity-data-token-listing/entity-data-token-listing.component';
import { EntityDataTokenDetailComponent } from './security-data-components/entity-data-token/entity-data-token-detail/entity-data-token-detail.component';
import { EntityDataTokenEventListingComponent } from './security-data-components/entity-data-token-event/entity-data-token-event-listing/entity-data-token-event-listing.component';
import { EntityDataTokenEventDetailComponent } from './security-data-components/entity-data-token-event/entity-data-token-event-detail/entity-data-token-event-detail.component';
import { EntityDataTokenEventTypeListingComponent } from './security-data-components/entity-data-token-event-type/entity-data-token-event-type-listing/entity-data-token-event-type-listing.component';
import { EntityDataTokenEventTypeDetailComponent } from './security-data-components/entity-data-token-event-type/entity-data-token-event-type-detail/entity-data-token-event-type-detail.component';
import { LoginAttemptListingComponent } from './security-data-components/login-attempt/login-attempt-listing/login-attempt-listing.component';
import { LoginAttemptDetailComponent } from './security-data-components/login-attempt/login-attempt-detail/login-attempt-detail.component';
import { ModuleListingComponent } from './security-data-components/module/module-listing/module-listing.component';
import { ModuleDetailComponent } from './security-data-components/module/module-detail/module-detail.component';
import { ModuleSecurityRoleListingComponent } from './security-data-components/module-security-role/module-security-role-listing/module-security-role-listing.component';
import { ModuleSecurityRoleDetailComponent } from './security-data-components/module-security-role/module-security-role-detail/module-security-role-detail.component';
import { OAUTHTokenListingComponent } from './security-data-components/o-a-u-t-h-token/o-a-u-t-h-token-listing/o-a-u-t-h-token-listing.component';
import { OAUTHTokenDetailComponent } from './security-data-components/o-a-u-t-h-token/o-a-u-t-h-token-detail/o-a-u-t-h-token-detail.component';
import { PrivilegeListingComponent } from './security-data-components/privilege/privilege-listing/privilege-listing.component';
import { PrivilegeDetailComponent } from './security-data-components/privilege/privilege-detail/privilege-detail.component';
import { SecurityDepartmentListingComponent } from './security-data-components/security-department/security-department-listing/security-department-listing.component';
import { SecurityDepartmentDetailComponent } from './security-data-components/security-department/security-department-detail/security-department-detail.component';
import { SecurityDepartmentUserListingComponent } from './security-data-components/security-department-user/security-department-user-listing/security-department-user-listing.component';
import { SecurityDepartmentUserDetailComponent } from './security-data-components/security-department-user/security-department-user-detail/security-department-user-detail.component';
import { SecurityGroupListingComponent } from './security-data-components/security-group/security-group-listing/security-group-listing.component';
import { SecurityGroupDetailComponent } from './security-data-components/security-group/security-group-detail/security-group-detail.component';
import { SecurityGroupSecurityRoleListingComponent } from './security-data-components/security-group-security-role/security-group-security-role-listing/security-group-security-role-listing.component';
import { SecurityGroupSecurityRoleDetailComponent } from './security-data-components/security-group-security-role/security-group-security-role-detail/security-group-security-role-detail.component';
import { SecurityOrganizationListingComponent } from './security-data-components/security-organization/security-organization-listing/security-organization-listing.component';
import { SecurityOrganizationDetailComponent } from './security-data-components/security-organization/security-organization-detail/security-organization-detail.component';
import { SecurityOrganizationUserListingComponent } from './security-data-components/security-organization-user/security-organization-user-listing/security-organization-user-listing.component';
import { SecurityOrganizationUserDetailComponent } from './security-data-components/security-organization-user/security-organization-user-detail/security-organization-user-detail.component';
import { SecurityRoleListingComponent } from './security-data-components/security-role/security-role-listing/security-role-listing.component';
import { SecurityRoleDetailComponent } from './security-data-components/security-role/security-role-detail/security-role-detail.component';
import { SecurityTeamListingComponent } from './security-data-components/security-team/security-team-listing/security-team-listing.component';
import { SecurityTeamDetailComponent } from './security-data-components/security-team/security-team-detail/security-team-detail.component';
import { SecurityTeamUserListingComponent } from './security-data-components/security-team-user/security-team-user-listing/security-team-user-listing.component';
import { SecurityTeamUserDetailComponent } from './security-data-components/security-team-user/security-team-user-detail/security-team-user-detail.component';
import { SecurityTenantListingComponent } from './security-data-components/security-tenant/security-tenant-listing/security-tenant-listing.component';
import { SecurityTenantDetailComponent } from './security-data-components/security-tenant/security-tenant-detail/security-tenant-detail.component';
import { SecurityTenantUserListingComponent } from './security-data-components/security-tenant-user/security-tenant-user-listing/security-tenant-user-listing.component';
import { SecurityTenantUserDetailComponent } from './security-data-components/security-tenant-user/security-tenant-user-detail/security-tenant-user-detail.component';
import { SecurityUserListingComponent } from './security-data-components/security-user/security-user-listing/security-user-listing.component';
import { SecurityUserDetailComponent } from './security-data-components/security-user/security-user-detail/security-user-detail.component';
import { SecurityUserEventListingComponent } from './security-data-components/security-user-event/security-user-event-listing/security-user-event-listing.component';
import { SecurityUserEventDetailComponent } from './security-data-components/security-user-event/security-user-event-detail/security-user-event-detail.component';
import { SecurityUserEventTypeListingComponent } from './security-data-components/security-user-event-type/security-user-event-type-listing/security-user-event-type-listing.component';
import { SecurityUserEventTypeDetailComponent } from './security-data-components/security-user-event-type/security-user-event-type-detail/security-user-event-type-detail.component';
import { SecurityUserPasswordResetTokenListingComponent } from './security-data-components/security-user-password-reset-token/security-user-password-reset-token-listing/security-user-password-reset-token-listing.component';
import { SecurityUserPasswordResetTokenDetailComponent } from './security-data-components/security-user-password-reset-token/security-user-password-reset-token-detail/security-user-password-reset-token-detail.component';
import { SecurityUserSecurityGroupListingComponent } from './security-data-components/security-user-security-group/security-user-security-group-listing/security-user-security-group-listing.component';
import { SecurityUserSecurityGroupDetailComponent } from './security-data-components/security-user-security-group/security-user-security-group-detail/security-user-security-group-detail.component';
import { SecurityUserSecurityRoleListingComponent } from './security-data-components/security-user-security-role/security-user-security-role-listing/security-user-security-role-listing.component';
import { SecurityUserSecurityRoleDetailComponent } from './security-data-components/security-user-security-role/security-user-security-role-detail/security-user-security-role-detail.component';
import { SecurityUserTitleListingComponent } from './security-data-components/security-user-title/security-user-title-listing/security-user-title-listing.component';
import { SecurityUserTitleDetailComponent } from './security-data-components/security-user-title/security-user-title-detail/security-user-title-detail.component';
import { SystemSettingListingComponent } from './security-data-components/system-setting/system-setting-listing/system-setting-listing.component';
import { SystemSettingDetailComponent } from './security-data-components/system-setting/system-setting-detail/system-setting-detail.component';
//
// End of imports for Security Data Components
//



//
// Beginning of imports for Auditor Data Components
//
import { AuditAccessTypeListingComponent } from './auditor-data-components/audit-access-type/audit-access-type-listing/audit-access-type-listing.component';
import { AuditAccessTypeDetailComponent } from './auditor-data-components/audit-access-type/audit-access-type-detail/audit-access-type-detail.component';
import { AuditEventListingComponent } from './auditor-data-components/audit-event/audit-event-listing/audit-event-listing.component';
import { AuditEventDetailComponent } from './auditor-data-components/audit-event/audit-event-detail/audit-event-detail.component';
import { AuditEventEntityStateListingComponent } from './auditor-data-components/audit-event-entity-state/audit-event-entity-state-listing/audit-event-entity-state-listing.component';
import { AuditEventEntityStateDetailComponent } from './auditor-data-components/audit-event-entity-state/audit-event-entity-state-detail/audit-event-entity-state-detail.component';
import { AuditEventErrorMessageListingComponent } from './auditor-data-components/audit-event-error-message/audit-event-error-message-listing/audit-event-error-message-listing.component';
import { AuditEventErrorMessageDetailComponent } from './auditor-data-components/audit-event-error-message/audit-event-error-message-detail/audit-event-error-message-detail.component';
import { AuditHostSystemListingComponent } from './auditor-data-components/audit-host-system/audit-host-system-listing/audit-host-system-listing.component';
import { AuditHostSystemDetailComponent } from './auditor-data-components/audit-host-system/audit-host-system-detail/audit-host-system-detail.component';
import { AuditModuleListingComponent } from './auditor-data-components/audit-module/audit-module-listing/audit-module-listing.component';
import { AuditModuleDetailComponent } from './auditor-data-components/audit-module/audit-module-detail/audit-module-detail.component';
import { AuditModuleEntityListingComponent } from './auditor-data-components/audit-module-entity/audit-module-entity-listing/audit-module-entity-listing.component';
import { AuditModuleEntityDetailComponent } from './auditor-data-components/audit-module-entity/audit-module-entity-detail/audit-module-entity-detail.component';
import { AuditPlanBListingComponent } from './auditor-data-components/audit-plan-b/audit-plan-b-listing/audit-plan-b-listing.component';
import { AuditPlanBDetailComponent } from './auditor-data-components/audit-plan-b/audit-plan-b-detail/audit-plan-b-detail.component';
import { AuditResourceListingComponent } from './auditor-data-components/audit-resource/audit-resource-listing/audit-resource-listing.component';
import { AuditResourceDetailComponent } from './auditor-data-components/audit-resource/audit-resource-detail/audit-resource-detail.component';
import { AuditSessionListingComponent } from './auditor-data-components/audit-session/audit-session-listing/audit-session-listing.component';
import { AuditSessionDetailComponent } from './auditor-data-components/audit-session/audit-session-detail/audit-session-detail.component';
import { AuditSourceListingComponent } from './auditor-data-components/audit-source/audit-source-listing/audit-source-listing.component';
import { AuditSourceDetailComponent } from './auditor-data-components/audit-source/audit-source-detail/audit-source-detail.component';
import { AuditTypeListingComponent } from './auditor-data-components/audit-type/audit-type-listing/audit-type-listing.component';
import { AuditTypeDetailComponent } from './auditor-data-components/audit-type/audit-type-detail/audit-type-detail.component';
import { AuditUserListingComponent } from './auditor-data-components/audit-user/audit-user-listing/audit-user-listing.component';
import { AuditUserDetailComponent } from './auditor-data-components/audit-user/audit-user-detail/audit-user-detail.component';
import { AuditUserAgentListingComponent } from './auditor-data-components/audit-user-agent/audit-user-agent-listing/audit-user-agent-listing.component';
import { AuditUserAgentDetailComponent } from './auditor-data-components/audit-user-agent/audit-user-agent-detail/audit-user-agent-detail.component';
import { ExternalCommunicationListingComponent } from './auditor-data-components/external-communication/external-communication-listing/external-communication-listing.component';
import { ExternalCommunicationDetailComponent } from './auditor-data-components/external-communication/external-communication-detail/external-communication-detail.component';
import { ExternalCommunicationRecipientListingComponent } from './auditor-data-components/external-communication-recipient/external-communication-recipient-listing/external-communication-recipient-listing.component';
import { ExternalCommunicationRecipientDetailComponent } from './auditor-data-components/external-communication-recipient/external-communication-recipient-detail/external-communication-recipient-detail.component';
//
// End of imports for Auditor Data Components
//


@Injectable()
export class LowerCaseUrlSerializer extends DefaultUrlSerializer {
  override parse(url: string): UrlTree {
    const possibleSeparators = /[?;#]/;
    const indexOfSeparator = url.search(possibleSeparators);
    let processedUrl: string;

    if (indexOfSeparator > -1) {
      const separator = url.charAt(indexOfSeparator);
      const urlParts = Utilities.splitInTwo(url, separator);
      urlParts.firstPart = urlParts.firstPart.toLowerCase();

      processedUrl = urlParts.firstPart + separator + urlParts.secondPart;
    } else {
      processedUrl = url.toLowerCase();
    }

    return super.parse(processedUrl);
  }
}


const routes: Routes = [

  //
  // Admin routes
  //
  { path: '', component: OverviewComponent, canActivate: [AuthGuard], title: 'Overview' },
  { path: 'login', component: LoginComponent, title: 'Login' },
  { path: 'google-login', component: AuthCallbackComponent, title: 'Google Login' },
  { path: 'facebook-login', component: AuthCallbackComponent, title: 'Facebook Login' },
  { path: 'twitter-login', component: AuthCallbackComponent, title: 'Twitter Login' },
  { path: 'microsoft-login', component: AuthCallbackComponent, title: 'Microsoft Login' },

  //
  // Custom component routes
  //
  { path: 'overview', component: OverviewComponent, canActivate: [AuthGuard], title: 'Overview' },
  { path: 'reset-password/:token', component: ResetPasswordComponent, title: 'Reset Password' },
  { path: 'users', component: UserCustomListingComponent, canActivate: [AuthGuard], title: 'Users' },
  { path: 'user/:id', component: UserCustomDetailComponent, canActivate: [AuthGuard], title: 'User Detail' },


  //
  // Security Data Component references - Auto Generated.
  //
  // Don't manaully change these.  Rather, the content here should be cut and pasted from the code generator to follow the system's data structures.
  //
  // Put these after the custom routes so we can override these with earlier route rules.
  //
  // Note also that this application uses a 'LowerCaseUrlSerializer' to gain case insensitivity on route matching.
  //

  //
  // Beginning of routes for Security Data Components
  //
  { path: 'entitydatatokens', component: EntityDataTokenListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Entity Data Tokens' },
  { path: 'entitydatatokens/new', component: EntityDataTokenDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Entity Data Token' },
  { path: 'entitydatatokens/:entityDataTokenId', component: EntityDataTokenDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Entity Data Token' },
  { path: 'entitydatatoken/:entityDataTokenId', component: EntityDataTokenDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Entity Data Token' },
  { path: 'entitydatatoken', redirectTo: 'entitydatatokens' },
  { path: 'entitydatatokenevents', component: EntityDataTokenEventListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Entity Data Token Events' },
  { path: 'entitydatatokenevents/new', component: EntityDataTokenEventDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Entity Data Token Event' },
  { path: 'entitydatatokenevents/:entityDataTokenEventId', component: EntityDataTokenEventDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Entity Data Token Event' },
  { path: 'entitydatatokenevent/:entityDataTokenEventId', component: EntityDataTokenEventDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Entity Data Token Event' },
  { path: 'entitydatatokenevent', redirectTo: 'entitydatatokenevents' },
  { path: 'entitydatatokeneventtypes', component: EntityDataTokenEventTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Entity Data Token Event Types' },
  { path: 'entitydatatokeneventtypes/new', component: EntityDataTokenEventTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Entity Data Token Event Type' },
  { path: 'entitydatatokeneventtypes/:entityDataTokenEventTypeId', component: EntityDataTokenEventTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Entity Data Token Event Type' },
  { path: 'entitydatatokeneventtype/:entityDataTokenEventTypeId', component: EntityDataTokenEventTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Entity Data Token Event Type' },
  { path: 'entitydatatokeneventtype', redirectTo: 'entitydatatokeneventtypes' },
  { path: 'loginattempts', component: LoginAttemptListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Login Attempts' },
  { path: 'loginattempts/new', component: LoginAttemptDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Login Attempt' },
  { path: 'loginattempts/:loginAttemptId', component: LoginAttemptDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Login Attempt' },
  { path: 'loginattempt/:loginAttemptId', component: LoginAttemptDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Login Attempt' },
  { path: 'loginattempt', redirectTo: 'loginattempts' },
  { path: 'modules', component: ModuleListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Modules' },
  { path: 'modules/new', component: ModuleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Module' },
  { path: 'modules/:moduleId', component: ModuleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Module' },
  { path: 'module/:moduleId', component: ModuleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Module' },
  { path: 'module', redirectTo: 'modules' },
  { path: 'modulesecurityroles', component: ModuleSecurityRoleListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Module Security Roles' },
  { path: 'modulesecurityroles/new', component: ModuleSecurityRoleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Module Security Role' },
  { path: 'modulesecurityroles/:moduleSecurityRoleId', component: ModuleSecurityRoleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Module Security Role' },
  { path: 'modulesecurityrole/:moduleSecurityRoleId', component: ModuleSecurityRoleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Module Security Role' },
  { path: 'modulesecurityrole', redirectTo: 'modulesecurityroles' },
  { path: 'oauthtokens', component: OAUTHTokenListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'O A U T H Tokens' },
  { path: 'oauthtokens/new', component: OAUTHTokenDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create O A U T H Token' },
  { path: 'oauthtokens/:oAUTHTokenId', component: OAUTHTokenDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit O A U T H Token' },
  { path: 'oauthtoken/:oAUTHTokenId', component: OAUTHTokenDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit O A U T H Token' },
  { path: 'oauthtoken', redirectTo: 'oauthtokens' },
  { path: 'privileges', component: PrivilegeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Privileges' },
  { path: 'privileges/new', component: PrivilegeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Privilege' },
  { path: 'privileges/:privilegeId', component: PrivilegeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Privilege' },
  { path: 'privilege/:privilegeId', component: PrivilegeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Privilege' },
  { path: 'privilege', redirectTo: 'privileges' },
  { path: 'securitydepartments', component: SecurityDepartmentListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security Departments' },
  { path: 'securitydepartments/new', component: SecurityDepartmentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security Department' },
  { path: 'securitydepartments/:securityDepartmentId', component: SecurityDepartmentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Department' },
  { path: 'securitydepartment/:securityDepartmentId', component: SecurityDepartmentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Department' },
  { path: 'securitydepartment', redirectTo: 'securitydepartments' },
  { path: 'securitydepartmentusers', component: SecurityDepartmentUserListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security Department Users' },
  { path: 'securitydepartmentusers/new', component: SecurityDepartmentUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security Department User' },
  { path: 'securitydepartmentusers/:securityDepartmentUserId', component: SecurityDepartmentUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Department User' },
  { path: 'securitydepartmentuser/:securityDepartmentUserId', component: SecurityDepartmentUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Department User' },
  { path: 'securitydepartmentuser', redirectTo: 'securitydepartmentusers' },
  { path: 'securitygroups', component: SecurityGroupListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security Groups' },
  { path: 'securitygroups/new', component: SecurityGroupDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security Group' },
  { path: 'securitygroups/:securityGroupId', component: SecurityGroupDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Group' },
  { path: 'securitygroup/:securityGroupId', component: SecurityGroupDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Group' },
  { path: 'securitygroup', redirectTo: 'securitygroups' },
  { path: 'securitygroupsecurityroles', component: SecurityGroupSecurityRoleListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security Group Security Roles' },
  { path: 'securitygroupsecurityroles/new', component: SecurityGroupSecurityRoleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security Group Security Role' },
  { path: 'securitygroupsecurityroles/:securityGroupSecurityRoleId', component: SecurityGroupSecurityRoleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Group Security Role' },
  { path: 'securitygroupsecurityrole/:securityGroupSecurityRoleId', component: SecurityGroupSecurityRoleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Group Security Role' },
  { path: 'securitygroupsecurityrole', redirectTo: 'securitygroupsecurityroles' },
  { path: 'securityorganizations', component: SecurityOrganizationListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security Organizations' },
  { path: 'securityorganizations/new', component: SecurityOrganizationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security Organization' },
  { path: 'securityorganizations/:securityOrganizationId', component: SecurityOrganizationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Organization' },
  { path: 'securityorganization/:securityOrganizationId', component: SecurityOrganizationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Organization' },
  { path: 'securityorganization', redirectTo: 'securityorganizations' },
  { path: 'securityorganizationusers', component: SecurityOrganizationUserListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security Organization Users' },
  { path: 'securityorganizationusers/new', component: SecurityOrganizationUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security Organization User' },
  { path: 'securityorganizationusers/:securityOrganizationUserId', component: SecurityOrganizationUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Organization User' },
  { path: 'securityorganizationuser/:securityOrganizationUserId', component: SecurityOrganizationUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Organization User' },
  { path: 'securityorganizationuser', redirectTo: 'securityorganizationusers' },
  { path: 'securityroles', component: SecurityRoleListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security Roles' },
  { path: 'securityroles/new', component: SecurityRoleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security Role' },
  { path: 'securityroles/:securityRoleId', component: SecurityRoleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Role' },
  { path: 'securityrole/:securityRoleId', component: SecurityRoleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Role' },
  { path: 'securityrole', redirectTo: 'securityroles' },
  { path: 'securityteams', component: SecurityTeamListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security Teams' },
  { path: 'securityteams/new', component: SecurityTeamDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security Team' },
  { path: 'securityteams/:securityTeamId', component: SecurityTeamDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Team' },
  { path: 'securityteam/:securityTeamId', component: SecurityTeamDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Team' },
  { path: 'securityteam', redirectTo: 'securityteams' },
  { path: 'securityteamusers', component: SecurityTeamUserListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security Team Users' },
  { path: 'securityteamusers/new', component: SecurityTeamUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security Team User' },
  { path: 'securityteamusers/:securityTeamUserId', component: SecurityTeamUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Team User' },
  { path: 'securityteamuser/:securityTeamUserId', component: SecurityTeamUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Team User' },
  { path: 'securityteamuser', redirectTo: 'securityteamusers' },
  { path: 'securitytenants', component: SecurityTenantListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security Tenants' },
  { path: 'securitytenants/new', component: SecurityTenantDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security Tenant' },
  { path: 'securitytenants/:securityTenantId', component: SecurityTenantDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Tenant' },
  { path: 'securitytenant/:securityTenantId', component: SecurityTenantDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Tenant' },
  { path: 'securitytenant', redirectTo: 'securitytenants' },
  { path: 'securitytenantusers', component: SecurityTenantUserListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security Tenant Users' },
  { path: 'securitytenantusers/new', component: SecurityTenantUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security Tenant User' },
  { path: 'securitytenantusers/:securityTenantUserId', component: SecurityTenantUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Tenant User' },
  { path: 'securitytenantuser/:securityTenantUserId', component: SecurityTenantUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security Tenant User' },
  { path: 'securitytenantuser', redirectTo: 'securitytenantusers' },
  { path: 'securityusers', component: SecurityUserListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security Users' },
  { path: 'securityusers/new', component: SecurityUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security User' },
  { path: 'securityusers/:securityUserId', component: SecurityUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security User' },
  { path: 'securityuser/:securityUserId', component: SecurityUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security User' },
  { path: 'securityuser', redirectTo: 'securityusers' },
  { path: 'securityuserevents', component: SecurityUserEventListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security User Events' },
  { path: 'securityuserevents/new', component: SecurityUserEventDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security User Event' },
  { path: 'securityuserevents/:securityUserEventId', component: SecurityUserEventDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security User Event' },
  { path: 'securityuserevent/:securityUserEventId', component: SecurityUserEventDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security User Event' },
  { path: 'securityuserevent', redirectTo: 'securityuserevents' },
  { path: 'securityusereventtypes', component: SecurityUserEventTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security User Event Types' },
  { path: 'securityusereventtypes/new', component: SecurityUserEventTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security User Event Type' },
  { path: 'securityusereventtypes/:securityUserEventTypeId', component: SecurityUserEventTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security User Event Type' },
  { path: 'securityusereventtype/:securityUserEventTypeId', component: SecurityUserEventTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security User Event Type' },
  { path: 'securityusereventtype', redirectTo: 'securityusereventtypes' },
  { path: 'securityuserpasswordresettokens', component: SecurityUserPasswordResetTokenListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security User Password Reset Tokens' },
  { path: 'securityuserpasswordresettokens/new', component: SecurityUserPasswordResetTokenDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security User Password Reset Token' },
  { path: 'securityuserpasswordresettokens/:securityUserPasswordResetTokenId', component: SecurityUserPasswordResetTokenDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security User Password Reset Token' },
  { path: 'securityuserpasswordresettoken/:securityUserPasswordResetTokenId', component: SecurityUserPasswordResetTokenDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security User Password Reset Token' },
  { path: 'securityuserpasswordresettoken', redirectTo: 'securityuserpasswordresettokens' },
  { path: 'securityusersecuritygroups', component: SecurityUserSecurityGroupListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security User Security Groups' },
  { path: 'securityusersecuritygroups/new', component: SecurityUserSecurityGroupDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security User Security Group' },
  { path: 'securityusersecuritygroups/:securityUserSecurityGroupId', component: SecurityUserSecurityGroupDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security User Security Group' },
  { path: 'securityusersecuritygroup/:securityUserSecurityGroupId', component: SecurityUserSecurityGroupDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security User Security Group' },
  { path: 'securityusersecuritygroup', redirectTo: 'securityusersecuritygroups' },
  { path: 'securityusersecurityroles', component: SecurityUserSecurityRoleListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security User Security Roles' },
  { path: 'securityusersecurityroles/new', component: SecurityUserSecurityRoleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security User Security Role' },
  { path: 'securityusersecurityroles/:securityUserSecurityRoleId', component: SecurityUserSecurityRoleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security User Security Role' },
  { path: 'securityusersecurityrole/:securityUserSecurityRoleId', component: SecurityUserSecurityRoleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security User Security Role' },
  { path: 'securityusersecurityrole', redirectTo: 'securityusersecurityroles' },
  { path: 'securityusertitles', component: SecurityUserTitleListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Security User Titles' },
  { path: 'securityusertitles/new', component: SecurityUserTitleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Security User Title' },
  { path: 'securityusertitles/:securityUserTitleId', component: SecurityUserTitleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security User Title' },
  { path: 'securityusertitle/:securityUserTitleId', component: SecurityUserTitleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Security User Title' },
  { path: 'securityusertitle', redirectTo: 'securityusertitles' },
  { path: 'systemsettings', component: SystemSettingListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'System Settings' },
  { path: 'systemsettings/new', component: SystemSettingDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create System Setting' },
  { path: 'systemsettings/:systemSettingId', component: SystemSettingDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit System Setting' },
  { path: 'systemsetting/:systemSettingId', component: SystemSettingDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit System Setting' },
  { path: 'systemsetting', redirectTo: 'systemsettings' },
  //
  // End of routes for Security Data Components
  //





  //
  // Auditor Data Component references - Auto Generated.
  //
  // Don't manaully change these.  Rather, the content here should be cut and pasted from the code generator to follow the system's data structures.
  //
  // Put these after the custom routes so we can override these with earlier route rules.
  //
  // Note also that this application uses a 'LowerCaseUrlSerializer' to gain case insensitivity on route matching.
  //


  //
  // Beginning of routes for Auditor Data Components
  //
  { path: 'auditaccesstypes', component: AuditAccessTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Audit Access Types' },
  { path: 'auditaccesstypes/new', component: AuditAccessTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Audit Access Type' },
  { path: 'auditaccesstypes/:auditAccessTypeId', component: AuditAccessTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Access Type' },
  { path: 'auditaccesstype/:auditAccessTypeId', component: AuditAccessTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Access Type' },
  { path: 'auditaccesstype', redirectTo: 'auditaccesstypes' },
  { path: 'auditevents', component: AuditEventListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Audit Events' },
  { path: 'auditevents/new', component: AuditEventDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Audit Event' },
  { path: 'auditevents/:auditEventId', component: AuditEventDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Event' },
  { path: 'auditevent/:auditEventId', component: AuditEventDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Event' },
  { path: 'auditevent', redirectTo: 'auditevents' },
  { path: 'auditevententitystates', component: AuditEventEntityStateListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Audit Event Entity States' },
  { path: 'auditevententitystates/new', component: AuditEventEntityStateDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Audit Event Entity State' },
  { path: 'auditevententitystates/:auditEventEntityStateId', component: AuditEventEntityStateDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Event Entity State' },
  { path: 'auditevententitystate/:auditEventEntityStateId', component: AuditEventEntityStateDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Event Entity State' },
  { path: 'auditevententitystate', redirectTo: 'auditevententitystates' },
  { path: 'auditeventerrormessages', component: AuditEventErrorMessageListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Audit Event Error Messages' },
  { path: 'auditeventerrormessages/new', component: AuditEventErrorMessageDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Audit Event Error Message' },
  { path: 'auditeventerrormessages/:auditEventErrorMessageId', component: AuditEventErrorMessageDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Event Error Message' },
  { path: 'auditeventerrormessage/:auditEventErrorMessageId', component: AuditEventErrorMessageDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Event Error Message' },
  { path: 'auditeventerrormessage', redirectTo: 'auditeventerrormessages' },
  { path: 'audithostsystems', component: AuditHostSystemListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Audit Host Systems' },
  { path: 'audithostsystems/new', component: AuditHostSystemDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Audit Host System' },
  { path: 'audithostsystems/:auditHostSystemId', component: AuditHostSystemDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Host System' },
  { path: 'audithostsystem/:auditHostSystemId', component: AuditHostSystemDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Host System' },
  { path: 'audithostsystem', redirectTo: 'audithostsystems' },
  { path: 'auditmodules', component: AuditModuleListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Audit Modules' },
  { path: 'auditmodules/new', component: AuditModuleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Audit Module' },
  { path: 'auditmodules/:auditModuleId', component: AuditModuleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Module' },
  { path: 'auditmodule/:auditModuleId', component: AuditModuleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Module' },
  { path: 'auditmodule', redirectTo: 'auditmodules' },
  { path: 'auditmoduleentities', component: AuditModuleEntityListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Audit Module Entities' },
  { path: 'auditmoduleentities/new', component: AuditModuleEntityDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Audit Module Entity' },
  { path: 'auditmoduleentities/:auditModuleEntityId', component: AuditModuleEntityDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Module Entity' },
  { path: 'auditmoduleentity/:auditModuleEntityId', component: AuditModuleEntityDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Module Entity' },
  { path: 'auditmoduleentity', redirectTo: 'auditmoduleentities' },
  { path: 'auditplanbs', component: AuditPlanBListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Audit Plan Bs' },
  { path: 'auditplanbs/new', component: AuditPlanBDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Audit Plan B' },
  { path: 'auditplanbs/:auditPlanBId', component: AuditPlanBDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Plan B' },
  { path: 'auditplanb/:auditPlanBId', component: AuditPlanBDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Plan B' },
  { path: 'auditplanb', redirectTo: 'auditplanbs' },
  { path: 'auditresources', component: AuditResourceListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Audit Resources' },
  { path: 'auditresources/new', component: AuditResourceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Audit Resource' },
  { path: 'auditresources/:auditResourceId', component: AuditResourceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Resource' },
  { path: 'auditresource/:auditResourceId', component: AuditResourceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Resource' },
  { path: 'auditresource', redirectTo: 'auditresources' },
  { path: 'auditsessions', component: AuditSessionListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Audit Sessions' },
  { path: 'auditsessions/new', component: AuditSessionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Audit Session' },
  { path: 'auditsessions/:auditSessionId', component: AuditSessionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Session' },
  { path: 'auditsession/:auditSessionId', component: AuditSessionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Session' },
  { path: 'auditsession', redirectTo: 'auditsessions' },
  { path: 'auditsources', component: AuditSourceListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Audit Sources' },
  { path: 'auditsources/new', component: AuditSourceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Audit Source' },
  { path: 'auditsources/:auditSourceId', component: AuditSourceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Source' },
  { path: 'auditsource/:auditSourceId', component: AuditSourceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Source' },
  { path: 'auditsource', redirectTo: 'auditsources' },
  { path: 'audittypes', component: AuditTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Audit Types' },
  { path: 'audittypes/new', component: AuditTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Audit Type' },
  { path: 'audittypes/:auditTypeId', component: AuditTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Type' },
  { path: 'audittype/:auditTypeId', component: AuditTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit Type' },
  { path: 'audittype', redirectTo: 'audittypes' },
  { path: 'auditusers', component: AuditUserListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Audit Users' },
  { path: 'auditusers/new', component: AuditUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Audit User' },
  { path: 'auditusers/:auditUserId', component: AuditUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit User' },
  { path: 'audituser/:auditUserId', component: AuditUserDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit User' },
  { path: 'audituser', redirectTo: 'auditusers' },
  { path: 'audituseragents', component: AuditUserAgentListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Audit User Agents' },
  { path: 'audituseragents/new', component: AuditUserAgentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Audit User Agent' },
  { path: 'audituseragents/:auditUserAgentId', component: AuditUserAgentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit User Agent' },
  { path: 'audituseragent/:auditUserAgentId', component: AuditUserAgentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Audit User Agent' },
  { path: 'audituseragent', redirectTo: 'audituseragents' },
  { path: 'externalcommunications', component: ExternalCommunicationListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'External Communications' },
  { path: 'externalcommunications/new', component: ExternalCommunicationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create External Communication' },
  { path: 'externalcommunications/:externalCommunicationId', component: ExternalCommunicationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit External Communication' },
  { path: 'externalcommunication/:externalCommunicationId', component: ExternalCommunicationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit External Communication' },
  { path: 'externalcommunication', redirectTo: 'externalcommunications' },
  { path: 'externalcommunicationrecipients', component: ExternalCommunicationRecipientListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'External Communication Recipients' },
  { path: 'externalcommunicationrecipients/new', component: ExternalCommunicationRecipientDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create External Communication Recipient' },
  { path: 'externalcommunicationrecipients/:externalCommunicationRecipientId', component: ExternalCommunicationRecipientDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit External Communication Recipient' },
  { path: 'externalcommunicationrecipient/:externalCommunicationRecipientId', component: ExternalCommunicationRecipientDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit External Communication Recipient' },
  { path: 'externalcommunicationrecipient', redirectTo: 'externalcommunicationrecipients' },
  //
  // End of routes for Auditor Data Components
  //


  //
  // Default routes
  //
  { path: 'home', redirectTo: '/', pathMatch: 'full' },
  { path: '**', component: NotFoundComponent, title: 'Page Not Found' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {
    scrollPositionRestoration: 'enabled',   // Scroll to top or previous position when opening components
    anchorScrolling: 'enabled'              // Enables anchor scrolling so browser will scroll to to fragments within component templates when links with fragment identifiers are clicked (eg. #section1 )
  })],
  exports: [RouterModule],
  providers: [
    AuthService,
    { provide: TitleStrategy, useClass: AppTitleService },
    { provide: UrlSerializer, useClass: LowerCaseUrlSerializer }]
})
export class AppRoutingModule { }
