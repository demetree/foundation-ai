/*
   GENERATED FORM FOR THE GIFT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Gift table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to gift-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { GiftService, GiftData, GiftSubmitData } from '../../../scheduler-data-services/gift.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { ConstituentService } from '../../../scheduler-data-services/constituent.service';
import { PledgeService } from '../../../scheduler-data-services/pledge.service';
import { FundService } from '../../../scheduler-data-services/fund.service';
import { CampaignService } from '../../../scheduler-data-services/campaign.service';
import { AppealService } from '../../../scheduler-data-services/appeal.service';
import { PaymentTypeService } from '../../../scheduler-data-services/payment-type.service';
import { BatchService } from '../../../scheduler-data-services/batch.service';
import { ReceiptTypeService } from '../../../scheduler-data-services/receipt-type.service';
import { TributeService } from '../../../scheduler-data-services/tribute.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface GiftFormValues {
  officeId: number | bigint | null,       // For FK link number
  constituentId: number | bigint,       // For FK link number
  pledgeId: number | bigint | null,       // For FK link number
  amount: string,     // Stored as string for form input, converted to number on submit.
  receivedDate: string,
  postedDate: string | null,
  fundId: number | bigint,       // For FK link number
  campaignId: number | bigint | null,       // For FK link number
  appealId: number | bigint | null,       // For FK link number
  paymentTypeId: number | bigint,       // For FK link number
  referenceNumber: string | null,
  batchId: number | bigint | null,       // For FK link number
  receiptTypeId: number | bigint | null,       // For FK link number
  receiptDate: string | null,
  tributeId: number | bigint | null,       // For FK link number
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-gift-add-edit',
  templateUrl: './gift-add-edit.component.html',
  styleUrls: ['./gift-add-edit.component.scss']
})
export class GiftAddEditComponent {
  @ViewChild('giftModal') giftModal!: TemplateRef<any>;
  @Output() giftChanged = new Subject<GiftData[]>();
  @Input() giftSubmitData: GiftSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<GiftFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public giftForm: FormGroup = this.fb.group({
        officeId: [null],
        constituentId: [null, Validators.required],
        pledgeId: [null],
        amount: ['', Validators.required],
        receivedDate: ['', Validators.required],
        postedDate: [''],
        fundId: [null, Validators.required],
        campaignId: [null],
        appealId: [null],
        paymentTypeId: [null, Validators.required],
        referenceNumber: [''],
        batchId: [null],
        receiptTypeId: [null],
        receiptDate: [''],
        tributeId: [null],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  gifts$ = this.giftService.GetGiftList();
  offices$ = this.officeService.GetOfficeList();
  constituents$ = this.constituentService.GetConstituentList();
  pledges$ = this.pledgeService.GetPledgeList();
  funds$ = this.fundService.GetFundList();
  campaigns$ = this.campaignService.GetCampaignList();
  appeals$ = this.appealService.GetAppealList();
  paymentTypes$ = this.paymentTypeService.GetPaymentTypeList();
  batches$ = this.batchService.GetBatchList();
  receiptTypes$ = this.receiptTypeService.GetReceiptTypeList();
  tributes$ = this.tributeService.GetTributeList();

  constructor(
    private modalService: NgbModal,
    private giftService: GiftService,
    private officeService: OfficeService,
    private constituentService: ConstituentService,
    private pledgeService: PledgeService,
    private fundService: FundService,
    private campaignService: CampaignService,
    private appealService: AppealService,
    private paymentTypeService: PaymentTypeService,
    private batchService: BatchService,
    private receiptTypeService: ReceiptTypeService,
    private tributeService: TributeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(giftData?: GiftData) {

    if (giftData != null) {

      if (!this.giftService.userIsSchedulerGiftReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Gifts`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.giftSubmitData = this.giftService.ConvertToGiftSubmitData(giftData);
      this.isEditMode = true;
      this.objectGuid = giftData.objectGuid;

      this.buildFormValues(giftData);

    } else {

      if (!this.giftService.userIsSchedulerGiftWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Gifts`,
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
        this.giftForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.giftForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.giftModal, {
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

    if (this.giftService.userIsSchedulerGiftWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Gifts`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.giftForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.giftForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.giftForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const giftSubmitData: GiftSubmitData = {
        id: this.giftSubmitData?.id || 0,
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        constituentId: Number(formValue.constituentId),
        pledgeId: formValue.pledgeId ? Number(formValue.pledgeId) : null,
        amount: Number(formValue.amount),
        receivedDate: dateTimeLocalToIsoUtc(formValue.receivedDate!.trim())!,
        postedDate: formValue.postedDate ? dateTimeLocalToIsoUtc(formValue.postedDate.trim()) : null,
        fundId: Number(formValue.fundId),
        campaignId: formValue.campaignId ? Number(formValue.campaignId) : null,
        appealId: formValue.appealId ? Number(formValue.appealId) : null,
        paymentTypeId: Number(formValue.paymentTypeId),
        referenceNumber: formValue.referenceNumber?.trim() || null,
        batchId: formValue.batchId ? Number(formValue.batchId) : null,
        receiptTypeId: formValue.receiptTypeId ? Number(formValue.receiptTypeId) : null,
        receiptDate: formValue.receiptDate ? dateTimeLocalToIsoUtc(formValue.receiptDate.trim()) : null,
        tributeId: formValue.tributeId ? Number(formValue.tributeId) : null,
        notes: formValue.notes?.trim() || null,
        versionNumber: this.giftSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateGift(giftSubmitData);
      } else {
        this.addGift(giftSubmitData);
      }
  }

  private addGift(giftData: GiftSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    giftData.versionNumber = 0;
    giftData.active = true;
    giftData.deleted = false;
    this.giftService.PostGift(giftData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newGift) => {

        this.giftService.ClearAllCaches();

        this.giftChanged.next([newGift]);

        this.alertService.showMessage("Gift added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/gift', newGift.id]);
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
                                   'You do not have permission to save this Gift.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Gift.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Gift could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateGift(giftData: GiftSubmitData) {
    this.giftService.PutGift(giftData.id, giftData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedGift) => {

        this.giftService.ClearAllCaches();

        this.giftChanged.next([updatedGift]);

        this.alertService.showMessage("Gift updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Gift.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Gift.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Gift could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(giftData: GiftData | null) {

    if (giftData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.giftForm.reset({
        officeId: null,
        constituentId: null,
        pledgeId: null,
        amount: '',
        receivedDate: '',
        postedDate: '',
        fundId: null,
        campaignId: null,
        appealId: null,
        paymentTypeId: null,
        referenceNumber: '',
        batchId: null,
        receiptTypeId: null,
        receiptDate: '',
        tributeId: null,
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
        this.giftForm.reset({
        officeId: giftData.officeId,
        constituentId: giftData.constituentId,
        pledgeId: giftData.pledgeId,
        amount: giftData.amount?.toString() ?? '',
        receivedDate: isoUtcStringToDateTimeLocal(giftData.receivedDate) ?? '',
        postedDate: isoUtcStringToDateTimeLocal(giftData.postedDate) ?? '',
        fundId: giftData.fundId,
        campaignId: giftData.campaignId,
        appealId: giftData.appealId,
        paymentTypeId: giftData.paymentTypeId,
        referenceNumber: giftData.referenceNumber ?? '',
        batchId: giftData.batchId,
        receiptTypeId: giftData.receiptTypeId,
        receiptDate: isoUtcStringToDateTimeLocal(giftData.receiptDate) ?? '',
        tributeId: giftData.tributeId,
        notes: giftData.notes ?? '',
        versionNumber: giftData.versionNumber?.toString() ?? '',
        active: giftData.active ?? true,
        deleted: giftData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.giftForm.markAsPristine();
    this.giftForm.markAsUntouched();
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


  public userIsSchedulerGiftReader(): boolean {
    return this.giftService.userIsSchedulerGiftReader();
  }

  public userIsSchedulerGiftWriter(): boolean {
    return this.giftService.userIsSchedulerGiftWriter();
  }
}
