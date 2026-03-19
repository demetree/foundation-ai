/*
   GENERATED FORM FOR THE DOCUMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Document table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to document-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { DocumentService, DocumentData, DocumentSubmitData } from '../../../scheduler-data-services/document.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { DocumentTypeService } from '../../../scheduler-data-services/document-type.service';
import { DocumentFolderService } from '../../../scheduler-data-services/document-folder.service';
import { InvoiceService } from '../../../scheduler-data-services/invoice.service';
import { ReceiptService } from '../../../scheduler-data-services/receipt.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { FinancialTransactionService } from '../../../scheduler-data-services/financial-transaction.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { CrewService } from '../../../scheduler-data-services/crew.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { PaymentTransactionService } from '../../../scheduler-data-services/payment-transaction.service';
import { FinancialOfficeService } from '../../../scheduler-data-services/financial-office.service';
import { TenantProfileService } from '../../../scheduler-data-services/tenant-profile.service';
import { CampaignService } from '../../../scheduler-data-services/campaign.service';
import { HouseholdService } from '../../../scheduler-data-services/household.service';
import { ConstituentService } from '../../../scheduler-data-services/constituent.service';
import { TributeService } from '../../../scheduler-data-services/tribute.service';
import { VolunteerProfileService } from '../../../scheduler-data-services/volunteer-profile.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface DocumentFormValues {
  documentTypeId: number | bigint | null,       // For FK link number
  documentFolderId: number | bigint | null,       // For FK link number
  name: string,
  description: string | null,
  fileName: string,
  mimeType: string,
  fileSizeBytes: string,     // Stored as string for form input, converted to number on submit.
  fileDataFileName: string | null,
  fileDataSize: string | null,     // Stored as string for form input, converted to number on submit.
  fileDataData: string | null,
  fileDataMimeType: string | null,
  invoiceId: number | bigint | null,       // For FK link number
  receiptId: number | bigint | null,       // For FK link number
  scheduledEventId: number | bigint | null,       // For FK link number
  financialTransactionId: number | bigint | null,       // For FK link number
  contactId: number | bigint | null,       // For FK link number
  resourceId: number | bigint | null,       // For FK link number
  clientId: number | bigint | null,       // For FK link number
  officeId: number | bigint | null,       // For FK link number
  crewId: number | bigint | null,       // For FK link number
  schedulingTargetId: number | bigint | null,       // For FK link number
  paymentTransactionId: number | bigint | null,       // For FK link number
  financialOfficeId: number | bigint | null,       // For FK link number
  tenantProfileId: number | bigint | null,       // For FK link number
  campaignId: number | bigint | null,       // For FK link number
  householdId: number | bigint | null,       // For FK link number
  constituentId: number | bigint | null,       // For FK link number
  tributeId: number | bigint | null,       // For FK link number
  volunteerProfileId: number | bigint | null,       // For FK link number
  status: string | null,
  statusDate: string | null,
  statusChangedBy: string | null,
  uploadedDate: string,
  uploadedBy: string | null,
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-document-add-edit',
  templateUrl: './document-add-edit.component.html',
  styleUrls: ['./document-add-edit.component.scss']
})
export class DocumentAddEditComponent {
  @ViewChild('documentModal') documentModal!: TemplateRef<any>;
  @Output() documentChanged = new Subject<DocumentData[]>();
  @Input() documentSubmitData: DocumentSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<DocumentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public documentForm: FormGroup = this.fb.group({
        documentTypeId: [null],
        documentFolderId: [null],
        name: ['', Validators.required],
        description: [''],
        fileName: ['', Validators.required],
        mimeType: ['', Validators.required],
        fileSizeBytes: ['', Validators.required],
        fileDataFileName: [''],
        fileDataSize: [''],
        fileDataData: [''],
        fileDataMimeType: [''],
        invoiceId: [null],
        receiptId: [null],
        scheduledEventId: [null],
        financialTransactionId: [null],
        contactId: [null],
        resourceId: [null],
        clientId: [null],
        officeId: [null],
        crewId: [null],
        schedulingTargetId: [null],
        paymentTransactionId: [null],
        financialOfficeId: [null],
        tenantProfileId: [null],
        campaignId: [null],
        householdId: [null],
        constituentId: [null],
        tributeId: [null],
        volunteerProfileId: [null],
        status: [''],
        statusDate: [''],
        statusChangedBy: [''],
        uploadedDate: ['', Validators.required],
        uploadedBy: [''],
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

  documents$ = this.documentService.GetDocumentList();
  documentTypes$ = this.documentTypeService.GetDocumentTypeList();
  documentFolders$ = this.documentFolderService.GetDocumentFolderList();
  invoices$ = this.invoiceService.GetInvoiceList();
  receipts$ = this.receiptService.GetReceiptList();
  scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  financialTransactions$ = this.financialTransactionService.GetFinancialTransactionList();
  contacts$ = this.contactService.GetContactList();
  resources$ = this.resourceService.GetResourceList();
  clients$ = this.clientService.GetClientList();
  offices$ = this.officeService.GetOfficeList();
  crews$ = this.crewService.GetCrewList();
  schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  paymentTransactions$ = this.paymentTransactionService.GetPaymentTransactionList();
  financialOffices$ = this.financialOfficeService.GetFinancialOfficeList();
  tenantProfiles$ = this.tenantProfileService.GetTenantProfileList();
  campaigns$ = this.campaignService.GetCampaignList();
  households$ = this.householdService.GetHouseholdList();
  constituents$ = this.constituentService.GetConstituentList();
  tributes$ = this.tributeService.GetTributeList();
  volunteerProfiles$ = this.volunteerProfileService.GetVolunteerProfileList();

  constructor(
    private modalService: NgbModal,
    private documentService: DocumentService,
    private documentTypeService: DocumentTypeService,
    private documentFolderService: DocumentFolderService,
    private invoiceService: InvoiceService,
    private receiptService: ReceiptService,
    private scheduledEventService: ScheduledEventService,
    private financialTransactionService: FinancialTransactionService,
    private contactService: ContactService,
    private resourceService: ResourceService,
    private clientService: ClientService,
    private officeService: OfficeService,
    private crewService: CrewService,
    private schedulingTargetService: SchedulingTargetService,
    private paymentTransactionService: PaymentTransactionService,
    private financialOfficeService: FinancialOfficeService,
    private tenantProfileService: TenantProfileService,
    private campaignService: CampaignService,
    private householdService: HouseholdService,
    private constituentService: ConstituentService,
    private tributeService: TributeService,
    private volunteerProfileService: VolunteerProfileService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(documentData?: DocumentData) {

    if (documentData != null) {

      if (!this.documentService.userIsSchedulerDocumentReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Documents`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.documentSubmitData = this.documentService.ConvertToDocumentSubmitData(documentData);
      this.isEditMode = true;
      this.objectGuid = documentData.objectGuid;

      this.buildFormValues(documentData);

    } else {

      if (!this.documentService.userIsSchedulerDocumentWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Documents`,
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
        this.documentForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.documentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.documentModal, {
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

    if (this.documentService.userIsSchedulerDocumentWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Documents`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.documentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.documentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.documentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const documentSubmitData: DocumentSubmitData = {
        id: this.documentSubmitData?.id || 0,
        documentTypeId: formValue.documentTypeId ? Number(formValue.documentTypeId) : null,
        documentFolderId: formValue.documentFolderId ? Number(formValue.documentFolderId) : null,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        fileName: formValue.fileName!.trim(),
        mimeType: formValue.mimeType!.trim(),
        fileSizeBytes: Number(formValue.fileSizeBytes),
        fileDataFileName: formValue.fileDataFileName?.trim() || null,
        fileDataSize: formValue.fileDataSize ? Number(formValue.fileDataSize) : null,
        fileDataData: formValue.fileDataData?.trim() || null,
        fileDataMimeType: formValue.fileDataMimeType?.trim() || null,
        invoiceId: formValue.invoiceId ? Number(formValue.invoiceId) : null,
        receiptId: formValue.receiptId ? Number(formValue.receiptId) : null,
        scheduledEventId: formValue.scheduledEventId ? Number(formValue.scheduledEventId) : null,
        financialTransactionId: formValue.financialTransactionId ? Number(formValue.financialTransactionId) : null,
        contactId: formValue.contactId ? Number(formValue.contactId) : null,
        resourceId: formValue.resourceId ? Number(formValue.resourceId) : null,
        clientId: formValue.clientId ? Number(formValue.clientId) : null,
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        crewId: formValue.crewId ? Number(formValue.crewId) : null,
        schedulingTargetId: formValue.schedulingTargetId ? Number(formValue.schedulingTargetId) : null,
        paymentTransactionId: formValue.paymentTransactionId ? Number(formValue.paymentTransactionId) : null,
        financialOfficeId: formValue.financialOfficeId ? Number(formValue.financialOfficeId) : null,
        tenantProfileId: formValue.tenantProfileId ? Number(formValue.tenantProfileId) : null,
        campaignId: formValue.campaignId ? Number(formValue.campaignId) : null,
        householdId: formValue.householdId ? Number(formValue.householdId) : null,
        constituentId: formValue.constituentId ? Number(formValue.constituentId) : null,
        tributeId: formValue.tributeId ? Number(formValue.tributeId) : null,
        volunteerProfileId: formValue.volunteerProfileId ? Number(formValue.volunteerProfileId) : null,
        status: formValue.status?.trim() || null,
        statusDate: formValue.statusDate ? dateTimeLocalToIsoUtc(formValue.statusDate.trim()) : null,
        statusChangedBy: formValue.statusChangedBy?.trim() || null,
        uploadedDate: dateTimeLocalToIsoUtc(formValue.uploadedDate!.trim())!,
        uploadedBy: formValue.uploadedBy?.trim() || null,
        notes: formValue.notes?.trim() || null,
        versionNumber: this.documentSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateDocument(documentSubmitData);
      } else {
        this.addDocument(documentSubmitData);
      }
  }

  private addDocument(documentData: DocumentSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    documentData.versionNumber = 0;
    documentData.active = true;
    documentData.deleted = false;
    this.documentService.PostDocument(documentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newDocument) => {

        this.documentService.ClearAllCaches();

        this.documentChanged.next([newDocument]);

        this.alertService.showMessage("Document added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/document', newDocument.id]);
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
                                   'You do not have permission to save this Document.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Document.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Document could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateDocument(documentData: DocumentSubmitData) {
    this.documentService.PutDocument(documentData.id, documentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedDocument) => {

        this.documentService.ClearAllCaches();

        this.documentChanged.next([updatedDocument]);

        this.alertService.showMessage("Document updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Document.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Document.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Document could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(documentData: DocumentData | null) {

    if (documentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.documentForm.reset({
        documentTypeId: null,
        documentFolderId: null,
        name: '',
        description: '',
        fileName: '',
        mimeType: '',
        fileSizeBytes: '',
        fileDataFileName: '',
        fileDataSize: '',
        fileDataData: '',
        fileDataMimeType: '',
        invoiceId: null,
        receiptId: null,
        scheduledEventId: null,
        financialTransactionId: null,
        contactId: null,
        resourceId: null,
        clientId: null,
        officeId: null,
        crewId: null,
        schedulingTargetId: null,
        paymentTransactionId: null,
        financialOfficeId: null,
        tenantProfileId: null,
        campaignId: null,
        householdId: null,
        constituentId: null,
        tributeId: null,
        volunteerProfileId: null,
        status: '',
        statusDate: '',
        statusChangedBy: '',
        uploadedDate: '',
        uploadedBy: '',
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
        this.documentForm.reset({
        documentTypeId: documentData.documentTypeId,
        documentFolderId: documentData.documentFolderId,
        name: documentData.name ?? '',
        description: documentData.description ?? '',
        fileName: documentData.fileName ?? '',
        mimeType: documentData.mimeType ?? '',
        fileSizeBytes: documentData.fileSizeBytes?.toString() ?? '',
        fileDataFileName: documentData.fileDataFileName ?? '',
        fileDataSize: documentData.fileDataSize?.toString() ?? '',
        fileDataData: documentData.fileDataData ?? '',
        fileDataMimeType: documentData.fileDataMimeType ?? '',
        invoiceId: documentData.invoiceId,
        receiptId: documentData.receiptId,
        scheduledEventId: documentData.scheduledEventId,
        financialTransactionId: documentData.financialTransactionId,
        contactId: documentData.contactId,
        resourceId: documentData.resourceId,
        clientId: documentData.clientId,
        officeId: documentData.officeId,
        crewId: documentData.crewId,
        schedulingTargetId: documentData.schedulingTargetId,
        paymentTransactionId: documentData.paymentTransactionId,
        financialOfficeId: documentData.financialOfficeId,
        tenantProfileId: documentData.tenantProfileId,
        campaignId: documentData.campaignId,
        householdId: documentData.householdId,
        constituentId: documentData.constituentId,
        tributeId: documentData.tributeId,
        volunteerProfileId: documentData.volunteerProfileId,
        status: documentData.status ?? '',
        statusDate: isoUtcStringToDateTimeLocal(documentData.statusDate) ?? '',
        statusChangedBy: documentData.statusChangedBy ?? '',
        uploadedDate: isoUtcStringToDateTimeLocal(documentData.uploadedDate) ?? '',
        uploadedBy: documentData.uploadedBy ?? '',
        notes: documentData.notes ?? '',
        versionNumber: documentData.versionNumber?.toString() ?? '',
        active: documentData.active ?? true,
        deleted: documentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.documentForm.markAsPristine();
    this.documentForm.markAsUntouched();
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


  public userIsSchedulerDocumentReader(): boolean {
    return this.documentService.userIsSchedulerDocumentReader();
  }

  public userIsSchedulerDocumentWriter(): boolean {
    return this.documentService.userIsSchedulerDocumentWriter();
  }
}
