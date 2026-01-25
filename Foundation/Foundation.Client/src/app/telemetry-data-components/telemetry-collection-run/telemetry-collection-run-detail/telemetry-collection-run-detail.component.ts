/*
   GENERATED FORM FOR THE TELEMETRYCOLLECTIONRUN TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryCollectionRun table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-collection-run-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetryCollectionRunService, TelemetryCollectionRunData, TelemetryCollectionRunSubmitData } from '../../../telemetry-data-services/telemetry-collection-run.service';
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
interface TelemetryCollectionRunFormValues {
  startTime: string,
  endTime: string | null,
  applicationsPolled: string | null,     // Stored as string for form input, converted to number on submit.
  applicationsSucceeded: string | null,     // Stored as string for form input, converted to number on submit.
  errorMessage: string | null,
};


@Component({
  selector: 'app-telemetry-collection-run-detail',
  templateUrl: './telemetry-collection-run-detail.component.html',
  styleUrls: ['./telemetry-collection-run-detail.component.scss']
})

export class TelemetryCollectionRunDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetryCollectionRunFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetryCollectionRunForm: FormGroup = this.fb.group({
        startTime: ['', Validators.required],
        endTime: [''],
        applicationsPolled: [''],
        applicationsSucceeded: [''],
        errorMessage: [''],
      });


  public telemetryCollectionRunId: string | null = null;
  public telemetryCollectionRunData: TelemetryCollectionRunData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  telemetryCollectionRuns$ = this.telemetryCollectionRunService.GetTelemetryCollectionRunList();
  public telemetrySnapshots$ = this.telemetrySnapshotService.GetTelemetrySnapshotList();

  private destroy$ = new Subject<void>();

  constructor(
    public telemetryCollectionRunService: TelemetryCollectionRunService,
    public telemetrySnapshotService: TelemetrySnapshotService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the telemetryCollectionRunId from the route parameters
    this.telemetryCollectionRunId = this.route.snapshot.paramMap.get('telemetryCollectionRunId');

    if (this.telemetryCollectionRunId === 'new' ||
        this.telemetryCollectionRunId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.telemetryCollectionRunData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.telemetryCollectionRunForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetryCollectionRunForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Telemetry Collection Run';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Telemetry Collection Run';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.telemetryCollectionRunForm.dirty) {
      return confirm('You have unsaved Telemetry Collection Run changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.telemetryCollectionRunId != null && this.telemetryCollectionRunId !== 'new') {

      const id = parseInt(this.telemetryCollectionRunId, 10);

      if (!isNaN(id)) {
        return { telemetryCollectionRunId: id };
      }
    }

    return null;
  }


/*
  * Loads the TelemetryCollectionRun data for the current telemetryCollectionRunId.
  *
  * Fully respects the TelemetryCollectionRunService caching strategy and error handling strategy.
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
    if (!this.telemetryCollectionRunService.userIsTelemetryTelemetryCollectionRunReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read TelemetryCollectionRuns.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate telemetryCollectionRunId
    //
    if (!this.telemetryCollectionRunId) {

      this.alertService.showMessage('No TelemetryCollectionRun ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const telemetryCollectionRunId = Number(this.telemetryCollectionRunId);

    if (isNaN(telemetryCollectionRunId) || telemetryCollectionRunId <= 0) {

      this.alertService.showMessage(`Invalid Telemetry Collection Run ID: "${this.telemetryCollectionRunId}"`,
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
      // This is the most targeted way: clear only this TelemetryCollectionRun + relations

      this.telemetryCollectionRunService.ClearRecordCache(telemetryCollectionRunId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.telemetryCollectionRunService.GetTelemetryCollectionRun(telemetryCollectionRunId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (telemetryCollectionRunData) => {

        //
        // Success path — telemetryCollectionRunData can legitimately be null if 404'd but request succeeded
        //
        if (!telemetryCollectionRunData) {

          this.handleTelemetryCollectionRunNotFound(telemetryCollectionRunId);

        } else {

          this.telemetryCollectionRunData = telemetryCollectionRunData;
          this.buildFormValues(this.telemetryCollectionRunData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'TelemetryCollectionRun loaded successfully',
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
        this.handleTelemetryCollectionRunLoadError(error, telemetryCollectionRunId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleTelemetryCollectionRunNotFound(telemetryCollectionRunId: number): void {

    this.telemetryCollectionRunData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `TelemetryCollectionRun #${telemetryCollectionRunId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleTelemetryCollectionRunLoadError(error: any, telemetryCollectionRunId: number): void {

    let message = 'Failed to load Telemetry Collection Run.';
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
          message = 'You do not have permission to view this Telemetry Collection Run.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Telemetry Collection Run #${telemetryCollectionRunId} was not found.`;
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

    console.error(`Telemetry Collection Run load failed (ID: ${telemetryCollectionRunId})`, error);

    //
    // Reset UI to safe state
    //
    this.telemetryCollectionRunData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(telemetryCollectionRunData: TelemetryCollectionRunData | null) {

    if (telemetryCollectionRunData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetryCollectionRunForm.reset({
        startTime: '',
        endTime: '',
        applicationsPolled: '',
        applicationsSucceeded: '',
        errorMessage: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetryCollectionRunForm.reset({
        startTime: isoUtcStringToDateTimeLocal(telemetryCollectionRunData.startTime) ?? '',
        endTime: isoUtcStringToDateTimeLocal(telemetryCollectionRunData.endTime) ?? '',
        applicationsPolled: telemetryCollectionRunData.applicationsPolled?.toString() ?? '',
        applicationsSucceeded: telemetryCollectionRunData.applicationsSucceeded?.toString() ?? '',
        errorMessage: telemetryCollectionRunData.errorMessage ?? '',
      }, { emitEvent: false});
    }

    this.telemetryCollectionRunForm.markAsPristine();
    this.telemetryCollectionRunForm.markAsUntouched();
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

    if (this.telemetryCollectionRunService.userIsTelemetryTelemetryCollectionRunWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Telemetry Collection Runs", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.telemetryCollectionRunForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetryCollectionRunForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetryCollectionRunForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetryCollectionRunSubmitData: TelemetryCollectionRunSubmitData = {
        id: this.telemetryCollectionRunData?.id || 0,
        startTime: dateTimeLocalToIsoUtc(formValue.startTime!.trim())!,
        endTime: formValue.endTime ? dateTimeLocalToIsoUtc(formValue.endTime.trim()) : null,
        applicationsPolled: formValue.applicationsPolled ? Number(formValue.applicationsPolled) : null,
        applicationsSucceeded: formValue.applicationsSucceeded ? Number(formValue.applicationsSucceeded) : null,
        errorMessage: formValue.errorMessage?.trim() || null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.telemetryCollectionRunService.PutTelemetryCollectionRun(telemetryCollectionRunSubmitData.id, telemetryCollectionRunSubmitData)
      : this.telemetryCollectionRunService.PostTelemetryCollectionRun(telemetryCollectionRunSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedTelemetryCollectionRunData) => {

        this.telemetryCollectionRunService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Telemetry Collection Run's detail page
          //
          this.telemetryCollectionRunForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.telemetryCollectionRunForm.markAsUntouched();

          this.router.navigate(['/telemetrycollectionruns', savedTelemetryCollectionRunData.id]);
          this.alertService.showMessage('Telemetry Collection Run added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.telemetryCollectionRunData = savedTelemetryCollectionRunData;
          this.buildFormValues(this.telemetryCollectionRunData);

          this.alertService.showMessage("Telemetry Collection Run saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Telemetry Collection Run.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Collection Run.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Collection Run could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsTelemetryTelemetryCollectionRunReader(): boolean {
    return this.telemetryCollectionRunService.userIsTelemetryTelemetryCollectionRunReader();
  }

  public userIsTelemetryTelemetryCollectionRunWriter(): boolean {
    return this.telemetryCollectionRunService.userIsTelemetryTelemetryCollectionRunWriter();
  }
}
