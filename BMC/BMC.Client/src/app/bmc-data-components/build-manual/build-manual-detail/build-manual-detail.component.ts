/*
   GENERATED FORM FOR THE BUILDMANUAL TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildManual table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-manual-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildManualService, BuildManualData, BuildManualSubmitData } from '../../../bmc-data-services/build-manual.service';
import { ProjectService } from '../../../bmc-data-services/project.service';
import { BuildManualChangeHistoryService } from '../../../bmc-data-services/build-manual-change-history.service';
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
interface BuildManualFormValues {
  projectId: number | bigint,       // For FK link number
  name: string,
  description: string,
  pageWidthMm: string | null,     // Stored as string for form input, converted to number on submit.
  pageHeightMm: string | null,     // Stored as string for form input, converted to number on submit.
  isPublished: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-build-manual-detail',
  templateUrl: './build-manual-detail.component.html',
  styleUrls: ['./build-manual-detail.component.scss']
})

export class BuildManualDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildManualFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildManualForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        name: ['', Validators.required],
        description: ['', Validators.required],
        pageWidthMm: [''],
        pageHeightMm: [''],
        isPublished: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public buildManualId: string | null = null;
  public buildManualData: BuildManualData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  buildManuals$ = this.buildManualService.GetBuildManualList();
  public projects$ = this.projectService.GetProjectList();
  public buildManualChangeHistories$ = this.buildManualChangeHistoryService.GetBuildManualChangeHistoryList();
  public buildManualPages$ = this.buildManualPageService.GetBuildManualPageList();

  private destroy$ = new Subject<void>();

  constructor(
    public buildManualService: BuildManualService,
    public projectService: ProjectService,
    public buildManualChangeHistoryService: BuildManualChangeHistoryService,
    public buildManualPageService: BuildManualPageService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the buildManualId from the route parameters
    this.buildManualId = this.route.snapshot.paramMap.get('buildManualId');

    if (this.buildManualId === 'new' ||
        this.buildManualId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.buildManualData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.buildManualForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildManualForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Build Manual';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Build Manual';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.buildManualForm.dirty) {
      return confirm('You have unsaved Build Manual changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.buildManualId != null && this.buildManualId !== 'new') {

      const id = parseInt(this.buildManualId, 10);

      if (!isNaN(id)) {
        return { buildManualId: id };
      }
    }

    return null;
  }


/*
  * Loads the BuildManual data for the current buildManualId.
  *
  * Fully respects the BuildManualService caching strategy and error handling strategy.
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
    if (!this.buildManualService.userIsBMCBuildManualReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BuildManuals.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate buildManualId
    //
    if (!this.buildManualId) {

      this.alertService.showMessage('No BuildManual ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const buildManualId = Number(this.buildManualId);

    if (isNaN(buildManualId) || buildManualId <= 0) {

      this.alertService.showMessage(`Invalid Build Manual ID: "${this.buildManualId}"`,
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
      // This is the most targeted way: clear only this BuildManual + relations

      this.buildManualService.ClearRecordCache(buildManualId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.buildManualService.GetBuildManual(buildManualId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (buildManualData) => {

        //
        // Success path — buildManualData can legitimately be null if 404'd but request succeeded
        //
        if (!buildManualData) {

          this.handleBuildManualNotFound(buildManualId);

        } else {

          this.buildManualData = buildManualData;
          this.buildFormValues(this.buildManualData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BuildManual loaded successfully',
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
        this.handleBuildManualLoadError(error, buildManualId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBuildManualNotFound(buildManualId: number): void {

    this.buildManualData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BuildManual #${buildManualId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBuildManualLoadError(error: any, buildManualId: number): void {

    let message = 'Failed to load Build Manual.';
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
          message = 'You do not have permission to view this Build Manual.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Build Manual #${buildManualId} was not found.`;
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

    console.error(`Build Manual load failed (ID: ${buildManualId})`, error);

    //
    // Reset UI to safe state
    //
    this.buildManualData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(buildManualData: BuildManualData | null) {

    if (buildManualData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildManualForm.reset({
        projectId: null,
        name: '',
        description: '',
        pageWidthMm: '',
        pageHeightMm: '',
        isPublished: false,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.buildManualForm.reset({
        projectId: buildManualData.projectId,
        name: buildManualData.name ?? '',
        description: buildManualData.description ?? '',
        pageWidthMm: buildManualData.pageWidthMm?.toString() ?? '',
        pageHeightMm: buildManualData.pageHeightMm?.toString() ?? '',
        isPublished: buildManualData.isPublished ?? false,
        versionNumber: buildManualData.versionNumber?.toString() ?? '',
        active: buildManualData.active ?? true,
        deleted: buildManualData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.buildManualForm.markAsPristine();
    this.buildManualForm.markAsUntouched();
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

    if (this.buildManualService.userIsBMCBuildManualWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Build Manuals", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.buildManualForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildManualForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildManualForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildManualSubmitData: BuildManualSubmitData = {
        id: this.buildManualData?.id || 0,
        projectId: Number(formValue.projectId),
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        pageWidthMm: formValue.pageWidthMm ? Number(formValue.pageWidthMm) : null,
        pageHeightMm: formValue.pageHeightMm ? Number(formValue.pageHeightMm) : null,
        isPublished: !!formValue.isPublished,
        versionNumber: this.buildManualData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.buildManualService.PutBuildManual(buildManualSubmitData.id, buildManualSubmitData)
      : this.buildManualService.PostBuildManual(buildManualSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBuildManualData) => {

        this.buildManualService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Build Manual's detail page
          //
          this.buildManualForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.buildManualForm.markAsUntouched();

          this.router.navigate(['/buildmanuals', savedBuildManualData.id]);
          this.alertService.showMessage('Build Manual added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.buildManualData = savedBuildManualData;
          this.buildFormValues(this.buildManualData);

          this.alertService.showMessage("Build Manual saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Build Manual.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Manual.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Manual could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBuildManualReader(): boolean {
    return this.buildManualService.userIsBMCBuildManualReader();
  }

  public userIsBMCBuildManualWriter(): boolean {
    return this.buildManualService.userIsBMCBuildManualWriter();
  }
}
