/*
   GENERATED FORM FOR THE CAMPAIGN TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Campaign table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to campaign-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CampaignService, CampaignData, CampaignSubmitData } from '../../../scheduler-data-services/campaign.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface CampaignFormValues {
  name: string,
  description: string | null,
  startDate: string | null,
  endDate: string | null,
  fundRaisingGoal: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-campaign-add-edit',
  templateUrl: './campaign-add-edit.component.html',
  styleUrls: ['./campaign-add-edit.component.scss']
})
export class CampaignAddEditComponent {
  @ViewChild('campaignModal') campaignModal!: TemplateRef<any>;
  @Output() campaignChanged = new Subject<CampaignData[]>();
  @Input() campaignSubmitData: CampaignSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CampaignFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public campaignForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        startDate: [''],
        endDate: [''],
        fundRaisingGoal: [''],
        notes: [''],
        iconId: [null],
        color: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  campaigns$ = this.campaignService.GetCampaignList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private campaignService: CampaignService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(campaignData?: CampaignData) {

    if (campaignData != null) {

      if (!this.campaignService.userIsSchedulerCampaignReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Campaigns`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.campaignSubmitData = this.campaignService.ConvertToCampaignSubmitData(campaignData);
      this.isEditMode = true;
      this.objectGuid = campaignData.objectGuid;

      this.buildFormValues(campaignData);

    } else {

      if (!this.campaignService.userIsSchedulerCampaignWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Campaigns`,
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
        this.campaignForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.campaignForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.campaignModal, {
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

    if (this.campaignService.userIsSchedulerCampaignWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Campaigns`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.campaignForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.campaignForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.campaignForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const campaignSubmitData: CampaignSubmitData = {
        id: this.campaignSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        startDate: formValue.startDate?.trim() || null,
        endDate: formValue.endDate?.trim() || null,
        fundRaisingGoal: formValue.fundRaisingGoal ? Number(formValue.fundRaisingGoal) : null,
        notes: formValue.notes?.trim() || null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        versionNumber: this.campaignSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateCampaign(campaignSubmitData);
      } else {
        this.addCampaign(campaignSubmitData);
      }
  }

  private addCampaign(campaignData: CampaignSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    campaignData.versionNumber = 0;
    campaignData.active = true;
    campaignData.deleted = false;
    this.campaignService.PostCampaign(campaignData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newCampaign) => {

        this.campaignService.ClearAllCaches();

        this.campaignChanged.next([newCampaign]);

        this.alertService.showMessage("Campaign added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/campaign', newCampaign.id]);
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
                                   'You do not have permission to save this Campaign.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Campaign.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Campaign could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateCampaign(campaignData: CampaignSubmitData) {
    this.campaignService.PutCampaign(campaignData.id, campaignData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedCampaign) => {

        this.campaignService.ClearAllCaches();

        this.campaignChanged.next([updatedCampaign]);

        this.alertService.showMessage("Campaign updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Campaign.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Campaign.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Campaign could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(campaignData: CampaignData | null) {

    if (campaignData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.campaignForm.reset({
        name: '',
        description: '',
        startDate: '',
        endDate: '',
        fundRaisingGoal: '',
        notes: '',
        iconId: null,
        color: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.campaignForm.reset({
        name: campaignData.name ?? '',
        description: campaignData.description ?? '',
        startDate: campaignData.startDate ?? '',
        endDate: campaignData.endDate ?? '',
        fundRaisingGoal: campaignData.fundRaisingGoal?.toString() ?? '',
        notes: campaignData.notes ?? '',
        iconId: campaignData.iconId,
        color: campaignData.color ?? '',
        versionNumber: campaignData.versionNumber?.toString() ?? '',
        active: campaignData.active ?? true,
        deleted: campaignData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.campaignForm.markAsPristine();
    this.campaignForm.markAsUntouched();
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


  public userIsSchedulerCampaignReader(): boolean {
    return this.campaignService.userIsSchedulerCampaignReader();
  }

  public userIsSchedulerCampaignWriter(): boolean {
    return this.campaignService.userIsSchedulerCampaignWriter();
  }
}
