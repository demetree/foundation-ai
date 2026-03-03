/*
   GENERATED FORM FOR THE BRICKECONOMYTRANSACTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickEconomyTransaction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-economy-transaction-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickEconomyTransactionService, BrickEconomyTransactionData, BrickEconomyTransactionSubmitData } from '../../../bmc-data-services/brick-economy-transaction.service';
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
interface BrickEconomyTransactionFormValues {
  transactionDate: string | null,
  direction: string,
  methodName: string,
  requestSummary: string | null,
  success: boolean,
  errorMessage: string | null,
  triggeredBy: string,
  recordCount: string | null,     // Stored as string for form input, converted to number on submit.
  dailyQuotaRemaining: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-brick-economy-transaction-detail',
  templateUrl: './brick-economy-transaction-detail.component.html',
  styleUrls: ['./brick-economy-transaction-detail.component.scss']
})

export class BrickEconomyTransactionDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickEconomyTransactionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickEconomyTransactionForm: FormGroup = this.fb.group({
        transactionDate: [''],
        direction: ['', Validators.required],
        methodName: ['', Validators.required],
        requestSummary: [''],
        success: [false],
        errorMessage: [''],
        triggeredBy: ['', Validators.required],
        recordCount: [''],
        dailyQuotaRemaining: [''],
        active: [true],
        deleted: [false],
      });


  public brickEconomyTransactionId: string | null = null;
  public brickEconomyTransactionData: BrickEconomyTransactionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  brickEconomyTransactions$ = this.brickEconomyTransactionService.GetBrickEconomyTransactionList();

  private destroy$ = new Subject<void>();

  constructor(
    public brickEconomyTransactionService: BrickEconomyTransactionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the brickEconomyTransactionId from the route parameters
    this.brickEconomyTransactionId = this.route.snapshot.paramMap.get('brickEconomyTransactionId');

    if (this.brickEconomyTransactionId === 'new' ||
        this.brickEconomyTransactionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.brickEconomyTransactionData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.brickEconomyTransactionForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickEconomyTransactionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Brick Economy Transaction';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Brick Economy Transaction';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.brickEconomyTransactionForm.dirty) {
      return confirm('You have unsaved Brick Economy Transaction changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.brickEconomyTransactionId != null && this.brickEconomyTransactionId !== 'new') {

      const id = parseInt(this.brickEconomyTransactionId, 10);

      if (!isNaN(id)) {
        return { brickEconomyTransactionId: id };
      }
    }

    return null;
  }


/*
  * Loads the BrickEconomyTransaction data for the current brickEconomyTransactionId.
  *
  * Fully respects the BrickEconomyTransactionService caching strategy and error handling strategy.
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
    if (!this.brickEconomyTransactionService.userIsBMCBrickEconomyTransactionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BrickEconomyTransactions.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate brickEconomyTransactionId
    //
    if (!this.brickEconomyTransactionId) {

      this.alertService.showMessage('No BrickEconomyTransaction ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const brickEconomyTransactionId = Number(this.brickEconomyTransactionId);

    if (isNaN(brickEconomyTransactionId) || brickEconomyTransactionId <= 0) {

      this.alertService.showMessage(`Invalid Brick Economy Transaction ID: "${this.brickEconomyTransactionId}"`,
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
      // This is the most targeted way: clear only this BrickEconomyTransaction + relations

      this.brickEconomyTransactionService.ClearRecordCache(brickEconomyTransactionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.brickEconomyTransactionService.GetBrickEconomyTransaction(brickEconomyTransactionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (brickEconomyTransactionData) => {

        //
        // Success path — brickEconomyTransactionData can legitimately be null if 404'd but request succeeded
        //
        if (!brickEconomyTransactionData) {

          this.handleBrickEconomyTransactionNotFound(brickEconomyTransactionId);

        } else {

          this.brickEconomyTransactionData = brickEconomyTransactionData;
          this.buildFormValues(this.brickEconomyTransactionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BrickEconomyTransaction loaded successfully',
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
        this.handleBrickEconomyTransactionLoadError(error, brickEconomyTransactionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBrickEconomyTransactionNotFound(brickEconomyTransactionId: number): void {

    this.brickEconomyTransactionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BrickEconomyTransaction #${brickEconomyTransactionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBrickEconomyTransactionLoadError(error: any, brickEconomyTransactionId: number): void {

    let message = 'Failed to load Brick Economy Transaction.';
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
          message = 'You do not have permission to view this Brick Economy Transaction.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Brick Economy Transaction #${brickEconomyTransactionId} was not found.`;
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

    console.error(`Brick Economy Transaction load failed (ID: ${brickEconomyTransactionId})`, error);

    //
    // Reset UI to safe state
    //
    this.brickEconomyTransactionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(brickEconomyTransactionData: BrickEconomyTransactionData | null) {

    if (brickEconomyTransactionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickEconomyTransactionForm.reset({
        transactionDate: '',
        direction: '',
        methodName: '',
        requestSummary: '',
        success: false,
        errorMessage: '',
        triggeredBy: '',
        recordCount: '',
        dailyQuotaRemaining: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickEconomyTransactionForm.reset({
        transactionDate: isoUtcStringToDateTimeLocal(brickEconomyTransactionData.transactionDate) ?? '',
        direction: brickEconomyTransactionData.direction ?? '',
        methodName: brickEconomyTransactionData.methodName ?? '',
        requestSummary: brickEconomyTransactionData.requestSummary ?? '',
        success: brickEconomyTransactionData.success ?? false,
        errorMessage: brickEconomyTransactionData.errorMessage ?? '',
        triggeredBy: brickEconomyTransactionData.triggeredBy ?? '',
        recordCount: brickEconomyTransactionData.recordCount?.toString() ?? '',
        dailyQuotaRemaining: brickEconomyTransactionData.dailyQuotaRemaining?.toString() ?? '',
        active: brickEconomyTransactionData.active ?? true,
        deleted: brickEconomyTransactionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickEconomyTransactionForm.markAsPristine();
    this.brickEconomyTransactionForm.markAsUntouched();
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

    if (this.brickEconomyTransactionService.userIsBMCBrickEconomyTransactionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Brick Economy Transactions", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.brickEconomyTransactionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickEconomyTransactionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickEconomyTransactionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickEconomyTransactionSubmitData: BrickEconomyTransactionSubmitData = {
        id: this.brickEconomyTransactionData?.id || 0,
        transactionDate: formValue.transactionDate ? dateTimeLocalToIsoUtc(formValue.transactionDate.trim()) : null,
        direction: formValue.direction!.trim(),
        methodName: formValue.methodName!.trim(),
        requestSummary: formValue.requestSummary?.trim() || null,
        success: !!formValue.success,
        errorMessage: formValue.errorMessage?.trim() || null,
        triggeredBy: formValue.triggeredBy!.trim(),
        recordCount: formValue.recordCount ? Number(formValue.recordCount) : null,
        dailyQuotaRemaining: formValue.dailyQuotaRemaining ? Number(formValue.dailyQuotaRemaining) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.brickEconomyTransactionService.PutBrickEconomyTransaction(brickEconomyTransactionSubmitData.id, brickEconomyTransactionSubmitData)
      : this.brickEconomyTransactionService.PostBrickEconomyTransaction(brickEconomyTransactionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBrickEconomyTransactionData) => {

        this.brickEconomyTransactionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Brick Economy Transaction's detail page
          //
          this.brickEconomyTransactionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.brickEconomyTransactionForm.markAsUntouched();

          this.router.navigate(['/brickeconomytransactions', savedBrickEconomyTransactionData.id]);
          this.alertService.showMessage('Brick Economy Transaction added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.brickEconomyTransactionData = savedBrickEconomyTransactionData;
          this.buildFormValues(this.brickEconomyTransactionData);

          this.alertService.showMessage("Brick Economy Transaction saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Brick Economy Transaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Economy Transaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Economy Transaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBrickEconomyTransactionReader(): boolean {
    return this.brickEconomyTransactionService.userIsBMCBrickEconomyTransactionReader();
  }

  public userIsBMCBrickEconomyTransactionWriter(): boolean {
    return this.brickEconomyTransactionService.userIsBMCBrickEconomyTransactionWriter();
  }
}
