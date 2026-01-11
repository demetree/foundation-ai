/*
   GENERATED FORM FOR THE OFFICE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Office table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to office-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { OfficeService, OfficeData, OfficeSubmitData } from '../../../scheduler-data-services/office.service';
import { OfficeTypeService } from '../../../scheduler-data-services/office-type.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { StateProvinceService } from '../../../scheduler-data-services/state-province.service';
import { CountryService } from '../../../scheduler-data-services/country.service';
import { OfficeChangeHistoryService } from '../../../scheduler-data-services/office-change-history.service';
import { OfficeContactService } from '../../../scheduler-data-services/office-contact.service';
import { CalendarService } from '../../../scheduler-data-services/calendar.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { RateSheetService } from '../../../scheduler-data-services/rate-sheet.service';
import { CrewService } from '../../../scheduler-data-services/crew.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
import { GiftService } from '../../../scheduler-data-services/gift.service';
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
interface OfficeFormValues {
  name: string,
  description: string | null,
  officeTypeId: number | bigint,       // For FK link number
  timeZoneId: number | bigint,       // For FK link number
  currencyId: number | bigint,       // For FK link number
  addressLine1: string,
  addressLine2: string | null,
  city: string,
  postalCode: string | null,
  stateProvinceId: number | bigint,       // For FK link number
  countryId: number | bigint,       // For FK link number
  phone: string | null,
  email: string | null,
  latitude: string | null,     // Stored as string for form input, converted to number on submit.
  longitude: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  externalId: string | null,
  color: string | null,
  attributes: string | null,
  avatarFileName: string | null,
  avatarSize: string | null,     // Stored as string for form input, converted to number on submit.
  avatarData: string | null,
  avatarMimeType: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-office-detail',
  templateUrl: './office-detail.component.html',
  styleUrls: ['./office-detail.component.scss']
})

export class OfficeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<OfficeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public officeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        officeTypeId: [null, Validators.required],
        timeZoneId: [null, Validators.required],
        currencyId: [null, Validators.required],
        addressLine1: ['', Validators.required],
        addressLine2: [''],
        city: ['', Validators.required],
        postalCode: [''],
        stateProvinceId: [null, Validators.required],
        countryId: [null, Validators.required],
        phone: [''],
        email: [''],
        latitude: [''],
        longitude: [''],
        notes: [''],
        externalId: [''],
        color: [''],
        attributes: [''],
        avatarFileName: [''],
        avatarSize: [''],
        avatarData: [''],
        avatarMimeType: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public officeId: string | null = null;
  public officeData: OfficeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  offices$ = this.officeService.GetOfficeList();
  public officeTypes$ = this.officeTypeService.GetOfficeTypeList();
  public timeZones$ = this.timeZoneService.GetTimeZoneList();
  public currencies$ = this.currencyService.GetCurrencyList();
  public stateProvinces$ = this.stateProvinceService.GetStateProvinceList();
  public countries$ = this.countryService.GetCountryList();
  public officeChangeHistories$ = this.officeChangeHistoryService.GetOfficeChangeHistoryList();
  public officeContacts$ = this.officeContactService.GetOfficeContactList();
  public calendars$ = this.calendarService.GetCalendarList();
  public schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  public resources$ = this.resourceService.GetResourceList();
  public rateSheets$ = this.rateSheetService.GetRateSheetList();
  public crews$ = this.crewService.GetCrewList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public eventResourceAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentList();
  public gifts$ = this.giftService.GetGiftList();

  private destroy$ = new Subject<void>();

  constructor(
    public officeService: OfficeService,
    public officeTypeService: OfficeTypeService,
    public timeZoneService: TimeZoneService,
    public currencyService: CurrencyService,
    public stateProvinceService: StateProvinceService,
    public countryService: CountryService,
    public officeChangeHistoryService: OfficeChangeHistoryService,
    public officeContactService: OfficeContactService,
    public calendarService: CalendarService,
    public schedulingTargetService: SchedulingTargetService,
    public resourceService: ResourceService,
    public rateSheetService: RateSheetService,
    public crewService: CrewService,
    public scheduledEventService: ScheduledEventService,
    public eventResourceAssignmentService: EventResourceAssignmentService,
    public giftService: GiftService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the officeId from the route parameters
    this.officeId = this.route.snapshot.paramMap.get('officeId');

    if (this.officeId === 'new' ||
        this.officeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.officeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.officeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.officeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Office';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Office';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.officeForm.dirty) {
      return confirm('You have unsaved Office changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.officeId != null && this.officeId !== 'new') {

      const id = parseInt(this.officeId, 10);

      if (!isNaN(id)) {
        return { officeId: id };
      }
    }

    return null;
  }


/*
  * Loads the Office data for the current officeId.
  *
  * Fully respects the OfficeService caching strategy and error handling strategy.
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
    if (!this.officeService.userIsSchedulerOfficeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Offices.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate officeId
    //
    if (!this.officeId) {

      this.alertService.showMessage('No Office ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const officeId = Number(this.officeId);

    if (isNaN(officeId) || officeId <= 0) {

      this.alertService.showMessage(`Invalid Office ID: "${this.officeId}"`,
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
      // This is the most targeted way: clear only this Office + relations

      this.officeService.ClearRecordCache(officeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.officeService.GetOffice(officeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (officeData) => {

        //
        // Success path — officeData can legitimately be null if 404'd but request succeeded
        //
        if (!officeData) {

          this.handleOfficeNotFound(officeId);

        } else {

          this.officeData = officeData;
          this.buildFormValues(this.officeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Office loaded successfully',
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
        this.handleOfficeLoadError(error, officeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleOfficeNotFound(officeId: number): void {

    this.officeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Office #${officeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleOfficeLoadError(error: any, officeId: number): void {

    let message = 'Failed to load Office.';
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
          message = 'You do not have permission to view this Office.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Office #${officeId} was not found.`;
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

    console.error(`Office load failed (ID: ${officeId})`, error);

    //
    // Reset UI to safe state
    //
    this.officeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(officeData: OfficeData | null) {

    if (officeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.officeForm.reset({
        name: '',
        description: '',
        officeTypeId: null,
        timeZoneId: null,
        currencyId: null,
        addressLine1: '',
        addressLine2: '',
        city: '',
        postalCode: '',
        stateProvinceId: null,
        countryId: null,
        phone: '',
        email: '',
        latitude: '',
        longitude: '',
        notes: '',
        externalId: '',
        color: '',
        attributes: '',
        avatarFileName: '',
        avatarSize: '',
        avatarData: '',
        avatarMimeType: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.officeForm.reset({
        name: officeData.name ?? '',
        description: officeData.description ?? '',
        officeTypeId: officeData.officeTypeId,
        timeZoneId: officeData.timeZoneId,
        currencyId: officeData.currencyId,
        addressLine1: officeData.addressLine1 ?? '',
        addressLine2: officeData.addressLine2 ?? '',
        city: officeData.city ?? '',
        postalCode: officeData.postalCode ?? '',
        stateProvinceId: officeData.stateProvinceId,
        countryId: officeData.countryId,
        phone: officeData.phone ?? '',
        email: officeData.email ?? '',
        latitude: officeData.latitude?.toString() ?? '',
        longitude: officeData.longitude?.toString() ?? '',
        notes: officeData.notes ?? '',
        externalId: officeData.externalId ?? '',
        color: officeData.color ?? '',
        attributes: officeData.attributes ?? '',
        avatarFileName: officeData.avatarFileName ?? '',
        avatarSize: officeData.avatarSize?.toString() ?? '',
        avatarData: officeData.avatarData ?? '',
        avatarMimeType: officeData.avatarMimeType ?? '',
        versionNumber: officeData.versionNumber?.toString() ?? '',
        active: officeData.active ?? true,
        deleted: officeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.officeForm.markAsPristine();
    this.officeForm.markAsUntouched();
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

    if (this.officeService.userIsSchedulerOfficeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Offices", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.officeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.officeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.officeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const officeSubmitData: OfficeSubmitData = {
        id: this.officeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        officeTypeId: Number(formValue.officeTypeId),
        timeZoneId: Number(formValue.timeZoneId),
        currencyId: Number(formValue.currencyId),
        addressLine1: formValue.addressLine1!.trim(),
        addressLine2: formValue.addressLine2?.trim() || null,
        city: formValue.city!.trim(),
        postalCode: formValue.postalCode?.trim() || null,
        stateProvinceId: Number(formValue.stateProvinceId),
        countryId: Number(formValue.countryId),
        phone: formValue.phone?.trim() || null,
        email: formValue.email?.trim() || null,
        latitude: formValue.latitude ? Number(formValue.latitude) : null,
        longitude: formValue.longitude ? Number(formValue.longitude) : null,
        notes: formValue.notes?.trim() || null,
        externalId: formValue.externalId?.trim() || null,
        color: formValue.color?.trim() || null,
        attributes: formValue.attributes?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        versionNumber: this.officeData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.officeService.PutOffice(officeSubmitData.id, officeSubmitData)
      : this.officeService.PostOffice(officeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedOfficeData) => {

        this.officeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Office's detail page
          //
          this.officeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.officeForm.markAsUntouched();

          this.router.navigate(['/offices', savedOfficeData.id]);
          this.alertService.showMessage('Office added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.officeData = savedOfficeData;
          this.buildFormValues(this.officeData);

          this.alertService.showMessage("Office saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Office.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Office.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Office could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerOfficeReader(): boolean {
    return this.officeService.userIsSchedulerOfficeReader();
  }

  public userIsSchedulerOfficeWriter(): boolean {
    return this.officeService.userIsSchedulerOfficeWriter();
  }
}
