/*
   GENERATED FORM FOR THE SECURITYROLE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityRole table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-role-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityRoleService, SecurityRoleData, SecurityRoleSubmitData } from '../../../security-data-services/security-role.service';
import { PrivilegeService } from '../../../security-data-services/privilege.service';
import { SecurityUserSecurityRoleService } from '../../../security-data-services/security-user-security-role.service';
import { SecurityGroupSecurityRoleService } from '../../../security-data-services/security-group-security-role.service';
import { ModuleSecurityRoleService } from '../../../security-data-services/module-security-role.service';
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
interface SecurityRoleFormValues {
  privilegeId: number | bigint,       // For FK link number
  name: string,
  description: string | null,
  comments: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-security-role-detail',
  templateUrl: './security-role-detail.component.html',
  styleUrls: ['./security-role-detail.component.scss']
})

export class SecurityRoleDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SecurityRoleFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public securityRoleForm: FormGroup = this.fb.group({
        privilegeId: [null, Validators.required],
        name: ['', Validators.required],
        description: [''],
        comments: [''],
        active: [true],
        deleted: [false],
      });


  public securityRoleId: string | null = null;
  public securityRoleData: SecurityRoleData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  securityRoles$ = this.securityRoleService.GetSecurityRoleList();
  public privileges$ = this.privilegeService.GetPrivilegeList();
  public securityUserSecurityRoles$ = this.securityUserSecurityRoleService.GetSecurityUserSecurityRoleList();
  public securityGroupSecurityRoles$ = this.securityGroupSecurityRoleService.GetSecurityGroupSecurityRoleList();
  public moduleSecurityRoles$ = this.moduleSecurityRoleService.GetModuleSecurityRoleList();

  private destroy$ = new Subject<void>();

  constructor(
    public securityRoleService: SecurityRoleService,
    public privilegeService: PrivilegeService,
    public securityUserSecurityRoleService: SecurityUserSecurityRoleService,
    public securityGroupSecurityRoleService: SecurityGroupSecurityRoleService,
    public moduleSecurityRoleService: ModuleSecurityRoleService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the securityRoleId from the route parameters
    this.securityRoleId = this.route.snapshot.paramMap.get('securityRoleId');

    if (this.securityRoleId === 'new' ||
        this.securityRoleId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.securityRoleData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.securityRoleForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.securityRoleForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Security Role';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Security Role';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.securityRoleForm.dirty) {
      return confirm('You have unsaved Security Role changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.securityRoleId != null && this.securityRoleId !== 'new') {

      const id = parseInt(this.securityRoleId, 10);

      if (!isNaN(id)) {
        return { securityRoleId: id };
      }
    }

    return null;
  }


/*
  * Loads the SecurityRole data for the current securityRoleId.
  *
  * Fully respects the SecurityRoleService caching strategy and error handling strategy.
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
    if (!this.securityRoleService.userIsSecuritySecurityRoleReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SecurityRoles.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate securityRoleId
    //
    if (!this.securityRoleId) {

      this.alertService.showMessage('No SecurityRole ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const securityRoleId = Number(this.securityRoleId);

    if (isNaN(securityRoleId) || securityRoleId <= 0) {

      this.alertService.showMessage(`Invalid Security Role ID: "${this.securityRoleId}"`,
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
      // This is the most targeted way: clear only this SecurityRole + relations

      this.securityRoleService.ClearRecordCache(securityRoleId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.securityRoleService.GetSecurityRole(securityRoleId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (securityRoleData) => {

        //
        // Success path — securityRoleData can legitimately be null if 404'd but request succeeded
        //
        if (!securityRoleData) {

          this.handleSecurityRoleNotFound(securityRoleId);

        } else {

          this.securityRoleData = securityRoleData;
          this.buildFormValues(this.securityRoleData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SecurityRole loaded successfully',
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
        this.handleSecurityRoleLoadError(error, securityRoleId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSecurityRoleNotFound(securityRoleId: number): void {

    this.securityRoleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SecurityRole #${securityRoleId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSecurityRoleLoadError(error: any, securityRoleId: number): void {

    let message = 'Failed to load Security Role.';
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
          message = 'You do not have permission to view this Security Role.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Security Role #${securityRoleId} was not found.`;
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

    console.error(`Security Role load failed (ID: ${securityRoleId})`, error);

    //
    // Reset UI to safe state
    //
    this.securityRoleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(securityRoleData: SecurityRoleData | null) {

    if (securityRoleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityRoleForm.reset({
        privilegeId: null,
        name: '',
        description: '',
        comments: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityRoleForm.reset({
        privilegeId: securityRoleData.privilegeId,
        name: securityRoleData.name ?? '',
        description: securityRoleData.description ?? '',
        comments: securityRoleData.comments ?? '',
        active: securityRoleData.active ?? true,
        deleted: securityRoleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityRoleForm.markAsPristine();
    this.securityRoleForm.markAsUntouched();
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

    if (this.securityRoleService.userIsSecuritySecurityRoleWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Security Roles", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.securityRoleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityRoleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityRoleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityRoleSubmitData: SecurityRoleSubmitData = {
        id: this.securityRoleData?.id || 0,
        privilegeId: Number(formValue.privilegeId),
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        comments: formValue.comments?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.securityRoleService.PutSecurityRole(securityRoleSubmitData.id, securityRoleSubmitData)
      : this.securityRoleService.PostSecurityRole(securityRoleSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSecurityRoleData) => {

        this.securityRoleService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Security Role's detail page
          //
          this.securityRoleForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.securityRoleForm.markAsUntouched();

          this.router.navigate(['/securityroles', savedSecurityRoleData.id]);
          this.alertService.showMessage('Security Role added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.securityRoleData = savedSecurityRoleData;
          this.buildFormValues(this.securityRoleData);

          this.alertService.showMessage("Security Role saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Security Role.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Role.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Role could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecuritySecurityRoleReader(): boolean {
    return this.securityRoleService.userIsSecuritySecurityRoleReader();
  }

  public userIsSecuritySecurityRoleWriter(): boolean {
    return this.securityRoleService.userIsSecuritySecurityRoleWriter();
  }
}
