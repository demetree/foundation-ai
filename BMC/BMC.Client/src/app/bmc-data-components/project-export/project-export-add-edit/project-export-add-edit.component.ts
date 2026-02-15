/*
   GENERATED FORM FOR THE PROJECTEXPORT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ProjectExport table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to project-export-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ProjectExportService, ProjectExportData, ProjectExportSubmitData } from '../../../bmc-data-services/project-export.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ProjectService } from '../../../bmc-data-services/project.service';
import { ExportFormatService } from '../../../bmc-data-services/export-format.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ProjectExportFormValues {
  projectId: number | bigint,       // For FK link number
  exportFormatId: number | bigint,       // For FK link number
  name: string,
  outputFilePath: string | null,
  exportedDate: string | null,
  includeInstructions: boolean,
  includePartsList: boolean,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-project-export-add-edit',
  templateUrl: './project-export-add-edit.component.html',
  styleUrls: ['./project-export-add-edit.component.scss']
})
export class ProjectExportAddEditComponent {
  @ViewChild('projectExportModal') projectExportModal!: TemplateRef<any>;
  @Output() projectExportChanged = new Subject<ProjectExportData[]>();
  @Input() projectExportSubmitData: ProjectExportSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ProjectExportFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public projectExportForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        exportFormatId: [null, Validators.required],
        name: ['', Validators.required],
        outputFilePath: [''],
        exportedDate: [''],
        includeInstructions: [false],
        includePartsList: [false],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  projectExports$ = this.projectExportService.GetProjectExportList();
  projects$ = this.projectService.GetProjectList();
  exportFormats$ = this.exportFormatService.GetExportFormatList();

  constructor(
    private modalService: NgbModal,
    private projectExportService: ProjectExportService,
    private projectService: ProjectService,
    private exportFormatService: ExportFormatService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(projectExportData?: ProjectExportData) {

    if (projectExportData != null) {

      if (!this.projectExportService.userIsBMCProjectExportReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Project Exports`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.projectExportSubmitData = this.projectExportService.ConvertToProjectExportSubmitData(projectExportData);
      this.isEditMode = true;
      this.objectGuid = projectExportData.objectGuid;

      this.buildFormValues(projectExportData);

    } else {

      if (!this.projectExportService.userIsBMCProjectExportWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Project Exports`,
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
        this.projectExportForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.projectExportForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.projectExportModal, {
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

    if (this.projectExportService.userIsBMCProjectExportWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Project Exports`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.projectExportForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.projectExportForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.projectExportForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const projectExportSubmitData: ProjectExportSubmitData = {
        id: this.projectExportSubmitData?.id || 0,
        projectId: Number(formValue.projectId),
        exportFormatId: Number(formValue.exportFormatId),
        name: formValue.name!.trim(),
        outputFilePath: formValue.outputFilePath?.trim() || null,
        exportedDate: formValue.exportedDate ? dateTimeLocalToIsoUtc(formValue.exportedDate.trim()) : null,
        includeInstructions: !!formValue.includeInstructions,
        includePartsList: !!formValue.includePartsList,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateProjectExport(projectExportSubmitData);
      } else {
        this.addProjectExport(projectExportSubmitData);
      }
  }

  private addProjectExport(projectExportData: ProjectExportSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    projectExportData.active = true;
    projectExportData.deleted = false;
    this.projectExportService.PostProjectExport(projectExportData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newProjectExport) => {

        this.projectExportService.ClearAllCaches();

        this.projectExportChanged.next([newProjectExport]);

        this.alertService.showMessage("Project Export added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/projectexport', newProjectExport.id]);
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
                                   'You do not have permission to save this Project Export.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Project Export.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Project Export could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateProjectExport(projectExportData: ProjectExportSubmitData) {
    this.projectExportService.PutProjectExport(projectExportData.id, projectExportData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedProjectExport) => {

        this.projectExportService.ClearAllCaches();

        this.projectExportChanged.next([updatedProjectExport]);

        this.alertService.showMessage("Project Export updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Project Export.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Project Export.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Project Export could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(projectExportData: ProjectExportData | null) {

    if (projectExportData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.projectExportForm.reset({
        projectId: null,
        exportFormatId: null,
        name: '',
        outputFilePath: '',
        exportedDate: '',
        includeInstructions: false,
        includePartsList: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.projectExportForm.reset({
        projectId: projectExportData.projectId,
        exportFormatId: projectExportData.exportFormatId,
        name: projectExportData.name ?? '',
        outputFilePath: projectExportData.outputFilePath ?? '',
        exportedDate: isoUtcStringToDateTimeLocal(projectExportData.exportedDate) ?? '',
        includeInstructions: projectExportData.includeInstructions ?? false,
        includePartsList: projectExportData.includePartsList ?? false,
        active: projectExportData.active ?? true,
        deleted: projectExportData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.projectExportForm.markAsPristine();
    this.projectExportForm.markAsUntouched();
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


  public userIsBMCProjectExportReader(): boolean {
    return this.projectExportService.userIsBMCProjectExportReader();
  }

  public userIsBMCProjectExportWriter(): boolean {
    return this.projectExportService.userIsBMCProjectExportWriter();
  }
}
