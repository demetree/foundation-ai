/*
   GENERATED FORM FOR THE BUILDSTEPANNOTATIONCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildStepAnnotationChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-step-annotation-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildStepAnnotationChangeHistoryService, BuildStepAnnotationChangeHistoryData, BuildStepAnnotationChangeHistorySubmitData } from '../../../bmc-data-services/build-step-annotation-change-history.service';
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
interface BuildStepAnnotationChangeHistoryFormValues {
  buildStepAnnotationId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-build-step-annotation-change-history-detail',
  templateUrl: './build-step-annotation-change-history-detail.component.html',
  styleUrls: ['./build-step-annotation-change-history-detail.component.scss']
})

export class BuildStepAnnotationChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildStepAnnotationChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildStepAnnotationChangeHistoryForm: FormGroup = this.fb.group({
        buildStepAnnotationId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public buildStepAnnotationChangeHistoryId: string | null = null;
  public buildStepAnnotationChangeHistoryData: BuildStepAnnotationChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  buildStepAnnotationChangeHistories$ = this.buildStepAnnotationChangeHistoryService.GetBuildStepAnnotationChangeHistoryList();
  public buildStepAnnotations$ = this.buildStepAnnotationService.GetBuildStepAnnotationList();

  private destroy$ = new Subject<void>();

  constructor(
    public buildStepAnnotationChangeHistoryService: BuildStepAnnotationChangeHistoryService,
    public buildStepAnnotationService: BuildStepAnnotationService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the buildStepAnnotationChangeHistoryId from the route parameters
    this.buildStepAnnotationChangeHistoryId = this.route.snapshot.paramMap.get('buildStepAnnotationChangeHistoryId');

    if (this.buildStepAnnotationChangeHistoryId === 'new' ||
        this.buildStepAnnotationChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.buildStepAnnotationChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.buildStepAnnotationChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildStepAnnotationChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Build Step Annotation Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Build Step Annotation Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.buildStepAnnotationChangeHistoryForm.dirty) {
      return confirm('You have unsaved Build Step Annotation Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.buildStepAnnotationChangeHistoryId != null && this.buildStepAnnotationChangeHistoryId !== 'new') {

      const id = parseInt(this.buildStepAnnotationChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { buildStepAnnotationChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the BuildStepAnnotationChangeHistory data for the current buildStepAnnotationChangeHistoryId.
  *
  * Fully respects the BuildStepAnnotationChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.buildStepAnnotationChangeHistoryService.userIsBMCBuildStepAnnotationChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BuildStepAnnotationChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate buildStepAnnotationChangeHistoryId
    //
    if (!this.buildStepAnnotationChangeHistoryId) {

      this.alertService.showMessage('No BuildStepAnnotationChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const buildStepAnnotationChangeHistoryId = Number(this.buildStepAnnotationChangeHistoryId);

    if (isNaN(buildStepAnnotationChangeHistoryId) || buildStepAnnotationChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Build Step Annotation Change History ID: "${this.buildStepAnnotationChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this BuildStepAnnotationChangeHistory + relations

      this.buildStepAnnotationChangeHistoryService.ClearRecordCache(buildStepAnnotationChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.buildStepAnnotationChangeHistoryService.GetBuildStepAnnotationChangeHistory(buildStepAnnotationChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (buildStepAnnotationChangeHistoryData) => {

        //
        // Success path — buildStepAnnotationChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!buildStepAnnotationChangeHistoryData) {

          this.handleBuildStepAnnotationChangeHistoryNotFound(buildStepAnnotationChangeHistoryId);

        } else {

          this.buildStepAnnotationChangeHistoryData = buildStepAnnotationChangeHistoryData;
          this.buildFormValues(this.buildStepAnnotationChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BuildStepAnnotationChangeHistory loaded successfully',
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
        this.handleBuildStepAnnotationChangeHistoryLoadError(error, buildStepAnnotationChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBuildStepAnnotationChangeHistoryNotFound(buildStepAnnotationChangeHistoryId: number): void {

    this.buildStepAnnotationChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BuildStepAnnotationChangeHistory #${buildStepAnnotationChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBuildStepAnnotationChangeHistoryLoadError(error: any, buildStepAnnotationChangeHistoryId: number): void {

    let message = 'Failed to load Build Step Annotation Change History.';
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
          message = 'You do not have permission to view this Build Step Annotation Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Build Step Annotation Change History #${buildStepAnnotationChangeHistoryId} was not found.`;
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

    console.error(`Build Step Annotation Change History load failed (ID: ${buildStepAnnotationChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.buildStepAnnotationChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(buildStepAnnotationChangeHistoryData: BuildStepAnnotationChangeHistoryData | null) {

    if (buildStepAnnotationChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildStepAnnotationChangeHistoryForm.reset({
        buildStepAnnotationId: null,
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
        this.buildStepAnnotationChangeHistoryForm.reset({
        buildStepAnnotationId: buildStepAnnotationChangeHistoryData.buildStepAnnotationId,
        versionNumber: buildStepAnnotationChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(buildStepAnnotationChangeHistoryData.timeStamp) ?? '',
        userId: buildStepAnnotationChangeHistoryData.userId?.toString() ?? '',
        data: buildStepAnnotationChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.buildStepAnnotationChangeHistoryForm.markAsPristine();
    this.buildStepAnnotationChangeHistoryForm.markAsUntouched();
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

    if (this.buildStepAnnotationChangeHistoryService.userIsBMCBuildStepAnnotationChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Build Step Annotation Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.buildStepAnnotationChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildStepAnnotationChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildStepAnnotationChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildStepAnnotationChangeHistorySubmitData: BuildStepAnnotationChangeHistorySubmitData = {
        id: this.buildStepAnnotationChangeHistoryData?.id || 0,
        buildStepAnnotationId: Number(formValue.buildStepAnnotationId),
        versionNumber: this.buildStepAnnotationChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.buildStepAnnotationChangeHistoryService.PutBuildStepAnnotationChangeHistory(buildStepAnnotationChangeHistorySubmitData.id, buildStepAnnotationChangeHistorySubmitData)
      : this.buildStepAnnotationChangeHistoryService.PostBuildStepAnnotationChangeHistory(buildStepAnnotationChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBuildStepAnnotationChangeHistoryData) => {

        this.buildStepAnnotationChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Build Step Annotation Change History's detail page
          //
          this.buildStepAnnotationChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.buildStepAnnotationChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/buildstepannotationchangehistories', savedBuildStepAnnotationChangeHistoryData.id]);
          this.alertService.showMessage('Build Step Annotation Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.buildStepAnnotationChangeHistoryData = savedBuildStepAnnotationChangeHistoryData;
          this.buildFormValues(this.buildStepAnnotationChangeHistoryData);

          this.alertService.showMessage("Build Step Annotation Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Build Step Annotation Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Step Annotation Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Step Annotation Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBuildStepAnnotationChangeHistoryReader(): boolean {
    return this.buildStepAnnotationChangeHistoryService.userIsBMCBuildStepAnnotationChangeHistoryReader();
  }

  public userIsBMCBuildStepAnnotationChangeHistoryWriter(): boolean {
    return this.buildStepAnnotationChangeHistoryService.userIsBMCBuildStepAnnotationChangeHistoryWriter();
  }
}
