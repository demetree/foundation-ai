/*
   GENERATED FORM FOR THE TELEMETRYAPPLICATIONMETRIC TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryApplicationMetric table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-application-metric-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetryApplicationMetricService, TelemetryApplicationMetricData, TelemetryApplicationMetricSubmitData } from '../../../telemetry-data-services/telemetry-application-metric.service';
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
interface TelemetryApplicationMetricFormValues {
  telemetrySnapshotId: number | bigint,       // For FK link number
  metricName: string,
  metricValue: string | null,
  state: string | null,     // Stored as string for form input, converted to number on submit.
  dataType: string | null,     // Stored as string for form input, converted to number on submit.
  numericValue: string | null,     // Stored as string for form input, converted to number on submit.
  category: string | null,
};


@Component({
  selector: 'app-telemetry-application-metric-detail',
  templateUrl: './telemetry-application-metric-detail.component.html',
  styleUrls: ['./telemetry-application-metric-detail.component.scss']
})

export class TelemetryApplicationMetricDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetryApplicationMetricFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetryApplicationMetricForm: FormGroup = this.fb.group({
        telemetrySnapshotId: [null, Validators.required],
        metricName: ['', Validators.required],
        metricValue: [''],
        state: [''],
        dataType: [''],
        numericValue: [''],
        category: [''],
      });


  public telemetryApplicationMetricId: string | null = null;
  public telemetryApplicationMetricData: TelemetryApplicationMetricData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  telemetryApplicationMetrics$ = this.telemetryApplicationMetricService.GetTelemetryApplicationMetricList();
  public telemetrySnapshots$ = this.telemetrySnapshotService.GetTelemetrySnapshotList();

  private destroy$ = new Subject<void>();

  constructor(
    public telemetryApplicationMetricService: TelemetryApplicationMetricService,
    public telemetrySnapshotService: TelemetrySnapshotService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the telemetryApplicationMetricId from the route parameters
    this.telemetryApplicationMetricId = this.route.snapshot.paramMap.get('telemetryApplicationMetricId');

    if (this.telemetryApplicationMetricId === 'new' ||
        this.telemetryApplicationMetricId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.telemetryApplicationMetricData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.telemetryApplicationMetricForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetryApplicationMetricForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Telemetry Application Metric';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Telemetry Application Metric';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.telemetryApplicationMetricForm.dirty) {
      return confirm('You have unsaved Telemetry Application Metric changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.telemetryApplicationMetricId != null && this.telemetryApplicationMetricId !== 'new') {

      const id = parseInt(this.telemetryApplicationMetricId, 10);

      if (!isNaN(id)) {
        return { telemetryApplicationMetricId: id };
      }
    }

    return null;
  }


/*
  * Loads the TelemetryApplicationMetric data for the current telemetryApplicationMetricId.
  *
  * Fully respects the TelemetryApplicationMetricService caching strategy and error handling strategy.
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
    if (!this.telemetryApplicationMetricService.userIsTelemetryTelemetryApplicationMetricReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read TelemetryApplicationMetrics.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate telemetryApplicationMetricId
    //
    if (!this.telemetryApplicationMetricId) {

      this.alertService.showMessage('No TelemetryApplicationMetric ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const telemetryApplicationMetricId = Number(this.telemetryApplicationMetricId);

    if (isNaN(telemetryApplicationMetricId) || telemetryApplicationMetricId <= 0) {

      this.alertService.showMessage(`Invalid Telemetry Application Metric ID: "${this.telemetryApplicationMetricId}"`,
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
      // This is the most targeted way: clear only this TelemetryApplicationMetric + relations

      this.telemetryApplicationMetricService.ClearRecordCache(telemetryApplicationMetricId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.telemetryApplicationMetricService.GetTelemetryApplicationMetric(telemetryApplicationMetricId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (telemetryApplicationMetricData) => {

        //
        // Success path — telemetryApplicationMetricData can legitimately be null if 404'd but request succeeded
        //
        if (!telemetryApplicationMetricData) {

          this.handleTelemetryApplicationMetricNotFound(telemetryApplicationMetricId);

        } else {

          this.telemetryApplicationMetricData = telemetryApplicationMetricData;
          this.buildFormValues(this.telemetryApplicationMetricData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'TelemetryApplicationMetric loaded successfully',
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
        this.handleTelemetryApplicationMetricLoadError(error, telemetryApplicationMetricId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleTelemetryApplicationMetricNotFound(telemetryApplicationMetricId: number): void {

    this.telemetryApplicationMetricData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `TelemetryApplicationMetric #${telemetryApplicationMetricId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleTelemetryApplicationMetricLoadError(error: any, telemetryApplicationMetricId: number): void {

    let message = 'Failed to load Telemetry Application Metric.';
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
          message = 'You do not have permission to view this Telemetry Application Metric.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Telemetry Application Metric #${telemetryApplicationMetricId} was not found.`;
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

    console.error(`Telemetry Application Metric load failed (ID: ${telemetryApplicationMetricId})`, error);

    //
    // Reset UI to safe state
    //
    this.telemetryApplicationMetricData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(telemetryApplicationMetricData: TelemetryApplicationMetricData | null) {

    if (telemetryApplicationMetricData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetryApplicationMetricForm.reset({
        telemetrySnapshotId: null,
        metricName: '',
        metricValue: '',
        state: '',
        dataType: '',
        numericValue: '',
        category: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetryApplicationMetricForm.reset({
        telemetrySnapshotId: telemetryApplicationMetricData.telemetrySnapshotId,
        metricName: telemetryApplicationMetricData.metricName ?? '',
        metricValue: telemetryApplicationMetricData.metricValue ?? '',
        state: telemetryApplicationMetricData.state?.toString() ?? '',
        dataType: telemetryApplicationMetricData.dataType?.toString() ?? '',
        numericValue: telemetryApplicationMetricData.numericValue?.toString() ?? '',
        category: telemetryApplicationMetricData.category ?? '',
      }, { emitEvent: false});
    }

    this.telemetryApplicationMetricForm.markAsPristine();
    this.telemetryApplicationMetricForm.markAsUntouched();
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

    if (this.telemetryApplicationMetricService.userIsTelemetryTelemetryApplicationMetricWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Telemetry Application Metrics", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.telemetryApplicationMetricForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetryApplicationMetricForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetryApplicationMetricForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetryApplicationMetricSubmitData: TelemetryApplicationMetricSubmitData = {
        id: this.telemetryApplicationMetricData?.id || 0,
        telemetrySnapshotId: Number(formValue.telemetrySnapshotId),
        metricName: formValue.metricName!.trim(),
        metricValue: formValue.metricValue?.trim() || null,
        state: formValue.state ? Number(formValue.state) : null,
        dataType: formValue.dataType ? Number(formValue.dataType) : null,
        numericValue: formValue.numericValue ? Number(formValue.numericValue) : null,
        category: formValue.category?.trim() || null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.telemetryApplicationMetricService.PutTelemetryApplicationMetric(telemetryApplicationMetricSubmitData.id, telemetryApplicationMetricSubmitData)
      : this.telemetryApplicationMetricService.PostTelemetryApplicationMetric(telemetryApplicationMetricSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedTelemetryApplicationMetricData) => {

        this.telemetryApplicationMetricService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Telemetry Application Metric's detail page
          //
          this.telemetryApplicationMetricForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.telemetryApplicationMetricForm.markAsUntouched();

          this.router.navigate(['/telemetryapplicationmetrics', savedTelemetryApplicationMetricData.id]);
          this.alertService.showMessage('Telemetry Application Metric added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.telemetryApplicationMetricData = savedTelemetryApplicationMetricData;
          this.buildFormValues(this.telemetryApplicationMetricData);

          this.alertService.showMessage("Telemetry Application Metric saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Telemetry Application Metric.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Application Metric.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Application Metric could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsTelemetryTelemetryApplicationMetricReader(): boolean {
    return this.telemetryApplicationMetricService.userIsTelemetryTelemetryApplicationMetricReader();
  }

  public userIsTelemetryTelemetryApplicationMetricWriter(): boolean {
    return this.telemetryApplicationMetricService.userIsTelemetryTelemetryApplicationMetricWriter();
  }
}
