/*
   GENERATED FORM FOR THE TENANTPROFILE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TenantProfile table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to tenant-profile-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TenantProfileService, TenantProfileData, TenantProfileSubmitData } from '../../../scheduler-data-services/tenant-profile.service';
import { StateProvinceService } from '../../../scheduler-data-services/state-province.service';
import { CountryService } from '../../../scheduler-data-services/country.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { TenantProfileChangeHistoryService } from '../../../scheduler-data-services/tenant-profile-change-history.service';
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
interface TenantProfileFormValues {
  name: string,
  description: string | null,
  companyLogoFileName: string | null,
  companyLogoSize: string | null,     // Stored as string for form input, converted to number on submit.
  companyLogoData: string | null,
  companyLogoMimeType: string | null,
  addressLine1: string | null,
  addressLine2: string | null,
  addressLine3: string | null,
  city: string | null,
  postalCode: string | null,
  stateProvinceId: number | bigint | null,       // For FK link number
  countryId: number | bigint | null,       // For FK link number
  timeZoneId: number | bigint | null,       // For FK link number
  phoneNumber: string | null,
  email: string | null,
  website: string | null,
  latitude: string | null,     // Stored as string for form input, converted to number on submit.
  longitude: string | null,     // Stored as string for form input, converted to number on submit.
  primaryColor: string | null,
  secondaryColor: string | null,
  displaysMetric: boolean,
  displaysUSTerms: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-tenant-profile-detail',
  templateUrl: './tenant-profile-detail.component.html',
  styleUrls: ['./tenant-profile-detail.component.scss']
})

export class TenantProfileDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TenantProfileFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public tenantProfileForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        companyLogoFileName: [''],
        companyLogoSize: [''],
        companyLogoData: [''],
        companyLogoMimeType: [''],
        addressLine1: [''],
        addressLine2: [''],
        addressLine3: [''],
        city: [''],
        postalCode: [''],
        stateProvinceId: [null],
        countryId: [null],
        timeZoneId: [null],
        phoneNumber: [''],
        email: [''],
        website: [''],
        latitude: [''],
        longitude: [''],
        primaryColor: [''],
        secondaryColor: [''],
        displaysMetric: [false],
        displaysUSTerms: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public tenantProfileId: string | null = null;
  public tenantProfileData: TenantProfileData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  tenantProfiles$ = this.tenantProfileService.GetTenantProfileList();
  public stateProvinces$ = this.stateProvinceService.GetStateProvinceList();
  public countries$ = this.countryService.GetCountryList();
  public timeZones$ = this.timeZoneService.GetTimeZoneList();
  public tenantProfileChangeHistories$ = this.tenantProfileChangeHistoryService.GetTenantProfileChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public tenantProfileService: TenantProfileService,
    public stateProvinceService: StateProvinceService,
    public countryService: CountryService,
    public timeZoneService: TimeZoneService,
    public tenantProfileChangeHistoryService: TenantProfileChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the tenantProfileId from the route parameters
    this.tenantProfileId = this.route.snapshot.paramMap.get('tenantProfileId');

    if (this.tenantProfileId === 'new' ||
        this.tenantProfileId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.tenantProfileData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.tenantProfileForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.tenantProfileForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Tenant Profile';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Tenant Profile';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.tenantProfileForm.dirty) {
      return confirm('You have unsaved Tenant Profile changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.tenantProfileId != null && this.tenantProfileId !== 'new') {

      const id = parseInt(this.tenantProfileId, 10);

      if (!isNaN(id)) {
        return { tenantProfileId: id };
      }
    }

    return null;
  }


/*
  * Loads the TenantProfile data for the current tenantProfileId.
  *
  * Fully respects the TenantProfileService caching strategy and error handling strategy.
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
    if (!this.tenantProfileService.userIsSchedulerTenantProfileReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read TenantProfiles.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate tenantProfileId
    //
    if (!this.tenantProfileId) {

      this.alertService.showMessage('No TenantProfile ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const tenantProfileId = Number(this.tenantProfileId);

    if (isNaN(tenantProfileId) || tenantProfileId <= 0) {

      this.alertService.showMessage(`Invalid Tenant Profile ID: "${this.tenantProfileId}"`,
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
      // This is the most targeted way: clear only this TenantProfile + relations

      this.tenantProfileService.ClearRecordCache(tenantProfileId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.tenantProfileService.GetTenantProfile(tenantProfileId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (tenantProfileData) => {

        //
        // Success path — tenantProfileData can legitimately be null if 404'd but request succeeded
        //
        if (!tenantProfileData) {

          this.handleTenantProfileNotFound(tenantProfileId);

        } else {

          this.tenantProfileData = tenantProfileData;
          this.buildFormValues(this.tenantProfileData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'TenantProfile loaded successfully',
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
        this.handleTenantProfileLoadError(error, tenantProfileId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleTenantProfileNotFound(tenantProfileId: number): void {

    this.tenantProfileData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `TenantProfile #${tenantProfileId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleTenantProfileLoadError(error: any, tenantProfileId: number): void {

    let message = 'Failed to load Tenant Profile.';
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
          message = 'You do not have permission to view this Tenant Profile.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Tenant Profile #${tenantProfileId} was not found.`;
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

    console.error(`Tenant Profile load failed (ID: ${tenantProfileId})`, error);

    //
    // Reset UI to safe state
    //
    this.tenantProfileData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(tenantProfileData: TenantProfileData | null) {

    if (tenantProfileData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.tenantProfileForm.reset({
        name: '',
        description: '',
        companyLogoFileName: '',
        companyLogoSize: '',
        companyLogoData: '',
        companyLogoMimeType: '',
        addressLine1: '',
        addressLine2: '',
        addressLine3: '',
        city: '',
        postalCode: '',
        stateProvinceId: null,
        countryId: null,
        timeZoneId: null,
        phoneNumber: '',
        email: '',
        website: '',
        latitude: '',
        longitude: '',
        primaryColor: '',
        secondaryColor: '',
        displaysMetric: false,
        displaysUSTerms: false,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.tenantProfileForm.reset({
        name: tenantProfileData.name ?? '',
        description: tenantProfileData.description ?? '',
        companyLogoFileName: tenantProfileData.companyLogoFileName ?? '',
        companyLogoSize: tenantProfileData.companyLogoSize?.toString() ?? '',
        companyLogoData: tenantProfileData.companyLogoData ?? '',
        companyLogoMimeType: tenantProfileData.companyLogoMimeType ?? '',
        addressLine1: tenantProfileData.addressLine1 ?? '',
        addressLine2: tenantProfileData.addressLine2 ?? '',
        addressLine3: tenantProfileData.addressLine3 ?? '',
        city: tenantProfileData.city ?? '',
        postalCode: tenantProfileData.postalCode ?? '',
        stateProvinceId: tenantProfileData.stateProvinceId,
        countryId: tenantProfileData.countryId,
        timeZoneId: tenantProfileData.timeZoneId,
        phoneNumber: tenantProfileData.phoneNumber ?? '',
        email: tenantProfileData.email ?? '',
        website: tenantProfileData.website ?? '',
        latitude: tenantProfileData.latitude?.toString() ?? '',
        longitude: tenantProfileData.longitude?.toString() ?? '',
        primaryColor: tenantProfileData.primaryColor ?? '',
        secondaryColor: tenantProfileData.secondaryColor ?? '',
        displaysMetric: tenantProfileData.displaysMetric ?? false,
        displaysUSTerms: tenantProfileData.displaysUSTerms ?? false,
        versionNumber: tenantProfileData.versionNumber?.toString() ?? '',
        active: tenantProfileData.active ?? true,
        deleted: tenantProfileData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.tenantProfileForm.markAsPristine();
    this.tenantProfileForm.markAsUntouched();
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

    if (this.tenantProfileService.userIsSchedulerTenantProfileWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Tenant Profiles", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.tenantProfileForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.tenantProfileForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.tenantProfileForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const tenantProfileSubmitData: TenantProfileSubmitData = {
        id: this.tenantProfileData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        companyLogoFileName: formValue.companyLogoFileName?.trim() || null,
        companyLogoSize: formValue.companyLogoSize ? Number(formValue.companyLogoSize) : null,
        companyLogoData: formValue.companyLogoData?.trim() || null,
        companyLogoMimeType: formValue.companyLogoMimeType?.trim() || null,
        addressLine1: formValue.addressLine1?.trim() || null,
        addressLine2: formValue.addressLine2?.trim() || null,
        addressLine3: formValue.addressLine3?.trim() || null,
        city: formValue.city?.trim() || null,
        postalCode: formValue.postalCode?.trim() || null,
        stateProvinceId: formValue.stateProvinceId ? Number(formValue.stateProvinceId) : null,
        countryId: formValue.countryId ? Number(formValue.countryId) : null,
        timeZoneId: formValue.timeZoneId ? Number(formValue.timeZoneId) : null,
        phoneNumber: formValue.phoneNumber?.trim() || null,
        email: formValue.email?.trim() || null,
        website: formValue.website?.trim() || null,
        latitude: formValue.latitude ? Number(formValue.latitude) : null,
        longitude: formValue.longitude ? Number(formValue.longitude) : null,
        primaryColor: formValue.primaryColor?.trim() || null,
        secondaryColor: formValue.secondaryColor?.trim() || null,
        displaysMetric: !!formValue.displaysMetric,
        displaysUSTerms: !!formValue.displaysUSTerms,
        versionNumber: this.tenantProfileData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.tenantProfileService.PutTenantProfile(tenantProfileSubmitData.id, tenantProfileSubmitData)
      : this.tenantProfileService.PostTenantProfile(tenantProfileSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedTenantProfileData) => {

        this.tenantProfileService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Tenant Profile's detail page
          //
          this.tenantProfileForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.tenantProfileForm.markAsUntouched();

          this.router.navigate(['/tenantprofiles', savedTenantProfileData.id]);
          this.alertService.showMessage('Tenant Profile added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.tenantProfileData = savedTenantProfileData;
          this.buildFormValues(this.tenantProfileData);

          this.alertService.showMessage("Tenant Profile saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Tenant Profile.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Tenant Profile.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Tenant Profile could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerTenantProfileReader(): boolean {
    return this.tenantProfileService.userIsSchedulerTenantProfileReader();
  }

  public userIsSchedulerTenantProfileWriter(): boolean {
    return this.tenantProfileService.userIsSchedulerTenantProfileWriter();
  }
}
