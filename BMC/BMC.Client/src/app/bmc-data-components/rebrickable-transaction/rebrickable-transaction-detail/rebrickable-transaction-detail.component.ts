/*
   GENERATED FORM FOR THE REBRICKABLETRANSACTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RebrickableTransaction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to rebrickable-transaction-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RebrickableTransactionService, RebrickableTransactionData, RebrickableTransactionSubmitData } from '../../../bmc-data-services/rebrickable-transaction.service';
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
interface RebrickableTransactionFormValues {
  transactionDate: string | null,
  direction: string,
  httpMethod: string,
  endpoint: string,
  requestSummary: string | null,
  responseStatusCode: string | null,     // Stored as string for form input, converted to number on submit.
  responseBody: string | null,
  success: boolean,
  errorMessage: string | null,
  triggeredBy: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-rebrickable-transaction-detail',
  templateUrl: './rebrickable-transaction-detail.component.html',
  styleUrls: ['./rebrickable-transaction-detail.component.scss']
})

export class RebrickableTransactionDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<RebrickableTransactionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public rebrickableTransactionForm: FormGroup = this.fb.group({
        transactionDate: [''],
        direction: ['', Validators.required],
        httpMethod: ['', Validators.required],
        endpoint: ['', Validators.required],
        requestSummary: [''],
        responseStatusCode: [''],
        responseBody: [''],
        success: [false],
        errorMessage: [''],
        triggeredBy: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public rebrickableTransactionId: string | null = null;
  public rebrickableTransactionData: RebrickableTransactionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  rebrickableTransactions$ = this.rebrickableTransactionService.GetRebrickableTransactionList();

  private destroy$ = new Subject<void>();

  constructor(
    public rebrickableTransactionService: RebrickableTransactionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the rebrickableTransactionId from the route parameters
    this.rebrickableTransactionId = this.route.snapshot.paramMap.get('rebrickableTransactionId');

    if (this.rebrickableTransactionId === 'new' ||
        this.rebrickableTransactionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.rebrickableTransactionData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.rebrickableTransactionForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.rebrickableTransactionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Rebrickable Transaction';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Rebrickable Transaction';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.rebrickableTransactionForm.dirty) {
      return confirm('You have unsaved Rebrickable Transaction changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.rebrickableTransactionId != null && this.rebrickableTransactionId !== 'new') {

      const id = parseInt(this.rebrickableTransactionId, 10);

      if (!isNaN(id)) {
        return { rebrickableTransactionId: id };
      }
    }

    return null;
  }


/*
  * Loads the RebrickableTransaction data for the current rebrickableTransactionId.
  *
  * Fully respects the RebrickableTransactionService caching strategy and error handling strategy.
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
    if (!this.rebrickableTransactionService.userIsBMCRebrickableTransactionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read RebrickableTransactions.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate rebrickableTransactionId
    //
    if (!this.rebrickableTransactionId) {

      this.alertService.showMessage('No RebrickableTransaction ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const rebrickableTransactionId = Number(this.rebrickableTransactionId);

    if (isNaN(rebrickableTransactionId) || rebrickableTransactionId <= 0) {

      this.alertService.showMessage(`Invalid Rebrickable Transaction ID: "${this.rebrickableTransactionId}"`,
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
      // This is the most targeted way: clear only this RebrickableTransaction + relations

      this.rebrickableTransactionService.ClearRecordCache(rebrickableTransactionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.rebrickableTransactionService.GetRebrickableTransaction(rebrickableTransactionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (rebrickableTransactionData) => {

        //
        // Success path — rebrickableTransactionData can legitimately be null if 404'd but request succeeded
        //
        if (!rebrickableTransactionData) {

          this.handleRebrickableTransactionNotFound(rebrickableTransactionId);

        } else {

          this.rebrickableTransactionData = rebrickableTransactionData;
          this.buildFormValues(this.rebrickableTransactionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'RebrickableTransaction loaded successfully',
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
        this.handleRebrickableTransactionLoadError(error, rebrickableTransactionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleRebrickableTransactionNotFound(rebrickableTransactionId: number): void {

    this.rebrickableTransactionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `RebrickableTransaction #${rebrickableTransactionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleRebrickableTransactionLoadError(error: any, rebrickableTransactionId: number): void {

    let message = 'Failed to load Rebrickable Transaction.';
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
          message = 'You do not have permission to view this Rebrickable Transaction.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Rebrickable Transaction #${rebrickableTransactionId} was not found.`;
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

    console.error(`Rebrickable Transaction load failed (ID: ${rebrickableTransactionId})`, error);

    //
    // Reset UI to safe state
    //
    this.rebrickableTransactionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(rebrickableTransactionData: RebrickableTransactionData | null) {

    if (rebrickableTransactionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.rebrickableTransactionForm.reset({
        transactionDate: '',
        direction: '',
        httpMethod: '',
        endpoint: '',
        requestSummary: '',
        responseStatusCode: '',
        responseBody: '',
        success: false,
        errorMessage: '',
        triggeredBy: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.rebrickableTransactionForm.reset({
        transactionDate: isoUtcStringToDateTimeLocal(rebrickableTransactionData.transactionDate) ?? '',
        direction: rebrickableTransactionData.direction ?? '',
        httpMethod: rebrickableTransactionData.httpMethod ?? '',
        endpoint: rebrickableTransactionData.endpoint ?? '',
        requestSummary: rebrickableTransactionData.requestSummary ?? '',
        responseStatusCode: rebrickableTransactionData.responseStatusCode?.toString() ?? '',
        responseBody: rebrickableTransactionData.responseBody ?? '',
        success: rebrickableTransactionData.success ?? false,
        errorMessage: rebrickableTransactionData.errorMessage ?? '',
        triggeredBy: rebrickableTransactionData.triggeredBy ?? '',
        active: rebrickableTransactionData.active ?? true,
        deleted: rebrickableTransactionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.rebrickableTransactionForm.markAsPristine();
    this.rebrickableTransactionForm.markAsUntouched();
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

    if (this.rebrickableTransactionService.userIsBMCRebrickableTransactionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Rebrickable Transactions", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.rebrickableTransactionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.rebrickableTransactionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.rebrickableTransactionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const rebrickableTransactionSubmitData: RebrickableTransactionSubmitData = {
        id: this.rebrickableTransactionData?.id || 0,
        transactionDate: formValue.transactionDate ? dateTimeLocalToIsoUtc(formValue.transactionDate.trim()) : null,
        direction: formValue.direction!.trim(),
        httpMethod: formValue.httpMethod!.trim(),
        endpoint: formValue.endpoint!.trim(),
        requestSummary: formValue.requestSummary?.trim() || null,
        responseStatusCode: formValue.responseStatusCode ? Number(formValue.responseStatusCode) : null,
        responseBody: formValue.responseBody?.trim() || null,
        success: !!formValue.success,
        errorMessage: formValue.errorMessage?.trim() || null,
        triggeredBy: formValue.triggeredBy!.trim(),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.rebrickableTransactionService.PutRebrickableTransaction(rebrickableTransactionSubmitData.id, rebrickableTransactionSubmitData)
      : this.rebrickableTransactionService.PostRebrickableTransaction(rebrickableTransactionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedRebrickableTransactionData) => {

        this.rebrickableTransactionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Rebrickable Transaction's detail page
          //
          this.rebrickableTransactionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.rebrickableTransactionForm.markAsUntouched();

          this.router.navigate(['/rebrickabletransactions', savedRebrickableTransactionData.id]);
          this.alertService.showMessage('Rebrickable Transaction added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.rebrickableTransactionData = savedRebrickableTransactionData;
          this.buildFormValues(this.rebrickableTransactionData);

          this.alertService.showMessage("Rebrickable Transaction saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Rebrickable Transaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Rebrickable Transaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Rebrickable Transaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCRebrickableTransactionReader(): boolean {
    return this.rebrickableTransactionService.userIsBMCRebrickableTransactionReader();
  }

  public userIsBMCRebrickableTransactionWriter(): boolean {
    return this.rebrickableTransactionService.userIsBMCRebrickableTransactionWriter();
  }
}
