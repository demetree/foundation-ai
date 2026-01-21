/*
   GENERATED FORM FOR THE EXTERNALCOMMUNICATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ExternalCommunication table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to external-communication-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ExternalCommunicationService, ExternalCommunicationData, ExternalCommunicationSubmitData } from '../../../auditor-data-services/external-communication.service';
import { AuditUserService } from '../../../auditor-data-services/audit-user.service';
import { ExternalCommunicationRecipientService } from '../../../auditor-data-services/external-communication-recipient.service';
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
interface ExternalCommunicationFormValues {
  timeStamp: string | null,
  auditUserId: number | bigint | null,       // For FK link number
  communicationType: string | null,
  subject: string | null,
  message: string | null,
  completedSuccessfully: boolean,
  responseMessage: string | null,
  exceptionText: string | null,
};


@Component({
  selector: 'app-external-communication-detail',
  templateUrl: './external-communication-detail.component.html',
  styleUrls: ['./external-communication-detail.component.scss']
})

export class ExternalCommunicationDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ExternalCommunicationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public externalCommunicationForm: FormGroup = this.fb.group({
        timeStamp: [''],
        auditUserId: [null],
        communicationType: [''],
        subject: [''],
        message: [''],
        completedSuccessfully: [false],
        responseMessage: [''],
        exceptionText: [''],
      });


  public externalCommunicationId: string | null = null;
  public externalCommunicationData: ExternalCommunicationData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  externalCommunications$ = this.externalCommunicationService.GetExternalCommunicationList();
  public auditUsers$ = this.auditUserService.GetAuditUserList();
  public externalCommunicationRecipients$ = this.externalCommunicationRecipientService.GetExternalCommunicationRecipientList();

  private destroy$ = new Subject<void>();

  constructor(
    public externalCommunicationService: ExternalCommunicationService,
    public auditUserService: AuditUserService,
    public externalCommunicationRecipientService: ExternalCommunicationRecipientService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the externalCommunicationId from the route parameters
    this.externalCommunicationId = this.route.snapshot.paramMap.get('externalCommunicationId');

    if (this.externalCommunicationId === 'new' ||
        this.externalCommunicationId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.externalCommunicationData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.externalCommunicationForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.externalCommunicationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New External Communication';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit External Communication';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.externalCommunicationForm.dirty) {
      return confirm('You have unsaved External Communication changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.externalCommunicationId != null && this.externalCommunicationId !== 'new') {

      const id = parseInt(this.externalCommunicationId, 10);

      if (!isNaN(id)) {
        return { externalCommunicationId: id };
      }
    }

    return null;
  }


/*
  * Loads the ExternalCommunication data for the current externalCommunicationId.
  *
  * Fully respects the ExternalCommunicationService caching strategy and error handling strategy.
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
    if (!this.externalCommunicationService.userIsAuditorExternalCommunicationReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ExternalCommunications.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate externalCommunicationId
    //
    if (!this.externalCommunicationId) {

      this.alertService.showMessage('No ExternalCommunication ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const externalCommunicationId = Number(this.externalCommunicationId);

    if (isNaN(externalCommunicationId) || externalCommunicationId <= 0) {

      this.alertService.showMessage(`Invalid External Communication ID: "${this.externalCommunicationId}"`,
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
      // This is the most targeted way: clear only this ExternalCommunication + relations

      this.externalCommunicationService.ClearRecordCache(externalCommunicationId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.externalCommunicationService.GetExternalCommunication(externalCommunicationId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (externalCommunicationData) => {

        //
        // Success path — externalCommunicationData can legitimately be null if 404'd but request succeeded
        //
        if (!externalCommunicationData) {

          this.handleExternalCommunicationNotFound(externalCommunicationId);

        } else {

          this.externalCommunicationData = externalCommunicationData;
          this.buildFormValues(this.externalCommunicationData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ExternalCommunication loaded successfully',
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
        this.handleExternalCommunicationLoadError(error, externalCommunicationId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleExternalCommunicationNotFound(externalCommunicationId: number): void {

    this.externalCommunicationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ExternalCommunication #${externalCommunicationId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleExternalCommunicationLoadError(error: any, externalCommunicationId: number): void {

    let message = 'Failed to load External Communication.';
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
          message = 'You do not have permission to view this External Communication.';
          title = 'Forbidden';
          break;
        case 404:
          message = `External Communication #${externalCommunicationId} was not found.`;
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

    console.error(`External Communication load failed (ID: ${externalCommunicationId})`, error);

    //
    // Reset UI to safe state
    //
    this.externalCommunicationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(externalCommunicationData: ExternalCommunicationData | null) {

    if (externalCommunicationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.externalCommunicationForm.reset({
        timeStamp: '',
        auditUserId: null,
        communicationType: '',
        subject: '',
        message: '',
        completedSuccessfully: false,
        responseMessage: '',
        exceptionText: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.externalCommunicationForm.reset({
        timeStamp: isoUtcStringToDateTimeLocal(externalCommunicationData.timeStamp) ?? '',
        auditUserId: externalCommunicationData.auditUserId,
        communicationType: externalCommunicationData.communicationType ?? '',
        subject: externalCommunicationData.subject ?? '',
        message: externalCommunicationData.message ?? '',
        completedSuccessfully: externalCommunicationData.completedSuccessfully ?? false,
        responseMessage: externalCommunicationData.responseMessage ?? '',
        exceptionText: externalCommunicationData.exceptionText ?? '',
      }, { emitEvent: false});
    }

    this.externalCommunicationForm.markAsPristine();
    this.externalCommunicationForm.markAsUntouched();
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

    if (this.externalCommunicationService.userIsAuditorExternalCommunicationWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to External Communications", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.externalCommunicationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.externalCommunicationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.externalCommunicationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const externalCommunicationSubmitData: ExternalCommunicationSubmitData = {
        id: this.externalCommunicationData?.id || 0,
        timeStamp: formValue.timeStamp ? dateTimeLocalToIsoUtc(formValue.timeStamp.trim()) : null,
        auditUserId: formValue.auditUserId ? Number(formValue.auditUserId) : null,
        communicationType: formValue.communicationType?.trim() || null,
        subject: formValue.subject?.trim() || null,
        message: formValue.message?.trim() || null,
        completedSuccessfully: !!formValue.completedSuccessfully,
        responseMessage: formValue.responseMessage?.trim() || null,
        exceptionText: formValue.exceptionText?.trim() || null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.externalCommunicationService.PutExternalCommunication(externalCommunicationSubmitData.id, externalCommunicationSubmitData)
      : this.externalCommunicationService.PostExternalCommunication(externalCommunicationSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedExternalCommunicationData) => {

        this.externalCommunicationService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created External Communication's detail page
          //
          this.externalCommunicationForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.externalCommunicationForm.markAsUntouched();

          this.router.navigate(['/externalcommunications', savedExternalCommunicationData.id]);
          this.alertService.showMessage('External Communication added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.externalCommunicationData = savedExternalCommunicationData;
          this.buildFormValues(this.externalCommunicationData);

          this.alertService.showMessage("External Communication saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this External Communication.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the External Communication.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('External Communication could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAuditorExternalCommunicationReader(): boolean {
    return this.externalCommunicationService.userIsAuditorExternalCommunicationReader();
  }

  public userIsAuditorExternalCommunicationWriter(): boolean {
    return this.externalCommunicationService.userIsAuditorExternalCommunicationWriter();
  }
}
