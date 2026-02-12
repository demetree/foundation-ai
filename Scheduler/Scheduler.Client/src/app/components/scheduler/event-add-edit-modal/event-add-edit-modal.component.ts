import { Component, OnInit, OnDestroy, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { forkJoin, Subscription, lastValueFrom } from 'rxjs';
import { ScheduledEventService, ScheduledEventData, ScheduledEventSubmitData } from '../../../scheduler-data-services/scheduled-event.service';
import { EventResourceAssignmentService, EventResourceAssignmentData, EventResourceAssignmentSubmitData } from '../../../scheduler-data-services/event-resource-assignment.service';
import { SchedulingTargetService, SchedulingTargetData } from '../../../scheduler-data-services/scheduling-target.service';
import { ScheduledEventTemplateService, ScheduledEventTemplateData } from '../../../scheduler-data-services/scheduled-event-template.service';
import { CrewService, CrewData } from '../../../scheduler-data-services/crew.service';
import { ResourceService, ResourceData } from '../../../scheduler-data-services/resource.service';
import { AssignmentRoleService, AssignmentRoleData } from '../../../scheduler-data-services/assignment-role.service';
import { QualificationService } from '../../../scheduler-data-services/qualification.service';
import { AssignmentRoleQualificationRequirementService } from '../../../scheduler-data-services/assignment-role-qualification-requirement.service';
import { SchedulingTargetQualificationRequirementService } from '../../../scheduler-data-services/scheduling-target-qualification-requirement.service';
import { ResourceQualificationService } from '../../../scheduler-data-services/resource-qualification.service';
import { RecurrenceRuleService, RecurrenceRuleData } from '../../../scheduler-data-services/recurrence-rule.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-event-add-edit-modal',
  templateUrl: './event-add-edit-modal.component.html',
  styleUrls: ['./event-add-edit-modal.component.scss']
})
export class EventAddEditModalComponent implements OnInit, OnDestroy {
  // -------------------------------------------------------------------------
  // Inputs / Outputs
  // -------------------------------------------------------------------------
  @Input() event: ScheduledEventData | null = null; // null = create mode
  @Output() saved = new EventEmitter<boolean>();

  @Input() initialStart: string | null = null;
  @Input() initialEnd: string | null = null;

  // -------------------------------------------------------------------------
  // UI State
  // -------------------------------------------------------------------------
  isEditMode = false;
  isVisible = false;
  saving = false;
  isRecurring = false;
  recurrenceRule: RecurrenceRuleData | null = null;

  // Template picker
  selectedTemplateId: number | null = null;
  templates: ScheduledEventTemplateData[] = [];

  // Main form
  eventForm!: FormGroup;
  assignmentsFormArray!: FormArray;

  // Lookup data
  targets: SchedulingTargetData[] = [];
  crews: CrewData[] = [];
  resources: ResourceData[] = [];
  roles: AssignmentRoleData[] = [];

  // Derived data
  selectedTarget: SchedulingTargetData | null = null;
  qualificationWarnings: string[] = [];

  // Track which existing assignments to keep (for diff-based save)
  private existingAssignmentIds: Set<number> = new Set();

  private subscriptions = new Subscription();

  constructor(
    public activeModal: NgbActiveModal,
    private fb: FormBuilder,
    private scheduledEventService: ScheduledEventService,
    private assignmentService: EventResourceAssignmentService,
    private targetService: SchedulingTargetService,
    private templateService: ScheduledEventTemplateService,
    private crewService: CrewService,
    private resourceService: ResourceService,
    private roleService: AssignmentRoleService,
    private qualificationService: QualificationService,
    private roleQualService: AssignmentRoleQualificationRequirementService,
    private targetQualService: SchedulingTargetQualificationRequirementService,
    private resourceQualService: ResourceQualificationService,
    private recurrenceRuleService: RecurrenceRuleService,
    private alertService: AlertService
  ) {
    this.buildForm();
  }

