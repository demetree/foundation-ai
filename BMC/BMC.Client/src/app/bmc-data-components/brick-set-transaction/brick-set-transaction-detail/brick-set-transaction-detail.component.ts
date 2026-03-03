/*
   GENERATED FORM FOR THE BRICKSETTRANSACTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickSetTransaction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-set-transaction-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickSetTransactionService, BrickSetTransactionData, BrickSetTransactionSubmitData } from '../../../bmc-data-services/brick-set-transaction.service';
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
interface BrickSetTransactionFormValues {
  transactionDate: string | null,
  direction: string,
  methodName: string,
  requestSummary: string | null,
  success: boolean,
  errorMessage: string | null,
  triggeredBy: string,
  recordCount: string | null,     // Stored as string for form input, converted to number on submit.
  apiCallsRemaining: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-brick-set-transaction-detail',
  templateUrl: './brick-set-transaction-detail.component.html',
  styleUrls: ['./brick-set-transaction-detail.component.scss']
})

export class BrickSetTransactionDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickSetTransactionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickSetTransactionForm: FormGroup = this.fb.group({
        transactionDate: [''],
        direction: ['', Validators.required],
        methodName: ['', Validators.required],
        requestSummary: [''],
        success: [false],
        errorMessage: [''],
        triggeredBy: ['', Validators.required],
        recordCount: [''],
        apiCallsRemaining: [''],
        active: [true],
        deleted: [false],
      });


  public brickSetTransactionId: string | null = null;
  public brickSetTransactionData: BrickSetTransactionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  brickSetTransactions$ = this.brickSetTransactionService.GetBrickSetTransactionList();

  private destroy$ = new Subject<void>();

  constructor(
    public brickSetTransactionService: BrickSetTransactionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the brickSetTransactionId from the route parameters
    this.brickSetTransactionId = this.route.snapshot.paramMap.get('brickSetTransactionId');

    if (this.brickSetTransactionId === 'new' ||
        this.brickSetTransactionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.brickSetTransactionData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.brickSetTransactionForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickSetTransactionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Brick Set Transaction';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Brick Set Transaction';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.brickSetTransactionForm.dirty) {
      return confirm('You have unsaved Brick Set Transaction changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.brickSetTransactionId != null && this.brickSetTransactionId !== 'new') {

      const id = parseInt(this.brickSetTransactionId, 10);

      if (!isNaN(id)) {
        return { brickSetTransactionId: id };
      }
    }

    return null;
  }


/*
  * Loads the BrickSetTransaction data for the current brickSetTransactionId.
  *
  * Fully respects the BrickSetTransactionService caching strategy and error handling strategy.
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
    if (!this.brickSetTransactionService.userIsBMCBrickSetTransactionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BrickSetTransactions.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate brickSetTransactionId
    //
    if (!this.brickSetTransactionId) {

      this.alertService.showMessage('No BrickSetTransaction ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const brickSetTransactionId = Number(this.brickSetTransactionId);

    if (isNaN(brickSetTransactionId) || brickSetTransactionId <= 0) {

      this.alertService.showMessage(`Invalid Brick Set Transaction ID: "${this.brickSetTransactionId}"`,
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
      // This is the most targeted way: clear only this BrickSetTransaction + relations

      this.brickSetTransactionService.ClearRecordCache(brickSetTransactionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.brickSetTransactionService.GetBrickSetTransaction(brickSetTransactionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (brickSetTransactionData) => {

        //
        // Success path — brickSetTransactionData can legitimately be null if 404'd but request succeeded
        //
        if (!brickSetTransactionData) {

          this.handleBrickSetTransactionNotFound(brickSetTransactionId);

        } else {

          this.brickSetTransactionData = brickSetTransactionData;
          this.buildFormValues(this.brickSetTransactionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BrickSetTransaction loaded successfully',
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
        this.handleBrickSetTransactionLoadError(error, brickSetTransactionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBrickSetTransactionNotFound(brickSetTransactionId: number): void {

    this.brickSetTransactionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BrickSetTransaction #${brickSetTransactionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBrickSetTransactionLoadError(error: any, brickSetTransactionId: number): void {

    let message = 'Failed to load Brick Set Transaction.';
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
          message = 'You do not have permission to view this Brick Set Transaction.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Brick Set Transaction #${brickSetTransactionId} was not found.`;
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

    console.error(`Brick Set Transaction load failed (ID: ${brickSetTransactionId})`, error);

    //
    // Reset UI to safe state
    //
    this.brickSetTransactionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(brickSetTransactionData: BrickSetTransactionData | null) {

    if (brickSetTransactionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickSetTransactionForm.reset({
        transactionDate: '',
        direction: '',
        methodName: '',
        requestSummary: '',
        success: false,
        errorMessage: '',
        triggeredBy: '',
        recordCount: '',
        apiCallsRemaining: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickSetTransactionForm.reset({
        transactionDate: isoUtcStringToDateTimeLocal(brickSetTransactionData.transactionDate) ?? '',
        direction: brickSetTransactionData.direction ?? '',
        methodName: brickSetTransactionData.methodName ?? '',
        requestSummary: brickSetTransactionData.requestSummary ?? '',
        success: brickSetTransactionData.success ?? false,
        errorMessage: brickSetTransactionData.errorMessage ?? '',
        triggeredBy: brickSetTransactionData.triggeredBy ?? '',
        recordCount: brickSetTransactionData.recordCount?.toString() ?? '',
        apiCallsRemaining: brickSetTransactionData.apiCallsRemaining?.toString() ?? '',
        active: brickSetTransactionData.active ?? true,
        deleted: brickSetTransactionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickSetTransactionForm.markAsPristine();
    this.brickSetTransactionForm.markAsUntouched();
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

    if (this.brickSetTransactionService.userIsBMCBrickSetTransactionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Brick Set Transactions", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.brickSetTransactionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickSetTransactionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickSetTransactionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickSetTransactionSubmitData: BrickSetTransactionSubmitData = {
        id: this.brickSetTransactionData?.id || 0,
        transactionDate: formValue.transactionDate ? dateTimeLocalToIsoUtc(formValue.transactionDate.trim()) : null,
        direction: formValue.direction!.trim(),
        methodName: formValue.methodName!.trim(),
        requestSummary: formValue.requestSummary?.trim() || null,
        success: !!formValue.success,
        errorMessage: formValue.errorMessage?.trim() || null,
        triggeredBy: formValue.triggeredBy!.trim(),
        recordCount: formValue.recordCount ? Number(formValue.recordCount) : null,
        apiCallsRemaining: formValue.apiCallsRemaining ? Number(formValue.apiCallsRemaining) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.brickSetTransactionService.PutBrickSetTransaction(brickSetTransactionSubmitData.id, brickSetTransactionSubmitData)
      : this.brickSetTransactionService.PostBrickSetTransaction(brickSetTransactionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBrickSetTransactionData) => {

        this.brickSetTransactionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Brick Set Transaction's detail page
          //
          this.brickSetTransactionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.brickSetTransactionForm.markAsUntouched();

          this.router.navigate(['/bricksettransactions', savedBrickSetTransactionData.id]);
          this.alertService.showMessage('Brick Set Transaction added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.brickSetTransactionData = savedBrickSetTransactionData;
          this.buildFormValues(this.brickSetTransactionData);

          this.alertService.showMessage("Brick Set Transaction saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Brick Set Transaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Set Transaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Set Transaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBrickSetTransactionReader(): boolean {
    return this.brickSetTransactionService.userIsBMCBrickSetTransactionReader();
  }

  public userIsBMCBrickSetTransactionWriter(): boolean {
    return this.brickSetTransactionService.userIsBMCBrickSetTransactionWriter();
  }
}
