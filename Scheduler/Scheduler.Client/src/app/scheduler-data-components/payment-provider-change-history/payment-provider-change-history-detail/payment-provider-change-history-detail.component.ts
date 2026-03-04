/*
   GENERATED FORM FOR THE PAYMENTPROVIDERCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PaymentProviderChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to payment-provider-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PaymentProviderChangeHistoryService, PaymentProviderChangeHistoryData, PaymentProviderChangeHistorySubmitData } from '../../../scheduler-data-services/payment-provider-change-history.service';
import { PaymentProviderService } from '../../../scheduler-data-services/payment-provider.service';
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
interface PaymentProviderChangeHistoryFormValues {
  paymentProviderId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-payment-provider-change-history-detail',
  templateUrl: './payment-provider-change-history-detail.component.html',
  styleUrls: ['./payment-provider-change-history-detail.component.scss']
})

export class PaymentProviderChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PaymentProviderChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public paymentProviderChangeHistoryForm: FormGroup = this.fb.group({
        paymentProviderId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public paymentProviderChangeHistoryId: string | null = null;
  public paymentProviderChangeHistoryData: PaymentProviderChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  paymentProviderChangeHistories$ = this.paymentProviderChangeHistoryService.GetPaymentProviderChangeHistoryList();
  public paymentProviders$ = this.paymentProviderService.GetPaymentProviderList();

  private destroy$ = new Subject<void>();

  constructor(
    public paymentProviderChangeHistoryService: PaymentProviderChangeHistoryService,
    public paymentProviderService: PaymentProviderService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the paymentProviderChangeHistoryId from the route parameters
    this.paymentProviderChangeHistoryId = this.route.snapshot.paramMap.get('paymentProviderChangeHistoryId');

    if (this.paymentProviderChangeHistoryId === 'new' ||
        this.paymentProviderChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.paymentProviderChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.paymentProviderChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.paymentProviderChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Payment Provider Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Payment Provider Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.paymentProviderChangeHistoryForm.dirty) {
      return confirm('You have unsaved Payment Provider Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.paymentProviderChangeHistoryId != null && this.paymentProviderChangeHistoryId !== 'new') {

      const id = parseInt(this.paymentProviderChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { paymentProviderChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the PaymentProviderChangeHistory data for the current paymentProviderChangeHistoryId.
  *
  * Fully respects the PaymentProviderChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.paymentProviderChangeHistoryService.userIsSchedulerPaymentProviderChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read PaymentProviderChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate paymentProviderChangeHistoryId
    //
    if (!this.paymentProviderChangeHistoryId) {

      this.alertService.showMessage('No PaymentProviderChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const paymentProviderChangeHistoryId = Number(this.paymentProviderChangeHistoryId);

    if (isNaN(paymentProviderChangeHistoryId) || paymentProviderChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Payment Provider Change History ID: "${this.paymentProviderChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this PaymentProviderChangeHistory + relations

      this.paymentProviderChangeHistoryService.ClearRecordCache(paymentProviderChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.paymentProviderChangeHistoryService.GetPaymentProviderChangeHistory(paymentProviderChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (paymentProviderChangeHistoryData) => {

        //
        // Success path — paymentProviderChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!paymentProviderChangeHistoryData) {

          this.handlePaymentProviderChangeHistoryNotFound(paymentProviderChangeHistoryId);

        } else {

          this.paymentProviderChangeHistoryData = paymentProviderChangeHistoryData;
          this.buildFormValues(this.paymentProviderChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'PaymentProviderChangeHistory loaded successfully',
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
        this.handlePaymentProviderChangeHistoryLoadError(error, paymentProviderChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePaymentProviderChangeHistoryNotFound(paymentProviderChangeHistoryId: number): void {

    this.paymentProviderChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `PaymentProviderChangeHistory #${paymentProviderChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePaymentProviderChangeHistoryLoadError(error: any, paymentProviderChangeHistoryId: number): void {

    let message = 'Failed to load Payment Provider Change History.';
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
          message = 'You do not have permission to view this Payment Provider Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Payment Provider Change History #${paymentProviderChangeHistoryId} was not found.`;
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

    console.error(`Payment Provider Change History load failed (ID: ${paymentProviderChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.paymentProviderChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(paymentProviderChangeHistoryData: PaymentProviderChangeHistoryData | null) {

    if (paymentProviderChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.paymentProviderChangeHistoryForm.reset({
        paymentProviderId: null,
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
        this.paymentProviderChangeHistoryForm.reset({
        paymentProviderId: paymentProviderChangeHistoryData.paymentProviderId,
        versionNumber: paymentProviderChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(paymentProviderChangeHistoryData.timeStamp) ?? '',
        userId: paymentProviderChangeHistoryData.userId?.toString() ?? '',
        data: paymentProviderChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.paymentProviderChangeHistoryForm.markAsPristine();
    this.paymentProviderChangeHistoryForm.markAsUntouched();
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

    if (this.paymentProviderChangeHistoryService.userIsSchedulerPaymentProviderChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Payment Provider Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.paymentProviderChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.paymentProviderChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.paymentProviderChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const paymentProviderChangeHistorySubmitData: PaymentProviderChangeHistorySubmitData = {
        id: this.paymentProviderChangeHistoryData?.id || 0,
        paymentProviderId: Number(formValue.paymentProviderId),
        versionNumber: this.paymentProviderChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.paymentProviderChangeHistoryService.PutPaymentProviderChangeHistory(paymentProviderChangeHistorySubmitData.id, paymentProviderChangeHistorySubmitData)
      : this.paymentProviderChangeHistoryService.PostPaymentProviderChangeHistory(paymentProviderChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPaymentProviderChangeHistoryData) => {

        this.paymentProviderChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Payment Provider Change History's detail page
          //
          this.paymentProviderChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.paymentProviderChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/paymentproviderchangehistories', savedPaymentProviderChangeHistoryData.id]);
          this.alertService.showMessage('Payment Provider Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.paymentProviderChangeHistoryData = savedPaymentProviderChangeHistoryData;
          this.buildFormValues(this.paymentProviderChangeHistoryData);

          this.alertService.showMessage("Payment Provider Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Payment Provider Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Payment Provider Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Payment Provider Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerPaymentProviderChangeHistoryReader(): boolean {
    return this.paymentProviderChangeHistoryService.userIsSchedulerPaymentProviderChangeHistoryReader();
  }

  public userIsSchedulerPaymentProviderChangeHistoryWriter(): boolean {
    return this.paymentProviderChangeHistoryService.userIsSchedulerPaymentProviderChangeHistoryWriter();
  }
}
