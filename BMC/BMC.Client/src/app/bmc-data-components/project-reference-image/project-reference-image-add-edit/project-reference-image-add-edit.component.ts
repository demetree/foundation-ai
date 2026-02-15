/*
   GENERATED FORM FOR THE PROJECTREFERENCEIMAGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ProjectReferenceImage table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to project-reference-image-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ProjectReferenceImageService, ProjectReferenceImageData, ProjectReferenceImageSubmitData } from '../../../bmc-data-services/project-reference-image.service';
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
interface ProjectReferenceImageFormValues {
  projectId: number | bigint,       // For FK link number
  name: string,
  imageFilePath: string | null,
  opacity: string | null,     // Stored as string for form input, converted to number on submit.
  positionX: string | null,     // Stored as string for form input, converted to number on submit.
  positionY: string | null,     // Stored as string for form input, converted to number on submit.
  positionZ: string | null,     // Stored as string for form input, converted to number on submit.
  scaleX: string | null,     // Stored as string for form input, converted to number on submit.
  scaleY: string | null,     // Stored as string for form input, converted to number on submit.
  isVisible: boolean,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-project-reference-image-add-edit',
  templateUrl: './project-reference-image-add-edit.component.html',
  styleUrls: ['./project-reference-image-add-edit.component.scss']
})
export class ProjectReferenceImageAddEditComponent {
  @ViewChild('projectReferenceImageModal') projectReferenceImageModal!: TemplateRef<any>;
  @Output() projectReferenceImageChanged = new Subject<ProjectReferenceImageData[]>();
  @Input() projectReferenceImageSubmitData: ProjectReferenceImageSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ProjectReferenceImageFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public projectReferenceImageForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        name: ['', Validators.required],
        imageFilePath: [''],
        opacity: [''],
        positionX: [''],
        positionY: [''],
        positionZ: [''],
        scaleX: [''],
        scaleY: [''],
        isVisible: [false],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  projectReferenceImages$ = this.projectReferenceImageService.GetProjectReferenceImageList();
  projects$ = this.projectService.GetProjectList();

  constructor(
    private modalService: NgbModal,
    private projectReferenceImageService: ProjectReferenceImageService,
    private projectService: ProjectService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(projectReferenceImageData?: ProjectReferenceImageData) {

    if (projectReferenceImageData != null) {

      if (!this.projectReferenceImageService.userIsBMCProjectReferenceImageReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Project Reference Images`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.projectReferenceImageSubmitData = this.projectReferenceImageService.ConvertToProjectReferenceImageSubmitData(projectReferenceImageData);
      this.isEditMode = true;
      this.objectGuid = projectReferenceImageData.objectGuid;

      this.buildFormValues(projectReferenceImageData);

    } else {

      if (!this.projectReferenceImageService.userIsBMCProjectReferenceImageWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Project Reference Images`,
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
        this.projectReferenceImageForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.projectReferenceImageForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.projectReferenceImageModal, {
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

    if (this.projectReferenceImageService.userIsBMCProjectReferenceImageWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Project Reference Images`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.projectReferenceImageForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.projectReferenceImageForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.projectReferenceImageForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const projectReferenceImageSubmitData: ProjectReferenceImageSubmitData = {
        id: this.projectReferenceImageSubmitData?.id || 0,
        projectId: Number(formValue.projectId),
        name: formValue.name!.trim(),
        imageFilePath: formValue.imageFilePath?.trim() || null,
        opacity: formValue.opacity ? Number(formValue.opacity) : null,
        positionX: formValue.positionX ? Number(formValue.positionX) : null,
        positionY: formValue.positionY ? Number(formValue.positionY) : null,
        positionZ: formValue.positionZ ? Number(formValue.positionZ) : null,
        scaleX: formValue.scaleX ? Number(formValue.scaleX) : null,
        scaleY: formValue.scaleY ? Number(formValue.scaleY) : null,
        isVisible: !!formValue.isVisible,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateProjectReferenceImage(projectReferenceImageSubmitData);
      } else {
        this.addProjectReferenceImage(projectReferenceImageSubmitData);
      }
  }

  private addProjectReferenceImage(projectReferenceImageData: ProjectReferenceImageSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    projectReferenceImageData.active = true;
    projectReferenceImageData.deleted = false;
    this.projectReferenceImageService.PostProjectReferenceImage(projectReferenceImageData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newProjectReferenceImage) => {

        this.projectReferenceImageService.ClearAllCaches();

        this.projectReferenceImageChanged.next([newProjectReferenceImage]);

        this.alertService.showMessage("Project Reference Image added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/projectreferenceimage', newProjectReferenceImage.id]);
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
                                   'You do not have permission to save this Project Reference Image.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Project Reference Image.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Project Reference Image could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateProjectReferenceImage(projectReferenceImageData: ProjectReferenceImageSubmitData) {
    this.projectReferenceImageService.PutProjectReferenceImage(projectReferenceImageData.id, projectReferenceImageData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedProjectReferenceImage) => {

        this.projectReferenceImageService.ClearAllCaches();

        this.projectReferenceImageChanged.next([updatedProjectReferenceImage]);

        this.alertService.showMessage("Project Reference Image updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Project Reference Image.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Project Reference Image.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Project Reference Image could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(projectReferenceImageData: ProjectReferenceImageData | null) {

    if (projectReferenceImageData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.projectReferenceImageForm.reset({
        projectId: null,
        name: '',
        imageFilePath: '',
        opacity: '',
        positionX: '',
        positionY: '',
        positionZ: '',
        scaleX: '',
        scaleY: '',
        isVisible: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.projectReferenceImageForm.reset({
        projectId: projectReferenceImageData.projectId,
        name: projectReferenceImageData.name ?? '',
        imageFilePath: projectReferenceImageData.imageFilePath ?? '',
        opacity: projectReferenceImageData.opacity?.toString() ?? '',
        positionX: projectReferenceImageData.positionX?.toString() ?? '',
        positionY: projectReferenceImageData.positionY?.toString() ?? '',
        positionZ: projectReferenceImageData.positionZ?.toString() ?? '',
        scaleX: projectReferenceImageData.scaleX?.toString() ?? '',
        scaleY: projectReferenceImageData.scaleY?.toString() ?? '',
        isVisible: projectReferenceImageData.isVisible ?? false,
        active: projectReferenceImageData.active ?? true,
        deleted: projectReferenceImageData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.projectReferenceImageForm.markAsPristine();
    this.projectReferenceImageForm.markAsUntouched();
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


  public userIsBMCProjectReferenceImageReader(): boolean {
    return this.projectReferenceImageService.userIsBMCProjectReferenceImageReader();
  }

  public userIsBMCProjectReferenceImageWriter(): boolean {
    return this.projectReferenceImageService.userIsBMCProjectReferenceImageWriter();
  }
}
