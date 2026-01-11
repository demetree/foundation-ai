/*
   GENERATED FORM FOR THE CAMPAIGNCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from CampaignChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to campaign-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CampaignChangeHistoryService, CampaignChangeHistoryData, CampaignChangeHistorySubmitData } from '../../../scheduler-data-services/campaign-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { CampaignService } from '../../../scheduler-data-services/campaign.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface CampaignChangeHistoryFormValues {
  campaignId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-campaign-change-history-add-edit',
  templateUrl: './campaign-change-history-add-edit.component.html',
  styleUrls: ['./campaign-change-history-add-edit.component.scss']
})
export class CampaignChangeHistoryAddEditComponent {
  @ViewChild('campaignChangeHistoryModal') campaignChangeHistoryModal!: TemplateRef<any>;
  @Output() campaignChangeHistoryChanged = new Subject<CampaignChangeHistoryData[]>();
  @Input() campaignChangeHistorySubmitData: CampaignChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CampaignChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public campaignChangeHistoryForm: FormGroup = this.fb.group({
        campaignId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  campaignChangeHistories$ = this.campaignChangeHistoryService.GetCampaignChangeHistoryList();
  campaigns$ = this.campaignService.GetCampaignList();

  constructor(
    private modalService: NgbModal,
    private campaignChangeHistoryService: CampaignChangeHistoryService,
    private campaignService: CampaignService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(campaignChangeHistoryData?: CampaignChangeHistoryData) {

    if (campaignChangeHistoryData != null) {

      if (!this.campaignChangeHistoryService.userIsSchedulerCampaignChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Campaign Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.campaignChangeHistorySubmitData = this.campaignChangeHistoryService.ConvertToCampaignChangeHistorySubmitData(campaignChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(campaignChangeHistoryData);

    } else {

      if (!this.campaignChangeHistoryService.userIsSchedulerCampaignChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Campaign Change Histories`,
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
        this.campaignChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.campaignChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.campaignChangeHistoryModal, {
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

    if (this.campaignChangeHistoryService.userIsSchedulerCampaignChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Campaign Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.campaignChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.campaignChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.campaignChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const campaignChangeHistorySubmitData: CampaignChangeHistorySubmitData = {
        id: this.campaignChangeHistorySubmitData?.id || 0,
        campaignId: Number(formValue.campaignId),
        versionNumber: this.campaignChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateCampaignChangeHistory(campaignChangeHistorySubmitData);
      } else {
        this.addCampaignChangeHistory(campaignChangeHistorySubmitData);
      }
  }

  private addCampaignChangeHistory(campaignChangeHistoryData: CampaignChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    campaignChangeHistoryData.versionNumber = 0;
    this.campaignChangeHistoryService.PostCampaignChangeHistory(campaignChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newCampaignChangeHistory) => {

        this.campaignChangeHistoryService.ClearAllCaches();

        this.campaignChangeHistoryChanged.next([newCampaignChangeHistory]);

        this.alertService.showMessage("Campaign Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/campaignchangehistory', newCampaignChangeHistory.id]);
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
                                   'You do not have permission to save this Campaign Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Campaign Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Campaign Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateCampaignChangeHistory(campaignChangeHistoryData: CampaignChangeHistorySubmitData) {
    this.campaignChangeHistoryService.PutCampaignChangeHistory(campaignChangeHistoryData.id, campaignChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedCampaignChangeHistory) => {

        this.campaignChangeHistoryService.ClearAllCaches();

        this.campaignChangeHistoryChanged.next([updatedCampaignChangeHistory]);

        this.alertService.showMessage("Campaign Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Campaign Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Campaign Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Campaign Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(campaignChangeHistoryData: CampaignChangeHistoryData | null) {

    if (campaignChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.campaignChangeHistoryForm.reset({
        campaignId: null,
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
        this.campaignChangeHistoryForm.reset({
        campaignId: campaignChangeHistoryData.campaignId,
        versionNumber: campaignChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(campaignChangeHistoryData.timeStamp) ?? '',
        userId: campaignChangeHistoryData.userId?.toString() ?? '',
        data: campaignChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.campaignChangeHistoryForm.markAsPristine();
    this.campaignChangeHistoryForm.markAsUntouched();
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


  public userIsSchedulerCampaignChangeHistoryReader(): boolean {
    return this.campaignChangeHistoryService.userIsSchedulerCampaignChangeHistoryReader();
  }

  public userIsSchedulerCampaignChangeHistoryWriter(): boolean {
    return this.campaignChangeHistoryService.userIsSchedulerCampaignChangeHistoryWriter();
  }
}
