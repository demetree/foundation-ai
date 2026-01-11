import { NgModule, ErrorHandler } from '@angular/core';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { FormsModule } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';
import { RouteReuseStrategy, RouterModule } from '@angular/router';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { MatExpansionModule } from '@angular/material/expansion';


import { NgbModalModule, NgbNavModule, NgbTooltipModule, NgbPopoverModule, NgbAccordionModule, NgbDropdownModule, NgbCarouselModule }   from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
import { OAuthModule, OAuthStorage } from 'angular-oauth2-oidc';
import { ToastaModule } from 'ngx-toasta';
import { NgSelectModule } from '@ng-select/ng-select';
import { NgChartsModule } from 'ng2-charts';

import { ZXingScannerModule } from '@zxing/ngx-scanner';

import { AppRoutingModule } from './app-routing.module';
import { AppErrorHandler } from './app-error.handler';
import { AppTranslationService, TranslateLanguageLoader } from './services/app-translation.service';
import { ConfigurationService } from './services/configuration.service';
import { AlertService } from './services/alert.service';
import { LocalStoreManager } from './services/local-store-manager.service';
import { OidcHelperService, OidcTempStorage } from './services/oidc-helper.service';

import { EqualValidator } from './directives/equal-validator.directive';
import { AutofocusDirective } from './directives/autofocus.directive';
import { BootstrapTabDirective } from './directives/bootstrap-tab.directive';
import { SpinnerDirective } from './directives/spinner.directive';


//
// Custom pipes
//
import { GroupByPipe } from './pipes/group-by.pipe';
import { FilterAndJoinPipe } from './pipes/filter-and-join.pipe';
import { BigNumberFormatPipe } from './pipes/big-number-format.pipe';


//
// Custom controls
//
import { BooleanIconComponent } from './components/controls/boolean-icon.component';


//
// Admin components
//
import { AppComponent } from './app.component';
import { LoginComponent } from './components/login/login.component';
import { AuthCallbackComponent } from './components/auth-callback/auth-callback.component';
import { NotFoundComponent } from './components/not-found/not-found.component';


import { SearchBoxComponent } from './components/controls/search-box.component';
import { OverviewComponent } from './components/overview/overview.component';
import { ModalComponent } from './components/modal/modal.component';
import { HeaderComponent } from './components/header/header.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { ResetPasswordComponent } from './components/reset-password/reset-password.component';
import { NewUserComponent } from './components/new-user/new-user.component';

//
// For optimizing what happens when you go back with the back button, to retain state.
//
import { RouteReuseService } from './utility-services/route-reuse.service';

//
// Standard way to get current and previous routes
//
import { NavigationService } from './utility-services/navigation.service';

//
// Custom confirmation dialog
//
import { ConfirmationService } from './services/confirmation-service';

//
// Data support services
//
import { CurrentUserService } from './services/current-user.service';


//
// Security Data services - Auto Generated
//
import { SecurityDataServiceManagerService } from './security-data-services/security-data-service-manager.service';
import { EntityDataTokenService } from './security-data-services/entity-data-token.service';
import { EntityDataTokenEventService } from './security-data-services/entity-data-token-event.service';
import { EntityDataTokenEventTypeService } from './security-data-services/entity-data-token-event-type.service';
import { LoginAttemptService } from './security-data-services/login-attempt.service';
import { ModuleService } from './security-data-services/module.service';
import { ModuleSecurityRoleService } from './security-data-services/module-security-role.service';
import { OAUTHTokenService } from './security-data-services/o-a-u-t-h-token.service';
import { PrivilegeService } from './security-data-services/privilege.service';
import { SecurityDepartmentService } from './security-data-services/security-department.service';
import { SecurityDepartmentUserService } from './security-data-services/security-department-user.service';
import { SecurityGroupService } from './security-data-services/security-group.service';
import { SecurityGroupSecurityRoleService } from './security-data-services/security-group-security-role.service';
import { SecurityOrganizationService } from './security-data-services/security-organization.service';
import { SecurityOrganizationUserService } from './security-data-services/security-organization-user.service';
import { SecurityRoleService } from './security-data-services/security-role.service';
import { SecurityTeamService } from './security-data-services/security-team.service';
import { SecurityTeamUserService } from './security-data-services/security-team-user.service';
import { SecurityTenantService } from './security-data-services/security-tenant.service';
import { SecurityTenantUserService } from './security-data-services/security-tenant-user.service';
import { SecurityUserService } from './security-data-services/security-user.service';
import { SecurityUserEventService } from './security-data-services/security-user-event.service';
import { SecurityUserEventTypeService } from './security-data-services/security-user-event-type.service';
import { SecurityUserPasswordResetTokenService } from './security-data-services/security-user-password-reset-token.service';
import { SecurityUserSecurityGroupService } from './security-data-services/security-user-security-group.service';
import { SecurityUserSecurityRoleService } from './security-data-services/security-user-security-role.service';
import { SecurityUserTitleService } from './security-data-services/security-user-title.service';
import { SystemSettingService } from './security-data-services/system-setting.service';
//
// End Security Data Services
//

