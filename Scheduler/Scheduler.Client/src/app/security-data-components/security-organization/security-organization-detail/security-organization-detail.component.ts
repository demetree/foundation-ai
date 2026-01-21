/*
   GENERATED FORM FOR THE SECURITYORGANIZATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityOrganization table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-organization-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityOrganizationService, SecurityOrganizationData, SecurityOrganizationSubmitData } from '../../../security-data-services/security-organization.service';
import { SecurityTenantService } from '../../../security-data-services/security-tenant.service';
import { SecurityDepartmentService } from '../../../security-data-services/security-department.service';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { SecurityOrganizationUserService } from '../../../security-data-services/security-organization-user.service';
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
interface SecurityOrganizationFormValues {
  securityTenantId: number | bigint,       // For FK link number
  name: string,
  description: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-security-organization-detail',
  templateUrl: './security-organization-detail.component.html',
  styleUrls: ['./security-organization-detail.component.scss']
})

export class SecurityOrganizationDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SecurityOrganizationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public securityOrganizationForm: FormGroup = this.fb.group({
        securityTenantId: [null, Validators.required],
        name: ['', Validators.required],
        description: [''],
        active: [true],
        deleted: [false],
      });


  public securityOrganizationId: string | null = null;
  public securityOrganizationData: SecurityOrganizationData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  securityOrganizations$ = this.securityOrganizationService.GetSecurityOrganizationList();
  public securityTenants$ = this.securityTenantService.GetSecurityTenantList();
  public securityDepartments$ = this.securityDepartmentService.GetSecurityDepartmentList();
  public securityUsers$ = this.securityUserService.GetSecurityUserList();
  public securityOrganizationUsers$ = this.securityOrganizationUserService.GetSecurityOrganizationUserList();

  private destroy$ = new Subject<void>();

  constructor(
    public securityOrganizationService: SecurityOrganizationService,
    public securityTenantService: SecurityTenantService,
    public securityDepartmentService: SecurityDepartmentService,
    public securityUserService: SecurityUserService,
    public securityOrganizationUserService: SecurityOrganizationUserService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the securityOrganizationId from the route parameters
    this.securityOrganizationId = this.route.snapshot.paramMap.get('securityOrganizationId');

    if (this.securityOrganizationId === 'new' ||
        this.securityOrganizationId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.securityOrganizationData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.securityOrganizationForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.securityOrganizationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Security Organization';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Security Organization';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.securityOrganizationForm.dirty) {
      return confirm('You have unsaved Security Organization changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.securityOrganizationId != null && this.securityOrganizationId !== 'new') {

      const id = parseInt(this.securityOrganizationId, 10);

      if (!isNaN(id)) {
        return { securityOrganizationId: id };
      }
    }

    return null;
  }


/*
  * Loads the SecurityOrganization data for the current securityOrganizationId.
  *
  * Fully respects the SecurityOrganizationService caching strategy and error handling strategy.
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
    if (!this.securityOrganizationService.userIsSecuritySecurityOrganizationReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SecurityOrganizations.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate securityOrganizationId
    //
    if (!this.securityOrganizationId) {

      this.alertService.showMessage('No SecurityOrganization ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const securityOrganizationId = Number(this.securityOrganizationId);

    if (isNaN(securityOrganizationId) || securityOrganizationId <= 0) {

      this.alertService.showMessage(`Invalid Security Organization ID: "${this.securityOrganizationId}"`,
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
      // This is the most targeted way: clear only this SecurityOrganization + relations

      this.securityOrganizationService.ClearRecordCache(securityOrganizationId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.securityOrganizationService.GetSecurityOrganization(securityOrganizationId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (securityOrganizationData) => {

        //
        // Success path — securityOrganizationData can legitimately be null if 404'd but request succeeded
        //
        if (!securityOrganizationData) {

          this.handleSecurityOrganizationNotFound(securityOrganizationId);

        } else {

          this.securityOrganizationData = securityOrganizationData;
          this.buildFormValues(this.securityOrganizationData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SecurityOrganization loaded successfully',
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
        this.handleSecurityOrganizationLoadError(error, securityOrganizationId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSecurityOrganizationNotFound(securityOrganizationId: number): void {

    this.securityOrganizationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SecurityOrganization #${securityOrganizationId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSecurityOrganizationLoadError(error: any, securityOrganizationId: number): void {

    let message = 'Failed to load Security Organization.';
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
          message = 'You do not have permission to view this Security Organization.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Security Organization #${securityOrganizationId} was not found.`;
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

    console.error(`Security Organization load failed (ID: ${securityOrganizationId})`, error);

    //
    // Reset UI to safe state
    //
    this.securityOrganizationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(securityOrganizationData: SecurityOrganizationData | null) {

    if (securityOrganizationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityOrganizationForm.reset({
        securityTenantId: null,
        name: '',
        description: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityOrganizationForm.reset({
        securityTenantId: securityOrganizationData.securityTenantId,
        name: securityOrganizationData.name ?? '',
        description: securityOrganizationData.description ?? '',
        active: securityOrganizationData.active ?? true,
        deleted: securityOrganizationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityOrganizationForm.markAsPristine();
    this.securityOrganizationForm.markAsUntouched();
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

    if (this.securityOrganizationService.userIsSecuritySecurityOrganizationWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Security Organizations", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.securityOrganizationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityOrganizationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityOrganizationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityOrganizationSubmitData: SecurityOrganizationSubmitData = {
        id: this.securityOrganizationData?.id || 0,
        securityTenantId: Number(formValue.securityTenantId),
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.securityOrganizationService.PutSecurityOrganization(securityOrganizationSubmitData.id, securityOrganizationSubmitData)
      : this.securityOrganizationService.PostSecurityOrganization(securityOrganizationSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSecurityOrganizationData) => {

        this.securityOrganizationService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Security Organization's detail page
          //
          this.securityOrganizationForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.securityOrganizationForm.markAsUntouched();

          this.router.navigate(['/securityorganizations', savedSecurityOrganizationData.id]);
          this.alertService.showMessage('Security Organization added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.securityOrganizationData = savedSecurityOrganizationData;
          this.buildFormValues(this.securityOrganizationData);

          this.alertService.showMessage("Security Organization saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Security Organization.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Organization.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Organization could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecuritySecurityOrganizationReader(): boolean {
    return this.securityOrganizationService.userIsSecuritySecurityOrganizationReader();
  }

  public userIsSecuritySecurityOrganizationWriter(): boolean {
    return this.securityOrganizationService.userIsSecuritySecurityOrganizationWriter();
  }
}