  ngOnInit(): void {

    this.isEditMode = !!this.event;

    this.loadLookupData();

    if (!this.isEditMode) {
      // Pre-fill dates from calendar selection
      if (this.initialStart) {
        this.eventForm.patchValue({
          startDateTime: this.initialStart.slice(0, 16),
          endDateTime: this.initialEnd?.slice(0, 16) || this.initialStart.slice(0, 16)
        });
      }
    } else if (this.event) {
      this.populateForm(this.event);
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  // -------------------------------------------------------------------------
  // Form Setup
  // -------------------------------------------------------------------------
  private buildForm(): void {
    this.eventForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      schedulingTargetId: [null],
      startDateTime: ['', Validators.required],
      endDateTime: ['', Validators.required],
      location: [''],
      timeZoneId: [null],
      notes: [''],
      externalId: ['']
    });

    // Assignments array (managed separately for easier sub-table integration)
    this.assignmentsFormArray = this.fb.array([]);
    this.eventForm.addControl('assignments', this.assignmentsFormArray);
  }

  // -------------------------------------------------------------------------
  // Data Loading
  // -------------------------------------------------------------------------
  private loadLookupData(): void {
    forkJoin({
      templates: this.templateService.GetScheduledEventTemplateList({ active: true }),
      targets: this.targetService.GetSchedulingTargetList({ active: true, includeRelations: true }),
      crews: this.crewService.GetCrewList({ active: true, includeRelations: true }),
      resources: this.resourceService.GetResourceList({ active: true, includeRelations: true }),
      roles: this.roleService.GetAssignmentRoleList({ active: true })
    }).subscribe({
      next: (data) => {
        this.templates = data.templates;
        this.targets = data.targets;
        this.crews = data.crews;
        this.resources = data.resources;
        this.roles = data.roles;

        if (this.isEditMode) {
          this.loadExistingAssignments();
        }
      },
      error: () => {
        this.alertService.showMessage('Failed to load supporting data', '', MessageSeverity.error);
      }
    });
  }

  private loadExistingAssignments(): void {
    if (!this.event) return;

    this.assignmentService.GetEventResourceAssignmentList({
      scheduledEventId: this.event.id,
      includeRelations: true,
      active: true
    }).subscribe(assignments => {
      this.existingAssignmentIds.clear();
      assignments.forEach(assignment => {
        this.existingAssignmentIds.add(Number(assignment.id));
        this.addAssignmentToForm(assignment);
      });
      this.validateQualifications();
    });
  }

  // -------------------------------------------------------------------------
  // Template Handling
  // -------------------------------------------------------------------------
  applyTemplate(): void {
    if (!this.selectedTemplateId) {
      this.eventForm.reset();
      this.assignmentsFormArray.clear();
      return;
    }

    const template = this.templates.find(t => t.id === this.selectedTemplateId);
    if (!template) return;

    // Apply basic fields
    this.eventForm.patchValue({
      name: template.name,
      description: template.description || '',
      defaultLocationPattern: template.defaultLocationPattern || ''
    });

    // Apply duration if dates are empty
    if (!this.eventForm.get('startDateTime')?.value) {
      const now = new Date();
      const end = new Date(now.getTime() + (template.defaultDurationMinutes as number) * 60000);
      this.eventForm.patchValue({
        startDateTime: now.toISOString().slice(0, 16),
        endDateTime: end.toISOString().slice(0, 16)
      });
    }

    this.validateQualifications();
  }

  // -------------------------------------------------------------------------
  // Assignment Management
  // -------------------------------------------------------------------------
  get assignments(): FormArray {
    return this.eventForm.get('assignments') as FormArray;
  }

