/*
   GENERATED FORM FOR THE PLEDGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Pledge table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to pledge-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PledgeService, PledgeData, PledgeSubmitData } from '../../../scheduler-data-services/pledge.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ConstituentService } from '../../../scheduler-data-services/constituent.service';
import { RecurrenceFrequencyService } from '../../../scheduler-data-services/recurrence-frequency.service';
import { FundService } from '../../../scheduler-data-services/fund.service';
import { CampaignService } from '../../../scheduler-data-services/campaign.service';
import { AppealService } from '../../../scheduler-data-services/appeal.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface PledgeFormValues {
  constituentId: number | bigint,       // For FK link number
  totalAmount: string,     // Stored as string for form input, converted to number on submit.
  balanceAmount: string,     // Stored as string for form input, converted to number on submit.
  pledgeDate: string,
  startDate: string | null,
  endDate: string | null,
  recurrenceFrequencyId: number | bigint | null,       // For FK link number
  fundId: number | bigint,       // For FK link number
  campaignId: number | bigint | null,       // For FK link number
  appealId: number | bigint | null,       // For FK link number
  writeOffAmount: string,     // Stored as string for form input, converted to number on submit.
  isWrittenOff: boolean,
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-pledge-add-edit',
  templateUrl: './pledge-add-edit.component.html',
  styleUrls: ['./pledge-add-edit.component.scss']
})
export class PledgeAddEditComponent {
  @ViewChild('pledgeModal') pledgeModal!: TemplateRef<any>;
  @Output() pledgeChanged = new Subject<PledgeData[]>();
  @Input() pledgeSubmitData: PledgeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PledgeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public pledgeForm: FormGroup = this.fb.group({
        constituentId: [null, Validators.required],
        totalAmount: ['', Validators.required],
        balanceAmount: ['', Validators.required],
        pledgeDate: ['', Validators.required],
        startDate: [''],
        endDate: [''],
        recurrenceFrequencyId: [null],
        fundId: [null, Validators.required],
        campaignId: [null],
        appealId: [null],
        writeOffAmount: ['', Validators.required],
        isWrittenOff: [false],
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

  pledges$ = this.pledgeService.GetPledgeList();
  constituents$ = this.constituentService.GetConstituentList();
  recurrenceFrequencies$ = this.recurrenceFrequencyService.GetRecurrenceFrequencyList();
  funds$ = this.fundService.GetFundList();
  campaigns$ = this.campaignService.GetCampaignList();
  appeals$ = this.appealService.GetAppealList();

  constructor(
    private modalService: NgbModal,
    private pledgeService: PledgeService,
    private constituentService: ConstituentService,
    private recurrenceFrequencyService: RecurrenceFrequencyService,
    private fundService: FundService,
    private campaignService: CampaignService,
    private appealService: AppealService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(pledgeData?: PledgeData) {

    if (pledgeData != null) {

      if (!this.pledgeService.userIsSchedulerPledgeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Pledges`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.pledgeSubmitData = this.pledgeService.ConvertToPledgeSubmitData(pledgeData);
      this.isEditMode = true;
      this.objectGuid = pledgeData.objectGuid;

      this.buildFormValues(pledgeData);

    } else {

      if (!this.pledgeService.userIsSchedulerPledgeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Pledges`,
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
        this.pledgeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.pledgeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.pledgeModal, {
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

    if (this.pledgeService.userIsSchedulerPledgeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Pledges`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.pledgeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.pledgeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.pledgeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const pledgeSubmitData: PledgeSubmitData = {
        id: this.pledgeSubmitData?.id || 0,
        constituentId: Number(formValue.constituentId),
        totalAmount: Number(formValue.totalAmount),
        balanceAmount: Number(formValue.balanceAmount),
        pledgeDate: formValue.pledgeDate!.trim(),
        startDate: formValue.startDate?.trim() || null,
        endDate: formValue.endDate?.trim() || null,
        recurrenceFrequencyId: formValue.recurrenceFrequencyId ? Number(formValue.recurrenceFrequencyId) : null,
        fundId: Number(formValue.fundId),
        campaignId: formValue.campaignId ? Number(formValue.campaignId) : null,
        appealId: formValue.appealId ? Number(formValue.appealId) : null,
        writeOffAmount: Number(formValue.writeOffAmount),
        isWrittenOff: !!formValue.isWrittenOff,
        notes: formValue.notes?.trim() || null,
        versionNumber: this.pledgeSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updatePledge(pledgeSubmitData);
      } else {
        this.addPledge(pledgeSubmitData);
      }
  }

  private addPledge(pledgeData: PledgeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    pledgeData.versionNumber = 0;
    pledgeData.active = true;
    pledgeData.deleted = false;
    this.pledgeService.PostPledge(pledgeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPledge) => {

        this.pledgeService.ClearAllCaches();

        this.pledgeChanged.next([newPledge]);

        this.alertService.showMessage("Pledge added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/pledge', newPledge.id]);
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
                                   'You do not have permission to save this Pledge.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Pledge.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Pledge could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePledge(pledgeData: PledgeSubmitData) {
    this.pledgeService.PutPledge(pledgeData.id, pledgeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPledge) => {

        this.pledgeService.ClearAllCaches();

        this.pledgeChanged.next([updatedPledge]);

        this.alertService.showMessage("Pledge updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Pledge.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Pledge.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Pledge could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(pledgeData: PledgeData | null) {

    if (pledgeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.pledgeForm.reset({
        constituentId: null,
        totalAmount: '',
        balanceAmount: '',
        pledgeDate: '',
        startDate: '',
        endDate: '',
        recurrenceFrequencyId: null,
        fundId: null,
        campaignId: null,
        appealId: null,
        writeOffAmount: '',
        isWrittenOff: false,
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
        this.pledgeForm.reset({
        constituentId: pledgeData.constituentId,
        totalAmount: pledgeData.totalAmount?.toString() ?? '',
        balanceAmount: pledgeData.balanceAmount?.toString() ?? '',
        pledgeDate: pledgeData.pledgeDate ?? '',
        startDate: pledgeData.startDate ?? '',
        endDate: pledgeData.endDate ?? '',
        recurrenceFrequencyId: pledgeData.recurrenceFrequencyId,
        fundId: pledgeData.fundId,
        campaignId: pledgeData.campaignId,
        appealId: pledgeData.appealId,
        writeOffAmount: pledgeData.writeOffAmount?.toString() ?? '',
        isWrittenOff: pledgeData.isWrittenOff ?? false,
        notes: pledgeData.notes ?? '',
        versionNumber: pledgeData.versionNumber?.toString() ?? '',
        active: pledgeData.active ?? true,
        deleted: pledgeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.pledgeForm.markAsPristine();
    this.pledgeForm.markAsUntouched();
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


  public userIsSchedulerPledgeReader(): boolean {
    return this.pledgeService.userIsSchedulerPledgeReader();
  }

  public userIsSchedulerPledgeWriter(): boolean {
    return this.pledgeService.userIsSchedulerPledgeWriter();
  }
}
