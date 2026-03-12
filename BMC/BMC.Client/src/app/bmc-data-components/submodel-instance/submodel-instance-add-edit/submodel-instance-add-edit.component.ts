/*
   GENERATED FORM FOR THE SUBMODELINSTANCE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SubmodelInstance table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to submodel-instance-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SubmodelInstanceService, SubmodelInstanceData, SubmodelInstanceSubmitData } from '../../../bmc-data-services/submodel-instance.service';
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
interface SubmodelInstanceFormValues {
  submodelId: number | bigint,       // For FK link number
  parentSubmodelId: number | bigint | null,       // For FK link number
  positionX: string | null,     // Stored as string for form input, converted to number on submit.
  positionY: string | null,     // Stored as string for form input, converted to number on submit.
  positionZ: string | null,     // Stored as string for form input, converted to number on submit.
  rotationX: string | null,     // Stored as string for form input, converted to number on submit.
  rotationY: string | null,     // Stored as string for form input, converted to number on submit.
  rotationZ: string | null,     // Stored as string for form input, converted to number on submit.
  rotationW: string | null,     // Stored as string for form input, converted to number on submit.
  colourCode: string,     // Stored as string for form input, converted to number on submit.
  buildStepNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-submodel-instance-add-edit',
  templateUrl: './submodel-instance-add-edit.component.html',
  styleUrls: ['./submodel-instance-add-edit.component.scss']
})
export class SubmodelInstanceAddEditComponent {
  @ViewChild('submodelInstanceModal') submodelInstanceModal!: TemplateRef<any>;
  @Output() submodelInstanceChanged = new Subject<SubmodelInstanceData[]>();
  @Input() submodelInstanceSubmitData: SubmodelInstanceSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SubmodelInstanceFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public submodelInstanceForm: FormGroup = this.fb.group({
        submodelId: [null, Validators.required],
        parentSubmodelId: [null],
        positionX: [''],
        positionY: [''],
        positionZ: [''],
        rotationX: [''],
        rotationY: [''],
        rotationZ: [''],
        rotationW: [''],
        colourCode: ['', Validators.required],
        buildStepNumber: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  submodelInstances$ = this.submodelInstanceService.GetSubmodelInstanceList();
  submodels$ = this.submodelService.GetSubmodelList();

  constructor(
    private modalService: NgbModal,
    private submodelInstanceService: SubmodelInstanceService,
    private submodelService: SubmodelService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(submodelInstanceData?: SubmodelInstanceData) {

    if (submodelInstanceData != null) {

      if (!this.submodelInstanceService.userIsBMCSubmodelInstanceReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Submodel Instances`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.submodelInstanceSubmitData = this.submodelInstanceService.ConvertToSubmodelInstanceSubmitData(submodelInstanceData);
      this.isEditMode = true;
      this.objectGuid = submodelInstanceData.objectGuid;

      this.buildFormValues(submodelInstanceData);

    } else {

      if (!this.submodelInstanceService.userIsBMCSubmodelInstanceWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Submodel Instances`,
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
        this.submodelInstanceForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.submodelInstanceForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.submodelInstanceModal, {
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

    if (this.submodelInstanceService.userIsBMCSubmodelInstanceWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Submodel Instances`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.submodelInstanceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.submodelInstanceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.submodelInstanceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const submodelInstanceSubmitData: SubmodelInstanceSubmitData = {
        id: this.submodelInstanceSubmitData?.id || 0,
        submodelId: Number(formValue.submodelId),
        parentSubmodelId: formValue.parentSubmodelId ? Number(formValue.parentSubmodelId) : null,
        positionX: formValue.positionX ? Number(formValue.positionX) : null,
        positionY: formValue.positionY ? Number(formValue.positionY) : null,
        positionZ: formValue.positionZ ? Number(formValue.positionZ) : null,
        rotationX: formValue.rotationX ? Number(formValue.rotationX) : null,
        rotationY: formValue.rotationY ? Number(formValue.rotationY) : null,
        rotationZ: formValue.rotationZ ? Number(formValue.rotationZ) : null,
        rotationW: formValue.rotationW ? Number(formValue.rotationW) : null,
        colourCode: Number(formValue.colourCode),
        buildStepNumber: Number(formValue.buildStepNumber),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSubmodelInstance(submodelInstanceSubmitData);
      } else {
        this.addSubmodelInstance(submodelInstanceSubmitData);
      }
  }

  private addSubmodelInstance(submodelInstanceData: SubmodelInstanceSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    submodelInstanceData.active = true;
    submodelInstanceData.deleted = false;
    this.submodelInstanceService.PostSubmodelInstance(submodelInstanceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSubmodelInstance) => {

        this.submodelInstanceService.ClearAllCaches();

        this.submodelInstanceChanged.next([newSubmodelInstance]);

        this.alertService.showMessage("Submodel Instance added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/submodelinstance', newSubmodelInstance.id]);
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
                                   'You do not have permission to save this Submodel Instance.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Submodel Instance.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Submodel Instance could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSubmodelInstance(submodelInstanceData: SubmodelInstanceSubmitData) {
    this.submodelInstanceService.PutSubmodelInstance(submodelInstanceData.id, submodelInstanceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSubmodelInstance) => {

        this.submodelInstanceService.ClearAllCaches();

        this.submodelInstanceChanged.next([updatedSubmodelInstance]);

        this.alertService.showMessage("Submodel Instance updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Submodel Instance.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Submodel Instance.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Submodel Instance could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(submodelInstanceData: SubmodelInstanceData | null) {

    if (submodelInstanceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.submodelInstanceForm.reset({
        submodelId: null,
        parentSubmodelId: null,
        positionX: '',
        positionY: '',
        positionZ: '',
        rotationX: '',
        rotationY: '',
        rotationZ: '',
        rotationW: '',
        colourCode: '',
        buildStepNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.submodelInstanceForm.reset({
        submodelId: submodelInstanceData.submodelId,
        parentSubmodelId: submodelInstanceData.parentSubmodelId,
        positionX: submodelInstanceData.positionX?.toString() ?? '',
        positionY: submodelInstanceData.positionY?.toString() ?? '',
        positionZ: submodelInstanceData.positionZ?.toString() ?? '',
        rotationX: submodelInstanceData.rotationX?.toString() ?? '',
        rotationY: submodelInstanceData.rotationY?.toString() ?? '',
        rotationZ: submodelInstanceData.rotationZ?.toString() ?? '',
        rotationW: submodelInstanceData.rotationW?.toString() ?? '',
        colourCode: submodelInstanceData.colourCode?.toString() ?? '',
        buildStepNumber: submodelInstanceData.buildStepNumber?.toString() ?? '',
        active: submodelInstanceData.active ?? true,
        deleted: submodelInstanceData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.submodelInstanceForm.markAsPristine();
    this.submodelInstanceForm.markAsUntouched();
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


  public userIsBMCSubmodelInstanceReader(): boolean {
    return this.submodelInstanceService.userIsBMCSubmodelInstanceReader();
  }

  public userIsBMCSubmodelInstanceWriter(): boolean {
    return this.submodelInstanceService.userIsBMCSubmodelInstanceWriter();
  }
}
