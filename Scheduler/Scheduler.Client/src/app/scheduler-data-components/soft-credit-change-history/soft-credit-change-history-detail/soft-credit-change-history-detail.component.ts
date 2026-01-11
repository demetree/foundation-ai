/*
   GENERATED FORM FOR THE SOFTCREDITCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SoftCreditChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to soft-credit-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SoftCreditChangeHistoryService, SoftCreditChangeHistoryData, SoftCreditChangeHistorySubmitData } from '../../../scheduler-data-services/soft-credit-change-history.service';
import { SoftCreditService } from '../../../scheduler-data-services/soft-credit.service';
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
interface SoftCreditChangeHistoryFormValues {
  softCreditId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-soft-credit-change-history-detail',
  templateUrl: './soft-credit-change-history-detail.component.html',
  styleUrls: ['./soft-credit-change-history-detail.component.scss']
})

export class SoftCreditChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SoftCreditChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public softCreditChangeHistoryForm: FormGroup = this.fb.group({
        softCreditId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public softCreditChangeHistoryId: string | null = null;
  public softCreditChangeHistoryData: SoftCreditChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  softCreditChangeHistories$ = this.softCreditChangeHistoryService.GetSoftCreditChangeHistoryList();
  public softCredits$ = this.softCreditService.GetSoftCreditList();

  private destroy$ = new Subject<void>();

  constructor(
    public softCreditChangeHistoryService: SoftCreditChangeHistoryService,
    public softCreditService: SoftCreditService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the softCreditChangeHistoryId from the route parameters
    this.softCreditChangeHistoryId = this.route.snapshot.paramMap.get('softCreditChangeHistoryId');

    if (this.softCreditChangeHistoryId === 'new' ||
        this.softCreditChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.softCreditChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.softCreditChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.softCreditChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Soft Credit Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Soft Credit Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.softCreditChangeHistoryForm.dirty) {
      return confirm('You have unsaved Soft Credit Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.softCreditChangeHistoryId != null && this.softCreditChangeHistoryId !== 'new') {

      const id = parseInt(this.softCreditChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { softCreditChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the SoftCreditChangeHistory data for the current softCreditChangeHistoryId.
  *
  * Fully respects the SoftCreditChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.softCreditChangeHistoryService.userIsSchedulerSoftCreditChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SoftCreditChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate softCreditChangeHistoryId
    //
    if (!this.softCreditChangeHistoryId) {

      this.alertService.showMessage('No SoftCreditChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const softCreditChangeHistoryId = Number(this.softCreditChangeHistoryId);

    if (isNaN(softCreditChangeHistoryId) || softCreditChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Soft Credit Change History ID: "${this.softCreditChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this SoftCreditChangeHistory + relations

      this.softCreditChangeHistoryService.ClearRecordCache(softCreditChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.softCreditChangeHistoryService.GetSoftCreditChangeHistory(softCreditChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (softCreditChangeHistoryData) => {

        //
        // Success path — softCreditChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!softCreditChangeHistoryData) {

          this.handleSoftCreditChangeHistoryNotFound(softCreditChangeHistoryId);

        } else {

          this.softCreditChangeHistoryData = softCreditChangeHistoryData;
          this.buildFormValues(this.softCreditChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SoftCreditChangeHistory loaded successfully',
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
        this.handleSoftCreditChangeHistoryLoadError(error, softCreditChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSoftCreditChangeHistoryNotFound(softCreditChangeHistoryId: number): void {

    this.softCreditChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SoftCreditChangeHistory #${softCreditChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSoftCreditChangeHistoryLoadError(error: any, softCreditChangeHistoryId: number): void {

    let message = 'Failed to load Soft Credit Change History.';
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
          message = 'You do not have permission to view this Soft Credit Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Soft Credit Change History #${softCreditChangeHistoryId} was not found.`;
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

    console.error(`Soft Credit Change History load failed (ID: ${softCreditChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.softCreditChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(softCreditChangeHistoryData: SoftCreditChangeHistoryData | null) {

    if (softCreditChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.softCreditChangeHistoryForm.reset({
        softCreditId: null,
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
        this.softCreditChangeHistoryForm.reset({
        softCreditId: softCreditChangeHistoryData.softCreditId,
        versionNumber: softCreditChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(softCreditChangeHistoryData.timeStamp) ?? '',
        userId: softCreditChangeHistoryData.userId?.toString() ?? '',
        data: softCreditChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.softCreditChangeHistoryForm.markAsPristine();
    this.softCreditChangeHistoryForm.markAsUntouched();
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

    if (this.softCreditChangeHistoryService.userIsSchedulerSoftCreditChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Soft Credit Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.softCreditChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.softCreditChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.softCreditChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const softCreditChangeHistorySubmitData: SoftCreditChangeHistorySubmitData = {
        id: this.softCreditChangeHistoryData?.id || 0,
        softCreditId: Number(formValue.softCreditId),
        versionNumber: this.softCreditChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.softCreditChangeHistoryService.PutSoftCreditChangeHistory(softCreditChangeHistorySubmitData.id, softCreditChangeHistorySubmitData)
      : this.softCreditChangeHistoryService.PostSoftCreditChangeHistory(softCreditChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSoftCreditChangeHistoryData) => {

        this.softCreditChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Soft Credit Change History's detail page
          //
          this.softCreditChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.softCreditChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/softcreditchangehistories', savedSoftCreditChangeHistoryData.id]);
          this.alertService.showMessage('Soft Credit Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.softCreditChangeHistoryData = savedSoftCreditChangeHistoryData;
          this.buildFormValues(this.softCreditChangeHistoryData);

          this.alertService.showMessage("Soft Credit Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Soft Credit Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Soft Credit Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Soft Credit Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerSoftCreditChangeHistoryReader(): boolean {
    return this.softCreditChangeHistoryService.userIsSchedulerSoftCreditChangeHistoryReader();
  }

  public userIsSchedulerSoftCreditChangeHistoryWriter(): boolean {
    return this.softCreditChangeHistoryService.userIsSchedulerSoftCreditChangeHistoryWriter();
  }
}
