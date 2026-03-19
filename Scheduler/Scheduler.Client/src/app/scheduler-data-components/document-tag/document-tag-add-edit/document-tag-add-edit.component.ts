/*
   GENERATED FORM FOR THE DOCUMENTTAG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from DocumentTag table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to document-tag-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { DocumentTagService, DocumentTagData, DocumentTagSubmitData } from '../../../scheduler-data-services/document-tag.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface DocumentTagFormValues {
  name: string,
  description: string | null,
  color: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-document-tag-add-edit',
  templateUrl: './document-tag-add-edit.component.html',
  styleUrls: ['./document-tag-add-edit.component.scss']
})
export class DocumentTagAddEditComponent {
  @ViewChild('documentTagModal') documentTagModal!: TemplateRef<any>;
  @Output() documentTagChanged = new Subject<DocumentTagData[]>();
  @Input() documentTagSubmitData: DocumentTagSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<DocumentTagFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public documentTagForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        color: [''],
        sequence: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  documentTags$ = this.documentTagService.GetDocumentTagList();

  constructor(
    private modalService: NgbModal,
    private documentTagService: DocumentTagService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(documentTagData?: DocumentTagData) {

    if (documentTagData != null) {

      if (!this.documentTagService.userIsSchedulerDocumentTagReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Document Tags`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.documentTagSubmitData = this.documentTagService.ConvertToDocumentTagSubmitData(documentTagData);
      this.isEditMode = true;
      this.objectGuid = documentTagData.objectGuid;

      this.buildFormValues(documentTagData);

    } else {

      if (!this.documentTagService.userIsSchedulerDocumentTagWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Document Tags`,
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
        this.documentTagForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.documentTagForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.documentTagModal, {
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

    if (this.documentTagService.userIsSchedulerDocumentTagWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Document Tags`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.documentTagForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.documentTagForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.documentTagForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const documentTagSubmitData: DocumentTagSubmitData = {
        id: this.documentTagSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        color: formValue.color?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        versionNumber: this.documentTagSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateDocumentTag(documentTagSubmitData);
      } else {
        this.addDocumentTag(documentTagSubmitData);
      }
  }

  private addDocumentTag(documentTagData: DocumentTagSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    documentTagData.versionNumber = 0;
    documentTagData.active = true;
    documentTagData.deleted = false;
    this.documentTagService.PostDocumentTag(documentTagData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newDocumentTag) => {

        this.documentTagService.ClearAllCaches();

        this.documentTagChanged.next([newDocumentTag]);

        this.alertService.showMessage("Document Tag added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/documenttag', newDocumentTag.id]);
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
                                   'You do not have permission to save this Document Tag.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Document Tag.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Document Tag could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateDocumentTag(documentTagData: DocumentTagSubmitData) {
    this.documentTagService.PutDocumentTag(documentTagData.id, documentTagData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedDocumentTag) => {

        this.documentTagService.ClearAllCaches();

        this.documentTagChanged.next([updatedDocumentTag]);

        this.alertService.showMessage("Document Tag updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Document Tag.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Document Tag.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Document Tag could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(documentTagData: DocumentTagData | null) {

    if (documentTagData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.documentTagForm.reset({
        name: '',
        description: '',
        color: '',
        sequence: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.documentTagForm.reset({
        name: documentTagData.name ?? '',
        description: documentTagData.description ?? '',
        color: documentTagData.color ?? '',
        sequence: documentTagData.sequence?.toString() ?? '',
        versionNumber: documentTagData.versionNumber?.toString() ?? '',
        active: documentTagData.active ?? true,
        deleted: documentTagData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.documentTagForm.markAsPristine();
    this.documentTagForm.markAsUntouched();
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


  public userIsSchedulerDocumentTagReader(): boolean {
    return this.documentTagService.userIsSchedulerDocumentTagReader();
  }

  public userIsSchedulerDocumentTagWriter(): boolean {
    return this.documentTagService.userIsSchedulerDocumentTagWriter();
  }
}
