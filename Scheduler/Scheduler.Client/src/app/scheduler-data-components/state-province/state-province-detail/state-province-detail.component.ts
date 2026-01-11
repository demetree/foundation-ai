/*
   GENERATED FORM FOR THE STATEPROVINCE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from StateProvince table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to state-province-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { StateProvinceService, StateProvinceData, StateProvinceSubmitData } from '../../../scheduler-data-services/state-province.service';
import { CountryService } from '../../../scheduler-data-services/country.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { TenantProfileService } from '../../../scheduler-data-services/tenant-profile.service';
import { SchedulingTargetAddressService } from '../../../scheduler-data-services/scheduling-target-address.service';
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
interface StateProvinceFormValues {
  countryId: number | bigint,       // For FK link number
  name: string,
  description: string,
  abbreviation: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-state-province-detail',
  templateUrl: './state-province-detail.component.html',
  styleUrls: ['./state-province-detail.component.scss']
})

export class StateProvinceDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<StateProvinceFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public stateProvinceForm: FormGroup = this.fb.group({
        countryId: [null, Validators.required],
        name: ['', Validators.required],
        description: ['', Validators.required],
        abbreviation: ['', Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public stateProvinceId: string | null = null;
  public stateProvinceData: StateProvinceData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  stateProvinces$ = this.stateProvinceService.GetStateProvinceList();
  public countries$ = this.countryService.GetCountryList();
  public offices$ = this.officeService.GetOfficeList();
  public clients$ = this.clientService.GetClientList();
  public tenantProfiles$ = this.tenantProfileService.GetTenantProfileList();
  public schedulingTargetAddresses$ = this.schedulingTargetAddressService.GetSchedulingTargetAddressList();

  private destroy$ = new Subject<void>();

  constructor(
    public stateProvinceService: StateProvinceService,
    public countryService: CountryService,
    public officeService: OfficeService,
    public clientService: ClientService,
    public tenantProfileService: TenantProfileService,
    public schedulingTargetAddressService: SchedulingTargetAddressService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the stateProvinceId from the route parameters
    this.stateProvinceId = this.route.snapshot.paramMap.get('stateProvinceId');

    if (this.stateProvinceId === 'new' ||
        this.stateProvinceId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.stateProvinceData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.stateProvinceForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.stateProvinceForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New State Province';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit State Province';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.stateProvinceForm.dirty) {
      return confirm('You have unsaved State Province changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.stateProvinceId != null && this.stateProvinceId !== 'new') {

      const id = parseInt(this.stateProvinceId, 10);

      if (!isNaN(id)) {
        return { stateProvinceId: id };
      }
    }

    return null;
  }


/*
  * Loads the StateProvince data for the current stateProvinceId.
  *
  * Fully respects the StateProvinceService caching strategy and error handling strategy.
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
    if (!this.stateProvinceService.userIsSchedulerStateProvinceReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read StateProvinces.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate stateProvinceId
    //
    if (!this.stateProvinceId) {

      this.alertService.showMessage('No StateProvince ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const stateProvinceId = Number(this.stateProvinceId);

    if (isNaN(stateProvinceId) || stateProvinceId <= 0) {

      this.alertService.showMessage(`Invalid State Province ID: "${this.stateProvinceId}"`,
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
      // This is the most targeted way: clear only this StateProvince + relations

      this.stateProvinceService.ClearRecordCache(stateProvinceId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.stateProvinceService.GetStateProvince(stateProvinceId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (stateProvinceData) => {

        //
        // Success path — stateProvinceData can legitimately be null if 404'd but request succeeded
        //
        if (!stateProvinceData) {

          this.handleStateProvinceNotFound(stateProvinceId);

        } else {

          this.stateProvinceData = stateProvinceData;
          this.buildFormValues(this.stateProvinceData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'StateProvince loaded successfully',
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
        this.handleStateProvinceLoadError(error, stateProvinceId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleStateProvinceNotFound(stateProvinceId: number): void {

    this.stateProvinceData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `StateProvince #${stateProvinceId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleStateProvinceLoadError(error: any, stateProvinceId: number): void {

    let message = 'Failed to load State Province.';
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
          message = 'You do not have permission to view this State Province.';
          title = 'Forbidden';
          break;
        case 404:
          message = `State Province #${stateProvinceId} was not found.`;
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

    console.error(`State Province load failed (ID: ${stateProvinceId})`, error);

    //
    // Reset UI to safe state
    //
    this.stateProvinceData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(stateProvinceData: StateProvinceData | null) {

    if (stateProvinceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.stateProvinceForm.reset({
        countryId: null,
        name: '',
        description: '',
        abbreviation: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.stateProvinceForm.reset({
        countryId: stateProvinceData.countryId,
        name: stateProvinceData.name ?? '',
        description: stateProvinceData.description ?? '',
        abbreviation: stateProvinceData.abbreviation ?? '',
        sequence: stateProvinceData.sequence?.toString() ?? '',
        active: stateProvinceData.active ?? true,
        deleted: stateProvinceData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.stateProvinceForm.markAsPristine();
    this.stateProvinceForm.markAsUntouched();
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

    if (this.stateProvinceService.userIsSchedulerStateProvinceWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to State Provinces", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.stateProvinceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.stateProvinceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.stateProvinceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const stateProvinceSubmitData: StateProvinceSubmitData = {
        id: this.stateProvinceData?.id || 0,
        countryId: Number(formValue.countryId),
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        abbreviation: formValue.abbreviation!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.stateProvinceService.PutStateProvince(stateProvinceSubmitData.id, stateProvinceSubmitData)
      : this.stateProvinceService.PostStateProvince(stateProvinceSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedStateProvinceData) => {

        this.stateProvinceService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created State Province's detail page
          //
          this.stateProvinceForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.stateProvinceForm.markAsUntouched();

          this.router.navigate(['/stateprovinces', savedStateProvinceData.id]);
          this.alertService.showMessage('State Province added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.stateProvinceData = savedStateProvinceData;
          this.buildFormValues(this.stateProvinceData);

          this.alertService.showMessage("State Province saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this State Province.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the State Province.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('State Province could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerStateProvinceReader(): boolean {
    return this.stateProvinceService.userIsSchedulerStateProvinceReader();
  }

  public userIsSchedulerStateProvinceWriter(): boolean {
    return this.stateProvinceService.userIsSchedulerStateProvinceWriter();
  }
}
