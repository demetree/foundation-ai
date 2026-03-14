/*
   GENERATED FORM FOR THE BUILDMANUALPAGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildManualPage table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-manual-page-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildManualPageService, BuildManualPageData, BuildManualPageSubmitData } from '../../../bmc-data-services/build-manual-page.service';
import { BuildManualService } from '../../../bmc-data-services/build-manual.service';
import { BuildManualPageChangeHistoryService } from '../../../bmc-data-services/build-manual-page-change-history.service';
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
interface BuildManualPageFormValues {
  buildManualId: number | bigint,       // For FK link number
  pageNum: string | null,     // Stored as string for form input, converted to number on submit.
  title: string | null,
  notes: string | null,
  backgroundTheme: string | null,
  layoutPreset: string | null,
  backgroundColorHex: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-build-manual-page-detail',
  templateUrl: './build-manual-page-detail.component.html',
  styleUrls: ['./build-manual-page-detail.component.scss']
})

export class BuildManualPageDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildManualPageFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildManualPageForm: FormGroup = this.fb.group({
        buildManualId: [null, Validators.required],
        pageNum: [''],
        title: [''],
        notes: [''],
        backgroundTheme: [''],
        layoutPreset: [''],
        backgroundColorHex: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public buildManualPageId: string | null = null;
  public buildManualPageData: BuildManualPageData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  buildManualPages$ = this.buildManualPageService.GetBuildManualPageList();
  public buildManuals$ = this.buildManualService.GetBuildManualList();
  public buildManualPageChangeHistories$ = this.buildManualPageChangeHistoryService.GetBuildManualPageChangeHistoryList();
  public buildManualSteps$ = this.buildManualStepService.GetBuildManualStepList();

  private destroy$ = new Subject<void>();

  constructor(
    public buildManualPageService: BuildManualPageService,
    public buildManualService: BuildManualService,
    public buildManualPageChangeHistoryService: BuildManualPageChangeHistoryService,
    public buildManualStepService: BuildManualStepService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the buildManualPageId from the route parameters
    this.buildManualPageId = this.route.snapshot.paramMap.get('buildManualPageId');

    if (this.buildManualPageId === 'new' ||
        this.buildManualPageId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.buildManualPageData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.buildManualPageForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildManualPageForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Build Manual Page';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Build Manual Page';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.buildManualPageForm.dirty) {
      return confirm('You have unsaved Build Manual Page changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.buildManualPageId != null && this.buildManualPageId !== 'new') {

      const id = parseInt(this.buildManualPageId, 10);

      if (!isNaN(id)) {
        return { buildManualPageId: id };
      }
    }

    return null;
  }


/*
  * Loads the BuildManualPage data for the current buildManualPageId.
  *
  * Fully respects the BuildManualPageService caching strategy and error handling strategy.
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
    if (!this.buildManualPageService.userIsBMCBuildManualPageReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BuildManualPages.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate buildManualPageId
    //
    if (!this.buildManualPageId) {

      this.alertService.showMessage('No BuildManualPage ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const buildManualPageId = Number(this.buildManualPageId);

    if (isNaN(buildManualPageId) || buildManualPageId <= 0) {

      this.alertService.showMessage(`Invalid Build Manual Page ID: "${this.buildManualPageId}"`,
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
      // This is the most targeted way: clear only this BuildManualPage + relations

      this.buildManualPageService.ClearRecordCache(buildManualPageId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.buildManualPageService.GetBuildManualPage(buildManualPageId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (buildManualPageData) => {

        //
        // Success path — buildManualPageData can legitimately be null if 404'd but request succeeded
        //
        if (!buildManualPageData) {

          this.handleBuildManualPageNotFound(buildManualPageId);

        } else {

          this.buildManualPageData = buildManualPageData;
          this.buildFormValues(this.buildManualPageData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BuildManualPage loaded successfully',
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
        this.handleBuildManualPageLoadError(error, buildManualPageId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBuildManualPageNotFound(buildManualPageId: number): void {

    this.buildManualPageData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BuildManualPage #${buildManualPageId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBuildManualPageLoadError(error: any, buildManualPageId: number): void {

    let message = 'Failed to load Build Manual Page.';
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
          message = 'You do not have permission to view this Build Manual Page.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Build Manual Page #${buildManualPageId} was not found.`;
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

    console.error(`Build Manual Page load failed (ID: ${buildManualPageId})`, error);

    //
    // Reset UI to safe state
    //
    this.buildManualPageData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(buildManualPageData: BuildManualPageData | null) {

    if (buildManualPageData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildManualPageForm.reset({
        buildManualId: null,
        pageNum: '',
        title: '',
        notes: '',
        backgroundTheme: '',
        layoutPreset: '',
        backgroundColorHex: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.buildManualPageForm.reset({
        buildManualId: buildManualPageData.buildManualId,
        pageNum: buildManualPageData.pageNum?.toString() ?? '',
        title: buildManualPageData.title ?? '',
        notes: buildManualPageData.notes ?? '',
        backgroundTheme: buildManualPageData.backgroundTheme ?? '',
        layoutPreset: buildManualPageData.layoutPreset ?? '',
        backgroundColorHex: buildManualPageData.backgroundColorHex ?? '',
        versionNumber: buildManualPageData.versionNumber?.toString() ?? '',
        active: buildManualPageData.active ?? true,
        deleted: buildManualPageData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.buildManualPageForm.markAsPristine();
    this.buildManualPageForm.markAsUntouched();
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

    if (this.buildManualPageService.userIsBMCBuildManualPageWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Build Manual Pages", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.buildManualPageForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildManualPageForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildManualPageForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildManualPageSubmitData: BuildManualPageSubmitData = {
        id: this.buildManualPageData?.id || 0,
        buildManualId: Number(formValue.buildManualId),
        pageNum: formValue.pageNum ? Number(formValue.pageNum) : null,
        title: formValue.title?.trim() || null,
        notes: formValue.notes?.trim() || null,
        backgroundTheme: formValue.backgroundTheme?.trim() || null,
        layoutPreset: formValue.layoutPreset?.trim() || null,
        backgroundColorHex: formValue.backgroundColorHex?.trim() || null,
        versionNumber: this.buildManualPageData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.buildManualPageService.PutBuildManualPage(buildManualPageSubmitData.id, buildManualPageSubmitData)
      : this.buildManualPageService.PostBuildManualPage(buildManualPageSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBuildManualPageData) => {

        this.buildManualPageService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Build Manual Page's detail page
          //
          this.buildManualPageForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.buildManualPageForm.markAsUntouched();

          this.router.navigate(['/buildmanualpages', savedBuildManualPageData.id]);
          this.alertService.showMessage('Build Manual Page added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.buildManualPageData = savedBuildManualPageData;
          this.buildFormValues(this.buildManualPageData);

          this.alertService.showMessage("Build Manual Page saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Build Manual Page.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Manual Page.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Manual Page could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBuildManualPageReader(): boolean {
    return this.buildManualPageService.userIsBMCBuildManualPageReader();
  }

  public userIsBMCBuildManualPageWriter(): boolean {
    return this.buildManualPageService.userIsBMCBuildManualPageWriter();
  }
}
