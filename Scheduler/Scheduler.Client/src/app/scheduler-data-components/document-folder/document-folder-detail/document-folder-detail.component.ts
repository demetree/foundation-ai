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
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { DocumentFolderService, DocumentFolderData, DocumentFolderSubmitData } from '../../../scheduler-data-services/document-folder.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { DocumentFolderChangeHistoryService } from '../../../scheduler-data-services/document-folder-change-history.service';
import { DocumentService } from '../../../scheduler-data-services/document.service';
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
  selector: 'app-document-folder-detail',
  templateUrl: './document-folder-detail.component.html',
  styleUrls: ['./document-folder-detail.component.scss']
})

export class DocumentFolderDetailComponent implements OnInit, CanComponentDeactivate {


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


  public documentFolderId: string | null = null;
  public documentFolderData: DocumentFolderData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  documentFolders$ = this.documentFolderService.GetDocumentFolderList();
  public icons$ = this.iconService.GetIconList();
  public documentFolderChangeHistories$ = this.documentFolderChangeHistoryService.GetDocumentFolderChangeHistoryList();
  public documents$ = this.documentService.GetDocumentList();

  private destroy$ = new Subject<void>();

  constructor(
    public documentFolderService: DocumentFolderService,
    public iconService: IconService,
    public documentFolderChangeHistoryService: DocumentFolderChangeHistoryService,
    public documentService: DocumentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the documentFolderId from the route parameters
    this.documentFolderId = this.route.snapshot.paramMap.get('documentFolderId');

    if (this.documentFolderId === 'new' ||
        this.documentFolderId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.documentFolderData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.documentFolderForm.patchValue(this.preSeededData);
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


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Document Folder';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Document Folder';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.documentFolderForm.dirty) {
      return confirm('You have unsaved Document Folder changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.documentFolderId != null && this.documentFolderId !== 'new') {

      const id = parseInt(this.documentFolderId, 10);

      if (!isNaN(id)) {
        return { documentFolderId: id };
      }
    }

    return null;
  }


/*
  * Loads the DocumentFolder data for the current documentFolderId.
  *
  * Fully respects the DocumentFolderService caching strategy and error handling strategy.
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
    if (!this.documentFolderService.userIsSchedulerDocumentFolderReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read DocumentFolders.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate documentFolderId
    //
    if (!this.documentFolderId) {

      this.alertService.showMessage('No DocumentFolder ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const documentFolderId = Number(this.documentFolderId);

    if (isNaN(documentFolderId) || documentFolderId <= 0) {

      this.alertService.showMessage(`Invalid Document Folder ID: "${this.documentFolderId}"`,
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
      // This is the most targeted way: clear only this DocumentFolder + relations

      this.documentFolderService.ClearRecordCache(documentFolderId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.documentFolderService.GetDocumentFolder(documentFolderId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (documentFolderData) => {

        //
        // Success path — documentFolderData can legitimately be null if 404'd but request succeeded
        //
        if (!documentFolderData) {

          this.handleDocumentFolderNotFound(documentFolderId);

        } else {

          this.documentFolderData = documentFolderData;
          this.buildFormValues(this.documentFolderData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'DocumentFolder loaded successfully',
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
        this.handleDocumentFolderLoadError(error, documentFolderId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleDocumentFolderNotFound(documentFolderId: number): void {

    this.documentFolderData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `DocumentFolder #${documentFolderId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleDocumentFolderLoadError(error: any, documentFolderId: number): void {

    let message = 'Failed to load Document Folder.';
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
          message = 'You do not have permission to view this Document Folder.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Document Folder #${documentFolderId} was not found.`;
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

    console.error(`Document Folder load failed (ID: ${documentFolderId})`, error);

    //
    // Reset UI to safe state
    //
    this.documentFolderData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
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

    if (this.documentFolderService.userIsSchedulerDocumentFolderWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Document Folders", 'Access Denied', MessageSeverity.info);
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
        id: this.documentFolderData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        parentDocumentFolderId: formValue.parentDocumentFolderId ? Number(formValue.parentDocumentFolderId) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        sequence: Number(formValue.sequence),
        notes: formValue.notes?.trim() || null,
        versionNumber: this.documentFolderData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.documentFolderService.PutDocumentFolder(documentFolderSubmitData.id, documentFolderSubmitData)
      : this.documentFolderService.PostDocumentFolder(documentFolderSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedDocumentFolderData) => {

        this.documentFolderService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Document Folder's detail page
          //
          this.documentFolderForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.documentFolderForm.markAsUntouched();

          this.router.navigate(['/documentfolders', savedDocumentFolderData.id]);
          this.alertService.showMessage('Document Folder added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.documentFolderData = savedDocumentFolderData;
          this.buildFormValues(this.documentFolderData);

          this.alertService.showMessage("Document Folder saved successfully", '', MessageSeverity.success);
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

  public userIsSchedulerDocumentFolderReader(): boolean {
    return this.documentFolderService.userIsSchedulerDocumentFolderReader();
  }

  public userIsSchedulerDocumentFolderWriter(): boolean {
    return this.documentFolderService.userIsSchedulerDocumentFolderWriter();
  }
}
