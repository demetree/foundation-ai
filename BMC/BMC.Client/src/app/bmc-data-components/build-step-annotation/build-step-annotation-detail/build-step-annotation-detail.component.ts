/*
   GENERATED FORM FOR THE BUILDSTEPANNOTATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildStepAnnotation table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-step-annotation-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildStepAnnotationService, BuildStepAnnotationData, BuildStepAnnotationSubmitData } from '../../../bmc-data-services/build-step-annotation.service';
import { BuildManualStepService } from '../../../bmc-data-services/build-manual-step.service';
import { BuildStepAnnotationTypeService } from '../../../bmc-data-services/build-step-annotation-type.service';
import { PlacedBrickService } from '../../../bmc-data-services/placed-brick.service';
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
interface BuildStepAnnotationFormValues {
  buildManualStepId: number | bigint,       // For FK link number
  buildStepAnnotationTypeId: number | bigint,       // For FK link number
  positionX: string | null,     // Stored as string for form input, converted to number on submit.
  positionY: string | null,     // Stored as string for form input, converted to number on submit.
  width: string | null,     // Stored as string for form input, converted to number on submit.
  height: string | null,     // Stored as string for form input, converted to number on submit.
  text: string | null,
  placedBrickId: number | bigint | null,       // For FK link number
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-build-step-annotation-detail',
  templateUrl: './build-step-annotation-detail.component.html',
  styleUrls: ['./build-step-annotation-detail.component.scss']
})

export class BuildStepAnnotationDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildStepAnnotationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildStepAnnotationForm: FormGroup = this.fb.group({
        buildManualStepId: [null, Validators.required],
        buildStepAnnotationTypeId: [null, Validators.required],
        positionX: [''],
        positionY: [''],
        width: [''],
        height: [''],
        text: [''],
        placedBrickId: [null],
        active: [true],
        deleted: [false],
      });


  public buildStepAnnotationId: string | null = null;
  public buildStepAnnotationData: BuildStepAnnotationData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  buildStepAnnotations$ = this.buildStepAnnotationService.GetBuildStepAnnotationList();
  public buildManualSteps$ = this.buildManualStepService.GetBuildManualStepList();
  public buildStepAnnotationTypes$ = this.buildStepAnnotationTypeService.GetBuildStepAnnotationTypeList();
  public placedBricks$ = this.placedBrickService.GetPlacedBrickList();

  private destroy$ = new Subject<void>();

  constructor(
    public buildStepAnnotationService: BuildStepAnnotationService,
    public buildManualStepService: BuildManualStepService,
    public buildStepAnnotationTypeService: BuildStepAnnotationTypeService,
    public placedBrickService: PlacedBrickService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the buildStepAnnotationId from the route parameters
    this.buildStepAnnotationId = this.route.snapshot.paramMap.get('buildStepAnnotationId');

    if (this.buildStepAnnotationId === 'new' ||
        this.buildStepAnnotationId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.buildStepAnnotationData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.buildStepAnnotationForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildStepAnnotationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Build Step Annotation';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Build Step Annotation';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.buildStepAnnotationForm.dirty) {
      return confirm('You have unsaved Build Step Annotation changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.buildStepAnnotationId != null && this.buildStepAnnotationId !== 'new') {

      const id = parseInt(this.buildStepAnnotationId, 10);

      if (!isNaN(id)) {
        return { buildStepAnnotationId: id };
      }
    }

    return null;
  }


/*
  * Loads the BuildStepAnnotation data for the current buildStepAnnotationId.
  *
  * Fully respects the BuildStepAnnotationService caching strategy and error handling strategy.
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
    if (!this.buildStepAnnotationService.userIsBMCBuildStepAnnotationReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BuildStepAnnotations.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate buildStepAnnotationId
    //
    if (!this.buildStepAnnotationId) {

      this.alertService.showMessage('No BuildStepAnnotation ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const buildStepAnnotationId = Number(this.buildStepAnnotationId);

    if (isNaN(buildStepAnnotationId) || buildStepAnnotationId <= 0) {

      this.alertService.showMessage(`Invalid Build Step Annotation ID: "${this.buildStepAnnotationId}"`,
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
      // This is the most targeted way: clear only this BuildStepAnnotation + relations

      this.buildStepAnnotationService.ClearRecordCache(buildStepAnnotationId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.buildStepAnnotationService.GetBuildStepAnnotation(buildStepAnnotationId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (buildStepAnnotationData) => {

        //
        // Success path — buildStepAnnotationData can legitimately be null if 404'd but request succeeded
        //
        if (!buildStepAnnotationData) {

          this.handleBuildStepAnnotationNotFound(buildStepAnnotationId);

        } else {

          this.buildStepAnnotationData = buildStepAnnotationData;
          this.buildFormValues(this.buildStepAnnotationData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BuildStepAnnotation loaded successfully',
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
        this.handleBuildStepAnnotationLoadError(error, buildStepAnnotationId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBuildStepAnnotationNotFound(buildStepAnnotationId: number): void {

    this.buildStepAnnotationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BuildStepAnnotation #${buildStepAnnotationId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBuildStepAnnotationLoadError(error: any, buildStepAnnotationId: number): void {

    let message = 'Failed to load Build Step Annotation.';
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
          message = 'You do not have permission to view this Build Step Annotation.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Build Step Annotation #${buildStepAnnotationId} was not found.`;
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

    console.error(`Build Step Annotation load failed (ID: ${buildStepAnnotationId})`, error);

    //
    // Reset UI to safe state
    //
    this.buildStepAnnotationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(buildStepAnnotationData: BuildStepAnnotationData | null) {

    if (buildStepAnnotationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildStepAnnotationForm.reset({
        buildManualStepId: null,
        buildStepAnnotationTypeId: null,
        positionX: '',
        positionY: '',
        width: '',
        height: '',
        text: '',
        placedBrickId: null,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.buildStepAnnotationForm.reset({
        buildManualStepId: buildStepAnnotationData.buildManualStepId,
        buildStepAnnotationTypeId: buildStepAnnotationData.buildStepAnnotationTypeId,
        positionX: buildStepAnnotationData.positionX?.toString() ?? '',
        positionY: buildStepAnnotationData.positionY?.toString() ?? '',
        width: buildStepAnnotationData.width?.toString() ?? '',
        height: buildStepAnnotationData.height?.toString() ?? '',
        text: buildStepAnnotationData.text ?? '',
        placedBrickId: buildStepAnnotationData.placedBrickId,
        active: buildStepAnnotationData.active ?? true,
        deleted: buildStepAnnotationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.buildStepAnnotationForm.markAsPristine();
    this.buildStepAnnotationForm.markAsUntouched();
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

    if (this.buildStepAnnotationService.userIsBMCBuildStepAnnotationWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Build Step Annotations", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.buildStepAnnotationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildStepAnnotationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildStepAnnotationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildStepAnnotationSubmitData: BuildStepAnnotationSubmitData = {
        id: this.buildStepAnnotationData?.id || 0,
        buildManualStepId: Number(formValue.buildManualStepId),
        buildStepAnnotationTypeId: Number(formValue.buildStepAnnotationTypeId),
        positionX: formValue.positionX ? Number(formValue.positionX) : null,
        positionY: formValue.positionY ? Number(formValue.positionY) : null,
        width: formValue.width ? Number(formValue.width) : null,
        height: formValue.height ? Number(formValue.height) : null,
        text: formValue.text?.trim() || null,
        placedBrickId: formValue.placedBrickId ? Number(formValue.placedBrickId) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.buildStepAnnotationService.PutBuildStepAnnotation(buildStepAnnotationSubmitData.id, buildStepAnnotationSubmitData)
      : this.buildStepAnnotationService.PostBuildStepAnnotation(buildStepAnnotationSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBuildStepAnnotationData) => {

        this.buildStepAnnotationService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Build Step Annotation's detail page
          //
          this.buildStepAnnotationForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.buildStepAnnotationForm.markAsUntouched();

          this.router.navigate(['/buildstepannotations', savedBuildStepAnnotationData.id]);
          this.alertService.showMessage('Build Step Annotation added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.buildStepAnnotationData = savedBuildStepAnnotationData;
          this.buildFormValues(this.buildStepAnnotationData);

          this.alertService.showMessage("Build Step Annotation saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Build Step Annotation.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Step Annotation.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Step Annotation could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBuildStepAnnotationReader(): boolean {
    return this.buildStepAnnotationService.userIsBMCBuildStepAnnotationReader();
  }

  public userIsBMCBuildStepAnnotationWriter(): boolean {
    return this.buildStepAnnotationService.userIsBMCBuildStepAnnotationWriter();
  }
}
