/*
   GENERATED FORM FOR THE BUILDSTEPPART TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildStepPart table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-step-part-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildStepPartService, BuildStepPartData, BuildStepPartSubmitData } from '../../../bmc-data-services/build-step-part.service';
import { BuildManualStepService } from '../../../bmc-data-services/build-manual-step.service';
import { PlacedBrickService } from '../../../bmc-data-services/placed-brick.service';
import { BuildStepPartChangeHistoryService } from '../../../bmc-data-services/build-step-part-change-history.service';
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
interface BuildStepPartFormValues {
  buildManualStepId: number | bigint,       // For FK link number
  placedBrickId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-build-step-part-detail',
  templateUrl: './build-step-part-detail.component.html',
  styleUrls: ['./build-step-part-detail.component.scss']
})

export class BuildStepPartDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildStepPartFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildStepPartForm: FormGroup = this.fb.group({
        buildManualStepId: [null, Validators.required],
        placedBrickId: [null, Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public buildStepPartId: string | null = null;
  public buildStepPartData: BuildStepPartData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  buildStepParts$ = this.buildStepPartService.GetBuildStepPartList();
  public buildManualSteps$ = this.buildManualStepService.GetBuildManualStepList();
  public placedBricks$ = this.placedBrickService.GetPlacedBrickList();
  public buildStepPartChangeHistories$ = this.buildStepPartChangeHistoryService.GetBuildStepPartChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public buildStepPartService: BuildStepPartService,
    public buildManualStepService: BuildManualStepService,
    public placedBrickService: PlacedBrickService,
    public buildStepPartChangeHistoryService: BuildStepPartChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the buildStepPartId from the route parameters
    this.buildStepPartId = this.route.snapshot.paramMap.get('buildStepPartId');

    if (this.buildStepPartId === 'new' ||
        this.buildStepPartId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.buildStepPartData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.buildStepPartForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildStepPartForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Build Step Part';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Build Step Part';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.buildStepPartForm.dirty) {
      return confirm('You have unsaved Build Step Part changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.buildStepPartId != null && this.buildStepPartId !== 'new') {

      const id = parseInt(this.buildStepPartId, 10);

      if (!isNaN(id)) {
        return { buildStepPartId: id };
      }
    }

    return null;
  }


/*
  * Loads the BuildStepPart data for the current buildStepPartId.
  *
  * Fully respects the BuildStepPartService caching strategy and error handling strategy.
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
    if (!this.buildStepPartService.userIsBMCBuildStepPartReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BuildStepParts.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate buildStepPartId
    //
    if (!this.buildStepPartId) {

      this.alertService.showMessage('No BuildStepPart ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const buildStepPartId = Number(this.buildStepPartId);

    if (isNaN(buildStepPartId) || buildStepPartId <= 0) {

      this.alertService.showMessage(`Invalid Build Step Part ID: "${this.buildStepPartId}"`,
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
      // This is the most targeted way: clear only this BuildStepPart + relations

      this.buildStepPartService.ClearRecordCache(buildStepPartId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.buildStepPartService.GetBuildStepPart(buildStepPartId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (buildStepPartData) => {

        //
        // Success path — buildStepPartData can legitimately be null if 404'd but request succeeded
        //
        if (!buildStepPartData) {

          this.handleBuildStepPartNotFound(buildStepPartId);

        } else {

          this.buildStepPartData = buildStepPartData;
          this.buildFormValues(this.buildStepPartData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BuildStepPart loaded successfully',
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
        this.handleBuildStepPartLoadError(error, buildStepPartId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBuildStepPartNotFound(buildStepPartId: number): void {

    this.buildStepPartData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BuildStepPart #${buildStepPartId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBuildStepPartLoadError(error: any, buildStepPartId: number): void {

    let message = 'Failed to load Build Step Part.';
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
          message = 'You do not have permission to view this Build Step Part.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Build Step Part #${buildStepPartId} was not found.`;
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

    console.error(`Build Step Part load failed (ID: ${buildStepPartId})`, error);

    //
    // Reset UI to safe state
    //
    this.buildStepPartData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(buildStepPartData: BuildStepPartData | null) {

    if (buildStepPartData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildStepPartForm.reset({
        buildManualStepId: null,
        placedBrickId: null,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.buildStepPartForm.reset({
        buildManualStepId: buildStepPartData.buildManualStepId,
        placedBrickId: buildStepPartData.placedBrickId,
        versionNumber: buildStepPartData.versionNumber?.toString() ?? '',
        active: buildStepPartData.active ?? true,
        deleted: buildStepPartData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.buildStepPartForm.markAsPristine();
    this.buildStepPartForm.markAsUntouched();
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

    if (this.buildStepPartService.userIsBMCBuildStepPartWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Build Step Parts", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.buildStepPartForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildStepPartForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildStepPartForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildStepPartSubmitData: BuildStepPartSubmitData = {
        id: this.buildStepPartData?.id || 0,
        buildManualStepId: Number(formValue.buildManualStepId),
        placedBrickId: Number(formValue.placedBrickId),
        versionNumber: this.buildStepPartData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.buildStepPartService.PutBuildStepPart(buildStepPartSubmitData.id, buildStepPartSubmitData)
      : this.buildStepPartService.PostBuildStepPart(buildStepPartSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBuildStepPartData) => {

        this.buildStepPartService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Build Step Part's detail page
          //
          this.buildStepPartForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.buildStepPartForm.markAsUntouched();

          this.router.navigate(['/buildstepparts', savedBuildStepPartData.id]);
          this.alertService.showMessage('Build Step Part added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.buildStepPartData = savedBuildStepPartData;
          this.buildFormValues(this.buildStepPartData);

          this.alertService.showMessage("Build Step Part saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Build Step Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Step Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Step Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBuildStepPartReader(): boolean {
    return this.buildStepPartService.userIsBMCBuildStepPartReader();
  }

  public userIsBMCBuildStepPartWriter(): boolean {
    return this.buildStepPartService.userIsBMCBuildStepPartWriter();
  }
}
