/*
   GENERATED FORM FOR THE SUBMODELCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SubmodelChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to submodel-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SubmodelChangeHistoryService, SubmodelChangeHistoryData, SubmodelChangeHistorySubmitData } from '../../../bmc-data-services/submodel-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SubmodelService } from '../../../bmc-data-services/submodel.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SubmodelChangeHistoryFormValues {
  submodelId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-submodel-change-history-add-edit',
  templateUrl: './submodel-change-history-add-edit.component.html',
  styleUrls: ['./submodel-change-history-add-edit.component.scss']
})
export class SubmodelChangeHistoryAddEditComponent {
  @ViewChild('submodelChangeHistoryModal') submodelChangeHistoryModal!: TemplateRef<any>;
  @Output() submodelChangeHistoryChanged = new Subject<SubmodelChangeHistoryData[]>();
  @Input() submodelChangeHistorySubmitData: SubmodelChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SubmodelChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public submodelChangeHistoryForm: FormGroup = this.fb.group({
        submodelId: [null, Validators.required],
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

  submodelChangeHistories$ = this.submodelChangeHistoryService.GetSubmodelChangeHistoryList();
  submodels$ = this.submodelService.GetSubmodelList();

  constructor(
    private modalService: NgbModal,
    private submodelChangeHistoryService: SubmodelChangeHistoryService,
    private submodelService: SubmodelService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(submodelChangeHistoryData?: SubmodelChangeHistoryData) {

    if (submodelChangeHistoryData != null) {

      if (!this.submodelChangeHistoryService.userIsBMCSubmodelChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Submodel Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.submodelChangeHistorySubmitData = this.submodelChangeHistoryService.ConvertToSubmodelChangeHistorySubmitData(submodelChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(submodelChangeHistoryData);

    } else {

      if (!this.submodelChangeHistoryService.userIsBMCSubmodelChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Submodel Change Histories`,
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
        this.submodelChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.submodelChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.submodelChangeHistoryModal, {
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

    if (this.submodelChangeHistoryService.userIsBMCSubmodelChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Submodel Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.submodelChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.submodelChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.submodelChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const submodelChangeHistorySubmitData: SubmodelChangeHistorySubmitData = {
        id: this.submodelChangeHistorySubmitData?.id || 0,
        submodelId: Number(formValue.submodelId),
        versionNumber: this.submodelChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateSubmodelChangeHistory(submodelChangeHistorySubmitData);
      } else {
        this.addSubmodelChangeHistory(submodelChangeHistorySubmitData);
      }
  }

  private addSubmodelChangeHistory(submodelChangeHistoryData: SubmodelChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    submodelChangeHistoryData.versionNumber = 0;
    this.submodelChangeHistoryService.PostSubmodelChangeHistory(submodelChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSubmodelChangeHistory) => {

        this.submodelChangeHistoryService.ClearAllCaches();

        this.submodelChangeHistoryChanged.next([newSubmodelChangeHistory]);

        this.alertService.showMessage("Submodel Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/submodelchangehistory', newSubmodelChangeHistory.id]);
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
                                   'You do not have permission to save this Submodel Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Submodel Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Submodel Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSubmodelChangeHistory(submodelChangeHistoryData: SubmodelChangeHistorySubmitData) {
    this.submodelChangeHistoryService.PutSubmodelChangeHistory(submodelChangeHistoryData.id, submodelChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSubmodelChangeHistory) => {

        this.submodelChangeHistoryService.ClearAllCaches();

        this.submodelChangeHistoryChanged.next([updatedSubmodelChangeHistory]);

        this.alertService.showMessage("Submodel Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Submodel Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Submodel Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Submodel Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(submodelChangeHistoryData: SubmodelChangeHistoryData | null) {

    if (submodelChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.submodelChangeHistoryForm.reset({
        submodelId: null,
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
        this.submodelChangeHistoryForm.reset({
        submodelId: submodelChangeHistoryData.submodelId,
        versionNumber: submodelChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(submodelChangeHistoryData.timeStamp) ?? '',
        userId: submodelChangeHistoryData.userId?.toString() ?? '',
        data: submodelChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.submodelChangeHistoryForm.markAsPristine();
    this.submodelChangeHistoryForm.markAsUntouched();
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


  public userIsBMCSubmodelChangeHistoryReader(): boolean {
    return this.submodelChangeHistoryService.userIsBMCSubmodelChangeHistoryReader();
  }

  public userIsBMCSubmodelChangeHistoryWriter(): boolean {
    return this.submodelChangeHistoryService.userIsBMCSubmodelChangeHistoryWriter();
  }
}
