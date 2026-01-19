/*
   GENERATED FORM FOR THE SECURITYTENANT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityTenant table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-tenant-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityTenantService, SecurityTenantData, SecurityTenantSubmitData } from '../../../security-data-services/security-tenant.service';
import { SecurityOrganizationService } from '../../../security-data-services/security-organization.service';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { SecurityTenantUserService } from '../../../security-data-services/security-tenant-user.service';
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
interface SecurityTenantFormValues {
  name: string,
  description: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-security-tenant-detail',
  templateUrl: './security-tenant-detail.component.html',
  styleUrls: ['./security-tenant-detail.component.scss']
})

export class SecurityTenantDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SecurityTenantFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public securityTenantForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        active: [true],
        deleted: [false],
      });


  public securityTenantId: string | null = null;
  public securityTenantData: SecurityTenantData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  securityTenants$ = this.securityTenantService.GetSecurityTenantList();
  public securityOrganizations$ = this.securityOrganizationService.GetSecurityOrganizationList();
  public securityUsers$ = this.securityUserService.GetSecurityUserList();
  public securityTenantUsers$ = this.securityTenantUserService.GetSecurityTenantUserList();

  private destroy$ = new Subject<void>();

  constructor(
    public securityTenantService: SecurityTenantService,
    public securityOrganizationService: SecurityOrganizationService,
    public securityUserService: SecurityUserService,
    public securityTenantUserService: SecurityTenantUserService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the securityTenantId from the route parameters
    this.securityTenantId = this.route.snapshot.paramMap.get('securityTenantId');

    if (this.securityTenantId === 'new' ||
        this.securityTenantId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.securityTenantData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.securityTenantForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.securityTenantForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Security Tenant';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Security Tenant';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.securityTenantForm.dirty) {
      return confirm('You have unsaved Security Tenant changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.securityTenantId != null && this.securityTenantId !== 'new') {

      const id = parseInt(this.securityTenantId, 10);

      if (!isNaN(id)) {
        return { securityTenantId: id };
      }
    }

    return null;
  }


/*
  * Loads the SecurityTenant data for the current securityTenantId.
  *
  * Fully respects the SecurityTenantService caching strategy and error handling strategy.
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
    if (!this.securityTenantService.userIsSecuritySecurityTenantReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SecurityTenants.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate securityTenantId
    //
    if (!this.securityTenantId) {

      this.alertService.showMessage('No SecurityTenant ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const securityTenantId = Number(this.securityTenantId);

    if (isNaN(securityTenantId) || securityTenantId <= 0) {

      this.alertService.showMessage(`Invalid Security Tenant ID: "${this.securityTenantId}"`,
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
      // This is the most targeted way: clear only this SecurityTenant + relations

      this.securityTenantService.ClearRecordCache(securityTenantId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.securityTenantService.GetSecurityTenant(securityTenantId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (securityTenantData) => {

        //
        // Success path — securityTenantData can legitimately be null if 404'd but request succeeded
        //
        if (!securityTenantData) {

          this.handleSecurityTenantNotFound(securityTenantId);

        } else {

          this.securityTenantData = securityTenantData;
          this.buildFormValues(this.securityTenantData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SecurityTenant loaded successfully',
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
        this.handleSecurityTenantLoadError(error, securityTenantId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSecurityTenantNotFound(securityTenantId: number): void {

    this.securityTenantData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SecurityTenant #${securityTenantId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSecurityTenantLoadError(error: any, securityTenantId: number): void {

    let message = 'Failed to load Security Tenant.';
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
          message = 'You do not have permission to view this Security Tenant.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Security Tenant #${securityTenantId} was not found.`;
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

    console.error(`Security Tenant load failed (ID: ${securityTenantId})`, error);

    //
    // Reset UI to safe state
    //
    this.securityTenantData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(securityTenantData: SecurityTenantData | null) {

    if (securityTenantData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityTenantForm.reset({
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
        this.securityTenantForm.reset({
        name: securityTenantData.name ?? '',
        description: securityTenantData.description ?? '',
        active: securityTenantData.active ?? true,
        deleted: securityTenantData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityTenantForm.markAsPristine();
    this.securityTenantForm.markAsUntouched();
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

    if (this.securityTenantService.userIsSecuritySecurityTenantWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Security Tenants", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.securityTenantForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityTenantForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityTenantForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityTenantSubmitData: SecurityTenantSubmitData = {
        id: this.securityTenantData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.securityTenantService.PutSecurityTenant(securityTenantSubmitData.id, securityTenantSubmitData)
      : this.securityTenantService.PostSecurityTenant(securityTenantSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSecurityTenantData) => {

        this.securityTenantService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Security Tenant's detail page
          //
          this.securityTenantForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.securityTenantForm.markAsUntouched();

          this.router.navigate(['/securitytenants', savedSecurityTenantData.id]);
          this.alertService.showMessage('Security Tenant added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.securityTenantData = savedSecurityTenantData;
          this.buildFormValues(this.securityTenantData);

          this.alertService.showMessage("Security Tenant saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Security Tenant.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Tenant.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Tenant could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecuritySecurityTenantReader(): boolean {
    return this.securityTenantService.userIsSecuritySecurityTenantReader();
  }

  public userIsSecuritySecurityTenantWriter(): boolean {
    return this.securityTenantService.userIsSecuritySecurityTenantWriter();
  }
}