  addIndividualAssignment(): void {
    const group = this.fb.group({
      id: [null],
      resourceId: [null, Validators.required],
      crewId: [null],
      assignmentRoleId: [null],
      assignmentStartDateTime: [this.eventForm.get('startDateTime')?.value],
      assignmentEndDateTime: [this.eventForm.get('endDateTime')?.value],
      notes: ['']
    });
    this.assignments.push(group);
    this.validateQualifications();
  }

  addCrewAssignment(): void {
    const group = this.fb.group({
      id: [null],
      crewId: [null, Validators.required],
      resourceId: [null],
      assignmentRoleId: [null],
      assignmentStartDateTime: [this.eventForm.get('startDateTime')?.value],
      assignmentEndDateTime: [this.eventForm.get('endDateTime')?.value],
      notes: ['']
    });
    this.assignments.push(group);
    this.validateQualifications();
  }

  editAssignment(index: number): void {
    // Toggle expanded/editing state for the assignment row
    const control = this.assignments.at(index);
    if (control) {
      const current = control.get('_editing')?.value ?? false;
      if (control.get('_editing')) {
        control.get('_editing')!.setValue(!current);
      } else {
        // Add a transient editing flag
        (control as FormGroup).addControl('_editing', this.fb.control(true));
      }
    }
  }

  removeAssignment(index: number): void {
    this.assignments.removeAt(index);
    this.validateQualifications();
  }

  private addAssignmentToForm(assignment: EventResourceAssignmentData): void {
    const group = this.fb.group({
      id: [assignment.id],
      resourceId: [assignment.resourceId],
      crewId: [assignment.crewId],
      assignmentRoleId: [assignment.assignmentRoleId],
      assignmentStartDateTime: [assignment.assignmentStartDateTime],
      assignmentEndDateTime: [assignment.assignmentEndDateTime],
      notes: [assignment.notes]
    });
    this.assignments.push(group);
  }

  // -------------------------------------------------------------------------
  // Qualification Validation
  // -------------------------------------------------------------------------
  private async validateQualifications(): Promise<void> {
    this.qualificationWarnings = [];

    const targetId = this.eventForm.get('schedulingTargetId')?.value;

    // Collect required qualification IDs from all sources
    const requiredQualIds = new Set<number>();

    // From assignment roles
    for (const control of this.assignments.controls) {
      const roleId = control.get('assignmentRoleId')?.value;
      if (roleId) {
        try {
          const roleQuals = await lastValueFrom(
            this.roleQualService.GetAssignmentRoleQualificationRequirementList({
              assignmentRoleId: roleId,
              active: true
            })
          );
          roleQuals.forEach(rq => requiredQualIds.add(Number((rq as any).qualificationId)));
        } catch {
          // Silently skip — don't block the user over qualification lookup failures
        }
      }
    }

    // From scheduling target
    if (targetId) {
      try {
        const targetQuals = await lastValueFrom(
          this.targetQualService.GetSchedulingTargetQualificationRequirementList({
            schedulingTargetId: targetId,
            active: true
          })
        );
        targetQuals.forEach(tq => requiredQualIds.add(Number((tq as any).qualificationId)));
      } catch {
        // Silently skip
      }
    }

    if (requiredQualIds.size === 0) return;

    // Check each assigned resource against required qualifications
    for (const control of this.assignments.controls) {
      const resourceId = control.get('resourceId')?.value;
      if (!resourceId) continue;

      try {
        const resourceQuals = await lastValueFrom(
          this.resourceQualService.GetResourceQualificationList({
            resourceId: resourceId,
            active: true
          })
        );
        const resourceQualIds = new Set(resourceQuals.map(rq => Number((rq as any).qualificationId)));

        for (const reqQualId of requiredQualIds) {
          if (!resourceQualIds.has(reqQualId)) {
            // Find resource name for a readable warning
            const resource = this.resources.find(r => Number(r.id) === Number(resourceId));
            const resourceName = resource?.name || `Resource #${resourceId}`;
            this.qualificationWarnings.push(
              `${resourceName} may be missing a required qualification (ID: ${reqQualId})`
            );
          }
        }
      } catch {
        // Skip resource qualification check on error
      }
    }
  }

