/*
   GENERATED FORM FOR THE PROJECTTAG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ProjectTag table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to project-tag-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ProjectTagService, ProjectTagData, ProjectTagSubmitData } from '../../../bmc-data-services/project-tag.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ProjectTagFormValues {
  name: string,
  description: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-project-tag-add-edit',
  templateUrl: './project-tag-add-edit.component.html',
  styleUrls: ['./project-tag-add-edit.component.scss']
})
export class ProjectTagAddEditComponent {
  @ViewChild('projectTagModal') projectTagModal!: TemplateRef<any>;
  @Output() projectTagChanged = new Subject<ProjectTagData[]>();
  @Input() projectTagSubmitData: ProjectTagSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ProjectTagFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public projectTagForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  projectTags$ = this.projectTagService.GetProjectTagList();

  constructor(
    private modalService: NgbModal,
    private projectTagService: ProjectTagService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(projectTagData?: ProjectTagData) {

    if (projectTagData != null) {

      if (!this.projectTagService.userIsBMCProjectTagReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Project Tags`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.projectTagSubmitData = this.projectTagService.ConvertToProjectTagSubmitData(projectTagData);
      this.isEditMode = true;
      this.objectGuid = projectTagData.objectGuid;

      this.buildFormValues(projectTagData);

    } else {

      if (!this.projectTagService.userIsBMCProjectTagWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Project Tags`,
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
        this.projectTagForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.projectTagForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.projectTagModal, {
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

    if (this.projectTagService.userIsBMCProjectTagWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Project Tags`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.projectTagForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.projectTagForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.projectTagForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const projectTagSubmitData: ProjectTagSubmitData = {
        id: this.projectTagSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateProjectTag(projectTagSubmitData);
      } else {
        this.addProjectTag(projectTagSubmitData);
      }
  }

  private addProjectTag(projectTagData: ProjectTagSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    projectTagData.active = true;
    projectTagData.deleted = false;
    this.projectTagService.PostProjectTag(projectTagData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newProjectTag) => {

        this.projectTagService.ClearAllCaches();

        this.projectTagChanged.next([newProjectTag]);

        this.alertService.showMessage("Project Tag added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/projecttag', newProjectTag.id]);
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
                                   'You do not have permission to save this Project Tag.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Project Tag.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Project Tag could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateProjectTag(projectTagData: ProjectTagSubmitData) {
    this.projectTagService.PutProjectTag(projectTagData.id, projectTagData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedProjectTag) => {

        this.projectTagService.ClearAllCaches();

        this.projectTagChanged.next([updatedProjectTag]);

        this.alertService.showMessage("Project Tag updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Project Tag.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Project Tag.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Project Tag could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(projectTagData: ProjectTagData | null) {

    if (projectTagData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.projectTagForm.reset({
        name: '',
        description: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.projectTagForm.reset({
        name: projectTagData.name ?? '',
        description: projectTagData.description ?? '',
        active: projectTagData.active ?? true,
        deleted: projectTagData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.projectTagForm.markAsPristine();
    this.projectTagForm.markAsUntouched();
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


  public userIsBMCProjectTagReader(): boolean {
    return this.projectTagService.userIsBMCProjectTagReader();
  }

  public userIsBMCProjectTagWriter(): boolean {
    return this.projectTagService.userIsBMCProjectTagWriter();
  }
}
