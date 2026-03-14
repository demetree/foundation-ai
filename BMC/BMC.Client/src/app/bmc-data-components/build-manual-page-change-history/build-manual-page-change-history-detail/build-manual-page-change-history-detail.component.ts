/*
   GENERATED FORM FOR THE BUILDMANUALPAGECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildManualPageChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-manual-page-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildManualPageChangeHistoryService, BuildManualPageChangeHistoryData, BuildManualPageChangeHistorySubmitData } from '../../../bmc-data-services/build-manual-page-change-history.service';
import { BuildManualPageService } from '../../../bmc-data-services/build-manual-page.service';
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
interface BuildManualPageChangeHistoryFormValues {
  buildManualPageId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-build-manual-page-change-history-detail',
  templateUrl: './build-manual-page-change-history-detail.component.html',
  styleUrls: ['./build-manual-page-change-history-detail.component.scss']
})

export class BuildManualPageChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildManualPageChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildManualPageChangeHistoryForm: FormGroup = this.fb.group({
        buildManualPageId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public buildManualPageChangeHistoryId: string | null = null;
  public buildManualPageChangeHistoryData: BuildManualPageChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  buildManualPageChangeHistories$ = this.buildManualPageChangeHistoryService.GetBuildManualPageChangeHistoryList();
  public buildManualPages$ = this.buildManualPageService.GetBuildManualPageList();

  private destroy$ = new Subject<void>();

  constructor(
    public buildManualPageChangeHistoryService: BuildManualPageChangeHistoryService,
    public buildManualPageService: BuildManualPageService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the buildManualPageChangeHistoryId from the route parameters
    this.buildManualPageChangeHistoryId = this.route.snapshot.paramMap.get('buildManualPageChangeHistoryId');

    if (this.buildManualPageChangeHistoryId === 'new' ||
        this.buildManualPageChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.buildManualPageChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.buildManualPageChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildManualPageChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Build Manual Page Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Build Manual Page Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.buildManualPageChangeHistoryForm.dirty) {
      return confirm('You have unsaved Build Manual Page Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.buildManualPageChangeHistoryId != null && this.buildManualPageChangeHistoryId !== 'new') {

      const id = parseInt(this.buildManualPageChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { buildManualPageChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the BuildManualPageChangeHistory data for the current buildManualPageChangeHistoryId.
  *
  * Fully respects the BuildManualPageChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.buildManualPageChangeHistoryService.userIsBMCBuildManualPageChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BuildManualPageChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate buildManualPageChangeHistoryId
    //
    if (!this.buildManualPageChangeHistoryId) {

      this.alertService.showMessage('No BuildManualPageChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const buildManualPageChangeHistoryId = Number(this.buildManualPageChangeHistoryId);

    if (isNaN(buildManualPageChangeHistoryId) || buildManualPageChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Build Manual Page Change History ID: "${this.buildManualPageChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this BuildManualPageChangeHistory + relations

      this.buildManualPageChangeHistoryService.ClearRecordCache(buildManualPageChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.buildManualPageChangeHistoryService.GetBuildManualPageChangeHistory(buildManualPageChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (buildManualPageChangeHistoryData) => {

        //
        // Success path — buildManualPageChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!buildManualPageChangeHistoryData) {

          this.handleBuildManualPageChangeHistoryNotFound(buildManualPageChangeHistoryId);

        } else {

          this.buildManualPageChangeHistoryData = buildManualPageChangeHistoryData;
          this.buildFormValues(this.buildManualPageChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BuildManualPageChangeHistory loaded successfully',
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
        this.handleBuildManualPageChangeHistoryLoadError(error, buildManualPageChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBuildManualPageChangeHistoryNotFound(buildManualPageChangeHistoryId: number): void {

    this.buildManualPageChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BuildManualPageChangeHistory #${buildManualPageChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBuildManualPageChangeHistoryLoadError(error: any, buildManualPageChangeHistoryId: number): void {

    let message = 'Failed to load Build Manual Page Change History.';
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
          message = 'You do not have permission to view this Build Manual Page Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Build Manual Page Change History #${buildManualPageChangeHistoryId} was not found.`;
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

    console.error(`Build Manual Page Change History load failed (ID: ${buildManualPageChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.buildManualPageChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(buildManualPageChangeHistoryData: BuildManualPageChangeHistoryData | null) {

    if (buildManualPageChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildManualPageChangeHistoryForm.reset({
        buildManualPageId: null,
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
        this.buildManualPageChangeHistoryForm.reset({
        buildManualPageId: buildManualPageChangeHistoryData.buildManualPageId,
        versionNumber: buildManualPageChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(buildManualPageChangeHistoryData.timeStamp) ?? '',
        userId: buildManualPageChangeHistoryData.userId?.toString() ?? '',
        data: buildManualPageChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.buildManualPageChangeHistoryForm.markAsPristine();
    this.buildManualPageChangeHistoryForm.markAsUntouched();
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

    if (this.buildManualPageChangeHistoryService.userIsBMCBuildManualPageChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Build Manual Page Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.buildManualPageChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildManualPageChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildManualPageChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildManualPageChangeHistorySubmitData: BuildManualPageChangeHistorySubmitData = {
        id: this.buildManualPageChangeHistoryData?.id || 0,
        buildManualPageId: Number(formValue.buildManualPageId),
        versionNumber: this.buildManualPageChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.buildManualPageChangeHistoryService.PutBuildManualPageChangeHistory(buildManualPageChangeHistorySubmitData.id, buildManualPageChangeHistorySubmitData)
      : this.buildManualPageChangeHistoryService.PostBuildManualPageChangeHistory(buildManualPageChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBuildManualPageChangeHistoryData) => {

        this.buildManualPageChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Build Manual Page Change History's detail page
          //
          this.buildManualPageChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.buildManualPageChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/buildmanualpagechangehistories', savedBuildManualPageChangeHistoryData.id]);
          this.alertService.showMessage('Build Manual Page Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.buildManualPageChangeHistoryData = savedBuildManualPageChangeHistoryData;
          this.buildFormValues(this.buildManualPageChangeHistoryData);

          this.alertService.showMessage("Build Manual Page Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Build Manual Page Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Manual Page Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Manual Page Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBuildManualPageChangeHistoryReader(): boolean {
    return this.buildManualPageChangeHistoryService.userIsBMCBuildManualPageChangeHistoryReader();
  }

  public userIsBMCBuildManualPageChangeHistoryWriter(): boolean {
    return this.buildManualPageChangeHistoryService.userIsBMCBuildManualPageChangeHistoryWriter();
  }
}
