/*
   GENERATED FORM FOR THE SECURITYGROUPSECURITYROLE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityGroupSecurityRole table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-group-security-role-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityGroupSecurityRoleService, SecurityGroupSecurityRoleData, SecurityGroupSecurityRoleSubmitData } from '../../../security-data-services/security-group-security-role.service';
import { SecurityGroupService } from '../../../security-data-services/security-group.service';
import { SecurityRoleService } from '../../../security-data-services/security-role.service';
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
interface SecurityGroupSecurityRoleFormValues {
  securityGroupId: number | bigint,       // For FK link number
  securityRoleId: number | bigint,       // For FK link number
  comments: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-security-group-security-role-detail',
  templateUrl: './security-group-security-role-detail.component.html',
  styleUrls: ['./security-group-security-role-detail.component.scss']
})

export class SecurityGroupSecurityRoleDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SecurityGroupSecurityRoleFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public securityGroupSecurityRoleForm: FormGroup = this.fb.group({
        securityGroupId: [null, Validators.required],
        securityRoleId: [null, Validators.required],
        comments: [''],
        active: [true],
        deleted: [false],
      });


  public securityGroupSecurityRoleId: string | null = null;
  public securityGroupSecurityRoleData: SecurityGroupSecurityRoleData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  securityGroupSecurityRoles$ = this.securityGroupSecurityRoleService.GetSecurityGroupSecurityRoleList();
  public securityGroups$ = this.securityGroupService.GetSecurityGroupList();
  public securityRoles$ = this.securityRoleService.GetSecurityRoleList();

  private destroy$ = new Subject<void>();

  constructor(
    public securityGroupSecurityRoleService: SecurityGroupSecurityRoleService,
    public securityGroupService: SecurityGroupService,
    public securityRoleService: SecurityRoleService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the securityGroupSecurityRoleId from the route parameters
    this.securityGroupSecurityRoleId = this.route.snapshot.paramMap.get('securityGroupSecurityRoleId');

    if (this.securityGroupSecurityRoleId === 'new' ||
        this.securityGroupSecurityRoleId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.securityGroupSecurityRoleData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.securityGroupSecurityRoleForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.securityGroupSecurityRoleForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Security Group Security Role';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Security Group Security Role';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.securityGroupSecurityRoleForm.dirty) {
      return confirm('You have unsaved Security Group Security Role changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.securityGroupSecurityRoleId != null && this.securityGroupSecurityRoleId !== 'new') {

      const id = parseInt(this.securityGroupSecurityRoleId, 10);

      if (!isNaN(id)) {
        return { securityGroupSecurityRoleId: id };
      }
    }

    return null;
  }


/*
  * Loads the SecurityGroupSecurityRole data for the current securityGroupSecurityRoleId.
  *
  * Fully respects the SecurityGroupSecurityRoleService caching strategy and error handling strategy.
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
    if (!this.securityGroupSecurityRoleService.userIsSecuritySecurityGroupSecurityRoleReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SecurityGroupSecurityRoles.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate securityGroupSecurityRoleId
    //
    if (!this.securityGroupSecurityRoleId) {

      this.alertService.showMessage('No SecurityGroupSecurityRole ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const securityGroupSecurityRoleId = Number(this.securityGroupSecurityRoleId);

    if (isNaN(securityGroupSecurityRoleId) || securityGroupSecurityRoleId <= 0) {

      this.alertService.showMessage(`Invalid Security Group Security Role ID: "${this.securityGroupSecurityRoleId}"`,
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
      // This is the most targeted way: clear only this SecurityGroupSecurityRole + relations

      this.securityGroupSecurityRoleService.ClearRecordCache(securityGroupSecurityRoleId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.securityGroupSecurityRoleService.GetSecurityGroupSecurityRole(securityGroupSecurityRoleId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (securityGroupSecurityRoleData) => {

        //
        // Success path — securityGroupSecurityRoleData can legitimately be null if 404'd but request succeeded
        //
        if (!securityGroupSecurityRoleData) {

          this.handleSecurityGroupSecurityRoleNotFound(securityGroupSecurityRoleId);

        } else {

          this.securityGroupSecurityRoleData = securityGroupSecurityRoleData;
          this.buildFormValues(this.securityGroupSecurityRoleData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SecurityGroupSecurityRole loaded successfully',
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
        this.handleSecurityGroupSecurityRoleLoadError(error, securityGroupSecurityRoleId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSecurityGroupSecurityRoleNotFound(securityGroupSecurityRoleId: number): void {

    this.securityGroupSecurityRoleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SecurityGroupSecurityRole #${securityGroupSecurityRoleId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSecurityGroupSecurityRoleLoadError(error: any, securityGroupSecurityRoleId: number): void {

    let message = 'Failed to load Security Group Security Role.';
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
          message = 'You do not have permission to view this Security Group Security Role.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Security Group Security Role #${securityGroupSecurityRoleId} was not found.`;
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

    console.error(`Security Group Security Role load failed (ID: ${securityGroupSecurityRoleId})`, error);

    //
    // Reset UI to safe state
    //
    this.securityGroupSecurityRoleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(securityGroupSecurityRoleData: SecurityGroupSecurityRoleData | null) {

    if (securityGroupSecurityRoleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityGroupSecurityRoleForm.reset({
        securityGroupId: null,
        securityRoleId: null,
        comments: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityGroupSecurityRoleForm.reset({
        securityGroupId: securityGroupSecurityRoleData.securityGroupId,
        securityRoleId: securityGroupSecurityRoleData.securityRoleId,
        comments: securityGroupSecurityRoleData.comments ?? '',
        active: securityGroupSecurityRoleData.active ?? true,
        deleted: securityGroupSecurityRoleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityGroupSecurityRoleForm.markAsPristine();
    this.securityGroupSecurityRoleForm.markAsUntouched();
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

    if (this.securityGroupSecurityRoleService.userIsSecuritySecurityGroupSecurityRoleWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Security Group Security Roles", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.securityGroupSecurityRoleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityGroupSecurityRoleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityGroupSecurityRoleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityGroupSecurityRoleSubmitData: SecurityGroupSecurityRoleSubmitData = {
        id: this.securityGroupSecurityRoleData?.id || 0,
        securityGroupId: Number(formValue.securityGroupId),
        securityRoleId: Number(formValue.securityRoleId),
        comments: formValue.comments?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.securityGroupSecurityRoleService.PutSecurityGroupSecurityRole(securityGroupSecurityRoleSubmitData.id, securityGroupSecurityRoleSubmitData)
      : this.securityGroupSecurityRoleService.PostSecurityGroupSecurityRole(securityGroupSecurityRoleSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSecurityGroupSecurityRoleData) => {

        this.securityGroupSecurityRoleService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Security Group Security Role's detail page
          //
          this.securityGroupSecurityRoleForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.securityGroupSecurityRoleForm.markAsUntouched();

          this.router.navigate(['/securitygroupsecurityroles', savedSecurityGroupSecurityRoleData.id]);
          this.alertService.showMessage('Security Group Security Role added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.securityGroupSecurityRoleData = savedSecurityGroupSecurityRoleData;
          this.buildFormValues(this.securityGroupSecurityRoleData);

          this.alertService.showMessage("Security Group Security Role saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Security Group Security Role.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Group Security Role.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Group Security Role could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecuritySecurityGroupSecurityRoleReader(): boolean {
    return this.securityGroupSecurityRoleService.userIsSecuritySecurityGroupSecurityRoleReader();
  }

  public userIsSecuritySecurityGroupSecurityRoleWriter(): boolean {
    return this.securityGroupSecurityRoleService.userIsSecuritySecurityGroupSecurityRoleWriter();
  }
}
