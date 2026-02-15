/*
   GENERATED FORM FOR THE BUILDMANUAL TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildManual table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-manual-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildManualService, BuildManualData, BuildManualSubmitData } from '../../../bmc-data-services/build-manual.service';
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
interface BuildManualFormValues {
  projectId: number | bigint,       // For FK link number
  name: string,
  description: string,
  pageWidthMm: string | null,     // Stored as string for form input, converted to number on submit.
  pageHeightMm: string | null,     // Stored as string for form input, converted to number on submit.
  isPublished: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-build-manual-add-edit',
  templateUrl: './build-manual-add-edit.component.html',
  styleUrls: ['./build-manual-add-edit.component.scss']
})
export class BuildManualAddEditComponent {
  @ViewChild('buildManualModal') buildManualModal!: TemplateRef<any>;
  @Output() buildManualChanged = new Subject<BuildManualData[]>();
  @Input() buildManualSubmitData: BuildManualSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildManualFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildManualForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        name: ['', Validators.required],
        description: ['', Validators.required],
        pageWidthMm: [''],
        pageHeightMm: [''],
        isPublished: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  buildManuals$ = this.buildManualService.GetBuildManualList();
  projects$ = this.projectService.GetProjectList();

  constructor(
    private modalService: NgbModal,
    private buildManualService: BuildManualService,
    private projectService: ProjectService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(buildManualData?: BuildManualData) {

    if (buildManualData != null) {

      if (!this.buildManualService.userIsBMCBuildManualReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Build Manuals`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.buildManualSubmitData = this.buildManualService.ConvertToBuildManualSubmitData(buildManualData);
      this.isEditMode = true;
      this.objectGuid = buildManualData.objectGuid;

      this.buildFormValues(buildManualData);

    } else {

      if (!this.buildManualService.userIsBMCBuildManualWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Build Manuals`,
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
        this.buildManualForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildManualForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.buildManualModal, {
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

    if (this.buildManualService.userIsBMCBuildManualWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Build Manuals`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.buildManualForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildManualForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildManualForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildManualSubmitData: BuildManualSubmitData = {
        id: this.buildManualSubmitData?.id || 0,
        projectId: Number(formValue.projectId),
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        pageWidthMm: formValue.pageWidthMm ? Number(formValue.pageWidthMm) : null,
        pageHeightMm: formValue.pageHeightMm ? Number(formValue.pageHeightMm) : null,
        isPublished: !!formValue.isPublished,
        versionNumber: this.buildManualSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBuildManual(buildManualSubmitData);
      } else {
        this.addBuildManual(buildManualSubmitData);
      }
  }

  private addBuildManual(buildManualData: BuildManualSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    buildManualData.versionNumber = 0;
    buildManualData.active = true;
    buildManualData.deleted = false;
    this.buildManualService.PostBuildManual(buildManualData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBuildManual) => {

        this.buildManualService.ClearAllCaches();

        this.buildManualChanged.next([newBuildManual]);

        this.alertService.showMessage("Build Manual added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/buildmanual', newBuildManual.id]);
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
                                   'You do not have permission to save this Build Manual.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Manual.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Manual could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBuildManual(buildManualData: BuildManualSubmitData) {
    this.buildManualService.PutBuildManual(buildManualData.id, buildManualData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBuildManual) => {

        this.buildManualService.ClearAllCaches();

        this.buildManualChanged.next([updatedBuildManual]);

        this.alertService.showMessage("Build Manual updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Build Manual.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Manual.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Manual could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(buildManualData: BuildManualData | null) {

    if (buildManualData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildManualForm.reset({
        projectId: null,
        name: '',
        description: '',
        pageWidthMm: '',
        pageHeightMm: '',
        isPublished: false,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.buildManualForm.reset({
        projectId: buildManualData.projectId,
        name: buildManualData.name ?? '',
        description: buildManualData.description ?? '',
        pageWidthMm: buildManualData.pageWidthMm?.toString() ?? '',
        pageHeightMm: buildManualData.pageHeightMm?.toString() ?? '',
        isPublished: buildManualData.isPublished ?? false,
        versionNumber: buildManualData.versionNumber?.toString() ?? '',
        active: buildManualData.active ?? true,
        deleted: buildManualData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.buildManualForm.markAsPristine();
    this.buildManualForm.markAsUntouched();
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


  public userIsBMCBuildManualReader(): boolean {
    return this.buildManualService.userIsBMCBuildManualReader();
  }

  public userIsBMCBuildManualWriter(): boolean {
    return this.buildManualService.userIsBMCBuildManualWriter();
  }
}
