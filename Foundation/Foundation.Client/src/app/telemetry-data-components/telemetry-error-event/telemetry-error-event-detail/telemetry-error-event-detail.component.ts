/*
   GENERATED FORM FOR THE TELEMETRYERROREVENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryErrorEvent table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-error-event-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetryErrorEventService, TelemetryErrorEventData, TelemetryErrorEventSubmitData } from '../../../telemetry-data-services/telemetry-error-event.service';
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
interface TelemetryErrorEventFormValues {
  telemetryApplicationId: number | bigint,       // For FK link number
  telemetrySnapshotId: number | bigint | null,       // For FK link number
  originalAuditEventId: string | null,     // Stored as string for form input, converted to number on submit.
  occurredAt: string,
  auditTypeName: string | null,
  moduleName: string | null,
  entityName: string | null,
  userName: string | null,
  message: string | null,
  errorMessage: string | null,
};


@Component({
  selector: 'app-telemetry-error-event-detail',
  templateUrl: './telemetry-error-event-detail.component.html',
  styleUrls: ['./telemetry-error-event-detail.component.scss']
})

export class TelemetryErrorEventDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetryErrorEventFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetryErrorEventForm: FormGroup = this.fb.group({
        telemetryApplicationId: [null, Validators.required],
        telemetrySnapshotId: [null],
        originalAuditEventId: [''],
        occurredAt: ['', Validators.required],
        auditTypeName: [''],
        moduleName: [''],
        entityName: [''],
        userName: [''],
        message: [''],
        errorMessage: [''],
      });


  public telemetryErrorEventId: string | null = null;
  public telemetryErrorEventData: TelemetryErrorEventData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  telemetryErrorEvents$ = this.telemetryErrorEventService.GetTelemetryErrorEventList();
  public telemetryApplications$ = this.telemetryApplicationService.GetTelemetryApplicationList();
  public telemetrySnapshots$ = this.telemetrySnapshotService.GetTelemetrySnapshotList();

  private destroy$ = new Subject<void>();

  constructor(
    public telemetryErrorEventService: TelemetryErrorEventService,
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

    // Get the telemetryErrorEventId from the route parameters
    this.telemetryErrorEventId = this.route.snapshot.paramMap.get('telemetryErrorEventId');

    if (this.telemetryErrorEventId === 'new' ||
        this.telemetryErrorEventId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.telemetryErrorEventData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.telemetryErrorEventForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetryErrorEventForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Telemetry Error Event';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Telemetry Error Event';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.telemetryErrorEventForm.dirty) {
      return confirm('You have unsaved Telemetry Error Event changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.telemetryErrorEventId != null && this.telemetryErrorEventId !== 'new') {

      const id = parseInt(this.telemetryErrorEventId, 10);

      if (!isNaN(id)) {
        return { telemetryErrorEventId: id };
      }
    }

    return null;
  }


/*
  * Loads the TelemetryErrorEvent data for the current telemetryErrorEventId.
  *
  * Fully respects the TelemetryErrorEventService caching strategy and error handling strategy.
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
    if (!this.telemetryErrorEventService.userIsTelemetryTelemetryErrorEventReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read TelemetryErrorEvents.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate telemetryErrorEventId
    //
    if (!this.telemetryErrorEventId) {

      this.alertService.showMessage('No TelemetryErrorEvent ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const telemetryErrorEventId = Number(this.telemetryErrorEventId);

    if (isNaN(telemetryErrorEventId) || telemetryErrorEventId <= 0) {

      this.alertService.showMessage(`Invalid Telemetry Error Event ID: "${this.telemetryErrorEventId}"`,
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
      // This is the most targeted way: clear only this TelemetryErrorEvent + relations

      this.telemetryErrorEventService.ClearRecordCache(telemetryErrorEventId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.telemetryErrorEventService.GetTelemetryErrorEvent(telemetryErrorEventId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (telemetryErrorEventData) => {

        //
        // Success path — telemetryErrorEventData can legitimately be null if 404'd but request succeeded
        //
        if (!telemetryErrorEventData) {

          this.handleTelemetryErrorEventNotFound(telemetryErrorEventId);

        } else {

          this.telemetryErrorEventData = telemetryErrorEventData;
          this.buildFormValues(this.telemetryErrorEventData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'TelemetryErrorEvent loaded successfully',
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
        this.handleTelemetryErrorEventLoadError(error, telemetryErrorEventId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleTelemetryErrorEventNotFound(telemetryErrorEventId: number): void {

    this.telemetryErrorEventData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `TelemetryErrorEvent #${telemetryErrorEventId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleTelemetryErrorEventLoadError(error: any, telemetryErrorEventId: number): void {

    let message = 'Failed to load Telemetry Error Event.';
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
          message = 'You do not have permission to view this Telemetry Error Event.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Telemetry Error Event #${telemetryErrorEventId} was not found.`;
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

    console.error(`Telemetry Error Event load failed (ID: ${telemetryErrorEventId})`, error);

    //
    // Reset UI to safe state
    //
    this.telemetryErrorEventData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(telemetryErrorEventData: TelemetryErrorEventData | null) {

    if (telemetryErrorEventData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetryErrorEventForm.reset({
        telemetryApplicationId: null,
        telemetrySnapshotId: null,
        originalAuditEventId: '',
        occurredAt: '',
        auditTypeName: '',
        moduleName: '',
        entityName: '',
        userName: '',
        message: '',
        errorMessage: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetryErrorEventForm.reset({
        telemetryApplicationId: telemetryErrorEventData.telemetryApplicationId,
        telemetrySnapshotId: telemetryErrorEventData.telemetrySnapshotId,
        originalAuditEventId: telemetryErrorEventData.originalAuditEventId?.toString() ?? '',
        occurredAt: isoUtcStringToDateTimeLocal(telemetryErrorEventData.occurredAt) ?? '',
        auditTypeName: telemetryErrorEventData.auditTypeName ?? '',
        moduleName: telemetryErrorEventData.moduleName ?? '',
        entityName: telemetryErrorEventData.entityName ?? '',
        userName: telemetryErrorEventData.userName ?? '',
        message: telemetryErrorEventData.message ?? '',
        errorMessage: telemetryErrorEventData.errorMessage ?? '',
      }, { emitEvent: false});
    }

    this.telemetryErrorEventForm.markAsPristine();
    this.telemetryErrorEventForm.markAsUntouched();
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

    if (this.telemetryErrorEventService.userIsTelemetryTelemetryErrorEventWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Telemetry Error Events", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.telemetryErrorEventForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetryErrorEventForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetryErrorEventForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetryErrorEventSubmitData: TelemetryErrorEventSubmitData = {
        id: this.telemetryErrorEventData?.id || 0,
        telemetryApplicationId: Number(formValue.telemetryApplicationId),
        telemetrySnapshotId: formValue.telemetrySnapshotId ? Number(formValue.telemetrySnapshotId) : null,
        originalAuditEventId: formValue.originalAuditEventId ? Number(formValue.originalAuditEventId) : null,
        occurredAt: dateTimeLocalToIsoUtc(formValue.occurredAt!.trim())!,
        auditTypeName: formValue.auditTypeName?.trim() || null,
        moduleName: formValue.moduleName?.trim() || null,
        entityName: formValue.entityName?.trim() || null,
        userName: formValue.userName?.trim() || null,
        message: formValue.message?.trim() || null,
        errorMessage: formValue.errorMessage?.trim() || null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.telemetryErrorEventService.PutTelemetryErrorEvent(telemetryErrorEventSubmitData.id, telemetryErrorEventSubmitData)
      : this.telemetryErrorEventService.PostTelemetryErrorEvent(telemetryErrorEventSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedTelemetryErrorEventData) => {

        this.telemetryErrorEventService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Telemetry Error Event's detail page
          //
          this.telemetryErrorEventForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.telemetryErrorEventForm.markAsUntouched();

          this.router.navigate(['/telemetryerrorevents', savedTelemetryErrorEventData.id]);
          this.alertService.showMessage('Telemetry Error Event added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.telemetryErrorEventData = savedTelemetryErrorEventData;
          this.buildFormValues(this.telemetryErrorEventData);

          this.alertService.showMessage("Telemetry Error Event saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Telemetry Error Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Error Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Error Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsTelemetryTelemetryErrorEventReader(): boolean {
    return this.telemetryErrorEventService.userIsTelemetryTelemetryErrorEventReader();
  }

  public userIsTelemetryTelemetryErrorEventWriter(): boolean {
    return this.telemetryErrorEventService.userIsTelemetryTelemetryErrorEventWriter();
  }
}
