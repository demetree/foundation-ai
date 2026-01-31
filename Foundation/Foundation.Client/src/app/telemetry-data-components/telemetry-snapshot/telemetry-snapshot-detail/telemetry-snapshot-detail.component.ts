/*
   GENERATED FORM FOR THE TELEMETRYSNAPSHOT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetrySnapshot table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-snapshot-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetrySnapshotService, TelemetrySnapshotData, TelemetrySnapshotSubmitData } from '../../../telemetry-data-services/telemetry-snapshot.service';
import { TelemetryApplicationService } from '../../../telemetry-data-services/telemetry-application.service';
import { TelemetryCollectionRunService } from '../../../telemetry-data-services/telemetry-collection-run.service';
import { TelemetryDatabaseHealthService } from '../../../telemetry-data-services/telemetry-database-health.service';
import { TelemetryDiskHealthService } from '../../../telemetry-data-services/telemetry-disk-health.service';
import { TelemetrySessionSnapshotService } from '../../../telemetry-data-services/telemetry-session-snapshot.service';
import { TelemetryApplicationMetricService } from '../../../telemetry-data-services/telemetry-application-metric.service';
import { TelemetryErrorEventService } from '../../../telemetry-data-services/telemetry-error-event.service';
import { TelemetryLogErrorService } from '../../../telemetry-data-services/telemetry-log-error.service';
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
interface TelemetrySnapshotFormValues {
  telemetryApplicationId: number | bigint,       // For FK link number
  telemetryCollectionRunId: number | bigint,       // For FK link number
  collectedAt: string,
  isOnline: boolean,
  uptimeSeconds: string | null,     // Stored as string for form input, converted to number on submit.
  memoryWorkingSetMB: string | null,     // Stored as string for form input, converted to number on submit.
  memoryGcHeapMB: string | null,     // Stored as string for form input, converted to number on submit.
  memoryPercent: string | null,     // Stored as string for form input, converted to number on submit.
  systemMemoryPercent: string | null,     // Stored as string for form input, converted to number on submit.
  cpuPercent: string | null,     // Stored as string for form input, converted to number on submit.
  systemCpuPercent: string | null,     // Stored as string for form input, converted to number on submit.
  threadPoolWorkerThreads: string | null,     // Stored as string for form input, converted to number on submit.
  threadPoolCompletionPortThreads: string | null,     // Stored as string for form input, converted to number on submit.
  threadPoolPendingWorkItems: string | null,     // Stored as string for form input, converted to number on submit.
  machineName: string | null,
  dotNetVersion: string | null,
  statusJson: string | null,
};


@Component({
  selector: 'app-telemetry-snapshot-detail',
  templateUrl: './telemetry-snapshot-detail.component.html',
  styleUrls: ['./telemetry-snapshot-detail.component.scss']
})

export class TelemetrySnapshotDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetrySnapshotFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetrySnapshotForm: FormGroup = this.fb.group({
        telemetryApplicationId: [null, Validators.required],
        telemetryCollectionRunId: [null, Validators.required],
        collectedAt: ['', Validators.required],
        isOnline: [false],
        uptimeSeconds: [''],
        memoryWorkingSetMB: [''],
        memoryGcHeapMB: [''],
        memoryPercent: [''],
        systemMemoryPercent: [''],
        cpuPercent: [''],
        systemCpuPercent: [''],
        threadPoolWorkerThreads: [''],
        threadPoolCompletionPortThreads: [''],
        threadPoolPendingWorkItems: [''],
        machineName: [''],
        dotNetVersion: [''],
        statusJson: [''],
      });


  public telemetrySnapshotId: string | null = null;
  public telemetrySnapshotData: TelemetrySnapshotData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  telemetrySnapshots$ = this.telemetrySnapshotService.GetTelemetrySnapshotList();
  public telemetryApplications$ = this.telemetryApplicationService.GetTelemetryApplicationList();
  public telemetryCollectionRuns$ = this.telemetryCollectionRunService.GetTelemetryCollectionRunList();
  public telemetryDatabaseHealths$ = this.telemetryDatabaseHealthService.GetTelemetryDatabaseHealthList();
  public telemetryDiskHealths$ = this.telemetryDiskHealthService.GetTelemetryDiskHealthList();
  public telemetrySessionSnapshots$ = this.telemetrySessionSnapshotService.GetTelemetrySessionSnapshotList();
  public telemetryApplicationMetrics$ = this.telemetryApplicationMetricService.GetTelemetryApplicationMetricList();
  public telemetryErrorEvents$ = this.telemetryErrorEventService.GetTelemetryErrorEventList();
  public telemetryLogErrors$ = this.telemetryLogErrorService.GetTelemetryLogErrorList();

  private destroy$ = new Subject<void>();

  constructor(
    public telemetrySnapshotService: TelemetrySnapshotService,
    public telemetryApplicationService: TelemetryApplicationService,
    public telemetryCollectionRunService: TelemetryCollectionRunService,
    public telemetryDatabaseHealthService: TelemetryDatabaseHealthService,
    public telemetryDiskHealthService: TelemetryDiskHealthService,
    public telemetrySessionSnapshotService: TelemetrySessionSnapshotService,
    public telemetryApplicationMetricService: TelemetryApplicationMetricService,
    public telemetryErrorEventService: TelemetryErrorEventService,
    public telemetryLogErrorService: TelemetryLogErrorService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the telemetrySnapshotId from the route parameters
    this.telemetrySnapshotId = this.route.snapshot.paramMap.get('telemetrySnapshotId');

    if (this.telemetrySnapshotId === 'new' ||
        this.telemetrySnapshotId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.telemetrySnapshotData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.telemetrySnapshotForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetrySnapshotForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Telemetry Snapshot';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Telemetry Snapshot';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.telemetrySnapshotForm.dirty) {
      return confirm('You have unsaved Telemetry Snapshot changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.telemetrySnapshotId != null && this.telemetrySnapshotId !== 'new') {

      const id = parseInt(this.telemetrySnapshotId, 10);

      if (!isNaN(id)) {
        return { telemetrySnapshotId: id };
      }
    }

    return null;
  }


/*
  * Loads the TelemetrySnapshot data for the current telemetrySnapshotId.
  *
  * Fully respects the TelemetrySnapshotService caching strategy and error handling strategy.
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
    if (!this.telemetrySnapshotService.userIsTelemetryTelemetrySnapshotReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read TelemetrySnapshots.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate telemetrySnapshotId
    //
    if (!this.telemetrySnapshotId) {

      this.alertService.showMessage('No TelemetrySnapshot ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const telemetrySnapshotId = Number(this.telemetrySnapshotId);

    if (isNaN(telemetrySnapshotId) || telemetrySnapshotId <= 0) {

      this.alertService.showMessage(`Invalid Telemetry Snapshot ID: "${this.telemetrySnapshotId}"`,
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
      // This is the most targeted way: clear only this TelemetrySnapshot + relations

      this.telemetrySnapshotService.ClearRecordCache(telemetrySnapshotId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.telemetrySnapshotService.GetTelemetrySnapshot(telemetrySnapshotId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (telemetrySnapshotData) => {

        //
        // Success path — telemetrySnapshotData can legitimately be null if 404'd but request succeeded
        //
        if (!telemetrySnapshotData) {

          this.handleTelemetrySnapshotNotFound(telemetrySnapshotId);

        } else {

          this.telemetrySnapshotData = telemetrySnapshotData;
          this.buildFormValues(this.telemetrySnapshotData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'TelemetrySnapshot loaded successfully',
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
        this.handleTelemetrySnapshotLoadError(error, telemetrySnapshotId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleTelemetrySnapshotNotFound(telemetrySnapshotId: number): void {

    this.telemetrySnapshotData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `TelemetrySnapshot #${telemetrySnapshotId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleTelemetrySnapshotLoadError(error: any, telemetrySnapshotId: number): void {

    let message = 'Failed to load Telemetry Snapshot.';
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
          message = 'You do not have permission to view this Telemetry Snapshot.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Telemetry Snapshot #${telemetrySnapshotId} was not found.`;
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

    console.error(`Telemetry Snapshot load failed (ID: ${telemetrySnapshotId})`, error);

    //
    // Reset UI to safe state
    //
    this.telemetrySnapshotData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(telemetrySnapshotData: TelemetrySnapshotData | null) {

    if (telemetrySnapshotData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetrySnapshotForm.reset({
        telemetryApplicationId: null,
        telemetryCollectionRunId: null,
        collectedAt: '',
        isOnline: false,
        uptimeSeconds: '',
        memoryWorkingSetMB: '',
        memoryGcHeapMB: '',
        memoryPercent: '',
        systemMemoryPercent: '',
        cpuPercent: '',
        systemCpuPercent: '',
        threadPoolWorkerThreads: '',
        threadPoolCompletionPortThreads: '',
        threadPoolPendingWorkItems: '',
        machineName: '',
        dotNetVersion: '',
        statusJson: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetrySnapshotForm.reset({
        telemetryApplicationId: telemetrySnapshotData.telemetryApplicationId,
        telemetryCollectionRunId: telemetrySnapshotData.telemetryCollectionRunId,
        collectedAt: isoUtcStringToDateTimeLocal(telemetrySnapshotData.collectedAt) ?? '',
        isOnline: telemetrySnapshotData.isOnline ?? false,
        uptimeSeconds: telemetrySnapshotData.uptimeSeconds?.toString() ?? '',
        memoryWorkingSetMB: telemetrySnapshotData.memoryWorkingSetMB?.toString() ?? '',
        memoryGcHeapMB: telemetrySnapshotData.memoryGcHeapMB?.toString() ?? '',
        memoryPercent: telemetrySnapshotData.memoryPercent?.toString() ?? '',
        systemMemoryPercent: telemetrySnapshotData.systemMemoryPercent?.toString() ?? '',
        cpuPercent: telemetrySnapshotData.cpuPercent?.toString() ?? '',
        systemCpuPercent: telemetrySnapshotData.systemCpuPercent?.toString() ?? '',
        threadPoolWorkerThreads: telemetrySnapshotData.threadPoolWorkerThreads?.toString() ?? '',
        threadPoolCompletionPortThreads: telemetrySnapshotData.threadPoolCompletionPortThreads?.toString() ?? '',
        threadPoolPendingWorkItems: telemetrySnapshotData.threadPoolPendingWorkItems?.toString() ?? '',
        machineName: telemetrySnapshotData.machineName ?? '',
        dotNetVersion: telemetrySnapshotData.dotNetVersion ?? '',
        statusJson: telemetrySnapshotData.statusJson ?? '',
      }, { emitEvent: false});
    }

    this.telemetrySnapshotForm.markAsPristine();
    this.telemetrySnapshotForm.markAsUntouched();
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

    if (this.telemetrySnapshotService.userIsTelemetryTelemetrySnapshotWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Telemetry Snapshots", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.telemetrySnapshotForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetrySnapshotForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetrySnapshotForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetrySnapshotSubmitData: TelemetrySnapshotSubmitData = {
        id: this.telemetrySnapshotData?.id || 0,
        telemetryApplicationId: Number(formValue.telemetryApplicationId),
        telemetryCollectionRunId: Number(formValue.telemetryCollectionRunId),
        collectedAt: dateTimeLocalToIsoUtc(formValue.collectedAt!.trim())!,
        isOnline: !!formValue.isOnline,
        uptimeSeconds: formValue.uptimeSeconds ? Number(formValue.uptimeSeconds) : null,
        memoryWorkingSetMB: formValue.memoryWorkingSetMB ? Number(formValue.memoryWorkingSetMB) : null,
        memoryGcHeapMB: formValue.memoryGcHeapMB ? Number(formValue.memoryGcHeapMB) : null,
        memoryPercent: formValue.memoryPercent ? Number(formValue.memoryPercent) : null,
        systemMemoryPercent: formValue.systemMemoryPercent ? Number(formValue.systemMemoryPercent) : null,
        cpuPercent: formValue.cpuPercent ? Number(formValue.cpuPercent) : null,
        systemCpuPercent: formValue.systemCpuPercent ? Number(formValue.systemCpuPercent) : null,
        threadPoolWorkerThreads: formValue.threadPoolWorkerThreads ? Number(formValue.threadPoolWorkerThreads) : null,
        threadPoolCompletionPortThreads: formValue.threadPoolCompletionPortThreads ? Number(formValue.threadPoolCompletionPortThreads) : null,
        threadPoolPendingWorkItems: formValue.threadPoolPendingWorkItems ? Number(formValue.threadPoolPendingWorkItems) : null,
        machineName: formValue.machineName?.trim() || null,
        dotNetVersion: formValue.dotNetVersion?.trim() || null,
        statusJson: formValue.statusJson?.trim() || null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.telemetrySnapshotService.PutTelemetrySnapshot(telemetrySnapshotSubmitData.id, telemetrySnapshotSubmitData)
      : this.telemetrySnapshotService.PostTelemetrySnapshot(telemetrySnapshotSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedTelemetrySnapshotData) => {

        this.telemetrySnapshotService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Telemetry Snapshot's detail page
          //
          this.telemetrySnapshotForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.telemetrySnapshotForm.markAsUntouched();

          this.router.navigate(['/telemetrysnapshots', savedTelemetrySnapshotData.id]);
          this.alertService.showMessage('Telemetry Snapshot added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.telemetrySnapshotData = savedTelemetrySnapshotData;
          this.buildFormValues(this.telemetrySnapshotData);

          this.alertService.showMessage("Telemetry Snapshot saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Telemetry Snapshot.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Snapshot.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Snapshot could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsTelemetryTelemetrySnapshotReader(): boolean {
    return this.telemetrySnapshotService.userIsTelemetryTelemetrySnapshotReader();
  }

  public userIsTelemetryTelemetrySnapshotWriter(): boolean {
    return this.telemetrySnapshotService.userIsTelemetryTelemetrySnapshotWriter();
  }
}
