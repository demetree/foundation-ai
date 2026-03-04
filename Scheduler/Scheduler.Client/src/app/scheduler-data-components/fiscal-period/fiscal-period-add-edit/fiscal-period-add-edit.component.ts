/*
   GENERATED FORM FOR THE FISCALPERIOD TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from FiscalPeriod table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to fiscal-period-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { FiscalPeriodService, FiscalPeriodData, FiscalPeriodSubmitData } from '../../../scheduler-data-services/fiscal-period.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface FiscalPeriodFormValues {
  name: string,
  description: string,
  startDate: string,
  endDate: string,
  periodType: string,
  fiscalYear: string,     // Stored as string for form input, converted to number on submit.
  periodNumber: string,     // Stored as string for form input, converted to number on submit.
  isClosed: boolean,
  closedDate: string | null,
  closedBy: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-fiscal-period-add-edit',
  templateUrl: './fiscal-period-add-edit.component.html',
  styleUrls: ['./fiscal-period-add-edit.component.scss']
})
export class FiscalPeriodAddEditComponent {
  @ViewChild('fiscalPeriodModal') fiscalPeriodModal!: TemplateRef<any>;
  @Output() fiscalPeriodChanged = new Subject<FiscalPeriodData[]>();
  @Input() fiscalPeriodSubmitData: FiscalPeriodSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<FiscalPeriodFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public fiscalPeriodForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        startDate: ['', Validators.required],
        endDate: ['', Validators.required],
        periodType: ['', Validators.required],
        fiscalYear: ['', Validators.required],
        periodNumber: ['', Validators.required],
        isClosed: [false],
        closedDate: [''],
        closedBy: [''],
        sequence: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  fiscalPeriods$ = this.fiscalPeriodService.GetFiscalPeriodList();

  constructor(
    private modalService: NgbModal,
    private fiscalPeriodService: FiscalPeriodService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(fiscalPeriodData?: FiscalPeriodData) {

    if (fiscalPeriodData != null) {

      if (!this.fiscalPeriodService.userIsSchedulerFiscalPeriodReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Fiscal Periods`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.fiscalPeriodSubmitData = this.fiscalPeriodService.ConvertToFiscalPeriodSubmitData(fiscalPeriodData);
      this.isEditMode = true;
      this.objectGuid = fiscalPeriodData.objectGuid;

      this.buildFormValues(fiscalPeriodData);

    } else {

      if (!this.fiscalPeriodService.userIsSchedulerFiscalPeriodWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Fiscal Periods`,
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
        this.fiscalPeriodForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.fiscalPeriodForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.fiscalPeriodModal, {
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

    if (this.fiscalPeriodService.userIsSchedulerFiscalPeriodWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Fiscal Periods`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.fiscalPeriodForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.fiscalPeriodForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.fiscalPeriodForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const fiscalPeriodSubmitData: FiscalPeriodSubmitData = {
        id: this.fiscalPeriodSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        startDate: dateTimeLocalToIsoUtc(formValue.startDate!.trim())!,
        endDate: dateTimeLocalToIsoUtc(formValue.endDate!.trim())!,
        periodType: formValue.periodType!.trim(),
        fiscalYear: Number(formValue.fiscalYear),
        periodNumber: Number(formValue.periodNumber),
        isClosed: !!formValue.isClosed,
        closedDate: formValue.closedDate ? dateTimeLocalToIsoUtc(formValue.closedDate.trim()) : null,
        closedBy: formValue.closedBy?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        versionNumber: this.fiscalPeriodSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateFiscalPeriod(fiscalPeriodSubmitData);
      } else {
        this.addFiscalPeriod(fiscalPeriodSubmitData);
      }
  }

  private addFiscalPeriod(fiscalPeriodData: FiscalPeriodSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    fiscalPeriodData.versionNumber = 0;
    fiscalPeriodData.active = true;
    fiscalPeriodData.deleted = false;
    this.fiscalPeriodService.PostFiscalPeriod(fiscalPeriodData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newFiscalPeriod) => {

        this.fiscalPeriodService.ClearAllCaches();

        this.fiscalPeriodChanged.next([newFiscalPeriod]);

        this.alertService.showMessage("Fiscal Period added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/fiscalperiod', newFiscalPeriod.id]);
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
                                   'You do not have permission to save this Fiscal Period.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Fiscal Period.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Fiscal Period could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateFiscalPeriod(fiscalPeriodData: FiscalPeriodSubmitData) {
    this.fiscalPeriodService.PutFiscalPeriod(fiscalPeriodData.id, fiscalPeriodData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedFiscalPeriod) => {

        this.fiscalPeriodService.ClearAllCaches();

        this.fiscalPeriodChanged.next([updatedFiscalPeriod]);

        this.alertService.showMessage("Fiscal Period updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Fiscal Period.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Fiscal Period.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Fiscal Period could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(fiscalPeriodData: FiscalPeriodData | null) {

    if (fiscalPeriodData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.fiscalPeriodForm.reset({
        name: '',
        description: '',
        startDate: '',
        endDate: '',
        periodType: '',
        fiscalYear: '',
        periodNumber: '',
        isClosed: false,
        closedDate: '',
        closedBy: '',
        sequence: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.fiscalPeriodForm.reset({
        name: fiscalPeriodData.name ?? '',
        description: fiscalPeriodData.description ?? '',
        startDate: isoUtcStringToDateTimeLocal(fiscalPeriodData.startDate) ?? '',
        endDate: isoUtcStringToDateTimeLocal(fiscalPeriodData.endDate) ?? '',
        periodType: fiscalPeriodData.periodType ?? '',
        fiscalYear: fiscalPeriodData.fiscalYear?.toString() ?? '',
        periodNumber: fiscalPeriodData.periodNumber?.toString() ?? '',
        isClosed: fiscalPeriodData.isClosed ?? false,
        closedDate: isoUtcStringToDateTimeLocal(fiscalPeriodData.closedDate) ?? '',
        closedBy: fiscalPeriodData.closedBy ?? '',
        sequence: fiscalPeriodData.sequence?.toString() ?? '',
        versionNumber: fiscalPeriodData.versionNumber?.toString() ?? '',
        active: fiscalPeriodData.active ?? true,
        deleted: fiscalPeriodData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.fiscalPeriodForm.markAsPristine();
    this.fiscalPeriodForm.markAsUntouched();
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


  public userIsSchedulerFiscalPeriodReader(): boolean {
    return this.fiscalPeriodService.userIsSchedulerFiscalPeriodReader();
  }

  public userIsSchedulerFiscalPeriodWriter(): boolean {
    return this.fiscalPeriodService.userIsSchedulerFiscalPeriodWriter();
  }
}