//
// Auditor Data Services - Auto Genrated
//
import { AuditorDataServiceManagerService } from './auditor-data-services/auditor-data-service-manager.service';
import { AuditAccessTypeService } from './auditor-data-services/audit-access-type.service';
import { AuditEventService } from './auditor-data-services/audit-event.service';
import { AuditEventEntityStateService } from './auditor-data-services/audit-event-entity-state.service';
import { AuditEventErrorMessageService } from './auditor-data-services/audit-event-error-message.service';
import { AuditHostSystemService } from './auditor-data-services/audit-host-system.service';
import { AuditModuleService } from './auditor-data-services/audit-module.service';
import { AuditModuleEntityService } from './auditor-data-services/audit-module-entity.service';
import { AuditPlanBService } from './auditor-data-services/audit-plan-b.service';
import { AuditResourceService } from './auditor-data-services/audit-resource.service';
import { AuditSessionService } from './auditor-data-services/audit-session.service';
import { AuditSourceService } from './auditor-data-services/audit-source.service';
import { AuditTypeService } from './auditor-data-services/audit-type.service';
import { AuditUserService } from './auditor-data-services/audit-user.service';
import { AuditUserAgentService } from './auditor-data-services/audit-user-agent.service';
import { ExternalCommunicationService } from './auditor-data-services/external-communication.service';
import { ExternalCommunicationRecipientService } from './auditor-data-services/external-communication-recipient.service'
//
// End Auditor Data Services
//




