/*
   GENERATED FORM FOR THE DOCUMENTDOWNLOAD TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from DocumentDownload table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to document-download-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { DocumentDownloadService, DocumentDownloadData, DocumentDownloadSubmitData } from '../../../community-data-services/document-download.service';
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
interface DocumentDownloadFormValues {
  title: string,
  description: string | null,
  filePath: string,
  fileName: string,
  mimeType: string | null,
  fileSizeBytes: string | null,     // Stored as string for form input, converted to number on submit.
  categoryName: string | null,
  documentDate: string | null,
  isPublished: boolean,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-document-download-detail',
  templateUrl: './document-download-detail.component.html',
  styleUrls: ['./document-download-detail.component.scss']
})

export class DocumentDownloadDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<DocumentDownloadFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public documentDownloadForm: FormGroup = this.fb.group({
        title: ['', Validators.required],
        description: [''],
        filePath: ['', Validators.required],
        fileName: ['', Validators.required],
        mimeType: [''],
        fileSizeBytes: [''],
        categoryName: [''],
        documentDate: [''],
        isPublished: [false],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public documentDownloadId: string | null = null;
  public documentDownloadData: DocumentDownloadData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  documentDownloads$ = this.documentDownloadService.GetDocumentDownloadList();

  private destroy$ = new Subject<void>();

  constructor(
    public documentDownloadService: DocumentDownloadService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the documentDownloadId from the route parameters
    this.documentDownloadId = this.route.snapshot.paramMap.get('documentDownloadId');

    if (this.documentDownloadId === 'new' ||
        this.documentDownloadId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.documentDownloadData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.documentDownloadForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.documentDownloadForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Document Download';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Document Download';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.documentDownloadForm.dirty) {
      return confirm('You have unsaved Document Download changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.documentDownloadId != null && this.documentDownloadId !== 'new') {

      const id = parseInt(this.documentDownloadId, 10);

      if (!isNaN(id)) {
        return { documentDownloadId: id };
      }
    }

    return null;
  }


/*
  * Loads the DocumentDownload data for the current documentDownloadId.
  *
  * Fully respects the DocumentDownloadService caching strategy and error handling strategy.
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
    if (!this.documentDownloadService.userIsCommunityDocumentDownloadReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read DocumentDownloads.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate documentDownloadId
    //
    if (!this.documentDownloadId) {

      this.alertService.showMessage('No DocumentDownload ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const documentDownloadId = Number(this.documentDownloadId);

    if (isNaN(documentDownloadId) || documentDownloadId <= 0) {

      this.alertService.showMessage(`Invalid Document Download ID: "${this.documentDownloadId}"`,
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
      // This is the most targeted way: clear only this DocumentDownload + relations

      this.documentDownloadService.ClearRecordCache(documentDownloadId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.documentDownloadService.GetDocumentDownload(documentDownloadId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (documentDownloadData) => {

        //
        // Success path — documentDownloadData can legitimately be null if 404'd but request succeeded
        //
        if (!documentDownloadData) {

          this.handleDocumentDownloadNotFound(documentDownloadId);

        } else {

          this.documentDownloadData = documentDownloadData;
          this.buildFormValues(this.documentDownloadData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'DocumentDownload loaded successfully',
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
        this.handleDocumentDownloadLoadError(error, documentDownloadId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleDocumentDownloadNotFound(documentDownloadId: number): void {

    this.documentDownloadData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `DocumentDownload #${documentDownloadId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleDocumentDownloadLoadError(error: any, documentDownloadId: number): void {

    let message = 'Failed to load Document Download.';
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
          message = 'You do not have permission to view this Document Download.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Document Download #${documentDownloadId} was not found.`;
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

    console.error(`Document Download load failed (ID: ${documentDownloadId})`, error);

    //
    // Reset UI to safe state
    //
    this.documentDownloadData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(documentDownloadData: DocumentDownloadData | null) {

    if (documentDownloadData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.documentDownloadForm.reset({
        title: '',
        description: '',
        filePath: '',
        fileName: '',
        mimeType: '',
        fileSizeBytes: '',
        categoryName: '',
        documentDate: '',
        isPublished: false,
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.documentDownloadForm.reset({
        title: documentDownloadData.title ?? '',
        description: documentDownloadData.description ?? '',
        filePath: documentDownloadData.filePath ?? '',
        fileName: documentDownloadData.fileName ?? '',
        mimeType: documentDownloadData.mimeType ?? '',
        fileSizeBytes: documentDownloadData.fileSizeBytes?.toString() ?? '',
        categoryName: documentDownloadData.categoryName ?? '',
        documentDate: isoUtcStringToDateTimeLocal(documentDownloadData.documentDate) ?? '',
        isPublished: documentDownloadData.isPublished ?? false,
        sequence: documentDownloadData.sequence?.toString() ?? '',
        active: documentDownloadData.active ?? true,
        deleted: documentDownloadData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.documentDownloadForm.markAsPristine();
    this.documentDownloadForm.markAsUntouched();
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

    if (this.documentDownloadService.userIsCommunityDocumentDownloadWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Document Downloads", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.documentDownloadForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.documentDownloadForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.documentDownloadForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const documentDownloadSubmitData: DocumentDownloadSubmitData = {
        id: this.documentDownloadData?.id || 0,
        title: formValue.title!.trim(),
        description: formValue.description?.trim() || null,
        filePath: formValue.filePath!.trim(),
        fileName: formValue.fileName!.trim(),
        mimeType: formValue.mimeType?.trim() || null,
        fileSizeBytes: formValue.fileSizeBytes ? Number(formValue.fileSizeBytes) : null,
        categoryName: formValue.categoryName?.trim() || null,
        documentDate: formValue.documentDate ? dateTimeLocalToIsoUtc(formValue.documentDate.trim()) : null,
        isPublished: !!formValue.isPublished,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.documentDownloadService.PutDocumentDownload(documentDownloadSubmitData.id, documentDownloadSubmitData)
      : this.documentDownloadService.PostDocumentDownload(documentDownloadSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedDocumentDownloadData) => {

        this.documentDownloadService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Document Download's detail page
          //
          this.documentDownloadForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.documentDownloadForm.markAsUntouched();

          this.router.navigate(['/documentdownloads', savedDocumentDownloadData.id]);
          this.alertService.showMessage('Document Download added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.documentDownloadData = savedDocumentDownloadData;
          this.buildFormValues(this.documentDownloadData);

          this.alertService.showMessage("Document Download saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Document Download.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Document Download.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Document Download could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsCommunityDocumentDownloadReader(): boolean {
    return this.documentDownloadService.userIsCommunityDocumentDownloadReader();
  }

  public userIsCommunityDocumentDownloadWriter(): boolean {
    return this.documentDownloadService.userIsCommunityDocumentDownloadWriter();
  }
}
