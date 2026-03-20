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
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { DocumentService, DocumentData, DocumentSubmitData } from '../../../scheduler-data-services/document.service';
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
import { DocumentChangeHistoryService } from '../../../scheduler-data-services/document-change-history.service';
import { DocumentDocumentTagService } from '../../../scheduler-data-services/document-document-tag.service';
import { DocumentShareLinkService } from '../../../scheduler-data-services/document-share-link.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
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
  selector: 'app-document-detail',
  templateUrl: './document-detail.component.html',
  styleUrls: ['./document-detail.component.scss']
})

export class DocumentDetailComponent implements OnInit, CanComponentDeactivate {


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


  public documentId: string | null = null;
  public documentData: DocumentData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  documents$ = this.documentService.GetDocumentList();
  public documentTypes$ = this.documentTypeService.GetDocumentTypeList();
  public documentFolders$ = this.documentFolderService.GetDocumentFolderList();
  public invoices$ = this.invoiceService.GetInvoiceList();
  public receipts$ = this.receiptService.GetReceiptList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public financialTransactions$ = this.financialTransactionService.GetFinancialTransactionList();
  public contacts$ = this.contactService.GetContactList();
  public resources$ = this.resourceService.GetResourceList();
  public clients$ = this.clientService.GetClientList();
  public offices$ = this.officeService.GetOfficeList();
  public crews$ = this.crewService.GetCrewList();
  public schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  public paymentTransactions$ = this.paymentTransactionService.GetPaymentTransactionList();
  public financialOffices$ = this.financialOfficeService.GetFinancialOfficeList();
  public tenantProfiles$ = this.tenantProfileService.GetTenantProfileList();
  public campaigns$ = this.campaignService.GetCampaignList();
  public households$ = this.householdService.GetHouseholdList();
  public constituents$ = this.constituentService.GetConstituentList();
  public tributes$ = this.tributeService.GetTributeList();
  public volunteerProfiles$ = this.volunteerProfileService.GetVolunteerProfileList();
  public documentChangeHistories$ = this.documentChangeHistoryService.GetDocumentChangeHistoryList();
  public documentDocumentTags$ = this.documentDocumentTagService.GetDocumentDocumentTagList();
  public documentShareLinks$ = this.documentShareLinkService.GetDocumentShareLinkList();

  private destroy$ = new Subject<void>();

