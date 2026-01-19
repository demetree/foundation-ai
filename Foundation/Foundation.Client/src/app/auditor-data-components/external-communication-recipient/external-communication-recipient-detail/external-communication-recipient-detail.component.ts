/*
   GENERATED FORM FOR THE EXTERNALCOMMUNICATIONRECIPIENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ExternalCommunicationRecipient table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to external-communication-recipient-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ExternalCommunicationRecipientService, ExternalCommunicationRecipientData, ExternalCommunicationRecipientSubmitData } from '../../../auditor-data-services/external-communication-recipient.service';
import { ExternalCommunicationService } from '../../../auditor-data-services/external-communication.service';
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
interface ExternalCommunicationRecipientFormValues {
  externalCommunicationId: number | bigint | null,       // For FK link number
  recipient: string | null,
  type: string | null,
};


@Component({
  selector: 'app-external-communication-recipient-detail',
  templateUrl: './external-communication-recipient-detail.component.html',
  styleUrls: ['./external-communication-recipient-detail.component.scss']
})

export class ExternalCommunicationRecipientDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ExternalCommunicationRecipientFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public externalCommunicationRecipientForm: FormGroup = this.fb.group({
        externalCommunicationId: [null],
        recipient: [''],
        type: [''],
      });


  public externalCommunicationRecipientId: string | null = null;
  public externalCommunicationRecipientData: ExternalCommunicationRecipientData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  externalCommunicationRecipients$ = this.externalCommunicationRecipientService.GetExternalCommunicationRecipientList();
  public externalCommunications$ = this.externalCommunicationService.GetExternalCommunicationList();

  private destroy$ = new Subject<void>();

  constructor(
    public externalCommunicationRecipientService: ExternalCommunicationRecipientService,
    public externalCommunicationService: ExternalCommunicationService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the externalCommunicationRecipientId from the route parameters
    this.externalCommunicationRecipientId = this.route.snapshot.paramMap.get('externalCommunicationRecipientId');

    if (this.externalCommunicationRecipientId === 'new' ||
        this.externalCommunicationRecipientId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.externalCommunicationRecipientData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.externalCommunicationRecipientForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.externalCommunicationRecipientForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New External Communication Recipient';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit External Communication Recipient';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.externalCommunicationRecipientForm.dirty) {
      return confirm('You have unsaved External Communication Recipient changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.externalCommunicationRecipientId != null && this.externalCommunicationRecipientId !== 'new') {

      const id = parseInt(this.externalCommunicationRecipientId, 10);

      if (!isNaN(id)) {
        return { externalCommunicationRecipientId: id };
      }
    }

    return null;
  }


/*
  * Loads the ExternalCommunicationRecipient data for the current externalCommunicationRecipientId.
  *
  * Fully respects the ExternalCommunicationRecipientService caching strategy and error handling strategy.
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
    if (!this.externalCommunicationRecipientService.userIsAuditorExternalCommunicationRecipientReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ExternalCommunicationRecipients.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate externalCommunicationRecipientId
    //
    if (!this.externalCommunicationRecipientId) {

      this.alertService.showMessage('No ExternalCommunicationRecipient ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const externalCommunicationRecipientId = Number(this.externalCommunicationRecipientId);

    if (isNaN(externalCommunicationRecipientId) || externalCommunicationRecipientId <= 0) {

      this.alertService.showMessage(`Invalid External Communication Recipient ID: "${this.externalCommunicationRecipientId}"`,
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
      // This is the most targeted way: clear only this ExternalCommunicationRecipient + relations

      this.externalCommunicationRecipientService.ClearRecordCache(externalCommunicationRecipientId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.externalCommunicationRecipientService.GetExternalCommunicationRecipient(externalCommunicationRecipientId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (externalCommunicationRecipientData) => {

        //
        // Success path — externalCommunicationRecipientData can legitimately be null if 404'd but request succeeded
        //
        if (!externalCommunicationRecipientData) {

          this.handleExternalCommunicationRecipientNotFound(externalCommunicationRecipientId);

        } else {

          this.externalCommunicationRecipientData = externalCommunicationRecipientData;
          this.buildFormValues(this.externalCommunicationRecipientData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ExternalCommunicationRecipient loaded successfully',
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
        this.handleExternalCommunicationRecipientLoadError(error, externalCommunicationRecipientId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleExternalCommunicationRecipientNotFound(externalCommunicationRecipientId: number): void {

    this.externalCommunicationRecipientData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ExternalCommunicationRecipient #${externalCommunicationRecipientId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleExternalCommunicationRecipientLoadError(error: any, externalCommunicationRecipientId: number): void {

    let message = 'Failed to load External Communication Recipient.';
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
          message = 'You do not have permission to view this External Communication Recipient.';
          title = 'Forbidden';
          break;
        case 404:
          message = `External Communication Recipient #${externalCommunicationRecipientId} was not found.`;
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

    console.error(`External Communication Recipient load failed (ID: ${externalCommunicationRecipientId})`, error);

    //
    // Reset UI to safe state
    //
    this.externalCommunicationRecipientData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(externalCommunicationRecipientData: ExternalCommunicationRecipientData | null) {

    if (externalCommunicationRecipientData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.externalCommunicationRecipientForm.reset({
        externalCommunicationId: null,
        recipient: '',
        type: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.externalCommunicationRecipientForm.reset({
        externalCommunicationId: externalCommunicationRecipientData.externalCommunicationId,
        recipient: externalCommunicationRecipientData.recipient ?? '',
        type: externalCommunicationRecipientData.type ?? '',
      }, { emitEvent: false});
    }

    this.externalCommunicationRecipientForm.markAsPristine();
    this.externalCommunicationRecipientForm.markAsUntouched();
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

    if (this.externalCommunicationRecipientService.userIsAuditorExternalCommunicationRecipientWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to External Communication Recipients", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.externalCommunicationRecipientForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.externalCommunicationRecipientForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.externalCommunicationRecipientForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const externalCommunicationRecipientSubmitData: ExternalCommunicationRecipientSubmitData = {
        id: this.externalCommunicationRecipientData?.id || 0,
        externalCommunicationId: formValue.externalCommunicationId ? Number(formValue.externalCommunicationId) : null,
        recipient: formValue.recipient?.trim() || null,
        type: formValue.type?.trim() || null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.externalCommunicationRecipientService.PutExternalCommunicationRecipient(externalCommunicationRecipientSubmitData.id, externalCommunicationRecipientSubmitData)
      : this.externalCommunicationRecipientService.PostExternalCommunicationRecipient(externalCommunicationRecipientSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedExternalCommunicationRecipientData) => {

        this.externalCommunicationRecipientService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created External Communication Recipient's detail page
          //
          this.externalCommunicationRecipientForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.externalCommunicationRecipientForm.markAsUntouched();

          this.router.navigate(['/externalcommunicationrecipients', savedExternalCommunicationRecipientData.id]);
          this.alertService.showMessage('External Communication Recipient added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.externalCommunicationRecipientData = savedExternalCommunicationRecipientData;
          this.buildFormValues(this.externalCommunicationRecipientData);

          this.alertService.showMessage("External Communication Recipient saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this External Communication Recipient.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the External Communication Recipient.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('External Communication Recipient could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAuditorExternalCommunicationRecipientReader(): boolean {
    return this.externalCommunicationRecipientService.userIsAuditorExternalCommunicationRecipientReader();
  }

  public userIsAuditorExternalCommunicationRecipientWriter(): boolean {
    return this.externalCommunicationRecipientService.userIsAuditorExternalCommunicationRecipientWriter();
  }
}
