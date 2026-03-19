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
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { DocumentDocumentTagService, DocumentDocumentTagData, DocumentDocumentTagSubmitData } from '../../../scheduler-data-services/document-document-tag.service';
import { DocumentService } from '../../../scheduler-data-services/document.service';
import { DocumentTagService } from '../../../scheduler-data-services/document-tag.service';
import { DocumentDocumentTagChangeHistoryService } from '../../../scheduler-data-services/document-document-tag-change-history.service';
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
interface DocumentDocumentTagFormValues {
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
  documentId: number | bigint,       // For FK link number
  documentTagId: number | bigint,       // For FK link number
};


@Component({
  selector: 'app-document-document-tag-detail',
  templateUrl: './document-document-tag-detail.component.html',
  styleUrls: ['./document-document-tag-detail.component.scss']
})

export class DocumentDocumentTagDetailComponent implements OnInit, CanComponentDeactivate {


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


  public documentDocumentTagId: string | null = null;
  public documentDocumentTagData: DocumentDocumentTagData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  documentDocumentTags$ = this.documentDocumentTagService.GetDocumentDocumentTagList();
  public documents$ = this.documentService.GetDocumentList();
  public documentTags$ = this.documentTagService.GetDocumentTagList();
  public documentDocumentTagChangeHistories$ = this.documentDocumentTagChangeHistoryService.GetDocumentDocumentTagChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public documentDocumentTagService: DocumentDocumentTagService,
    public documentService: DocumentService,
    public documentTagService: DocumentTagService,
    public documentDocumentTagChangeHistoryService: DocumentDocumentTagChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the documentDocumentTagId from the route parameters
    this.documentDocumentTagId = this.route.snapshot.paramMap.get('documentDocumentTagId');

    if (this.documentDocumentTagId === 'new' ||
        this.documentDocumentTagId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.documentDocumentTagData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.documentDocumentTagForm.patchValue(this.preSeededData);
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


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Document Document Tag';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Document Document Tag';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.documentDocumentTagForm.dirty) {
      return confirm('You have unsaved Document Document Tag changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.documentDocumentTagId != null && this.documentDocumentTagId !== 'new') {

      const id = parseInt(this.documentDocumentTagId, 10);

      if (!isNaN(id)) {
        return { documentDocumentTagId: id };
      }
    }

    return null;
  }


/*
  * Loads the DocumentDocumentTag data for the current documentDocumentTagId.
  *
  * Fully respects the DocumentDocumentTagService caching strategy and error handling strategy.
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
    if (!this.documentDocumentTagService.userIsSchedulerDocumentDocumentTagReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read DocumentDocumentTags.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate documentDocumentTagId
    //
    if (!this.documentDocumentTagId) {

      this.alertService.showMessage('No DocumentDocumentTag ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const documentDocumentTagId = Number(this.documentDocumentTagId);

    if (isNaN(documentDocumentTagId) || documentDocumentTagId <= 0) {

      this.alertService.showMessage(`Invalid Document Document Tag ID: "${this.documentDocumentTagId}"`,
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
      // This is the most targeted way: clear only this DocumentDocumentTag + relations

      this.documentDocumentTagService.ClearRecordCache(documentDocumentTagId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.documentDocumentTagService.GetDocumentDocumentTag(documentDocumentTagId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (documentDocumentTagData) => {

        //
        // Success path — documentDocumentTagData can legitimately be null if 404'd but request succeeded
        //
        if (!documentDocumentTagData) {

          this.handleDocumentDocumentTagNotFound(documentDocumentTagId);

        } else {

          this.documentDocumentTagData = documentDocumentTagData;
          this.buildFormValues(this.documentDocumentTagData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'DocumentDocumentTag loaded successfully',
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
        this.handleDocumentDocumentTagLoadError(error, documentDocumentTagId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleDocumentDocumentTagNotFound(documentDocumentTagId: number): void {

    this.documentDocumentTagData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `DocumentDocumentTag #${documentDocumentTagId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleDocumentDocumentTagLoadError(error: any, documentDocumentTagId: number): void {

    let message = 'Failed to load Document Document Tag.';
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
          message = 'You do not have permission to view this Document Document Tag.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Document Document Tag #${documentDocumentTagId} was not found.`;
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

    console.error(`Document Document Tag load failed (ID: ${documentDocumentTagId})`, error);

    //
    // Reset UI to safe state
    //
    this.documentDocumentTagData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
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

    if (this.documentDocumentTagService.userIsSchedulerDocumentDocumentTagWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Document Document Tags", 'Access Denied', MessageSeverity.info);
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
        id: this.documentDocumentTagData?.id || 0,
        versionNumber: this.documentDocumentTagData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
        documentId: Number(formValue.documentId),
        documentTagId: Number(formValue.documentTagId),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.documentDocumentTagService.PutDocumentDocumentTag(documentDocumentTagSubmitData.id, documentDocumentTagSubmitData)
      : this.documentDocumentTagService.PostDocumentDocumentTag(documentDocumentTagSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedDocumentDocumentTagData) => {

        this.documentDocumentTagService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Document Document Tag's detail page
          //
          this.documentDocumentTagForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.documentDocumentTagForm.markAsUntouched();

          this.router.navigate(['/documentdocumenttags', savedDocumentDocumentTagData.id]);
          this.alertService.showMessage('Document Document Tag added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.documentDocumentTagData = savedDocumentDocumentTagData;
          this.buildFormValues(this.documentDocumentTagData);

          this.alertService.showMessage("Document Document Tag saved successfully", '', MessageSeverity.success);
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

  public userIsSchedulerDocumentDocumentTagReader(): boolean {
    return this.documentDocumentTagService.userIsSchedulerDocumentDocumentTagReader();
  }

  public userIsSchedulerDocumentDocumentTagWriter(): boolean {
    return this.documentDocumentTagService.userIsSchedulerDocumentDocumentTagWriter();
  }
}