//
// Security Data Components - Auto generated
//
import { EntityDataTokenListingComponent } from './security-data-components/entity-data-token/entity-data-token-listing/entity-data-token-listing.component';
import { EntityDataTokenAddEditComponent } from './security-data-components/entity-data-token/entity-data-token-add-edit/entity-data-token-add-edit.component';
import { EntityDataTokenDetailComponent } from './security-data-components/entity-data-token/entity-data-token-detail/entity-data-token-detail.component';
import { EntityDataTokenTableComponent } from './security-data-components/entity-data-token/entity-data-token-table/entity-data-token-table.component';
import { EntityDataTokenEventListingComponent } from './security-data-components/entity-data-token-event/entity-data-token-event-listing/entity-data-token-event-listing.component';
import { EntityDataTokenEventAddEditComponent } from './security-data-components/entity-data-token-event/entity-data-token-event-add-edit/entity-data-token-event-add-edit.component';
import { EntityDataTokenEventDetailComponent } from './security-data-components/entity-data-token-event/entity-data-token-event-detail/entity-data-token-event-detail.component';
import { EntityDataTokenEventTableComponent } from './security-data-components/entity-data-token-event/entity-data-token-event-table/entity-data-token-event-table.component';
import { EntityDataTokenEventTypeListingComponent } from './security-data-components/entity-data-token-event-type/entity-data-token-event-type-listing/entity-data-token-event-type-listing.component';
import { EntityDataTokenEventTypeAddEditComponent } from './security-data-components/entity-data-token-event-type/entity-data-token-event-type-add-edit/entity-data-token-event-type-add-edit.component';
import { EntityDataTokenEventTypeDetailComponent } from './security-data-components/entity-data-token-event-type/entity-data-token-event-type-detail/entity-data-token-event-type-detail.component';
import { EntityDataTokenEventTypeTableComponent } from './security-data-components/entity-data-token-event-type/entity-data-token-event-type-table/entity-data-token-event-type-table.component';
import { LoginAttemptListingComponent } from './security-data-components/login-attempt/login-attempt-listing/login-attempt-listing.component';
import { LoginAttemptAddEditComponent } from './security-data-components/login-attempt/login-attempt-add-edit/login-attempt-add-edit.component';
import { LoginAttemptDetailComponent } from './security-data-components/login-attempt/login-attempt-detail/login-attempt-detail.component';
import { LoginAttemptTableComponent } from './security-data-components/login-attempt/login-attempt-table/login-attempt-table.component';
import { ModuleListingComponent } from './security-data-components/module/module-listing/module-listing.component';
import { ModuleAddEditComponent } from './security-data-components/module/module-add-edit/module-add-edit.component';
import { ModuleDetailComponent } from './security-data-components/module/module-detail/module-detail.component';
import { ModuleTableComponent } from './security-data-components/module/module-table/module-table.component';
import { ModuleSecurityRoleListingComponent } from './security-data-components/module-security-role/module-security-role-listing/module-security-role-listing.component';
import { ModuleSecurityRoleAddEditComponent } from './security-data-components/module-security-role/module-security-role-add-edit/module-security-role-add-edit.component';
import { ModuleSecurityRoleDetailComponent } from './security-data-components/module-security-role/module-security-role-detail/module-security-role-detail.component';
import { ModuleSecurityRoleTableComponent } from './security-data-components/module-security-role/module-security-role-table/module-security-role-table.component';
import { OAUTHTokenListingComponent } from './security-data-components/o-a-u-t-h-token/o-a-u-t-h-token-listing/o-a-u-t-h-token-listing.component';
import { OAUTHTokenAddEditComponent } from './security-data-components/o-a-u-t-h-token/o-a-u-t-h-token-add-edit/o-a-u-t-h-token-add-edit.component';
import { OAUTHTokenDetailComponent } from './security-data-components/o-a-u-t-h-token/o-a-u-t-h-token-detail/o-a-u-t-h-token-detail.component';
import { OAUTHTokenTableComponent } from './security-data-components/o-a-u-t-h-token/o-a-u-t-h-token-table/o-a-u-t-h-token-table.component';
import { PrivilegeListingComponent } from './security-data-components/privilege/privilege-listing/privilege-listing.component';
import { PrivilegeAddEditComponent } from './security-data-components/privilege/privilege-add-edit/privilege-add-edit.component';
import { PrivilegeDetailComponent } from './security-data-components/privilege/privilege-detail/privilege-detail.component';
import { PrivilegeTableComponent } from './security-data-components/privilege/privilege-table/privilege-table.component';
import { SecurityDepartmentListingComponent } from './security-data-components/security-department/security-department-listing/security-department-listing.component';
import { SecurityDepartmentAddEditComponent } from './security-data-components/security-department/security-department-add-edit/security-department-add-edit.component';
import { SecurityDepartmentDetailComponent } from './security-data-components/security-department/security-department-detail/security-department-detail.component';
import { SecurityDepartmentTableComponent } from './security-data-components/security-department/security-department-table/security-department-table.component';
import { SecurityDepartmentUserListingComponent } from './security-data-components/security-department-user/security-department-user-listing/security-department-user-listing.component';
import { SecurityDepartmentUserAddEditComponent } from './security-data-components/security-department-user/security-department-user-add-edit/security-department-user-add-edit.component';
import { SecurityDepartmentUserDetailComponent } from './security-data-components/security-department-user/security-department-user-detail/security-department-user-detail.component';
import { SecurityDepartmentUserTableComponent } from './security-data-components/security-department-user/security-department-user-table/security-department-user-table.component';
import { SecurityGroupListingComponent } from './security-data-components/security-group/security-group-listing/security-group-listing.component';
import { SecurityGroupAddEditComponent } from './security-data-components/security-group/security-group-add-edit/security-group-add-edit.component';
import { SecurityGroupDetailComponent } from './security-data-components/security-group/security-group-detail/security-group-detail.component';
import { SecurityGroupTableComponent } from './security-data-components/security-group/security-group-table/security-group-table.component';
import { SecurityGroupSecurityRoleListingComponent } from './security-data-components/security-group-security-role/security-group-security-role-listing/security-group-security-role-listing.component';
import { SecurityGroupSecurityRoleAddEditComponent } from './security-data-components/security-group-security-role/security-group-security-role-add-edit/security-group-security-role-add-edit.component';
import { SecurityGroupSecurityRoleDetailComponent } from './security-data-components/security-group-security-role/security-group-security-role-detail/security-group-security-role-detail.component';
import { SecurityGroupSecurityRoleTableComponent } from './security-data-components/security-group-security-role/security-group-security-role-table/security-group-security-role-table.component';
import { SecurityOrganizationListingComponent } from './security-data-components/security-organization/security-organization-listing/security-organization-listing.component';
import { SecurityOrganizationAddEditComponent } from './security-data-components/security-organization/security-organization-add-edit/security-organization-add-edit.component';
import { SecurityOrganizationDetailComponent } from './security-data-components/security-organization/security-organization-detail/security-organization-detail.component';
import { SecurityOrganizationTableComponent } from './security-data-components/security-organization/security-organization-table/security-organization-table.component';
import { SecurityOrganizationUserListingComponent } from './security-data-components/security-organization-user/security-organization-user-listing/security-organization-user-listing.component';
import { SecurityOrganizationUserAddEditComponent } from './security-data-components/security-organization-user/security-organization-user-add-edit/security-organization-user-add-edit.component';
import { SecurityOrganizationUserDetailComponent } from './security-data-components/security-organization-user/security-organization-user-detail/security-organization-user-detail.component';
import { SecurityOrganizationUserTableComponent } from './security-data-components/security-organization-user/security-organization-user-table/security-organization-user-table.component';
import { SecurityRoleListingComponent } from './security-data-components/security-role/security-role-listing/security-role-listing.component';
import { SecurityRoleAddEditComponent } from './security-data-components/security-role/security-role-add-edit/security-role-add-edit.component';
import { SecurityRoleDetailComponent } from './security-data-components/security-role/security-role-detail/security-role-detail.component';
import { SecurityRoleTableComponent } from './security-data-components/security-role/security-role-table/security-role-table.component';
import { SecurityTeamListingComponent } from './security-data-components/security-team/security-team-listing/security-team-listing.component';
import { SecurityTeamAddEditComponent } from './security-data-components/security-team/security-team-add-edit/security-team-add-edit.component';
import { SecurityTeamDetailComponent } from './security-data-components/security-team/security-team-detail/security-team-detail.component';
import { SecurityTeamTableComponent } from './security-data-components/security-team/security-team-table/security-team-table.component';
import { SecurityTeamUserListingComponent } from './security-data-components/security-team-user/security-team-user-listing/security-team-user-listing.component';
import { SecurityTeamUserAddEditComponent } from './security-data-components/security-team-user/security-team-user-add-edit/security-team-user-add-edit.component';
import { SecurityTeamUserDetailComponent } from './security-data-components/security-team-user/security-team-user-detail/security-team-user-detail.component';
import { SecurityTeamUserTableComponent } from './security-data-components/security-team-user/security-team-user-table/security-team-user-table.component';
import { SecurityTenantListingComponent } from './security-data-components/security-tenant/security-tenant-listing/security-tenant-listing.component';
import { SecurityTenantAddEditComponent } from './security-data-components/security-tenant/security-tenant-add-edit/security-tenant-add-edit.component';
import { SecurityTenantDetailComponent } from './security-data-components/security-tenant/security-tenant-detail/security-tenant-detail.component';
import { SecurityTenantTableComponent } from './security-data-components/security-tenant/security-tenant-table/security-tenant-table.component';
import { SecurityTenantUserListingComponent } from './security-data-components/security-tenant-user/security-tenant-user-listing/security-tenant-user-listing.component';
import { SecurityTenantUserAddEditComponent } from './security-data-components/security-tenant-user/security-tenant-user-add-edit/security-tenant-user-add-edit.component';
import { SecurityTenantUserDetailComponent } from './security-data-components/security-tenant-user/security-tenant-user-detail/security-tenant-user-detail.component';
import { SecurityTenantUserTableComponent } from './security-data-components/security-tenant-user/security-tenant-user-table/security-tenant-user-table.component';
import { SecurityUserListingComponent } from './security-data-components/security-user/security-user-listing/security-user-listing.component';
import { SecurityUserAddEditComponent } from './security-data-components/security-user/security-user-add-edit/security-user-add-edit.component';
import { SecurityUserDetailComponent } from './security-data-components/security-user/security-user-detail/security-user-detail.component';
import { SecurityUserTableComponent } from './security-data-components/security-user/security-user-table/security-user-table.component';
import { SecurityUserEventListingComponent } from './security-data-components/security-user-event/security-user-event-listing/security-user-event-listing.component';
import { SecurityUserEventAddEditComponent } from './security-data-components/security-user-event/security-user-event-add-edit/security-user-event-add-edit.component';
import { SecurityUserEventDetailComponent } from './security-data-components/security-user-event/security-user-event-detail/security-user-event-detail.component';
import { SecurityUserEventTableComponent } from './security-data-components/security-user-event/security-user-event-table/security-user-event-table.component';
import { SecurityUserEventTypeListingComponent } from './security-data-components/security-user-event-type/security-user-event-type-listing/security-user-event-type-listing.component';
import { SecurityUserEventTypeAddEditComponent } from './security-data-components/security-user-event-type/security-user-event-type-add-edit/security-user-event-type-add-edit.component';
import { SecurityUserEventTypeDetailComponent } from './security-data-components/security-user-event-type/security-user-event-type-detail/security-user-event-type-detail.component';
import { SecurityUserEventTypeTableComponent } from './security-data-components/security-user-event-type/security-user-event-type-table/security-user-event-type-table.component';
import { SecurityUserPasswordResetTokenListingComponent } from './security-data-components/security-user-password-reset-token/security-user-password-reset-token-listing/security-user-password-reset-token-listing.component';
import { SecurityUserPasswordResetTokenAddEditComponent } from './security-data-components/security-user-password-reset-token/security-user-password-reset-token-add-edit/security-user-password-reset-token-add-edit.component';
import { SecurityUserPasswordResetTokenDetailComponent } from './security-data-components/security-user-password-reset-token/security-user-password-reset-token-detail/security-user-password-reset-token-detail.component';
import { SecurityUserPasswordResetTokenTableComponent } from './security-data-components/security-user-password-reset-token/security-user-password-reset-token-table/security-user-password-reset-token-table.component';
import { SecurityUserSecurityGroupListingComponent } from './security-data-components/security-user-security-group/security-user-security-group-listing/security-user-security-group-listing.component';
import { SecurityUserSecurityGroupAddEditComponent } from './security-data-components/security-user-security-group/security-user-security-group-add-edit/security-user-security-group-add-edit.component';
import { SecurityUserSecurityGroupDetailComponent } from './security-data-components/security-user-security-group/security-user-security-group-detail/security-user-security-group-detail.component';
import { SecurityUserSecurityGroupTableComponent } from './security-data-components/security-user-security-group/security-user-security-group-table/security-user-security-group-table.component';
import { SecurityUserSecurityRoleListingComponent } from './security-data-components/security-user-security-role/security-user-security-role-listing/security-user-security-role-listing.component';
import { SecurityUserSecurityRoleAddEditComponent } from './security-data-components/security-user-security-role/security-user-security-role-add-edit/security-user-security-role-add-edit.component';
import { SecurityUserSecurityRoleDetailComponent } from './security-data-components/security-user-security-role/security-user-security-role-detail/security-user-security-role-detail.component';
import { SecurityUserSecurityRoleTableComponent } from './security-data-components/security-user-security-role/security-user-security-role-table/security-user-security-role-table.component';
import { SecurityUserTitleListingComponent } from './security-data-components/security-user-title/security-user-title-listing/security-user-title-listing.component';
import { SecurityUserTitleAddEditComponent } from './security-data-components/security-user-title/security-user-title-add-edit/security-user-title-add-edit.component';
import { SecurityUserTitleDetailComponent } from './security-data-components/security-user-title/security-user-title-detail/security-user-title-detail.component';
import { SecurityUserTitleTableComponent } from './security-data-components/security-user-title/security-user-title-table/security-user-title-table.component';
import { SystemSettingListingComponent } from './security-data-components/system-setting/system-setting-listing/system-setting-listing.component';
import { SystemSettingAddEditComponent } from './security-data-components/system-setting/system-setting-add-edit/system-setting-add-edit.component';
import { SystemSettingDetailComponent } from './security-data-components/system-setting/system-setting-detail/system-setting-detail.component';
import { SystemSettingTableComponent } from './security-data-components/system-setting/system-setting-table/system-setting-table.component';
//
// End Security Data Components
//




