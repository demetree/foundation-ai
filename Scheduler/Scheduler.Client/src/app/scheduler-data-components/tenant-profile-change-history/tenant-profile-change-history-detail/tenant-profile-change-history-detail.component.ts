/*
   GENERATED FORM FOR THE TENANTPROFILECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TenantProfileChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to tenant-profile-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TenantProfileChangeHistoryService, TenantProfileChangeHistoryData, TenantProfileChangeHistorySubmitData } from '../../../scheduler-data-services/tenant-profile-change-history.service';
import { TenantProfileService } from '../../../scheduler-data-services/tenant-profile.service';
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
interface TenantProfileChangeHistoryFormValues {
  tenantProfileId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-tenant-profile-change-history-detail',
  templateUrl: './tenant-profile-change-history-detail.component.html',
  styleUrls: ['./tenant-profile-change-history-detail.component.scss']
})

export class TenantProfileChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TenantProfileChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public tenantProfileChangeHistoryForm: FormGroup = this.fb.group({
        tenantProfileId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public tenantProfileChangeHistoryId: string | null = null;
  public tenantProfileChangeHistoryData: TenantProfileChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  tenantProfileChangeHistories$ = this.tenantProfileChangeHistoryService.GetTenantProfileChangeHistoryList();
  public tenantProfiles$ = this.tenantProfileService.GetTenantProfileList();

  private destroy$ = new Subject<void>();

  constructor(
    public tenantProfileChangeHistoryService: TenantProfileChangeHistoryService,
    public tenantProfileService: TenantProfileService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the tenantProfileChangeHistoryId from the route parameters
    this.tenantProfileChangeHistoryId = this.route.snapshot.paramMap.get('tenantProfileChangeHistoryId');

    if (this.tenantProfileChangeHistoryId === 'new' ||
        this.tenantProfileChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.tenantProfileChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.tenantProfileChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.tenantProfileChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Tenant Profile Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Tenant Profile Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.tenantProfileChangeHistoryForm.dirty) {
      return confirm('You have unsaved Tenant Profile Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.tenantProfileChangeHistoryId != null && this.tenantProfileChangeHistoryId !== 'new') {

      const id = parseInt(this.tenantProfileChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { tenantProfileChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the TenantProfileChangeHistory data for the current tenantProfileChangeHistoryId.
  *
  * Fully respects the TenantProfileChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.tenantProfileChangeHistoryService.userIsSchedulerTenantProfileChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read TenantProfileChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate tenantProfileChangeHistoryId
    //
    if (!this.tenantProfileChangeHistoryId) {

      this.alertService.showMessage('No TenantProfileChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const tenantProfileChangeHistoryId = Number(this.tenantProfileChangeHistoryId);

    if (isNaN(tenantProfileChangeHistoryId) || tenantProfileChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Tenant Profile Change History ID: "${this.tenantProfileChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this TenantProfileChangeHistory + relations

      this.tenantProfileChangeHistoryService.ClearRecordCache(tenantProfileChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.tenantProfileChangeHistoryService.GetTenantProfileChangeHistory(tenantProfileChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (tenantProfileChangeHistoryData) => {

        //
        // Success path — tenantProfileChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!tenantProfileChangeHistoryData) {

          this.handleTenantProfileChangeHistoryNotFound(tenantProfileChangeHistoryId);

        } else {

          this.tenantProfileChangeHistoryData = tenantProfileChangeHistoryData;
          this.buildFormValues(this.tenantProfileChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'TenantProfileChangeHistory loaded successfully',
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
        this.handleTenantProfileChangeHistoryLoadError(error, tenantProfileChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleTenantProfileChangeHistoryNotFound(tenantProfileChangeHistoryId: number): void {

    this.tenantProfileChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `TenantProfileChangeHistory #${tenantProfileChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleTenantProfileChangeHistoryLoadError(error: any, tenantProfileChangeHistoryId: number): void {

    let message = 'Failed to load Tenant Profile Change History.';
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
          message = 'You do not have permission to view this Tenant Profile Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Tenant Profile Change History #${tenantProfileChangeHistoryId} was not found.`;
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

    console.error(`Tenant Profile Change History load failed (ID: ${tenantProfileChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.tenantProfileChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(tenantProfileChangeHistoryData: TenantProfileChangeHistoryData | null) {

    if (tenantProfileChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.tenantProfileChangeHistoryForm.reset({
        tenantProfileId: null,
        versionNumber: '',
        timeStamp: '',
        userId: '',
        data: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.tenantProfileChangeHistoryForm.reset({
        tenantProfileId: tenantProfileChangeHistoryData.tenantProfileId,
        versionNumber: tenantProfileChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(tenantProfileChangeHistoryData.timeStamp) ?? '',
        userId: tenantProfileChangeHistoryData.userId?.toString() ?? '',
        data: tenantProfileChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.tenantProfileChangeHistoryForm.markAsPristine();
    this.tenantProfileChangeHistoryForm.markAsUntouched();
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

    if (this.tenantProfileChangeHistoryService.userIsSchedulerTenantProfileChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Tenant Profile Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.tenantProfileChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.tenantProfileChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.tenantProfileChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const tenantProfileChangeHistorySubmitData: TenantProfileChangeHistorySubmitData = {
        id: this.tenantProfileChangeHistoryData?.id || 0,
        tenantProfileId: Number(formValue.tenantProfileId),
        versionNumber: this.tenantProfileChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.tenantProfileChangeHistoryService.PutTenantProfileChangeHistory(tenantProfileChangeHistorySubmitData.id, tenantProfileChangeHistorySubmitData)
      : this.tenantProfileChangeHistoryService.PostTenantProfileChangeHistory(tenantProfileChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedTenantProfileChangeHistoryData) => {

        this.tenantProfileChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Tenant Profile Change History's detail page
          //
          this.tenantProfileChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.tenantProfileChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/tenantprofilechangehistories', savedTenantProfileChangeHistoryData.id]);
          this.alertService.showMessage('Tenant Profile Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.tenantProfileChangeHistoryData = savedTenantProfileChangeHistoryData;
          this.buildFormValues(this.tenantProfileChangeHistoryData);

          this.alertService.showMessage("Tenant Profile Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Tenant Profile Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Tenant Profile Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Tenant Profile Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerTenantProfileChangeHistoryReader(): boolean {
    return this.tenantProfileChangeHistoryService.userIsSchedulerTenantProfileChangeHistoryReader();
  }

  public userIsSchedulerTenantProfileChangeHistoryWriter(): boolean {
    return this.tenantProfileChangeHistoryService.userIsSchedulerTenantProfileChangeHistoryWriter();
  }
}
