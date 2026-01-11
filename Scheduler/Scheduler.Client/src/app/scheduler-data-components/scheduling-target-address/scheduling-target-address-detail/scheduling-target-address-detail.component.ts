/*
   GENERATED FORM FOR THE SCHEDULINGTARGETADDRESS TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SchedulingTargetAddress table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduling-target-address-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SchedulingTargetAddressService, SchedulingTargetAddressData, SchedulingTargetAddressSubmitData } from '../../../scheduler-data-services/scheduling-target-address.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { StateProvinceService } from '../../../scheduler-data-services/state-province.service';
import { CountryService } from '../../../scheduler-data-services/country.service';
import { SchedulingTargetAddressChangeHistoryService } from '../../../scheduler-data-services/scheduling-target-address-change-history.service';
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
interface SchedulingTargetAddressFormValues {
  schedulingTargetId: number | bigint,       // For FK link number
  clientId: number | bigint | null,       // For FK link number
  addressLine1: string,
  addressLine2: string | null,
  city: string,
  postalCode: string | null,
  stateProvinceId: number | bigint,       // For FK link number
  countryId: number | bigint,       // For FK link number
  latitude: string | null,     // Stored as string for form input, converted to number on submit.
  longitude: string | null,     // Stored as string for form input, converted to number on submit.
  label: string | null,
  isPrimary: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-scheduling-target-address-detail',
  templateUrl: './scheduling-target-address-detail.component.html',
  styleUrls: ['./scheduling-target-address-detail.component.scss']
})

export class SchedulingTargetAddressDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SchedulingTargetAddressFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public schedulingTargetAddressForm: FormGroup = this.fb.group({
        schedulingTargetId: [null, Validators.required],
        clientId: [null],
        addressLine1: ['', Validators.required],
        addressLine2: [''],
        city: ['', Validators.required],
        postalCode: [''],
        stateProvinceId: [null, Validators.required],
        countryId: [null, Validators.required],
        latitude: [''],
        longitude: [''],
        label: [''],
        isPrimary: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public schedulingTargetAddressId: string | null = null;
  public schedulingTargetAddressData: SchedulingTargetAddressData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  schedulingTargetAddresses$ = this.schedulingTargetAddressService.GetSchedulingTargetAddressList();
  public schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  public clients$ = this.clientService.GetClientList();
  public stateProvinces$ = this.stateProvinceService.GetStateProvinceList();
  public countries$ = this.countryService.GetCountryList();
  public schedulingTargetAddressChangeHistories$ = this.schedulingTargetAddressChangeHistoryService.GetSchedulingTargetAddressChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public schedulingTargetAddressService: SchedulingTargetAddressService,
    public schedulingTargetService: SchedulingTargetService,
    public clientService: ClientService,
    public stateProvinceService: StateProvinceService,
    public countryService: CountryService,
    public schedulingTargetAddressChangeHistoryService: SchedulingTargetAddressChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the schedulingTargetAddressId from the route parameters
    this.schedulingTargetAddressId = this.route.snapshot.paramMap.get('schedulingTargetAddressId');

    if (this.schedulingTargetAddressId === 'new' ||
        this.schedulingTargetAddressId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.schedulingTargetAddressData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.schedulingTargetAddressForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.schedulingTargetAddressForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Scheduling Target Address';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Scheduling Target Address';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.schedulingTargetAddressForm.dirty) {
      return confirm('You have unsaved Scheduling Target Address changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.schedulingTargetAddressId != null && this.schedulingTargetAddressId !== 'new') {

      const id = parseInt(this.schedulingTargetAddressId, 10);

      if (!isNaN(id)) {
        return { schedulingTargetAddressId: id };
      }
    }

    return null;
  }


/*
  * Loads the SchedulingTargetAddress data for the current schedulingTargetAddressId.
  *
  * Fully respects the SchedulingTargetAddressService caching strategy and error handling strategy.
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
    if (!this.schedulingTargetAddressService.userIsSchedulerSchedulingTargetAddressReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SchedulingTargetAddresses.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate schedulingTargetAddressId
    //
    if (!this.schedulingTargetAddressId) {

      this.alertService.showMessage('No SchedulingTargetAddress ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const schedulingTargetAddressId = Number(this.schedulingTargetAddressId);

    if (isNaN(schedulingTargetAddressId) || schedulingTargetAddressId <= 0) {

      this.alertService.showMessage(`Invalid Scheduling Target Address ID: "${this.schedulingTargetAddressId}"`,
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
      // This is the most targeted way: clear only this SchedulingTargetAddress + relations

      this.schedulingTargetAddressService.ClearRecordCache(schedulingTargetAddressId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.schedulingTargetAddressService.GetSchedulingTargetAddress(schedulingTargetAddressId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (schedulingTargetAddressData) => {

        //
        // Success path — schedulingTargetAddressData can legitimately be null if 404'd but request succeeded
        //
        if (!schedulingTargetAddressData) {

          this.handleSchedulingTargetAddressNotFound(schedulingTargetAddressId);

        } else {

          this.schedulingTargetAddressData = schedulingTargetAddressData;
          this.buildFormValues(this.schedulingTargetAddressData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SchedulingTargetAddress loaded successfully',
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
        this.handleSchedulingTargetAddressLoadError(error, schedulingTargetAddressId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSchedulingTargetAddressNotFound(schedulingTargetAddressId: number): void {

    this.schedulingTargetAddressData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SchedulingTargetAddress #${schedulingTargetAddressId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSchedulingTargetAddressLoadError(error: any, schedulingTargetAddressId: number): void {

    let message = 'Failed to load Scheduling Target Address.';
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
          message = 'You do not have permission to view this Scheduling Target Address.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Scheduling Target Address #${schedulingTargetAddressId} was not found.`;
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

    console.error(`Scheduling Target Address load failed (ID: ${schedulingTargetAddressId})`, error);

    //
    // Reset UI to safe state
    //
    this.schedulingTargetAddressData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(schedulingTargetAddressData: SchedulingTargetAddressData | null) {

    if (schedulingTargetAddressData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.schedulingTargetAddressForm.reset({
        schedulingTargetId: null,
        clientId: null,
        addressLine1: '',
        addressLine2: '',
        city: '',
        postalCode: '',
        stateProvinceId: null,
        countryId: null,
        latitude: '',
        longitude: '',
        label: '',
        isPrimary: false,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.schedulingTargetAddressForm.reset({
        schedulingTargetId: schedulingTargetAddressData.schedulingTargetId,
        clientId: schedulingTargetAddressData.clientId,
        addressLine1: schedulingTargetAddressData.addressLine1 ?? '',
        addressLine2: schedulingTargetAddressData.addressLine2 ?? '',
        city: schedulingTargetAddressData.city ?? '',
        postalCode: schedulingTargetAddressData.postalCode ?? '',
        stateProvinceId: schedulingTargetAddressData.stateProvinceId,
        countryId: schedulingTargetAddressData.countryId,
        latitude: schedulingTargetAddressData.latitude?.toString() ?? '',
        longitude: schedulingTargetAddressData.longitude?.toString() ?? '',
        label: schedulingTargetAddressData.label ?? '',
        isPrimary: schedulingTargetAddressData.isPrimary ?? false,
        versionNumber: schedulingTargetAddressData.versionNumber?.toString() ?? '',
        active: schedulingTargetAddressData.active ?? true,
        deleted: schedulingTargetAddressData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.schedulingTargetAddressForm.markAsPristine();
    this.schedulingTargetAddressForm.markAsUntouched();
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

    if (this.schedulingTargetAddressService.userIsSchedulerSchedulingTargetAddressWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Scheduling Target Addresses", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.schedulingTargetAddressForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.schedulingTargetAddressForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.schedulingTargetAddressForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const schedulingTargetAddressSubmitData: SchedulingTargetAddressSubmitData = {
        id: this.schedulingTargetAddressData?.id || 0,
        schedulingTargetId: Number(formValue.schedulingTargetId),
        clientId: formValue.clientId ? Number(formValue.clientId) : null,
        addressLine1: formValue.addressLine1!.trim(),
        addressLine2: formValue.addressLine2?.trim() || null,
        city: formValue.city!.trim(),
        postalCode: formValue.postalCode?.trim() || null,
        stateProvinceId: Number(formValue.stateProvinceId),
        countryId: Number(formValue.countryId),
        latitude: formValue.latitude ? Number(formValue.latitude) : null,
        longitude: formValue.longitude ? Number(formValue.longitude) : null,
        label: formValue.label?.trim() || null,
        isPrimary: !!formValue.isPrimary,
        versionNumber: this.schedulingTargetAddressData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.schedulingTargetAddressService.PutSchedulingTargetAddress(schedulingTargetAddressSubmitData.id, schedulingTargetAddressSubmitData)
      : this.schedulingTargetAddressService.PostSchedulingTargetAddress(schedulingTargetAddressSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSchedulingTargetAddressData) => {

        this.schedulingTargetAddressService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Scheduling Target Address's detail page
          //
          this.schedulingTargetAddressForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.schedulingTargetAddressForm.markAsUntouched();

          this.router.navigate(['/schedulingtargetaddresses', savedSchedulingTargetAddressData.id]);
          this.alertService.showMessage('Scheduling Target Address added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.schedulingTargetAddressData = savedSchedulingTargetAddressData;
          this.buildFormValues(this.schedulingTargetAddressData);

          this.alertService.showMessage("Scheduling Target Address saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Scheduling Target Address.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target Address.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target Address could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerSchedulingTargetAddressReader(): boolean {
    return this.schedulingTargetAddressService.userIsSchedulerSchedulingTargetAddressReader();
  }

  public userIsSchedulerSchedulingTargetAddressWriter(): boolean {
    return this.schedulingTargetAddressService.userIsSchedulerSchedulingTargetAddressWriter();
  }
}
