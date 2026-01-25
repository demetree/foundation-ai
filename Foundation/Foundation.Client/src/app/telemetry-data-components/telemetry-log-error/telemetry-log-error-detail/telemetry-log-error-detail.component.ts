/*
   GENERATED FORM FOR THE TELEMETRYLOGERROR TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryLogError table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-log-error-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetryLogErrorService, TelemetryLogErrorData, TelemetryLogErrorSubmitData } from '../../../telemetry-data-services/telemetry-log-error.service';
import { TelemetryApplicationService } from '../../../telemetry-data-services/telemetry-application.service';
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
interface TelemetryLogErrorFormValues {
  telemetryApplicationId: number | bigint,       // For FK link number
  telemetrySnapshotId: number | bigint | null,       // For FK link number
  capturedAt: string,
  logFileName: string | null,
  logTimestamp: string | null,
  level: string | null,
  message: string | null,
  exception: string | null,
};


@Component({
  selector: 'app-telemetry-log-error-detail',
  templateUrl: './telemetry-log-error-detail.component.html',
  styleUrls: ['./telemetry-log-error-detail.component.scss']
})

export class TelemetryLogErrorDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetryLogErrorFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetryLogErrorForm: FormGroup = this.fb.group({
        telemetryApplicationId: [null, Validators.required],
        telemetrySnapshotId: [null],
        capturedAt: ['', Validators.required],
        logFileName: [''],
        logTimestamp: [''],
        level: [''],
        message: [''],
        exception: [''],
      });


  public telemetryLogErrorId: string | null = null;
  public telemetryLogErrorData: TelemetryLogErrorData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  telemetryLogErrors$ = this.telemetryLogErrorService.GetTelemetryLogErrorList();
  public telemetryApplications$ = this.telemetryApplicationService.GetTelemetryApplicationList();
  public telemetrySnapshots$ = this.telemetrySnapshotService.GetTelemetrySnapshotList();

  private destroy$ = new Subject<void>();

  constructor(
    public telemetryLogErrorService: TelemetryLogErrorService,
    public telemetryApplicationService: TelemetryApplicationService,
    public telemetrySnapshotService: TelemetrySnapshotService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the telemetryLogErrorId from the route parameters
    this.telemetryLogErrorId = this.route.snapshot.paramMap.get('telemetryLogErrorId');

    if (this.telemetryLogErrorId === 'new' ||
        this.telemetryLogErrorId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.telemetryLogErrorData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.telemetryLogErrorForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetryLogErrorForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Telemetry Log Error';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Telemetry Log Error';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.telemetryLogErrorForm.dirty) {
      return confirm('You have unsaved Telemetry Log Error changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.telemetryLogErrorId != null && this.telemetryLogErrorId !== 'new') {

      const id = parseInt(this.telemetryLogErrorId, 10);

      if (!isNaN(id)) {
        return { telemetryLogErrorId: id };
      }
    }

    return null;
  }


/*
  * Loads the TelemetryLogError data for the current telemetryLogErrorId.
  *
  * Fully respects the TelemetryLogErrorService caching strategy and error handling strategy.
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
    if (!this.telemetryLogErrorService.userIsTelemetryTelemetryLogErrorReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read TelemetryLogErrors.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate telemetryLogErrorId
    //
    if (!this.telemetryLogErrorId) {

      this.alertService.showMessage('No TelemetryLogError ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const telemetryLogErrorId = Number(this.telemetryLogErrorId);

    if (isNaN(telemetryLogErrorId) || telemetryLogErrorId <= 0) {

      this.alertService.showMessage(`Invalid Telemetry Log Error ID: "${this.telemetryLogErrorId}"`,
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
      // This is the most targeted way: clear only this TelemetryLogError + relations

      this.telemetryLogErrorService.ClearRecordCache(telemetryLogErrorId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.telemetryLogErrorService.GetTelemetryLogError(telemetryLogErrorId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (telemetryLogErrorData) => {

        //
        // Success path — telemetryLogErrorData can legitimately be null if 404'd but request succeeded
        //
        if (!telemetryLogErrorData) {

          this.handleTelemetryLogErrorNotFound(telemetryLogErrorId);

        } else {

          this.telemetryLogErrorData = telemetryLogErrorData;
          this.buildFormValues(this.telemetryLogErrorData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'TelemetryLogError loaded successfully',
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
        this.handleTelemetryLogErrorLoadError(error, telemetryLogErrorId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleTelemetryLogErrorNotFound(telemetryLogErrorId: number): void {

    this.telemetryLogErrorData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `TelemetryLogError #${telemetryLogErrorId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleTelemetryLogErrorLoadError(error: any, telemetryLogErrorId: number): void {

    let message = 'Failed to load Telemetry Log Error.';
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
          message = 'You do not have permission to view this Telemetry Log Error.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Telemetry Log Error #${telemetryLogErrorId} was not found.`;
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

    console.error(`Telemetry Log Error load failed (ID: ${telemetryLogErrorId})`, error);

    //
    // Reset UI to safe state
    //
    this.telemetryLogErrorData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(telemetryLogErrorData: TelemetryLogErrorData | null) {

    if (telemetryLogErrorData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetryLogErrorForm.reset({
        telemetryApplicationId: null,
        telemetrySnapshotId: null,
        capturedAt: '',
        logFileName: '',
        logTimestamp: '',
        level: '',
        message: '',
        exception: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetryLogErrorForm.reset({
        telemetryApplicationId: telemetryLogErrorData.telemetryApplicationId,
        telemetrySnapshotId: telemetryLogErrorData.telemetrySnapshotId,
        capturedAt: isoUtcStringToDateTimeLocal(telemetryLogErrorData.capturedAt) ?? '',
        logFileName: telemetryLogErrorData.logFileName ?? '',
        logTimestamp: isoUtcStringToDateTimeLocal(telemetryLogErrorData.logTimestamp) ?? '',
        level: telemetryLogErrorData.level ?? '',
        message: telemetryLogErrorData.message ?? '',
        exception: telemetryLogErrorData.exception ?? '',
      }, { emitEvent: false});
    }

    this.telemetryLogErrorForm.markAsPristine();
    this.telemetryLogErrorForm.markAsUntouched();
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

    if (this.telemetryLogErrorService.userIsTelemetryTelemetryLogErrorWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Telemetry Log Errors", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.telemetryLogErrorForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetryLogErrorForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetryLogErrorForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetryLogErrorSubmitData: TelemetryLogErrorSubmitData = {
        id: this.telemetryLogErrorData?.id || 0,
        telemetryApplicationId: Number(formValue.telemetryApplicationId),
        telemetrySnapshotId: formValue.telemetrySnapshotId ? Number(formValue.telemetrySnapshotId) : null,
        capturedAt: dateTimeLocalToIsoUtc(formValue.capturedAt!.trim())!,
        logFileName: formValue.logFileName?.trim() || null,
        logTimestamp: formValue.logTimestamp ? dateTimeLocalToIsoUtc(formValue.logTimestamp.trim()) : null,
        level: formValue.level?.trim() || null,
        message: formValue.message?.trim() || null,
        exception: formValue.exception?.trim() || null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.telemetryLogErrorService.PutTelemetryLogError(telemetryLogErrorSubmitData.id, telemetryLogErrorSubmitData)
      : this.telemetryLogErrorService.PostTelemetryLogError(telemetryLogErrorSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedTelemetryLogErrorData) => {

        this.telemetryLogErrorService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Telemetry Log Error's detail page
          //
          this.telemetryLogErrorForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.telemetryLogErrorForm.markAsUntouched();

          this.router.navigate(['/telemetrylogerrors', savedTelemetryLogErrorData.id]);
          this.alertService.showMessage('Telemetry Log Error added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.telemetryLogErrorData = savedTelemetryLogErrorData;
          this.buildFormValues(this.telemetryLogErrorData);

          this.alertService.showMessage("Telemetry Log Error saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Telemetry Log Error.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Log Error.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Log Error could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsTelemetryTelemetryLogErrorReader(): boolean {
    return this.telemetryLogErrorService.userIsTelemetryTelemetryLogErrorReader();
  }

  public userIsTelemetryTelemetryLogErrorWriter(): boolean {
    return this.telemetryLogErrorService.userIsTelemetryTelemetryLogErrorWriter();
  }
}
