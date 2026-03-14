/*
   GENERATED FORM FOR THE BUILDMANUALSTEPCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildManualStepChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-manual-step-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildManualStepChangeHistoryService, BuildManualStepChangeHistoryData, BuildManualStepChangeHistorySubmitData } from '../../../bmc-data-services/build-manual-step-change-history.service';
import { BuildManualStepService } from '../../../bmc-data-services/build-manual-step.service';
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
interface BuildManualStepChangeHistoryFormValues {
  buildManualStepId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-build-manual-step-change-history-detail',
  templateUrl: './build-manual-step-change-history-detail.component.html',
  styleUrls: ['./build-manual-step-change-history-detail.component.scss']
})

export class BuildManualStepChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildManualStepChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildManualStepChangeHistoryForm: FormGroup = this.fb.group({
        buildManualStepId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public buildManualStepChangeHistoryId: string | null = null;
  public buildManualStepChangeHistoryData: BuildManualStepChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  buildManualStepChangeHistories$ = this.buildManualStepChangeHistoryService.GetBuildManualStepChangeHistoryList();
  public buildManualSteps$ = this.buildManualStepService.GetBuildManualStepList();

  private destroy$ = new Subject<void>();

  constructor(
    public buildManualStepChangeHistoryService: BuildManualStepChangeHistoryService,
    public buildManualStepService: BuildManualStepService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the buildManualStepChangeHistoryId from the route parameters
    this.buildManualStepChangeHistoryId = this.route.snapshot.paramMap.get('buildManualStepChangeHistoryId');

    if (this.buildManualStepChangeHistoryId === 'new' ||
        this.buildManualStepChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.buildManualStepChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.buildManualStepChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildManualStepChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Build Manual Step Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Build Manual Step Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.buildManualStepChangeHistoryForm.dirty) {
      return confirm('You have unsaved Build Manual Step Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.buildManualStepChangeHistoryId != null && this.buildManualStepChangeHistoryId !== 'new') {

      const id = parseInt(this.buildManualStepChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { buildManualStepChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the BuildManualStepChangeHistory data for the current buildManualStepChangeHistoryId.
  *
  * Fully respects the BuildManualStepChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.buildManualStepChangeHistoryService.userIsBMCBuildManualStepChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BuildManualStepChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate buildManualStepChangeHistoryId
    //
    if (!this.buildManualStepChangeHistoryId) {

      this.alertService.showMessage('No BuildManualStepChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const buildManualStepChangeHistoryId = Number(this.buildManualStepChangeHistoryId);

    if (isNaN(buildManualStepChangeHistoryId) || buildManualStepChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Build Manual Step Change History ID: "${this.buildManualStepChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this BuildManualStepChangeHistory + relations

      this.buildManualStepChangeHistoryService.ClearRecordCache(buildManualStepChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.buildManualStepChangeHistoryService.GetBuildManualStepChangeHistory(buildManualStepChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (buildManualStepChangeHistoryData) => {

        //
        // Success path — buildManualStepChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!buildManualStepChangeHistoryData) {

          this.handleBuildManualStepChangeHistoryNotFound(buildManualStepChangeHistoryId);

        } else {

          this.buildManualStepChangeHistoryData = buildManualStepChangeHistoryData;
          this.buildFormValues(this.buildManualStepChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BuildManualStepChangeHistory loaded successfully',
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
        this.handleBuildManualStepChangeHistoryLoadError(error, buildManualStepChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBuildManualStepChangeHistoryNotFound(buildManualStepChangeHistoryId: number): void {

    this.buildManualStepChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BuildManualStepChangeHistory #${buildManualStepChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBuildManualStepChangeHistoryLoadError(error: any, buildManualStepChangeHistoryId: number): void {

    let message = 'Failed to load Build Manual Step Change History.';
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
          message = 'You do not have permission to view this Build Manual Step Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Build Manual Step Change History #${buildManualStepChangeHistoryId} was not found.`;
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

    console.error(`Build Manual Step Change History load failed (ID: ${buildManualStepChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.buildManualStepChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(buildManualStepChangeHistoryData: BuildManualStepChangeHistoryData | null) {

    if (buildManualStepChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildManualStepChangeHistoryForm.reset({
        buildManualStepId: null,
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
        this.buildManualStepChangeHistoryForm.reset({
        buildManualStepId: buildManualStepChangeHistoryData.buildManualStepId,
        versionNumber: buildManualStepChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(buildManualStepChangeHistoryData.timeStamp) ?? '',
        userId: buildManualStepChangeHistoryData.userId?.toString() ?? '',
        data: buildManualStepChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.buildManualStepChangeHistoryForm.markAsPristine();
    this.buildManualStepChangeHistoryForm.markAsUntouched();
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

    if (this.buildManualStepChangeHistoryService.userIsBMCBuildManualStepChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Build Manual Step Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.buildManualStepChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildManualStepChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildManualStepChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildManualStepChangeHistorySubmitData: BuildManualStepChangeHistorySubmitData = {
        id: this.buildManualStepChangeHistoryData?.id || 0,
        buildManualStepId: Number(formValue.buildManualStepId),
        versionNumber: this.buildManualStepChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.buildManualStepChangeHistoryService.PutBuildManualStepChangeHistory(buildManualStepChangeHistorySubmitData.id, buildManualStepChangeHistorySubmitData)
      : this.buildManualStepChangeHistoryService.PostBuildManualStepChangeHistory(buildManualStepChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBuildManualStepChangeHistoryData) => {

        this.buildManualStepChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Build Manual Step Change History's detail page
          //
          this.buildManualStepChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.buildManualStepChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/buildmanualstepchangehistories', savedBuildManualStepChangeHistoryData.id]);
          this.alertService.showMessage('Build Manual Step Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.buildManualStepChangeHistoryData = savedBuildManualStepChangeHistoryData;
          this.buildFormValues(this.buildManualStepChangeHistoryData);

          this.alertService.showMessage("Build Manual Step Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Build Manual Step Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Manual Step Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Manual Step Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBuildManualStepChangeHistoryReader(): boolean {
    return this.buildManualStepChangeHistoryService.userIsBMCBuildManualStepChangeHistoryReader();
  }

  public userIsBMCBuildManualStepChangeHistoryWriter(): boolean {
    return this.buildManualStepChangeHistoryService.userIsBMCBuildManualStepChangeHistoryWriter();
  }
}
