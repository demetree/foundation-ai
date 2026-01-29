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
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EventResourceAssignmentService, EventResourceAssignmentData, EventResourceAssignmentSubmitData } from '../../../scheduler-data-services/event-resource-assignment.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { CrewService } from '../../../scheduler-data-services/crew.service';
import { VolunteerGroupService } from '../../../scheduler-data-services/volunteer-group.service';
import { AssignmentRoleService } from '../../../scheduler-data-services/assignment-role.service';
import { AssignmentStatusService } from '../../../scheduler-data-services/assignment-status.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { ChargeTypeService } from '../../../scheduler-data-services/charge-type.service';
import { AuthService } from '../../../services/auth.service';

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
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-event-resource-assignment-add-edit',
  templateUrl: './event-resource-assignment-add-edit.component.html',
  styleUrls: ['./event-resource-assignment-add-edit.component.scss']
})
export class EventResourceAssignmentAddEditComponent {
  @ViewChild('eventResourceAssignmentModal') eventResourceAssignmentModal!: TemplateRef<any>;
  @Output() eventResourceAssignmentChanged = new Subject<EventResourceAssignmentData[]>();
  @Input() eventResourceAssignmentSubmitData: EventResourceAssignmentSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


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
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  eventResourceAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentList();
  scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  offices$ = this.officeService.GetOfficeList();
  resources$ = this.resourceService.GetResourceList();
  crews$ = this.crewService.GetCrewList();
  volunteerGroups$ = this.volunteerGroupService.GetVolunteerGroupList();
  assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList();
  assignmentStatuses$ = this.assignmentStatusService.GetAssignmentStatusList();
  contacts$ = this.contactService.GetContactList();
  chargeTypes$ = this.chargeTypeService.GetChargeTypeList();

  constructor(
    private modalService: NgbModal,
    private eventResourceAssignmentService: EventResourceAssignmentService,
    private scheduledEventService: ScheduledEventService,
    private officeService: OfficeService,
    private resourceService: ResourceService,
    private crewService: CrewService,
    private volunteerGroupService: VolunteerGroupService,
    private assignmentRoleService: AssignmentRoleService,
    private assignmentStatusService: AssignmentStatusService,
    private contactService: ContactService,
    private chargeTypeService: ChargeTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(eventResourceAssignmentData?: EventResourceAssignmentData) {

    if (eventResourceAssignmentData != null) {

      if (!this.eventResourceAssignmentService.userIsSchedulerEventResourceAssignmentReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Event Resource Assignments`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.eventResourceAssignmentSubmitData = this.eventResourceAssignmentService.ConvertToEventResourceAssignmentSubmitData(eventResourceAssignmentData);
      this.isEditMode = true;
      this.objectGuid = eventResourceAssignmentData.objectGuid;

      this.buildFormValues(eventResourceAssignmentData);

    } else {

      if (!this.eventResourceAssignmentService.userIsSchedulerEventResourceAssignmentWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Event Resource Assignments`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.eventResourceAssignmentForm.patchValue(this.preSeededData);
      }

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

    this.modalRef = this.modalService.open(this.eventResourceAssignmentModal, {
      size: 'xl',
      scrollable: true,
      backdrop: 'static',
      keyboard: true,
      windowClass: 'custom-modal'
    });
    this.modalIsDisplayed = true;
  }


  closeModal() {
    if (this.modalRef) {
      this.modalRef.dismiss('cancel');
    }
    this.modalIsDisplayed = false;
  }


  submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.eventResourceAssignmentService.userIsSchedulerEventResourceAssignmentWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Event Resource Assignments`,
        '',
        MessageSeverity.info
      );
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
        id: this.eventResourceAssignmentSubmitData?.id || 0,
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
        versionNumber: this.eventResourceAssignmentSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateEventResourceAssignment(eventResourceAssignmentSubmitData);
      } else {
        this.addEventResourceAssignment(eventResourceAssignmentSubmitData);
      }
  }

  private addEventResourceAssignment(eventResourceAssignmentData: EventResourceAssignmentSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    eventResourceAssignmentData.versionNumber = 0;
    eventResourceAssignmentData.active = true;
    eventResourceAssignmentData.deleted = false;
    this.eventResourceAssignmentService.PostEventResourceAssignment(eventResourceAssignmentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newEventResourceAssignment) => {

        this.eventResourceAssignmentService.ClearAllCaches();

        this.eventResourceAssignmentChanged.next([newEventResourceAssignment]);

        this.alertService.showMessage("Event Resource Assignment added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/eventresourceassignment', newEventResourceAssignment.id]);
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


  private updateEventResourceAssignment(eventResourceAssignmentData: EventResourceAssignmentSubmitData) {
    this.eventResourceAssignmentService.PutEventResourceAssignment(eventResourceAssignmentData.id, eventResourceAssignmentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedEventResourceAssignment) => {

        this.eventResourceAssignmentService.ClearAllCaches();

        this.eventResourceAssignmentChanged.next([updatedEventResourceAssignment]);

        this.alertService.showMessage("Event Resource Assignment updated successfully", '', MessageSeverity.success);

        this.closeModal();
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
        versionNumber: eventResourceAssignmentData.versionNumber?.toString() ?? '',
        active: eventResourceAssignmentData.active ?? true,
        deleted: eventResourceAssignmentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.eventResourceAssignmentForm.markAsPristine();
    this.eventResourceAssignmentForm.markAsUntouched();
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


  public userIsSchedulerEventResourceAssignmentReader(): boolean {
    return this.eventResourceAssignmentService.userIsSchedulerEventResourceAssignmentReader();
  }

  public userIsSchedulerEventResourceAssignmentWriter(): boolean {
    return this.eventResourceAssignmentService.userIsSchedulerEventResourceAssignmentWriter();
  }
}
