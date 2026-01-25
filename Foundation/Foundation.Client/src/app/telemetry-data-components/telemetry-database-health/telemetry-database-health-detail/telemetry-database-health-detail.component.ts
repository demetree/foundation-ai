/*
   GENERATED FORM FOR THE TELEMETRYDATABASEHEALTH TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryDatabaseHealth table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-database-health-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetryDatabaseHealthService, TelemetryDatabaseHealthData, TelemetryDatabaseHealthSubmitData } from '../../../telemetry-data-services/telemetry-database-health.service';
import { TelemetrySnapshotService } from '../../../telemetry-data-services/telemetry-snapshot.service';
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
interface TelemetryDatabaseHealthFormValues {
  telemetrySnapshotId: number | bigint,       // For FK link number
  databaseName: string,
  isConnected: boolean,
  status: string | null,
  server: string | null,
  provider: string | null,
  errorMessage: string | null,
};


@Component({
  selector: 'app-telemetry-database-health-detail',
  templateUrl: './telemetry-database-health-detail.component.html',
  styleUrls: ['./telemetry-database-health-detail.component.scss']
})

export class TelemetryDatabaseHealthDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetryDatabaseHealthFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetryDatabaseHealthForm: FormGroup = this.fb.group({
        telemetrySnapshotId: [null, Validators.required],
        databaseName: ['', Validators.required],
        isConnected: [false],
        status: [''],
        server: [''],
        provider: [''],
        errorMessage: [''],
      });


  public telemetryDatabaseHealthId: string | null = null;
  public telemetryDatabaseHealthData: TelemetryDatabaseHealthData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  telemetryDatabaseHealths$ = this.telemetryDatabaseHealthService.GetTelemetryDatabaseHealthList();
  public telemetrySnapshots$ = this.telemetrySnapshotService.GetTelemetrySnapshotList();

  private destroy$ = new Subject<void>();

  constructor(
    public telemetryDatabaseHealthService: TelemetryDatabaseHealthService,
    public telemetrySnapshotService: TelemetrySnapshotService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the telemetryDatabaseHealthId from the route parameters
    this.telemetryDatabaseHealthId = this.route.snapshot.paramMap.get('telemetryDatabaseHealthId');

    if (this.telemetryDatabaseHealthId === 'new' ||
        this.telemetryDatabaseHealthId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.telemetryDatabaseHealthData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.telemetryDatabaseHealthForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetryDatabaseHealthForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Telemetry Database Health';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Telemetry Database Health';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.telemetryDatabaseHealthForm.dirty) {
      return confirm('You have unsaved Telemetry Database Health changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.telemetryDatabaseHealthId != null && this.telemetryDatabaseHealthId !== 'new') {

      const id = parseInt(this.telemetryDatabaseHealthId, 10);

      if (!isNaN(id)) {
        return { telemetryDatabaseHealthId: id };
      }
    }

    return null;
  }


/*
  * Loads the TelemetryDatabaseHealth data for the current telemetryDatabaseHealthId.
  *
  * Fully respects the TelemetryDatabaseHealthService caching strategy and error handling strategy.
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
    if (!this.telemetryDatabaseHealthService.userIsTelemetryTelemetryDatabaseHealthReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read TelemetryDatabaseHealths.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate telemetryDatabaseHealthId
    //
    if (!this.telemetryDatabaseHealthId) {

      this.alertService.showMessage('No TelemetryDatabaseHealth ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const telemetryDatabaseHealthId = Number(this.telemetryDatabaseHealthId);

    if (isNaN(telemetryDatabaseHealthId) || telemetryDatabaseHealthId <= 0) {

      this.alertService.showMessage(`Invalid Telemetry Database Health ID: "${this.telemetryDatabaseHealthId}"`,
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
      // This is the most targeted way: clear only this TelemetryDatabaseHealth + relations

      this.telemetryDatabaseHealthService.ClearRecordCache(telemetryDatabaseHealthId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.telemetryDatabaseHealthService.GetTelemetryDatabaseHealth(telemetryDatabaseHealthId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (telemetryDatabaseHealthData) => {

        //
        // Success path — telemetryDatabaseHealthData can legitimately be null if 404'd but request succeeded
        //
        if (!telemetryDatabaseHealthData) {

          this.handleTelemetryDatabaseHealthNotFound(telemetryDatabaseHealthId);

        } else {

          this.telemetryDatabaseHealthData = telemetryDatabaseHealthData;
          this.buildFormValues(this.telemetryDatabaseHealthData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'TelemetryDatabaseHealth loaded successfully',
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
        this.handleTelemetryDatabaseHealthLoadError(error, telemetryDatabaseHealthId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleTelemetryDatabaseHealthNotFound(telemetryDatabaseHealthId: number): void {

    this.telemetryDatabaseHealthData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `TelemetryDatabaseHealth #${telemetryDatabaseHealthId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleTelemetryDatabaseHealthLoadError(error: any, telemetryDatabaseHealthId: number): void {

    let message = 'Failed to load Telemetry Database Health.';
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
          message = 'You do not have permission to view this Telemetry Database Health.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Telemetry Database Health #${telemetryDatabaseHealthId} was not found.`;
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

    console.error(`Telemetry Database Health load failed (ID: ${telemetryDatabaseHealthId})`, error);

    //
    // Reset UI to safe state
    //
    this.telemetryDatabaseHealthData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(telemetryDatabaseHealthData: TelemetryDatabaseHealthData | null) {

    if (telemetryDatabaseHealthData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetryDatabaseHealthForm.reset({
        telemetrySnapshotId: null,
        databaseName: '',
        isConnected: false,
        status: '',
        server: '',
        provider: '',
        errorMessage: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetryDatabaseHealthForm.reset({
        telemetrySnapshotId: telemetryDatabaseHealthData.telemetrySnapshotId,
        databaseName: telemetryDatabaseHealthData.databaseName ?? '',
        isConnected: telemetryDatabaseHealthData.isConnected ?? false,
        status: telemetryDatabaseHealthData.status ?? '',
        server: telemetryDatabaseHealthData.server ?? '',
        provider: telemetryDatabaseHealthData.provider ?? '',
        errorMessage: telemetryDatabaseHealthData.errorMessage ?? '',
      }, { emitEvent: false});
    }

    this.telemetryDatabaseHealthForm.markAsPristine();
    this.telemetryDatabaseHealthForm.markAsUntouched();
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

    if (this.telemetryDatabaseHealthService.userIsTelemetryTelemetryDatabaseHealthWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Telemetry Database Healths", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.telemetryDatabaseHealthForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetryDatabaseHealthForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetryDatabaseHealthForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetryDatabaseHealthSubmitData: TelemetryDatabaseHealthSubmitData = {
        id: this.telemetryDatabaseHealthData?.id || 0,
        telemetrySnapshotId: Number(formValue.telemetrySnapshotId),
        databaseName: formValue.databaseName!.trim(),
        isConnected: !!formValue.isConnected,
        status: formValue.status?.trim() || null,
        server: formValue.server?.trim() || null,
        provider: formValue.provider?.trim() || null,
        errorMessage: formValue.errorMessage?.trim() || null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.telemetryDatabaseHealthService.PutTelemetryDatabaseHealth(telemetryDatabaseHealthSubmitData.id, telemetryDatabaseHealthSubmitData)
      : this.telemetryDatabaseHealthService.PostTelemetryDatabaseHealth(telemetryDatabaseHealthSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedTelemetryDatabaseHealthData) => {

        this.telemetryDatabaseHealthService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Telemetry Database Health's detail page
          //
          this.telemetryDatabaseHealthForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.telemetryDatabaseHealthForm.markAsUntouched();

          this.router.navigate(['/telemetrydatabasehealths', savedTelemetryDatabaseHealthData.id]);
          this.alertService.showMessage('Telemetry Database Health added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.telemetryDatabaseHealthData = savedTelemetryDatabaseHealthData;
          this.buildFormValues(this.telemetryDatabaseHealthData);

          this.alertService.showMessage("Telemetry Database Health saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Telemetry Database Health.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Database Health.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Database Health could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsTelemetryTelemetryDatabaseHealthReader(): boolean {
    return this.telemetryDatabaseHealthService.userIsTelemetryTelemetryDatabaseHealthReader();
  }

  public userIsTelemetryTelemetryDatabaseHealthWriter(): boolean {
    return this.telemetryDatabaseHealthService.userIsTelemetryTelemetryDatabaseHealthWriter();
  }
}
