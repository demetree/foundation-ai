/*
   GENERATED FORM FOR THE TELEMETRYAPPLICATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryApplication table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-application-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetryApplicationService, TelemetryApplicationData, TelemetryApplicationSubmitData } from '../../../telemetry-data-services/telemetry-application.service';
import { TelemetrySnapshotService } from '../../../telemetry-data-services/telemetry-snapshot.service';
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
interface TelemetryApplicationFormValues {
  name: string,
  url: string | null,
  isSelf: boolean,
  firstSeen: string,
  lastSeen: string | null,
};


@Component({
  selector: 'app-telemetry-application-detail',
  templateUrl: './telemetry-application-detail.component.html',
  styleUrls: ['./telemetry-application-detail.component.scss']
})

export class TelemetryApplicationDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetryApplicationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetryApplicationForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        url: [''],
        isSelf: [false],
        firstSeen: ['', Validators.required],
        lastSeen: [''],
      });


  public telemetryApplicationId: string | null = null;
  public telemetryApplicationData: TelemetryApplicationData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  telemetryApplications$ = this.telemetryApplicationService.GetTelemetryApplicationList();
  public telemetrySnapshots$ = this.telemetrySnapshotService.GetTelemetrySnapshotList();
  public telemetryErrorEvents$ = this.telemetryErrorEventService.GetTelemetryErrorEventList();
  public telemetryLogErrors$ = this.telemetryLogErrorService.GetTelemetryLogErrorList();

  private destroy$ = new Subject<void>();

  constructor(
    public telemetryApplicationService: TelemetryApplicationService,
    public telemetrySnapshotService: TelemetrySnapshotService,
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

    // Get the telemetryApplicationId from the route parameters
    this.telemetryApplicationId = this.route.snapshot.paramMap.get('telemetryApplicationId');

    if (this.telemetryApplicationId === 'new' ||
        this.telemetryApplicationId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.telemetryApplicationData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.telemetryApplicationForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetryApplicationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Telemetry Application';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Telemetry Application';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.telemetryApplicationForm.dirty) {
      return confirm('You have unsaved Telemetry Application changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.telemetryApplicationId != null && this.telemetryApplicationId !== 'new') {

      const id = parseInt(this.telemetryApplicationId, 10);

      if (!isNaN(id)) {
        return { telemetryApplicationId: id };
      }
    }

    return null;
  }


/*
  * Loads the TelemetryApplication data for the current telemetryApplicationId.
  *
  * Fully respects the TelemetryApplicationService caching strategy and error handling strategy.
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
    if (!this.telemetryApplicationService.userIsTelemetryTelemetryApplicationReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read TelemetryApplications.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate telemetryApplicationId
    //
    if (!this.telemetryApplicationId) {

      this.alertService.showMessage('No TelemetryApplication ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const telemetryApplicationId = Number(this.telemetryApplicationId);

    if (isNaN(telemetryApplicationId) || telemetryApplicationId <= 0) {

      this.alertService.showMessage(`Invalid Telemetry Application ID: "${this.telemetryApplicationId}"`,
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
      // This is the most targeted way: clear only this TelemetryApplication + relations

      this.telemetryApplicationService.ClearRecordCache(telemetryApplicationId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.telemetryApplicationService.GetTelemetryApplication(telemetryApplicationId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (telemetryApplicationData) => {

        //
        // Success path — telemetryApplicationData can legitimately be null if 404'd but request succeeded
        //
        if (!telemetryApplicationData) {

          this.handleTelemetryApplicationNotFound(telemetryApplicationId);

        } else {

          this.telemetryApplicationData = telemetryApplicationData;
          this.buildFormValues(this.telemetryApplicationData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'TelemetryApplication loaded successfully',
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
        this.handleTelemetryApplicationLoadError(error, telemetryApplicationId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleTelemetryApplicationNotFound(telemetryApplicationId: number): void {

    this.telemetryApplicationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `TelemetryApplication #${telemetryApplicationId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleTelemetryApplicationLoadError(error: any, telemetryApplicationId: number): void {

    let message = 'Failed to load Telemetry Application.';
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
          message = 'You do not have permission to view this Telemetry Application.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Telemetry Application #${telemetryApplicationId} was not found.`;
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

    console.error(`Telemetry Application load failed (ID: ${telemetryApplicationId})`, error);

    //
    // Reset UI to safe state
    //
    this.telemetryApplicationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(telemetryApplicationData: TelemetryApplicationData | null) {

    if (telemetryApplicationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetryApplicationForm.reset({
        name: '',
        url: '',
        isSelf: false,
        firstSeen: '',
        lastSeen: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetryApplicationForm.reset({
        name: telemetryApplicationData.name ?? '',
        url: telemetryApplicationData.url ?? '',
        isSelf: telemetryApplicationData.isSelf ?? false,
        firstSeen: isoUtcStringToDateTimeLocal(telemetryApplicationData.firstSeen) ?? '',
        lastSeen: isoUtcStringToDateTimeLocal(telemetryApplicationData.lastSeen) ?? '',
      }, { emitEvent: false});
    }

    this.telemetryApplicationForm.markAsPristine();
    this.telemetryApplicationForm.markAsUntouched();
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

    if (this.telemetryApplicationService.userIsTelemetryTelemetryApplicationWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Telemetry Applications", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.telemetryApplicationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetryApplicationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetryApplicationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetryApplicationSubmitData: TelemetryApplicationSubmitData = {
        id: this.telemetryApplicationData?.id || 0,
        name: formValue.name!.trim(),
        url: formValue.url?.trim() || null,
        isSelf: !!formValue.isSelf,
        firstSeen: dateTimeLocalToIsoUtc(formValue.firstSeen!.trim())!,
        lastSeen: formValue.lastSeen ? dateTimeLocalToIsoUtc(formValue.lastSeen.trim()) : null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.telemetryApplicationService.PutTelemetryApplication(telemetryApplicationSubmitData.id, telemetryApplicationSubmitData)
      : this.telemetryApplicationService.PostTelemetryApplication(telemetryApplicationSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedTelemetryApplicationData) => {

        this.telemetryApplicationService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Telemetry Application's detail page
          //
          this.telemetryApplicationForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.telemetryApplicationForm.markAsUntouched();

          this.router.navigate(['/telemetryapplications', savedTelemetryApplicationData.id]);
          this.alertService.showMessage('Telemetry Application added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.telemetryApplicationData = savedTelemetryApplicationData;
          this.buildFormValues(this.telemetryApplicationData);

          this.alertService.showMessage("Telemetry Application saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Telemetry Application.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Application.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Application could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsTelemetryTelemetryApplicationReader(): boolean {
    return this.telemetryApplicationService.userIsTelemetryTelemetryApplicationReader();
  }

  public userIsTelemetryTelemetryApplicationWriter(): boolean {
    return this.telemetryApplicationService.userIsTelemetryTelemetryApplicationWriter();
  }
}
