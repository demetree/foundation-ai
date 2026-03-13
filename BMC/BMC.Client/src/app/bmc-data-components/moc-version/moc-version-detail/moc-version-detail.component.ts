/*
   GENERATED FORM FOR THE MOCVERSION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MocVersion table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to moc-version-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MocVersionService, MocVersionData, MocVersionSubmitData } from '../../../bmc-data-services/moc-version.service';
import { PublishedMocService } from '../../../bmc-data-services/published-moc.service';
import { MocVersionChangeHistoryService } from '../../../bmc-data-services/moc-version-change-history.service';
import { MocForkService } from '../../../bmc-data-services/moc-fork.service';
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
interface MocVersionFormValues {
  publishedMocId: number | bigint,       // For FK link number
  commitMessage: string,
  mpdSnapshot: string,
  partCount: string | null,     // Stored as string for form input, converted to number on submit.
  addedPartCount: string | null,     // Stored as string for form input, converted to number on submit.
  removedPartCount: string | null,     // Stored as string for form input, converted to number on submit.
  modifiedPartCount: string | null,     // Stored as string for form input, converted to number on submit.
  snapshotDate: string,
  authorTenantGuid: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-moc-version-detail',
  templateUrl: './moc-version-detail.component.html',
  styleUrls: ['./moc-version-detail.component.scss']
})

export class MocVersionDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MocVersionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public mocVersionForm: FormGroup = this.fb.group({
        publishedMocId: [null, Validators.required],
        commitMessage: ['', Validators.required],
        mpdSnapshot: ['', Validators.required],
        partCount: [''],
        addedPartCount: [''],
        removedPartCount: [''],
        modifiedPartCount: [''],
        snapshotDate: ['', Validators.required],
        authorTenantGuid: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public mocVersionId: string | null = null;
  public mocVersionData: MocVersionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  mocVersions$ = this.mocVersionService.GetMocVersionList();
  public publishedMocs$ = this.publishedMocService.GetPublishedMocList();
  public mocVersionChangeHistories$ = this.mocVersionChangeHistoryService.GetMocVersionChangeHistoryList();
  public mocForks$ = this.mocForkService.GetMocForkList();

  private destroy$ = new Subject<void>();

  constructor(
    public mocVersionService: MocVersionService,
    public publishedMocService: PublishedMocService,
    public mocVersionChangeHistoryService: MocVersionChangeHistoryService,
    public mocForkService: MocForkService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the mocVersionId from the route parameters
    this.mocVersionId = this.route.snapshot.paramMap.get('mocVersionId');

    if (this.mocVersionId === 'new' ||
        this.mocVersionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.mocVersionData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.mocVersionForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.mocVersionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Moc Version';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Moc Version';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.mocVersionForm.dirty) {
      return confirm('You have unsaved Moc Version changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.mocVersionId != null && this.mocVersionId !== 'new') {

      const id = parseInt(this.mocVersionId, 10);

      if (!isNaN(id)) {
        return { mocVersionId: id };
      }
    }

    return null;
  }


/*
  * Loads the MocVersion data for the current mocVersionId.
  *
  * Fully respects the MocVersionService caching strategy and error handling strategy.
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
    if (!this.mocVersionService.userIsBMCMocVersionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read MocVersions.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate mocVersionId
    //
    if (!this.mocVersionId) {

      this.alertService.showMessage('No MocVersion ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const mocVersionId = Number(this.mocVersionId);

    if (isNaN(mocVersionId) || mocVersionId <= 0) {

      this.alertService.showMessage(`Invalid Moc Version ID: "${this.mocVersionId}"`,
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
      // This is the most targeted way: clear only this MocVersion + relations

      this.mocVersionService.ClearRecordCache(mocVersionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.mocVersionService.GetMocVersion(mocVersionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (mocVersionData) => {

        //
        // Success path — mocVersionData can legitimately be null if 404'd but request succeeded
        //
        if (!mocVersionData) {

          this.handleMocVersionNotFound(mocVersionId);

        } else {

          this.mocVersionData = mocVersionData;
          this.buildFormValues(this.mocVersionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'MocVersion loaded successfully',
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
        this.handleMocVersionLoadError(error, mocVersionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleMocVersionNotFound(mocVersionId: number): void {

    this.mocVersionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `MocVersion #${mocVersionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleMocVersionLoadError(error: any, mocVersionId: number): void {

    let message = 'Failed to load Moc Version.';
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
          message = 'You do not have permission to view this Moc Version.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Moc Version #${mocVersionId} was not found.`;
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

    console.error(`Moc Version load failed (ID: ${mocVersionId})`, error);

    //
    // Reset UI to safe state
    //
    this.mocVersionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(mocVersionData: MocVersionData | null) {

    if (mocVersionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.mocVersionForm.reset({
        publishedMocId: null,
        commitMessage: '',
        mpdSnapshot: '',
        partCount: '',
        addedPartCount: '',
        removedPartCount: '',
        modifiedPartCount: '',
        snapshotDate: '',
        authorTenantGuid: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.mocVersionForm.reset({
        publishedMocId: mocVersionData.publishedMocId,
        commitMessage: mocVersionData.commitMessage ?? '',
        mpdSnapshot: mocVersionData.mpdSnapshot ?? '',
        partCount: mocVersionData.partCount?.toString() ?? '',
        addedPartCount: mocVersionData.addedPartCount?.toString() ?? '',
        removedPartCount: mocVersionData.removedPartCount?.toString() ?? '',
        modifiedPartCount: mocVersionData.modifiedPartCount?.toString() ?? '',
        snapshotDate: isoUtcStringToDateTimeLocal(mocVersionData.snapshotDate) ?? '',
        authorTenantGuid: mocVersionData.authorTenantGuid ?? '',
        versionNumber: mocVersionData.versionNumber?.toString() ?? '',
        active: mocVersionData.active ?? true,
        deleted: mocVersionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.mocVersionForm.markAsPristine();
    this.mocVersionForm.markAsUntouched();
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

    if (this.mocVersionService.userIsBMCMocVersionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Moc Versions", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.mocVersionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.mocVersionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.mocVersionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const mocVersionSubmitData: MocVersionSubmitData = {
        id: this.mocVersionData?.id || 0,
        publishedMocId: Number(formValue.publishedMocId),
        commitMessage: formValue.commitMessage!.trim(),
        mpdSnapshot: formValue.mpdSnapshot!.trim(),
        partCount: formValue.partCount ? Number(formValue.partCount) : null,
        addedPartCount: formValue.addedPartCount ? Number(formValue.addedPartCount) : null,
        removedPartCount: formValue.removedPartCount ? Number(formValue.removedPartCount) : null,
        modifiedPartCount: formValue.modifiedPartCount ? Number(formValue.modifiedPartCount) : null,
        snapshotDate: dateTimeLocalToIsoUtc(formValue.snapshotDate!.trim())!,
        authorTenantGuid: formValue.authorTenantGuid!.trim(),
        versionNumber: this.mocVersionData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.mocVersionService.PutMocVersion(mocVersionSubmitData.id, mocVersionSubmitData)
      : this.mocVersionService.PostMocVersion(mocVersionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedMocVersionData) => {

        this.mocVersionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Moc Version's detail page
          //
          this.mocVersionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.mocVersionForm.markAsUntouched();

          this.router.navigate(['/mocversions', savedMocVersionData.id]);
          this.alertService.showMessage('Moc Version added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.mocVersionData = savedMocVersionData;
          this.buildFormValues(this.mocVersionData);

          this.alertService.showMessage("Moc Version saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Moc Version.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Moc Version.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Moc Version could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCMocVersionReader(): boolean {
    return this.mocVersionService.userIsBMCMocVersionReader();
  }

  public userIsBMCMocVersionWriter(): boolean {
    return this.mocVersionService.userIsBMCMocVersionWriter();
  }
}
