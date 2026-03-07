/*
   GENERATED FORM FOR THE FINANCIALOFFICECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from FinancialOfficeChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to financial-office-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { FinancialOfficeChangeHistoryService, FinancialOfficeChangeHistoryData, FinancialOfficeChangeHistorySubmitData } from '../../../scheduler-data-services/financial-office-change-history.service';
import { FinancialOfficeService } from '../../../scheduler-data-services/financial-office.service';
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
interface FinancialOfficeChangeHistoryFormValues {
  financialOfficeId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-financial-office-change-history-detail',
  templateUrl: './financial-office-change-history-detail.component.html',
  styleUrls: ['./financial-office-change-history-detail.component.scss']
})

export class FinancialOfficeChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<FinancialOfficeChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public financialOfficeChangeHistoryForm: FormGroup = this.fb.group({
        financialOfficeId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public financialOfficeChangeHistoryId: string | null = null;
  public financialOfficeChangeHistoryData: FinancialOfficeChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  financialOfficeChangeHistories$ = this.financialOfficeChangeHistoryService.GetFinancialOfficeChangeHistoryList();
  public financialOffices$ = this.financialOfficeService.GetFinancialOfficeList();

  private destroy$ = new Subject<void>();

  constructor(
    public financialOfficeChangeHistoryService: FinancialOfficeChangeHistoryService,
    public financialOfficeService: FinancialOfficeService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the financialOfficeChangeHistoryId from the route parameters
    this.financialOfficeChangeHistoryId = this.route.snapshot.paramMap.get('financialOfficeChangeHistoryId');

    if (this.financialOfficeChangeHistoryId === 'new' ||
        this.financialOfficeChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.financialOfficeChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.financialOfficeChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.financialOfficeChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Financial Office Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Financial Office Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.financialOfficeChangeHistoryForm.dirty) {
      return confirm('You have unsaved Financial Office Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.financialOfficeChangeHistoryId != null && this.financialOfficeChangeHistoryId !== 'new') {

      const id = parseInt(this.financialOfficeChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { financialOfficeChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the FinancialOfficeChangeHistory data for the current financialOfficeChangeHistoryId.
  *
  * Fully respects the FinancialOfficeChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.financialOfficeChangeHistoryService.userIsSchedulerFinancialOfficeChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read FinancialOfficeChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate financialOfficeChangeHistoryId
    //
    if (!this.financialOfficeChangeHistoryId) {

      this.alertService.showMessage('No FinancialOfficeChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const financialOfficeChangeHistoryId = Number(this.financialOfficeChangeHistoryId);

    if (isNaN(financialOfficeChangeHistoryId) || financialOfficeChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Financial Office Change History ID: "${this.financialOfficeChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this FinancialOfficeChangeHistory + relations

      this.financialOfficeChangeHistoryService.ClearRecordCache(financialOfficeChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.financialOfficeChangeHistoryService.GetFinancialOfficeChangeHistory(financialOfficeChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (financialOfficeChangeHistoryData) => {

        //
        // Success path — financialOfficeChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!financialOfficeChangeHistoryData) {

          this.handleFinancialOfficeChangeHistoryNotFound(financialOfficeChangeHistoryId);

        } else {

          this.financialOfficeChangeHistoryData = financialOfficeChangeHistoryData;
          this.buildFormValues(this.financialOfficeChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'FinancialOfficeChangeHistory loaded successfully',
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
        this.handleFinancialOfficeChangeHistoryLoadError(error, financialOfficeChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleFinancialOfficeChangeHistoryNotFound(financialOfficeChangeHistoryId: number): void {

    this.financialOfficeChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `FinancialOfficeChangeHistory #${financialOfficeChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleFinancialOfficeChangeHistoryLoadError(error: any, financialOfficeChangeHistoryId: number): void {

    let message = 'Failed to load Financial Office Change History.';
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
          message = 'You do not have permission to view this Financial Office Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Financial Office Change History #${financialOfficeChangeHistoryId} was not found.`;
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

    console.error(`Financial Office Change History load failed (ID: ${financialOfficeChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.financialOfficeChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(financialOfficeChangeHistoryData: FinancialOfficeChangeHistoryData | null) {

    if (financialOfficeChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.financialOfficeChangeHistoryForm.reset({
        financialOfficeId: null,
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
        this.financialOfficeChangeHistoryForm.reset({
        financialOfficeId: financialOfficeChangeHistoryData.financialOfficeId,
        versionNumber: financialOfficeChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(financialOfficeChangeHistoryData.timeStamp) ?? '',
        userId: financialOfficeChangeHistoryData.userId?.toString() ?? '',
        data: financialOfficeChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.financialOfficeChangeHistoryForm.markAsPristine();
    this.financialOfficeChangeHistoryForm.markAsUntouched();
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

    if (this.financialOfficeChangeHistoryService.userIsSchedulerFinancialOfficeChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Financial Office Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.financialOfficeChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.financialOfficeChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.financialOfficeChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const financialOfficeChangeHistorySubmitData: FinancialOfficeChangeHistorySubmitData = {
        id: this.financialOfficeChangeHistoryData?.id || 0,
        financialOfficeId: Number(formValue.financialOfficeId),
        versionNumber: this.financialOfficeChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.financialOfficeChangeHistoryService.PutFinancialOfficeChangeHistory(financialOfficeChangeHistorySubmitData.id, financialOfficeChangeHistorySubmitData)
      : this.financialOfficeChangeHistoryService.PostFinancialOfficeChangeHistory(financialOfficeChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedFinancialOfficeChangeHistoryData) => {

        this.financialOfficeChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Financial Office Change History's detail page
          //
          this.financialOfficeChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.financialOfficeChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/financialofficechangehistories', savedFinancialOfficeChangeHistoryData.id]);
          this.alertService.showMessage('Financial Office Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.financialOfficeChangeHistoryData = savedFinancialOfficeChangeHistoryData;
          this.buildFormValues(this.financialOfficeChangeHistoryData);

          this.alertService.showMessage("Financial Office Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Financial Office Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Financial Office Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Financial Office Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerFinancialOfficeChangeHistoryReader(): boolean {
    return this.financialOfficeChangeHistoryService.userIsSchedulerFinancialOfficeChangeHistoryReader();
  }

  public userIsSchedulerFinancialOfficeChangeHistoryWriter(): boolean {
    return this.financialOfficeChangeHistoryService.userIsSchedulerFinancialOfficeChangeHistoryWriter();
  }
}
