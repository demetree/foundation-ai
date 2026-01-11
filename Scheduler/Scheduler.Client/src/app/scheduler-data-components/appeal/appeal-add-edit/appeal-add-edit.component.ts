/*
   GENERATED FORM FOR THE APPEAL TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Appeal table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to appeal-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AppealService, AppealData, AppealSubmitData } from '../../../scheduler-data-services/appeal.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { CampaignService } from '../../../scheduler-data-services/campaign.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface AppealFormValues {
  campaignId: number | bigint | null,       // For FK link number
  name: string,
  description: string | null,
  costPerUnit: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-appeal-add-edit',
  templateUrl: './appeal-add-edit.component.html',
  styleUrls: ['./appeal-add-edit.component.scss']
})
export class AppealAddEditComponent {
  @ViewChild('appealModal') appealModal!: TemplateRef<any>;
  @Output() appealChanged = new Subject<AppealData[]>();
  @Input() appealSubmitData: AppealSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AppealFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public appealForm: FormGroup = this.fb.group({
        campaignId: [null],
        name: ['', Validators.required],
        description: [''],
        costPerUnit: [''],
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

  appeals$ = this.appealService.GetAppealList();
  campaigns$ = this.campaignService.GetCampaignList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private appealService: AppealService,
    private campaignService: CampaignService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(appealData?: AppealData) {

    if (appealData != null) {

      if (!this.appealService.userIsSchedulerAppealReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Appeals`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.appealSubmitData = this.appealService.ConvertToAppealSubmitData(appealData);
      this.isEditMode = true;
      this.objectGuid = appealData.objectGuid;

      this.buildFormValues(appealData);

    } else {

      if (!this.appealService.userIsSchedulerAppealWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Appeals`,
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
        this.appealForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.appealForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.appealModal, {
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

    if (this.appealService.userIsSchedulerAppealWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Appeals`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.appealForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.appealForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.appealForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const appealSubmitData: AppealSubmitData = {
        id: this.appealSubmitData?.id || 0,
        campaignId: formValue.campaignId ? Number(formValue.campaignId) : null,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        costPerUnit: formValue.costPerUnit ? Number(formValue.costPerUnit) : null,
        notes: formValue.notes?.trim() || null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        versionNumber: this.appealSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateAppeal(appealSubmitData);
      } else {
        this.addAppeal(appealSubmitData);
      }
  }

  private addAppeal(appealData: AppealSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    appealData.versionNumber = 0;
    appealData.active = true;
    appealData.deleted = false;
    this.appealService.PostAppeal(appealData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAppeal) => {

        this.appealService.ClearAllCaches();

        this.appealChanged.next([newAppeal]);

        this.alertService.showMessage("Appeal added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/appeal', newAppeal.id]);
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
                                   'You do not have permission to save this Appeal.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Appeal.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Appeal could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAppeal(appealData: AppealSubmitData) {
    this.appealService.PutAppeal(appealData.id, appealData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAppeal) => {

        this.appealService.ClearAllCaches();

        this.appealChanged.next([updatedAppeal]);

        this.alertService.showMessage("Appeal updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Appeal.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Appeal.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Appeal could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(appealData: AppealData | null) {

    if (appealData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.appealForm.reset({
        campaignId: null,
        name: '',
        description: '',
        costPerUnit: '',
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
        this.appealForm.reset({
        campaignId: appealData.campaignId,
        name: appealData.name ?? '',
        description: appealData.description ?? '',
        costPerUnit: appealData.costPerUnit?.toString() ?? '',
        notes: appealData.notes ?? '',
        iconId: appealData.iconId,
        color: appealData.color ?? '',
        versionNumber: appealData.versionNumber?.toString() ?? '',
        active: appealData.active ?? true,
        deleted: appealData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.appealForm.markAsPristine();
    this.appealForm.markAsUntouched();
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


  public userIsSchedulerAppealReader(): boolean {
    return this.appealService.userIsSchedulerAppealReader();
  }

  public userIsSchedulerAppealWriter(): boolean {
    return this.appealService.userIsSchedulerAppealWriter();
  }
}
