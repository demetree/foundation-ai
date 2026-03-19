/*
   GENERATED FORM FOR THE DOCUMENTDOCUMENTTAG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from DocumentDocumentTag table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to document-document-tag-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { DocumentDocumentTagService, DocumentDocumentTagData, DocumentDocumentTagSubmitData } from '../../../scheduler-data-services/document-document-tag.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { DocumentService } from '../../../scheduler-data-services/document.service';
import { DocumentTagService } from '../../../scheduler-data-services/document-tag.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface DocumentDocumentTagFormValues {
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
  documentId: number | bigint,       // For FK link number
  documentTagId: number | bigint,       // For FK link number
};

@Component({
  selector: 'app-document-document-tag-add-edit',
  templateUrl: './document-document-tag-add-edit.component.html',
  styleUrls: ['./document-document-tag-add-edit.component.scss']
})
export class DocumentDocumentTagAddEditComponent {
  @ViewChild('documentDocumentTagModal') documentDocumentTagModal!: TemplateRef<any>;
  @Output() documentDocumentTagChanged = new Subject<DocumentDocumentTagData[]>();
  @Input() documentDocumentTagSubmitData: DocumentDocumentTagSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<DocumentDocumentTagFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public documentDocumentTagForm: FormGroup = this.fb.group({
        versionNumber: [''],
        active: [true],
        deleted: [false],
        documentId: [null, Validators.required],
        documentTagId: [null, Validators.required],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  documentDocumentTags$ = this.documentDocumentTagService.GetDocumentDocumentTagList();
  documents$ = this.documentService.GetDocumentList();
  documentTags$ = this.documentTagService.GetDocumentTagList();

  constructor(
    private modalService: NgbModal,
    private documentDocumentTagService: DocumentDocumentTagService,
    private documentService: DocumentService,
    private documentTagService: DocumentTagService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(documentDocumentTagData?: DocumentDocumentTagData) {

    if (documentDocumentTagData != null) {

      if (!this.documentDocumentTagService.userIsSchedulerDocumentDocumentTagReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Document Document Tags`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.documentDocumentTagSubmitData = this.documentDocumentTagService.ConvertToDocumentDocumentTagSubmitData(documentDocumentTagData);
      this.isEditMode = true;
      this.objectGuid = documentDocumentTagData.objectGuid;

      this.buildFormValues(documentDocumentTagData);

    } else {

      if (!this.documentDocumentTagService.userIsSchedulerDocumentDocumentTagWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Document Document Tags`,
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
        this.documentDocumentTagForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.documentDocumentTagForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.documentDocumentTagModal, {
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

    if (this.documentDocumentTagService.userIsSchedulerDocumentDocumentTagWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Document Document Tags`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.documentDocumentTagForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.documentDocumentTagForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.documentDocumentTagForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const documentDocumentTagSubmitData: DocumentDocumentTagSubmitData = {
        id: this.documentDocumentTagSubmitData?.id || 0,
        versionNumber: this.documentDocumentTagSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
        documentId: Number(formValue.documentId),
        documentTagId: Number(formValue.documentTagId),
   };

      if (this.isEditMode) {
        this.updateDocumentDocumentTag(documentDocumentTagSubmitData);
      } else {
        this.addDocumentDocumentTag(documentDocumentTagSubmitData);
      }
  }

  private addDocumentDocumentTag(documentDocumentTagData: DocumentDocumentTagSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    documentDocumentTagData.versionNumber = 0;
    documentDocumentTagData.active = true;
    documentDocumentTagData.deleted = false;
    this.documentDocumentTagService.PostDocumentDocumentTag(documentDocumentTagData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newDocumentDocumentTag) => {

        this.documentDocumentTagService.ClearAllCaches();

        this.documentDocumentTagChanged.next([newDocumentDocumentTag]);

        this.alertService.showMessage("Document Document Tag added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/documentdocumenttag', newDocumentDocumentTag.id]);
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
                                   'You do not have permission to save this Document Document Tag.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Document Document Tag.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Document Document Tag could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateDocumentDocumentTag(documentDocumentTagData: DocumentDocumentTagSubmitData) {
    this.documentDocumentTagService.PutDocumentDocumentTag(documentDocumentTagData.id, documentDocumentTagData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedDocumentDocumentTag) => {

        this.documentDocumentTagService.ClearAllCaches();

        this.documentDocumentTagChanged.next([updatedDocumentDocumentTag]);

        this.alertService.showMessage("Document Document Tag updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Document Document Tag.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Document Document Tag.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Document Document Tag could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(documentDocumentTagData: DocumentDocumentTagData | null) {

    if (documentDocumentTagData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.documentDocumentTagForm.reset({
        versionNumber: '',
        active: true,
        deleted: false,
        documentId: null,
        documentTagId: null,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.documentDocumentTagForm.reset({
        versionNumber: documentDocumentTagData.versionNumber?.toString() ?? '',
        active: documentDocumentTagData.active ?? true,
        deleted: documentDocumentTagData.deleted ?? false,
        documentId: documentDocumentTagData.documentId,
        documentTagId: documentDocumentTagData.documentTagId,
      }, { emitEvent: false});
    }

    this.documentDocumentTagForm.markAsPristine();
    this.documentDocumentTagForm.markAsUntouched();
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


  public userIsSchedulerDocumentDocumentTagReader(): boolean {
    return this.documentDocumentTagService.userIsSchedulerDocumentDocumentTagReader();
  }

  public userIsSchedulerDocumentDocumentTagWriter(): boolean {
    return this.documentDocumentTagService.userIsSchedulerDocumentDocumentTagWriter();
  }
}
