/*
   GENERATED FORM FOR THE SITESETTING TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SiteSetting table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to site-setting-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SiteSettingService, SiteSettingData, SiteSettingSubmitData } from '../../../community-data-services/site-setting.service';
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
interface SiteSettingFormValues {
  settingKey: string,
  settingValue: string | null,
  description: string | null,
  settingGroup: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-site-setting-detail',
  templateUrl: './site-setting-detail.component.html',
  styleUrls: ['./site-setting-detail.component.scss']
})

export class SiteSettingDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SiteSettingFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public siteSettingForm: FormGroup = this.fb.group({
        settingKey: ['', Validators.required],
        settingValue: [''],
        description: [''],
        settingGroup: [''],
        active: [true],
        deleted: [false],
      });


  public siteSettingId: string | null = null;
  public siteSettingData: SiteSettingData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  siteSettings$ = this.siteSettingService.GetSiteSettingList();

  private destroy$ = new Subject<void>();

  constructor(
    public siteSettingService: SiteSettingService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the siteSettingId from the route parameters
    this.siteSettingId = this.route.snapshot.paramMap.get('siteSettingId');

    if (this.siteSettingId === 'new' ||
        this.siteSettingId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.siteSettingData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.siteSettingForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.siteSettingForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Site Setting';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Site Setting';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.siteSettingForm.dirty) {
      return confirm('You have unsaved Site Setting changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.siteSettingId != null && this.siteSettingId !== 'new') {

      const id = parseInt(this.siteSettingId, 10);

      if (!isNaN(id)) {
        return { siteSettingId: id };
      }
    }

    return null;
  }


/*
  * Loads the SiteSetting data for the current siteSettingId.
  *
  * Fully respects the SiteSettingService caching strategy and error handling strategy.
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
    if (!this.siteSettingService.userIsCommunitySiteSettingReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SiteSettings.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate siteSettingId
    //
    if (!this.siteSettingId) {

      this.alertService.showMessage('No SiteSetting ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const siteSettingId = Number(this.siteSettingId);

    if (isNaN(siteSettingId) || siteSettingId <= 0) {

      this.alertService.showMessage(`Invalid Site Setting ID: "${this.siteSettingId}"`,
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
      // This is the most targeted way: clear only this SiteSetting + relations

      this.siteSettingService.ClearRecordCache(siteSettingId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.siteSettingService.GetSiteSetting(siteSettingId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (siteSettingData) => {

        //
        // Success path — siteSettingData can legitimately be null if 404'd but request succeeded
        //
        if (!siteSettingData) {

          this.handleSiteSettingNotFound(siteSettingId);

        } else {

          this.siteSettingData = siteSettingData;
          this.buildFormValues(this.siteSettingData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SiteSetting loaded successfully',
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
        this.handleSiteSettingLoadError(error, siteSettingId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSiteSettingNotFound(siteSettingId: number): void {

    this.siteSettingData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SiteSetting #${siteSettingId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSiteSettingLoadError(error: any, siteSettingId: number): void {

    let message = 'Failed to load Site Setting.';
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
          message = 'You do not have permission to view this Site Setting.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Site Setting #${siteSettingId} was not found.`;
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

    console.error(`Site Setting load failed (ID: ${siteSettingId})`, error);

    //
    // Reset UI to safe state
    //
    this.siteSettingData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(siteSettingData: SiteSettingData | null) {

    if (siteSettingData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.siteSettingForm.reset({
        settingKey: '',
        settingValue: '',
        description: '',
        settingGroup: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.siteSettingForm.reset({
        settingKey: siteSettingData.settingKey ?? '',
        settingValue: siteSettingData.settingValue ?? '',
        description: siteSettingData.description ?? '',
        settingGroup: siteSettingData.settingGroup ?? '',
        active: siteSettingData.active ?? true,
        deleted: siteSettingData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.siteSettingForm.markAsPristine();
    this.siteSettingForm.markAsUntouched();
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

    if (this.siteSettingService.userIsCommunitySiteSettingWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Site Settings", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.siteSettingForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.siteSettingForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.siteSettingForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const siteSettingSubmitData: SiteSettingSubmitData = {
        id: this.siteSettingData?.id || 0,
        settingKey: formValue.settingKey!.trim(),
        settingValue: formValue.settingValue?.trim() || null,
        description: formValue.description?.trim() || null,
        settingGroup: formValue.settingGroup?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.siteSettingService.PutSiteSetting(siteSettingSubmitData.id, siteSettingSubmitData)
      : this.siteSettingService.PostSiteSetting(siteSettingSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSiteSettingData) => {

        this.siteSettingService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Site Setting's detail page
          //
          this.siteSettingForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.siteSettingForm.markAsUntouched();

          this.router.navigate(['/sitesettings', savedSiteSettingData.id]);
          this.alertService.showMessage('Site Setting added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.siteSettingData = savedSiteSettingData;
          this.buildFormValues(this.siteSettingData);

          this.alertService.showMessage("Site Setting saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Site Setting.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Site Setting.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Site Setting could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsCommunitySiteSettingReader(): boolean {
    return this.siteSettingService.userIsCommunitySiteSettingReader();
  }

  public userIsCommunitySiteSettingWriter(): boolean {
    return this.siteSettingService.userIsCommunitySiteSettingWriter();
  }
}
