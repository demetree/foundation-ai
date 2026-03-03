/*
   GENERATED FORM FOR THE BRICKLINKTRANSACTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickLinkTransaction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-link-transaction-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickLinkTransactionService, BrickLinkTransactionData, BrickLinkTransactionSubmitData } from '../../../bmc-data-services/brick-link-transaction.service';
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
interface BrickLinkTransactionFormValues {
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
  selector: 'app-brick-link-transaction-detail',
  templateUrl: './brick-link-transaction-detail.component.html',
  styleUrls: ['./brick-link-transaction-detail.component.scss']
})

export class BrickLinkTransactionDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickLinkTransactionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickLinkTransactionForm: FormGroup = this.fb.group({
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


  public brickLinkTransactionId: string | null = null;
  public brickLinkTransactionData: BrickLinkTransactionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  brickLinkTransactions$ = this.brickLinkTransactionService.GetBrickLinkTransactionList();

  private destroy$ = new Subject<void>();

  constructor(
    public brickLinkTransactionService: BrickLinkTransactionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the brickLinkTransactionId from the route parameters
    this.brickLinkTransactionId = this.route.snapshot.paramMap.get('brickLinkTransactionId');

    if (this.brickLinkTransactionId === 'new' ||
        this.brickLinkTransactionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.brickLinkTransactionData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.brickLinkTransactionForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickLinkTransactionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Brick Link Transaction';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Brick Link Transaction';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.brickLinkTransactionForm.dirty) {
      return confirm('You have unsaved Brick Link Transaction changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.brickLinkTransactionId != null && this.brickLinkTransactionId !== 'new') {

      const id = parseInt(this.brickLinkTransactionId, 10);

      if (!isNaN(id)) {
        return { brickLinkTransactionId: id };
      }
    }

    return null;
  }


/*
  * Loads the BrickLinkTransaction data for the current brickLinkTransactionId.
  *
  * Fully respects the BrickLinkTransactionService caching strategy and error handling strategy.
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
    if (!this.brickLinkTransactionService.userIsBMCBrickLinkTransactionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BrickLinkTransactions.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate brickLinkTransactionId
    //
    if (!this.brickLinkTransactionId) {

      this.alertService.showMessage('No BrickLinkTransaction ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const brickLinkTransactionId = Number(this.brickLinkTransactionId);

    if (isNaN(brickLinkTransactionId) || brickLinkTransactionId <= 0) {

      this.alertService.showMessage(`Invalid Brick Link Transaction ID: "${this.brickLinkTransactionId}"`,
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
      // This is the most targeted way: clear only this BrickLinkTransaction + relations

      this.brickLinkTransactionService.ClearRecordCache(brickLinkTransactionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.brickLinkTransactionService.GetBrickLinkTransaction(brickLinkTransactionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (brickLinkTransactionData) => {

        //
        // Success path — brickLinkTransactionData can legitimately be null if 404'd but request succeeded
        //
        if (!brickLinkTransactionData) {

          this.handleBrickLinkTransactionNotFound(brickLinkTransactionId);

        } else {

          this.brickLinkTransactionData = brickLinkTransactionData;
          this.buildFormValues(this.brickLinkTransactionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BrickLinkTransaction loaded successfully',
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
        this.handleBrickLinkTransactionLoadError(error, brickLinkTransactionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBrickLinkTransactionNotFound(brickLinkTransactionId: number): void {

    this.brickLinkTransactionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BrickLinkTransaction #${brickLinkTransactionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBrickLinkTransactionLoadError(error: any, brickLinkTransactionId: number): void {

    let message = 'Failed to load Brick Link Transaction.';
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
          message = 'You do not have permission to view this Brick Link Transaction.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Brick Link Transaction #${brickLinkTransactionId} was not found.`;
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

    console.error(`Brick Link Transaction load failed (ID: ${brickLinkTransactionId})`, error);

    //
    // Reset UI to safe state
    //
    this.brickLinkTransactionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(brickLinkTransactionData: BrickLinkTransactionData | null) {

    if (brickLinkTransactionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickLinkTransactionForm.reset({
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
        this.brickLinkTransactionForm.reset({
        transactionDate: isoUtcStringToDateTimeLocal(brickLinkTransactionData.transactionDate) ?? '',
        direction: brickLinkTransactionData.direction ?? '',
        methodName: brickLinkTransactionData.methodName ?? '',
        requestSummary: brickLinkTransactionData.requestSummary ?? '',
        success: brickLinkTransactionData.success ?? false,
        errorMessage: brickLinkTransactionData.errorMessage ?? '',
        triggeredBy: brickLinkTransactionData.triggeredBy ?? '',
        recordCount: brickLinkTransactionData.recordCount?.toString() ?? '',
        active: brickLinkTransactionData.active ?? true,
        deleted: brickLinkTransactionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickLinkTransactionForm.markAsPristine();
    this.brickLinkTransactionForm.markAsUntouched();
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

    if (this.brickLinkTransactionService.userIsBMCBrickLinkTransactionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Brick Link Transactions", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.brickLinkTransactionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickLinkTransactionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickLinkTransactionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickLinkTransactionSubmitData: BrickLinkTransactionSubmitData = {
        id: this.brickLinkTransactionData?.id || 0,
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
      ? this.brickLinkTransactionService.PutBrickLinkTransaction(brickLinkTransactionSubmitData.id, brickLinkTransactionSubmitData)
      : this.brickLinkTransactionService.PostBrickLinkTransaction(brickLinkTransactionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBrickLinkTransactionData) => {

        this.brickLinkTransactionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Brick Link Transaction's detail page
          //
          this.brickLinkTransactionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.brickLinkTransactionForm.markAsUntouched();

          this.router.navigate(['/bricklinktransactions', savedBrickLinkTransactionData.id]);
          this.alertService.showMessage('Brick Link Transaction added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.brickLinkTransactionData = savedBrickLinkTransactionData;
          this.buildFormValues(this.brickLinkTransactionData);

          this.alertService.showMessage("Brick Link Transaction saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Brick Link Transaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Link Transaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Link Transaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBrickLinkTransactionReader(): boolean {
    return this.brickLinkTransactionService.userIsBMCBrickLinkTransactionReader();
  }

  public userIsBMCBrickLinkTransactionWriter(): boolean {
    return this.brickLinkTransactionService.userIsBMCBrickLinkTransactionWriter();
  }
}
