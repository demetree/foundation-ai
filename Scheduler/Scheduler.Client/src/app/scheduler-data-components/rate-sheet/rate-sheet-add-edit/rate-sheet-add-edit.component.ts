/*
   GENERATED FORM FOR THE RATESHEET TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RateSheet table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to rate-sheet-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RateSheetService, RateSheetData, RateSheetSubmitData } from '../../../scheduler-data-services/rate-sheet.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { AssignmentRoleService } from '../../../scheduler-data-services/assignment-role.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { RateTypeService } from '../../../scheduler-data-services/rate-type.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface RateSheetFormValues {
  officeId: number | bigint | null,       // For FK link number
  assignmentRoleId: number | bigint | null,       // For FK link number
  resourceId: number | bigint | null,       // For FK link number
  schedulingTargetId: number | bigint | null,       // For FK link number
  rateTypeId: number | bigint,       // For FK link number
  effectiveDate: string,
  currencyId: number | bigint,       // For FK link number
  costRate: string,     // Stored as string for form input, converted to number on submit.
  billingRate: string,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-rate-sheet-add-edit',
  templateUrl: './rate-sheet-add-edit.component.html',
  styleUrls: ['./rate-sheet-add-edit.component.scss']
})
export class RateSheetAddEditComponent {
  @ViewChild('rateSheetModal') rateSheetModal!: TemplateRef<any>;
  @Output() rateSheetChanged = new Subject<RateSheetData[]>();
  @Input() rateSheetSubmitData: RateSheetSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<RateSheetFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public rateSheetForm: FormGroup = this.fb.group({
        officeId: [null],
        assignmentRoleId: [null],
        resourceId: [null],
        schedulingTargetId: [null],
        rateTypeId: [null, Validators.required],
        effectiveDate: ['', Validators.required],
        currencyId: [null, Validators.required],
        costRate: ['', Validators.required],
        billingRate: ['', Validators.required],
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

  rateSheets$ = this.rateSheetService.GetRateSheetList();
  offices$ = this.officeService.GetOfficeList();
  assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList();
  resources$ = this.resourceService.GetResourceList();
  schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  rateTypes$ = this.rateTypeService.GetRateTypeList();
  currencies$ = this.currencyService.GetCurrencyList();

  constructor(
    private modalService: NgbModal,
    private rateSheetService: RateSheetService,
    private officeService: OfficeService,
    private assignmentRoleService: AssignmentRoleService,
    private resourceService: ResourceService,
    private schedulingTargetService: SchedulingTargetService,
    private rateTypeService: RateTypeService,
    private currencyService: CurrencyService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(rateSheetData?: RateSheetData) {

    if (rateSheetData != null) {

      if (!this.rateSheetService.userIsSchedulerRateSheetReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Rate Sheets`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.rateSheetSubmitData = this.rateSheetService.ConvertToRateSheetSubmitData(rateSheetData);
      this.isEditMode = true;
      this.objectGuid = rateSheetData.objectGuid;

      this.buildFormValues(rateSheetData);

    } else {

      if (!this.rateSheetService.userIsSchedulerRateSheetWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Rate Sheets`,
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
        this.rateSheetForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.rateSheetForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.rateSheetModal, {
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

    if (this.rateSheetService.userIsSchedulerRateSheetWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Rate Sheets`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.rateSheetForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.rateSheetForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.rateSheetForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const rateSheetSubmitData: RateSheetSubmitData = {
        id: this.rateSheetSubmitData?.id || 0,
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        assignmentRoleId: formValue.assignmentRoleId ? Number(formValue.assignmentRoleId) : null,
        resourceId: formValue.resourceId ? Number(formValue.resourceId) : null,
        schedulingTargetId: formValue.schedulingTargetId ? Number(formValue.schedulingTargetId) : null,
        rateTypeId: Number(formValue.rateTypeId),
        effectiveDate: dateTimeLocalToIsoUtc(formValue.effectiveDate!.trim())!,
        currencyId: Number(formValue.currencyId),
        costRate: Number(formValue.costRate),
        billingRate: Number(formValue.billingRate),
        notes: formValue.notes?.trim() || null,
        versionNumber: this.rateSheetSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateRateSheet(rateSheetSubmitData);
      } else {
        this.addRateSheet(rateSheetSubmitData);
      }
  }

  private addRateSheet(rateSheetData: RateSheetSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    rateSheetData.versionNumber = 0;
    rateSheetData.active = true;
    rateSheetData.deleted = false;
    this.rateSheetService.PostRateSheet(rateSheetData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newRateSheet) => {

        this.rateSheetService.ClearAllCaches();

        this.rateSheetChanged.next([newRateSheet]);

        this.alertService.showMessage("Rate Sheet added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/ratesheet', newRateSheet.id]);
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
                                   'You do not have permission to save this Rate Sheet.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Rate Sheet.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Rate Sheet could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateRateSheet(rateSheetData: RateSheetSubmitData) {
    this.rateSheetService.PutRateSheet(rateSheetData.id, rateSheetData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedRateSheet) => {

        this.rateSheetService.ClearAllCaches();

        this.rateSheetChanged.next([updatedRateSheet]);

        this.alertService.showMessage("Rate Sheet updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Rate Sheet.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Rate Sheet.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Rate Sheet could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(rateSheetData: RateSheetData | null) {

    if (rateSheetData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.rateSheetForm.reset({
        officeId: null,
        assignmentRoleId: null,
        resourceId: null,
        schedulingTargetId: null,
        rateTypeId: null,
        effectiveDate: '',
        currencyId: null,
        costRate: '',
        billingRate: '',
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
        this.rateSheetForm.reset({
        officeId: rateSheetData.officeId,
        assignmentRoleId: rateSheetData.assignmentRoleId,
        resourceId: rateSheetData.resourceId,
        schedulingTargetId: rateSheetData.schedulingTargetId,
        rateTypeId: rateSheetData.rateTypeId,
        effectiveDate: isoUtcStringToDateTimeLocal(rateSheetData.effectiveDate) ?? '',
        currencyId: rateSheetData.currencyId,
        costRate: rateSheetData.costRate?.toString() ?? '',
        billingRate: rateSheetData.billingRate?.toString() ?? '',
        notes: rateSheetData.notes ?? '',
        versionNumber: rateSheetData.versionNumber?.toString() ?? '',
        active: rateSheetData.active ?? true,
        deleted: rateSheetData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.rateSheetForm.markAsPristine();
    this.rateSheetForm.markAsUntouched();
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


  public userIsSchedulerRateSheetReader(): boolean {
    return this.rateSheetService.userIsSchedulerRateSheetReader();
  }

  public userIsSchedulerRateSheetWriter(): boolean {
    return this.rateSheetService.userIsSchedulerRateSheetWriter();
  }
}
