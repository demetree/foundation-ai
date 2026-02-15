/*
   GENERATED FORM FOR THE PROJECTCAMERAPRESET TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ProjectCameraPreset table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to project-camera-preset-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ProjectCameraPresetService, ProjectCameraPresetData, ProjectCameraPresetSubmitData } from '../../../bmc-data-services/project-camera-preset.service';
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
interface ProjectCameraPresetFormValues {
  projectId: number | bigint,       // For FK link number
  name: string,
  positionX: string | null,     // Stored as string for form input, converted to number on submit.
  positionY: string | null,     // Stored as string for form input, converted to number on submit.
  positionZ: string | null,     // Stored as string for form input, converted to number on submit.
  targetX: string | null,     // Stored as string for form input, converted to number on submit.
  targetY: string | null,     // Stored as string for form input, converted to number on submit.
  targetZ: string | null,     // Stored as string for form input, converted to number on submit.
  zoom: string | null,     // Stored as string for form input, converted to number on submit.
  isPerspective: boolean,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-project-camera-preset-add-edit',
  templateUrl: './project-camera-preset-add-edit.component.html',
  styleUrls: ['./project-camera-preset-add-edit.component.scss']
})
export class ProjectCameraPresetAddEditComponent {
  @ViewChild('projectCameraPresetModal') projectCameraPresetModal!: TemplateRef<any>;
  @Output() projectCameraPresetChanged = new Subject<ProjectCameraPresetData[]>();
  @Input() projectCameraPresetSubmitData: ProjectCameraPresetSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ProjectCameraPresetFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public projectCameraPresetForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        name: ['', Validators.required],
        positionX: [''],
        positionY: [''],
        positionZ: [''],
        targetX: [''],
        targetY: [''],
        targetZ: [''],
        zoom: [''],
        isPerspective: [false],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  projectCameraPresets$ = this.projectCameraPresetService.GetProjectCameraPresetList();
  projects$ = this.projectService.GetProjectList();

  constructor(
    private modalService: NgbModal,
    private projectCameraPresetService: ProjectCameraPresetService,
    private projectService: ProjectService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(projectCameraPresetData?: ProjectCameraPresetData) {

    if (projectCameraPresetData != null) {

      if (!this.projectCameraPresetService.userIsBMCProjectCameraPresetReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Project Camera Presets`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.projectCameraPresetSubmitData = this.projectCameraPresetService.ConvertToProjectCameraPresetSubmitData(projectCameraPresetData);
      this.isEditMode = true;
      this.objectGuid = projectCameraPresetData.objectGuid;

      this.buildFormValues(projectCameraPresetData);

    } else {

      if (!this.projectCameraPresetService.userIsBMCProjectCameraPresetWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Project Camera Presets`,
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
        this.projectCameraPresetForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.projectCameraPresetForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.projectCameraPresetModal, {
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

    if (this.projectCameraPresetService.userIsBMCProjectCameraPresetWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Project Camera Presets`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.projectCameraPresetForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.projectCameraPresetForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.projectCameraPresetForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const projectCameraPresetSubmitData: ProjectCameraPresetSubmitData = {
        id: this.projectCameraPresetSubmitData?.id || 0,
        projectId: Number(formValue.projectId),
        name: formValue.name!.trim(),
        positionX: formValue.positionX ? Number(formValue.positionX) : null,
        positionY: formValue.positionY ? Number(formValue.positionY) : null,
        positionZ: formValue.positionZ ? Number(formValue.positionZ) : null,
        targetX: formValue.targetX ? Number(formValue.targetX) : null,
        targetY: formValue.targetY ? Number(formValue.targetY) : null,
        targetZ: formValue.targetZ ? Number(formValue.targetZ) : null,
        zoom: formValue.zoom ? Number(formValue.zoom) : null,
        isPerspective: !!formValue.isPerspective,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateProjectCameraPreset(projectCameraPresetSubmitData);
      } else {
        this.addProjectCameraPreset(projectCameraPresetSubmitData);
      }
  }

  private addProjectCameraPreset(projectCameraPresetData: ProjectCameraPresetSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    projectCameraPresetData.active = true;
    projectCameraPresetData.deleted = false;
    this.projectCameraPresetService.PostProjectCameraPreset(projectCameraPresetData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newProjectCameraPreset) => {

        this.projectCameraPresetService.ClearAllCaches();

        this.projectCameraPresetChanged.next([newProjectCameraPreset]);

        this.alertService.showMessage("Project Camera Preset added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/projectcamerapreset', newProjectCameraPreset.id]);
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
                                   'You do not have permission to save this Project Camera Preset.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Project Camera Preset.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Project Camera Preset could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateProjectCameraPreset(projectCameraPresetData: ProjectCameraPresetSubmitData) {
    this.projectCameraPresetService.PutProjectCameraPreset(projectCameraPresetData.id, projectCameraPresetData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedProjectCameraPreset) => {

        this.projectCameraPresetService.ClearAllCaches();

        this.projectCameraPresetChanged.next([updatedProjectCameraPreset]);

        this.alertService.showMessage("Project Camera Preset updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Project Camera Preset.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Project Camera Preset.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Project Camera Preset could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(projectCameraPresetData: ProjectCameraPresetData | null) {

    if (projectCameraPresetData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.projectCameraPresetForm.reset({
        projectId: null,
        name: '',
        positionX: '',
        positionY: '',
        positionZ: '',
        targetX: '',
        targetY: '',
        targetZ: '',
        zoom: '',
        isPerspective: false,
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.projectCameraPresetForm.reset({
        projectId: projectCameraPresetData.projectId,
        name: projectCameraPresetData.name ?? '',
        positionX: projectCameraPresetData.positionX?.toString() ?? '',
        positionY: projectCameraPresetData.positionY?.toString() ?? '',
        positionZ: projectCameraPresetData.positionZ?.toString() ?? '',
        targetX: projectCameraPresetData.targetX?.toString() ?? '',
        targetY: projectCameraPresetData.targetY?.toString() ?? '',
        targetZ: projectCameraPresetData.targetZ?.toString() ?? '',
        zoom: projectCameraPresetData.zoom?.toString() ?? '',
        isPerspective: projectCameraPresetData.isPerspective ?? false,
        sequence: projectCameraPresetData.sequence?.toString() ?? '',
        active: projectCameraPresetData.active ?? true,
        deleted: projectCameraPresetData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.projectCameraPresetForm.markAsPristine();
    this.projectCameraPresetForm.markAsUntouched();
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


  public userIsBMCProjectCameraPresetReader(): boolean {
    return this.projectCameraPresetService.userIsBMCProjectCameraPresetReader();
  }

  public userIsBMCProjectCameraPresetWriter(): boolean {
    return this.projectCameraPresetService.userIsBMCProjectCameraPresetWriter();
  }
}
