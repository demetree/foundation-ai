/*
   GENERATED FORM FOR THE INCIDENTSTATUSTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IncidentStatusType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to incident-status-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IncidentStatusTypeService, IncidentStatusTypeData, IncidentStatusTypeSubmitData } from '../../../alerting-data-services/incident-status-type.service';
import { IncidentService } from '../../../alerting-data-services/incident.service';
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
interface IncidentStatusTypeFormValues {
  name: string,
  description: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-incident-status-type-detail',
  templateUrl: './incident-status-type-detail.component.html',
  styleUrls: ['./incident-status-type-detail.component.scss']
})

export class IncidentStatusTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IncidentStatusTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public incidentStatusTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        active: [true],
        deleted: [false],
      });


  public incidentStatusTypeId: string | null = null;
  public incidentStatusTypeData: IncidentStatusTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  incidentStatusTypes$ = this.incidentStatusTypeService.GetIncidentStatusTypeList();
  public incidents$ = this.incidentService.GetIncidentList();

  private destroy$ = new Subject<void>();

  constructor(
    public incidentStatusTypeService: IncidentStatusTypeService,
    public incidentService: IncidentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the incidentStatusTypeId from the route parameters
    this.incidentStatusTypeId = this.route.snapshot.paramMap.get('incidentStatusTypeId');

    if (this.incidentStatusTypeId === 'new' ||
        this.incidentStatusTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.incidentStatusTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.incidentStatusTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.incidentStatusTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Incident Status Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Incident Status Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.incidentStatusTypeForm.dirty) {
      return confirm('You have unsaved Incident Status Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.incidentStatusTypeId != null && this.incidentStatusTypeId !== 'new') {

      const id = parseInt(this.incidentStatusTypeId, 10);

      if (!isNaN(id)) {
        return { incidentStatusTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the IncidentStatusType data for the current incidentStatusTypeId.
  *
  * Fully respects the IncidentStatusTypeService caching strategy and error handling strategy.
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
    if (!this.incidentStatusTypeService.userIsAlertingIncidentStatusTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read IncidentStatusTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate incidentStatusTypeId
    //
    if (!this.incidentStatusTypeId) {

      this.alertService.showMessage('No IncidentStatusType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const incidentStatusTypeId = Number(this.incidentStatusTypeId);

    if (isNaN(incidentStatusTypeId) || incidentStatusTypeId <= 0) {

      this.alertService.showMessage(`Invalid Incident Status Type ID: "${this.incidentStatusTypeId}"`,
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
      // This is the most targeted way: clear only this IncidentStatusType + relations

      this.incidentStatusTypeService.ClearRecordCache(incidentStatusTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.incidentStatusTypeService.GetIncidentStatusType(incidentStatusTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (incidentStatusTypeData) => {

        //
        // Success path — incidentStatusTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!incidentStatusTypeData) {

          this.handleIncidentStatusTypeNotFound(incidentStatusTypeId);

        } else {

          this.incidentStatusTypeData = incidentStatusTypeData;
          this.buildFormValues(this.incidentStatusTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'IncidentStatusType loaded successfully',
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
        this.handleIncidentStatusTypeLoadError(error, incidentStatusTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleIncidentStatusTypeNotFound(incidentStatusTypeId: number): void {

    this.incidentStatusTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `IncidentStatusType #${incidentStatusTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleIncidentStatusTypeLoadError(error: any, incidentStatusTypeId: number): void {

    let message = 'Failed to load Incident Status Type.';
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
          message = 'You do not have permission to view this Incident Status Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Incident Status Type #${incidentStatusTypeId} was not found.`;
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

    console.error(`Incident Status Type load failed (ID: ${incidentStatusTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.incidentStatusTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(incidentStatusTypeData: IncidentStatusTypeData | null) {

    if (incidentStatusTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.incidentStatusTypeForm.reset({
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
        this.incidentStatusTypeForm.reset({
        name: incidentStatusTypeData.name ?? '',
        description: incidentStatusTypeData.description ?? '',
        active: incidentStatusTypeData.active ?? true,
        deleted: incidentStatusTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.incidentStatusTypeForm.markAsPristine();
    this.incidentStatusTypeForm.markAsUntouched();
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

    if (this.incidentStatusTypeService.userIsAlertingIncidentStatusTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Incident Status Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.incidentStatusTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.incidentStatusTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.incidentStatusTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const incidentStatusTypeSubmitData: IncidentStatusTypeSubmitData = {
        id: this.incidentStatusTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.incidentStatusTypeService.PutIncidentStatusType(incidentStatusTypeSubmitData.id, incidentStatusTypeSubmitData)
      : this.incidentStatusTypeService.PostIncidentStatusType(incidentStatusTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedIncidentStatusTypeData) => {

        this.incidentStatusTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Incident Status Type's detail page
          //
          this.incidentStatusTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.incidentStatusTypeForm.markAsUntouched();

          this.router.navigate(['/incidentstatustypes', savedIncidentStatusTypeData.id]);
          this.alertService.showMessage('Incident Status Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.incidentStatusTypeData = savedIncidentStatusTypeData;
          this.buildFormValues(this.incidentStatusTypeData);

          this.alertService.showMessage("Incident Status Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Incident Status Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident Status Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident Status Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAlertingIncidentStatusTypeReader(): boolean {
    return this.incidentStatusTypeService.userIsAlertingIncidentStatusTypeReader();
  }

  public userIsAlertingIncidentStatusTypeWriter(): boolean {
    return this.incidentStatusTypeService.userIsAlertingIncidentStatusTypeWriter();
  }
}
