/*
   GENERATED FORM FOR THE INCIDENTNOTECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IncidentNoteChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to incident-note-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IncidentNoteChangeHistoryService, IncidentNoteChangeHistoryData, IncidentNoteChangeHistorySubmitData } from '../../../alerting-data-services/incident-note-change-history.service';
import { IncidentNoteService } from '../../../alerting-data-services/incident-note.service';
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
interface IncidentNoteChangeHistoryFormValues {
  incidentNoteId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-incident-note-change-history-detail',
  templateUrl: './incident-note-change-history-detail.component.html',
  styleUrls: ['./incident-note-change-history-detail.component.scss']
})

export class IncidentNoteChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IncidentNoteChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public incidentNoteChangeHistoryForm: FormGroup = this.fb.group({
        incidentNoteId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public incidentNoteChangeHistoryId: string | null = null;
  public incidentNoteChangeHistoryData: IncidentNoteChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  incidentNoteChangeHistories$ = this.incidentNoteChangeHistoryService.GetIncidentNoteChangeHistoryList();
  public incidentNotes$ = this.incidentNoteService.GetIncidentNoteList();

  private destroy$ = new Subject<void>();

  constructor(
    public incidentNoteChangeHistoryService: IncidentNoteChangeHistoryService,
    public incidentNoteService: IncidentNoteService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the incidentNoteChangeHistoryId from the route parameters
    this.incidentNoteChangeHistoryId = this.route.snapshot.paramMap.get('incidentNoteChangeHistoryId');

    if (this.incidentNoteChangeHistoryId === 'new' ||
        this.incidentNoteChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.incidentNoteChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.incidentNoteChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.incidentNoteChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Incident Note Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Incident Note Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.incidentNoteChangeHistoryForm.dirty) {
      return confirm('You have unsaved Incident Note Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.incidentNoteChangeHistoryId != null && this.incidentNoteChangeHistoryId !== 'new') {

      const id = parseInt(this.incidentNoteChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { incidentNoteChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the IncidentNoteChangeHistory data for the current incidentNoteChangeHistoryId.
  *
  * Fully respects the IncidentNoteChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.incidentNoteChangeHistoryService.userIsAlertingIncidentNoteChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read IncidentNoteChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate incidentNoteChangeHistoryId
    //
    if (!this.incidentNoteChangeHistoryId) {

      this.alertService.showMessage('No IncidentNoteChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const incidentNoteChangeHistoryId = Number(this.incidentNoteChangeHistoryId);

    if (isNaN(incidentNoteChangeHistoryId) || incidentNoteChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Incident Note Change History ID: "${this.incidentNoteChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this IncidentNoteChangeHistory + relations

      this.incidentNoteChangeHistoryService.ClearRecordCache(incidentNoteChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.incidentNoteChangeHistoryService.GetIncidentNoteChangeHistory(incidentNoteChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (incidentNoteChangeHistoryData) => {

        //
        // Success path — incidentNoteChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!incidentNoteChangeHistoryData) {

          this.handleIncidentNoteChangeHistoryNotFound(incidentNoteChangeHistoryId);

        } else {

          this.incidentNoteChangeHistoryData = incidentNoteChangeHistoryData;
          this.buildFormValues(this.incidentNoteChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'IncidentNoteChangeHistory loaded successfully',
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
        this.handleIncidentNoteChangeHistoryLoadError(error, incidentNoteChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleIncidentNoteChangeHistoryNotFound(incidentNoteChangeHistoryId: number): void {

    this.incidentNoteChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `IncidentNoteChangeHistory #${incidentNoteChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleIncidentNoteChangeHistoryLoadError(error: any, incidentNoteChangeHistoryId: number): void {

    let message = 'Failed to load Incident Note Change History.';
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
          message = 'You do not have permission to view this Incident Note Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Incident Note Change History #${incidentNoteChangeHistoryId} was not found.`;
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

    console.error(`Incident Note Change History load failed (ID: ${incidentNoteChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.incidentNoteChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(incidentNoteChangeHistoryData: IncidentNoteChangeHistoryData | null) {

    if (incidentNoteChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.incidentNoteChangeHistoryForm.reset({
        incidentNoteId: null,
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
        this.incidentNoteChangeHistoryForm.reset({
        incidentNoteId: incidentNoteChangeHistoryData.incidentNoteId,
        versionNumber: incidentNoteChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(incidentNoteChangeHistoryData.timeStamp) ?? '',
        userId: incidentNoteChangeHistoryData.userId?.toString() ?? '',
        data: incidentNoteChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.incidentNoteChangeHistoryForm.markAsPristine();
    this.incidentNoteChangeHistoryForm.markAsUntouched();
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

    if (this.incidentNoteChangeHistoryService.userIsAlertingIncidentNoteChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Incident Note Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.incidentNoteChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.incidentNoteChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.incidentNoteChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const incidentNoteChangeHistorySubmitData: IncidentNoteChangeHistorySubmitData = {
        id: this.incidentNoteChangeHistoryData?.id || 0,
        incidentNoteId: Number(formValue.incidentNoteId),
        versionNumber: this.incidentNoteChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.incidentNoteChangeHistoryService.PutIncidentNoteChangeHistory(incidentNoteChangeHistorySubmitData.id, incidentNoteChangeHistorySubmitData)
      : this.incidentNoteChangeHistoryService.PostIncidentNoteChangeHistory(incidentNoteChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedIncidentNoteChangeHistoryData) => {

        this.incidentNoteChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Incident Note Change History's detail page
          //
          this.incidentNoteChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.incidentNoteChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/incidentnotechangehistories', savedIncidentNoteChangeHistoryData.id]);
          this.alertService.showMessage('Incident Note Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.incidentNoteChangeHistoryData = savedIncidentNoteChangeHistoryData;
          this.buildFormValues(this.incidentNoteChangeHistoryData);

          this.alertService.showMessage("Incident Note Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Incident Note Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident Note Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident Note Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAlertingIncidentNoteChangeHistoryReader(): boolean {
    return this.incidentNoteChangeHistoryService.userIsAlertingIncidentNoteChangeHistoryReader();
  }

  public userIsAlertingIncidentNoteChangeHistoryWriter(): boolean {
    return this.incidentNoteChangeHistoryService.userIsAlertingIncidentNoteChangeHistoryWriter();
  }
}
