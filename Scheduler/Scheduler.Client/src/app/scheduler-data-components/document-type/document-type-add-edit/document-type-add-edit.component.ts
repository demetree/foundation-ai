/*
   GENERATED FORM FOR THE DOCUMENTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from DocumentType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to document-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { DocumentTypeService, DocumentTypeData, DocumentTypeSubmitData } from '../../../scheduler-data-services/document-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface DocumentTypeFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  color: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-document-type-add-edit',
  templateUrl: './document-type-add-edit.component.html',
  styleUrls: ['./document-type-add-edit.component.scss']
})
export class DocumentTypeAddEditComponent {
  @ViewChild('documentTypeModal') documentTypeModal!: TemplateRef<any>;
  @Output() documentTypeChanged = new Subject<DocumentTypeData[]>();
  @Input() documentTypeSubmitData: DocumentTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<DocumentTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public documentTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        color: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  documentTypes$ = this.documentTypeService.GetDocumentTypeList();

  constructor(
    private modalService: NgbModal,
    private documentTypeService: DocumentTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(documentTypeData?: DocumentTypeData) {

    if (documentTypeData != null) {

      if (!this.documentTypeService.userIsSchedulerDocumentTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Document Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.documentTypeSubmitData = this.documentTypeService.ConvertToDocumentTypeSubmitData(documentTypeData);
      this.isEditMode = true;
      this.objectGuid = documentTypeData.objectGuid;

      this.buildFormValues(documentTypeData);

    } else {

      if (!this.documentTypeService.userIsSchedulerDocumentTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Document Types`,
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
        this.documentTypeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.documentTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.documentTypeModal, {
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

    if (this.documentTypeService.userIsSchedulerDocumentTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Document Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.documentTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.documentTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.documentTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const documentTypeSubmitData: DocumentTypeSubmitData = {
        id: this.documentTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateDocumentType(documentTypeSubmitData);
      } else {
        this.addDocumentType(documentTypeSubmitData);
      }
  }

  private addDocumentType(documentTypeData: DocumentTypeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    documentTypeData.active = true;
    documentTypeData.deleted = false;
    this.documentTypeService.PostDocumentType(documentTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newDocumentType) => {

        this.documentTypeService.ClearAllCaches();

        this.documentTypeChanged.next([newDocumentType]);

        this.alertService.showMessage("Document Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/documenttype', newDocumentType.id]);
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
                                   'You do not have permission to save this Document Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Document Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Document Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateDocumentType(documentTypeData: DocumentTypeSubmitData) {
    this.documentTypeService.PutDocumentType(documentTypeData.id, documentTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedDocumentType) => {

        this.documentTypeService.ClearAllCaches();

        this.documentTypeChanged.next([updatedDocumentType]);

        this.alertService.showMessage("Document Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Document Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Document Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Document Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(documentTypeData: DocumentTypeData | null) {

    if (documentTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.documentTypeForm.reset({
        name: '',
        description: '',
        sequence: '',
        color: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.documentTypeForm.reset({
        name: documentTypeData.name ?? '',
        description: documentTypeData.description ?? '',
        sequence: documentTypeData.sequence?.toString() ?? '',
        color: documentTypeData.color ?? '',
        active: documentTypeData.active ?? true,
        deleted: documentTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.documentTypeForm.markAsPristine();
    this.documentTypeForm.markAsUntouched();
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


  public userIsSchedulerDocumentTypeReader(): boolean {
    return this.documentTypeService.userIsSchedulerDocumentTypeReader();
  }

  public userIsSchedulerDocumentTypeWriter(): boolean {
    return this.documentTypeService.userIsSchedulerDocumentTypeWriter();
  }
}
