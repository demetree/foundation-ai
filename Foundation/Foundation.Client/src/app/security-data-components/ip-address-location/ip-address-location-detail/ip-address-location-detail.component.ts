/*
   GENERATED FORM FOR THE IPADDRESSLOCATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IpAddressLocation table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to ip-address-location-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IpAddressLocationService, IpAddressLocationData, IpAddressLocationSubmitData } from '../../../security-data-services/ip-address-location.service';
import { LoginAttemptService } from '../../../security-data-services/login-attempt.service';
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
interface IpAddressLocationFormValues {
  ipAddress: string,
  countryCode: string | null,
  countryName: string | null,
  city: string | null,
  latitude: string | null,     // Stored as string for form input, converted to number on submit.
  longitude: string | null,     // Stored as string for form input, converted to number on submit.
  lastLookupDate: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-ip-address-location-detail',
  templateUrl: './ip-address-location-detail.component.html',
  styleUrls: ['./ip-address-location-detail.component.scss']
})

export class IpAddressLocationDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IpAddressLocationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public ipAddressLocationForm: FormGroup = this.fb.group({
        ipAddress: ['', Validators.required],
        countryCode: [''],
        countryName: [''],
        city: [''],
        latitude: [''],
        longitude: [''],
        lastLookupDate: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public ipAddressLocationId: string | null = null;
  public ipAddressLocationData: IpAddressLocationData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  ipAddressLocations$ = this.ipAddressLocationService.GetIpAddressLocationList();
  public loginAttempts$ = this.loginAttemptService.GetLoginAttemptList();

  private destroy$ = new Subject<void>();

  constructor(
    public ipAddressLocationService: IpAddressLocationService,
    public loginAttemptService: LoginAttemptService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the ipAddressLocationId from the route parameters
    this.ipAddressLocationId = this.route.snapshot.paramMap.get('ipAddressLocationId');

    if (this.ipAddressLocationId === 'new' ||
        this.ipAddressLocationId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.ipAddressLocationData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.ipAddressLocationForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.ipAddressLocationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Ip Address Location';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Ip Address Location';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.ipAddressLocationForm.dirty) {
      return confirm('You have unsaved Ip Address Location changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.ipAddressLocationId != null && this.ipAddressLocationId !== 'new') {

      const id = parseInt(this.ipAddressLocationId, 10);

      if (!isNaN(id)) {
        return { ipAddressLocationId: id };
      }
    }

    return null;
  }


/*
  * Loads the IpAddressLocation data for the current ipAddressLocationId.
  *
  * Fully respects the IpAddressLocationService caching strategy and error handling strategy.
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
    if (!this.ipAddressLocationService.userIsSecurityIpAddressLocationReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read IpAddressLocations.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate ipAddressLocationId
    //
    if (!this.ipAddressLocationId) {

      this.alertService.showMessage('No IpAddressLocation ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const ipAddressLocationId = Number(this.ipAddressLocationId);

    if (isNaN(ipAddressLocationId) || ipAddressLocationId <= 0) {

      this.alertService.showMessage(`Invalid Ip Address Location ID: "${this.ipAddressLocationId}"`,
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
      // This is the most targeted way: clear only this IpAddressLocation + relations

      this.ipAddressLocationService.ClearRecordCache(ipAddressLocationId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.ipAddressLocationService.GetIpAddressLocation(ipAddressLocationId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (ipAddressLocationData) => {

        //
        // Success path — ipAddressLocationData can legitimately be null if 404'd but request succeeded
        //
        if (!ipAddressLocationData) {

          this.handleIpAddressLocationNotFound(ipAddressLocationId);

        } else {

          this.ipAddressLocationData = ipAddressLocationData;
          this.buildFormValues(this.ipAddressLocationData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'IpAddressLocation loaded successfully',
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
        this.handleIpAddressLocationLoadError(error, ipAddressLocationId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleIpAddressLocationNotFound(ipAddressLocationId: number): void {

    this.ipAddressLocationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `IpAddressLocation #${ipAddressLocationId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleIpAddressLocationLoadError(error: any, ipAddressLocationId: number): void {

    let message = 'Failed to load Ip Address Location.';
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
          message = 'You do not have permission to view this Ip Address Location.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Ip Address Location #${ipAddressLocationId} was not found.`;
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

    console.error(`Ip Address Location load failed (ID: ${ipAddressLocationId})`, error);

    //
    // Reset UI to safe state
    //
    this.ipAddressLocationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(ipAddressLocationData: IpAddressLocationData | null) {

    if (ipAddressLocationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.ipAddressLocationForm.reset({
        ipAddress: '',
        countryCode: '',
        countryName: '',
        city: '',
        latitude: '',
        longitude: '',
        lastLookupDate: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.ipAddressLocationForm.reset({
        ipAddress: ipAddressLocationData.ipAddress ?? '',
        countryCode: ipAddressLocationData.countryCode ?? '',
        countryName: ipAddressLocationData.countryName ?? '',
        city: ipAddressLocationData.city ?? '',
        latitude: ipAddressLocationData.latitude?.toString() ?? '',
        longitude: ipAddressLocationData.longitude?.toString() ?? '',
        lastLookupDate: isoUtcStringToDateTimeLocal(ipAddressLocationData.lastLookupDate) ?? '',
        active: ipAddressLocationData.active ?? true,
        deleted: ipAddressLocationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.ipAddressLocationForm.markAsPristine();
    this.ipAddressLocationForm.markAsUntouched();
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

    if (this.ipAddressLocationService.userIsSecurityIpAddressLocationWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Ip Address Locations", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.ipAddressLocationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.ipAddressLocationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.ipAddressLocationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const ipAddressLocationSubmitData: IpAddressLocationSubmitData = {
        id: this.ipAddressLocationData?.id || 0,
        ipAddress: formValue.ipAddress!.trim(),
        countryCode: formValue.countryCode?.trim() || null,
        countryName: formValue.countryName?.trim() || null,
        city: formValue.city?.trim() || null,
        latitude: formValue.latitude ? Number(formValue.latitude) : null,
        longitude: formValue.longitude ? Number(formValue.longitude) : null,
        lastLookupDate: dateTimeLocalToIsoUtc(formValue.lastLookupDate!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.ipAddressLocationService.PutIpAddressLocation(ipAddressLocationSubmitData.id, ipAddressLocationSubmitData)
      : this.ipAddressLocationService.PostIpAddressLocation(ipAddressLocationSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedIpAddressLocationData) => {

        this.ipAddressLocationService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Ip Address Location's detail page
          //
          this.ipAddressLocationForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.ipAddressLocationForm.markAsUntouched();

          this.router.navigate(['/ipaddresslocations', savedIpAddressLocationData.id]);
          this.alertService.showMessage('Ip Address Location added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.ipAddressLocationData = savedIpAddressLocationData;
          this.buildFormValues(this.ipAddressLocationData);

          this.alertService.showMessage("Ip Address Location saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Ip Address Location.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Ip Address Location.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Ip Address Location could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecurityIpAddressLocationReader(): boolean {
    return this.ipAddressLocationService.userIsSecurityIpAddressLocationReader();
  }

  public userIsSecurityIpAddressLocationWriter(): boolean {
    return this.ipAddressLocationService.userIsSecurityIpAddressLocationWriter();
  }
}
