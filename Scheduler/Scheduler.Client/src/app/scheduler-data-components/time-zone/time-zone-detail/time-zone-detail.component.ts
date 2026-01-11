/*
   GENERATED FORM FOR THE TIMEZONE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TimeZone table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to time-zone-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TimeZoneService, TimeZoneData, TimeZoneSubmitData } from '../../../scheduler-data-services/time-zone.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { TenantProfileService } from '../../../scheduler-data-services/tenant-profile.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { ShiftPatternService } from '../../../scheduler-data-services/shift-pattern.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { ResourceAvailabilityService } from '../../../scheduler-data-services/resource-availability.service';
import { ResourceShiftService } from '../../../scheduler-data-services/resource-shift.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
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
interface TimeZoneFormValues {
  name: string,
  description: string,
  ianaTimeZone: string,
  abbreviation: string,
  abbreviationDaylightSavings: string,
  supportsDaylightSavings: boolean,
  standardUTCOffsetHours: string,     // Stored as string for form input, converted to number on submit.
  dstUTCOffsetHours: string,     // Stored as string for form input, converted to number on submit.
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-time-zone-detail',
  templateUrl: './time-zone-detail.component.html',
  styleUrls: ['./time-zone-detail.component.scss']
})

export class TimeZoneDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TimeZoneFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public timeZoneForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        ianaTimeZone: ['', Validators.required],
        abbreviation: ['', Validators.required],
        abbreviationDaylightSavings: ['', Validators.required],
        supportsDaylightSavings: [false],
        standardUTCOffsetHours: ['', Validators.required],
        dstUTCOffsetHours: ['', Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public timeZoneId: string | null = null;
  public timeZoneData: TimeZoneData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  timeZones$ = this.timeZoneService.GetTimeZoneList();
  public contacts$ = this.contactService.GetContactList();
  public offices$ = this.officeService.GetOfficeList();
  public clients$ = this.clientService.GetClientList();
  public tenantProfiles$ = this.tenantProfileService.GetTenantProfileList();
  public schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  public shiftPatterns$ = this.shiftPatternService.GetShiftPatternList();
  public resources$ = this.resourceService.GetResourceList();
  public resourceAvailabilities$ = this.resourceAvailabilityService.GetResourceAvailabilityList();
  public resourceShifts$ = this.resourceShiftService.GetResourceShiftList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public timeZoneService: TimeZoneService,
    public contactService: ContactService,
    public officeService: OfficeService,
    public clientService: ClientService,
    public tenantProfileService: TenantProfileService,
    public schedulingTargetService: SchedulingTargetService,
    public shiftPatternService: ShiftPatternService,
    public resourceService: ResourceService,
    public resourceAvailabilityService: ResourceAvailabilityService,
    public resourceShiftService: ResourceShiftService,
    public scheduledEventService: ScheduledEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the timeZoneId from the route parameters
    this.timeZoneId = this.route.snapshot.paramMap.get('timeZoneId');

    if (this.timeZoneId === 'new' ||
        this.timeZoneId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.timeZoneData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.timeZoneForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.timeZoneForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Time Zone';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Time Zone';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.timeZoneForm.dirty) {
      return confirm('You have unsaved Time Zone changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.timeZoneId != null && this.timeZoneId !== 'new') {

      const id = parseInt(this.timeZoneId, 10);

      if (!isNaN(id)) {
        return { timeZoneId: id };
      }
    }

    return null;
  }


/*
  * Loads the TimeZone data for the current timeZoneId.
  *
  * Fully respects the TimeZoneService caching strategy and error handling strategy.
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
    if (!this.timeZoneService.userIsSchedulerTimeZoneReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read TimeZones.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate timeZoneId
    //
    if (!this.timeZoneId) {

      this.alertService.showMessage('No TimeZone ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const timeZoneId = Number(this.timeZoneId);

    if (isNaN(timeZoneId) || timeZoneId <= 0) {

      this.alertService.showMessage(`Invalid Time Zone ID: "${this.timeZoneId}"`,
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
      // This is the most targeted way: clear only this TimeZone + relations

      this.timeZoneService.ClearRecordCache(timeZoneId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.timeZoneService.GetTimeZone(timeZoneId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (timeZoneData) => {

        //
        // Success path — timeZoneData can legitimately be null if 404'd but request succeeded
        //
        if (!timeZoneData) {

          this.handleTimeZoneNotFound(timeZoneId);

        } else {

          this.timeZoneData = timeZoneData;
          this.buildFormValues(this.timeZoneData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'TimeZone loaded successfully',
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
        this.handleTimeZoneLoadError(error, timeZoneId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleTimeZoneNotFound(timeZoneId: number): void {

    this.timeZoneData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `TimeZone #${timeZoneId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleTimeZoneLoadError(error: any, timeZoneId: number): void {

    let message = 'Failed to load Time Zone.';
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
          message = 'You do not have permission to view this Time Zone.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Time Zone #${timeZoneId} was not found.`;
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

    console.error(`Time Zone load failed (ID: ${timeZoneId})`, error);

    //
    // Reset UI to safe state
    //
    this.timeZoneData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(timeZoneData: TimeZoneData | null) {

    if (timeZoneData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.timeZoneForm.reset({
        name: '',
        description: '',
        ianaTimeZone: '',
        abbreviation: '',
        abbreviationDaylightSavings: '',
        supportsDaylightSavings: false,
        standardUTCOffsetHours: '',
        dstUTCOffsetHours: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.timeZoneForm.reset({
        name: timeZoneData.name ?? '',
        description: timeZoneData.description ?? '',
        ianaTimeZone: timeZoneData.ianaTimeZone ?? '',
        abbreviation: timeZoneData.abbreviation ?? '',
        abbreviationDaylightSavings: timeZoneData.abbreviationDaylightSavings ?? '',
        supportsDaylightSavings: timeZoneData.supportsDaylightSavings ?? false,
        standardUTCOffsetHours: timeZoneData.standardUTCOffsetHours?.toString() ?? '',
        dstUTCOffsetHours: timeZoneData.dstUTCOffsetHours?.toString() ?? '',
        sequence: timeZoneData.sequence?.toString() ?? '',
        active: timeZoneData.active ?? true,
        deleted: timeZoneData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.timeZoneForm.markAsPristine();
    this.timeZoneForm.markAsUntouched();
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

    if (this.timeZoneService.userIsSchedulerTimeZoneWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Time Zones", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.timeZoneForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.timeZoneForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.timeZoneForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const timeZoneSubmitData: TimeZoneSubmitData = {
        id: this.timeZoneData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        ianaTimeZone: formValue.ianaTimeZone!.trim(),
        abbreviation: formValue.abbreviation!.trim(),
        abbreviationDaylightSavings: formValue.abbreviationDaylightSavings!.trim(),
        supportsDaylightSavings: !!formValue.supportsDaylightSavings,
        standardUTCOffsetHours: Number(formValue.standardUTCOffsetHours),
        dstUTCOffsetHours: Number(formValue.dstUTCOffsetHours),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.timeZoneService.PutTimeZone(timeZoneSubmitData.id, timeZoneSubmitData)
      : this.timeZoneService.PostTimeZone(timeZoneSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedTimeZoneData) => {

        this.timeZoneService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Time Zone's detail page
          //
          this.timeZoneForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.timeZoneForm.markAsUntouched();

          this.router.navigate(['/timezones', savedTimeZoneData.id]);
          this.alertService.showMessage('Time Zone added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.timeZoneData = savedTimeZoneData;
          this.buildFormValues(this.timeZoneData);

          this.alertService.showMessage("Time Zone saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Time Zone.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Time Zone.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Time Zone could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerTimeZoneReader(): boolean {
    return this.timeZoneService.userIsSchedulerTimeZoneReader();
  }

  public userIsSchedulerTimeZoneWriter(): boolean {
    return this.timeZoneService.userIsSchedulerTimeZoneWriter();
  }
}
