/*
   GENERATED FORM FOR THE MODELSUBFILE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ModelSubFile table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to model-sub-file-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ModelSubFileService, ModelSubFileData, ModelSubFileSubmitData } from '../../../bmc-data-services/model-sub-file.service';
import { ModelDocumentService } from '../../../bmc-data-services/model-document.service';
import { ModelBuildStepService } from '../../../bmc-data-services/model-build-step.service';
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
interface ModelSubFileFormValues {
  modelDocumentId: number | bigint,       // For FK link number
  fileName: string,
  isMainModel: boolean,
  parentModelSubFileId: number | bigint | null,       // For FK link number
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-model-sub-file-detail',
  templateUrl: './model-sub-file-detail.component.html',
  styleUrls: ['./model-sub-file-detail.component.scss']
})

export class ModelSubFileDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ModelSubFileFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public modelSubFileForm: FormGroup = this.fb.group({
        modelDocumentId: [null, Validators.required],
        fileName: ['', Validators.required],
        isMainModel: [false],
        parentModelSubFileId: [null],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public modelSubFileId: string | null = null;
  public modelSubFileData: ModelSubFileData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  modelSubFiles$ = this.modelSubFileService.GetModelSubFileList();
  public modelDocuments$ = this.modelDocumentService.GetModelDocumentList();
  public modelBuildSteps$ = this.modelBuildStepService.GetModelBuildStepList();

  private destroy$ = new Subject<void>();

  constructor(
    public modelSubFileService: ModelSubFileService,
    public modelDocumentService: ModelDocumentService,
    public modelBuildStepService: ModelBuildStepService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the modelSubFileId from the route parameters
    this.modelSubFileId = this.route.snapshot.paramMap.get('modelSubFileId');

    if (this.modelSubFileId === 'new' ||
        this.modelSubFileId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.modelSubFileData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.modelSubFileForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.modelSubFileForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Model Sub File';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Model Sub File';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.modelSubFileForm.dirty) {
      return confirm('You have unsaved Model Sub File changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.modelSubFileId != null && this.modelSubFileId !== 'new') {

      const id = parseInt(this.modelSubFileId, 10);

      if (!isNaN(id)) {
        return { modelSubFileId: id };
      }
    }

    return null;
  }


/*
  * Loads the ModelSubFile data for the current modelSubFileId.
  *
  * Fully respects the ModelSubFileService caching strategy and error handling strategy.
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
    if (!this.modelSubFileService.userIsBMCModelSubFileReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ModelSubFiles.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate modelSubFileId
    //
    if (!this.modelSubFileId) {

      this.alertService.showMessage('No ModelSubFile ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const modelSubFileId = Number(this.modelSubFileId);

    if (isNaN(modelSubFileId) || modelSubFileId <= 0) {

      this.alertService.showMessage(`Invalid Model Sub File ID: "${this.modelSubFileId}"`,
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
      // This is the most targeted way: clear only this ModelSubFile + relations

      this.modelSubFileService.ClearRecordCache(modelSubFileId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.modelSubFileService.GetModelSubFile(modelSubFileId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (modelSubFileData) => {

        //
        // Success path — modelSubFileData can legitimately be null if 404'd but request succeeded
        //
        if (!modelSubFileData) {

          this.handleModelSubFileNotFound(modelSubFileId);

        } else {

          this.modelSubFileData = modelSubFileData;
          this.buildFormValues(this.modelSubFileData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ModelSubFile loaded successfully',
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
        this.handleModelSubFileLoadError(error, modelSubFileId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleModelSubFileNotFound(modelSubFileId: number): void {

    this.modelSubFileData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ModelSubFile #${modelSubFileId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleModelSubFileLoadError(error: any, modelSubFileId: number): void {

    let message = 'Failed to load Model Sub File.';
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
          message = 'You do not have permission to view this Model Sub File.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Model Sub File #${modelSubFileId} was not found.`;
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

    console.error(`Model Sub File load failed (ID: ${modelSubFileId})`, error);

    //
    // Reset UI to safe state
    //
    this.modelSubFileData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(modelSubFileData: ModelSubFileData | null) {

    if (modelSubFileData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.modelSubFileForm.reset({
        modelDocumentId: null,
        fileName: '',
        isMainModel: false,
        parentModelSubFileId: null,
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.modelSubFileForm.reset({
        modelDocumentId: modelSubFileData.modelDocumentId,
        fileName: modelSubFileData.fileName ?? '',
        isMainModel: modelSubFileData.isMainModel ?? false,
        parentModelSubFileId: modelSubFileData.parentModelSubFileId,
        sequence: modelSubFileData.sequence?.toString() ?? '',
        active: modelSubFileData.active ?? true,
        deleted: modelSubFileData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.modelSubFileForm.markAsPristine();
    this.modelSubFileForm.markAsUntouched();
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

    if (this.modelSubFileService.userIsBMCModelSubFileWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Model Sub Files", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.modelSubFileForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.modelSubFileForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.modelSubFileForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const modelSubFileSubmitData: ModelSubFileSubmitData = {
        id: this.modelSubFileData?.id || 0,
        modelDocumentId: Number(formValue.modelDocumentId),
        fileName: formValue.fileName!.trim(),
        isMainModel: !!formValue.isMainModel,
        parentModelSubFileId: formValue.parentModelSubFileId ? Number(formValue.parentModelSubFileId) : null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.modelSubFileService.PutModelSubFile(modelSubFileSubmitData.id, modelSubFileSubmitData)
      : this.modelSubFileService.PostModelSubFile(modelSubFileSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedModelSubFileData) => {

        this.modelSubFileService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Model Sub File's detail page
          //
          this.modelSubFileForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.modelSubFileForm.markAsUntouched();

          this.router.navigate(['/modelsubfiles', savedModelSubFileData.id]);
          this.alertService.showMessage('Model Sub File added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.modelSubFileData = savedModelSubFileData;
          this.buildFormValues(this.modelSubFileData);

          this.alertService.showMessage("Model Sub File saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Model Sub File.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Model Sub File.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Model Sub File could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCModelSubFileReader(): boolean {
    return this.modelSubFileService.userIsBMCModelSubFileReader();
  }

  public userIsBMCModelSubFileWriter(): boolean {
    return this.modelSubFileService.userIsBMCModelSubFileWriter();
  }
}
