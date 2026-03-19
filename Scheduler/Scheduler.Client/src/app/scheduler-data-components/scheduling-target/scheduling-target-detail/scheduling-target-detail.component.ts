/*
   GENERATED FORM FOR THE SCHEDULINGTARGET TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SchedulingTarget table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduling-target-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SchedulingTargetService, SchedulingTargetData, SchedulingTargetSubmitData } from '../../../scheduler-data-services/scheduling-target.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { SchedulingTargetTypeService } from '../../../scheduler-data-services/scheduling-target-type.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { CalendarService } from '../../../scheduler-data-services/calendar.service';
import { SchedulingTargetChangeHistoryService } from '../../../scheduler-data-services/scheduling-target-change-history.service';
import { SchedulingTargetContactService } from '../../../scheduler-data-services/scheduling-target-contact.service';
import { SchedulingTargetAddressService } from '../../../scheduler-data-services/scheduling-target-address.service';
import { SchedulingTargetQualificationRequirementService } from '../../../scheduler-data-services/scheduling-target-qualification-requirement.service';
import { RateSheetService } from '../../../scheduler-data-services/rate-sheet.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { HouseholdService } from '../../../scheduler-data-services/household.service';
import { DocumentService } from '../../../scheduler-data-services/document.service';
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
interface SchedulingTargetFormValues {
  name: string,
  description: string | null,
  officeId: number | bigint | null,       // For FK link number
  clientId: number | bigint,       // For FK link number
  schedulingTargetTypeId: number | bigint,       // For FK link number
  timeZoneId: number | bigint,       // For FK link number
  calendarId: number | bigint | null,       // For FK link number
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
  selector: 'app-scheduling-target-detail',
  templateUrl: './scheduling-target-detail.component.html',
  styleUrls: ['./scheduling-target-detail.component.scss']
})

export class SchedulingTargetDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SchedulingTargetFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public schedulingTargetForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        officeId: [null],
        clientId: [null, Validators.required],
        schedulingTargetTypeId: [null, Validators.required],
        timeZoneId: [null, Validators.required],
        calendarId: [null],
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


  public schedulingTargetId: string | null = null;
  public schedulingTargetData: SchedulingTargetData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  public offices$ = this.officeService.GetOfficeList();
  public clients$ = this.clientService.GetClientList();
  public schedulingTargetTypes$ = this.schedulingTargetTypeService.GetSchedulingTargetTypeList();
  public timeZones$ = this.timeZoneService.GetTimeZoneList();
  public calendars$ = this.calendarService.GetCalendarList();
  public schedulingTargetChangeHistories$ = this.schedulingTargetChangeHistoryService.GetSchedulingTargetChangeHistoryList();
  public schedulingTargetContacts$ = this.schedulingTargetContactService.GetSchedulingTargetContactList();
  public schedulingTargetAddresses$ = this.schedulingTargetAddressService.GetSchedulingTargetAddressList();
  public schedulingTargetQualificationRequirements$ = this.schedulingTargetQualificationRequirementService.GetSchedulingTargetQualificationRequirementList();
  public rateSheets$ = this.rateSheetService.GetRateSheetList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public households$ = this.householdService.GetHouseholdList();
  public documents$ = this.documentService.GetDocumentList();

  private destroy$ = new Subject<void>();

  constructor(
    public schedulingTargetService: SchedulingTargetService,
    public officeService: OfficeService,
    public clientService: ClientService,
    public schedulingTargetTypeService: SchedulingTargetTypeService,
    public timeZoneService: TimeZoneService,
    public calendarService: CalendarService,
    public schedulingTargetChangeHistoryService: SchedulingTargetChangeHistoryService,
    public schedulingTargetContactService: SchedulingTargetContactService,
    public schedulingTargetAddressService: SchedulingTargetAddressService,
    public schedulingTargetQualificationRequirementService: SchedulingTargetQualificationRequirementService,
    public rateSheetService: RateSheetService,
    public scheduledEventService: ScheduledEventService,
    public householdService: HouseholdService,
    public documentService: DocumentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the schedulingTargetId from the route parameters
    this.schedulingTargetId = this.route.snapshot.paramMap.get('schedulingTargetId');

    if (this.schedulingTargetId === 'new' ||
        this.schedulingTargetId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.schedulingTargetData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.schedulingTargetForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.schedulingTargetForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Scheduling Target';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Scheduling Target';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.schedulingTargetForm.dirty) {
      return confirm('You have unsaved Scheduling Target changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.schedulingTargetId != null && this.schedulingTargetId !== 'new') {

      const id = parseInt(this.schedulingTargetId, 10);

      if (!isNaN(id)) {
        return { schedulingTargetId: id };
      }
    }

    return null;
  }


/*
  * Loads the SchedulingTarget data for the current schedulingTargetId.
  *
  * Fully respects the SchedulingTargetService caching strategy and error handling strategy.
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
    if (!this.schedulingTargetService.userIsSchedulerSchedulingTargetReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SchedulingTargets.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate schedulingTargetId
    //
    if (!this.schedulingTargetId) {

      this.alertService.showMessage('No SchedulingTarget ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const schedulingTargetId = Number(this.schedulingTargetId);

    if (isNaN(schedulingTargetId) || schedulingTargetId <= 0) {

      this.alertService.showMessage(`Invalid Scheduling Target ID: "${this.schedulingTargetId}"`,
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
      // This is the most targeted way: clear only this SchedulingTarget + relations

      this.schedulingTargetService.ClearRecordCache(schedulingTargetId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.schedulingTargetService.GetSchedulingTarget(schedulingTargetId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (schedulingTargetData) => {

        //
        // Success path — schedulingTargetData can legitimately be null if 404'd but request succeeded
        //
        if (!schedulingTargetData) {

          this.handleSchedulingTargetNotFound(schedulingTargetId);

        } else {

          this.schedulingTargetData = schedulingTargetData;
          this.buildFormValues(this.schedulingTargetData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SchedulingTarget loaded successfully',
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
        this.handleSchedulingTargetLoadError(error, schedulingTargetId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSchedulingTargetNotFound(schedulingTargetId: number): void {

    this.schedulingTargetData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SchedulingTarget #${schedulingTargetId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSchedulingTargetLoadError(error: any, schedulingTargetId: number): void {

    let message = 'Failed to load Scheduling Target.';
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
          message = 'You do not have permission to view this Scheduling Target.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Scheduling Target #${schedulingTargetId} was not found.`;
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

    console.error(`Scheduling Target load failed (ID: ${schedulingTargetId})`, error);

    //
    // Reset UI to safe state
    //
    this.schedulingTargetData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(schedulingTargetData: SchedulingTargetData | null) {

    if (schedulingTargetData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.schedulingTargetForm.reset({
        name: '',
        description: '',
        officeId: null,
        clientId: null,
        schedulingTargetTypeId: null,
        timeZoneId: null,
        calendarId: null,
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
        this.schedulingTargetForm.reset({
        name: schedulingTargetData.name ?? '',
        description: schedulingTargetData.description ?? '',
        officeId: schedulingTargetData.officeId,
        clientId: schedulingTargetData.clientId,
        schedulingTargetTypeId: schedulingTargetData.schedulingTargetTypeId,
        timeZoneId: schedulingTargetData.timeZoneId,
        calendarId: schedulingTargetData.calendarId,
        notes: schedulingTargetData.notes ?? '',
        externalId: schedulingTargetData.externalId ?? '',
        color: schedulingTargetData.color ?? '',
        attributes: schedulingTargetData.attributes ?? '',
        avatarFileName: schedulingTargetData.avatarFileName ?? '',
        avatarSize: schedulingTargetData.avatarSize?.toString() ?? '',
        avatarData: schedulingTargetData.avatarData ?? '',
        avatarMimeType: schedulingTargetData.avatarMimeType ?? '',
        versionNumber: schedulingTargetData.versionNumber?.toString() ?? '',
        active: schedulingTargetData.active ?? true,
        deleted: schedulingTargetData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.schedulingTargetForm.markAsPristine();
    this.schedulingTargetForm.markAsUntouched();
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

    if (this.schedulingTargetService.userIsSchedulerSchedulingTargetWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Scheduling Targets", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.schedulingTargetForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.schedulingTargetForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.schedulingTargetForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const schedulingTargetSubmitData: SchedulingTargetSubmitData = {
        id: this.schedulingTargetData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        clientId: Number(formValue.clientId),
        schedulingTargetTypeId: Number(formValue.schedulingTargetTypeId),
        timeZoneId: Number(formValue.timeZoneId),
        calendarId: formValue.calendarId ? Number(formValue.calendarId) : null,
        notes: formValue.notes?.trim() || null,
        externalId: formValue.externalId?.trim() || null,
        color: formValue.color?.trim() || null,
        attributes: formValue.attributes?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        versionNumber: this.schedulingTargetData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.schedulingTargetService.PutSchedulingTarget(schedulingTargetSubmitData.id, schedulingTargetSubmitData)
      : this.schedulingTargetService.PostSchedulingTarget(schedulingTargetSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSchedulingTargetData) => {

        this.schedulingTargetService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Scheduling Target's detail page
          //
          this.schedulingTargetForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.schedulingTargetForm.markAsUntouched();

          this.router.navigate(['/schedulingtargets', savedSchedulingTargetData.id]);
          this.alertService.showMessage('Scheduling Target added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.schedulingTargetData = savedSchedulingTargetData;
          this.buildFormValues(this.schedulingTargetData);

          this.alertService.showMessage("Scheduling Target saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Scheduling Target.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerSchedulingTargetReader(): boolean {
    return this.schedulingTargetService.userIsSchedulerSchedulingTargetReader();
  }

  public userIsSchedulerSchedulingTargetWriter(): boolean {
    return this.schedulingTargetService.userIsSchedulerSchedulingTargetWriter();
  }
}
