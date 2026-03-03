/*
   GENERATED FORM FOR THE BRICKSETUSERLINK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickSetUserLink table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-set-user-link-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickSetUserLinkService, BrickSetUserLinkData, BrickSetUserLinkSubmitData } from '../../../bmc-data-services/brick-set-user-link.service';
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
interface BrickSetUserLinkFormValues {
  brickSetUsername: string,
  encryptedUserHash: string,
  encryptedPassword: string | null,
  syncEnabled: boolean,
  syncDirection: string,
  lastSyncDate: string | null,
  lastPullDate: string | null,
  lastPushDate: string | null,
  lastSyncError: string | null,
  userHashStoredDate: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-brick-set-user-link-detail',
  templateUrl: './brick-set-user-link-detail.component.html',
  styleUrls: ['./brick-set-user-link-detail.component.scss']
})

export class BrickSetUserLinkDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickSetUserLinkFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickSetUserLinkForm: FormGroup = this.fb.group({
        brickSetUsername: ['', Validators.required],
        encryptedUserHash: ['', Validators.required],
        encryptedPassword: [''],
        syncEnabled: [false],
        syncDirection: ['', Validators.required],
        lastSyncDate: [''],
        lastPullDate: [''],
        lastPushDate: [''],
        lastSyncError: [''],
        userHashStoredDate: [''],
        active: [true],
        deleted: [false],
      });


  public brickSetUserLinkId: string | null = null;
  public brickSetUserLinkData: BrickSetUserLinkData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  brickSetUserLinks$ = this.brickSetUserLinkService.GetBrickSetUserLinkList();

  private destroy$ = new Subject<void>();

  constructor(
    public brickSetUserLinkService: BrickSetUserLinkService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the brickSetUserLinkId from the route parameters
    this.brickSetUserLinkId = this.route.snapshot.paramMap.get('brickSetUserLinkId');

    if (this.brickSetUserLinkId === 'new' ||
        this.brickSetUserLinkId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.brickSetUserLinkData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.brickSetUserLinkForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickSetUserLinkForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Brick Set User Link';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Brick Set User Link';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.brickSetUserLinkForm.dirty) {
      return confirm('You have unsaved Brick Set User Link changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.brickSetUserLinkId != null && this.brickSetUserLinkId !== 'new') {

      const id = parseInt(this.brickSetUserLinkId, 10);

      if (!isNaN(id)) {
        return { brickSetUserLinkId: id };
      }
    }

    return null;
  }


/*
  * Loads the BrickSetUserLink data for the current brickSetUserLinkId.
  *
  * Fully respects the BrickSetUserLinkService caching strategy and error handling strategy.
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
    if (!this.brickSetUserLinkService.userIsBMCBrickSetUserLinkReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BrickSetUserLinks.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate brickSetUserLinkId
    //
    if (!this.brickSetUserLinkId) {

      this.alertService.showMessage('No BrickSetUserLink ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const brickSetUserLinkId = Number(this.brickSetUserLinkId);

    if (isNaN(brickSetUserLinkId) || brickSetUserLinkId <= 0) {

      this.alertService.showMessage(`Invalid Brick Set User Link ID: "${this.brickSetUserLinkId}"`,
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
      // This is the most targeted way: clear only this BrickSetUserLink + relations

      this.brickSetUserLinkService.ClearRecordCache(brickSetUserLinkId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.brickSetUserLinkService.GetBrickSetUserLink(brickSetUserLinkId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (brickSetUserLinkData) => {

        //
        // Success path — brickSetUserLinkData can legitimately be null if 404'd but request succeeded
        //
        if (!brickSetUserLinkData) {

          this.handleBrickSetUserLinkNotFound(brickSetUserLinkId);

        } else {

          this.brickSetUserLinkData = brickSetUserLinkData;
          this.buildFormValues(this.brickSetUserLinkData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BrickSetUserLink loaded successfully',
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
        this.handleBrickSetUserLinkLoadError(error, brickSetUserLinkId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBrickSetUserLinkNotFound(brickSetUserLinkId: number): void {

    this.brickSetUserLinkData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BrickSetUserLink #${brickSetUserLinkId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBrickSetUserLinkLoadError(error: any, brickSetUserLinkId: number): void {

    let message = 'Failed to load Brick Set User Link.';
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
          message = 'You do not have permission to view this Brick Set User Link.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Brick Set User Link #${brickSetUserLinkId} was not found.`;
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

    console.error(`Brick Set User Link load failed (ID: ${brickSetUserLinkId})`, error);

    //
    // Reset UI to safe state
    //
    this.brickSetUserLinkData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(brickSetUserLinkData: BrickSetUserLinkData | null) {

    if (brickSetUserLinkData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickSetUserLinkForm.reset({
        brickSetUsername: '',
        encryptedUserHash: '',
        encryptedPassword: '',
        syncEnabled: false,
        syncDirection: '',
        lastSyncDate: '',
        lastPullDate: '',
        lastPushDate: '',
        lastSyncError: '',
        userHashStoredDate: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickSetUserLinkForm.reset({
        brickSetUsername: brickSetUserLinkData.brickSetUsername ?? '',
        encryptedUserHash: brickSetUserLinkData.encryptedUserHash ?? '',
        encryptedPassword: brickSetUserLinkData.encryptedPassword ?? '',
        syncEnabled: brickSetUserLinkData.syncEnabled ?? false,
        syncDirection: brickSetUserLinkData.syncDirection ?? '',
        lastSyncDate: isoUtcStringToDateTimeLocal(brickSetUserLinkData.lastSyncDate) ?? '',
        lastPullDate: isoUtcStringToDateTimeLocal(brickSetUserLinkData.lastPullDate) ?? '',
        lastPushDate: isoUtcStringToDateTimeLocal(brickSetUserLinkData.lastPushDate) ?? '',
        lastSyncError: brickSetUserLinkData.lastSyncError ?? '',
        userHashStoredDate: isoUtcStringToDateTimeLocal(brickSetUserLinkData.userHashStoredDate) ?? '',
        active: brickSetUserLinkData.active ?? true,
        deleted: brickSetUserLinkData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickSetUserLinkForm.markAsPristine();
    this.brickSetUserLinkForm.markAsUntouched();
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

    if (this.brickSetUserLinkService.userIsBMCBrickSetUserLinkWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Brick Set User Links", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.brickSetUserLinkForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickSetUserLinkForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickSetUserLinkForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickSetUserLinkSubmitData: BrickSetUserLinkSubmitData = {
        id: this.brickSetUserLinkData?.id || 0,
        brickSetUsername: formValue.brickSetUsername!.trim(),
        encryptedUserHash: formValue.encryptedUserHash!.trim(),
        encryptedPassword: formValue.encryptedPassword?.trim() || null,
        syncEnabled: !!formValue.syncEnabled,
        syncDirection: formValue.syncDirection!.trim(),
        lastSyncDate: formValue.lastSyncDate ? dateTimeLocalToIsoUtc(formValue.lastSyncDate.trim()) : null,
        lastPullDate: formValue.lastPullDate ? dateTimeLocalToIsoUtc(formValue.lastPullDate.trim()) : null,
        lastPushDate: formValue.lastPushDate ? dateTimeLocalToIsoUtc(formValue.lastPushDate.trim()) : null,
        lastSyncError: formValue.lastSyncError?.trim() || null,
        userHashStoredDate: formValue.userHashStoredDate ? dateTimeLocalToIsoUtc(formValue.userHashStoredDate.trim()) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.brickSetUserLinkService.PutBrickSetUserLink(brickSetUserLinkSubmitData.id, brickSetUserLinkSubmitData)
      : this.brickSetUserLinkService.PostBrickSetUserLink(brickSetUserLinkSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBrickSetUserLinkData) => {

        this.brickSetUserLinkService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Brick Set User Link's detail page
          //
          this.brickSetUserLinkForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.brickSetUserLinkForm.markAsUntouched();

          this.router.navigate(['/bricksetuserlinks', savedBrickSetUserLinkData.id]);
          this.alertService.showMessage('Brick Set User Link added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.brickSetUserLinkData = savedBrickSetUserLinkData;
          this.buildFormValues(this.brickSetUserLinkData);

          this.alertService.showMessage("Brick Set User Link saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Brick Set User Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Set User Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Set User Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBrickSetUserLinkReader(): boolean {
    return this.brickSetUserLinkService.userIsBMCBrickSetUserLinkReader();
  }

  public userIsBMCBrickSetUserLinkWriter(): boolean {
    return this.brickSetUserLinkService.userIsBMCBrickSetUserLinkWriter();
  }
}
