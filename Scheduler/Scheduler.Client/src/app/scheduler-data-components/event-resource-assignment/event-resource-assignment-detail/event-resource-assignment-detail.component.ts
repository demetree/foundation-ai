/*
   GENERATED FORM FOR THE EVENTRESOURCEASSIGNMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EventResourceAssignment table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to event-resource-assignment-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EventResourceAssignmentService, EventResourceAssignmentData, EventResourceAssignmentSubmitData } from '../../../scheduler-data-services/event-resource-assignment.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { CrewService } from '../../../scheduler-data-services/crew.service';
import { VolunteerGroupService } from '../../../scheduler-data-services/volunteer-group.service';
import { AssignmentRoleService } from '../../../scheduler-data-services/assignment-role.service';
import { AssignmentStatusService } from '../../../scheduler-data-services/assignment-status.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { ChargeTypeService } from '../../../scheduler-data-services/charge-type.service';
import { EventResourceAssignmentChangeHistoryService } from '../../../scheduler-data-services/event-resource-assignment-change-history.service';
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
interface EventResourceAssignmentFormValues {
  scheduledEventId: number | bigint,       // For FK link number
  officeId: number | bigint | null,       // For FK link number
  resourceId: number | bigint | null,       // For FK link number
  crewId: number | bigint | null,       // For FK link number
  volunteerGroupId: number | bigint | null,       // For FK link number
  assignmentRoleId: number | bigint | null,       // For FK link number
  assignmentStatusId: number | bigint,       // For FK link number
  assignmentStartDateTime: string | null,
  assignmentEndDateTime: string | null,
  notes: string | null,
  isTravelRequired: boolean | null,
  travelDurationMinutes: string | null,     // Stored as string for form input, converted to number on submit.
  distanceKilometers: string | null,     // Stored as string for form input, converted to number on submit.
  startLocation: string | null,
  actualStartDateTime: string | null,
  actualEndDateTime: string | null,
  actualNotes: string | null,
  isVolunteer: boolean,
  reportedVolunteerHours: string | null,     // Stored as string for form input, converted to number on submit.
  approvedVolunteerHours: string | null,     // Stored as string for form input, converted to number on submit.
  hoursApprovedByContactId: number | bigint | null,       // For FK link number
  approvedDateTime: string | null,
  reimbursementAmount: string | null,     // Stored as string for form input, converted to number on submit.
  chargeTypeId: number | bigint | null,       // For FK link number
  reimbursementRequested: boolean,
  volunteerNotes: string | null,
  reminderSentDateTime: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-event-resource-assignment-detail',
  templateUrl: './event-resource-assignment-detail.component.html',
  styleUrls: ['./event-resource-assignment-detail.component.scss']
})

export class EventResourceAssignmentDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EventResourceAssignmentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public eventResourceAssignmentForm: FormGroup = this.fb.group({
        scheduledEventId: [null, Validators.required],
        officeId: [null],
        resourceId: [null],
        crewId: [null],
        volunteerGroupId: [null],
        assignmentRoleId: [null],
        assignmentStatusId: [null, Validators.required],
        assignmentStartDateTime: [''],
        assignmentEndDateTime: [''],
        notes: [''],
        isTravelRequired: [false],
        travelDurationMinutes: [''],
        distanceKilometers: [''],
        startLocation: [''],
        actualStartDateTime: [''],
        actualEndDateTime: [''],
        actualNotes: [''],
        isVolunteer: [false],
        reportedVolunteerHours: [''],
        approvedVolunteerHours: [''],
        hoursApprovedByContactId: [null],
        approvedDateTime: [''],
        reimbursementAmount: [''],
        chargeTypeId: [null],
        reimbursementRequested: [false],
        volunteerNotes: [''],
        reminderSentDateTime: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public eventResourceAssignmentId: string | null = null;
  public eventResourceAssignmentData: EventResourceAssignmentData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  eventResourceAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public offices$ = this.officeService.GetOfficeList();
  public resources$ = this.resourceService.GetResourceList();
  public crews$ = this.crewService.GetCrewList();
  public volunteerGroups$ = this.volunteerGroupService.GetVolunteerGroupList();
  public assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList();
  public assignmentStatuses$ = this.assignmentStatusService.GetAssignmentStatusList();
  public contacts$ = this.contactService.GetContactList();
  public chargeTypes$ = this.chargeTypeService.GetChargeTypeList();
  public eventResourceAssignmentChangeHistories$ = this.eventResourceAssignmentChangeHistoryService.GetEventResourceAssignmentChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public eventResourceAssignmentService: EventResourceAssignmentService,
    public scheduledEventService: ScheduledEventService,
    public officeService: OfficeService,
    public resourceService: ResourceService,
    public crewService: CrewService,
    public volunteerGroupService: VolunteerGroupService,
    public assignmentRoleService: AssignmentRoleService,
    public assignmentStatusService: AssignmentStatusService,
    public contactService: ContactService,
    public chargeTypeService: ChargeTypeService,
    public eventResourceAssignmentChangeHistoryService: EventResourceAssignmentChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the eventResourceAssignmentId from the route parameters
    this.eventResourceAssignmentId = this.route.snapshot.paramMap.get('eventResourceAssignmentId');

    if (this.eventResourceAssignmentId === 'new' ||
        this.eventResourceAssignmentId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.eventResourceAssignmentData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.eventResourceAssignmentForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.eventResourceAssignmentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Event Resource Assignment';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Event Resource Assignment';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.eventResourceAssignmentForm.dirty) {
      return confirm('You have unsaved Event Resource Assignment changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.eventResourceAssignmentId != null && this.eventResourceAssignmentId !== 'new') {

      const id = parseInt(this.eventResourceAssignmentId, 10);

      if (!isNaN(id)) {
        return { eventResourceAssignmentId: id };
      }
    }

    return null;
  }


/*
  * Loads the EventResourceAssignment data for the current eventResourceAssignmentId.
  *
  * Fully respects the EventResourceAssignmentService caching strategy and error handling strategy.
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
    if (!this.eventResourceAssignmentService.userIsSchedulerEventResourceAssignmentReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read EventResourceAssignments.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate eventResourceAssignmentId
    //
    if (!this.eventResourceAssignmentId) {

      this.alertService.showMessage('No EventResourceAssignment ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const eventResourceAssignmentId = Number(this.eventResourceAssignmentId);

    if (isNaN(eventResourceAssignmentId) || eventResourceAssignmentId <= 0) {

      this.alertService.showMessage(`Invalid Event Resource Assignment ID: "${this.eventResourceAssignmentId}"`,
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
      // This is the most targeted way: clear only this EventResourceAssignment + relations

      this.eventResourceAssignmentService.ClearRecordCache(eventResourceAssignmentId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.eventResourceAssignmentService.GetEventResourceAssignment(eventResourceAssignmentId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (eventResourceAssignmentData) => {

        //
        // Success path — eventResourceAssignmentData can legitimately be null if 404'd but request succeeded
        //
        if (!eventResourceAssignmentData) {

          this.handleEventResourceAssignmentNotFound(eventResourceAssignmentId);

        } else {

          this.eventResourceAssignmentData = eventResourceAssignmentData;
          this.buildFormValues(this.eventResourceAssignmentData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'EventResourceAssignment loaded successfully',
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
        this.handleEventResourceAssignmentLoadError(error, eventResourceAssignmentId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleEventResourceAssignmentNotFound(eventResourceAssignmentId: number): void {

    this.eventResourceAssignmentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `EventResourceAssignment #${eventResourceAssignmentId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleEventResourceAssignmentLoadError(error: any, eventResourceAssignmentId: number): void {

    let message = 'Failed to load Event Resource Assignment.';
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
          message = 'You do not have permission to view this Event Resource Assignment.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Event Resource Assignment #${eventResourceAssignmentId} was not found.`;
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

    console.error(`Event Resource Assignment load failed (ID: ${eventResourceAssignmentId})`, error);

    //
    // Reset UI to safe state
    //
    this.eventResourceAssignmentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(eventResourceAssignmentData: EventResourceAssignmentData | null) {

    if (eventResourceAssignmentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.eventResourceAssignmentForm.reset({
        scheduledEventId: null,
        officeId: null,
        resourceId: null,
        crewId: null,
        volunteerGroupId: null,
        assignmentRoleId: null,
        assignmentStatusId: null,
        assignmentStartDateTime: '',
        assignmentEndDateTime: '',
        notes: '',
        isTravelRequired: false,
        travelDurationMinutes: '',
        distanceKilometers: '',
        startLocation: '',
        actualStartDateTime: '',
        actualEndDateTime: '',
        actualNotes: '',
        isVolunteer: false,
        reportedVolunteerHours: '',
        approvedVolunteerHours: '',
        hoursApprovedByContactId: null,
        approvedDateTime: '',
        reimbursementAmount: '',
        chargeTypeId: null,
        reimbursementRequested: false,
        volunteerNotes: '',
        reminderSentDateTime: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.eventResourceAssignmentForm.reset({
        scheduledEventId: eventResourceAssignmentData.scheduledEventId,
        officeId: eventResourceAssignmentData.officeId,
        resourceId: eventResourceAssignmentData.resourceId,
        crewId: eventResourceAssignmentData.crewId,
        volunteerGroupId: eventResourceAssignmentData.volunteerGroupId,
        assignmentRoleId: eventResourceAssignmentData.assignmentRoleId,
        assignmentStatusId: eventResourceAssignmentData.assignmentStatusId,
        assignmentStartDateTime: isoUtcStringToDateTimeLocal(eventResourceAssignmentData.assignmentStartDateTime) ?? '',
        assignmentEndDateTime: isoUtcStringToDateTimeLocal(eventResourceAssignmentData.assignmentEndDateTime) ?? '',
        notes: eventResourceAssignmentData.notes ?? '',
        isTravelRequired: eventResourceAssignmentData.isTravelRequired ?? false,
        travelDurationMinutes: eventResourceAssignmentData.travelDurationMinutes?.toString() ?? '',
        distanceKilometers: eventResourceAssignmentData.distanceKilometers?.toString() ?? '',
        startLocation: eventResourceAssignmentData.startLocation ?? '',
        actualStartDateTime: isoUtcStringToDateTimeLocal(eventResourceAssignmentData.actualStartDateTime) ?? '',
        actualEndDateTime: isoUtcStringToDateTimeLocal(eventResourceAssignmentData.actualEndDateTime) ?? '',
        actualNotes: eventResourceAssignmentData.actualNotes ?? '',
        isVolunteer: eventResourceAssignmentData.isVolunteer ?? false,
        reportedVolunteerHours: eventResourceAssignmentData.reportedVolunteerHours?.toString() ?? '',
        approvedVolunteerHours: eventResourceAssignmentData.approvedVolunteerHours?.toString() ?? '',
        hoursApprovedByContactId: eventResourceAssignmentData.hoursApprovedByContactId,
        approvedDateTime: isoUtcStringToDateTimeLocal(eventResourceAssignmentData.approvedDateTime) ?? '',
        reimbursementAmount: eventResourceAssignmentData.reimbursementAmount?.toString() ?? '',
        chargeTypeId: eventResourceAssignmentData.chargeTypeId,
        reimbursementRequested: eventResourceAssignmentData.reimbursementRequested ?? false,
        volunteerNotes: eventResourceAssignmentData.volunteerNotes ?? '',
        reminderSentDateTime: isoUtcStringToDateTimeLocal(eventResourceAssignmentData.reminderSentDateTime) ?? '',
        versionNumber: eventResourceAssignmentData.versionNumber?.toString() ?? '',
        active: eventResourceAssignmentData.active ?? true,
        deleted: eventResourceAssignmentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.eventResourceAssignmentForm.markAsPristine();
    this.eventResourceAssignmentForm.markAsUntouched();
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

    if (this.eventResourceAssignmentService.userIsSchedulerEventResourceAssignmentWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Event Resource Assignments", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.eventResourceAssignmentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.eventResourceAssignmentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.eventResourceAssignmentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const eventResourceAssignmentSubmitData: EventResourceAssignmentSubmitData = {
        id: this.eventResourceAssignmentData?.id || 0,
        scheduledEventId: Number(formValue.scheduledEventId),
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        resourceId: formValue.resourceId ? Number(formValue.resourceId) : null,
        crewId: formValue.crewId ? Number(formValue.crewId) : null,
        volunteerGroupId: formValue.volunteerGroupId ? Number(formValue.volunteerGroupId) : null,
        assignmentRoleId: formValue.assignmentRoleId ? Number(formValue.assignmentRoleId) : null,
        assignmentStatusId: Number(formValue.assignmentStatusId),
        assignmentStartDateTime: formValue.assignmentStartDateTime ? dateTimeLocalToIsoUtc(formValue.assignmentStartDateTime.trim()) : null,
        assignmentEndDateTime: formValue.assignmentEndDateTime ? dateTimeLocalToIsoUtc(formValue.assignmentEndDateTime.trim()) : null,
        notes: formValue.notes?.trim() || null,
        isTravelRequired: formValue.isTravelRequired == true ? true : formValue.isTravelRequired == false ? false : null,
        travelDurationMinutes: formValue.travelDurationMinutes ? Number(formValue.travelDurationMinutes) : null,
        distanceKilometers: formValue.distanceKilometers ? Number(formValue.distanceKilometers) : null,
        startLocation: formValue.startLocation?.trim() || null,
        actualStartDateTime: formValue.actualStartDateTime ? dateTimeLocalToIsoUtc(formValue.actualStartDateTime.trim()) : null,
        actualEndDateTime: formValue.actualEndDateTime ? dateTimeLocalToIsoUtc(formValue.actualEndDateTime.trim()) : null,
        actualNotes: formValue.actualNotes?.trim() || null,
        isVolunteer: !!formValue.isVolunteer,
        reportedVolunteerHours: formValue.reportedVolunteerHours ? Number(formValue.reportedVolunteerHours) : null,
        approvedVolunteerHours: formValue.approvedVolunteerHours ? Number(formValue.approvedVolunteerHours) : null,
        hoursApprovedByContactId: formValue.hoursApprovedByContactId ? Number(formValue.hoursApprovedByContactId) : null,
        approvedDateTime: formValue.approvedDateTime ? dateTimeLocalToIsoUtc(formValue.approvedDateTime.trim()) : null,
        reimbursementAmount: formValue.reimbursementAmount ? Number(formValue.reimbursementAmount) : null,
        chargeTypeId: formValue.chargeTypeId ? Number(formValue.chargeTypeId) : null,
        reimbursementRequested: !!formValue.reimbursementRequested,
        volunteerNotes: formValue.volunteerNotes?.trim() || null,
        reminderSentDateTime: formValue.reminderSentDateTime ? dateTimeLocalToIsoUtc(formValue.reminderSentDateTime.trim()) : null,
        versionNumber: this.eventResourceAssignmentData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.eventResourceAssignmentService.PutEventResourceAssignment(eventResourceAssignmentSubmitData.id, eventResourceAssignmentSubmitData)
      : this.eventResourceAssignmentService.PostEventResourceAssignment(eventResourceAssignmentSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedEventResourceAssignmentData) => {

        this.eventResourceAssignmentService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Event Resource Assignment's detail page
          //
          this.eventResourceAssignmentForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.eventResourceAssignmentForm.markAsUntouched();

          this.router.navigate(['/eventresourceassignments', savedEventResourceAssignmentData.id]);
          this.alertService.showMessage('Event Resource Assignment added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.eventResourceAssignmentData = savedEventResourceAssignmentData;
          this.buildFormValues(this.eventResourceAssignmentData);

          this.alertService.showMessage("Event Resource Assignment saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Event Resource Assignment.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Event Resource Assignment.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Event Resource Assignment could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerEventResourceAssignmentReader(): boolean {
    return this.eventResourceAssignmentService.userIsSchedulerEventResourceAssignmentReader();
  }

  public userIsSchedulerEventResourceAssignmentWriter(): boolean {
    return this.eventResourceAssignmentService.userIsSchedulerEventResourceAssignmentWriter();
  }
}
