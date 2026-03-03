/*
   GENERATED FORM FOR THE BRICKOWLTRANSACTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickOwlTransaction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-owl-transaction-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickOwlTransactionService, BrickOwlTransactionData, BrickOwlTransactionSubmitData } from '../../../bmc-data-services/brick-owl-transaction.service';
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
interface BrickOwlTransactionFormValues {
  transactionDate: string | null,
  direction: string,
  methodName: string,
  requestSummary: string | null,
  success: boolean,
  errorMessage: string | null,
  triggeredBy: string,
  recordCount: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-brick-owl-transaction-detail',
  templateUrl: './brick-owl-transaction-detail.component.html',
  styleUrls: ['./brick-owl-transaction-detail.component.scss']
})

export class BrickOwlTransactionDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickOwlTransactionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickOwlTransactionForm: FormGroup = this.fb.group({
        transactionDate: [''],
        direction: ['', Validators.required],
        methodName: ['', Validators.required],
        requestSummary: [''],
        success: [false],
        errorMessage: [''],
        triggeredBy: ['', Validators.required],
        recordCount: [''],
        active: [true],
        deleted: [false],
      });


  public brickOwlTransactionId: string | null = null;
  public brickOwlTransactionData: BrickOwlTransactionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  brickOwlTransactions$ = this.brickOwlTransactionService.GetBrickOwlTransactionList();

  private destroy$ = new Subject<void>();

  constructor(
    public brickOwlTransactionService: BrickOwlTransactionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the brickOwlTransactionId from the route parameters
    this.brickOwlTransactionId = this.route.snapshot.paramMap.get('brickOwlTransactionId');

    if (this.brickOwlTransactionId === 'new' ||
        this.brickOwlTransactionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.brickOwlTransactionData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.brickOwlTransactionForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickOwlTransactionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Brick Owl Transaction';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Brick Owl Transaction';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.brickOwlTransactionForm.dirty) {
      return confirm('You have unsaved Brick Owl Transaction changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.brickOwlTransactionId != null && this.brickOwlTransactionId !== 'new') {

      const id = parseInt(this.brickOwlTransactionId, 10);

      if (!isNaN(id)) {
        return { brickOwlTransactionId: id };
      }
    }

    return null;
  }


/*
  * Loads the BrickOwlTransaction data for the current brickOwlTransactionId.
  *
  * Fully respects the BrickOwlTransactionService caching strategy and error handling strategy.
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
    if (!this.brickOwlTransactionService.userIsBMCBrickOwlTransactionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BrickOwlTransactions.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate brickOwlTransactionId
    //
    if (!this.brickOwlTransactionId) {

      this.alertService.showMessage('No BrickOwlTransaction ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const brickOwlTransactionId = Number(this.brickOwlTransactionId);

    if (isNaN(brickOwlTransactionId) || brickOwlTransactionId <= 0) {

      this.alertService.showMessage(`Invalid Brick Owl Transaction ID: "${this.brickOwlTransactionId}"`,
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
      // This is the most targeted way: clear only this BrickOwlTransaction + relations

      this.brickOwlTransactionService.ClearRecordCache(brickOwlTransactionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.brickOwlTransactionService.GetBrickOwlTransaction(brickOwlTransactionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (brickOwlTransactionData) => {

        //
        // Success path — brickOwlTransactionData can legitimately be null if 404'd but request succeeded
        //
        if (!brickOwlTransactionData) {

          this.handleBrickOwlTransactionNotFound(brickOwlTransactionId);

        } else {

          this.brickOwlTransactionData = brickOwlTransactionData;
          this.buildFormValues(this.brickOwlTransactionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BrickOwlTransaction loaded successfully',
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
        this.handleBrickOwlTransactionLoadError(error, brickOwlTransactionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBrickOwlTransactionNotFound(brickOwlTransactionId: number): void {

    this.brickOwlTransactionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BrickOwlTransaction #${brickOwlTransactionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBrickOwlTransactionLoadError(error: any, brickOwlTransactionId: number): void {

    let message = 'Failed to load Brick Owl Transaction.';
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
          message = 'You do not have permission to view this Brick Owl Transaction.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Brick Owl Transaction #${brickOwlTransactionId} was not found.`;
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

    console.error(`Brick Owl Transaction load failed (ID: ${brickOwlTransactionId})`, error);

    //
    // Reset UI to safe state
    //
    this.brickOwlTransactionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(brickOwlTransactionData: BrickOwlTransactionData | null) {

    if (brickOwlTransactionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickOwlTransactionForm.reset({
        transactionDate: '',
        direction: '',
        methodName: '',
        requestSummary: '',
        success: false,
        errorMessage: '',
        triggeredBy: '',
        recordCount: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickOwlTransactionForm.reset({
        transactionDate: isoUtcStringToDateTimeLocal(brickOwlTransactionData.transactionDate) ?? '',
        direction: brickOwlTransactionData.direction ?? '',
        methodName: brickOwlTransactionData.methodName ?? '',
        requestSummary: brickOwlTransactionData.requestSummary ?? '',
        success: brickOwlTransactionData.success ?? false,
        errorMessage: brickOwlTransactionData.errorMessage ?? '',
        triggeredBy: brickOwlTransactionData.triggeredBy ?? '',
        recordCount: brickOwlTransactionData.recordCount?.toString() ?? '',
        active: brickOwlTransactionData.active ?? true,
        deleted: brickOwlTransactionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickOwlTransactionForm.markAsPristine();
    this.brickOwlTransactionForm.markAsUntouched();
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

    if (this.brickOwlTransactionService.userIsBMCBrickOwlTransactionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Brick Owl Transactions", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.brickOwlTransactionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickOwlTransactionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickOwlTransactionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickOwlTransactionSubmitData: BrickOwlTransactionSubmitData = {
        id: this.brickOwlTransactionData?.id || 0,
        transactionDate: formValue.transactionDate ? dateTimeLocalToIsoUtc(formValue.transactionDate.trim()) : null,
        direction: formValue.direction!.trim(),
        methodName: formValue.methodName!.trim(),
        requestSummary: formValue.requestSummary?.trim() || null,
        success: !!formValue.success,
        errorMessage: formValue.errorMessage?.trim() || null,
        triggeredBy: formValue.triggeredBy!.trim(),
        recordCount: formValue.recordCount ? Number(formValue.recordCount) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.brickOwlTransactionService.PutBrickOwlTransaction(brickOwlTransactionSubmitData.id, brickOwlTransactionSubmitData)
      : this.brickOwlTransactionService.PostBrickOwlTransaction(brickOwlTransactionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBrickOwlTransactionData) => {

        this.brickOwlTransactionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Brick Owl Transaction's detail page
          //
          this.brickOwlTransactionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.brickOwlTransactionForm.markAsUntouched();

          this.router.navigate(['/brickowltransactions', savedBrickOwlTransactionData.id]);
          this.alertService.showMessage('Brick Owl Transaction added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.brickOwlTransactionData = savedBrickOwlTransactionData;
          this.buildFormValues(this.brickOwlTransactionData);

          this.alertService.showMessage("Brick Owl Transaction saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Brick Owl Transaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Owl Transaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Owl Transaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBrickOwlTransactionReader(): boolean {
    return this.brickOwlTransactionService.userIsBMCBrickOwlTransactionReader();
  }

  public userIsBMCBrickOwlTransactionWriter(): boolean {
    return this.brickOwlTransactionService.userIsBMCBrickOwlTransactionWriter();
  }
}
