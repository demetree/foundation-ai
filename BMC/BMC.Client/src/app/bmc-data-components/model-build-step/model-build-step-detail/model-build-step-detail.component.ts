/*
   GENERATED FORM FOR THE MODELBUILDSTEP TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ModelBuildStep table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to model-build-step-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ModelBuildStepService, ModelBuildStepData, ModelBuildStepSubmitData } from '../../../bmc-data-services/model-build-step.service';
import { ModelSubFileService } from '../../../bmc-data-services/model-sub-file.service';
import { ModelStepPartService } from '../../../bmc-data-services/model-step-part.service';
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
interface ModelBuildStepFormValues {
  modelSubFileId: number | bigint,       // For FK link number
  stepNumber: string,     // Stored as string for form input, converted to number on submit.
  rotationType: string | null,
  rotationX: string | null,     // Stored as string for form input, converted to number on submit.
  rotationY: string | null,     // Stored as string for form input, converted to number on submit.
  rotationZ: string | null,     // Stored as string for form input, converted to number on submit.
  description: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-model-build-step-detail',
  templateUrl: './model-build-step-detail.component.html',
  styleUrls: ['./model-build-step-detail.component.scss']
})

export class ModelBuildStepDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ModelBuildStepFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public modelBuildStepForm: FormGroup = this.fb.group({
        modelSubFileId: [null, Validators.required],
        stepNumber: ['', Validators.required],
        rotationType: [''],
        rotationX: [''],
        rotationY: [''],
        rotationZ: [''],
        description: [''],
        active: [true],
        deleted: [false],
      });


  public modelBuildStepId: string | null = null;
  public modelBuildStepData: ModelBuildStepData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  modelBuildSteps$ = this.modelBuildStepService.GetModelBuildStepList();
  public modelSubFiles$ = this.modelSubFileService.GetModelSubFileList();
  public modelStepParts$ = this.modelStepPartService.GetModelStepPartList();

  private destroy$ = new Subject<void>();

  constructor(
    public modelBuildStepService: ModelBuildStepService,
    public modelSubFileService: ModelSubFileService,
    public modelStepPartService: ModelStepPartService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the modelBuildStepId from the route parameters
    this.modelBuildStepId = this.route.snapshot.paramMap.get('modelBuildStepId');

    if (this.modelBuildStepId === 'new' ||
        this.modelBuildStepId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.modelBuildStepData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.modelBuildStepForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.modelBuildStepForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Model Build Step';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Model Build Step';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.modelBuildStepForm.dirty) {
      return confirm('You have unsaved Model Build Step changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.modelBuildStepId != null && this.modelBuildStepId !== 'new') {

      const id = parseInt(this.modelBuildStepId, 10);

      if (!isNaN(id)) {
        return { modelBuildStepId: id };
      }
    }

    return null;
  }


/*
  * Loads the ModelBuildStep data for the current modelBuildStepId.
  *
  * Fully respects the ModelBuildStepService caching strategy and error handling strategy.
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
    if (!this.modelBuildStepService.userIsBMCModelBuildStepReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ModelBuildSteps.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate modelBuildStepId
    //
    if (!this.modelBuildStepId) {

      this.alertService.showMessage('No ModelBuildStep ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const modelBuildStepId = Number(this.modelBuildStepId);

    if (isNaN(modelBuildStepId) || modelBuildStepId <= 0) {

      this.alertService.showMessage(`Invalid Model Build Step ID: "${this.modelBuildStepId}"`,
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
      // This is the most targeted way: clear only this ModelBuildStep + relations

      this.modelBuildStepService.ClearRecordCache(modelBuildStepId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.modelBuildStepService.GetModelBuildStep(modelBuildStepId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (modelBuildStepData) => {

        //
        // Success path — modelBuildStepData can legitimately be null if 404'd but request succeeded
        //
        if (!modelBuildStepData) {

          this.handleModelBuildStepNotFound(modelBuildStepId);

        } else {

          this.modelBuildStepData = modelBuildStepData;
          this.buildFormValues(this.modelBuildStepData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ModelBuildStep loaded successfully',
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
        this.handleModelBuildStepLoadError(error, modelBuildStepId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleModelBuildStepNotFound(modelBuildStepId: number): void {

    this.modelBuildStepData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ModelBuildStep #${modelBuildStepId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleModelBuildStepLoadError(error: any, modelBuildStepId: number): void {

    let message = 'Failed to load Model Build Step.';
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
          message = 'You do not have permission to view this Model Build Step.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Model Build Step #${modelBuildStepId} was not found.`;
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

    console.error(`Model Build Step load failed (ID: ${modelBuildStepId})`, error);

    //
    // Reset UI to safe state
    //
    this.modelBuildStepData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(modelBuildStepData: ModelBuildStepData | null) {

    if (modelBuildStepData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.modelBuildStepForm.reset({
        modelSubFileId: null,
        stepNumber: '',
        rotationType: '',
        rotationX: '',
        rotationY: '',
        rotationZ: '',
        description: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.modelBuildStepForm.reset({
        modelSubFileId: modelBuildStepData.modelSubFileId,
        stepNumber: modelBuildStepData.stepNumber?.toString() ?? '',
        rotationType: modelBuildStepData.rotationType ?? '',
        rotationX: modelBuildStepData.rotationX?.toString() ?? '',
        rotationY: modelBuildStepData.rotationY?.toString() ?? '',
        rotationZ: modelBuildStepData.rotationZ?.toString() ?? '',
        description: modelBuildStepData.description ?? '',
        active: modelBuildStepData.active ?? true,
        deleted: modelBuildStepData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.modelBuildStepForm.markAsPristine();
    this.modelBuildStepForm.markAsUntouched();
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

    if (this.modelBuildStepService.userIsBMCModelBuildStepWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Model Build Steps", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.modelBuildStepForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.modelBuildStepForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.modelBuildStepForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const modelBuildStepSubmitData: ModelBuildStepSubmitData = {
        id: this.modelBuildStepData?.id || 0,
        modelSubFileId: Number(formValue.modelSubFileId),
        stepNumber: Number(formValue.stepNumber),
        rotationType: formValue.rotationType?.trim() || null,
        rotationX: formValue.rotationX ? Number(formValue.rotationX) : null,
        rotationY: formValue.rotationY ? Number(formValue.rotationY) : null,
        rotationZ: formValue.rotationZ ? Number(formValue.rotationZ) : null,
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.modelBuildStepService.PutModelBuildStep(modelBuildStepSubmitData.id, modelBuildStepSubmitData)
      : this.modelBuildStepService.PostModelBuildStep(modelBuildStepSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedModelBuildStepData) => {

        this.modelBuildStepService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Model Build Step's detail page
          //
          this.modelBuildStepForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.modelBuildStepForm.markAsUntouched();

          this.router.navigate(['/modelbuildsteps', savedModelBuildStepData.id]);
          this.alertService.showMessage('Model Build Step added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.modelBuildStepData = savedModelBuildStepData;
          this.buildFormValues(this.modelBuildStepData);

          this.alertService.showMessage("Model Build Step saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Model Build Step.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Model Build Step.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Model Build Step could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCModelBuildStepReader(): boolean {
    return this.modelBuildStepService.userIsBMCModelBuildStepReader();
  }

  public userIsBMCModelBuildStepWriter(): boolean {
    return this.modelBuildStepService.userIsBMCModelBuildStepWriter();
  }
}
