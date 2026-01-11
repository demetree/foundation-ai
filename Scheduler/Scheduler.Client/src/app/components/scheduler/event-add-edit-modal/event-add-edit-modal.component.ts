import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import {  NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { forkJoin, Subscription } from 'rxjs';
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

  private subscriptions = new Subscription();


  //
  // Hacks for compilation purposes only
  //
  public currentAssignments: Array<EventResourceAssignmentData> = new Array<EventResourceAssignmentData>();
  public recurrenceInterval: any | null = null;
  public recurrenceFrequency: string | null = null;
  public recurrenceEndType: string | null = null;
  public recurrenceEndDate: string | null = null;
  public recurrenceCount: number | null = null;

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
      assignments.forEach(assignment => this.addAssignmentToForm(assignment));
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

    // TODO: Apply default assignments from template when you add that feature
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
      resourceId: [null, Validators.required],
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
      crewId: [null, Validators.required],
      assignmentRoleId: [null],
      assignmentStartDateTime: [this.eventForm.get('startDateTime')?.value],
      assignmentEndDateTime: [this.eventForm.get('endDateTime')?.value],
      notes: ['']
    });
    this.assignments.push(group);
    this.validateQualifications();
  }

  editAssignment(assignnment: any) {
    alert('fix this');
  }



  removeAssignment(index: any): void {
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
  private validateQualifications(): void {
    this.qualificationWarnings = [];

    const targetId = this.eventForm.get('schedulingTargetId')?.value;

    // Collect all required qualifications from role and target
    const requiredQualIds = new Set<number>();

    // From assignments (role requirements)
    this.assignments.controls.forEach(control => {
      const roleId = control.get('assignmentRoleId')?.value;
      if (roleId) {
        // You'd load these in advance or query here
        // For demo: assume you have a method to get required quals for role
      }
    });

    // From target
    if (targetId) {
      // Load target requirements and add to set
    }

    // Check each assigned resource
    this.assignments.controls.forEach(control => {
      const resourceId = control.get('resourceId')?.value;
      if (resourceId && requiredQualIds.size > 0) {
        // Check if resource has all required quals
        // Add warning if missing
      }
    });
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
  }

  // -------------------------------------------------------------------------
  // Save Logic
  // -------------------------------------------------------------------------
  save(): void {
    if (this.eventForm.invalid) {
      this.alertService.showMessage('Please correct form errors', '', MessageSeverity.warn);
      this.eventForm.markAllAsTouched();
      return;
    }

    if (this.qualificationWarnings.length > 0) {
      this.alertService.showMessage('Resolve qualification warnings before saving', '', MessageSeverity.warn);
      return;
    }

    this.saving = true;

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
      recurrenceRuleId: null,
      scheduledEventTemplateId: null, // need to fix this
      clientId: null,     // need to fix this
      resourceId: null,   // need to fix this
      crewId: null,     // fix this
      parentScheduledEventId: null,
      recurrenceInstanceDate: null,
      attributes: null,
      isAllDay: false,
      priorityId: null,
      eventStatusId: 1,   // fix this
      bookingSourceTypeId: null,  // fix this
      officeId: null, // fix this
      partySize: null,
      color: null,
      active: true,
      deleted: false
    };

    const eventSave$ = this.isEditMode
      ? this.scheduledEventService.PutScheduledEvent(submitData.id, submitData)
      : this.scheduledEventService.PostScheduledEvent(submitData);

    eventSave$.subscribe({
      next: (savedEvent) => {
        // Handle assignments save (your existing logic)
        this.handleAssignmentsSave(savedEvent.id as number);
      },
      error: (err) => {
        this.saving = false;
        this.alertService.showMessage('Failed to save event', err.message || 'Unknown error', MessageSeverity.error);
      }
    });
  }

  private handleAssignmentsSave(eventId: number): void {
    // Your existing assignment save logic (delete old, create new)
    // ... (reuse your previous implementation)
    this.saving = false;
    this.saved.emit(true);
    this.activeModal.close(true);
    this.alertService.showMessage(
      this.isEditMode ? 'Event updated successfully' : 'Event created successfully',
      '',
      MessageSeverity.success
    );
  }

  // -------------------------------------------------------------------------
  // Modal Control
  // -------------------------------------------------------------------------
  open(event?: ScheduledEventData): void {
    this.event = event || null;
    this.isVisible = true;
    this.isEditMode = !!event;
    if (event) {
      this.populateForm(event);
    }
  }

  close(): void {
    this.isVisible = false;
    this.activeModal.dismiss('cancel');
  }
}
