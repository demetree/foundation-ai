/*
   GENERATED FORM FOR THE SUBMODEL TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Submodel table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to submodel-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SubmodelService, SubmodelData, SubmodelSubmitData } from '../../../bmc-data-services/submodel.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ProjectService } from '../../../bmc-data-services/project.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SubmodelFormValues {
  projectId: number | bigint,       // For FK link number
  name: string,
  description: string,
  submodelId: number | bigint | null,       // For FK link number
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-submodel-add-edit',
  templateUrl: './submodel-add-edit.component.html',
  styleUrls: ['./submodel-add-edit.component.scss']
})
export class SubmodelAddEditComponent {
  @ViewChild('submodelModal') submodelModal!: TemplateRef<any>;
  @Output() submodelChanged = new Subject<SubmodelData[]>();
  @Input() submodelSubmitData: SubmodelSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SubmodelFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public submodelForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        name: ['', Validators.required],
        description: ['', Validators.required],
        submodelId: [null],
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

  submodels$ = this.submodelService.GetSubmodelList();
  projects$ = this.projectService.GetProjectList();

  constructor(
    private modalService: NgbModal,
    private submodelService: SubmodelService,
    private projectService: ProjectService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(submodelData?: SubmodelData) {

    if (submodelData != null) {

      if (!this.submodelService.userIsBMCSubmodelReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Submodels`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.submodelSubmitData = this.submodelService.ConvertToSubmodelSubmitData(submodelData);
      this.isEditMode = true;
      this.objectGuid = submodelData.objectGuid;

      this.buildFormValues(submodelData);

    } else {

      if (!this.submodelService.userIsBMCSubmodelWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Submodels`,
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
        this.submodelForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.submodelForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.submodelModal, {
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

    if (this.submodelService.userIsBMCSubmodelWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Submodels`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.submodelForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.submodelForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.submodelForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const submodelSubmitData: SubmodelSubmitData = {
        id: this.submodelSubmitData?.id || 0,
        projectId: Number(formValue.projectId),
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        submodelId: formValue.submodelId ? Number(formValue.submodelId) : null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        versionNumber: this.submodelSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSubmodel(submodelSubmitData);
      } else {
        this.addSubmodel(submodelSubmitData);
      }
  }

  private addSubmodel(submodelData: SubmodelSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    submodelData.versionNumber = 0;
    submodelData.active = true;
    submodelData.deleted = false;
    this.submodelService.PostSubmodel(submodelData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSubmodel) => {

        this.submodelService.ClearAllCaches();

        this.submodelChanged.next([newSubmodel]);

        this.alertService.showMessage("Submodel added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/submodel', newSubmodel.id]);
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
                                   'You do not have permission to save this Submodel.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Submodel.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Submodel could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSubmodel(submodelData: SubmodelSubmitData) {
    this.submodelService.PutSubmodel(submodelData.id, submodelData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSubmodel) => {

        this.submodelService.ClearAllCaches();

        this.submodelChanged.next([updatedSubmodel]);

        this.alertService.showMessage("Submodel updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Submodel.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Submodel.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Submodel could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(submodelData: SubmodelData | null) {

    if (submodelData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.submodelForm.reset({
        projectId: null,
        name: '',
        description: '',
        submodelId: null,
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
        this.submodelForm.reset({
        projectId: submodelData.projectId,
        name: submodelData.name ?? '',
        description: submodelData.description ?? '',
        submodelId: submodelData.submodelId,
        sequence: submodelData.sequence?.toString() ?? '',
        versionNumber: submodelData.versionNumber?.toString() ?? '',
        active: submodelData.active ?? true,
        deleted: submodelData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.submodelForm.markAsPristine();
    this.submodelForm.markAsUntouched();
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


  public userIsBMCSubmodelReader(): boolean {
    return this.submodelService.userIsBMCSubmodelReader();
  }

  public userIsBMCSubmodelWriter(): boolean {
    return this.submodelService.userIsBMCSubmodelWriter();
  }
}