  constructor(
    public documentService: DocumentService,
    public documentTypeService: DocumentTypeService,
    public documentFolderService: DocumentFolderService,
    public invoiceService: InvoiceService,
    public receiptService: ReceiptService,
    public scheduledEventService: ScheduledEventService,
    public financialTransactionService: FinancialTransactionService,
    public contactService: ContactService,
    public resourceService: ResourceService,
    public clientService: ClientService,
    public officeService: OfficeService,
    public crewService: CrewService,
    public schedulingTargetService: SchedulingTargetService,
    public paymentTransactionService: PaymentTransactionService,
    public financialOfficeService: FinancialOfficeService,
    public tenantProfileService: TenantProfileService,
    public campaignService: CampaignService,
    public householdService: HouseholdService,
    public constituentService: ConstituentService,
    public tributeService: TributeService,
    public volunteerProfileService: VolunteerProfileService,
    public documentChangeHistoryService: DocumentChangeHistoryService,
    public documentDocumentTagService: DocumentDocumentTagService,
    public documentShareLinkService: DocumentShareLinkService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the documentId from the route parameters
    this.documentId = this.route.snapshot.paramMap.get('documentId');

    if (this.documentId === 'new' ||
        this.documentId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.documentData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.documentForm.patchValue(this.preSeededData);
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


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Document';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Document';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.documentForm.dirty) {
      return confirm('You have unsaved Document changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.documentId != null && this.documentId !== 'new') {

      const id = parseInt(this.documentId, 10);

      if (!isNaN(id)) {
        return { documentId: id };
      }
    }

    return null;
  }


/*
  * Loads the Document data for the current documentId.
  *
  * Fully respects the DocumentService caching strategy and error handling strategy.
  *
  * @param forceLoadAndDisplaySuccessAlert
  *   - true  will bypass cache entirely and show success alert message
  *   - false/null will use cache if available, no alert message
  */
  public loadData(forceLoadAndDisplaySuccessAlert: boolean | null = null): void {

    //
    // Start loading indicator immediately
    //
    this.isLoadingSubject.next(true);


    //
    // Permission Check
    //
    if (!this.documentService.userIsSchedulerDocumentReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Documents.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate documentId
    //
    if (!this.documentId) {

      this.alertService.showMessage('No Document ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const documentId = Number(this.documentId);

    if (isNaN(documentId) || documentId <= 0) {

      this.alertService.showMessage(`Invalid Document ID: "${this.documentId}"`,
                                    'Invalid ID',
                                    MessageSeverity.error
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Force refresh: clear specific record cache only
    //
    if (forceLoadAndDisplaySuccessAlert === true) {
      // This is the most targeted way: clear only this Document + relations

      this.documentService.ClearRecordCache(documentId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.documentService.GetDocument(documentId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (documentData) => {

        //
        // Success path — documentData can legitimately be null if 404'd but request succeeded
        //
        if (!documentData) {

          this.handleDocumentNotFound(documentId);

        } else {

          this.documentData = documentData;
          this.buildFormValues(this.documentData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Document loaded successfully',
              '',
              MessageSeverity.success
            );
          }
        }

        this.isLoadingSubject.next(false);
      },

      error: (error: any) => {
        //
        // All HTTP/network/parsing errors flow here
        // The service already stripped sensitive info and re-threw cleanly
        //
        this.handleDocumentLoadError(error, documentId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleDocumentNotFound(documentId: number): void {

    this.documentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Document #${documentId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleDocumentLoadError(error: any, documentId: number): void {

    let message = 'Failed to load Document.';
    let title = 'Load Error';
    let severity = MessageSeverity.error;

    //
    // Leverage HTTP status if available
    //
    if (error?.status) {
      switch (error.status) {
        case 401:
          message = 'Your session has expired. Please log in again.';
          title = 'Unauthorized';
          break;
        case 403:
          message = 'You do not have permission to view this Document.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Document #${documentId} was not found.`;
          title = 'Not Found';
          severity = MessageSeverity.warn;
          break;
        case 500:
          message = 'Server error. Please try again or contact support.';
          title = 'Server Error';
          break;
        case 0:
          message = 'Cannot reach server. Check your internet connection.';
          title = 'Offline';
          break;
        default:
          message = `Server error ${error.status || 'unknown'}: ${error.statusText || 'Request failed'}`;
      }
    } else {
      message = error?.message || message;
    }

    console.error(`Document load failed (ID: ${documentId})`, error);

    //
    // Reset UI to safe state
    //
    this.documentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
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

  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
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


  public submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.documentService.userIsSchedulerDocumentWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Documents", 'Access Denied', MessageSeverity.info);
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
        id: this.documentData?.id || 0,
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
        versionNumber: this.documentData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.documentService.PutDocument(documentSubmitData.id, documentSubmitData)
      : this.documentService.PostDocument(documentSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedDocumentData) => {

        this.documentService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Document's detail page
          //
          this.documentForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.documentForm.markAsUntouched();

          this.router.navigate(['/documents', savedDocumentData.id]);
          this.alertService.showMessage('Document added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.documentData = savedDocumentData;
          this.buildFormValues(this.documentData);

          this.alertService.showMessage("Document saved successfully", '', MessageSeverity.success);
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

  public userIsSchedulerDocumentReader(): boolean {
    return this.documentService.userIsSchedulerDocumentReader();
  }

  public userIsSchedulerDocumentWriter(): boolean {
    return this.documentService.userIsSchedulerDocumentWriter();
  }
}