//
// Auditor Data Components - Auto generated
//
import { AuditAccessTypeListingComponent } from './auditor-data-components/audit-access-type/audit-access-type-listing/audit-access-type-listing.component';
import { AuditAccessTypeAddEditComponent } from './auditor-data-components/audit-access-type/audit-access-type-add-edit/audit-access-type-add-edit.component';
import { AuditAccessTypeDetailComponent } from './auditor-data-components/audit-access-type/audit-access-type-detail/audit-access-type-detail.component';
import { AuditAccessTypeTableComponent } from './auditor-data-components/audit-access-type/audit-access-type-table/audit-access-type-table.component';
import { AuditEventListingComponent } from './auditor-data-components/audit-event/audit-event-listing/audit-event-listing.component';
import { AuditEventAddEditComponent } from './auditor-data-components/audit-event/audit-event-add-edit/audit-event-add-edit.component';
import { AuditEventDetailComponent } from './auditor-data-components/audit-event/audit-event-detail/audit-event-detail.component';
import { AuditEventTableComponent } from './auditor-data-components/audit-event/audit-event-table/audit-event-table.component';
import { AuditEventEntityStateListingComponent } from './auditor-data-components/audit-event-entity-state/audit-event-entity-state-listing/audit-event-entity-state-listing.component';
import { AuditEventEntityStateAddEditComponent } from './auditor-data-components/audit-event-entity-state/audit-event-entity-state-add-edit/audit-event-entity-state-add-edit.component';
import { AuditEventEntityStateDetailComponent } from './auditor-data-components/audit-event-entity-state/audit-event-entity-state-detail/audit-event-entity-state-detail.component';
import { AuditEventEntityStateTableComponent } from './auditor-data-components/audit-event-entity-state/audit-event-entity-state-table/audit-event-entity-state-table.component';
import { AuditEventErrorMessageListingComponent } from './auditor-data-components/audit-event-error-message/audit-event-error-message-listing/audit-event-error-message-listing.component';
import { AuditEventErrorMessageAddEditComponent } from './auditor-data-components/audit-event-error-message/audit-event-error-message-add-edit/audit-event-error-message-add-edit.component';
import { AuditEventErrorMessageDetailComponent } from './auditor-data-components/audit-event-error-message/audit-event-error-message-detail/audit-event-error-message-detail.component';
import { AuditEventErrorMessageTableComponent } from './auditor-data-components/audit-event-error-message/audit-event-error-message-table/audit-event-error-message-table.component';
import { AuditHostSystemListingComponent } from './auditor-data-components/audit-host-system/audit-host-system-listing/audit-host-system-listing.component';
import { AuditHostSystemAddEditComponent } from './auditor-data-components/audit-host-system/audit-host-system-add-edit/audit-host-system-add-edit.component';
import { AuditHostSystemDetailComponent } from './auditor-data-components/audit-host-system/audit-host-system-detail/audit-host-system-detail.component';
import { AuditHostSystemTableComponent } from './auditor-data-components/audit-host-system/audit-host-system-table/audit-host-system-table.component';
import { AuditModuleListingComponent } from './auditor-data-components/audit-module/audit-module-listing/audit-module-listing.component';
import { AuditModuleAddEditComponent } from './auditor-data-components/audit-module/audit-module-add-edit/audit-module-add-edit.component';
import { AuditModuleDetailComponent } from './auditor-data-components/audit-module/audit-module-detail/audit-module-detail.component';
import { AuditModuleTableComponent } from './auditor-data-components/audit-module/audit-module-table/audit-module-table.component';
import { AuditModuleEntityListingComponent } from './auditor-data-components/audit-module-entity/audit-module-entity-listing/audit-module-entity-listing.component';
import { AuditModuleEntityAddEditComponent } from './auditor-data-components/audit-module-entity/audit-module-entity-add-edit/audit-module-entity-add-edit.component';
import { AuditModuleEntityDetailComponent } from './auditor-data-components/audit-module-entity/audit-module-entity-detail/audit-module-entity-detail.component';
import { AuditModuleEntityTableComponent } from './auditor-data-components/audit-module-entity/audit-module-entity-table/audit-module-entity-table.component';
import { AuditPlanBListingComponent } from './auditor-data-components/audit-plan-b/audit-plan-b-listing/audit-plan-b-listing.component';
import { AuditPlanBAddEditComponent } from './auditor-data-components/audit-plan-b/audit-plan-b-add-edit/audit-plan-b-add-edit.component';
import { AuditPlanBDetailComponent } from './auditor-data-components/audit-plan-b/audit-plan-b-detail/audit-plan-b-detail.component';
import { AuditPlanBTableComponent } from './auditor-data-components/audit-plan-b/audit-plan-b-table/audit-plan-b-table.component';
import { AuditResourceListingComponent } from './auditor-data-components/audit-resource/audit-resource-listing/audit-resource-listing.component';
import { AuditResourceAddEditComponent } from './auditor-data-components/audit-resource/audit-resource-add-edit/audit-resource-add-edit.component';
import { AuditResourceDetailComponent } from './auditor-data-components/audit-resource/audit-resource-detail/audit-resource-detail.component';
import { AuditResourceTableComponent } from './auditor-data-components/audit-resource/audit-resource-table/audit-resource-table.component';
import { AuditSessionListingComponent } from './auditor-data-components/audit-session/audit-session-listing/audit-session-listing.component';
import { AuditSessionAddEditComponent } from './auditor-data-components/audit-session/audit-session-add-edit/audit-session-add-edit.component';
import { AuditSessionDetailComponent } from './auditor-data-components/audit-session/audit-session-detail/audit-session-detail.component';
import { AuditSessionTableComponent } from './auditor-data-components/audit-session/audit-session-table/audit-session-table.component';
import { AuditSourceListingComponent } from './auditor-data-components/audit-source/audit-source-listing/audit-source-listing.component';
import { AuditSourceAddEditComponent } from './auditor-data-components/audit-source/audit-source-add-edit/audit-source-add-edit.component';
import { AuditSourceDetailComponent } from './auditor-data-components/audit-source/audit-source-detail/audit-source-detail.component';
import { AuditSourceTableComponent } from './auditor-data-components/audit-source/audit-source-table/audit-source-table.component';
import { AuditTypeListingComponent } from './auditor-data-components/audit-type/audit-type-listing/audit-type-listing.component';
import { AuditTypeAddEditComponent } from './auditor-data-components/audit-type/audit-type-add-edit/audit-type-add-edit.component';
import { AuditTypeDetailComponent } from './auditor-data-components/audit-type/audit-type-detail/audit-type-detail.component';
import { AuditTypeTableComponent } from './auditor-data-components/audit-type/audit-type-table/audit-type-table.component';
import { AuditUserListingComponent } from './auditor-data-components/audit-user/audit-user-listing/audit-user-listing.component';
import { AuditUserAddEditComponent } from './auditor-data-components/audit-user/audit-user-add-edit/audit-user-add-edit.component';
import { AuditUserDetailComponent } from './auditor-data-components/audit-user/audit-user-detail/audit-user-detail.component';
import { AuditUserTableComponent } from './auditor-data-components/audit-user/audit-user-table/audit-user-table.component';
import { AuditUserAgentListingComponent } from './auditor-data-components/audit-user-agent/audit-user-agent-listing/audit-user-agent-listing.component';
import { AuditUserAgentAddEditComponent } from './auditor-data-components/audit-user-agent/audit-user-agent-add-edit/audit-user-agent-add-edit.component';
import { AuditUserAgentDetailComponent } from './auditor-data-components/audit-user-agent/audit-user-agent-detail/audit-user-agent-detail.component';
import { AuditUserAgentTableComponent } from './auditor-data-components/audit-user-agent/audit-user-agent-table/audit-user-agent-table.component';
import { ExternalCommunicationListingComponent } from './auditor-data-components/external-communication/external-communication-listing/external-communication-listing.component';
import { ExternalCommunicationAddEditComponent } from './auditor-data-components/external-communication/external-communication-add-edit/external-communication-add-edit.component';
import { ExternalCommunicationDetailComponent } from './auditor-data-components/external-communication/external-communication-detail/external-communication-detail.component';
import { ExternalCommunicationTableComponent } from './auditor-data-components/external-communication/external-communication-table/external-communication-table.component';
import { ExternalCommunicationRecipientListingComponent } from './auditor-data-components/external-communication-recipient/external-communication-recipient-listing/external-communication-recipient-listing.component';
import { ExternalCommunicationRecipientAddEditComponent } from './auditor-data-components/external-communication-recipient/external-communication-recipient-add-edit/external-communication-recipient-add-edit.component';
import { ExternalCommunicationRecipientDetailComponent } from './auditor-data-components/external-communication-recipient/external-communication-recipient-detail/external-communication-recipient-detail.component';
import { ExternalCommunicationRecipientTableComponent } from './auditor-data-components/external-communication-recipient/external-communication-recipient-table/external-communication-recipient-table.component';
//
// End Auditor Data Components
//

