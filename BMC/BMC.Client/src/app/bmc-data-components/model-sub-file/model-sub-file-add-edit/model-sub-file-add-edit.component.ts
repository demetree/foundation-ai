/*
   GENERATED FORM FOR THE MODELSUBFILE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ModelSubFile table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to model-sub-file-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ModelSubFileService, ModelSubFileData, ModelSubFileSubmitData } from '../../../bmc-data-services/model-sub-file.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ModelDocumentService } from '../../../bmc-data-services/model-document.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ModelSubFileFormValues {
  modelDocumentId: number | bigint,       // For FK link number
  fileName: string,
  isMainModel: boolean,
  parentModelSubFileId: number | bigint | null,       // For FK link number
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-model-sub-file-add-edit',
  templateUrl: './model-sub-file-add-edit.component.html',
  styleUrls: ['./model-sub-file-add-edit.component.scss']
})
export class ModelSubFileAddEditComponent {
  @ViewChild('modelSubFileModal') modelSubFileModal!: TemplateRef<any>;
  @Output() modelSubFileChanged = new Subject<ModelSubFileData[]>();
  @Input() modelSubFileSubmitData: ModelSubFileSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ModelSubFileFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public modelSubFileForm: FormGroup = this.fb.group({
        modelDocumentId: [null, Validators.required],
        fileName: ['', Validators.required],
        isMainModel: [false],
        parentModelSubFileId: [null],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  modelSubFiles$ = this.modelSubFileService.GetModelSubFileList();
  modelDocuments$ = this.modelDocumentService.GetModelDocumentList();

  constructor(
    private modalService: NgbModal,
    private modelSubFileService: ModelSubFileService,
    private modelDocumentService: ModelDocumentService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(modelSubFileData?: ModelSubFileData) {

    if (modelSubFileData != null) {

      if (!this.modelSubFileService.userIsBMCModelSubFileReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Model Sub Files`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.modelSubFileSubmitData = this.modelSubFileService.ConvertToModelSubFileSubmitData(modelSubFileData);
      this.isEditMode = true;
      this.objectGuid = modelSubFileData.objectGuid;

      this.buildFormValues(modelSubFileData);

    } else {

      if (!this.modelSubFileService.userIsBMCModelSubFileWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Model Sub Files`,
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
        this.modelSubFileForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.modelSubFileForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.modelSubFileModal, {
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

    if (this.modelSubFileService.userIsBMCModelSubFileWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Model Sub Files`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.modelSubFileForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.modelSubFileForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.modelSubFileForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const modelSubFileSubmitData: ModelSubFileSubmitData = {
        id: this.modelSubFileSubmitData?.id || 0,
        modelDocumentId: Number(formValue.modelDocumentId),
        fileName: formValue.fileName!.trim(),
        isMainModel: !!formValue.isMainModel,
        parentModelSubFileId: formValue.parentModelSubFileId ? Number(formValue.parentModelSubFileId) : null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateModelSubFile(modelSubFileSubmitData);
      } else {
        this.addModelSubFile(modelSubFileSubmitData);
      }
  }

  private addModelSubFile(modelSubFileData: ModelSubFileSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    modelSubFileData.active = true;
    modelSubFileData.deleted = false;
    this.modelSubFileService.PostModelSubFile(modelSubFileData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newModelSubFile) => {

        this.modelSubFileService.ClearAllCaches();

        this.modelSubFileChanged.next([newModelSubFile]);

        this.alertService.showMessage("Model Sub File added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/modelsubfile', newModelSubFile.id]);
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
                                   'You do not have permission to save this Model Sub File.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Model Sub File.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Model Sub File could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateModelSubFile(modelSubFileData: ModelSubFileSubmitData) {
    this.modelSubFileService.PutModelSubFile(modelSubFileData.id, modelSubFileData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedModelSubFile) => {

        this.modelSubFileService.ClearAllCaches();

        this.modelSubFileChanged.next([updatedModelSubFile]);

        this.alertService.showMessage("Model Sub File updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Model Sub File.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Model Sub File.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Model Sub File could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(modelSubFileData: ModelSubFileData | null) {

    if (modelSubFileData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.modelSubFileForm.reset({
        modelDocumentId: null,
        fileName: '',
        isMainModel: false,
        parentModelSubFileId: null,
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.modelSubFileForm.reset({
        modelDocumentId: modelSubFileData.modelDocumentId,
        fileName: modelSubFileData.fileName ?? '',
        isMainModel: modelSubFileData.isMainModel ?? false,
        parentModelSubFileId: modelSubFileData.parentModelSubFileId,
        sequence: modelSubFileData.sequence?.toString() ?? '',
        active: modelSubFileData.active ?? true,
        deleted: modelSubFileData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.modelSubFileForm.markAsPristine();
    this.modelSubFileForm.markAsUntouched();
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


  public userIsBMCModelSubFileReader(): boolean {
    return this.modelSubFileService.userIsBMCModelSubFileReader();
  }

  public userIsBMCModelSubFileWriter(): boolean {
    return this.modelSubFileService.userIsBMCModelSubFileWriter();
  }
}
