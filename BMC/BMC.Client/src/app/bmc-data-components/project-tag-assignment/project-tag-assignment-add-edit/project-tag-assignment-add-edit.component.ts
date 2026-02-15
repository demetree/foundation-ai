/*
   GENERATED FORM FOR THE PROJECTTAGASSIGNMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ProjectTagAssignment table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to project-tag-assignment-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ProjectTagAssignmentService, ProjectTagAssignmentData, ProjectTagAssignmentSubmitData } from '../../../bmc-data-services/project-tag-assignment.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ProjectService } from '../../../bmc-data-services/project.service';
import { ProjectTagService } from '../../../bmc-data-services/project-tag.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ProjectTagAssignmentFormValues {
  projectId: number | bigint,       // For FK link number
  projectTagId: number | bigint,       // For FK link number
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-project-tag-assignment-add-edit',
  templateUrl: './project-tag-assignment-add-edit.component.html',
  styleUrls: ['./project-tag-assignment-add-edit.component.scss']
})
export class ProjectTagAssignmentAddEditComponent {
  @ViewChild('projectTagAssignmentModal') projectTagAssignmentModal!: TemplateRef<any>;
  @Output() projectTagAssignmentChanged = new Subject<ProjectTagAssignmentData[]>();
  @Input() projectTagAssignmentSubmitData: ProjectTagAssignmentSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ProjectTagAssignmentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public projectTagAssignmentForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        projectTagId: [null, Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  projectTagAssignments$ = this.projectTagAssignmentService.GetProjectTagAssignmentList();
  projects$ = this.projectService.GetProjectList();
  projectTags$ = this.projectTagService.GetProjectTagList();

  constructor(
    private modalService: NgbModal,
    private projectTagAssignmentService: ProjectTagAssignmentService,
    private projectService: ProjectService,
    private projectTagService: ProjectTagService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(projectTagAssignmentData?: ProjectTagAssignmentData) {

    if (projectTagAssignmentData != null) {

      if (!this.projectTagAssignmentService.userIsBMCProjectTagAssignmentReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Project Tag Assignments`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.projectTagAssignmentSubmitData = this.projectTagAssignmentService.ConvertToProjectTagAssignmentSubmitData(projectTagAssignmentData);
      this.isEditMode = true;
      this.objectGuid = projectTagAssignmentData.objectGuid;

      this.buildFormValues(projectTagAssignmentData);

    } else {

      if (!this.projectTagAssignmentService.userIsBMCProjectTagAssignmentWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Project Tag Assignments`,
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
        this.projectTagAssignmentForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.projectTagAssignmentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.projectTagAssignmentModal, {
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

    if (this.projectTagAssignmentService.userIsBMCProjectTagAssignmentWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Project Tag Assignments`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.projectTagAssignmentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.projectTagAssignmentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.projectTagAssignmentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const projectTagAssignmentSubmitData: ProjectTagAssignmentSubmitData = {
        id: this.projectTagAssignmentSubmitData?.id || 0,
        projectId: Number(formValue.projectId),
        projectTagId: Number(formValue.projectTagId),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateProjectTagAssignment(projectTagAssignmentSubmitData);
      } else {
        this.addProjectTagAssignment(projectTagAssignmentSubmitData);
      }
  }

  private addProjectTagAssignment(projectTagAssignmentData: ProjectTagAssignmentSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    projectTagAssignmentData.active = true;
    projectTagAssignmentData.deleted = false;
    this.projectTagAssignmentService.PostProjectTagAssignment(projectTagAssignmentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newProjectTagAssignment) => {

        this.projectTagAssignmentService.ClearAllCaches();

        this.projectTagAssignmentChanged.next([newProjectTagAssignment]);

        this.alertService.showMessage("Project Tag Assignment added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/projecttagassignment', newProjectTagAssignment.id]);
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
                                   'You do not have permission to save this Project Tag Assignment.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Project Tag Assignment.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Project Tag Assignment could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateProjectTagAssignment(projectTagAssignmentData: ProjectTagAssignmentSubmitData) {
    this.projectTagAssignmentService.PutProjectTagAssignment(projectTagAssignmentData.id, projectTagAssignmentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedProjectTagAssignment) => {

        this.projectTagAssignmentService.ClearAllCaches();

        this.projectTagAssignmentChanged.next([updatedProjectTagAssignment]);

        this.alertService.showMessage("Project Tag Assignment updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Project Tag Assignment.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Project Tag Assignment.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Project Tag Assignment could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(projectTagAssignmentData: ProjectTagAssignmentData | null) {

    if (projectTagAssignmentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.projectTagAssignmentForm.reset({
        projectId: null,
        projectTagId: null,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.projectTagAssignmentForm.reset({
        projectId: projectTagAssignmentData.projectId,
        projectTagId: projectTagAssignmentData.projectTagId,
        active: projectTagAssignmentData.active ?? true,
        deleted: projectTagAssignmentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.projectTagAssignmentForm.markAsPristine();
    this.projectTagAssignmentForm.markAsUntouched();
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


  public userIsBMCProjectTagAssignmentReader(): boolean {
    return this.projectTagAssignmentService.userIsBMCProjectTagAssignmentReader();
  }

  public userIsBMCProjectTagAssignmentWriter(): boolean {
    return this.projectTagAssignmentService.userIsBMCProjectTagAssignmentWriter();
  }
}
