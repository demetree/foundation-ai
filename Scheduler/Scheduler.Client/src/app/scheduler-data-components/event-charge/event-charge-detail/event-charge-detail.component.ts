/*
   GENERATED FORM FOR THE EVENTCHARGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EventCharge table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to event-charge-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EventChargeService, EventChargeData, EventChargeSubmitData } from '../../../scheduler-data-services/event-charge.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { ChargeTypeService } from '../../../scheduler-data-services/charge-type.service';
import { ChargeStatusService } from '../../../scheduler-data-services/charge-status.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { RateTypeService } from '../../../scheduler-data-services/rate-type.service';
import { TaxCodeService } from '../../../scheduler-data-services/tax-code.service';
import { EventChargeChangeHistoryService } from '../../../scheduler-data-services/event-charge-change-history.service';
import { PaymentTransactionService } from '../../../scheduler-data-services/payment-transaction.service';
import { InvoiceLineItemService } from '../../../scheduler-data-services/invoice-line-item.service';
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
interface EventChargeFormValues {
  scheduledEventId: number | bigint,       // For FK link number
  resourceId: number | bigint | null,       // For FK link number
  chargeTypeId: number | bigint,       // For FK link number
  chargeStatusId: number | bigint,       // For FK link number
  quantity: string | null,     // Stored as string for form input, converted to number on submit.
  unitPrice: string | null,     // Stored as string for form input, converted to number on submit.
  extendedAmount: string,     // Stored as string for form input, converted to number on submit.
  taxAmount: string,     // Stored as string for form input, converted to number on submit.
  totalAmount: string,     // Stored as string for form input, converted to number on submit.
  description: string | null,
  currencyId: number | bigint,       // For FK link number
  rateTypeId: number | bigint | null,       // For FK link number
  notes: string | null,
  isAutomatic: boolean,
  isDeposit: boolean,
  depositRefundedDate: string | null,
  exportedDate: string | null,
  externalId: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
  taxCodeId: number | bigint | null,       // For FK link number
};


@Component({
  selector: 'app-event-charge-detail',
  templateUrl: './event-charge-detail.component.html',
  styleUrls: ['./event-charge-detail.component.scss']
})

export class EventChargeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EventChargeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public eventChargeForm: FormGroup = this.fb.group({
        scheduledEventId: [null, Validators.required],
        resourceId: [null],
        chargeTypeId: [null, Validators.required],
        chargeStatusId: [null, Validators.required],
        quantity: [''],
        unitPrice: [''],
        extendedAmount: ['', Validators.required],
        taxAmount: ['', Validators.required],
        totalAmount: ['', Validators.required],
        description: [''],
        currencyId: [null, Validators.required],
        rateTypeId: [null],
        notes: [''],
        isAutomatic: [false],
        isDeposit: [false],
        depositRefundedDate: [''],
        exportedDate: [''],
        externalId: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
        taxCodeId: [null],
      });


  public eventChargeId: string | null = null;
  public eventChargeData: EventChargeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  eventCharges$ = this.eventChargeService.GetEventChargeList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public resources$ = this.resourceService.GetResourceList();
  public chargeTypes$ = this.chargeTypeService.GetChargeTypeList();
  public chargeStatuses$ = this.chargeStatusService.GetChargeStatusList();
  public currencies$ = this.currencyService.GetCurrencyList();
  public rateTypes$ = this.rateTypeService.GetRateTypeList();
  public taxCodes$ = this.taxCodeService.GetTaxCodeList();
  public eventChargeChangeHistories$ = this.eventChargeChangeHistoryService.GetEventChargeChangeHistoryList();
  public paymentTransactions$ = this.paymentTransactionService.GetPaymentTransactionList();
  public invoiceLineItems$ = this.invoiceLineItemService.GetInvoiceLineItemList();

  private destroy$ = new Subject<void>();

  constructor(
    public eventChargeService: EventChargeService,
    public scheduledEventService: ScheduledEventService,
    public resourceService: ResourceService,
    public chargeTypeService: ChargeTypeService,
    public chargeStatusService: ChargeStatusService,
    public currencyService: CurrencyService,
    public rateTypeService: RateTypeService,
    public taxCodeService: TaxCodeService,
    public eventChargeChangeHistoryService: EventChargeChangeHistoryService,
    public paymentTransactionService: PaymentTransactionService,
    public invoiceLineItemService: InvoiceLineItemService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the eventChargeId from the route parameters
    this.eventChargeId = this.route.snapshot.paramMap.get('eventChargeId');

    if (this.eventChargeId === 'new' ||
        this.eventChargeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.eventChargeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.eventChargeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.eventChargeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Event Charge';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Event Charge';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.eventChargeForm.dirty) {
      return confirm('You have unsaved Event Charge changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.eventChargeId != null && this.eventChargeId !== 'new') {

      const id = parseInt(this.eventChargeId, 10);

      if (!isNaN(id)) {
        return { eventChargeId: id };
      }
    }

    return null;
  }


/*
  * Loads the EventCharge data for the current eventChargeId.
  *
  * Fully respects the EventChargeService caching strategy and error handling strategy.
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
    if (!this.eventChargeService.userIsSchedulerEventChargeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read EventCharges.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate eventChargeId
    //
    if (!this.eventChargeId) {

      this.alertService.showMessage('No EventCharge ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const eventChargeId = Number(this.eventChargeId);

    if (isNaN(eventChargeId) || eventChargeId <= 0) {

      this.alertService.showMessage(`Invalid Event Charge ID: "${this.eventChargeId}"`,
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
      // This is the most targeted way: clear only this EventCharge + relations

      this.eventChargeService.ClearRecordCache(eventChargeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.eventChargeService.GetEventCharge(eventChargeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (eventChargeData) => {

        //
        // Success path — eventChargeData can legitimately be null if 404'd but request succeeded
        //
        if (!eventChargeData) {

          this.handleEventChargeNotFound(eventChargeId);

        } else {

          this.eventChargeData = eventChargeData;
          this.buildFormValues(this.eventChargeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'EventCharge loaded successfully',
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
        this.handleEventChargeLoadError(error, eventChargeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleEventChargeNotFound(eventChargeId: number): void {

    this.eventChargeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `EventCharge #${eventChargeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleEventChargeLoadError(error: any, eventChargeId: number): void {

    let message = 'Failed to load Event Charge.';
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
          message = 'You do not have permission to view this Event Charge.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Event Charge #${eventChargeId} was not found.`;
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

    console.error(`Event Charge load failed (ID: ${eventChargeId})`, error);

    //
    // Reset UI to safe state
    //
    this.eventChargeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(eventChargeData: EventChargeData | null) {

    if (eventChargeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.eventChargeForm.reset({
        scheduledEventId: null,
        resourceId: null,
        chargeTypeId: null,
        chargeStatusId: null,
        quantity: '',
        unitPrice: '',
        extendedAmount: '',
        taxAmount: '',
        totalAmount: '',
        description: '',
        currencyId: null,
        rateTypeId: null,
        notes: '',
        isAutomatic: false,
        isDeposit: false,
        depositRefundedDate: '',
        exportedDate: '',
        externalId: '',
        versionNumber: '',
        active: true,
        deleted: false,
        taxCodeId: null,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.eventChargeForm.reset({
        scheduledEventId: eventChargeData.scheduledEventId,
        resourceId: eventChargeData.resourceId,
        chargeTypeId: eventChargeData.chargeTypeId,
        chargeStatusId: eventChargeData.chargeStatusId,
        quantity: eventChargeData.quantity?.toString() ?? '',
        unitPrice: eventChargeData.unitPrice?.toString() ?? '',
        extendedAmount: eventChargeData.extendedAmount?.toString() ?? '',
        taxAmount: eventChargeData.taxAmount?.toString() ?? '',
        totalAmount: eventChargeData.totalAmount?.toString() ?? '',
        description: eventChargeData.description ?? '',
        currencyId: eventChargeData.currencyId,
        rateTypeId: eventChargeData.rateTypeId,
        notes: eventChargeData.notes ?? '',
        isAutomatic: eventChargeData.isAutomatic ?? false,
        isDeposit: eventChargeData.isDeposit ?? false,
        depositRefundedDate: isoUtcStringToDateTimeLocal(eventChargeData.depositRefundedDate) ?? '',
        exportedDate: isoUtcStringToDateTimeLocal(eventChargeData.exportedDate) ?? '',
        externalId: eventChargeData.externalId ?? '',
        versionNumber: eventChargeData.versionNumber?.toString() ?? '',
        active: eventChargeData.active ?? true,
        deleted: eventChargeData.deleted ?? false,
        taxCodeId: eventChargeData.taxCodeId,
      }, { emitEvent: false});
    }

    this.eventChargeForm.markAsPristine();
    this.eventChargeForm.markAsUntouched();
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

    if (this.eventChargeService.userIsSchedulerEventChargeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Event Charges", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.eventChargeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.eventChargeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.eventChargeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const eventChargeSubmitData: EventChargeSubmitData = {
        id: this.eventChargeData?.id || 0,
        scheduledEventId: Number(formValue.scheduledEventId),
        resourceId: formValue.resourceId ? Number(formValue.resourceId) : null,
        chargeTypeId: Number(formValue.chargeTypeId),
        chargeStatusId: Number(formValue.chargeStatusId),
        quantity: formValue.quantity ? Number(formValue.quantity) : null,
        unitPrice: formValue.unitPrice ? Number(formValue.unitPrice) : null,
        extendedAmount: Number(formValue.extendedAmount),
        taxAmount: Number(formValue.taxAmount),
        totalAmount: Number(formValue.totalAmount),
        description: formValue.description?.trim() || null,
        currencyId: Number(formValue.currencyId),
        rateTypeId: formValue.rateTypeId ? Number(formValue.rateTypeId) : null,
        notes: formValue.notes?.trim() || null,
        isAutomatic: !!formValue.isAutomatic,
        isDeposit: !!formValue.isDeposit,
        depositRefundedDate: formValue.depositRefundedDate ? dateTimeLocalToIsoUtc(formValue.depositRefundedDate.trim()) : null,
        exportedDate: formValue.exportedDate ? dateTimeLocalToIsoUtc(formValue.exportedDate.trim()) : null,
        externalId: formValue.externalId?.trim() || null,
        versionNumber: this.eventChargeData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
        taxCodeId: formValue.taxCodeId ? Number(formValue.taxCodeId) : null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.eventChargeService.PutEventCharge(eventChargeSubmitData.id, eventChargeSubmitData)
      : this.eventChargeService.PostEventCharge(eventChargeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedEventChargeData) => {

        this.eventChargeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Event Charge's detail page
          //
          this.eventChargeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.eventChargeForm.markAsUntouched();

          this.router.navigate(['/eventcharges', savedEventChargeData.id]);
          this.alertService.showMessage('Event Charge added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.eventChargeData = savedEventChargeData;
          this.buildFormValues(this.eventChargeData);

          this.alertService.showMessage("Event Charge saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Event Charge.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Event Charge.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Event Charge could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerEventChargeReader(): boolean {
    return this.eventChargeService.userIsSchedulerEventChargeReader();
  }

  public userIsSchedulerEventChargeWriter(): boolean {
    return this.eventChargeService.userIsSchedulerEventChargeWriter();
  }
}
