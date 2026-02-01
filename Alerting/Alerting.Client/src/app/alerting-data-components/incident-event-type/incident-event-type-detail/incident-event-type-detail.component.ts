/*
   GENERATED FORM FOR THE INCIDENTEVENTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IncidentEventType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to incident-event-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IncidentEventTypeService, IncidentEventTypeData, IncidentEventTypeSubmitData } from '../../../alerting-data-services/incident-event-type.service';
import { IncidentTimelineEventService } from '../../../alerting-data-services/incident-timeline-event.service';
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
interface IncidentEventTypeFormValues {
  name: string,
  description: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-incident-event-type-detail',
  templateUrl: './incident-event-type-detail.component.html',
  styleUrls: ['./incident-event-type-detail.component.scss']
})

export class IncidentEventTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IncidentEventTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public incidentEventTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        active: [true],
        deleted: [false],
      });


  public incidentEventTypeId: string | null = null;
  public incidentEventTypeData: IncidentEventTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  incidentEventTypes$ = this.incidentEventTypeService.GetIncidentEventTypeList();
  public incidentTimelineEvents$ = this.incidentTimelineEventService.GetIncidentTimelineEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public incidentEventTypeService: IncidentEventTypeService,
    public incidentTimelineEventService: IncidentTimelineEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the incidentEventTypeId from the route parameters
    this.incidentEventTypeId = this.route.snapshot.paramMap.get('incidentEventTypeId');

    if (this.incidentEventTypeId === 'new' ||
        this.incidentEventTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.incidentEventTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.incidentEventTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.incidentEventTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Incident Event Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Incident Event Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.incidentEventTypeForm.dirty) {
      return confirm('You have unsaved Incident Event Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.incidentEventTypeId != null && this.incidentEventTypeId !== 'new') {

      const id = parseInt(this.incidentEventTypeId, 10);

      if (!isNaN(id)) {
        return { incidentEventTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the IncidentEventType data for the current incidentEventTypeId.
  *
  * Fully respects the IncidentEventTypeService caching strategy and error handling strategy.
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
    if (!this.incidentEventTypeService.userIsAlertingIncidentEventTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read IncidentEventTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate incidentEventTypeId
    //
    if (!this.incidentEventTypeId) {

      this.alertService.showMessage('No IncidentEventType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const incidentEventTypeId = Number(this.incidentEventTypeId);

    if (isNaN(incidentEventTypeId) || incidentEventTypeId <= 0) {

      this.alertService.showMessage(`Invalid Incident Event Type ID: "${this.incidentEventTypeId}"`,
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
      // This is the most targeted way: clear only this IncidentEventType + relations

      this.incidentEventTypeService.ClearRecordCache(incidentEventTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.incidentEventTypeService.GetIncidentEventType(incidentEventTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (incidentEventTypeData) => {

        //
        // Success path — incidentEventTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!incidentEventTypeData) {

          this.handleIncidentEventTypeNotFound(incidentEventTypeId);

        } else {

          this.incidentEventTypeData = incidentEventTypeData;
          this.buildFormValues(this.incidentEventTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'IncidentEventType loaded successfully',
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
        this.handleIncidentEventTypeLoadError(error, incidentEventTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleIncidentEventTypeNotFound(incidentEventTypeId: number): void {

    this.incidentEventTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `IncidentEventType #${incidentEventTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleIncidentEventTypeLoadError(error: any, incidentEventTypeId: number): void {

    let message = 'Failed to load Incident Event Type.';
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
          message = 'You do not have permission to view this Incident Event Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Incident Event Type #${incidentEventTypeId} was not found.`;
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

    console.error(`Incident Event Type load failed (ID: ${incidentEventTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.incidentEventTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(incidentEventTypeData: IncidentEventTypeData | null) {

    if (incidentEventTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.incidentEventTypeForm.reset({
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
        this.incidentEventTypeForm.reset({
        name: incidentEventTypeData.name ?? '',
        description: incidentEventTypeData.description ?? '',
        active: incidentEventTypeData.active ?? true,
        deleted: incidentEventTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.incidentEventTypeForm.markAsPristine();
    this.incidentEventTypeForm.markAsUntouched();
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

    if (this.incidentEventTypeService.userIsAlertingIncidentEventTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Incident Event Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.incidentEventTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.incidentEventTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.incidentEventTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const incidentEventTypeSubmitData: IncidentEventTypeSubmitData = {
        id: this.incidentEventTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.incidentEventTypeService.PutIncidentEventType(incidentEventTypeSubmitData.id, incidentEventTypeSubmitData)
      : this.incidentEventTypeService.PostIncidentEventType(incidentEventTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedIncidentEventTypeData) => {

        this.incidentEventTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Incident Event Type's detail page
          //
          this.incidentEventTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.incidentEventTypeForm.markAsUntouched();

          this.router.navigate(['/incidenteventtypes', savedIncidentEventTypeData.id]);
          this.alertService.showMessage('Incident Event Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.incidentEventTypeData = savedIncidentEventTypeData;
          this.buildFormValues(this.incidentEventTypeData);

          this.alertService.showMessage("Incident Event Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Incident Event Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident Event Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident Event Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAlertingIncidentEventTypeReader(): boolean {
    return this.incidentEventTypeService.userIsAlertingIncidentEventTypeReader();
  }

  public userIsAlertingIncidentEventTypeWriter(): boolean {
    return this.incidentEventTypeService.userIsAlertingIncidentEventTypeWriter();
  }
}
