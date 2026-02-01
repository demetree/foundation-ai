/*
   GENERATED FORM FOR THE INCIDENTNOTE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IncidentNote table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to incident-note-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IncidentNoteService, IncidentNoteData, IncidentNoteSubmitData } from '../../../alerting-data-services/incident-note.service';
import { IncidentService } from '../../../alerting-data-services/incident.service';
import { IncidentNoteChangeHistoryService } from '../../../alerting-data-services/incident-note-change-history.service';
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
interface IncidentNoteFormValues {
  incidentId: number | bigint,       // For FK link number
  authorObjectGuid: string,
  createdAt: string,
  content: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-incident-note-detail',
  templateUrl: './incident-note-detail.component.html',
  styleUrls: ['./incident-note-detail.component.scss']
})

export class IncidentNoteDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IncidentNoteFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public incidentNoteForm: FormGroup = this.fb.group({
        incidentId: [null, Validators.required],
        authorObjectGuid: ['', Validators.required],
        createdAt: ['', Validators.required],
        content: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public incidentNoteId: string | null = null;
  public incidentNoteData: IncidentNoteData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  incidentNotes$ = this.incidentNoteService.GetIncidentNoteList();
  public incidents$ = this.incidentService.GetIncidentList();
  public incidentNoteChangeHistories$ = this.incidentNoteChangeHistoryService.GetIncidentNoteChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public incidentNoteService: IncidentNoteService,
    public incidentService: IncidentService,
    public incidentNoteChangeHistoryService: IncidentNoteChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the incidentNoteId from the route parameters
    this.incidentNoteId = this.route.snapshot.paramMap.get('incidentNoteId');

    if (this.incidentNoteId === 'new' ||
        this.incidentNoteId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.incidentNoteData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.incidentNoteForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.incidentNoteForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Incident Note';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Incident Note';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.incidentNoteForm.dirty) {
      return confirm('You have unsaved Incident Note changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.incidentNoteId != null && this.incidentNoteId !== 'new') {

      const id = parseInt(this.incidentNoteId, 10);

      if (!isNaN(id)) {
        return { incidentNoteId: id };
      }
    }

    return null;
  }


/*
  * Loads the IncidentNote data for the current incidentNoteId.
  *
  * Fully respects the IncidentNoteService caching strategy and error handling strategy.
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
    if (!this.incidentNoteService.userIsAlertingIncidentNoteReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read IncidentNotes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate incidentNoteId
    //
    if (!this.incidentNoteId) {

      this.alertService.showMessage('No IncidentNote ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const incidentNoteId = Number(this.incidentNoteId);

    if (isNaN(incidentNoteId) || incidentNoteId <= 0) {

      this.alertService.showMessage(`Invalid Incident Note ID: "${this.incidentNoteId}"`,
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
      // This is the most targeted way: clear only this IncidentNote + relations

      this.incidentNoteService.ClearRecordCache(incidentNoteId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.incidentNoteService.GetIncidentNote(incidentNoteId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (incidentNoteData) => {

        //
        // Success path — incidentNoteData can legitimately be null if 404'd but request succeeded
        //
        if (!incidentNoteData) {

          this.handleIncidentNoteNotFound(incidentNoteId);

        } else {

          this.incidentNoteData = incidentNoteData;
          this.buildFormValues(this.incidentNoteData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'IncidentNote loaded successfully',
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
        this.handleIncidentNoteLoadError(error, incidentNoteId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleIncidentNoteNotFound(incidentNoteId: number): void {

    this.incidentNoteData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `IncidentNote #${incidentNoteId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleIncidentNoteLoadError(error: any, incidentNoteId: number): void {

    let message = 'Failed to load Incident Note.';
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
          message = 'You do not have permission to view this Incident Note.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Incident Note #${incidentNoteId} was not found.`;
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

    console.error(`Incident Note load failed (ID: ${incidentNoteId})`, error);

    //
    // Reset UI to safe state
    //
    this.incidentNoteData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(incidentNoteData: IncidentNoteData | null) {

    if (incidentNoteData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.incidentNoteForm.reset({
        incidentId: null,
        authorObjectGuid: '',
        createdAt: '',
        content: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.incidentNoteForm.reset({
        incidentId: incidentNoteData.incidentId,
        authorObjectGuid: incidentNoteData.authorObjectGuid ?? '',
        createdAt: isoUtcStringToDateTimeLocal(incidentNoteData.createdAt) ?? '',
        content: incidentNoteData.content ?? '',
        versionNumber: incidentNoteData.versionNumber?.toString() ?? '',
        active: incidentNoteData.active ?? true,
        deleted: incidentNoteData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.incidentNoteForm.markAsPristine();
    this.incidentNoteForm.markAsUntouched();
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

    if (this.incidentNoteService.userIsAlertingIncidentNoteWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Incident Notes", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.incidentNoteForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.incidentNoteForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.incidentNoteForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const incidentNoteSubmitData: IncidentNoteSubmitData = {
        id: this.incidentNoteData?.id || 0,
        incidentId: Number(formValue.incidentId),
        authorObjectGuid: formValue.authorObjectGuid!.trim(),
        createdAt: dateTimeLocalToIsoUtc(formValue.createdAt!.trim())!,
        content: formValue.content!.trim(),
        versionNumber: this.incidentNoteData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.incidentNoteService.PutIncidentNote(incidentNoteSubmitData.id, incidentNoteSubmitData)
      : this.incidentNoteService.PostIncidentNote(incidentNoteSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedIncidentNoteData) => {

        this.incidentNoteService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Incident Note's detail page
          //
          this.incidentNoteForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.incidentNoteForm.markAsUntouched();

          this.router.navigate(['/incidentnotes', savedIncidentNoteData.id]);
          this.alertService.showMessage('Incident Note added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.incidentNoteData = savedIncidentNoteData;
          this.buildFormValues(this.incidentNoteData);

          this.alertService.showMessage("Incident Note saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Incident Note.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident Note.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident Note could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAlertingIncidentNoteReader(): boolean {
    return this.incidentNoteService.userIsAlertingIncidentNoteReader();
  }

  public userIsAlertingIncidentNoteWriter(): boolean {
    return this.incidentNoteService.userIsAlertingIncidentNoteWriter();
  }
}