  // -------------------------------------------------------------------------
  // Form Population (Edit Mode)
  // -------------------------------------------------------------------------
  private populateForm(eventData: ScheduledEventData): void {
    this.eventForm.patchValue({
      name: eventData.name,
      description: eventData.description,
      schedulingTargetId: eventData.schedulingTargetId,
      startDateTime: eventData.startDateTime.slice(0, 16),
      endDateTime: eventData.endDateTime.slice(0, 16),
      location: eventData.location,
      timeZoneId: eventData.timeZoneId,
      notes: eventData.notes,
      externalId: eventData.externalId
    });

    this.selectedTarget = eventData.schedulingTarget || null;

    if (eventData.recurrenceRule) {
      this.isRecurring = true;
      this.recurrenceRule = eventData.recurrenceRule;
    } else {
      this.isRecurring = false;
      this.recurrenceRule = null;
    }
  }

  onRecurrenceToggle(): void {
    if (this.isRecurring && !this.recurrenceRule) {
      // Initialize with defaults if enabling and no rule exists
      this.recurrenceRule = new RecurrenceRuleData();
      this.recurrenceRule.id = 0 as any;           // Explicitly 0 for "new rule"
      this.recurrenceRule.recurrenceFrequencyId = 2; // Daily default
      this.recurrenceRule.interval = 1;
      this.recurrenceRule.versionNumber = 0 as any;
      this.recurrenceRule.active = true;
      this.recurrenceRule.deleted = false;
    }
  }

  // -------------------------------------------------------------------------
  // Save Logic
  // -------------------------------------------------------------------------
  async save(): Promise<void> {
    if (this.eventForm.invalid) {
      this.alertService.showMessage('Please correct form errors', '', MessageSeverity.warn);
      this.eventForm.markAllAsTouched();
      return;
    }

    this.saving = true;

    try {
      let recurrenceRuleId: number | null = null;

      // 1. Handle Recurrence Rule Save/Update
      if (this.isRecurring && this.recurrenceRule) {
        const ruleId = Number(this.recurrenceRule.id || 0);
        if (ruleId > 0) {
          // Update existing
          await lastValueFrom(this.recurrenceRuleService.PutRecurrenceRule(this.recurrenceRule.id, this.recurrenceRule));
          recurrenceRuleId = ruleId;
        } else {
          // Create new
          const newRule = await lastValueFrom(this.recurrenceRuleService.PostRecurrenceRule(this.recurrenceRule));
          recurrenceRuleId = Number(newRule.id);
        }
      } else {
        // Not recurring, ensure we decouple if previously recurring
        recurrenceRuleId = null;
      }

      // 2. Prepare Event Data
      const submitData: ScheduledEventSubmitData = {
        id: this.event?.id || 0,
        name: this.eventForm.get('name')?.value.trim(),
        description: this.eventForm.get('description')?.value?.trim() || null,
        schedulingTargetId: this.eventForm.get('schedulingTargetId')?.value || null,
        startDateTime: this.eventForm.get('startDateTime')?.value,
        endDateTime: this.eventForm.get('endDateTime')?.value,
        location: this.eventForm.get('location')?.value?.trim() || null,
        timeZoneId: this.eventForm.get('timeZoneId')?.value || null,
        notes: this.eventForm.get('notes')?.value?.trim() || null,
        externalId: this.eventForm.get('externalId')?.value?.trim() || null,
        versionNumber: this.event?.versionNumber || 0,
        recurrenceRuleId: recurrenceRuleId as number,
        scheduledEventTemplateId: null,
        clientId: null,
        resourceId: null,
        crewId: null,
        parentScheduledEventId: null,
        recurrenceInstanceDate: null,
        attributes: null,
        isAllDay: false,
        priorityId: null as any,
        eventStatusId: 1,
        bookingSourceTypeId: null,
        officeId: null,
        partySize: null,
        color: null,
        active: true,
        deleted: false
      };

      // 3. Save Event
      const savedEvent = this.isEditMode
        ? await lastValueFrom(this.scheduledEventService.PutScheduledEvent(submitData.id, submitData))
        : await lastValueFrom(this.scheduledEventService.PostScheduledEvent(submitData));

      // 4. Handle Assignments
      await this.handleAssignmentsSave(Number(savedEvent.id));

      this.saving = false;
      this.saved.emit(true);
      this.activeModal.close(true);
      this.alertService.showMessage(
        this.isEditMode ? 'Event updated successfully' : 'Event created successfully',
        '',
        MessageSeverity.success
      );

    } catch (err: any) {
      this.saving = false;
      this.alertService.showMessage('Failed to save event', err.message || 'Unknown error', MessageSeverity.error);
    }
  }

