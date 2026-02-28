/*
   GENERATED FORM FOR THE MODELSTEPPART TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ModelStepPart table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to model-step-part-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ModelStepPartService, ModelStepPartData, ModelStepPartSubmitData } from '../../../bmc-data-services/model-step-part.service';
import { ModelBuildStepService } from '../../../bmc-data-services/model-build-step.service';
import { BrickPartService } from '../../../bmc-data-services/brick-part.service';
import { BrickColourService } from '../../../bmc-data-services/brick-colour.service';
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
interface ModelStepPartFormValues {
  modelBuildStepId: number | bigint,       // For FK link number
  brickPartId: number | bigint | null,       // For FK link number
  brickColourId: number | bigint | null,       // For FK link number
  partFileName: string,
  colorCode: string,     // Stored as string for form input, converted to number on submit.
  positionX: string | null,     // Stored as string for form input, converted to number on submit.
  positionY: string | null,     // Stored as string for form input, converted to number on submit.
  positionZ: string | null,     // Stored as string for form input, converted to number on submit.
  transformMatrix: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-model-step-part-detail',
  templateUrl: './model-step-part-detail.component.html',
  styleUrls: ['./model-step-part-detail.component.scss']
})

export class ModelStepPartDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ModelStepPartFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public modelStepPartForm: FormGroup = this.fb.group({
        modelBuildStepId: [null, Validators.required],
        brickPartId: [null],
        brickColourId: [null],
        partFileName: ['', Validators.required],
        colorCode: ['', Validators.required],
        positionX: [''],
        positionY: [''],
        positionZ: [''],
        transformMatrix: ['', Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public modelStepPartId: string | null = null;
  public modelStepPartData: ModelStepPartData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  modelStepParts$ = this.modelStepPartService.GetModelStepPartList();
  public modelBuildSteps$ = this.modelBuildStepService.GetModelBuildStepList();
  public brickParts$ = this.brickPartService.GetBrickPartList();
  public brickColours$ = this.brickColourService.GetBrickColourList();

  private destroy$ = new Subject<void>();

  constructor(
    public modelStepPartService: ModelStepPartService,
    public modelBuildStepService: ModelBuildStepService,
    public brickPartService: BrickPartService,
    public brickColourService: BrickColourService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the modelStepPartId from the route parameters
    this.modelStepPartId = this.route.snapshot.paramMap.get('modelStepPartId');

    if (this.modelStepPartId === 'new' ||
        this.modelStepPartId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.modelStepPartData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.modelStepPartForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.modelStepPartForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Model Step Part';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Model Step Part';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.modelStepPartForm.dirty) {
      return confirm('You have unsaved Model Step Part changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.modelStepPartId != null && this.modelStepPartId !== 'new') {

      const id = parseInt(this.modelStepPartId, 10);

      if (!isNaN(id)) {
        return { modelStepPartId: id };
      }
    }

    return null;
  }


/*
  * Loads the ModelStepPart data for the current modelStepPartId.
  *
  * Fully respects the ModelStepPartService caching strategy and error handling strategy.
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
    if (!this.modelStepPartService.userIsBMCModelStepPartReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ModelStepParts.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate modelStepPartId
    //
    if (!this.modelStepPartId) {

      this.alertService.showMessage('No ModelStepPart ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const modelStepPartId = Number(this.modelStepPartId);

    if (isNaN(modelStepPartId) || modelStepPartId <= 0) {

      this.alertService.showMessage(`Invalid Model Step Part ID: "${this.modelStepPartId}"`,
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
      // This is the most targeted way: clear only this ModelStepPart + relations

      this.modelStepPartService.ClearRecordCache(modelStepPartId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.modelStepPartService.GetModelStepPart(modelStepPartId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (modelStepPartData) => {

        //
        // Success path — modelStepPartData can legitimately be null if 404'd but request succeeded
        //
        if (!modelStepPartData) {

          this.handleModelStepPartNotFound(modelStepPartId);

        } else {

          this.modelStepPartData = modelStepPartData;
          this.buildFormValues(this.modelStepPartData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ModelStepPart loaded successfully',
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
        this.handleModelStepPartLoadError(error, modelStepPartId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleModelStepPartNotFound(modelStepPartId: number): void {

    this.modelStepPartData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ModelStepPart #${modelStepPartId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleModelStepPartLoadError(error: any, modelStepPartId: number): void {

    let message = 'Failed to load Model Step Part.';
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
          message = 'You do not have permission to view this Model Step Part.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Model Step Part #${modelStepPartId} was not found.`;
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

    console.error(`Model Step Part load failed (ID: ${modelStepPartId})`, error);

    //
    // Reset UI to safe state
    //
    this.modelStepPartData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(modelStepPartData: ModelStepPartData | null) {

    if (modelStepPartData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.modelStepPartForm.reset({
        modelBuildStepId: null,
        brickPartId: null,
        brickColourId: null,
        partFileName: '',
        colorCode: '',
        positionX: '',
        positionY: '',
        positionZ: '',
        transformMatrix: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.modelStepPartForm.reset({
        modelBuildStepId: modelStepPartData.modelBuildStepId,
        brickPartId: modelStepPartData.brickPartId,
        brickColourId: modelStepPartData.brickColourId,
        partFileName: modelStepPartData.partFileName ?? '',
        colorCode: modelStepPartData.colorCode?.toString() ?? '',
        positionX: modelStepPartData.positionX?.toString() ?? '',
        positionY: modelStepPartData.positionY?.toString() ?? '',
        positionZ: modelStepPartData.positionZ?.toString() ?? '',
        transformMatrix: modelStepPartData.transformMatrix ?? '',
        sequence: modelStepPartData.sequence?.toString() ?? '',
        active: modelStepPartData.active ?? true,
        deleted: modelStepPartData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.modelStepPartForm.markAsPristine();
    this.modelStepPartForm.markAsUntouched();
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

    if (this.modelStepPartService.userIsBMCModelStepPartWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Model Step Parts", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.modelStepPartForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.modelStepPartForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.modelStepPartForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const modelStepPartSubmitData: ModelStepPartSubmitData = {
        id: this.modelStepPartData?.id || 0,
        modelBuildStepId: Number(formValue.modelBuildStepId),
        brickPartId: formValue.brickPartId ? Number(formValue.brickPartId) : null,
        brickColourId: formValue.brickColourId ? Number(formValue.brickColourId) : null,
        partFileName: formValue.partFileName!.trim(),
        colorCode: Number(formValue.colorCode),
        positionX: formValue.positionX ? Number(formValue.positionX) : null,
        positionY: formValue.positionY ? Number(formValue.positionY) : null,
        positionZ: formValue.positionZ ? Number(formValue.positionZ) : null,
        transformMatrix: formValue.transformMatrix!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.modelStepPartService.PutModelStepPart(modelStepPartSubmitData.id, modelStepPartSubmitData)
      : this.modelStepPartService.PostModelStepPart(modelStepPartSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedModelStepPartData) => {

        this.modelStepPartService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Model Step Part's detail page
          //
          this.modelStepPartForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.modelStepPartForm.markAsUntouched();

          this.router.navigate(['/modelstepparts', savedModelStepPartData.id]);
          this.alertService.showMessage('Model Step Part added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.modelStepPartData = savedModelStepPartData;
          this.buildFormValues(this.modelStepPartData);

          this.alertService.showMessage("Model Step Part saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Model Step Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Model Step Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Model Step Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCModelStepPartReader(): boolean {
    return this.modelStepPartService.userIsBMCModelStepPartReader();
  }

  public userIsBMCModelStepPartWriter(): boolean {
    return this.modelStepPartService.userIsBMCModelStepPartWriter();
  }
}
