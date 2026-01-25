/*
   GENERATED FORM FOR THE SECURITYUSER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityUser table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-user-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityUserService, SecurityUserData, SecurityUserSubmitData } from '../../../security-data-services/security-user.service';
import { SecurityUserTitleService } from '../../../security-data-services/security-user-title.service';
import { SecurityTenantService } from '../../../security-data-services/security-tenant.service';
import { SecurityOrganizationService } from '../../../security-data-services/security-organization.service';
import { SecurityDepartmentService } from '../../../security-data-services/security-department.service';
import { SecurityTeamService } from '../../../security-data-services/security-team.service';
import { SecurityTenantUserService } from '../../../security-data-services/security-tenant-user.service';
import { SecurityOrganizationUserService } from '../../../security-data-services/security-organization-user.service';
import { SecurityDepartmentUserService } from '../../../security-data-services/security-department-user.service';
import { SecurityTeamUserService } from '../../../security-data-services/security-team-user.service';
import { SecurityUserEventService } from '../../../security-data-services/security-user-event.service';
import { SecurityUserPasswordResetTokenService } from '../../../security-data-services/security-user-password-reset-token.service';
import { SecurityUserSecurityGroupService } from '../../../security-data-services/security-user-security-group.service';
import { SecurityUserSecurityRoleService } from '../../../security-data-services/security-user-security-role.service';
import { EntityDataTokenService } from '../../../security-data-services/entity-data-token.service';
import { UserSessionService } from '../../../security-data-services/user-session.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SecurityUserFormValues {
  accountName: string,
  activeDirectoryAccount: boolean,
  canLogin: boolean,
  mustChangePassword: boolean,
  firstName: string | null,
  middleName: string | null,
  lastName: string | null,
  dateOfBirth: string | null,
  emailAddress: string | null,
  cellPhoneNumber: string | null,
  phoneNumber: string | null,
  phoneExtension: string | null,
  description: string | null,
  securityUserTitleId: number | bigint | null,       // For FK link number
  reportsToSecurityUserId: number | bigint | null,       // For FK link number
  authenticationDomain: string | null,
  failedLoginCount: string | null,     // Stored as string for form input, converted to number on submit.
  lastLoginAttempt: string | null,
  mostRecentActivity: string | null,
  alternateIdentifier: string | null,
  image: string | null,
  settings: string | null,
  securityTenantId: number | bigint | null,       // For FK link number
  readPermissionLevel: string,     // Stored as string for form input, converted to number on submit.
  writePermissionLevel: string,     // Stored as string for form input, converted to number on submit.
  securityOrganizationId: number | bigint | null,       // For FK link number
  securityDepartmentId: number | bigint | null,       // For FK link number
  securityTeamId: number | bigint | null,       // For FK link number
  authenticationToken: string | null,
  authenticationTokenExpiry: string | null,
  twoFactorToken: string | null,
  twoFactorTokenExpiry: string | null,
  twoFactorSendByEmail: boolean | null,
  twoFactorSendBySMS: boolean | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-security-user-detail',
  templateUrl: './security-user-detail.component.html',
  styleUrls: ['./security-user-detail.component.scss']
})

export class SecurityUserDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SecurityUserFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public securityUserForm: FormGroup = this.fb.group({
        accountName: ['', Validators.required],
        activeDirectoryAccount: [false],
        canLogin: [false],
        mustChangePassword: [false],
        firstName: [''],
        middleName: [''],
        lastName: [''],
        dateOfBirth: [''],
        emailAddress: [''],
        cellPhoneNumber: [''],
        phoneNumber: [''],
        phoneExtension: [''],
        description: [''],
        securityUserTitleId: [null],
        reportsToSecurityUserId: [null],
        authenticationDomain: [''],
        failedLoginCount: [''],
        lastLoginAttempt: [''],
        mostRecentActivity: [''],
        alternateIdentifier: [''],
        image: [''],
        settings: [''],
        securityTenantId: [null],
        readPermissionLevel: ['', Validators.required],
        writePermissionLevel: ['', Validators.required],
        securityOrganizationId: [null],
        securityDepartmentId: [null],
        securityTeamId: [null],
        authenticationToken: [''],
        authenticationTokenExpiry: [''],
        twoFactorToken: [''],
        twoFactorTokenExpiry: [''],
        twoFactorSendByEmail: [false],
        twoFactorSendBySMS: [false],
        active: [true],
        deleted: [false],
      });


  public securityUserId: string | null = null;
  public securityUserData: SecurityUserData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  securityUsers$ = this.securityUserService.GetSecurityUserList();
  public securityUserTitles$ = this.securityUserTitleService.GetSecurityUserTitleList();
  public securityTenants$ = this.securityTenantService.GetSecurityTenantList();
  public securityOrganizations$ = this.securityOrganizationService.GetSecurityOrganizationList();
  public securityDepartments$ = this.securityDepartmentService.GetSecurityDepartmentList();
  public securityTeams$ = this.securityTeamService.GetSecurityTeamList();
  public securityTenantUsers$ = this.securityTenantUserService.GetSecurityTenantUserList();
  public securityOrganizationUsers$ = this.securityOrganizationUserService.GetSecurityOrganizationUserList();
  public securityDepartmentUsers$ = this.securityDepartmentUserService.GetSecurityDepartmentUserList();
  public securityTeamUsers$ = this.securityTeamUserService.GetSecurityTeamUserList();
  public securityUserEvents$ = this.securityUserEventService.GetSecurityUserEventList();
  public securityUserPasswordResetTokens$ = this.securityUserPasswordResetTokenService.GetSecurityUserPasswordResetTokenList();
  public securityUserSecurityGroups$ = this.securityUserSecurityGroupService.GetSecurityUserSecurityGroupList();
  public securityUserSecurityRoles$ = this.securityUserSecurityRoleService.GetSecurityUserSecurityRoleList();
  public entityDataTokens$ = this.entityDataTokenService.GetEntityDataTokenList();
  public userSessions$ = this.userSessionService.GetUserSessionList();

  private destroy$ = new Subject<void>();

  constructor(
    public securityUserService: SecurityUserService,
    public securityUserTitleService: SecurityUserTitleService,
    public securityTenantService: SecurityTenantService,
    public securityOrganizationService: SecurityOrganizationService,
    public securityDepartmentService: SecurityDepartmentService,
    public securityTeamService: SecurityTeamService,
    public securityTenantUserService: SecurityTenantUserService,
    public securityOrganizationUserService: SecurityOrganizationUserService,
    public securityDepartmentUserService: SecurityDepartmentUserService,
    public securityTeamUserService: SecurityTeamUserService,
    public securityUserEventService: SecurityUserEventService,
    public securityUserPasswordResetTokenService: SecurityUserPasswordResetTokenService,
    public securityUserSecurityGroupService: SecurityUserSecurityGroupService,
    public securityUserSecurityRoleService: SecurityUserSecurityRoleService,
    public entityDataTokenService: EntityDataTokenService,
    public userSessionService: UserSessionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the securityUserId from the route parameters
    this.securityUserId = this.route.snapshot.paramMap.get('securityUserId');

    if (this.securityUserId === 'new' ||
        this.securityUserId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.securityUserData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.securityUserForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.securityUserForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Security User';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Security User';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.securityUserForm.dirty) {
      return confirm('You have unsaved Security User changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.securityUserId != null && this.securityUserId !== 'new') {

      const id = parseInt(this.securityUserId, 10);

      if (!isNaN(id)) {
        return { securityUserId: id };
      }
    }

    return null;
  }


/*
  * Loads the SecurityUser data for the current securityUserId.
  *
  * Fully respects the SecurityUserService caching strategy and error handling strategy.
  *
  * @param forceLoadAndDisplaySuccessAlert
  *   - true  will bypass cache entirely and show success alert message
  *   - false/null will use cache if available, no alert message
  */
  public loadData(forceLoadAndDisplaySuccessAlert: boolean | null = null): void {

    //
    // Start loading indicator immediately
    //
    this.isLoadingSubject.next(true);


    //
    // Permission Check
    //
    if (!this.securityUserService.userIsSecuritySecurityUserReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SecurityUsers.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate securityUserId
    //
    if (!this.securityUserId) {

      this.alertService.showMessage('No SecurityUser ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const securityUserId = Number(this.securityUserId);

    if (isNaN(securityUserId) || securityUserId <= 0) {

      this.alertService.showMessage(`Invalid Security User ID: "${this.securityUserId}"`,
                                    'Invalid ID',
                                    MessageSeverity.error
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Force refresh: clear specific record cache only
    //
    if (forceLoadAndDisplaySuccessAlert === true) {
      // This is the most targeted way: clear only this SecurityUser + relations

      this.securityUserService.ClearRecordCache(securityUserId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.securityUserService.GetSecurityUser(securityUserId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (securityUserData) => {

        //
        // Success path — securityUserData can legitimately be null if 404'd but request succeeded
        //
        if (!securityUserData) {

          this.handleSecurityUserNotFound(securityUserId);

        } else {

          this.securityUserData = securityUserData;
          this.buildFormValues(this.securityUserData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SecurityUser loaded successfully',
              '',
              MessageSeverity.success
            );
          }
        }

        this.isLoadingSubject.next(false);
      },

      error: (error: any) => {
        //
        // All HTTP/network/parsing errors flow here
        // The service already stripped sensitive info and re-threw cleanly
        //
        this.handleSecurityUserLoadError(error, securityUserId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSecurityUserNotFound(securityUserId: number): void {

    this.securityUserData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SecurityUser #${securityUserId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSecurityUserLoadError(error: any, securityUserId: number): void {

    let message = 'Failed to load Security User.';
    let title = 'Load Error';
    let severity = MessageSeverity.error;

    //
    // Leverage HTTP status if available
    //
    if (error?.status) {
      switch (error.status) {
        case 401:
          message = 'Your session has expired. Please log in again.';
          title = 'Unauthorized';
          break;
        case 403:
          message = 'You do not have permission to view this Security User.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Security User #${securityUserId} was not found.`;
          title = 'Not Found';
          severity = MessageSeverity.warn;
          break;
        case 500:
          message = 'Server error. Please try again or contact support.';
          title = 'Server Error';
          break;
        case 0:
          message = 'Cannot reach server. Check your internet connection.';
          title = 'Offline';
          break;
        default:
          message = `Server error ${error.status || 'unknown'}: ${error.statusText || 'Request failed'}`;
      }
    } else {
      message = error?.message || message;
    }

    console.error(`Security User load failed (ID: ${securityUserId})`, error);

    //
    // Reset UI to safe state
    //
    this.securityUserData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(securityUserData: SecurityUserData | null) {

    if (securityUserData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityUserForm.reset({
        accountName: '',
        activeDirectoryAccount: false,
        canLogin: false,
        mustChangePassword: false,
        firstName: '',
        middleName: '',
        lastName: '',
        dateOfBirth: '',
        emailAddress: '',
        cellPhoneNumber: '',
        phoneNumber: '',
        phoneExtension: '',
        description: '',
        securityUserTitleId: null,
        reportsToSecurityUserId: null,
        authenticationDomain: '',
        failedLoginCount: '',
        lastLoginAttempt: '',
        mostRecentActivity: '',
        alternateIdentifier: '',
        image: '',
        settings: '',
        securityTenantId: null,
        readPermissionLevel: '',
        writePermissionLevel: '',
        securityOrganizationId: null,
        securityDepartmentId: null,
        securityTeamId: null,
        authenticationToken: '',
        authenticationTokenExpiry: '',
        twoFactorToken: '',
        twoFactorTokenExpiry: '',
        twoFactorSendByEmail: false,
        twoFactorSendBySMS: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityUserForm.reset({
        accountName: securityUserData.accountName ?? '',
        activeDirectoryAccount: securityUserData.activeDirectoryAccount ?? false,
        canLogin: securityUserData.canLogin ?? false,
        mustChangePassword: securityUserData.mustChangePassword ?? false,
        firstName: securityUserData.firstName ?? '',
        middleName: securityUserData.middleName ?? '',
        lastName: securityUserData.lastName ?? '',
        dateOfBirth: isoUtcStringToDateTimeLocal(securityUserData.dateOfBirth) ?? '',
        emailAddress: securityUserData.emailAddress ?? '',
        cellPhoneNumber: securityUserData.cellPhoneNumber ?? '',
        phoneNumber: securityUserData.phoneNumber ?? '',
        phoneExtension: securityUserData.phoneExtension ?? '',
        description: securityUserData.description ?? '',
        securityUserTitleId: securityUserData.securityUserTitleId,
        reportsToSecurityUserId: securityUserData.reportsToSecurityUserId,
        authenticationDomain: securityUserData.authenticationDomain ?? '',
        failedLoginCount: securityUserData.failedLoginCount?.toString() ?? '',
        lastLoginAttempt: isoUtcStringToDateTimeLocal(securityUserData.lastLoginAttempt) ?? '',
        mostRecentActivity: isoUtcStringToDateTimeLocal(securityUserData.mostRecentActivity) ?? '',
        alternateIdentifier: securityUserData.alternateIdentifier ?? '',
        image: securityUserData.image ?? '',
        settings: securityUserData.settings ?? '',
        securityTenantId: securityUserData.securityTenantId,
        readPermissionLevel: securityUserData.readPermissionLevel?.toString() ?? '',
        writePermissionLevel: securityUserData.writePermissionLevel?.toString() ?? '',
        securityOrganizationId: securityUserData.securityOrganizationId,
        securityDepartmentId: securityUserData.securityDepartmentId,
        securityTeamId: securityUserData.securityTeamId,
        authenticationToken: securityUserData.authenticationToken ?? '',
        authenticationTokenExpiry: isoUtcStringToDateTimeLocal(securityUserData.authenticationTokenExpiry) ?? '',
        twoFactorToken: securityUserData.twoFactorToken ?? '',
        twoFactorTokenExpiry: isoUtcStringToDateTimeLocal(securityUserData.twoFactorTokenExpiry) ?? '',
        twoFactorSendByEmail: securityUserData.twoFactorSendByEmail ?? false,
        twoFactorSendBySMS: securityUserData.twoFactorSendBySMS ?? false,
        active: securityUserData.active ?? true,
        deleted: securityUserData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityUserForm.markAsPristine();
    this.securityUserForm.markAsUntouched();
  }

  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  //
  // Helper method to determine if a field should be hidden based on the hiddenFields input.
  // Returns true if the field is in the array, false otherwise.
  //
  public isFieldHidden(fieldName: string): boolean {
    // Explicit check for array existence to avoid runtime errors.
    if (this.hiddenFields === null || this.hiddenFields === undefined) {
      return false;
    }
    // Use traditional includes method for clarity.
    return this.hiddenFields.includes(fieldName);
  }


  public submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.securityUserService.userIsSecuritySecurityUserWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Security Users", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.securityUserForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityUserForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityUserForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityUserSubmitData: SecurityUserSubmitData = {
        id: this.securityUserData?.id || 0,
        accountName: formValue.accountName!.trim(),
        activeDirectoryAccount: !!formValue.activeDirectoryAccount,
        canLogin: !!formValue.canLogin,
        mustChangePassword: !!formValue.mustChangePassword,
        firstName: formValue.firstName?.trim() || null,
        middleName: formValue.middleName?.trim() || null,
        lastName: formValue.lastName?.trim() || null,
        dateOfBirth: formValue.dateOfBirth ? dateTimeLocalToIsoUtc(formValue.dateOfBirth.trim()) : null,
        emailAddress: formValue.emailAddress?.trim() || null,
        cellPhoneNumber: formValue.cellPhoneNumber?.trim() || null,
        phoneNumber: formValue.phoneNumber?.trim() || null,
        phoneExtension: formValue.phoneExtension?.trim() || null,
        description: formValue.description?.trim() || null,
        securityUserTitleId: formValue.securityUserTitleId ? Number(formValue.securityUserTitleId) : null,
        reportsToSecurityUserId: formValue.reportsToSecurityUserId ? Number(formValue.reportsToSecurityUserId) : null,
        authenticationDomain: formValue.authenticationDomain?.trim() || null,
        failedLoginCount: formValue.failedLoginCount ? Number(formValue.failedLoginCount) : null,
        lastLoginAttempt: formValue.lastLoginAttempt ? dateTimeLocalToIsoUtc(formValue.lastLoginAttempt.trim()) : null,
        mostRecentActivity: formValue.mostRecentActivity ? dateTimeLocalToIsoUtc(formValue.mostRecentActivity.trim()) : null,
        alternateIdentifier: formValue.alternateIdentifier?.trim() || null,
        image: formValue.image?.trim() || null,
        settings: formValue.settings?.trim() || null,
        securityTenantId: formValue.securityTenantId ? Number(formValue.securityTenantId) : null,
        readPermissionLevel: Number(formValue.readPermissionLevel),
        writePermissionLevel: Number(formValue.writePermissionLevel),
        securityOrganizationId: formValue.securityOrganizationId ? Number(formValue.securityOrganizationId) : null,
        securityDepartmentId: formValue.securityDepartmentId ? Number(formValue.securityDepartmentId) : null,
        securityTeamId: formValue.securityTeamId ? Number(formValue.securityTeamId) : null,
        authenticationToken: formValue.authenticationToken?.trim() || null,
        authenticationTokenExpiry: formValue.authenticationTokenExpiry ? dateTimeLocalToIsoUtc(formValue.authenticationTokenExpiry.trim()) : null,
        twoFactorToken: formValue.twoFactorToken?.trim() || null,
        twoFactorTokenExpiry: formValue.twoFactorTokenExpiry ? dateTimeLocalToIsoUtc(formValue.twoFactorTokenExpiry.trim()) : null,
        twoFactorSendByEmail: formValue.twoFactorSendByEmail == true ? true : formValue.twoFactorSendByEmail == false ? false : null,
        twoFactorSendBySMS: formValue.twoFactorSendBySMS == true ? true : formValue.twoFactorSendBySMS == false ? false : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.securityUserService.PutSecurityUser(securityUserSubmitData.id, securityUserSubmitData)
      : this.securityUserService.PostSecurityUser(securityUserSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSecurityUserData) => {

        this.securityUserService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Security User's detail page
          //
          this.securityUserForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.securityUserForm.markAsUntouched();

          this.router.navigate(['/securityusers', savedSecurityUserData.id]);
          this.alertService.showMessage('Security User added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.securityUserData = savedSecurityUserData;
          this.buildFormValues(this.securityUserData);

          this.alertService.showMessage("Security User saved successfully", '', MessageSeverity.success);
        }
      },
      error: (err) => {

            let errorMessage: string;

            // Check if err is an Error object (e.g., new Error('message'))
            if (err instanceof Error) {
                errorMessage = err.message || 'An unexpected error occurred.';
            }
            // Check if err is a ServerError object with status and error properties
            else if (err.status && err.error)
            {
                if (err.status === 403)
                {
                    errorMessage = err.error?.message ||
                                   'You do not have permission to save this Security User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecuritySecurityUserReader(): boolean {
    return this.securityUserService.userIsSecuritySecurityUserReader();
  }

  public userIsSecuritySecurityUserWriter(): boolean {
    return this.securityUserService.userIsSecuritySecurityUserWriter();
  }
}
