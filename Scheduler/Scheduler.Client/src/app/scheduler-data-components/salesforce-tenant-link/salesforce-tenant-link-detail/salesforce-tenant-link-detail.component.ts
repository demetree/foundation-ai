/*
   GENERATED FORM FOR THE SALESFORCETENANTLINK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SalesforceTenantLink table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to salesforce-tenant-link-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SalesforceTenantLinkService, SalesforceTenantLinkData, SalesforceTenantLinkSubmitData } from '../../../scheduler-data-services/salesforce-tenant-link.service';
import { SalesforceTenantLinkChangeHistoryService } from '../../../scheduler-data-services/salesforce-tenant-link-change-history.service';
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
interface SalesforceTenantLinkFormValues {
  syncEnabled: boolean,
  syncDirectionFlags: string | null,
  pullIntervalMinutes: string | null,     // Stored as string for form input, converted to number on submit.
  lastPullDate: string | null,
  loginUrl: string | null,
  sfClientId: string | null,
  sfClientSecret: string | null,
  sfUsername: string | null,
  sfPassword: string | null,
  sfSecurityToken: string | null,
  instanceUrl: string | null,
  apiVersion: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-salesforce-tenant-link-detail',
  templateUrl: './salesforce-tenant-link-detail.component.html',
  styleUrls: ['./salesforce-tenant-link-detail.component.scss']
})

export class SalesforceTenantLinkDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SalesforceTenantLinkFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public salesforceTenantLinkForm: FormGroup = this.fb.group({
        syncEnabled: [false],
        syncDirectionFlags: [''],
        pullIntervalMinutes: [''],
        lastPullDate: [''],
        loginUrl: [''],
        sfClientId: [''],
        sfClientSecret: [''],
        sfUsername: [''],
        sfPassword: [''],
        sfSecurityToken: [''],
        instanceUrl: [''],
        apiVersion: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public salesforceTenantLinkId: string | null = null;
  public salesforceTenantLinkData: SalesforceTenantLinkData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  salesforceTenantLinks$ = this.salesforceTenantLinkService.GetSalesforceTenantLinkList();
  public salesforceTenantLinkChangeHistories$ = this.salesforceTenantLinkChangeHistoryService.GetSalesforceTenantLinkChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public salesforceTenantLinkService: SalesforceTenantLinkService,
    public salesforceTenantLinkChangeHistoryService: SalesforceTenantLinkChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the salesforceTenantLinkId from the route parameters
    this.salesforceTenantLinkId = this.route.snapshot.paramMap.get('salesforceTenantLinkId');

    if (this.salesforceTenantLinkId === 'new' ||
        this.salesforceTenantLinkId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.salesforceTenantLinkData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.salesforceTenantLinkForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.salesforceTenantLinkForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Salesforce Tenant Link';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Salesforce Tenant Link';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.salesforceTenantLinkForm.dirty) {
      return confirm('You have unsaved Salesforce Tenant Link changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.salesforceTenantLinkId != null && this.salesforceTenantLinkId !== 'new') {

      const id = parseInt(this.salesforceTenantLinkId, 10);

      if (!isNaN(id)) {
        return { salesforceTenantLinkId: id };
      }
    }

    return null;
  }


/*
  * Loads the SalesforceTenantLink data for the current salesforceTenantLinkId.
  *
  * Fully respects the SalesforceTenantLinkService caching strategy and error handling strategy.
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
    if (!this.salesforceTenantLinkService.userIsSchedulerSalesforceTenantLinkReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SalesforceTenantLinks.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate salesforceTenantLinkId
    //
    if (!this.salesforceTenantLinkId) {

      this.alertService.showMessage('No SalesforceTenantLink ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const salesforceTenantLinkId = Number(this.salesforceTenantLinkId);

    if (isNaN(salesforceTenantLinkId) || salesforceTenantLinkId <= 0) {

      this.alertService.showMessage(`Invalid Salesforce Tenant Link ID: "${this.salesforceTenantLinkId}"`,
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
      // This is the most targeted way: clear only this SalesforceTenantLink + relations

      this.salesforceTenantLinkService.ClearRecordCache(salesforceTenantLinkId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.salesforceTenantLinkService.GetSalesforceTenantLink(salesforceTenantLinkId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (salesforceTenantLinkData) => {

        //
        // Success path — salesforceTenantLinkData can legitimately be null if 404'd but request succeeded
        //
        if (!salesforceTenantLinkData) {

          this.handleSalesforceTenantLinkNotFound(salesforceTenantLinkId);

        } else {

          this.salesforceTenantLinkData = salesforceTenantLinkData;
          this.buildFormValues(this.salesforceTenantLinkData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SalesforceTenantLink loaded successfully',
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
        this.handleSalesforceTenantLinkLoadError(error, salesforceTenantLinkId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSalesforceTenantLinkNotFound(salesforceTenantLinkId: number): void {

    this.salesforceTenantLinkData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SalesforceTenantLink #${salesforceTenantLinkId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSalesforceTenantLinkLoadError(error: any, salesforceTenantLinkId: number): void {

    let message = 'Failed to load Salesforce Tenant Link.';
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
          message = 'You do not have permission to view this Salesforce Tenant Link.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Salesforce Tenant Link #${salesforceTenantLinkId} was not found.`;
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

    console.error(`Salesforce Tenant Link load failed (ID: ${salesforceTenantLinkId})`, error);

    //
    // Reset UI to safe state
    //
    this.salesforceTenantLinkData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(salesforceTenantLinkData: SalesforceTenantLinkData | null) {

    if (salesforceTenantLinkData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.salesforceTenantLinkForm.reset({
        syncEnabled: false,
        syncDirectionFlags: '',
        pullIntervalMinutes: '',
        lastPullDate: '',
        loginUrl: '',
        sfClientId: '',
        sfClientSecret: '',
        sfUsername: '',
        sfPassword: '',
        sfSecurityToken: '',
        instanceUrl: '',
        apiVersion: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.salesforceTenantLinkForm.reset({
        syncEnabled: salesforceTenantLinkData.syncEnabled ?? false,
        syncDirectionFlags: salesforceTenantLinkData.syncDirectionFlags ?? '',
        pullIntervalMinutes: salesforceTenantLinkData.pullIntervalMinutes?.toString() ?? '',
        lastPullDate: isoUtcStringToDateTimeLocal(salesforceTenantLinkData.lastPullDate) ?? '',
        loginUrl: salesforceTenantLinkData.loginUrl ?? '',
        sfClientId: salesforceTenantLinkData.sfClientId ?? '',
        sfClientSecret: salesforceTenantLinkData.sfClientSecret ?? '',
        sfUsername: salesforceTenantLinkData.sfUsername ?? '',
        sfPassword: salesforceTenantLinkData.sfPassword ?? '',
        sfSecurityToken: salesforceTenantLinkData.sfSecurityToken ?? '',
        instanceUrl: salesforceTenantLinkData.instanceUrl ?? '',
        apiVersion: salesforceTenantLinkData.apiVersion ?? '',
        versionNumber: salesforceTenantLinkData.versionNumber?.toString() ?? '',
        active: salesforceTenantLinkData.active ?? true,
        deleted: salesforceTenantLinkData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.salesforceTenantLinkForm.markAsPristine();
    this.salesforceTenantLinkForm.markAsUntouched();
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

    if (this.salesforceTenantLinkService.userIsSchedulerSalesforceTenantLinkWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Salesforce Tenant Links", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.salesforceTenantLinkForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.salesforceTenantLinkForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.salesforceTenantLinkForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const salesforceTenantLinkSubmitData: SalesforceTenantLinkSubmitData = {
        id: this.salesforceTenantLinkData?.id || 0,
        syncEnabled: !!formValue.syncEnabled,
        syncDirectionFlags: formValue.syncDirectionFlags?.trim() || null,
        pullIntervalMinutes: formValue.pullIntervalMinutes ? Number(formValue.pullIntervalMinutes) : null,
        lastPullDate: formValue.lastPullDate ? dateTimeLocalToIsoUtc(formValue.lastPullDate.trim()) : null,
        loginUrl: formValue.loginUrl?.trim() || null,
        sfClientId: formValue.sfClientId?.trim() || null,
        sfClientSecret: formValue.sfClientSecret?.trim() || null,
        sfUsername: formValue.sfUsername?.trim() || null,
        sfPassword: formValue.sfPassword?.trim() || null,
        sfSecurityToken: formValue.sfSecurityToken?.trim() || null,
        instanceUrl: formValue.instanceUrl?.trim() || null,
        apiVersion: formValue.apiVersion?.trim() || null,
        versionNumber: this.salesforceTenantLinkData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.salesforceTenantLinkService.PutSalesforceTenantLink(salesforceTenantLinkSubmitData.id, salesforceTenantLinkSubmitData)
      : this.salesforceTenantLinkService.PostSalesforceTenantLink(salesforceTenantLinkSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSalesforceTenantLinkData) => {

        this.salesforceTenantLinkService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Salesforce Tenant Link's detail page
          //
          this.salesforceTenantLinkForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.salesforceTenantLinkForm.markAsUntouched();

          this.router.navigate(['/salesforcetenantlinks', savedSalesforceTenantLinkData.id]);
          this.alertService.showMessage('Salesforce Tenant Link added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.salesforceTenantLinkData = savedSalesforceTenantLinkData;
          this.buildFormValues(this.salesforceTenantLinkData);

          this.alertService.showMessage("Salesforce Tenant Link saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Salesforce Tenant Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Salesforce Tenant Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Salesforce Tenant Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerSalesforceTenantLinkReader(): boolean {
    return this.salesforceTenantLinkService.userIsSchedulerSalesforceTenantLinkReader();
  }

  public userIsSchedulerSalesforceTenantLinkWriter(): boolean {
    return this.salesforceTenantLinkService.userIsSchedulerSalesforceTenantLinkWriter();
  }
}
