/*
   GENERATED FORM FOR THE DOCUMENTFOLDER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from DocumentFolder table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to document-folder-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { DocumentFolderService, DocumentFolderData, DocumentFolderSubmitData } from '../../../scheduler-data-services/document-folder.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface DocumentFolderFormValues {
  name: string,
  description: string | null,
  parentDocumentFolderId: number | bigint | null,       // For FK link number
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  sequence: string,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-document-folder-add-edit',
  templateUrl: './document-folder-add-edit.component.html',
  styleUrls: ['./document-folder-add-edit.component.scss']
})
export class DocumentFolderAddEditComponent {
  @ViewChild('documentFolderModal') documentFolderModal!: TemplateRef<any>;
  @Output() documentFolderChanged = new Subject<DocumentFolderData[]>();
  @Input() documentFolderSubmitData: DocumentFolderSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<DocumentFolderFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public documentFolderForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        parentDocumentFolderId: [null],
        iconId: [null],
        color: [''],
        sequence: ['', Validators.required],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  documentFolders$ = this.documentFolderService.GetDocumentFolderList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private documentFolderService: DocumentFolderService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(documentFolderData?: DocumentFolderData) {

    if (documentFolderData != null) {

      if (!this.documentFolderService.userIsSchedulerDocumentFolderReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Document Folders`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.documentFolderSubmitData = this.documentFolderService.ConvertToDocumentFolderSubmitData(documentFolderData);
      this.isEditMode = true;
      this.objectGuid = documentFolderData.objectGuid;

      this.buildFormValues(documentFolderData);

    } else {

      if (!this.documentFolderService.userIsSchedulerDocumentFolderWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Document Folders`,
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
        this.documentFolderForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.documentFolderForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.documentFolderModal, {
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

    if (this.documentFolderService.userIsSchedulerDocumentFolderWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Document Folders`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.documentFolderForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.documentFolderForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.documentFolderForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const documentFolderSubmitData: DocumentFolderSubmitData = {
        id: this.documentFolderSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        parentDocumentFolderId: formValue.parentDocumentFolderId ? Number(formValue.parentDocumentFolderId) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        sequence: Number(formValue.sequence),
        notes: formValue.notes?.trim() || null,
        versionNumber: this.documentFolderSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateDocumentFolder(documentFolderSubmitData);
      } else {
        this.addDocumentFolder(documentFolderSubmitData);
      }
  }

  private addDocumentFolder(documentFolderData: DocumentFolderSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    documentFolderData.versionNumber = 0;
    documentFolderData.active = true;
    documentFolderData.deleted = false;
    this.documentFolderService.PostDocumentFolder(documentFolderData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newDocumentFolder) => {

        this.documentFolderService.ClearAllCaches();

        this.documentFolderChanged.next([newDocumentFolder]);

        this.alertService.showMessage("Document Folder added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/documentfolder', newDocumentFolder.id]);
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
                                   'You do not have permission to save this Document Folder.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Document Folder.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Document Folder could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateDocumentFolder(documentFolderData: DocumentFolderSubmitData) {
    this.documentFolderService.PutDocumentFolder(documentFolderData.id, documentFolderData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedDocumentFolder) => {

        this.documentFolderService.ClearAllCaches();

        this.documentFolderChanged.next([updatedDocumentFolder]);

        this.alertService.showMessage("Document Folder updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Document Folder.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Document Folder.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Document Folder could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(documentFolderData: DocumentFolderData | null) {

    if (documentFolderData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.documentFolderForm.reset({
        name: '',
        description: '',
        parentDocumentFolderId: null,
        iconId: null,
        color: '',
        sequence: '',
        notes: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.documentFolderForm.reset({
        name: documentFolderData.name ?? '',
        description: documentFolderData.description ?? '',
        parentDocumentFolderId: documentFolderData.parentDocumentFolderId,
        iconId: documentFolderData.iconId,
        color: documentFolderData.color ?? '',
        sequence: documentFolderData.sequence?.toString() ?? '',
        notes: documentFolderData.notes ?? '',
        versionNumber: documentFolderData.versionNumber?.toString() ?? '',
        active: documentFolderData.active ?? true,
        deleted: documentFolderData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.documentFolderForm.markAsPristine();
    this.documentFolderForm.markAsUntouched();
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


  public userIsSchedulerDocumentFolderReader(): boolean {
    return this.documentFolderService.userIsSchedulerDocumentFolderReader();
  }

  public userIsSchedulerDocumentFolderWriter(): boolean {
    return this.documentFolderService.userIsSchedulerDocumentFolderWriter();
  }
}
