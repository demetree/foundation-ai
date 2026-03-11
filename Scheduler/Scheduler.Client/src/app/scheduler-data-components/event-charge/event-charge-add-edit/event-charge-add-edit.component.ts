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
import { Component, ViewChild, Output, Input, TemplateRef, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EventChargeService, EventChargeData, EventChargeSubmitData } from '../../../scheduler-data-services/event-charge.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { ChargeTypeService } from '../../../scheduler-data-services/charge-type.service';
import { ChargeStatusService } from '../../../scheduler-data-services/charge-status.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { RateTypeService } from '../../../scheduler-data-services/rate-type.service';
import { TaxCodeService, TaxCodeData } from '../../../scheduler-data-services/tax-code.service';
import { AuthService } from '../../../services/auth.service';

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
  selector: 'app-event-charge-add-edit',
  templateUrl: './event-charge-add-edit.component.html',
  styleUrls: ['./event-charge-add-edit.component.scss']
})
export class EventChargeAddEditComponent implements OnInit {
  @ViewChild('eventChargeModal') eventChargeModal!: TemplateRef<any>;
  @Output() eventChargeChanged = new Subject<EventChargeData[]>();
  @Input() eventChargeSubmitData: EventChargeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


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

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  eventCharges$ = this.eventChargeService.GetEventChargeList();
  scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  resources$ = this.resourceService.GetResourceList();
  chargeTypes$ = this.chargeTypeService.GetChargeTypeList();
  chargeStatuses$ = this.chargeStatusService.GetChargeStatusList();
  currencies$ = this.currencyService.GetCurrencyList();
  rateTypes$ = this.rateTypeService.GetRateTypeList();
  taxCodes$ = this.taxCodeService.GetTaxCodeList();
  private taxCodesCache: TaxCodeData[] = [];


  ngOnInit(): void {
    // Cache tax codes for rate lookups
    this.taxCodeService.GetTaxCodeList({ active: true, deleted: false }).subscribe(codes => {
      this.taxCodesCache = codes;
    });

    // Auto-recalculate tax when tax code or extended amount changes
    this.eventChargeForm.get('taxCodeId')?.valueChanges.subscribe(() => this.autoCalculateTax());
    this.eventChargeForm.get('extendedAmount')?.valueChanges.subscribe(() => this.autoCalculateTax());
  }


  /**
   * Auto-select the default HST tax code when adding a new charge.
   * Called after the form is reset in add mode.
   */
  private autoSelectDefaultTaxCode(): void {
    if (this.taxCodesCache.length === 0) return;

    // Look for HST by name (case-insensitive)
    const hst = this.taxCodesCache.find(tc => tc.name?.toUpperCase().includes('HST'));
    if (hst) {
      this.eventChargeForm.patchValue({ taxCodeId: hst.id }, { emitEvent: true });
    } else if (this.taxCodesCache.length === 1) {
      // If only one tax code exists, auto-select it
      this.eventChargeForm.patchValue({ taxCodeId: this.taxCodesCache[0].id }, { emitEvent: true });
    }
  }


  /**
   * Auto-calculate taxAmount from extendedAmount × taxCode.rate.
   * Also updates totalAmount = extendedAmount + taxAmount.
   */
  private autoCalculateTax(): void {
    const taxCodeId = this.eventChargeForm.get('taxCodeId')?.value;
    const extendedAmount = Number(this.eventChargeForm.get('extendedAmount')?.value) || 0;

    if (!taxCodeId || extendedAmount === 0) return;

    const taxCode = this.taxCodesCache.find(tc => Number(tc.id) === Number(taxCodeId));
    if (!taxCode || !taxCode.rate) return;

    const taxAmount = Math.round(extendedAmount * taxCode.rate * 100) / 100;
    const totalAmount = Math.round((extendedAmount + taxAmount) * 100) / 100;

    this.eventChargeForm.patchValue({
      taxAmount: taxAmount.toString(),
      totalAmount: totalAmount.toString()
    }, { emitEvent: false });
  }

  constructor(
    private modalService: NgbModal,
    private eventChargeService: EventChargeService,
    private scheduledEventService: ScheduledEventService,
    private resourceService: ResourceService,
    private chargeTypeService: ChargeTypeService,
    private chargeStatusService: ChargeStatusService,
    private currencyService: CurrencyService,
    private rateTypeService: RateTypeService,
    private taxCodeService: TaxCodeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(eventChargeData?: EventChargeData) {

    if (eventChargeData != null) {

      if (!this.eventChargeService.userIsSchedulerEventChargeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Event Charges`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.eventChargeSubmitData = this.eventChargeService.ConvertToEventChargeSubmitData(eventChargeData);
      this.isEditMode = true;
      this.objectGuid = eventChargeData.objectGuid;

      this.buildFormValues(eventChargeData);

    } else {

      if (!this.eventChargeService.userIsSchedulerEventChargeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Event Charges`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.eventChargeForm.patchValue(this.preSeededData);
      }

      // Auto-select HST tax code for new charges
      this.autoSelectDefaultTaxCode();

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

    this.modalRef = this.modalService.open(this.eventChargeModal, {
      size: 'xl',
      scrollable: true,
      backdrop: 'static',
      keyboard: true,
      windowClass: 'custom-modal'
    });
    this.modalIsDisplayed = true;
  }


  closeModal() {
    if (this.modalRef) {
      this.modalRef.dismiss('cancel');
    }
    this.modalIsDisplayed = false;
  }


  submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.eventChargeService.userIsSchedulerEventChargeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Event Charges`,
        '',
        MessageSeverity.info
      );
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
        id: this.eventChargeSubmitData?.id || 0,
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
        versionNumber: this.eventChargeSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
        taxCodeId: formValue.taxCodeId ? Number(formValue.taxCodeId) : null,
   };

      if (this.isEditMode) {
        this.updateEventCharge(eventChargeSubmitData);
      } else {
        this.addEventCharge(eventChargeSubmitData);
      }
  }

  private addEventCharge(eventChargeData: EventChargeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    eventChargeData.versionNumber = 0;
    eventChargeData.active = true;
    eventChargeData.deleted = false;
    this.eventChargeService.PostEventCharge(eventChargeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newEventCharge) => {

        this.eventChargeService.ClearAllCaches();

        this.eventChargeChanged.next([newEventCharge]);

        this.alertService.showMessage("Event Charge added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/eventcharge', newEventCharge.id]);
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


  private updateEventCharge(eventChargeData: EventChargeSubmitData) {
    this.eventChargeService.PutEventCharge(eventChargeData.id, eventChargeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedEventCharge) => {

        this.eventChargeService.ClearAllCaches();

        this.eventChargeChanged.next([updatedEventCharge]);

        this.alertService.showMessage("Event Charge updated successfully", '', MessageSeverity.success);

        this.closeModal();
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


  public userIsSchedulerEventChargeReader(): boolean {
    return this.eventChargeService.userIsSchedulerEventChargeReader();
  }

  public userIsSchedulerEventChargeWriter(): boolean {
    return this.eventChargeService.userIsSchedulerEventChargeWriter();
  }
}
