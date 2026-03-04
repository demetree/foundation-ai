/*
   GENERATED FORM FOR THE PAYMENTMETHOD TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PaymentMethod table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to payment-method-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PaymentMethodService, PaymentMethodData, PaymentMethodSubmitData } from '../../../scheduler-data-services/payment-method.service';
import { PaymentTransactionService } from '../../../scheduler-data-services/payment-transaction.service';
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
interface PaymentMethodFormValues {
  name: string,
  description: string,
  isElectronic: boolean,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  color: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-payment-method-detail',
  templateUrl: './payment-method-detail.component.html',
  styleUrls: ['./payment-method-detail.component.scss']
})

export class PaymentMethodDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PaymentMethodFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public paymentMethodForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        isElectronic: [false],
        sequence: [''],
        color: [''],
        active: [true],
        deleted: [false],
      });


  public paymentMethodId: string | null = null;
  public paymentMethodData: PaymentMethodData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  paymentMethods$ = this.paymentMethodService.GetPaymentMethodList();
  public paymentTransactions$ = this.paymentTransactionService.GetPaymentTransactionList();

  private destroy$ = new Subject<void>();

  constructor(
    public paymentMethodService: PaymentMethodService,
    public paymentTransactionService: PaymentTransactionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the paymentMethodId from the route parameters
    this.paymentMethodId = this.route.snapshot.paramMap.get('paymentMethodId');

    if (this.paymentMethodId === 'new' ||
        this.paymentMethodId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.paymentMethodData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.paymentMethodForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.paymentMethodForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Payment Method';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Payment Method';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.paymentMethodForm.dirty) {
      return confirm('You have unsaved Payment Method changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.paymentMethodId != null && this.paymentMethodId !== 'new') {

      const id = parseInt(this.paymentMethodId, 10);

      if (!isNaN(id)) {
        return { paymentMethodId: id };
      }
    }

    return null;
  }


/*
  * Loads the PaymentMethod data for the current paymentMethodId.
  *
  * Fully respects the PaymentMethodService caching strategy and error handling strategy.
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
    if (!this.paymentMethodService.userIsSchedulerPaymentMethodReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read PaymentMethods.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate paymentMethodId
    //
    if (!this.paymentMethodId) {

      this.alertService.showMessage('No PaymentMethod ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const paymentMethodId = Number(this.paymentMethodId);

    if (isNaN(paymentMethodId) || paymentMethodId <= 0) {

      this.alertService.showMessage(`Invalid Payment Method ID: "${this.paymentMethodId}"`,
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
      // This is the most targeted way: clear only this PaymentMethod + relations

      this.paymentMethodService.ClearRecordCache(paymentMethodId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.paymentMethodService.GetPaymentMethod(paymentMethodId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (paymentMethodData) => {

        //
        // Success path — paymentMethodData can legitimately be null if 404'd but request succeeded
        //
        if (!paymentMethodData) {

          this.handlePaymentMethodNotFound(paymentMethodId);

        } else {

          this.paymentMethodData = paymentMethodData;
          this.buildFormValues(this.paymentMethodData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'PaymentMethod loaded successfully',
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
        this.handlePaymentMethodLoadError(error, paymentMethodId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePaymentMethodNotFound(paymentMethodId: number): void {

    this.paymentMethodData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `PaymentMethod #${paymentMethodId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePaymentMethodLoadError(error: any, paymentMethodId: number): void {

    let message = 'Failed to load Payment Method.';
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
          message = 'You do not have permission to view this Payment Method.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Payment Method #${paymentMethodId} was not found.`;
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

    console.error(`Payment Method load failed (ID: ${paymentMethodId})`, error);

    //
    // Reset UI to safe state
    //
    this.paymentMethodData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(paymentMethodData: PaymentMethodData | null) {

    if (paymentMethodData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.paymentMethodForm.reset({
        name: '',
        description: '',
        isElectronic: false,
        sequence: '',
        color: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.paymentMethodForm.reset({
        name: paymentMethodData.name ?? '',
        description: paymentMethodData.description ?? '',
        isElectronic: paymentMethodData.isElectronic ?? false,
        sequence: paymentMethodData.sequence?.toString() ?? '',
        color: paymentMethodData.color ?? '',
        active: paymentMethodData.active ?? true,
        deleted: paymentMethodData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.paymentMethodForm.markAsPristine();
    this.paymentMethodForm.markAsUntouched();
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

    if (this.paymentMethodService.userIsSchedulerPaymentMethodWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Payment Methods", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.paymentMethodForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.paymentMethodForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.paymentMethodForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const paymentMethodSubmitData: PaymentMethodSubmitData = {
        id: this.paymentMethodData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        isElectronic: !!formValue.isElectronic,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.paymentMethodService.PutPaymentMethod(paymentMethodSubmitData.id, paymentMethodSubmitData)
      : this.paymentMethodService.PostPaymentMethod(paymentMethodSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPaymentMethodData) => {

        this.paymentMethodService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Payment Method's detail page
          //
          this.paymentMethodForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.paymentMethodForm.markAsUntouched();

          this.router.navigate(['/paymentmethods', savedPaymentMethodData.id]);
          this.alertService.showMessage('Payment Method added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.paymentMethodData = savedPaymentMethodData;
          this.buildFormValues(this.paymentMethodData);

          this.alertService.showMessage("Payment Method saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Payment Method.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Payment Method.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Payment Method could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerPaymentMethodReader(): boolean {
    return this.paymentMethodService.userIsSchedulerPaymentMethodReader();
  }

  public userIsSchedulerPaymentMethodWriter(): boolean {
    return this.paymentMethodService.userIsSchedulerPaymentMethodWriter();
  }
}
