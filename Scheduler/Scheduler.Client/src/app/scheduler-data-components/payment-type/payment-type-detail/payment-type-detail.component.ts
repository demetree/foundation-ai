/*
   GENERATED FORM FOR THE PAYMENTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PaymentType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to payment-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PaymentTypeService, PaymentTypeData, PaymentTypeSubmitData } from '../../../scheduler-data-services/payment-type.service';
import { GiftService } from '../../../scheduler-data-services/gift.service';
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
interface PaymentTypeFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-payment-type-detail',
  templateUrl: './payment-type-detail.component.html',
  styleUrls: ['./payment-type-detail.component.scss']
})

export class PaymentTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PaymentTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public paymentTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public paymentTypeId: string | null = null;
  public paymentTypeData: PaymentTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  paymentTypes$ = this.paymentTypeService.GetPaymentTypeList();
  public gifts$ = this.giftService.GetGiftList();

  private destroy$ = new Subject<void>();

  constructor(
    public paymentTypeService: PaymentTypeService,
    public giftService: GiftService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the paymentTypeId from the route parameters
    this.paymentTypeId = this.route.snapshot.paramMap.get('paymentTypeId');

    if (this.paymentTypeId === 'new' ||
        this.paymentTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.paymentTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.paymentTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.paymentTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Payment Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Payment Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.paymentTypeForm.dirty) {
      return confirm('You have unsaved Payment Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.paymentTypeId != null && this.paymentTypeId !== 'new') {

      const id = parseInt(this.paymentTypeId, 10);

      if (!isNaN(id)) {
        return { paymentTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the PaymentType data for the current paymentTypeId.
  *
  * Fully respects the PaymentTypeService caching strategy and error handling strategy.
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
    if (!this.paymentTypeService.userIsSchedulerPaymentTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read PaymentTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate paymentTypeId
    //
    if (!this.paymentTypeId) {

      this.alertService.showMessage('No PaymentType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const paymentTypeId = Number(this.paymentTypeId);

    if (isNaN(paymentTypeId) || paymentTypeId <= 0) {

      this.alertService.showMessage(`Invalid Payment Type ID: "${this.paymentTypeId}"`,
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
      // This is the most targeted way: clear only this PaymentType + relations

      this.paymentTypeService.ClearRecordCache(paymentTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.paymentTypeService.GetPaymentType(paymentTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (paymentTypeData) => {

        //
        // Success path — paymentTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!paymentTypeData) {

          this.handlePaymentTypeNotFound(paymentTypeId);

        } else {

          this.paymentTypeData = paymentTypeData;
          this.buildFormValues(this.paymentTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'PaymentType loaded successfully',
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
        this.handlePaymentTypeLoadError(error, paymentTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePaymentTypeNotFound(paymentTypeId: number): void {

    this.paymentTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `PaymentType #${paymentTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePaymentTypeLoadError(error: any, paymentTypeId: number): void {

    let message = 'Failed to load Payment Type.';
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
          message = 'You do not have permission to view this Payment Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Payment Type #${paymentTypeId} was not found.`;
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

    console.error(`Payment Type load failed (ID: ${paymentTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.paymentTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(paymentTypeData: PaymentTypeData | null) {

    if (paymentTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.paymentTypeForm.reset({
        name: '',
        description: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.paymentTypeForm.reset({
        name: paymentTypeData.name ?? '',
        description: paymentTypeData.description ?? '',
        sequence: paymentTypeData.sequence?.toString() ?? '',
        active: paymentTypeData.active ?? true,
        deleted: paymentTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.paymentTypeForm.markAsPristine();
    this.paymentTypeForm.markAsUntouched();
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

    if (this.paymentTypeService.userIsSchedulerPaymentTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Payment Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.paymentTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.paymentTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.paymentTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const paymentTypeSubmitData: PaymentTypeSubmitData = {
        id: this.paymentTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.paymentTypeService.PutPaymentType(paymentTypeSubmitData.id, paymentTypeSubmitData)
      : this.paymentTypeService.PostPaymentType(paymentTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPaymentTypeData) => {

        this.paymentTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Payment Type's detail page
          //
          this.paymentTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.paymentTypeForm.markAsUntouched();

          this.router.navigate(['/paymenttypes', savedPaymentTypeData.id]);
          this.alertService.showMessage('Payment Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.paymentTypeData = savedPaymentTypeData;
          this.buildFormValues(this.paymentTypeData);

          this.alertService.showMessage("Payment Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Payment Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Payment Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Payment Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerPaymentTypeReader(): boolean {
    return this.paymentTypeService.userIsSchedulerPaymentTypeReader();
  }

  public userIsSchedulerPaymentTypeWriter(): boolean {
    return this.paymentTypeService.userIsSchedulerPaymentTypeWriter();
  }
}
