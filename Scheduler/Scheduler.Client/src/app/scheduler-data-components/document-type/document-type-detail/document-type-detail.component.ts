/*
   GENERATED FORM FOR THE DOCUMENTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from DocumentType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to document-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { DocumentTypeService, DocumentTypeData, DocumentTypeSubmitData } from '../../../scheduler-data-services/document-type.service';
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
interface DocumentTypeFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  color: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-document-type-detail',
  templateUrl: './document-type-detail.component.html',
  styleUrls: ['./document-type-detail.component.scss']
})

export class DocumentTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<DocumentTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public documentTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        color: [''],
        active: [true],
        deleted: [false],
      });


  public documentTypeId: string | null = null;
  public documentTypeData: DocumentTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  documentTypes$ = this.documentTypeService.GetDocumentTypeList();
  public documents$ = this.documentService.GetDocumentList();

  private destroy$ = new Subject<void>();

  constructor(
    public documentTypeService: DocumentTypeService,
    public documentService: DocumentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the documentTypeId from the route parameters
    this.documentTypeId = this.route.snapshot.paramMap.get('documentTypeId');

    if (this.documentTypeId === 'new' ||
        this.documentTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.documentTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.documentTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.documentTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Document Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Document Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.documentTypeForm.dirty) {
      return confirm('You have unsaved Document Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.documentTypeId != null && this.documentTypeId !== 'new') {

      const id = parseInt(this.documentTypeId, 10);

      if (!isNaN(id)) {
        return { documentTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the DocumentType data for the current documentTypeId.
  *
  * Fully respects the DocumentTypeService caching strategy and error handling strategy.
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
    if (!this.documentTypeService.userIsSchedulerDocumentTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read DocumentTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate documentTypeId
    //
    if (!this.documentTypeId) {

      this.alertService.showMessage('No DocumentType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const documentTypeId = Number(this.documentTypeId);

    if (isNaN(documentTypeId) || documentTypeId <= 0) {

      this.alertService.showMessage(`Invalid Document Type ID: "${this.documentTypeId}"`,
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
      // This is the most targeted way: clear only this DocumentType + relations

      this.documentTypeService.ClearRecordCache(documentTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.documentTypeService.GetDocumentType(documentTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (documentTypeData) => {

        //
        // Success path — documentTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!documentTypeData) {

          this.handleDocumentTypeNotFound(documentTypeId);

        } else {

          this.documentTypeData = documentTypeData;
          this.buildFormValues(this.documentTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'DocumentType loaded successfully',
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
        this.handleDocumentTypeLoadError(error, documentTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleDocumentTypeNotFound(documentTypeId: number): void {

    this.documentTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `DocumentType #${documentTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleDocumentTypeLoadError(error: any, documentTypeId: number): void {

    let message = 'Failed to load Document Type.';
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
          message = 'You do not have permission to view this Document Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Document Type #${documentTypeId} was not found.`;
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

    console.error(`Document Type load failed (ID: ${documentTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.documentTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(documentTypeData: DocumentTypeData | null) {

    if (documentTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.documentTypeForm.reset({
        name: '',
        description: '',
        sequence: '',
        color: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.documentTypeForm.reset({
        name: documentTypeData.name ?? '',
        description: documentTypeData.description ?? '',
        sequence: documentTypeData.sequence?.toString() ?? '',
        color: documentTypeData.color ?? '',
        active: documentTypeData.active ?? true,
        deleted: documentTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.documentTypeForm.markAsPristine();
    this.documentTypeForm.markAsUntouched();
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

    if (this.documentTypeService.userIsSchedulerDocumentTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Document Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.documentTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.documentTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.documentTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const documentTypeSubmitData: DocumentTypeSubmitData = {
        id: this.documentTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.documentTypeService.PutDocumentType(documentTypeSubmitData.id, documentTypeSubmitData)
      : this.documentTypeService.PostDocumentType(documentTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedDocumentTypeData) => {

        this.documentTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Document Type's detail page
          //
          this.documentTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.documentTypeForm.markAsUntouched();

          this.router.navigate(['/documenttypes', savedDocumentTypeData.id]);
          this.alertService.showMessage('Document Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.documentTypeData = savedDocumentTypeData;
          this.buildFormValues(this.documentTypeData);

          this.alertService.showMessage("Document Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Document Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Document Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Document Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerDocumentTypeReader(): boolean {
    return this.documentTypeService.userIsSchedulerDocumentTypeReader();
  }

  public userIsSchedulerDocumentTypeWriter(): boolean {
    return this.documentTypeService.userIsSchedulerDocumentTypeWriter();
  }
}
