/*
   GENERATED FORM FOR THE BRICKOWLUSERLINK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickOwlUserLink table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-owl-user-link-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickOwlUserLinkService, BrickOwlUserLinkData, BrickOwlUserLinkSubmitData } from '../../../bmc-data-services/brick-owl-user-link.service';
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
interface BrickOwlUserLinkFormValues {
  encryptedApiKey: string | null,
  syncEnabled: boolean,
  syncDirection: string | null,
  lastSyncDate: string | null,
  lastPullDate: string | null,
  lastPushDate: string | null,
  lastSyncError: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-brick-owl-user-link-detail',
  templateUrl: './brick-owl-user-link-detail.component.html',
  styleUrls: ['./brick-owl-user-link-detail.component.scss']
})

export class BrickOwlUserLinkDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickOwlUserLinkFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickOwlUserLinkForm: FormGroup = this.fb.group({
        encryptedApiKey: [''],
        syncEnabled: [false],
        syncDirection: [''],
        lastSyncDate: [''],
        lastPullDate: [''],
        lastPushDate: [''],
        lastSyncError: [''],
        active: [true],
        deleted: [false],
      });


  public brickOwlUserLinkId: string | null = null;
  public brickOwlUserLinkData: BrickOwlUserLinkData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  brickOwlUserLinks$ = this.brickOwlUserLinkService.GetBrickOwlUserLinkList();

  private destroy$ = new Subject<void>();

  constructor(
    public brickOwlUserLinkService: BrickOwlUserLinkService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the brickOwlUserLinkId from the route parameters
    this.brickOwlUserLinkId = this.route.snapshot.paramMap.get('brickOwlUserLinkId');

    if (this.brickOwlUserLinkId === 'new' ||
        this.brickOwlUserLinkId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.brickOwlUserLinkData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.brickOwlUserLinkForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickOwlUserLinkForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Brick Owl User Link';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Brick Owl User Link';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.brickOwlUserLinkForm.dirty) {
      return confirm('You have unsaved Brick Owl User Link changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.brickOwlUserLinkId != null && this.brickOwlUserLinkId !== 'new') {

      const id = parseInt(this.brickOwlUserLinkId, 10);

      if (!isNaN(id)) {
        return { brickOwlUserLinkId: id };
      }
    }

    return null;
  }


/*
  * Loads the BrickOwlUserLink data for the current brickOwlUserLinkId.
  *
  * Fully respects the BrickOwlUserLinkService caching strategy and error handling strategy.
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
    if (!this.brickOwlUserLinkService.userIsBMCBrickOwlUserLinkReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BrickOwlUserLinks.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate brickOwlUserLinkId
    //
    if (!this.brickOwlUserLinkId) {

      this.alertService.showMessage('No BrickOwlUserLink ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const brickOwlUserLinkId = Number(this.brickOwlUserLinkId);

    if (isNaN(brickOwlUserLinkId) || brickOwlUserLinkId <= 0) {

      this.alertService.showMessage(`Invalid Brick Owl User Link ID: "${this.brickOwlUserLinkId}"`,
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
      // This is the most targeted way: clear only this BrickOwlUserLink + relations

      this.brickOwlUserLinkService.ClearRecordCache(brickOwlUserLinkId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.brickOwlUserLinkService.GetBrickOwlUserLink(brickOwlUserLinkId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (brickOwlUserLinkData) => {

        //
        // Success path — brickOwlUserLinkData can legitimately be null if 404'd but request succeeded
        //
        if (!brickOwlUserLinkData) {

          this.handleBrickOwlUserLinkNotFound(brickOwlUserLinkId);

        } else {

          this.brickOwlUserLinkData = brickOwlUserLinkData;
          this.buildFormValues(this.brickOwlUserLinkData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BrickOwlUserLink loaded successfully',
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
        this.handleBrickOwlUserLinkLoadError(error, brickOwlUserLinkId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBrickOwlUserLinkNotFound(brickOwlUserLinkId: number): void {

    this.brickOwlUserLinkData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BrickOwlUserLink #${brickOwlUserLinkId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBrickOwlUserLinkLoadError(error: any, brickOwlUserLinkId: number): void {

    let message = 'Failed to load Brick Owl User Link.';
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
          message = 'You do not have permission to view this Brick Owl User Link.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Brick Owl User Link #${brickOwlUserLinkId} was not found.`;
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

    console.error(`Brick Owl User Link load failed (ID: ${brickOwlUserLinkId})`, error);

    //
    // Reset UI to safe state
    //
    this.brickOwlUserLinkData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(brickOwlUserLinkData: BrickOwlUserLinkData | null) {

    if (brickOwlUserLinkData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickOwlUserLinkForm.reset({
        encryptedApiKey: '',
        syncEnabled: false,
        syncDirection: '',
        lastSyncDate: '',
        lastPullDate: '',
        lastPushDate: '',
        lastSyncError: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickOwlUserLinkForm.reset({
        encryptedApiKey: brickOwlUserLinkData.encryptedApiKey ?? '',
        syncEnabled: brickOwlUserLinkData.syncEnabled ?? false,
        syncDirection: brickOwlUserLinkData.syncDirection ?? '',
        lastSyncDate: isoUtcStringToDateTimeLocal(brickOwlUserLinkData.lastSyncDate) ?? '',
        lastPullDate: isoUtcStringToDateTimeLocal(brickOwlUserLinkData.lastPullDate) ?? '',
        lastPushDate: isoUtcStringToDateTimeLocal(brickOwlUserLinkData.lastPushDate) ?? '',
        lastSyncError: brickOwlUserLinkData.lastSyncError ?? '',
        active: brickOwlUserLinkData.active ?? true,
        deleted: brickOwlUserLinkData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickOwlUserLinkForm.markAsPristine();
    this.brickOwlUserLinkForm.markAsUntouched();
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

    if (this.brickOwlUserLinkService.userIsBMCBrickOwlUserLinkWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Brick Owl User Links", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.brickOwlUserLinkForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickOwlUserLinkForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickOwlUserLinkForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickOwlUserLinkSubmitData: BrickOwlUserLinkSubmitData = {
        id: this.brickOwlUserLinkData?.id || 0,
        encryptedApiKey: formValue.encryptedApiKey?.trim() || null,
        syncEnabled: !!formValue.syncEnabled,
        syncDirection: formValue.syncDirection?.trim() || null,
        lastSyncDate: formValue.lastSyncDate ? dateTimeLocalToIsoUtc(formValue.lastSyncDate.trim()) : null,
        lastPullDate: formValue.lastPullDate ? dateTimeLocalToIsoUtc(formValue.lastPullDate.trim()) : null,
        lastPushDate: formValue.lastPushDate ? dateTimeLocalToIsoUtc(formValue.lastPushDate.trim()) : null,
        lastSyncError: formValue.lastSyncError?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.brickOwlUserLinkService.PutBrickOwlUserLink(brickOwlUserLinkSubmitData.id, brickOwlUserLinkSubmitData)
      : this.brickOwlUserLinkService.PostBrickOwlUserLink(brickOwlUserLinkSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBrickOwlUserLinkData) => {

        this.brickOwlUserLinkService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Brick Owl User Link's detail page
          //
          this.brickOwlUserLinkForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.brickOwlUserLinkForm.markAsUntouched();

          this.router.navigate(['/brickowluserlinks', savedBrickOwlUserLinkData.id]);
          this.alertService.showMessage('Brick Owl User Link added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.brickOwlUserLinkData = savedBrickOwlUserLinkData;
          this.buildFormValues(this.brickOwlUserLinkData);

          this.alertService.showMessage("Brick Owl User Link saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Brick Owl User Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Owl User Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Owl User Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBrickOwlUserLinkReader(): boolean {
    return this.brickOwlUserLinkService.userIsBMCBrickOwlUserLinkReader();
  }

  public userIsBMCBrickOwlUserLinkWriter(): boolean {
    return this.brickOwlUserLinkService.userIsBMCBrickOwlUserLinkWriter();
  }
}
