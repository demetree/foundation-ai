/*
   GENERATED FORM FOR THE DOCUMENTDOCUMENTTAGCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from DocumentDocumentTagChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to document-document-tag-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { DocumentDocumentTagChangeHistoryService, DocumentDocumentTagChangeHistoryData, DocumentDocumentTagChangeHistorySubmitData } from '../../../scheduler-data-services/document-document-tag-change-history.service';
import { DocumentDocumentTagService } from '../../../scheduler-data-services/document-document-tag.service';
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
interface DocumentDocumentTagChangeHistoryFormValues {
  documentDocumentTagId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-document-document-tag-change-history-detail',
  templateUrl: './document-document-tag-change-history-detail.component.html',
  styleUrls: ['./document-document-tag-change-history-detail.component.scss']
})

export class DocumentDocumentTagChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<DocumentDocumentTagChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public documentDocumentTagChangeHistoryForm: FormGroup = this.fb.group({
        documentDocumentTagId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public documentDocumentTagChangeHistoryId: string | null = null;
  public documentDocumentTagChangeHistoryData: DocumentDocumentTagChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  documentDocumentTagChangeHistories$ = this.documentDocumentTagChangeHistoryService.GetDocumentDocumentTagChangeHistoryList();
  public documentDocumentTags$ = this.documentDocumentTagService.GetDocumentDocumentTagList();

  private destroy$ = new Subject<void>();

  constructor(
    public documentDocumentTagChangeHistoryService: DocumentDocumentTagChangeHistoryService,
    public documentDocumentTagService: DocumentDocumentTagService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the documentDocumentTagChangeHistoryId from the route parameters
    this.documentDocumentTagChangeHistoryId = this.route.snapshot.paramMap.get('documentDocumentTagChangeHistoryId');

    if (this.documentDocumentTagChangeHistoryId === 'new' ||
        this.documentDocumentTagChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.documentDocumentTagChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.documentDocumentTagChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.documentDocumentTagChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Document Document Tag Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Document Document Tag Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.documentDocumentTagChangeHistoryForm.dirty) {
      return confirm('You have unsaved Document Document Tag Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.documentDocumentTagChangeHistoryId != null && this.documentDocumentTagChangeHistoryId !== 'new') {

      const id = parseInt(this.documentDocumentTagChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { documentDocumentTagChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the DocumentDocumentTagChangeHistory data for the current documentDocumentTagChangeHistoryId.
  *
  * Fully respects the DocumentDocumentTagChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.documentDocumentTagChangeHistoryService.userIsSchedulerDocumentDocumentTagChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read DocumentDocumentTagChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate documentDocumentTagChangeHistoryId
    //
    if (!this.documentDocumentTagChangeHistoryId) {

      this.alertService.showMessage('No DocumentDocumentTagChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const documentDocumentTagChangeHistoryId = Number(this.documentDocumentTagChangeHistoryId);

    if (isNaN(documentDocumentTagChangeHistoryId) || documentDocumentTagChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Document Document Tag Change History ID: "${this.documentDocumentTagChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this DocumentDocumentTagChangeHistory + relations

      this.documentDocumentTagChangeHistoryService.ClearRecordCache(documentDocumentTagChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.documentDocumentTagChangeHistoryService.GetDocumentDocumentTagChangeHistory(documentDocumentTagChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (documentDocumentTagChangeHistoryData) => {

        //
        // Success path — documentDocumentTagChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!documentDocumentTagChangeHistoryData) {

          this.handleDocumentDocumentTagChangeHistoryNotFound(documentDocumentTagChangeHistoryId);

        } else {

          this.documentDocumentTagChangeHistoryData = documentDocumentTagChangeHistoryData;
          this.buildFormValues(this.documentDocumentTagChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'DocumentDocumentTagChangeHistory loaded successfully',
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
        this.handleDocumentDocumentTagChangeHistoryLoadError(error, documentDocumentTagChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleDocumentDocumentTagChangeHistoryNotFound(documentDocumentTagChangeHistoryId: number): void {

    this.documentDocumentTagChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `DocumentDocumentTagChangeHistory #${documentDocumentTagChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleDocumentDocumentTagChangeHistoryLoadError(error: any, documentDocumentTagChangeHistoryId: number): void {

    let message = 'Failed to load Document Document Tag Change History.';
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
          message = 'You do not have permission to view this Document Document Tag Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Document Document Tag Change History #${documentDocumentTagChangeHistoryId} was not found.`;
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

    console.error(`Document Document Tag Change History load failed (ID: ${documentDocumentTagChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.documentDocumentTagChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(documentDocumentTagChangeHistoryData: DocumentDocumentTagChangeHistoryData | null) {

    if (documentDocumentTagChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.documentDocumentTagChangeHistoryForm.reset({
        documentDocumentTagId: null,
        versionNumber: '',
        timeStamp: '',
        userId: '',
        data: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.documentDocumentTagChangeHistoryForm.reset({
        documentDocumentTagId: documentDocumentTagChangeHistoryData.documentDocumentTagId,
        versionNumber: documentDocumentTagChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(documentDocumentTagChangeHistoryData.timeStamp) ?? '',
        userId: documentDocumentTagChangeHistoryData.userId?.toString() ?? '',
        data: documentDocumentTagChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.documentDocumentTagChangeHistoryForm.markAsPristine();
    this.documentDocumentTagChangeHistoryForm.markAsUntouched();
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

    if (this.documentDocumentTagChangeHistoryService.userIsSchedulerDocumentDocumentTagChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Document Document Tag Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.documentDocumentTagChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.documentDocumentTagChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.documentDocumentTagChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const documentDocumentTagChangeHistorySubmitData: DocumentDocumentTagChangeHistorySubmitData = {
        id: this.documentDocumentTagChangeHistoryData?.id || 0,
        documentDocumentTagId: Number(formValue.documentDocumentTagId),
        versionNumber: this.documentDocumentTagChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.documentDocumentTagChangeHistoryService.PutDocumentDocumentTagChangeHistory(documentDocumentTagChangeHistorySubmitData.id, documentDocumentTagChangeHistorySubmitData)
      : this.documentDocumentTagChangeHistoryService.PostDocumentDocumentTagChangeHistory(documentDocumentTagChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedDocumentDocumentTagChangeHistoryData) => {

        this.documentDocumentTagChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Document Document Tag Change History's detail page
          //
          this.documentDocumentTagChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.documentDocumentTagChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/documentdocumenttagchangehistories', savedDocumentDocumentTagChangeHistoryData.id]);
          this.alertService.showMessage('Document Document Tag Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.documentDocumentTagChangeHistoryData = savedDocumentDocumentTagChangeHistoryData;
          this.buildFormValues(this.documentDocumentTagChangeHistoryData);

          this.alertService.showMessage("Document Document Tag Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Document Document Tag Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Document Document Tag Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Document Document Tag Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerDocumentDocumentTagChangeHistoryReader(): boolean {
    return this.documentDocumentTagChangeHistoryService.userIsSchedulerDocumentDocumentTagChangeHistoryReader();
  }

  public userIsSchedulerDocumentDocumentTagChangeHistoryWriter(): boolean {
    return this.documentDocumentTagChangeHistoryService.userIsSchedulerDocumentDocumentTagChangeHistoryWriter();
  }
}
