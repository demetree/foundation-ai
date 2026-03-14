/*
   GENERATED FORM FOR THE BUILDMANUALSTEP TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildManualStep table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-manual-step-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildManualStepService, BuildManualStepData, BuildManualStepSubmitData } from '../../../bmc-data-services/build-manual-step.service';
import { BuildManualPageService } from '../../../bmc-data-services/build-manual-page.service';
import { BuildManualStepChangeHistoryService } from '../../../bmc-data-services/build-manual-step-change-history.service';
import { BuildStepPartService } from '../../../bmc-data-services/build-step-part.service';
import { BuildStepAnnotationService } from '../../../bmc-data-services/build-step-annotation.service';
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
interface BuildManualStepFormValues {
  buildManualPageId: number | bigint,       // For FK link number
  stepNumber: string | null,     // Stored as string for form input, converted to number on submit.
  cameraPositionX: string | null,     // Stored as string for form input, converted to number on submit.
  cameraPositionY: string | null,     // Stored as string for form input, converted to number on submit.
  cameraPositionZ: string | null,     // Stored as string for form input, converted to number on submit.
  cameraTargetX: string | null,     // Stored as string for form input, converted to number on submit.
  cameraTargetY: string | null,     // Stored as string for form input, converted to number on submit.
  cameraTargetZ: string | null,     // Stored as string for form input, converted to number on submit.
  cameraZoom: string | null,     // Stored as string for form input, converted to number on submit.
  showExplodedView: boolean,
  explodedDistance: string | null,     // Stored as string for form input, converted to number on submit.
  renderImagePath: string | null,
  pliImagePath: string | null,
  fadeStepEnabled: boolean,
  isCallout: boolean,
  calloutModelName: string | null,
  showPartsListImage: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-build-manual-step-detail',
  templateUrl: './build-manual-step-detail.component.html',
  styleUrls: ['./build-manual-step-detail.component.scss']
})

export class BuildManualStepDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildManualStepFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildManualStepForm: FormGroup = this.fb.group({
        buildManualPageId: [null, Validators.required],
        stepNumber: [''],
        cameraPositionX: [''],
        cameraPositionY: [''],
        cameraPositionZ: [''],
        cameraTargetX: [''],
        cameraTargetY: [''],
        cameraTargetZ: [''],
        cameraZoom: [''],
        showExplodedView: [false],
        explodedDistance: [''],
        renderImagePath: [''],
        pliImagePath: [''],
        fadeStepEnabled: [false],
        isCallout: [false],
        calloutModelName: [''],
        showPartsListImage: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public buildManualStepId: string | null = null;
  public buildManualStepData: BuildManualStepData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  buildManualSteps$ = this.buildManualStepService.GetBuildManualStepList();
  public buildManualPages$ = this.buildManualPageService.GetBuildManualPageList();
  public buildManualStepChangeHistories$ = this.buildManualStepChangeHistoryService.GetBuildManualStepChangeHistoryList();
  public buildStepParts$ = this.buildStepPartService.GetBuildStepPartList();
  public buildStepAnnotations$ = this.buildStepAnnotationService.GetBuildStepAnnotationList();

  private destroy$ = new Subject<void>();

  constructor(
    public buildManualStepService: BuildManualStepService,
    public buildManualPageService: BuildManualPageService,
    public buildManualStepChangeHistoryService: BuildManualStepChangeHistoryService,
    public buildStepPartService: BuildStepPartService,
    public buildStepAnnotationService: BuildStepAnnotationService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the buildManualStepId from the route parameters
    this.buildManualStepId = this.route.snapshot.paramMap.get('buildManualStepId');

    if (this.buildManualStepId === 'new' ||
        this.buildManualStepId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.buildManualStepData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.buildManualStepForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildManualStepForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Build Manual Step';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Build Manual Step';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.buildManualStepForm.dirty) {
      return confirm('You have unsaved Build Manual Step changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.buildManualStepId != null && this.buildManualStepId !== 'new') {

      const id = parseInt(this.buildManualStepId, 10);

      if (!isNaN(id)) {
        return { buildManualStepId: id };
      }
    }

    return null;
  }


/*
  * Loads the BuildManualStep data for the current buildManualStepId.
  *
  * Fully respects the BuildManualStepService caching strategy and error handling strategy.
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
    if (!this.buildManualStepService.userIsBMCBuildManualStepReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BuildManualSteps.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate buildManualStepId
    //
    if (!this.buildManualStepId) {

      this.alertService.showMessage('No BuildManualStep ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const buildManualStepId = Number(this.buildManualStepId);

    if (isNaN(buildManualStepId) || buildManualStepId <= 0) {

      this.alertService.showMessage(`Invalid Build Manual Step ID: "${this.buildManualStepId}"`,
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
      // This is the most targeted way: clear only this BuildManualStep + relations

      this.buildManualStepService.ClearRecordCache(buildManualStepId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.buildManualStepService.GetBuildManualStep(buildManualStepId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (buildManualStepData) => {

        //
        // Success path — buildManualStepData can legitimately be null if 404'd but request succeeded
        //
        if (!buildManualStepData) {

          this.handleBuildManualStepNotFound(buildManualStepId);

        } else {

          this.buildManualStepData = buildManualStepData;
          this.buildFormValues(this.buildManualStepData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BuildManualStep loaded successfully',
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
        this.handleBuildManualStepLoadError(error, buildManualStepId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBuildManualStepNotFound(buildManualStepId: number): void {

    this.buildManualStepData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BuildManualStep #${buildManualStepId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBuildManualStepLoadError(error: any, buildManualStepId: number): void {

    let message = 'Failed to load Build Manual Step.';
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
          message = 'You do not have permission to view this Build Manual Step.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Build Manual Step #${buildManualStepId} was not found.`;
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

    console.error(`Build Manual Step load failed (ID: ${buildManualStepId})`, error);

    //
    // Reset UI to safe state
    //
    this.buildManualStepData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(buildManualStepData: BuildManualStepData | null) {

    if (buildManualStepData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildManualStepForm.reset({
        buildManualPageId: null,
        stepNumber: '',
        cameraPositionX: '',
        cameraPositionY: '',
        cameraPositionZ: '',
        cameraTargetX: '',
        cameraTargetY: '',
        cameraTargetZ: '',
        cameraZoom: '',
        showExplodedView: false,
        explodedDistance: '',
        renderImagePath: '',
        pliImagePath: '',
        fadeStepEnabled: false,
        isCallout: false,
        calloutModelName: '',
        showPartsListImage: false,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.buildManualStepForm.reset({
        buildManualPageId: buildManualStepData.buildManualPageId,
        stepNumber: buildManualStepData.stepNumber?.toString() ?? '',
        cameraPositionX: buildManualStepData.cameraPositionX?.toString() ?? '',
        cameraPositionY: buildManualStepData.cameraPositionY?.toString() ?? '',
        cameraPositionZ: buildManualStepData.cameraPositionZ?.toString() ?? '',
        cameraTargetX: buildManualStepData.cameraTargetX?.toString() ?? '',
        cameraTargetY: buildManualStepData.cameraTargetY?.toString() ?? '',
        cameraTargetZ: buildManualStepData.cameraTargetZ?.toString() ?? '',
        cameraZoom: buildManualStepData.cameraZoom?.toString() ?? '',
        showExplodedView: buildManualStepData.showExplodedView ?? false,
        explodedDistance: buildManualStepData.explodedDistance?.toString() ?? '',
        renderImagePath: buildManualStepData.renderImagePath ?? '',
        pliImagePath: buildManualStepData.pliImagePath ?? '',
        fadeStepEnabled: buildManualStepData.fadeStepEnabled ?? false,
        isCallout: buildManualStepData.isCallout ?? false,
        calloutModelName: buildManualStepData.calloutModelName ?? '',
        showPartsListImage: buildManualStepData.showPartsListImage ?? false,
        versionNumber: buildManualStepData.versionNumber?.toString() ?? '',
        active: buildManualStepData.active ?? true,
        deleted: buildManualStepData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.buildManualStepForm.markAsPristine();
    this.buildManualStepForm.markAsUntouched();
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

    if (this.buildManualStepService.userIsBMCBuildManualStepWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Build Manual Steps", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.buildManualStepForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildManualStepForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildManualStepForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildManualStepSubmitData: BuildManualStepSubmitData = {
        id: this.buildManualStepData?.id || 0,
        buildManualPageId: Number(formValue.buildManualPageId),
        stepNumber: formValue.stepNumber ? Number(formValue.stepNumber) : null,
        cameraPositionX: formValue.cameraPositionX ? Number(formValue.cameraPositionX) : null,
        cameraPositionY: formValue.cameraPositionY ? Number(formValue.cameraPositionY) : null,
        cameraPositionZ: formValue.cameraPositionZ ? Number(formValue.cameraPositionZ) : null,
        cameraTargetX: formValue.cameraTargetX ? Number(formValue.cameraTargetX) : null,
        cameraTargetY: formValue.cameraTargetY ? Number(formValue.cameraTargetY) : null,
        cameraTargetZ: formValue.cameraTargetZ ? Number(formValue.cameraTargetZ) : null,
        cameraZoom: formValue.cameraZoom ? Number(formValue.cameraZoom) : null,
        showExplodedView: !!formValue.showExplodedView,
        explodedDistance: formValue.explodedDistance ? Number(formValue.explodedDistance) : null,
        renderImagePath: formValue.renderImagePath?.trim() || null,
        pliImagePath: formValue.pliImagePath?.trim() || null,
        fadeStepEnabled: !!formValue.fadeStepEnabled,
        isCallout: !!formValue.isCallout,
        calloutModelName: formValue.calloutModelName?.trim() || null,
        showPartsListImage: !!formValue.showPartsListImage,
        versionNumber: this.buildManualStepData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.buildManualStepService.PutBuildManualStep(buildManualStepSubmitData.id, buildManualStepSubmitData)
      : this.buildManualStepService.PostBuildManualStep(buildManualStepSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBuildManualStepData) => {

        this.buildManualStepService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Build Manual Step's detail page
          //
          this.buildManualStepForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.buildManualStepForm.markAsUntouched();

          this.router.navigate(['/buildmanualsteps', savedBuildManualStepData.id]);
          this.alertService.showMessage('Build Manual Step added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.buildManualStepData = savedBuildManualStepData;
          this.buildFormValues(this.buildManualStepData);

          this.alertService.showMessage("Build Manual Step saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Build Manual Step.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Manual Step.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Manual Step could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBuildManualStepReader(): boolean {
    return this.buildManualStepService.userIsBMCBuildManualStepReader();
  }

  public userIsBMCBuildManualStepWriter(): boolean {
    return this.buildManualStepService.userIsBMCBuildManualStepWriter();
  }
}
