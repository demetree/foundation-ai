/*
   GENERATED FORM FOR THE GENERALLEDGERENTRY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from GeneralLedgerEntry table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to general-ledger-entry-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { GeneralLedgerEntryService, GeneralLedgerEntryData, GeneralLedgerEntrySubmitData } from '../../../scheduler-data-services/general-ledger-entry.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { FinancialTransactionService } from '../../../scheduler-data-services/financial-transaction.service';
import { FiscalPeriodService } from '../../../scheduler-data-services/fiscal-period.service';
import { FinancialOfficeService } from '../../../scheduler-data-services/financial-office.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface GeneralLedgerEntryFormValues {
  journalEntryNumber: string,     // Stored as string for form input, converted to number on submit.
  transactionDate: string,
  description: string | null,
  referenceNumber: string | null,
  financialTransactionId: number | bigint | null,       // For FK link number
  fiscalPeriodId: number | bigint | null,       // For FK link number
  financialOfficeId: number | bigint | null,       // For FK link number
  postedBy: string,     // Stored as string for form input, converted to number on submit.
  postedDate: string,
  reversalOfId: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-general-ledger-entry-add-edit',
  templateUrl: './general-ledger-entry-add-edit.component.html',
  styleUrls: ['./general-ledger-entry-add-edit.component.scss']
})
export class GeneralLedgerEntryAddEditComponent {
  @ViewChild('generalLedgerEntryModal') generalLedgerEntryModal!: TemplateRef<any>;
  @Output() generalLedgerEntryChanged = new Subject<GeneralLedgerEntryData[]>();
  @Input() generalLedgerEntrySubmitData: GeneralLedgerEntrySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<GeneralLedgerEntryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public generalLedgerEntryForm: FormGroup = this.fb.group({
        journalEntryNumber: ['', Validators.required],
        transactionDate: ['', Validators.required],
        description: [''],
        referenceNumber: [''],
        financialTransactionId: [null],
        fiscalPeriodId: [null],
        financialOfficeId: [null],
        postedBy: ['', Validators.required],
        postedDate: ['', Validators.required],
        reversalOfId: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  generalLedgerEntries$ = this.generalLedgerEntryService.GetGeneralLedgerEntryList();
  financialTransactions$ = this.financialTransactionService.GetFinancialTransactionList();
  fiscalPeriods$ = this.fiscalPeriodService.GetFiscalPeriodList();
  financialOffices$ = this.financialOfficeService.GetFinancialOfficeList();

  constructor(
    private modalService: NgbModal,
    private generalLedgerEntryService: GeneralLedgerEntryService,
    private financialTransactionService: FinancialTransactionService,
    private fiscalPeriodService: FiscalPeriodService,
    private financialOfficeService: FinancialOfficeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(generalLedgerEntryData?: GeneralLedgerEntryData) {

    if (generalLedgerEntryData != null) {

      if (!this.generalLedgerEntryService.userIsSchedulerGeneralLedgerEntryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read General Ledger Entries`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.generalLedgerEntrySubmitData = this.generalLedgerEntryService.ConvertToGeneralLedgerEntrySubmitData(generalLedgerEntryData);
      this.isEditMode = true;
      this.objectGuid = generalLedgerEntryData.objectGuid;

      this.buildFormValues(generalLedgerEntryData);

    } else {

      if (!this.generalLedgerEntryService.userIsSchedulerGeneralLedgerEntryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write General Ledger Entries`,
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
        this.generalLedgerEntryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.generalLedgerEntryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.generalLedgerEntryModal, {
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

    if (this.generalLedgerEntryService.userIsSchedulerGeneralLedgerEntryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write General Ledger Entries`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.generalLedgerEntryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.generalLedgerEntryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.generalLedgerEntryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const generalLedgerEntrySubmitData: GeneralLedgerEntrySubmitData = {
        id: this.generalLedgerEntrySubmitData?.id || 0,
        journalEntryNumber: Number(formValue.journalEntryNumber),
        transactionDate: dateTimeLocalToIsoUtc(formValue.transactionDate!.trim())!,
        description: formValue.description?.trim() || null,
        referenceNumber: formValue.referenceNumber?.trim() || null,
        financialTransactionId: formValue.financialTransactionId ? Number(formValue.financialTransactionId) : null,
        fiscalPeriodId: formValue.fiscalPeriodId ? Number(formValue.fiscalPeriodId) : null,
        financialOfficeId: formValue.financialOfficeId ? Number(formValue.financialOfficeId) : null,
        postedBy: Number(formValue.postedBy),
        postedDate: dateTimeLocalToIsoUtc(formValue.postedDate!.trim())!,
        reversalOfId: formValue.reversalOfId ? Number(formValue.reversalOfId) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateGeneralLedgerEntry(generalLedgerEntrySubmitData);
      } else {
        this.addGeneralLedgerEntry(generalLedgerEntrySubmitData);
      }
  }

  private addGeneralLedgerEntry(generalLedgerEntryData: GeneralLedgerEntrySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    generalLedgerEntryData.active = true;
    generalLedgerEntryData.deleted = false;
    this.generalLedgerEntryService.PostGeneralLedgerEntry(generalLedgerEntryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newGeneralLedgerEntry) => {

        this.generalLedgerEntryService.ClearAllCaches();

        this.generalLedgerEntryChanged.next([newGeneralLedgerEntry]);

        this.alertService.showMessage("General Ledger Entry added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/generalledgerentry', newGeneralLedgerEntry.id]);
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
                                   'You do not have permission to save this General Ledger Entry.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the General Ledger Entry.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('General Ledger Entry could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateGeneralLedgerEntry(generalLedgerEntryData: GeneralLedgerEntrySubmitData) {
    this.generalLedgerEntryService.PutGeneralLedgerEntry(generalLedgerEntryData.id, generalLedgerEntryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedGeneralLedgerEntry) => {

        this.generalLedgerEntryService.ClearAllCaches();

        this.generalLedgerEntryChanged.next([updatedGeneralLedgerEntry]);

        this.alertService.showMessage("General Ledger Entry updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this General Ledger Entry.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the General Ledger Entry.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('General Ledger Entry could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(generalLedgerEntryData: GeneralLedgerEntryData | null) {

    if (generalLedgerEntryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.generalLedgerEntryForm.reset({
        journalEntryNumber: '',
        transactionDate: '',
        description: '',
        referenceNumber: '',
        financialTransactionId: null,
        fiscalPeriodId: null,
        financialOfficeId: null,
        postedBy: '',
        postedDate: '',
        reversalOfId: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.generalLedgerEntryForm.reset({
        journalEntryNumber: generalLedgerEntryData.journalEntryNumber?.toString() ?? '',
        transactionDate: isoUtcStringToDateTimeLocal(generalLedgerEntryData.transactionDate) ?? '',
        description: generalLedgerEntryData.description ?? '',
        referenceNumber: generalLedgerEntryData.referenceNumber ?? '',
        financialTransactionId: generalLedgerEntryData.financialTransactionId,
        fiscalPeriodId: generalLedgerEntryData.fiscalPeriodId,
        financialOfficeId: generalLedgerEntryData.financialOfficeId,
        postedBy: generalLedgerEntryData.postedBy?.toString() ?? '',
        postedDate: isoUtcStringToDateTimeLocal(generalLedgerEntryData.postedDate) ?? '',
        reversalOfId: generalLedgerEntryData.reversalOfId?.toString() ?? '',
        active: generalLedgerEntryData.active ?? true,
        deleted: generalLedgerEntryData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.generalLedgerEntryForm.markAsPristine();
    this.generalLedgerEntryForm.markAsUntouched();
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


  public userIsSchedulerGeneralLedgerEntryReader(): boolean {
    return this.generalLedgerEntryService.userIsSchedulerGeneralLedgerEntryReader();
  }

  public userIsSchedulerGeneralLedgerEntryWriter(): boolean {
    return this.generalLedgerEntryService.userIsSchedulerGeneralLedgerEntryWriter();
  }
}