//
// Custom screens
//


@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    AuthCallbackComponent,
    NotFoundComponent,
    SearchBoxComponent,
    BooleanIconComponent,

    EqualValidator,
    AutofocusDirective,
    BootstrapTabDirective,
    SpinnerDirective,

    GroupByPipe,
    FilterAndJoinPipe,
    BigNumberFormatPipe,

    OverviewComponent,
    ModalComponent,

    HeaderComponent,
    SidebarComponent,

    //
    // Security Data Components - Auto generated
    //
    EntityDataTokenListingComponent,
    EntityDataTokenAddEditComponent,
    EntityDataTokenDetailComponent,
    EntityDataTokenTableComponent,
    EntityDataTokenEventListingComponent,
    EntityDataTokenEventAddEditComponent,
    EntityDataTokenEventDetailComponent,
    EntityDataTokenEventTableComponent,
    EntityDataTokenEventTypeListingComponent,
    EntityDataTokenEventTypeAddEditComponent,
    EntityDataTokenEventTypeDetailComponent,
    EntityDataTokenEventTypeTableComponent,
    LoginAttemptListingComponent,
    LoginAttemptAddEditComponent,
    LoginAttemptDetailComponent,
    LoginAttemptTableComponent,
    ModuleListingComponent,
    ModuleAddEditComponent,
    ModuleDetailComponent,
    ModuleTableComponent,
    ModuleSecurityRoleListingComponent,
    ModuleSecurityRoleAddEditComponent,
    ModuleSecurityRoleDetailComponent,
    ModuleSecurityRoleTableComponent,
    OAUTHTokenListingComponent,
    OAUTHTokenAddEditComponent,
    OAUTHTokenDetailComponent,
    OAUTHTokenTableComponent,
    PrivilegeListingComponent,
    PrivilegeAddEditComponent,
    PrivilegeDetailComponent,
    PrivilegeTableComponent,
    SecurityDepartmentListingComponent,
    SecurityDepartmentAddEditComponent,
    SecurityDepartmentDetailComponent,
    SecurityDepartmentTableComponent,
    SecurityDepartmentUserListingComponent,
    SecurityDepartmentUserAddEditComponent,
    SecurityDepartmentUserDetailComponent,
    SecurityDepartmentUserTableComponent,
    SecurityGroupListingComponent,
    SecurityGroupAddEditComponent,
    SecurityGroupDetailComponent,
    SecurityGroupTableComponent,
    SecurityGroupSecurityRoleListingComponent,
    SecurityGroupSecurityRoleAddEditComponent,
    SecurityGroupSecurityRoleDetailComponent,
    SecurityGroupSecurityRoleTableComponent,
    SecurityOrganizationListingComponent,
    SecurityOrganizationAddEditComponent,
    SecurityOrganizationDetailComponent,
    SecurityOrganizationTableComponent,
    SecurityOrganizationUserListingComponent,
    SecurityOrganizationUserAddEditComponent,
    SecurityOrganizationUserDetailComponent,
    SecurityOrganizationUserTableComponent,
    SecurityRoleListingComponent,
    SecurityRoleAddEditComponent,
    SecurityRoleDetailComponent,
    SecurityRoleTableComponent,
    SecurityTeamListingComponent,
    SecurityTeamAddEditComponent,
    SecurityTeamDetailComponent,
    SecurityTeamTableComponent,
    SecurityTeamUserListingComponent,
    SecurityTeamUserAddEditComponent,
    SecurityTeamUserDetailComponent,
    SecurityTeamUserTableComponent,
    SecurityTenantListingComponent,
    SecurityTenantAddEditComponent,
    SecurityTenantDetailComponent,
    SecurityTenantTableComponent,
    SecurityTenantUserListingComponent,
    SecurityTenantUserAddEditComponent,
    SecurityTenantUserDetailComponent,
    SecurityTenantUserTableComponent,
    SecurityUserListingComponent,
    SecurityUserAddEditComponent,
    SecurityUserDetailComponent,
    SecurityUserTableComponent,
    SecurityUserEventListingComponent,
    SecurityUserEventAddEditComponent,
    SecurityUserEventDetailComponent,
    SecurityUserEventTableComponent,
    SecurityUserEventTypeListingComponent,
    SecurityUserEventTypeAddEditComponent,
    SecurityUserEventTypeDetailComponent,
    SecurityUserEventTypeTableComponent,
    SecurityUserPasswordResetTokenListingComponent,
    SecurityUserPasswordResetTokenAddEditComponent,
    SecurityUserPasswordResetTokenDetailComponent,
    SecurityUserPasswordResetTokenTableComponent,
    SecurityUserSecurityGroupListingComponent,
    SecurityUserSecurityGroupAddEditComponent,
    SecurityUserSecurityGroupDetailComponent,
    SecurityUserSecurityGroupTableComponent,
    SecurityUserSecurityRoleListingComponent,
    SecurityUserSecurityRoleAddEditComponent,
    SecurityUserSecurityRoleDetailComponent,
    SecurityUserSecurityRoleTableComponent,
    SecurityUserTitleListingComponent,
    SecurityUserTitleAddEditComponent,
    SecurityUserTitleDetailComponent,
    SecurityUserTitleTableComponent,
    SystemSettingListingComponent,
    SystemSettingAddEditComponent,
    SystemSettingDetailComponent,
    SystemSettingTableComponent,
    //
    // End Security Data Components
    //


    //
    // Auditor Data Components - Auto generated
    //
    AuditAccessTypeListingComponent,
    AuditAccessTypeAddEditComponent,
    AuditAccessTypeDetailComponent,
    AuditAccessTypeTableComponent,
    AuditEventListingComponent,
    AuditEventAddEditComponent,
    AuditEventDetailComponent,
    AuditEventTableComponent,
    AuditEventEntityStateListingComponent,
    AuditEventEntityStateAddEditComponent,
    AuditEventEntityStateDetailComponent,
    AuditEventEntityStateTableComponent,
    AuditEventErrorMessageListingComponent,
    AuditEventErrorMessageAddEditComponent,
    AuditEventErrorMessageDetailComponent,
    AuditEventErrorMessageTableComponent,
    AuditHostSystemListingComponent,
    AuditHostSystemAddEditComponent,
    AuditHostSystemDetailComponent,
    AuditHostSystemTableComponent,
    AuditModuleListingComponent,
    AuditModuleAddEditComponent,
    AuditModuleDetailComponent,
    AuditModuleTableComponent,
    AuditModuleEntityListingComponent,
    AuditModuleEntityAddEditComponent,
    AuditModuleEntityDetailComponent,
    AuditModuleEntityTableComponent,
    AuditPlanBListingComponent,
    AuditPlanBAddEditComponent,
    AuditPlanBDetailComponent,
    AuditPlanBTableComponent,
    AuditResourceListingComponent,
    AuditResourceAddEditComponent,
    AuditResourceDetailComponent,
    AuditResourceTableComponent,
    AuditSessionListingComponent,
    AuditSessionAddEditComponent,
    AuditSessionDetailComponent,
    AuditSessionTableComponent,
    AuditSourceListingComponent,
    AuditSourceAddEditComponent,
    AuditSourceDetailComponent,
    AuditSourceTableComponent,
    AuditTypeListingComponent,
    AuditTypeAddEditComponent,
    AuditTypeDetailComponent,
    AuditTypeTableComponent,
    AuditUserListingComponent,
    AuditUserAddEditComponent,
    AuditUserDetailComponent,
    AuditUserTableComponent,
    AuditUserAgentListingComponent,
    AuditUserAgentAddEditComponent,
    AuditUserAgentDetailComponent,
    AuditUserAgentTableComponent,
    ExternalCommunicationListingComponent,
    ExternalCommunicationAddEditComponent,
    ExternalCommunicationDetailComponent,
    ExternalCommunicationTableComponent,
    ExternalCommunicationRecipientListingComponent,
    ExternalCommunicationRecipientAddEditComponent,
    ExternalCommunicationRecipientDetailComponent,
    ExternalCommunicationRecipientTableComponent,
    //
    // End Auditor Data Components
    //



    //
    // Custom components
    //
    ResetPasswordComponent,
    NewUserComponent,
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    ScrollingModule,
    HttpClientModule,
    ZXingScannerModule,
    FormsModule,
    ReactiveFormsModule,
    MatExpansionModule,
    DragDropModule,
    AppRoutingModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useClass: TranslateLanguageLoader
      }
    }),
    NgbTooltipModule,
    NgbPopoverModule,
    NgbDropdownModule,
    NgbCarouselModule,
    NgbModalModule,
    NgbNavModule,
    NgxDatatableModule,
    OAuthModule.forRoot(),
    ToastaModule.forRoot(),
    NgSelectModule,
    NgChartsModule,
    NgbAccordionModule,
  ],
  exports: [SpinnerDirective],
  providers: [
    //{ provide: ErrorHandler, useClass: AppErrorHandler }, this is a wrapper around Javascript errors that just displays a popup, but makes it hard to debug
    { provide: OAuthStorage, useClass: OidcTempStorage },
    { provide: RouteReuseStrategy, useClass: RouteReuseService },
    AlertService,
    ConfigurationService,
    AppTranslationService,
    LocalStoreManager,
    OidcHelperService,

    //
    // Custom services
    //
    NavigationService,
    CurrentUserService,
    ConfirmationService,

    //
    // Security Data Services - Auto generated
    //
    SecurityDataServiceManagerService,
    EntityDataTokenService,
    EntityDataTokenEventService,
    EntityDataTokenEventTypeService,
    LoginAttemptService,
    ModuleService,
    ModuleSecurityRoleService,
    OAUTHTokenService,
    PrivilegeService,
    SecurityDepartmentService,
    SecurityDepartmentUserService,
    SecurityGroupService,
    SecurityGroupSecurityRoleService,
    SecurityOrganizationService,
    SecurityOrganizationUserService,
    SecurityRoleService,
    SecurityTeamService,
    SecurityTeamUserService,
    SecurityTenantService,
    SecurityTenantUserService,
    SecurityUserService,
    SecurityUserEventService,
    SecurityUserEventTypeService,
    SecurityUserPasswordResetTokenService,
    SecurityUserSecurityGroupService,
    SecurityUserSecurityRoleService,
    SecurityUserTitleService,
    SystemSettingService,
    //
    // End Security Data Services
    //


    //
    // Auditor Data Services - Auto Generated
    // 
    AuditorDataServiceManagerService,
    AuditAccessTypeService,
    AuditEventService,
    AuditEventEntityStateService,
    AuditEventErrorMessageService,
    AuditHostSystemService,
    AuditModuleService,
    AuditModuleEntityService,
    AuditPlanBService,
    AuditResourceService,
    AuditSessionService,
    AuditSourceService,
    AuditTypeService,
    AuditUserService,
    AuditUserAgentService,
    ExternalCommunicationService,
    ExternalCommunicationRecipientService,
    //
    // End Auditor Data Services
    // 


    //
    // For animations.
    //
    provideAnimationsAsync()
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
