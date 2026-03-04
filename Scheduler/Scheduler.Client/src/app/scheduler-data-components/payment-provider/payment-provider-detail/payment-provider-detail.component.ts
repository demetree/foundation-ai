/*
   GENERATED FORM FOR THE PAYMENTPROVIDER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PaymentProvider table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to payment-provider-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PaymentProviderService, PaymentProviderData, PaymentProviderSubmitData } from '../../../scheduler-data-services/payment-provider.service';
import { PaymentProviderChangeHistoryService } from '../../../scheduler-data-services/payment-provider-change-history.service';
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
interface PaymentProviderFormValues {
  name: string,
  description: string,
  providerType: string,
  isActive: boolean,
  apiKeyEncrypted: string | null,
  merchantId: string | null,
  webhookSecret: string | null,
  processingFeePercent: string | null,     // Stored as string for form input, converted to number on submit.
  processingFeeFixed: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-payment-provider-detail',
  templateUrl: './payment-provider-detail.component.html',
  styleUrls: ['./payment-provider-detail.component.scss']
})

export class PaymentProviderDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PaymentProviderFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public paymentProviderForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        providerType: ['', Validators.required],
        isActive: [false],
        apiKeyEncrypted: [''],
        merchantId: [''],
        webhookSecret: [''],
        processingFeePercent: [''],
        processingFeeFixed: [''],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public paymentProviderId: string | null = null;
  public paymentProviderData: PaymentProviderData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  paymentProviders$ = this.paymentProviderService.GetPaymentProviderList();
  public paymentProviderChangeHistories$ = this.paymentProviderChangeHistoryService.GetPaymentProviderChangeHistoryList();
  public paymentTransactions$ = this.paymentTransactionService.GetPaymentTransactionList();

  private destroy$ = new Subject<void>();

  constructor(
    public paymentProviderService: PaymentProviderService,
    public paymentProviderChangeHistoryService: PaymentProviderChangeHistoryService,
    public paymentTransactionService: PaymentTransactionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the paymentProviderId from the route parameters
    this.paymentProviderId = this.route.snapshot.paramMap.get('paymentProviderId');

    if (this.paymentProviderId === 'new' ||
        this.paymentProviderId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.paymentProviderData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.paymentProviderForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.paymentProviderForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Payment Provider';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Payment Provider';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.paymentProviderForm.dirty) {
      return confirm('You have unsaved Payment Provider changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.paymentProviderId != null && this.paymentProviderId !== 'new') {

      const id = parseInt(this.paymentProviderId, 10);

      if (!isNaN(id)) {
        return { paymentProviderId: id };
      }
    }

    return null;
  }


/*
  * Loads the PaymentProvider data for the current paymentProviderId.
  *
  * Fully respects the PaymentProviderService caching strategy and error handling strategy.
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
    if (!this.paymentProviderService.userIsSchedulerPaymentProviderReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read PaymentProviders.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate paymentProviderId
    //
    if (!this.paymentProviderId) {

      this.alertService.showMessage('No PaymentProvider ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const paymentProviderId = Number(this.paymentProviderId);

    if (isNaN(paymentProviderId) || paymentProviderId <= 0) {

      this.alertService.showMessage(`Invalid Payment Provider ID: "${this.paymentProviderId}"`,
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
      // This is the most targeted way: clear only this PaymentProvider + relations

      this.paymentProviderService.ClearRecordCache(paymentProviderId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.paymentProviderService.GetPaymentProvider(paymentProviderId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (paymentProviderData) => {

        //
        // Success path — paymentProviderData can legitimately be null if 404'd but request succeeded
        //
        if (!paymentProviderData) {

          this.handlePaymentProviderNotFound(paymentProviderId);

        } else {

          this.paymentProviderData = paymentProviderData;
          this.buildFormValues(this.paymentProviderData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'PaymentProvider loaded successfully',
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
        this.handlePaymentProviderLoadError(error, paymentProviderId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePaymentProviderNotFound(paymentProviderId: number): void {

    this.paymentProviderData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `PaymentProvider #${paymentProviderId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePaymentProviderLoadError(error: any, paymentProviderId: number): void {

    let message = 'Failed to load Payment Provider.';
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
          message = 'You do not have permission to view this Payment Provider.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Payment Provider #${paymentProviderId} was not found.`;
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

    console.error(`Payment Provider load failed (ID: ${paymentProviderId})`, error);

    //
    // Reset UI to safe state
    //
    this.paymentProviderData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(paymentProviderData: PaymentProviderData | null) {

    if (paymentProviderData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.paymentProviderForm.reset({
        name: '',
        description: '',
        providerType: '',
        isActive: false,
        apiKeyEncrypted: '',
        merchantId: '',
        webhookSecret: '',
        processingFeePercent: '',
        processingFeeFixed: '',
        notes: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.paymentProviderForm.reset({
        name: paymentProviderData.name ?? '',
        description: paymentProviderData.description ?? '',
        providerType: paymentProviderData.providerType ?? '',
        isActive: paymentProviderData.isActive ?? false,
        apiKeyEncrypted: paymentProviderData.apiKeyEncrypted ?? '',
        merchantId: paymentProviderData.merchantId ?? '',
        webhookSecret: paymentProviderData.webhookSecret ?? '',
        processingFeePercent: paymentProviderData.processingFeePercent?.toString() ?? '',
        processingFeeFixed: paymentProviderData.processingFeeFixed?.toString() ?? '',
        notes: paymentProviderData.notes ?? '',
        versionNumber: paymentProviderData.versionNumber?.toString() ?? '',
        active: paymentProviderData.active ?? true,
        deleted: paymentProviderData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.paymentProviderForm.markAsPristine();
    this.paymentProviderForm.markAsUntouched();
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

    if (this.paymentProviderService.userIsSchedulerPaymentProviderWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Payment Providers", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.paymentProviderForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.paymentProviderForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.paymentProviderForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const paymentProviderSubmitData: PaymentProviderSubmitData = {
        id: this.paymentProviderData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        providerType: formValue.providerType!.trim(),
        isActive: !!formValue.isActive,
        apiKeyEncrypted: formValue.apiKeyEncrypted?.trim() || null,
        merchantId: formValue.merchantId?.trim() || null,
        webhookSecret: formValue.webhookSecret?.trim() || null,
        processingFeePercent: formValue.processingFeePercent ? Number(formValue.processingFeePercent) : null,
        processingFeeFixed: formValue.processingFeeFixed ? Number(formValue.processingFeeFixed) : null,
        notes: formValue.notes?.trim() || null,
        versionNumber: this.paymentProviderData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.paymentProviderService.PutPaymentProvider(paymentProviderSubmitData.id, paymentProviderSubmitData)
      : this.paymentProviderService.PostPaymentProvider(paymentProviderSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPaymentProviderData) => {

        this.paymentProviderService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Payment Provider's detail page
          //
          this.paymentProviderForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.paymentProviderForm.markAsUntouched();

          this.router.navigate(['/paymentproviders', savedPaymentProviderData.id]);
          this.alertService.showMessage('Payment Provider added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.paymentProviderData = savedPaymentProviderData;
          this.buildFormValues(this.paymentProviderData);

          this.alertService.showMessage("Payment Provider saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Payment Provider.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Payment Provider.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Payment Provider could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerPaymentProviderReader(): boolean {
    return this.paymentProviderService.userIsSchedulerPaymentProviderReader();
  }

  public userIsSchedulerPaymentProviderWriter(): boolean {
    return this.paymentProviderService.userIsSchedulerPaymentProviderWriter();
  }
}
