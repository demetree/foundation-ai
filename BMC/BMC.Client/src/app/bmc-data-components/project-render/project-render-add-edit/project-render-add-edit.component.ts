/*
   GENERATED FORM FOR THE PROJECTRENDER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ProjectRender table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to project-render-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ProjectRenderService, ProjectRenderData, ProjectRenderSubmitData } from '../../../bmc-data-services/project-render.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ProjectService } from '../../../bmc-data-services/project.service';
import { RenderPresetService } from '../../../bmc-data-services/render-preset.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ProjectRenderFormValues {
  projectId: number | bigint,       // For FK link number
  renderPresetId: number | bigint | null,       // For FK link number
  name: string,
  outputFilePath: string | null,
  resolutionWidth: string | null,     // Stored as string for form input, converted to number on submit.
  resolutionHeight: string | null,     // Stored as string for form input, converted to number on submit.
  renderedDate: string | null,
  renderDurationSeconds: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-project-render-add-edit',
  templateUrl: './project-render-add-edit.component.html',
  styleUrls: ['./project-render-add-edit.component.scss']
})
export class ProjectRenderAddEditComponent {
  @ViewChild('projectRenderModal') projectRenderModal!: TemplateRef<any>;
  @Output() projectRenderChanged = new Subject<ProjectRenderData[]>();
  @Input() projectRenderSubmitData: ProjectRenderSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ProjectRenderFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public projectRenderForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        renderPresetId: [null],
        name: ['', Validators.required],
        outputFilePath: [''],
        resolutionWidth: [''],
        resolutionHeight: [''],
        renderedDate: [''],
        renderDurationSeconds: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  projectRenders$ = this.projectRenderService.GetProjectRenderList();
  projects$ = this.projectService.GetProjectList();
  renderPresets$ = this.renderPresetService.GetRenderPresetList();

  constructor(
    private modalService: NgbModal,
    private projectRenderService: ProjectRenderService,
    private projectService: ProjectService,
    private renderPresetService: RenderPresetService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(projectRenderData?: ProjectRenderData) {

    if (projectRenderData != null) {

      if (!this.projectRenderService.userIsBMCProjectRenderReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Project Renders`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.projectRenderSubmitData = this.projectRenderService.ConvertToProjectRenderSubmitData(projectRenderData);
      this.isEditMode = true;
      this.objectGuid = projectRenderData.objectGuid;

      this.buildFormValues(projectRenderData);

    } else {

      if (!this.projectRenderService.userIsBMCProjectRenderWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Project Renders`,
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
        this.projectRenderForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.projectRenderForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.projectRenderModal, {
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

    if (this.projectRenderService.userIsBMCProjectRenderWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Project Renders`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.projectRenderForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.projectRenderForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.projectRenderForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const projectRenderSubmitData: ProjectRenderSubmitData = {
        id: this.projectRenderSubmitData?.id || 0,
        projectId: Number(formValue.projectId),
        renderPresetId: formValue.renderPresetId ? Number(formValue.renderPresetId) : null,
        name: formValue.name!.trim(),
        outputFilePath: formValue.outputFilePath?.trim() || null,
        resolutionWidth: formValue.resolutionWidth ? Number(formValue.resolutionWidth) : null,
        resolutionHeight: formValue.resolutionHeight ? Number(formValue.resolutionHeight) : null,
        renderedDate: formValue.renderedDate ? dateTimeLocalToIsoUtc(formValue.renderedDate.trim()) : null,
        renderDurationSeconds: formValue.renderDurationSeconds ? Number(formValue.renderDurationSeconds) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateProjectRender(projectRenderSubmitData);
      } else {
        this.addProjectRender(projectRenderSubmitData);
      }
  }

  private addProjectRender(projectRenderData: ProjectRenderSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    projectRenderData.active = true;
    projectRenderData.deleted = false;
    this.projectRenderService.PostProjectRender(projectRenderData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newProjectRender) => {

        this.projectRenderService.ClearAllCaches();

        this.projectRenderChanged.next([newProjectRender]);

        this.alertService.showMessage("Project Render added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/projectrender', newProjectRender.id]);
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
                                   'You do not have permission to save this Project Render.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Project Render.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Project Render could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateProjectRender(projectRenderData: ProjectRenderSubmitData) {
    this.projectRenderService.PutProjectRender(projectRenderData.id, projectRenderData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedProjectRender) => {

        this.projectRenderService.ClearAllCaches();

        this.projectRenderChanged.next([updatedProjectRender]);

        this.alertService.showMessage("Project Render updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Project Render.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Project Render.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Project Render could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(projectRenderData: ProjectRenderData | null) {

    if (projectRenderData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.projectRenderForm.reset({
        projectId: null,
        renderPresetId: null,
        name: '',
        outputFilePath: '',
        resolutionWidth: '',
        resolutionHeight: '',
        renderedDate: '',
        renderDurationSeconds: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.projectRenderForm.reset({
        projectId: projectRenderData.projectId,
        renderPresetId: projectRenderData.renderPresetId,
        name: projectRenderData.name ?? '',
        outputFilePath: projectRenderData.outputFilePath ?? '',
        resolutionWidth: projectRenderData.resolutionWidth?.toString() ?? '',
        resolutionHeight: projectRenderData.resolutionHeight?.toString() ?? '',
        renderedDate: isoUtcStringToDateTimeLocal(projectRenderData.renderedDate) ?? '',
        renderDurationSeconds: projectRenderData.renderDurationSeconds?.toString() ?? '',
        active: projectRenderData.active ?? true,
        deleted: projectRenderData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.projectRenderForm.markAsPristine();
    this.projectRenderForm.markAsUntouched();
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


  public userIsBMCProjectRenderReader(): boolean {
    return this.projectRenderService.userIsBMCProjectRenderReader();
  }

  public userIsBMCProjectRenderWriter(): boolean {
    return this.projectRenderService.userIsBMCProjectRenderWriter();
  }
}