  private async handleAssignmentsSave(eventId: number): Promise<void> {
    // Collect the IDs of assignments currently in the form
    const currentFormIds = new Set<number>();

    for (const control of this.assignments.controls) {
      const assignmentId = control.get('id')?.value;
      if (assignmentId) {
        currentFormIds.add(Number(assignmentId));
      }
    }

    // Delete removed assignments (existed before but no longer in form)
    for (const existingId of this.existingAssignmentIds) {
      if (!currentFormIds.has(existingId)) {
        try {
          await lastValueFrom(this.assignmentService.DeleteEventResourceAssignment(existingId));
        } catch (err: any) {
          console.error(`Failed to delete assignment ${existingId}`, err);
        }
      }
    }

    // Create or update assignments
    for (const control of this.assignments.controls) {
      const assignmentId = control.get('id')?.value;
      const submitData = new EventResourceAssignmentSubmitData();

      submitData.scheduledEventId = eventId;
      submitData.resourceId = control.get('resourceId')?.value || null;
      submitData.crewId = control.get('crewId')?.value || null;
      submitData.assignmentRoleId = control.get('assignmentRoleId')?.value || null;
      submitData.assignmentStartDateTime = control.get('assignmentStartDateTime')?.value || null;
      submitData.assignmentEndDateTime = control.get('assignmentEndDateTime')?.value || null;
      submitData.notes = control.get('notes')?.value || null;
      submitData.active = true;
      submitData.deleted = false;
      submitData.isVolunteer = false;
      submitData.reimbursementRequested = false;
      submitData.assignmentStatusId = 1; // Default status

      try {
        if (assignmentId && Number(assignmentId) > 0) {
          // Update existing
          submitData.id = Number(assignmentId);
          submitData.versionNumber = 0; // Server handles optimistic concurrency
          await lastValueFrom(this.assignmentService.PutEventResourceAssignment(submitData.id, submitData));
        } else {
          // Create new
          submitData.id = 0 as any;
          submitData.versionNumber = 0 as any;
          await lastValueFrom(this.assignmentService.PostEventResourceAssignment(submitData));
        }
      } catch (err: any) {
        console.error('Failed to save assignment', err);
        // Continue with other assignments rather than failing completely
      }
    }
  }

  // -------------------------------------------------------------------------
  // Modal Control
  // -------------------------------------------------------------------------
  open(event?: ScheduledEventData): void {
    this.event = event || null;
    this.isVisible = true;
    this.isEditMode = !!event;

    // Reset form and state
    this.eventForm.reset();
    this.assignments.clear();
    this.isRecurring = false;
    this.recurrenceRule = null;
    this.existingAssignmentIds.clear();

    if (this.isEditMode && this.event) {
      // populateForm will handle setting isRecurring and recurrenceRule
      this.populateForm(this.event);
    }
  }

  close(): void {
    this.isVisible = false;
    this.activeModal.dismiss('cancel');
  }
}
