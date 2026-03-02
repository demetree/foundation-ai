/*
   GENERATED FORM FOR THE REBRICKABLEUSERLINK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RebrickableUserLink table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to rebrickable-user-link-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RebrickableUserLinkService, RebrickableUserLinkData, RebrickableUserLinkSubmitData } from '../../../bmc-data-services/rebrickable-user-link.service';
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
interface RebrickableUserLinkFormValues {
  rebrickableUsername: string,
  encryptedApiToken: string,
  authMode: string,
  encryptedPassword: string | null,
  syncEnabled: boolean,
  syncDirectionFlags: string,
  pullIntervalMinutes: string | null,     // Stored as string for form input, converted to number on submit.
  lastSyncDate: string | null,
  lastPullDate: string | null,
  lastPushDate: string | null,
  lastSyncError: string | null,
  tokenExpiryDays: string | null,     // Stored as string for form input, converted to number on submit.
  tokenStoredDate: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-rebrickable-user-link-detail',
  templateUrl: './rebrickable-user-link-detail.component.html',
  styleUrls: ['./rebrickable-user-link-detail.component.scss']
})

export class RebrickableUserLinkDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<RebrickableUserLinkFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public rebrickableUserLinkForm: FormGroup = this.fb.group({
        rebrickableUsername: ['', Validators.required],
        encryptedApiToken: ['', Validators.required],
        authMode: ['', Validators.required],
        encryptedPassword: [''],
        syncEnabled: [false],
        syncDirectionFlags: ['', Validators.required],
        pullIntervalMinutes: [''],
        lastSyncDate: [''],
        lastPullDate: [''],
        lastPushDate: [''],
        lastSyncError: [''],
        tokenExpiryDays: [''],
        tokenStoredDate: [''],
        active: [true],
        deleted: [false],
      });


  public rebrickableUserLinkId: string | null = null;
  public rebrickableUserLinkData: RebrickableUserLinkData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  rebrickableUserLinks$ = this.rebrickableUserLinkService.GetRebrickableUserLinkList();

  private destroy$ = new Subject<void>();

  constructor(
    public rebrickableUserLinkService: RebrickableUserLinkService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the rebrickableUserLinkId from the route parameters
    this.rebrickableUserLinkId = this.route.snapshot.paramMap.get('rebrickableUserLinkId');

    if (this.rebrickableUserLinkId === 'new' ||
        this.rebrickableUserLinkId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.rebrickableUserLinkData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.rebrickableUserLinkForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.rebrickableUserLinkForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Rebrickable User Link';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Rebrickable User Link';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.rebrickableUserLinkForm.dirty) {
      return confirm('You have unsaved Rebrickable User Link changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.rebrickableUserLinkId != null && this.rebrickableUserLinkId !== 'new') {

      const id = parseInt(this.rebrickableUserLinkId, 10);

      if (!isNaN(id)) {
        return { rebrickableUserLinkId: id };
      }
    }

    return null;
  }


/*
  * Loads the RebrickableUserLink data for the current rebrickableUserLinkId.
  *
  * Fully respects the RebrickableUserLinkService caching strategy and error handling strategy.
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
    if (!this.rebrickableUserLinkService.userIsBMCRebrickableUserLinkReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read RebrickableUserLinks.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate rebrickableUserLinkId
    //
    if (!this.rebrickableUserLinkId) {

      this.alertService.showMessage('No RebrickableUserLink ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const rebrickableUserLinkId = Number(this.rebrickableUserLinkId);

    if (isNaN(rebrickableUserLinkId) || rebrickableUserLinkId <= 0) {

      this.alertService.showMessage(`Invalid Rebrickable User Link ID: "${this.rebrickableUserLinkId}"`,
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
      // This is the most targeted way: clear only this RebrickableUserLink + relations

      this.rebrickableUserLinkService.ClearRecordCache(rebrickableUserLinkId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.rebrickableUserLinkService.GetRebrickableUserLink(rebrickableUserLinkId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (rebrickableUserLinkData) => {

        //
        // Success path — rebrickableUserLinkData can legitimately be null if 404'd but request succeeded
        //
        if (!rebrickableUserLinkData) {

          this.handleRebrickableUserLinkNotFound(rebrickableUserLinkId);

        } else {

          this.rebrickableUserLinkData = rebrickableUserLinkData;
          this.buildFormValues(this.rebrickableUserLinkData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'RebrickableUserLink loaded successfully',
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
        this.handleRebrickableUserLinkLoadError(error, rebrickableUserLinkId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleRebrickableUserLinkNotFound(rebrickableUserLinkId: number): void {

    this.rebrickableUserLinkData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `RebrickableUserLink #${rebrickableUserLinkId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleRebrickableUserLinkLoadError(error: any, rebrickableUserLinkId: number): void {

    let message = 'Failed to load Rebrickable User Link.';
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
          message = 'You do not have permission to view this Rebrickable User Link.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Rebrickable User Link #${rebrickableUserLinkId} was not found.`;
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

    console.error(`Rebrickable User Link load failed (ID: ${rebrickableUserLinkId})`, error);

    //
    // Reset UI to safe state
    //
    this.rebrickableUserLinkData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(rebrickableUserLinkData: RebrickableUserLinkData | null) {

    if (rebrickableUserLinkData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.rebrickableUserLinkForm.reset({
        rebrickableUsername: '',
        encryptedApiToken: '',
        authMode: '',
        encryptedPassword: '',
        syncEnabled: false,
        syncDirectionFlags: '',
        pullIntervalMinutes: '',
        lastSyncDate: '',
        lastPullDate: '',
        lastPushDate: '',
        lastSyncError: '',
        tokenExpiryDays: '',
        tokenStoredDate: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.rebrickableUserLinkForm.reset({
        rebrickableUsername: rebrickableUserLinkData.rebrickableUsername ?? '',
        encryptedApiToken: rebrickableUserLinkData.encryptedApiToken ?? '',
        authMode: rebrickableUserLinkData.authMode ?? '',
        encryptedPassword: rebrickableUserLinkData.encryptedPassword ?? '',
        syncEnabled: rebrickableUserLinkData.syncEnabled ?? false,
        syncDirectionFlags: rebrickableUserLinkData.syncDirectionFlags ?? '',
        pullIntervalMinutes: rebrickableUserLinkData.pullIntervalMinutes?.toString() ?? '',
        lastSyncDate: isoUtcStringToDateTimeLocal(rebrickableUserLinkData.lastSyncDate) ?? '',
        lastPullDate: isoUtcStringToDateTimeLocal(rebrickableUserLinkData.lastPullDate) ?? '',
        lastPushDate: isoUtcStringToDateTimeLocal(rebrickableUserLinkData.lastPushDate) ?? '',
        lastSyncError: rebrickableUserLinkData.lastSyncError ?? '',
        tokenExpiryDays: rebrickableUserLinkData.tokenExpiryDays?.toString() ?? '',
        tokenStoredDate: isoUtcStringToDateTimeLocal(rebrickableUserLinkData.tokenStoredDate) ?? '',
        active: rebrickableUserLinkData.active ?? true,
        deleted: rebrickableUserLinkData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.rebrickableUserLinkForm.markAsPristine();
    this.rebrickableUserLinkForm.markAsUntouched();
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

    if (this.rebrickableUserLinkService.userIsBMCRebrickableUserLinkWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Rebrickable User Links", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.rebrickableUserLinkForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.rebrickableUserLinkForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.rebrickableUserLinkForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const rebrickableUserLinkSubmitData: RebrickableUserLinkSubmitData = {
        id: this.rebrickableUserLinkData?.id || 0,
        rebrickableUsername: formValue.rebrickableUsername!.trim(),
        encryptedApiToken: formValue.encryptedApiToken!.trim(),
        authMode: formValue.authMode!.trim(),
        encryptedPassword: formValue.encryptedPassword?.trim() || null,
        syncEnabled: !!formValue.syncEnabled,
        syncDirectionFlags: formValue.syncDirectionFlags!.trim(),
        pullIntervalMinutes: formValue.pullIntervalMinutes ? Number(formValue.pullIntervalMinutes) : null,
        lastSyncDate: formValue.lastSyncDate ? dateTimeLocalToIsoUtc(formValue.lastSyncDate.trim()) : null,
        lastPullDate: formValue.lastPullDate ? dateTimeLocalToIsoUtc(formValue.lastPullDate.trim()) : null,
        lastPushDate: formValue.lastPushDate ? dateTimeLocalToIsoUtc(formValue.lastPushDate.trim()) : null,
        lastSyncError: formValue.lastSyncError?.trim() || null,
        tokenExpiryDays: formValue.tokenExpiryDays ? Number(formValue.tokenExpiryDays) : null,
        tokenStoredDate: formValue.tokenStoredDate ? dateTimeLocalToIsoUtc(formValue.tokenStoredDate.trim()) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.rebrickableUserLinkService.PutRebrickableUserLink(rebrickableUserLinkSubmitData.id, rebrickableUserLinkSubmitData)
      : this.rebrickableUserLinkService.PostRebrickableUserLink(rebrickableUserLinkSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedRebrickableUserLinkData) => {

        this.rebrickableUserLinkService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Rebrickable User Link's detail page
          //
          this.rebrickableUserLinkForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.rebrickableUserLinkForm.markAsUntouched();

          this.router.navigate(['/rebrickableuserlinks', savedRebrickableUserLinkData.id]);
          this.alertService.showMessage('Rebrickable User Link added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.rebrickableUserLinkData = savedRebrickableUserLinkData;
          this.buildFormValues(this.rebrickableUserLinkData);

          this.alertService.showMessage("Rebrickable User Link saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Rebrickable User Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Rebrickable User Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Rebrickable User Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCRebrickableUserLinkReader(): boolean {
    return this.rebrickableUserLinkService.userIsBMCRebrickableUserLinkReader();
  }

  public userIsBMCRebrickableUserLinkWriter(): boolean {
    return this.rebrickableUserLinkService.userIsBMCRebrickableUserLinkWriter();
  }
}
