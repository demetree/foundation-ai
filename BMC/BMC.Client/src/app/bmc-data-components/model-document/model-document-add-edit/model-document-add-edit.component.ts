/*
   GENERATED FORM FOR THE MODELDOCUMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ModelDocument table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to model-document-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ModelDocumentService, ModelDocumentData, ModelDocumentSubmitData } from '../../../bmc-data-services/model-document.service';
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
interface ModelDocumentFormValues {
  projectId: number | bigint | null,       // For FK link number
  name: string,
  description: string | null,
  sourceFormat: string,
  sourceFileName: string | null,
  sourceFileFileName: string | null,
  sourceFileSize: string | null,     // Stored as string for form input, converted to number on submit.
  sourceFileData: string | null,
  sourceFileMimeType: string | null,
  author: string | null,
  totalPartCount: string | null,     // Stored as string for form input, converted to number on submit.
  totalStepCount: string | null,     // Stored as string for form input, converted to number on submit.
  studioVersion: string | null,
  instructionSettingsXml: string | null,
  errorPartList: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-model-document-add-edit',
  templateUrl: './model-document-add-edit.component.html',
  styleUrls: ['./model-document-add-edit.component.scss']
})
export class ModelDocumentAddEditComponent {
  @ViewChild('modelDocumentModal') modelDocumentModal!: TemplateRef<any>;
  @Output() modelDocumentChanged = new Subject<ModelDocumentData[]>();
  @Input() modelDocumentSubmitData: ModelDocumentSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ModelDocumentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public modelDocumentForm: FormGroup = this.fb.group({
        projectId: [null],
        name: ['', Validators.required],
        description: [''],
        sourceFormat: ['', Validators.required],
        sourceFileName: [''],
        sourceFileFileName: [''],
        sourceFileSize: [''],
        sourceFileData: [''],
        sourceFileMimeType: [''],
        author: [''],
        totalPartCount: [''],
        totalStepCount: [''],
        studioVersion: [''],
        instructionSettingsXml: [''],
        errorPartList: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  modelDocuments$ = this.modelDocumentService.GetModelDocumentList();
  projects$ = this.projectService.GetProjectList();

  constructor(
    private modalService: NgbModal,
    private modelDocumentService: ModelDocumentService,
    private projectService: ProjectService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(modelDocumentData?: ModelDocumentData) {

    if (modelDocumentData != null) {

      if (!this.modelDocumentService.userIsBMCModelDocumentReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Model Documents`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.modelDocumentSubmitData = this.modelDocumentService.ConvertToModelDocumentSubmitData(modelDocumentData);
      this.isEditMode = true;
      this.objectGuid = modelDocumentData.objectGuid;

      this.buildFormValues(modelDocumentData);

    } else {

      if (!this.modelDocumentService.userIsBMCModelDocumentWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Model Documents`,
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
        this.modelDocumentForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.modelDocumentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.modelDocumentModal, {
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

    if (this.modelDocumentService.userIsBMCModelDocumentWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Model Documents`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.modelDocumentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.modelDocumentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.modelDocumentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const modelDocumentSubmitData: ModelDocumentSubmitData = {
        id: this.modelDocumentSubmitData?.id || 0,
        projectId: formValue.projectId ? Number(formValue.projectId) : null,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        sourceFormat: formValue.sourceFormat!.trim(),
        sourceFileName: formValue.sourceFileName?.trim() || null,
        sourceFileFileName: formValue.sourceFileFileName?.trim() || null,
        sourceFileSize: formValue.sourceFileSize ? Number(formValue.sourceFileSize) : null,
        sourceFileData: formValue.sourceFileData?.trim() || null,
        sourceFileMimeType: formValue.sourceFileMimeType?.trim() || null,
        author: formValue.author?.trim() || null,
        totalPartCount: formValue.totalPartCount ? Number(formValue.totalPartCount) : null,
        totalStepCount: formValue.totalStepCount ? Number(formValue.totalStepCount) : null,
        studioVersion: formValue.studioVersion?.trim() || null,
        instructionSettingsXml: formValue.instructionSettingsXml?.trim() || null,
        errorPartList: formValue.errorPartList?.trim() || null,
        versionNumber: this.modelDocumentSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateModelDocument(modelDocumentSubmitData);
      } else {
        this.addModelDocument(modelDocumentSubmitData);
      }
  }

  private addModelDocument(modelDocumentData: ModelDocumentSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    modelDocumentData.versionNumber = 0;
    modelDocumentData.active = true;
    modelDocumentData.deleted = false;
    this.modelDocumentService.PostModelDocument(modelDocumentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newModelDocument) => {

        this.modelDocumentService.ClearAllCaches();

        this.modelDocumentChanged.next([newModelDocument]);

        this.alertService.showMessage("Model Document added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/modeldocument', newModelDocument.id]);
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
                                   'You do not have permission to save this Model Document.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Model Document.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Model Document could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateModelDocument(modelDocumentData: ModelDocumentSubmitData) {
    this.modelDocumentService.PutModelDocument(modelDocumentData.id, modelDocumentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedModelDocument) => {

        this.modelDocumentService.ClearAllCaches();

        this.modelDocumentChanged.next([updatedModelDocument]);

        this.alertService.showMessage("Model Document updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Model Document.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Model Document.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Model Document could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(modelDocumentData: ModelDocumentData | null) {

    if (modelDocumentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.modelDocumentForm.reset({
        projectId: null,
        name: '',
        description: '',
        sourceFormat: '',
        sourceFileName: '',
        sourceFileFileName: '',
        sourceFileSize: '',
        sourceFileData: '',
        sourceFileMimeType: '',
        author: '',
        totalPartCount: '',
        totalStepCount: '',
        studioVersion: '',
        instructionSettingsXml: '',
        errorPartList: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.modelDocumentForm.reset({
        projectId: modelDocumentData.projectId,
        name: modelDocumentData.name ?? '',
        description: modelDocumentData.description ?? '',
        sourceFormat: modelDocumentData.sourceFormat ?? '',
        sourceFileName: modelDocumentData.sourceFileName ?? '',
        sourceFileFileName: modelDocumentData.sourceFileFileName ?? '',
        sourceFileSize: modelDocumentData.sourceFileSize?.toString() ?? '',
        sourceFileData: modelDocumentData.sourceFileData ?? '',
        sourceFileMimeType: modelDocumentData.sourceFileMimeType ?? '',
        author: modelDocumentData.author ?? '',
        totalPartCount: modelDocumentData.totalPartCount?.toString() ?? '',
        totalStepCount: modelDocumentData.totalStepCount?.toString() ?? '',
        studioVersion: modelDocumentData.studioVersion ?? '',
        instructionSettingsXml: modelDocumentData.instructionSettingsXml ?? '',
        errorPartList: modelDocumentData.errorPartList ?? '',
        versionNumber: modelDocumentData.versionNumber?.toString() ?? '',
        active: modelDocumentData.active ?? true,
        deleted: modelDocumentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.modelDocumentForm.markAsPristine();
    this.modelDocumentForm.markAsUntouched();
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


  public userIsBMCModelDocumentReader(): boolean {
    return this.modelDocumentService.userIsBMCModelDocumentReader();
  }

  public userIsBMCModelDocumentWriter(): boolean {
    return this.modelDocumentService.userIsBMCModelDocumentWriter();
  }
}
