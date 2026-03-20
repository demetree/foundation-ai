/*
   GENERATED FORM FOR THE DOCUMENTSHARELINK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from DocumentShareLink table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to document-share-link-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { DocumentShareLinkService, DocumentShareLinkData, DocumentShareLinkSubmitData } from '../../../scheduler-data-services/document-share-link.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { DocumentService } from '../../../scheduler-data-services/document.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface DocumentShareLinkFormValues {
  documentId: number | bigint,       // For FK link number
  token: string,
  passwordHash: string | null,
  expiresAt: string | null,
  maxDownloads: string | null,     // Stored as string for form input, converted to number on submit.
  downloadCount: string,     // Stored as string for form input, converted to number on submit.
  createdBy: string,
  createdDate: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-document-share-link-add-edit',
  templateUrl: './document-share-link-add-edit.component.html',
  styleUrls: ['./document-share-link-add-edit.component.scss']
})
export class DocumentShareLinkAddEditComponent {
  @ViewChild('documentShareLinkModal') documentShareLinkModal!: TemplateRef<any>;
  @Output() documentShareLinkChanged = new Subject<DocumentShareLinkData[]>();
  @Input() documentShareLinkSubmitData: DocumentShareLinkSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<DocumentShareLinkFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public documentShareLinkForm: FormGroup = this.fb.group({
        documentId: [null, Validators.required],
        token: ['', Validators.required],
        passwordHash: [''],
        expiresAt: [''],
        maxDownloads: [''],
        downloadCount: ['', Validators.required],
        createdBy: ['', Validators.required],
        createdDate: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  documentShareLinks$ = this.documentShareLinkService.GetDocumentShareLinkList();
  documents$ = this.documentService.GetDocumentList();

  constructor(
    private modalService: NgbModal,
    private documentShareLinkService: DocumentShareLinkService,
    private documentService: DocumentService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(documentShareLinkData?: DocumentShareLinkData) {

    if (documentShareLinkData != null) {

      if (!this.documentShareLinkService.userIsSchedulerDocumentShareLinkReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Document Share Links`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.documentShareLinkSubmitData = this.documentShareLinkService.ConvertToDocumentShareLinkSubmitData(documentShareLinkData);
      this.isEditMode = true;
      this.objectGuid = documentShareLinkData.objectGuid;

      this.buildFormValues(documentShareLinkData);

    } else {

      if (!this.documentShareLinkService.userIsSchedulerDocumentShareLinkWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Document Share Links`,
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
        this.documentShareLinkForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.documentShareLinkForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.documentShareLinkModal, {
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

    if (this.documentShareLinkService.userIsSchedulerDocumentShareLinkWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Document Share Links`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.documentShareLinkForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.documentShareLinkForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.documentShareLinkForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const documentShareLinkSubmitData: DocumentShareLinkSubmitData = {
        id: this.documentShareLinkSubmitData?.id || 0,
        documentId: Number(formValue.documentId),
        token: formValue.token!.trim(),
        passwordHash: formValue.passwordHash?.trim() || null,
        expiresAt: formValue.expiresAt ? dateTimeLocalToIsoUtc(formValue.expiresAt.trim()) : null,
        maxDownloads: formValue.maxDownloads ? Number(formValue.maxDownloads) : null,
        downloadCount: Number(formValue.downloadCount),
        createdBy: formValue.createdBy!.trim(),
        createdDate: dateTimeLocalToIsoUtc(formValue.createdDate!.trim())!,
        versionNumber: this.documentShareLinkSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateDocumentShareLink(documentShareLinkSubmitData);
      } else {
        this.addDocumentShareLink(documentShareLinkSubmitData);
      }
  }

  private addDocumentShareLink(documentShareLinkData: DocumentShareLinkSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    documentShareLinkData.versionNumber = 0;
    documentShareLinkData.active = true;
    documentShareLinkData.deleted = false;
    this.documentShareLinkService.PostDocumentShareLink(documentShareLinkData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newDocumentShareLink) => {

        this.documentShareLinkService.ClearAllCaches();

        this.documentShareLinkChanged.next([newDocumentShareLink]);

        this.alertService.showMessage("Document Share Link added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/documentsharelink', newDocumentShareLink.id]);
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
                                   'You do not have permission to save this Document Share Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Document Share Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Document Share Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateDocumentShareLink(documentShareLinkData: DocumentShareLinkSubmitData) {
    this.documentShareLinkService.PutDocumentShareLink(documentShareLinkData.id, documentShareLinkData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedDocumentShareLink) => {

        this.documentShareLinkService.ClearAllCaches();

        this.documentShareLinkChanged.next([updatedDocumentShareLink]);

        this.alertService.showMessage("Document Share Link updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Document Share Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Document Share Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Document Share Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(documentShareLinkData: DocumentShareLinkData | null) {

    if (documentShareLinkData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.documentShareLinkForm.reset({
        documentId: null,
        token: '',
        passwordHash: '',
        expiresAt: '',
        maxDownloads: '',
        downloadCount: '',
        createdBy: '',
        createdDate: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.documentShareLinkForm.reset({
        documentId: documentShareLinkData.documentId,
        token: documentShareLinkData.token ?? '',
        passwordHash: documentShareLinkData.passwordHash ?? '',
        expiresAt: isoUtcStringToDateTimeLocal(documentShareLinkData.expiresAt) ?? '',
        maxDownloads: documentShareLinkData.maxDownloads?.toString() ?? '',
        downloadCount: documentShareLinkData.downloadCount?.toString() ?? '',
        createdBy: documentShareLinkData.createdBy ?? '',
        createdDate: isoUtcStringToDateTimeLocal(documentShareLinkData.createdDate) ?? '',
        versionNumber: documentShareLinkData.versionNumber?.toString() ?? '',
        active: documentShareLinkData.active ?? true,
        deleted: documentShareLinkData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.documentShareLinkForm.markAsPristine();
    this.documentShareLinkForm.markAsUntouched();
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


  public userIsSchedulerDocumentShareLinkReader(): boolean {
    return this.documentShareLinkService.userIsSchedulerDocumentShareLinkReader();
  }

  public userIsSchedulerDocumentShareLinkWriter(): boolean {
    return this.documentShareLinkService.userIsSchedulerDocumentShareLinkWriter();
  }
}
