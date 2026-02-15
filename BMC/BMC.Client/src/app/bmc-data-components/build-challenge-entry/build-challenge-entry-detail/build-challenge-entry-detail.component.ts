/*
   GENERATED FORM FOR THE BUILDCHALLENGEENTRY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildChallengeEntry table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-challenge-entry-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildChallengeEntryService, BuildChallengeEntryData, BuildChallengeEntrySubmitData } from '../../../bmc-data-services/build-challenge-entry.service';
import { BuildChallengeService } from '../../../bmc-data-services/build-challenge.service';
import { PublishedMocService } from '../../../bmc-data-services/published-moc.service';
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
interface BuildChallengeEntryFormValues {
  buildChallengeId: number | bigint,       // For FK link number
  publishedMocId: number | bigint,       // For FK link number
  submittedDate: string,
  entryNotes: string | null,
  voteCount: string,     // Stored as string for form input, converted to number on submit.
  isWinner: boolean,
  isDisqualified: boolean,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-build-challenge-entry-detail',
  templateUrl: './build-challenge-entry-detail.component.html',
  styleUrls: ['./build-challenge-entry-detail.component.scss']
})

export class BuildChallengeEntryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildChallengeEntryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildChallengeEntryForm: FormGroup = this.fb.group({
        buildChallengeId: [null, Validators.required],
        publishedMocId: [null, Validators.required],
        submittedDate: ['', Validators.required],
        entryNotes: [''],
        voteCount: ['', Validators.required],
        isWinner: [false],
        isDisqualified: [false],
        active: [true],
        deleted: [false],
      });


  public buildChallengeEntryId: string | null = null;
  public buildChallengeEntryData: BuildChallengeEntryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  buildChallengeEntries$ = this.buildChallengeEntryService.GetBuildChallengeEntryList();
  public buildChallenges$ = this.buildChallengeService.GetBuildChallengeList();
  public publishedMocs$ = this.publishedMocService.GetPublishedMocList();

  private destroy$ = new Subject<void>();

  constructor(
    public buildChallengeEntryService: BuildChallengeEntryService,
    public buildChallengeService: BuildChallengeService,
    public publishedMocService: PublishedMocService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the buildChallengeEntryId from the route parameters
    this.buildChallengeEntryId = this.route.snapshot.paramMap.get('buildChallengeEntryId');

    if (this.buildChallengeEntryId === 'new' ||
        this.buildChallengeEntryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.buildChallengeEntryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.buildChallengeEntryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildChallengeEntryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Build Challenge Entry';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Build Challenge Entry';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.buildChallengeEntryForm.dirty) {
      return confirm('You have unsaved Build Challenge Entry changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.buildChallengeEntryId != null && this.buildChallengeEntryId !== 'new') {

      const id = parseInt(this.buildChallengeEntryId, 10);

      if (!isNaN(id)) {
        return { buildChallengeEntryId: id };
      }
    }

    return null;
  }


/*
  * Loads the BuildChallengeEntry data for the current buildChallengeEntryId.
  *
  * Fully respects the BuildChallengeEntryService caching strategy and error handling strategy.
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
    if (!this.buildChallengeEntryService.userIsBMCBuildChallengeEntryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BuildChallengeEntries.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate buildChallengeEntryId
    //
    if (!this.buildChallengeEntryId) {

      this.alertService.showMessage('No BuildChallengeEntry ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const buildChallengeEntryId = Number(this.buildChallengeEntryId);

    if (isNaN(buildChallengeEntryId) || buildChallengeEntryId <= 0) {

      this.alertService.showMessage(`Invalid Build Challenge Entry ID: "${this.buildChallengeEntryId}"`,
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
      // This is the most targeted way: clear only this BuildChallengeEntry + relations

      this.buildChallengeEntryService.ClearRecordCache(buildChallengeEntryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.buildChallengeEntryService.GetBuildChallengeEntry(buildChallengeEntryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (buildChallengeEntryData) => {

        //
        // Success path — buildChallengeEntryData can legitimately be null if 404'd but request succeeded
        //
        if (!buildChallengeEntryData) {

          this.handleBuildChallengeEntryNotFound(buildChallengeEntryId);

        } else {

          this.buildChallengeEntryData = buildChallengeEntryData;
          this.buildFormValues(this.buildChallengeEntryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BuildChallengeEntry loaded successfully',
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
        this.handleBuildChallengeEntryLoadError(error, buildChallengeEntryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBuildChallengeEntryNotFound(buildChallengeEntryId: number): void {

    this.buildChallengeEntryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BuildChallengeEntry #${buildChallengeEntryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBuildChallengeEntryLoadError(error: any, buildChallengeEntryId: number): void {

    let message = 'Failed to load Build Challenge Entry.';
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
          message = 'You do not have permission to view this Build Challenge Entry.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Build Challenge Entry #${buildChallengeEntryId} was not found.`;
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

    console.error(`Build Challenge Entry load failed (ID: ${buildChallengeEntryId})`, error);

    //
    // Reset UI to safe state
    //
    this.buildChallengeEntryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(buildChallengeEntryData: BuildChallengeEntryData | null) {

    if (buildChallengeEntryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildChallengeEntryForm.reset({
        buildChallengeId: null,
        publishedMocId: null,
        submittedDate: '',
        entryNotes: '',
        voteCount: '',
        isWinner: false,
        isDisqualified: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.buildChallengeEntryForm.reset({
        buildChallengeId: buildChallengeEntryData.buildChallengeId,
        publishedMocId: buildChallengeEntryData.publishedMocId,
        submittedDate: isoUtcStringToDateTimeLocal(buildChallengeEntryData.submittedDate) ?? '',
        entryNotes: buildChallengeEntryData.entryNotes ?? '',
        voteCount: buildChallengeEntryData.voteCount?.toString() ?? '',
        isWinner: buildChallengeEntryData.isWinner ?? false,
        isDisqualified: buildChallengeEntryData.isDisqualified ?? false,
        active: buildChallengeEntryData.active ?? true,
        deleted: buildChallengeEntryData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.buildChallengeEntryForm.markAsPristine();
    this.buildChallengeEntryForm.markAsUntouched();
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

    if (this.buildChallengeEntryService.userIsBMCBuildChallengeEntryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Build Challenge Entries", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.buildChallengeEntryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildChallengeEntryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildChallengeEntryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildChallengeEntrySubmitData: BuildChallengeEntrySubmitData = {
        id: this.buildChallengeEntryData?.id || 0,
        buildChallengeId: Number(formValue.buildChallengeId),
        publishedMocId: Number(formValue.publishedMocId),
        submittedDate: dateTimeLocalToIsoUtc(formValue.submittedDate!.trim())!,
        entryNotes: formValue.entryNotes?.trim() || null,
        voteCount: Number(formValue.voteCount),
        isWinner: !!formValue.isWinner,
        isDisqualified: !!formValue.isDisqualified,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.buildChallengeEntryService.PutBuildChallengeEntry(buildChallengeEntrySubmitData.id, buildChallengeEntrySubmitData)
      : this.buildChallengeEntryService.PostBuildChallengeEntry(buildChallengeEntrySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBuildChallengeEntryData) => {

        this.buildChallengeEntryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Build Challenge Entry's detail page
          //
          this.buildChallengeEntryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.buildChallengeEntryForm.markAsUntouched();

          this.router.navigate(['/buildchallengeentries', savedBuildChallengeEntryData.id]);
          this.alertService.showMessage('Build Challenge Entry added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.buildChallengeEntryData = savedBuildChallengeEntryData;
          this.buildFormValues(this.buildChallengeEntryData);

          this.alertService.showMessage("Build Challenge Entry saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Build Challenge Entry.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Challenge Entry.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Challenge Entry could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBuildChallengeEntryReader(): boolean {
    return this.buildChallengeEntryService.userIsBMCBuildChallengeEntryReader();
  }

  public userIsBMCBuildChallengeEntryWriter(): boolean {
    return this.buildChallengeEntryService.userIsBMCBuildChallengeEntryWriter();
  }
}
