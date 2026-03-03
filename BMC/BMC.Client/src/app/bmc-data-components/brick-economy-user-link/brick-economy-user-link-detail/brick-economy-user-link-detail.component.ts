/*
   GENERATED FORM FOR THE BRICKECONOMYUSERLINK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickEconomyUserLink table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-economy-user-link-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickEconomyUserLinkService, BrickEconomyUserLinkData, BrickEconomyUserLinkSubmitData } from '../../../bmc-data-services/brick-economy-user-link.service';
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
interface BrickEconomyUserLinkFormValues {
  encryptedApiKey: string | null,
  syncEnabled: boolean,
  lastSyncDate: string | null,
  lastSyncError: string | null,
  dailyQuotaUsed: string | null,     // Stored as string for form input, converted to number on submit.
  quotaResetDate: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-brick-economy-user-link-detail',
  templateUrl: './brick-economy-user-link-detail.component.html',
  styleUrls: ['./brick-economy-user-link-detail.component.scss']
})

export class BrickEconomyUserLinkDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickEconomyUserLinkFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickEconomyUserLinkForm: FormGroup = this.fb.group({
        encryptedApiKey: [''],
        syncEnabled: [false],
        lastSyncDate: [''],
        lastSyncError: [''],
        dailyQuotaUsed: [''],
        quotaResetDate: [''],
        active: [true],
        deleted: [false],
      });


  public brickEconomyUserLinkId: string | null = null;
  public brickEconomyUserLinkData: BrickEconomyUserLinkData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  brickEconomyUserLinks$ = this.brickEconomyUserLinkService.GetBrickEconomyUserLinkList();

  private destroy$ = new Subject<void>();

  constructor(
    public brickEconomyUserLinkService: BrickEconomyUserLinkService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the brickEconomyUserLinkId from the route parameters
    this.brickEconomyUserLinkId = this.route.snapshot.paramMap.get('brickEconomyUserLinkId');

    if (this.brickEconomyUserLinkId === 'new' ||
        this.brickEconomyUserLinkId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.brickEconomyUserLinkData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.brickEconomyUserLinkForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickEconomyUserLinkForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Brick Economy User Link';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Brick Economy User Link';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.brickEconomyUserLinkForm.dirty) {
      return confirm('You have unsaved Brick Economy User Link changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.brickEconomyUserLinkId != null && this.brickEconomyUserLinkId !== 'new') {

      const id = parseInt(this.brickEconomyUserLinkId, 10);

      if (!isNaN(id)) {
        return { brickEconomyUserLinkId: id };
      }
    }

    return null;
  }


/*
  * Loads the BrickEconomyUserLink data for the current brickEconomyUserLinkId.
  *
  * Fully respects the BrickEconomyUserLinkService caching strategy and error handling strategy.
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
    if (!this.brickEconomyUserLinkService.userIsBMCBrickEconomyUserLinkReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BrickEconomyUserLinks.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate brickEconomyUserLinkId
    //
    if (!this.brickEconomyUserLinkId) {

      this.alertService.showMessage('No BrickEconomyUserLink ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const brickEconomyUserLinkId = Number(this.brickEconomyUserLinkId);

    if (isNaN(brickEconomyUserLinkId) || brickEconomyUserLinkId <= 0) {

      this.alertService.showMessage(`Invalid Brick Economy User Link ID: "${this.brickEconomyUserLinkId}"`,
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
      // This is the most targeted way: clear only this BrickEconomyUserLink + relations

      this.brickEconomyUserLinkService.ClearRecordCache(brickEconomyUserLinkId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.brickEconomyUserLinkService.GetBrickEconomyUserLink(brickEconomyUserLinkId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (brickEconomyUserLinkData) => {

        //
        // Success path — brickEconomyUserLinkData can legitimately be null if 404'd but request succeeded
        //
        if (!brickEconomyUserLinkData) {

          this.handleBrickEconomyUserLinkNotFound(brickEconomyUserLinkId);

        } else {

          this.brickEconomyUserLinkData = brickEconomyUserLinkData;
          this.buildFormValues(this.brickEconomyUserLinkData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BrickEconomyUserLink loaded successfully',
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
        this.handleBrickEconomyUserLinkLoadError(error, brickEconomyUserLinkId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBrickEconomyUserLinkNotFound(brickEconomyUserLinkId: number): void {

    this.brickEconomyUserLinkData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BrickEconomyUserLink #${brickEconomyUserLinkId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBrickEconomyUserLinkLoadError(error: any, brickEconomyUserLinkId: number): void {

    let message = 'Failed to load Brick Economy User Link.';
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
          message = 'You do not have permission to view this Brick Economy User Link.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Brick Economy User Link #${brickEconomyUserLinkId} was not found.`;
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

    console.error(`Brick Economy User Link load failed (ID: ${brickEconomyUserLinkId})`, error);

    //
    // Reset UI to safe state
    //
    this.brickEconomyUserLinkData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(brickEconomyUserLinkData: BrickEconomyUserLinkData | null) {

    if (brickEconomyUserLinkData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickEconomyUserLinkForm.reset({
        encryptedApiKey: '',
        syncEnabled: false,
        lastSyncDate: '',
        lastSyncError: '',
        dailyQuotaUsed: '',
        quotaResetDate: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickEconomyUserLinkForm.reset({
        encryptedApiKey: brickEconomyUserLinkData.encryptedApiKey ?? '',
        syncEnabled: brickEconomyUserLinkData.syncEnabled ?? false,
        lastSyncDate: isoUtcStringToDateTimeLocal(brickEconomyUserLinkData.lastSyncDate) ?? '',
        lastSyncError: brickEconomyUserLinkData.lastSyncError ?? '',
        dailyQuotaUsed: brickEconomyUserLinkData.dailyQuotaUsed?.toString() ?? '',
        quotaResetDate: isoUtcStringToDateTimeLocal(brickEconomyUserLinkData.quotaResetDate) ?? '',
        active: brickEconomyUserLinkData.active ?? true,
        deleted: brickEconomyUserLinkData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickEconomyUserLinkForm.markAsPristine();
    this.brickEconomyUserLinkForm.markAsUntouched();
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

    if (this.brickEconomyUserLinkService.userIsBMCBrickEconomyUserLinkWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Brick Economy User Links", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.brickEconomyUserLinkForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickEconomyUserLinkForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickEconomyUserLinkForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickEconomyUserLinkSubmitData: BrickEconomyUserLinkSubmitData = {
        id: this.brickEconomyUserLinkData?.id || 0,
        encryptedApiKey: formValue.encryptedApiKey?.trim() || null,
        syncEnabled: !!formValue.syncEnabled,
        lastSyncDate: formValue.lastSyncDate ? dateTimeLocalToIsoUtc(formValue.lastSyncDate.trim()) : null,
        lastSyncError: formValue.lastSyncError?.trim() || null,
        dailyQuotaUsed: formValue.dailyQuotaUsed ? Number(formValue.dailyQuotaUsed) : null,
        quotaResetDate: formValue.quotaResetDate ? dateTimeLocalToIsoUtc(formValue.quotaResetDate.trim()) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.brickEconomyUserLinkService.PutBrickEconomyUserLink(brickEconomyUserLinkSubmitData.id, brickEconomyUserLinkSubmitData)
      : this.brickEconomyUserLinkService.PostBrickEconomyUserLink(brickEconomyUserLinkSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBrickEconomyUserLinkData) => {

        this.brickEconomyUserLinkService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Brick Economy User Link's detail page
          //
          this.brickEconomyUserLinkForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.brickEconomyUserLinkForm.markAsUntouched();

          this.router.navigate(['/brickeconomyuserlinks', savedBrickEconomyUserLinkData.id]);
          this.alertService.showMessage('Brick Economy User Link added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.brickEconomyUserLinkData = savedBrickEconomyUserLinkData;
          this.buildFormValues(this.brickEconomyUserLinkData);

          this.alertService.showMessage("Brick Economy User Link saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Brick Economy User Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Economy User Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Economy User Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBrickEconomyUserLinkReader(): boolean {
    return this.brickEconomyUserLinkService.userIsBMCBrickEconomyUserLinkReader();
  }

  public userIsBMCBrickEconomyUserLinkWriter(): boolean {
    return this.brickEconomyUserLinkService.userIsBMCBrickEconomyUserLinkWriter();
  }
}
