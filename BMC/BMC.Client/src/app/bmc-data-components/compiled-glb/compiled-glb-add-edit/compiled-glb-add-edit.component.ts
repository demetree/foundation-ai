/*
   GENERATED FORM FOR THE COMPILEDGLB TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from CompiledGlb table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to compiled-glb-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CompiledGlbService, CompiledGlbData, CompiledGlbSubmitData } from '../../../bmc-data-services/compiled-glb.service';
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
interface CompiledGlbFormValues {
  projectId: number | bigint,       // For FK link number
  projectVersionNumber: string,     // Stored as string for form input, converted to number on submit.
  includesEdgeLines: boolean,
  glbData: string | null,
  glbSizeBytes: string,     // Stored as string for form input, converted to number on submit.
  triangleCount: string | null,     // Stored as string for form input, converted to number on submit.
  stepCount: string | null,     // Stored as string for form input, converted to number on submit.
  compiledAt: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-compiled-glb-add-edit',
  templateUrl: './compiled-glb-add-edit.component.html',
  styleUrls: ['./compiled-glb-add-edit.component.scss']
})
export class CompiledGlbAddEditComponent {
  @ViewChild('compiledGlbModal') compiledGlbModal!: TemplateRef<any>;
  @Output() compiledGlbChanged = new Subject<CompiledGlbData[]>();
  @Input() compiledGlbSubmitData: CompiledGlbSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CompiledGlbFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public compiledGlbForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        projectVersionNumber: ['', Validators.required],
        includesEdgeLines: [false],
        glbData: [''],
        glbSizeBytes: ['', Validators.required],
        triangleCount: [''],
        stepCount: [''],
        compiledAt: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  compiledGlbs$ = this.compiledGlbService.GetCompiledGlbList();
  projects$ = this.projectService.GetProjectList();

  constructor(
    private modalService: NgbModal,
    private compiledGlbService: CompiledGlbService,
    private projectService: ProjectService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(compiledGlbData?: CompiledGlbData) {

    if (compiledGlbData != null) {

      if (!this.compiledGlbService.userIsBMCCompiledGlbReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Compiled Glbs`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.compiledGlbSubmitData = this.compiledGlbService.ConvertToCompiledGlbSubmitData(compiledGlbData);
      this.isEditMode = true;
      this.objectGuid = compiledGlbData.objectGuid;

      this.buildFormValues(compiledGlbData);

    } else {

      if (!this.compiledGlbService.userIsBMCCompiledGlbWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Compiled Glbs`,
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
        this.compiledGlbForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.compiledGlbForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.compiledGlbModal, {
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

    if (this.compiledGlbService.userIsBMCCompiledGlbWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Compiled Glbs`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.compiledGlbForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.compiledGlbForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.compiledGlbForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const compiledGlbSubmitData: CompiledGlbSubmitData = {
        id: this.compiledGlbSubmitData?.id || 0,
        projectId: Number(formValue.projectId),
        projectVersionNumber: Number(formValue.projectVersionNumber),
        includesEdgeLines: !!formValue.includesEdgeLines,
        glbData: formValue.glbData?.trim() || null,
        glbSizeBytes: Number(formValue.glbSizeBytes),
        triangleCount: formValue.triangleCount ? Number(formValue.triangleCount) : null,
        stepCount: formValue.stepCount ? Number(formValue.stepCount) : null,
        compiledAt: dateTimeLocalToIsoUtc(formValue.compiledAt!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateCompiledGlb(compiledGlbSubmitData);
      } else {
        this.addCompiledGlb(compiledGlbSubmitData);
      }
  }

  private addCompiledGlb(compiledGlbData: CompiledGlbSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    compiledGlbData.active = true;
    compiledGlbData.deleted = false;
    this.compiledGlbService.PostCompiledGlb(compiledGlbData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newCompiledGlb) => {

        this.compiledGlbService.ClearAllCaches();

        this.compiledGlbChanged.next([newCompiledGlb]);

        this.alertService.showMessage("Compiled Glb added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/compiledglb', newCompiledGlb.id]);
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
                                   'You do not have permission to save this Compiled Glb.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Compiled Glb.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Compiled Glb could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateCompiledGlb(compiledGlbData: CompiledGlbSubmitData) {
    this.compiledGlbService.PutCompiledGlb(compiledGlbData.id, compiledGlbData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedCompiledGlb) => {

        this.compiledGlbService.ClearAllCaches();

        this.compiledGlbChanged.next([updatedCompiledGlb]);

        this.alertService.showMessage("Compiled Glb updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Compiled Glb.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Compiled Glb.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Compiled Glb could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(compiledGlbData: CompiledGlbData | null) {

    if (compiledGlbData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.compiledGlbForm.reset({
        projectId: null,
        projectVersionNumber: '',
        includesEdgeLines: false,
        glbData: '',
        glbSizeBytes: '',
        triangleCount: '',
        stepCount: '',
        compiledAt: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.compiledGlbForm.reset({
        projectId: compiledGlbData.projectId,
        projectVersionNumber: compiledGlbData.projectVersionNumber?.toString() ?? '',
        includesEdgeLines: compiledGlbData.includesEdgeLines ?? false,
        glbData: compiledGlbData.glbData ?? '',
        glbSizeBytes: compiledGlbData.glbSizeBytes?.toString() ?? '',
        triangleCount: compiledGlbData.triangleCount?.toString() ?? '',
        stepCount: compiledGlbData.stepCount?.toString() ?? '',
        compiledAt: isoUtcStringToDateTimeLocal(compiledGlbData.compiledAt) ?? '',
        active: compiledGlbData.active ?? true,
        deleted: compiledGlbData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.compiledGlbForm.markAsPristine();
    this.compiledGlbForm.markAsUntouched();
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


  public userIsBMCCompiledGlbReader(): boolean {
    return this.compiledGlbService.userIsBMCCompiledGlbReader();
  }

  public userIsBMCCompiledGlbWriter(): boolean {
    return this.compiledGlbService.userIsBMCCompiledGlbWriter();
  }
}
