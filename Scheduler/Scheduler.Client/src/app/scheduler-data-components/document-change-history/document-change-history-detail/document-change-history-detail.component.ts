/*
   GENERATED FORM FOR THE DOCUMENTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from DocumentChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to document-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { DocumentChangeHistoryService, DocumentChangeHistoryData, DocumentChangeHistorySubmitData } from '../../../scheduler-data-services/document-change-history.service';
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
interface DocumentChangeHistoryFormValues {
  documentId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-document-change-history-detail',
  templateUrl: './document-change-history-detail.component.html',
  styleUrls: ['./document-change-history-detail.component.scss']
})

export class DocumentChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<DocumentChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public documentChangeHistoryForm: FormGroup = this.fb.group({
        documentId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public documentChangeHistoryId: string | null = null;
  public documentChangeHistoryData: DocumentChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  documentChangeHistories$ = this.documentChangeHistoryService.GetDocumentChangeHistoryList();
  public documents$ = this.documentService.GetDocumentList();

  private destroy$ = new Subject<void>();

  constructor(
    public documentChangeHistoryService: DocumentChangeHistoryService,
    public documentService: DocumentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the documentChangeHistoryId from the route parameters
    this.documentChangeHistoryId = this.route.snapshot.paramMap.get('documentChangeHistoryId');

    if (this.documentChangeHistoryId === 'new' ||
        this.documentChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.documentChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.documentChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.documentChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Document Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Document Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.documentChangeHistoryForm.dirty) {
      return confirm('You have unsaved Document Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.documentChangeHistoryId != null && this.documentChangeHistoryId !== 'new') {

      const id = parseInt(this.documentChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { documentChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the DocumentChangeHistory data for the current documentChangeHistoryId.
  *
  * Fully respects the DocumentChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.documentChangeHistoryService.userIsSchedulerDocumentChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read DocumentChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate documentChangeHistoryId
    //
    if (!this.documentChangeHistoryId) {

      this.alertService.showMessage('No DocumentChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const documentChangeHistoryId = Number(this.documentChangeHistoryId);

    if (isNaN(documentChangeHistoryId) || documentChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Document Change History ID: "${this.documentChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this DocumentChangeHistory + relations

      this.documentChangeHistoryService.ClearRecordCache(documentChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.documentChangeHistoryService.GetDocumentChangeHistory(documentChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (documentChangeHistoryData) => {

        //
        // Success path — documentChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!documentChangeHistoryData) {

          this.handleDocumentChangeHistoryNotFound(documentChangeHistoryId);

        } else {

          this.documentChangeHistoryData = documentChangeHistoryData;
          this.buildFormValues(this.documentChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'DocumentChangeHistory loaded successfully',
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
        this.handleDocumentChangeHistoryLoadError(error, documentChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleDocumentChangeHistoryNotFound(documentChangeHistoryId: number): void {

    this.documentChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `DocumentChangeHistory #${documentChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleDocumentChangeHistoryLoadError(error: any, documentChangeHistoryId: number): void {

    let message = 'Failed to load Document Change History.';
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
          message = 'You do not have permission to view this Document Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Document Change History #${documentChangeHistoryId} was not found.`;
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

    console.error(`Document Change History load failed (ID: ${documentChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.documentChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(documentChangeHistoryData: DocumentChangeHistoryData | null) {

    if (documentChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.documentChangeHistoryForm.reset({
        documentId: null,
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
        this.documentChangeHistoryForm.reset({
        documentId: documentChangeHistoryData.documentId,
        versionNumber: documentChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(documentChangeHistoryData.timeStamp) ?? '',
        userId: documentChangeHistoryData.userId?.toString() ?? '',
        data: documentChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.documentChangeHistoryForm.markAsPristine();
    this.documentChangeHistoryForm.markAsUntouched();
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

    if (this.documentChangeHistoryService.userIsSchedulerDocumentChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Document Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.documentChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.documentChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.documentChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const documentChangeHistorySubmitData: DocumentChangeHistorySubmitData = {
        id: this.documentChangeHistoryData?.id || 0,
        documentId: Number(formValue.documentId),
        versionNumber: this.documentChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.documentChangeHistoryService.PutDocumentChangeHistory(documentChangeHistorySubmitData.id, documentChangeHistorySubmitData)
      : this.documentChangeHistoryService.PostDocumentChangeHistory(documentChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedDocumentChangeHistoryData) => {

        this.documentChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Document Change History's detail page
          //
          this.documentChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.documentChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/documentchangehistories', savedDocumentChangeHistoryData.id]);
          this.alertService.showMessage('Document Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.documentChangeHistoryData = savedDocumentChangeHistoryData;
          this.buildFormValues(this.documentChangeHistoryData);

          this.alertService.showMessage("Document Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Document Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Document Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Document Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerDocumentChangeHistoryReader(): boolean {
    return this.documentChangeHistoryService.userIsSchedulerDocumentChangeHistoryReader();
  }

  public userIsSchedulerDocumentChangeHistoryWriter(): boolean {
    return this.documentChangeHistoryService.userIsSchedulerDocumentChangeHistoryWriter();
  }
}
