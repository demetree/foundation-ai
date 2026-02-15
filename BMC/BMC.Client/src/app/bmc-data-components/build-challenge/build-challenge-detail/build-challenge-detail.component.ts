/*
   GENERATED FORM FOR THE BUILDCHALLENGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildChallenge table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-challenge-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildChallengeService, BuildChallengeData, BuildChallengeSubmitData } from '../../../bmc-data-services/build-challenge.service';
import { BuildChallengeChangeHistoryService } from '../../../bmc-data-services/build-challenge-change-history.service';
import { BuildChallengeEntryService } from '../../../bmc-data-services/build-challenge-entry.service';
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
interface BuildChallengeFormValues {
  name: string,
  description: string | null,
  rules: string | null,
  thumbnailImagePath: string | null,
  startDate: string,
  endDate: string,
  votingEndDate: string | null,
  isActive: boolean,
  isFeatured: boolean,
  entryCount: string,     // Stored as string for form input, converted to number on submit.
  maxPartsLimit: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-build-challenge-detail',
  templateUrl: './build-challenge-detail.component.html',
  styleUrls: ['./build-challenge-detail.component.scss']
})

export class BuildChallengeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildChallengeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildChallengeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        rules: [''],
        thumbnailImagePath: [''],
        startDate: ['', Validators.required],
        endDate: ['', Validators.required],
        votingEndDate: [''],
        isActive: [false],
        isFeatured: [false],
        entryCount: ['', Validators.required],
        maxPartsLimit: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public buildChallengeId: string | null = null;
  public buildChallengeData: BuildChallengeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  buildChallenges$ = this.buildChallengeService.GetBuildChallengeList();
  public buildChallengeChangeHistories$ = this.buildChallengeChangeHistoryService.GetBuildChallengeChangeHistoryList();
  public buildChallengeEntries$ = this.buildChallengeEntryService.GetBuildChallengeEntryList();

  private destroy$ = new Subject<void>();

  constructor(
    public buildChallengeService: BuildChallengeService,
    public buildChallengeChangeHistoryService: BuildChallengeChangeHistoryService,
    public buildChallengeEntryService: BuildChallengeEntryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the buildChallengeId from the route parameters
    this.buildChallengeId = this.route.snapshot.paramMap.get('buildChallengeId');

    if (this.buildChallengeId === 'new' ||
        this.buildChallengeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.buildChallengeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.buildChallengeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildChallengeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Build Challenge';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Build Challenge';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.buildChallengeForm.dirty) {
      return confirm('You have unsaved Build Challenge changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.buildChallengeId != null && this.buildChallengeId !== 'new') {

      const id = parseInt(this.buildChallengeId, 10);

      if (!isNaN(id)) {
        return { buildChallengeId: id };
      }
    }

    return null;
  }


/*
  * Loads the BuildChallenge data for the current buildChallengeId.
  *
  * Fully respects the BuildChallengeService caching strategy and error handling strategy.
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
    if (!this.buildChallengeService.userIsBMCBuildChallengeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BuildChallenges.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate buildChallengeId
    //
    if (!this.buildChallengeId) {

      this.alertService.showMessage('No BuildChallenge ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const buildChallengeId = Number(this.buildChallengeId);

    if (isNaN(buildChallengeId) || buildChallengeId <= 0) {

      this.alertService.showMessage(`Invalid Build Challenge ID: "${this.buildChallengeId}"`,
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
      // This is the most targeted way: clear only this BuildChallenge + relations

      this.buildChallengeService.ClearRecordCache(buildChallengeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.buildChallengeService.GetBuildChallenge(buildChallengeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (buildChallengeData) => {

        //
        // Success path — buildChallengeData can legitimately be null if 404'd but request succeeded
        //
        if (!buildChallengeData) {

          this.handleBuildChallengeNotFound(buildChallengeId);

        } else {

          this.buildChallengeData = buildChallengeData;
          this.buildFormValues(this.buildChallengeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BuildChallenge loaded successfully',
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
        this.handleBuildChallengeLoadError(error, buildChallengeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBuildChallengeNotFound(buildChallengeId: number): void {

    this.buildChallengeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BuildChallenge #${buildChallengeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBuildChallengeLoadError(error: any, buildChallengeId: number): void {

    let message = 'Failed to load Build Challenge.';
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
          message = 'You do not have permission to view this Build Challenge.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Build Challenge #${buildChallengeId} was not found.`;
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

    console.error(`Build Challenge load failed (ID: ${buildChallengeId})`, error);

    //
    // Reset UI to safe state
    //
    this.buildChallengeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(buildChallengeData: BuildChallengeData | null) {

    if (buildChallengeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildChallengeForm.reset({
        name: '',
        description: '',
        rules: '',
        thumbnailImagePath: '',
        startDate: '',
        endDate: '',
        votingEndDate: '',
        isActive: false,
        isFeatured: false,
        entryCount: '',
        maxPartsLimit: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.buildChallengeForm.reset({
        name: buildChallengeData.name ?? '',
        description: buildChallengeData.description ?? '',
        rules: buildChallengeData.rules ?? '',
        thumbnailImagePath: buildChallengeData.thumbnailImagePath ?? '',
        startDate: isoUtcStringToDateTimeLocal(buildChallengeData.startDate) ?? '',
        endDate: isoUtcStringToDateTimeLocal(buildChallengeData.endDate) ?? '',
        votingEndDate: isoUtcStringToDateTimeLocal(buildChallengeData.votingEndDate) ?? '',
        isActive: buildChallengeData.isActive ?? false,
        isFeatured: buildChallengeData.isFeatured ?? false,
        entryCount: buildChallengeData.entryCount?.toString() ?? '',
        maxPartsLimit: buildChallengeData.maxPartsLimit?.toString() ?? '',
        versionNumber: buildChallengeData.versionNumber?.toString() ?? '',
        active: buildChallengeData.active ?? true,
        deleted: buildChallengeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.buildChallengeForm.markAsPristine();
    this.buildChallengeForm.markAsUntouched();
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

    if (this.buildChallengeService.userIsBMCBuildChallengeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Build Challenges", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.buildChallengeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildChallengeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildChallengeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildChallengeSubmitData: BuildChallengeSubmitData = {
        id: this.buildChallengeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        rules: formValue.rules?.trim() || null,
        thumbnailImagePath: formValue.thumbnailImagePath?.trim() || null,
        startDate: dateTimeLocalToIsoUtc(formValue.startDate!.trim())!,
        endDate: dateTimeLocalToIsoUtc(formValue.endDate!.trim())!,
        votingEndDate: formValue.votingEndDate ? dateTimeLocalToIsoUtc(formValue.votingEndDate.trim()) : null,
        isActive: !!formValue.isActive,
        isFeatured: !!formValue.isFeatured,
        entryCount: Number(formValue.entryCount),
        maxPartsLimit: formValue.maxPartsLimit ? Number(formValue.maxPartsLimit) : null,
        versionNumber: this.buildChallengeData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.buildChallengeService.PutBuildChallenge(buildChallengeSubmitData.id, buildChallengeSubmitData)
      : this.buildChallengeService.PostBuildChallenge(buildChallengeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBuildChallengeData) => {

        this.buildChallengeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Build Challenge's detail page
          //
          this.buildChallengeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.buildChallengeForm.markAsUntouched();

          this.router.navigate(['/buildchallenges', savedBuildChallengeData.id]);
          this.alertService.showMessage('Build Challenge added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.buildChallengeData = savedBuildChallengeData;
          this.buildFormValues(this.buildChallengeData);

          this.alertService.showMessage("Build Challenge saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Build Challenge.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Challenge.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Challenge could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBuildChallengeReader(): boolean {
    return this.buildChallengeService.userIsBMCBuildChallengeReader();
  }

  public userIsBMCBuildChallengeWriter(): boolean {
    return this.buildChallengeService.userIsBMCBuildChallengeWriter();
  }
}
