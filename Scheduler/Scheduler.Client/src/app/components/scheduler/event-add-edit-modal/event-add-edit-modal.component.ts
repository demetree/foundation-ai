import { Component, OnInit, OnDestroy, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { forkJoin, Subscription, lastValueFrom } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ScheduledEventService, ScheduledEventData, ScheduledEventSubmitData } from '../../../scheduler-data-services/scheduled-event.service';
import { EventResourceAssignmentService, EventResourceAssignmentData, EventResourceAssignmentSubmitData } from '../../../scheduler-data-services/event-resource-assignment.service';
import { CalendarService, CalendarData } from '../../../scheduler-data-services/calendar.service';
import { EventCalendarService, EventCalendarData, EventCalendarSubmitData } from '../../../scheduler-data-services/event-calendar.service';
import { SchedulingTargetService, SchedulingTargetData } from '../../../scheduler-data-services/scheduling-target.service';
import { ScheduledEventTemplateService, ScheduledEventTemplateData, ScheduledEventTemplateSubmitData } from '../../../scheduler-data-services/scheduled-event-template.service';
import { CrewService, CrewData } from '../../../scheduler-data-services/crew.service';
import { CrewMemberService, CrewMemberData } from '../../../scheduler-data-services/crew-member.service';
import { ResourceService, ResourceData } from '../../../scheduler-data-services/resource.service';
import { AssignmentRoleService, AssignmentRoleData } from '../../../scheduler-data-services/assignment-role.service';
import { QualificationService, QualificationData } from '../../../scheduler-data-services/qualification.service';
import {
  ScheduledEventQualificationRequirementService,
  ScheduledEventQualificationRequirementData,
  ScheduledEventQualificationRequirementSubmitData
} from '../../../scheduler-data-services/scheduled-event-qualification-requirement.service';
import {
  ScheduledEventTemplateQualificationRequirementService,
  ScheduledEventTemplateQualificationRequirementSubmitData
} from '../../../scheduler-data-services/scheduled-event-template-qualification-requirement.service';
import { AssignmentRoleQualificationRequirementService } from '../../../scheduler-data-services/assignment-role-qualification-requirement.service';
import { SchedulingTargetQualificationRequirementService } from '../../../scheduler-data-services/scheduling-target-qualification-requirement.service';
import { ResourceQualificationService } from '../../../scheduler-data-services/resource-qualification.service';
import { RecurrenceRuleService, RecurrenceRuleData } from '../../../scheduler-data-services/recurrence-rule.service';
import { ResourceAvailabilityService, ResourceAvailabilityData } from '../../../scheduler-data-services/resource-availability.service';
import { ResourceShiftService, ResourceShiftData } from '../../../scheduler-data-services/resource-shift.service';
import { ResourceScheduleContextService } from '../../../services/resource-schedule-context.service';
import { RecurrenceExceptionService, RecurrenceExceptionData, RecurrenceExceptionSubmitData } from '../../../scheduler-data-services/recurrence-exception.service';
import { TimeZoneService, TimeZoneData } from '../../../scheduler-data-services/time-zone.service';
import { EventStatusService, EventStatusData } from '../../../scheduler-data-services/event-status.service';
import { PriorityService, PriorityData } from '../../../scheduler-data-services/priority.service';
import { OfficeService, OfficeData } from '../../../scheduler-data-services/office.service';
import { ClientService, ClientData } from '../../../scheduler-data-services/client.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduledEventDependencyService, ScheduledEventDependencyData, ScheduledEventDependencySubmitData } from '../../../scheduler-data-services/scheduled-event-dependency.service';
import { DependencyTypeService, DependencyTypeData } from '../../../scheduler-data-services/dependency-type.service';
import { BookingSourceTypeService, BookingSourceTypeData } from '../../../scheduler-data-services/booking-source-type.service';
import { ScheduledEventBasicListData } from '../../../scheduler-data-services/scheduled-event.service';
import { ConflictDetectionService } from '../../../services/conflict-detection.service';
import { InputDialogService } from '../../../services/input-dialog.service';
import { SchedulerHelperService } from '../../../services/scheduler-helper.service';
import { EventChargeService, EventChargeData } from '../../../scheduler-data-services/event-charge.service';
import { FinancialTransactionService, FinancialTransactionData } from '../../../scheduler-data-services/financial-transaction.service';
import { ChargeTypeService, ChargeTypeData } from '../../../scheduler-data-services/charge-type.service';
import { ChargeStatusService, ChargeStatusData } from '../../../scheduler-data-services/charge-status.service';
import { CurrencyService, CurrencyData } from '../../../scheduler-data-services/currency.service';
import { InvoiceHelperService } from '../../../services/invoice-helper.service';
import { InvoiceService, InvoiceData } from '../../../scheduler-data-services/invoice.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { SchedulerModeService } from '../../../services/scheduler-mode.service';

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
  @Input() initialResourceId: number | null = null;
  @Input() initialCalendarIds: number[] = [];
  @Input() initialIsVolunteer: boolean = false;

  // -------------------------------------------------------------------------
  // UI State
  // -------------------------------------------------------------------------
  isEditMode = false;
  isVisible = false;
  saving = false;
  isRecurring = false;
  recurrenceRule: RecurrenceRuleData | null = null;
  activeTab = 'basic';
  isSimpleMode = true;

  // Dynamic attributes
  attributesParsed: any = {};

  onDynamicAttributeChange(data: any) {
    this.attributesParsed = data;
    this.eventForm.markAsDirty();
  }

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
  timeZones: TimeZoneData[] = [];
  eventStatuses: EventStatusData[] = [];
  priorities: PriorityData[] = [];
  offices: OfficeData[] = [];
  clients: ClientData[] = [];
  bookingSourceTypes: BookingSourceTypeData[] = [];

  // Calendar assignment
  calendars: CalendarData[] = [];
  selectedCalendarIds: Set<number> = new Set();
  existingEventCalendars: EventCalendarData[] = [];

  // Recurrence Exceptions
  recurrenceExceptions: RecurrenceExceptionData[] = [];
  newException = { exceptionDateTime: '', movedToDateTime: '', reason: '' };
  pendingNewExceptions: { exceptionDateTime: string; movedToDateTime: string | null; reason: string | null }[] = [];
  deletedExceptionIds: number[] = [];
  addingException = false;

  // Derived data
  selectedTarget: SchedulingTargetData | null = null;
  qualificationWarnings: string[] = [];
  schedulingWarnings: string[] = [];

  // Shift preview state (Feature 4)
  shiftPreviews: Map<number, string> = new Map();
  loadingShiftPreviews: Set<number> = new Set();

  // Event-specific qualification requirements
  allQualifications: QualificationData[] = [];
  existingEventQualReqs: ScheduledEventQualificationRequirementData[] = [];
  pendingNewQualReqs: number[] = [];  // qualificationIds to add
  deletedQualReqIds: number[] = [];   // existing requirement IDs to delete
  selectedNewQualId: number | null = null;

  // Track which existing assignments to keep (for diff-based save)
  private existingAssignmentIds: Set<number> = new Set();

  // Dependency state
  predecessors: ScheduledEventDependencyData[] = [];
  successors: ScheduledEventDependencyData[] = [];
  dependencyTypes: DependencyTypeData[] = [];
  allEventsList: ScheduledEventBasicListData[] = [];
  pendingNewDeps: { predecessorEventId: number; successorEventId: number; dependencyTypeId: number; lagMinutes: number }[] = [];
  deletedDepIds: number[] = [];
  newDep = { eventId: 0, dependencyTypeId: 0, lagMinutes: 0, direction: 'predecessor' as 'predecessor' | 'successor' };
  addingDependency = false;

  private subscriptions = new Subscription();

  // Financials tab state (lazy-loaded)
  eventCharges: EventChargeData[] = [];
  eventTransactions: FinancialTransactionData[] = [];
  financialsLoaded = false;
  loadingFinancials = false;
  financialsTotal = { charges: 0, income: 0, expenses: 0, net: 0 };

  // Add Charge inline form (P0-1)
  showAddCharge = false;
  savingCharge = false;
  chargeTypes: ChargeTypeData[] = [];
  chargeStatuses: ChargeStatusData[] = [];
  chargeCurrencies: CurrencyData[] = [];
  chargeLookupsLoaded = false;
  newCharge = {
    chargeTypeId: null as number | null,
    chargeStatusId: null as number | null,
    description: '',
    quantity: 1,
    unitPrice: 0,
    currencyId: null as number | null,
    isDeposit: false,
    notes: ''
  };

  // Generate Invoice (P0-2)
  generatingInvoice = false;
  generatedInvoice: { invoiceId: number; invoiceNumber: string } | null = null;

  // Linked invoices (P2-8)
  eventInvoices: InvoiceData[] = [];

  // Track whether the user has manually set the end date
  private endDateManuallySet = false;

  // Financial Timeline
  financialTimeline: TimelineEntry[] = [];

  // Booking conflict detection
  conflictWarnings: ConflictEvent[] = [];
  checkingConflicts = false;

  constructor(
    public activeModal: NgbActiveModal,
    private fb: FormBuilder,
    private http: HttpClient,
    private scheduledEventService: ScheduledEventService,
    private assignmentService: EventResourceAssignmentService,
    private targetService: SchedulingTargetService,
    private templateService: ScheduledEventTemplateService,
    private crewService: CrewService,
    private crewMemberService: CrewMemberService,
    private resourceService: ResourceService,
    private roleService: AssignmentRoleService,
    private qualificationService: QualificationService,
    private roleQualService: AssignmentRoleQualificationRequirementService,
    private targetQualService: SchedulingTargetQualificationRequirementService,
    private resourceQualService: ResourceQualificationService,
    private recurrenceRuleService: RecurrenceRuleService,
    private recurrenceExceptionService: RecurrenceExceptionService,
    private timeZoneService: TimeZoneService,
    private eventStatusService: EventStatusService,
    private priorityService: PriorityService,
    private officeService: OfficeService,
    private clientService: ClientService,
    private alertService: AlertService,
    private dependencyService: ScheduledEventDependencyService,
    private dependencyTypeService: DependencyTypeService,
    private bookingSourceTypeService: BookingSourceTypeService,
    private eventQualReqService: ScheduledEventQualificationRequirementService,
    private templateQualReqService: ScheduledEventTemplateQualificationRequirementService,
    private conflictDetectionService: ConflictDetectionService,
    private inputDialogService: InputDialogService,
    private calendarService: CalendarService,
    private eventCalendarService: EventCalendarService,
    private resourceAvailabilityService: ResourceAvailabilityService,
    private resourceShiftService: ResourceShiftService,
    private resourceScheduleContextService: ResourceScheduleContextService,
    private schedulerHelperService: SchedulerHelperService,
    private eventChargeService: EventChargeService,
    private financialTransactionService: FinancialTransactionService,
    private chargeTypeService: ChargeTypeService,
    private chargeStatusService: ChargeStatusService,
    private currencyService: CurrencyService,
    private invoiceHelperService: InvoiceHelperService,
    private invoiceService: InvoiceService,
    private router: Router,
    private authService: AuthService,
    private schedulerModeService: SchedulerModeService,
    private confirmationService: ConfirmationService
  ) {
    this.buildForm();

    //
    // Subscribe to the mode service for the event editor
    //
    this.subscriptions.add(
      this.schedulerModeService.isSimpleMode('eventEditor')
        .subscribe(simple => this.isSimpleMode = simple)
    );
  }


  private getAuthHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': 'Bearer ' + this.authService.accessToken
    });
  }

  ngOnInit(): void {
    this.isEditMode = !!this.event;
    this.loadLookupData();

    if (!this.isEditMode) {
      // Pre-fill dates from calendar selection
      // FullCalendar gives local ISO strings — just trim for datetime-local input
      if (this.initialStart) {
        this.eventForm.patchValue({
          startDateTime: this.initialStart.slice(0, 16),
          endDateTime: this.initialEnd?.slice(0, 16) || this.initialStart.slice(0, 16)
        });
      }

      // Pre-add a resource assignment if an initial resource was specified
      if (this.initialResourceId) {
        this.addIndividualAssignment();
        const lastIdx = this.assignments.length - 1;
        this.assignments.at(lastIdx).patchValue({ resourceId: this.initialResourceId });
      }

      // Pre-select calendars from the calendar view the user launched from
      if (this.initialCalendarIds.length > 0) {
        this.selectedCalendarIds = new Set(this.initialCalendarIds);
      }
    } else if (this.event) {
      this.populateForm(this.event);
      this.loadExistingDependencies();
    }
  }

  /**
   * Toggles the event editor between simple and advanced mode.
   * This sets a component-level override independent of the global mode.
   */
  toggleEventEditorMode(): void {
    const newMode = this.isSimpleMode ? 'advanced' : 'simple';
    this.schedulerModeService.setComponentOverride('eventEditor', newMode);
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
      externalId: [''],
      eventStatusId: [null],
      priorityId: [null],
      officeId: [null],
      clientId: [null],
      bookingSourceTypeId: [null],
      color: [null],
      isAllDay: [false],
      bookingContactName: [''],
      bookingContactEmail: [''],
      bookingContactPhone: [''],
      isOpenForVolunteers: [false],
      maxVolunteerSlots: [null]
    });

    // Assignments array (managed separately for easier sub-table integration)
    this.assignmentsFormArray = this.fb.array([]);
    this.eventForm.addControl('assignments', this.assignmentsFormArray);

    // Auto-populate Office and Client when Target changes
    this.eventForm.get('schedulingTargetId')?.valueChanges.subscribe(targetId => {
      if (targetId) {
        const target = this.targets.find(t => Number(t.id) === Number(targetId));
        if (target) {
          // Auto-set Office and Client from the selected Target
          // We overwrite existing values to ensure data consistency with the selected Target
          this.eventForm.patchValue({
            officeId: target.officeId,
            clientId: target.clientId
          });
        }
      }
    });

    // Auto-set end date when start changes (default +1h if end is empty)
    this.eventForm.get('startDateTime')?.valueChanges.subscribe(startVal => {
      if (!startVal) return;
      const endVal = this.eventForm.get('endDateTime')?.value;
      // Only auto-fill if end hasn't been manually set
      if (!endVal && !this.endDateManuallySet) {
        const start = new Date(startVal);
        start.setMinutes(start.getMinutes() + 60);
        const y = start.getFullYear();
        const mo = String(start.getMonth() + 1).padStart(2, '0');
        const d = String(start.getDate()).padStart(2, '0');
        const h = String(start.getHours()).padStart(2, '0');
        const mi = String(start.getMinutes()).padStart(2, '0');
        this.eventForm.patchValue({ endDateTime: `${y}-${mo}-${d}T${h}:${mi}` }, { emitEvent: false });
      }
    });

    // Track when user manually sets end date
    this.eventForm.get('endDateTime')?.valueChanges.subscribe(() => {
      this.endDateManuallySet = true;
      this.checkConflicts();
    });

    // Also check conflicts when start date changes
    this.eventForm.get('startDateTime')?.valueChanges.subscribe(() => {
      this.checkConflicts();
    });
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
      roles: this.roleService.GetAssignmentRoleList({ active: true }),
      timeZones: this.timeZoneService.GetTimeZoneList({ active: true }),
      eventStatuses: this.eventStatusService.GetEventStatusList({ active: true }),
      priorities: this.priorityService.GetPriorityList({ active: true }),
      offices: this.officeService.GetOfficeList({ active: true }),
      clients: this.clientService.GetClientList({ active: true }),
      bookingSourceTypes: this.bookingSourceTypeService.GetBookingSourceTypeList({ active: true }),
      qualifications: this.qualificationService.GetQualificationList({ active: true }),
      calendars: this.calendarService.GetCalendarList({ active: true, deleted: false })
    }).subscribe({
      next: (data) => {
        this.templates = data.templates;
        this.targets = data.targets;
        this.crews = data.crews;
        this.resources = data.resources;
        this.roles = data.roles;
        this.timeZones = data.timeZones;
        this.eventStatuses = data.eventStatuses;
        this.priorities = data.priorities;
        this.offices = data.offices;
        this.clients = data.clients;
        this.bookingSourceTypes = data.bookingSourceTypes;
        this.allQualifications = data.qualifications;
        this.calendars = data.calendars;

        if (this.isEditMode) {
          this.loadExistingAssignments();
          this.loadExistingExceptions();
          this.loadExistingEventQualReqs();
          this.loadExistingCalendarAssignments();
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

  private loadExistingExceptions(): void {
    if (!this.event) return;

    this.scheduledEventService.GetRecurrenceExceptionsForScheduledEvent(
      Number(this.event.id)
    ).subscribe(exceptions => {
      this.recurrenceExceptions = exceptions;
    });
  }

  // -------------------------------------------------------------------------
  // Tab Navigation
  // -------------------------------------------------------------------------
  setActiveTab(tab: string): void {
    this.activeTab = tab;
    if (tab === 'financials' && !this.financialsLoaded && this.isEditMode) {
      this.loadEventFinancials();
      this.loadEventTimeline();
    }
  }

  private loadEventFinancials(): void {
    if (!this.event || this.loadingFinancials) return;
    this.loadingFinancials = true;

    const requests: any = {
      charges: this.eventChargeService.GetEventChargeList({
        scheduledEventId: this.event.id,
        active: true,
        deleted: false,
        includeRelations: true
      }),
      transactions: this.financialTransactionService.GetFinancialTransactionList({
        scheduledEventId: this.event.id,
        active: true,
        deleted: false,
        includeRelations: true
      }),
      invoices: this.invoiceService.GetInvoiceList({
        scheduledEventId: this.event.id,
        active: true,
        deleted: false,
        includeRelations: true
      })
    };

    // Load charge form lookups lazily on first Financials tab visit
    if (!this.chargeLookupsLoaded) {
      requests.chargeTypes = this.chargeTypeService.GetChargeTypeList({ active: true });
      requests.chargeStatuses = this.chargeStatusService.GetChargeStatusList({ active: true });
      requests.currencies = this.currencyService.GetCurrencyList({ active: true });
    }

    forkJoin(requests).subscribe({
      next: (data: any) => {
        this.eventCharges = data.charges;
        this.eventTransactions = data.transactions;
        this.eventInvoices = data.invoices ?? [];
        this.financialsLoaded = true;
        this.loadingFinancials = false;

        // Populate charge form lookups
        if (data.chargeTypes) {
          this.chargeTypes = data.chargeTypes;
          this.chargeStatuses = data.chargeStatuses;
          this.chargeCurrencies = data.currencies;
          this.chargeLookupsLoaded = true;

          // Default currency to first available
          if (this.chargeCurrencies.length > 0 && !this.newCharge.currencyId) {
            this.newCharge.currencyId = Number(this.chargeCurrencies[0].id);
          }
          // Default charge status to first available
          if (this.chargeStatuses.length > 0 && !this.newCharge.chargeStatusId) {
            this.newCharge.chargeStatusId = Number(this.chargeStatuses[0].id);
          }
        }

        // Calculate totals
        this.financialsTotal.charges = this.eventCharges.reduce(
          (sum: number, c: any) => sum + (Number(c.totalAmount) || 0), 0
        );
        this.financialsTotal.income = this.eventTransactions
          .filter((t: any) => t.isRevenue === true)
          .reduce((sum: number, t: any) => sum + (Number(t.amount) || 0), 0);
        this.financialsTotal.expenses = this.eventTransactions
          .filter((t: any) => t.isRevenue === false)
          .reduce((sum: number, t: any) => sum + (Number(t.amount) || 0), 0);
        this.financialsTotal.net = this.financialsTotal.income - this.financialsTotal.expenses;
      },
      error: () => {
        this.loadingFinancials = false;
        this.alertService.showMessage('Failed to load financial data', '', MessageSeverity.error);
      }
    });
  }


  // -------------------------------------------------------------------------
  // Deposit Lifecycle
  // -------------------------------------------------------------------------
  showDepositsOnly = false;

  hasDeposits(): boolean {
    return this.eventCharges.some(c => c.isDeposit);
  }

  getFilteredCharges(): EventChargeData[] {
    if (!this.showDepositsOnly) return this.eventCharges;
    return this.eventCharges.filter(c => c.isDeposit);
  }

  refundDeposit(charge: EventChargeData): void {
    if (!charge || !charge.isDeposit || charge.depositRefundedDate) return;

    const now = new Date().toISOString();

    //
    // Route through FinancialManagementService endpoint instead of the
    // code-generated EventCharge controller (which is now read-only).
    //
    const submitData = this.eventChargeService.ConvertToEventChargeSubmitData(charge);
    submitData.depositRefundedDate = now;

    this.http.put(`/api/financial/charges/${charge.id}`, submitData, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: () => {
        //
        // Update the local charge object so the UI reflects the change immediately
        //
        charge.depositRefundedDate = now;
        this.alertService.showMessage('Deposit marked as refunded', '', MessageSeverity.success);
      },
      error: (err: any) => {
        this.alertService.showMessage('Failed to refund deposit', err?.message || '', MessageSeverity.error);
      }
    });
  }


  // -------------------------------------------------------------------------
  // Add Charge (P0-1)
  // -------------------------------------------------------------------------
  openAddCharge(): void {
    this.showAddCharge = true;
    this.resetNewCharge();
  }

  cancelAddCharge(): void {
    this.showAddCharge = false;
  }

  private resetNewCharge(): void {
    this.newCharge = {
      chargeTypeId: null,
      chargeStatusId: this.chargeStatuses.length > 0 ? Number(this.chargeStatuses[0].id) : null,
      description: '',
      quantity: 1,
      unitPrice: 0,
      currencyId: this.chargeCurrencies.length > 0 ? Number(this.chargeCurrencies[0].id) : null,
      isDeposit: false,
      notes: ''
    };
  }

  submitNewCharge(): void {
    if (!this.event || this.savingCharge) return;
    if (!this.newCharge.chargeTypeId || !this.newCharge.chargeStatusId || !this.newCharge.currencyId) {
      this.alertService.showMessage('Please fill in all required fields', '', MessageSeverity.warn);
      return;
    }

    this.savingCharge = true;

    const body = {
      scheduledEventId: Number(this.event.id),
      chargeTypeId: this.newCharge.chargeTypeId,
      chargeStatusId: this.newCharge.chargeStatusId,
      quantity: this.newCharge.quantity,
      unitPrice: this.newCharge.unitPrice,
      description: this.newCharge.description || null,
      currencyId: this.newCharge.currencyId,
      resourceId: null,
      rateTypeId: null,
      taxCodeId: null,
      notes: this.newCharge.notes || null,
      isAutomatic: false,
      isDeposit: this.newCharge.isDeposit
    };

    this.http.post('/api/financial/charges', body, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: () => {
        this.alertService.showMessage('Charge added successfully', '', MessageSeverity.success);
        this.savingCharge = false;
        this.showAddCharge = false;
        // Refresh charges
        this.financialsLoaded = false;
        this.eventChargeService.ClearAllCaches();
        this.loadEventFinancials();
        this.loadEventTimeline();
      },
      error: (err: any) => {
        this.alertService.showMessage('Failed to add charge', err?.error?.error || '', MessageSeverity.error);
        this.savingCharge = false;
      }
    });
  }


  // -------------------------------------------------------------------------
  // Generate Invoice (P0-2)
  // -------------------------------------------------------------------------
  generateInvoice(): void {
    if (!this.event || this.generatingInvoice) return;

    if (!this.event.clientId) {
      this.alertService.showMessage(
        'Client Required',
        'Please assign and save a client to this event before generating an invoice.',
        MessageSeverity.warn
      );
      return;
    }

    this.generatingInvoice = true;
    this.generatedInvoice = null;

    this.invoiceHelperService.createFromEvent(Number(this.event.id)).subscribe({
      next: (result) => {
        this.generatedInvoice = result;
        this.generatingInvoice = false;
        this.alertService.showMessage(
          `Invoice ${result.invoiceNumber} created`,
          'Draft invoice generated from event charges.',
          MessageSeverity.success
        );
        // Refresh linked invoices
        this.invoiceService.ClearAllCaches();
        if (this.event) {
          this.invoiceService.GetInvoiceList({
            scheduledEventId: this.event.id,
            active: true,
            deleted: false,
            includeRelations: true
          }).subscribe(invoices => this.eventInvoices = invoices);
        }
      },
      error: (err: any) => {
        this.generatingInvoice = false;
        this.alertService.showMessage(
          'Failed to generate invoice',
          err?.error?.error || err?.message || '',
          MessageSeverity.error
        );
      }
    });
  }

  viewInvoice(invoiceId: number | bigint): void {
    this.activeModal.dismiss('navigate');
    this.router.navigate(['/finances/invoices', Number(invoiceId)]);
  }


  // -------------------------------------------------------------------------
  // Charge Status Update (P2-9) and Charge Delete
  // -------------------------------------------------------------------------
  updateChargeStatus(charge: any, newStatusId: any): void {
    if (!charge || !newStatusId) return;

    const body = {
      scheduledEventId: Number(charge.scheduledEventId),
      chargeTypeId: Number(charge.chargeTypeId),
      chargeStatusId: Number(newStatusId),
      quantity: charge.quantity ?? 1,
      unitPrice: charge.unitPrice ?? 0,
      description: charge.description || null,
      currencyId: Number(charge.currencyId),
      resourceId: charge.resourceId ? Number(charge.resourceId) : null,
      rateTypeId: charge.rateTypeId ? Number(charge.rateTypeId) : null,
      taxCodeId: charge.taxCodeId ? Number(charge.taxCodeId) : null,
      notes: charge.notes || null,
      isAutomatic: charge.isAutomatic ?? false,
      isDeposit: charge.isDeposit ?? false
    };

    this.http.put(`/api/financial/charges/${charge.id}`, body, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: () => {
        charge.chargeStatusId = newStatusId;
        this.alertService.showMessage('Charge status updated', '', MessageSeverity.success);
        // Refresh financials — status change may trigger server-side side effects
        this.eventChargeService.ClearAllCaches();
        this.financialsLoaded = false;
        this.loadEventFinancials();
        this.loadEventTimeline();
      },
      error: (err: any) => {
        this.alertService.showMessage('Failed to update status', err?.error?.error || '', MessageSeverity.error);
      }
    });
  }

  /**
   * Returns true if the charge has been invoiced or paid (and therefore should not be deleted).
   */
  isChargeInvoicedOrPaid(charge: any): boolean {
    if (!charge?.chargeStatusId) return false;
    const status = this.chargeStatuses.find(cs => Number(cs.id) === Number(charge.chargeStatusId));
    if (!status) return false;
    const name = (status.name || '').toLowerCase();
    return name === 'invoiced' || name === 'paid';
  }

  deleteCharge(charge: any): void {
    if (!charge) return;

    // Guard: prevent deleting charges that are already invoiced or paid
    if (this.isChargeInvoicedOrPaid(charge)) {
      const status = this.chargeStatuses.find(cs => Number(cs.id) === Number(charge.chargeStatusId));
      this.alertService.showMessage(
        'Cannot delete this charge',
        `This charge has status "${status?.name}". Change its status to Pending first, or void the linked invoice.`,
        MessageSeverity.warn
      );
      return;
    }

    // Confirmation dialog — charge deletion is irreversible
    this.confirmationService.confirm(
      'Delete Charge',
      `Are you sure you want to remove charge "${charge.chargeType?.name || charge.description || 'this charge'}"?  This cannot be undone.`
    ).then((confirmed: boolean) => {
      if (!confirmed) return;

      this.http.delete(`/api/financial/charges/${charge.id}`, {
        headers: this.getAuthHeaders()
      }).subscribe({
        next: () => {
          this.alertService.showMessage('Charge removed', '', MessageSeverity.success);
          this.eventChargeService.ClearAllCaches();
          this.financialsLoaded = false;
          this.loadEventFinancials();
          this.loadEventTimeline();
        },
        error: (err: any) => {
          this.alertService.showMessage('Failed to remove charge', err?.error?.error || '', MessageSeverity.error);
        }
      });
    });
  }

  // -------------------------------------------------------------------------
  private loadEventTimeline(): void {
    if (!this.event) return;

    this.http.get<TimelineEntry[]>(
      `/api/FinancialTransactions/EventFinancialTimeline/${this.event.id}`,
      { headers: this.getAuthHeaders() }
    ).subscribe({
      next: (entries) => {
        this.financialTimeline = entries ?? [];
      },
      error: () => {
        this.financialTimeline = [];
      }
    });
  }


  // -------------------------------------------------------------------------
  // Booking Conflict Detection
  // -------------------------------------------------------------------------
  private checkConflicts(): void {
    const formVal = this.eventForm?.value;
    if (!formVal) return;

    const targetId = formVal.schedulingTargetId;
    const startVal = formVal.startDateTime;
    const endVal = formVal.endDateTime;

    //
    // Only check if we have all three values
    //
    if (!targetId || !startVal || !endVal) {
      this.conflictWarnings = [];
      return;
    }

    this.checkingConflicts = true;

    const startUtc = new Date(startVal).toISOString();
    const endUtc = new Date(endVal).toISOString();
    const excludeId = this.isEditMode && this.event ? `&excludeEventId=${this.event.id}` : '';

    const url = `/api/ScheduledEvents/CheckConflicts?schedulingTargetId=${targetId}&startDateTime=${startUtc}&endDateTime=${endUtc}${excludeId}`;

    this.http.get<ConflictResponse>(url, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: (response) => {
        this.conflictWarnings = response?.conflicts ?? [];
        this.checkingConflicts = false;
      },
      error: () => {
        this.conflictWarnings = [];
        this.checkingConflicts = false;
      }
    });
  }

  // -------------------------------------------------------------------------
  // Quick Duration Helpers
  // -------------------------------------------------------------------------
  /**
   * Sets the end date to start + the given duration in minutes.
   */
  setDuration(minutes: number): void {
    const startVal = this.eventForm.get('startDateTime')?.value;
    if (!startVal) return;

    const start = new Date(startVal);
    start.setMinutes(start.getMinutes() + minutes);
    const y = start.getFullYear();
    const mo = String(start.getMonth() + 1).padStart(2, '0');
    const d = String(start.getDate()).padStart(2, '0');
    const h = String(start.getHours()).padStart(2, '0');
    const mi = String(start.getMinutes()).padStart(2, '0');
    this.eventForm.patchValue({ endDateTime: `${y}-${mo}-${d}T${h}:${mi}` });
  }

  /**
   * Returns the currently active duration in minutes (for highlighting the active pill), or null.
   */
  getActiveDuration(): number | null {
    const startVal = this.eventForm.get('startDateTime')?.value;
    const endVal = this.eventForm.get('endDateTime')?.value;
    if (!startVal || !endVal) return null;
    const diff = (new Date(endVal).getTime() - new Date(startVal).getTime()) / 60000;
    return diff > 0 ? diff : null;
  }

  // -------------------------------------------------------------------------
  // Template Handling
  // -------------------------------------------------------------------------
  applyTemplate(): void {
    if (!this.selectedTemplateId) {
      // Reset to blank
      this.eventForm.patchValue({
        name: '',
        description: '',
        location: '',
        priorityId: null,
        isAllDay: false
      });
      return;
    }

    const template = this.templates.find(t => Number(t.id) === Number(this.selectedTemplateId));
    if (!template) return;

    // Apply template fields to form
    this.eventForm.patchValue({
      name: template.name || '',
      description: template.description || '',
      location: template.defaultLocationPattern || '',
      priorityId: template.priorityId ? Number(template.priorityId) : null,
      isAllDay: template.defaultAllDay || false
    });

    // Calculate end time from duration if start is set
    const startVal = this.eventForm.get('startDateTime')?.value;
    if (startVal && template.defaultDurationMinutes) {
      const startDate = new Date(startVal);
      startDate.setMinutes(startDate.getMinutes() + Number(template.defaultDurationMinutes));
      // Use local ISO format for datetime-local input
      const y = startDate.getFullYear();
      const mo = String(startDate.getMonth() + 1).padStart(2, '0');
      const d = String(startDate.getDate()).padStart(2, '0');
      const h = String(startDate.getHours()).padStart(2, '0');
      const mi = String(startDate.getMinutes()).padStart(2, '0');
      this.eventForm.patchValue({ endDateTime: `${y}-${mo}-${d}T${h}:${mi}` });
    }

    this.alertService.showMessage('Template Applied', `"${template.name}" applied`, MessageSeverity.success);

    // Load template qualification requirements and apply as event-level quals
    this.templateQualReqService.GetScheduledEventTemplateQualificationRequirementList({
      scheduledEventTemplateId: template.id,
      active: true
    }).subscribe({
      next: (templateQuals) => {
        // Clear any pending quals and add template's
        this.pendingNewQualReqs = [];
        for (const tq of templateQuals) {
          const qualId = Number(tq.qualificationId);
          // Avoid duplicates with existing event quals
          const alreadyExists = this.existingEventQualReqs.some(
            r => Number(r.qualificationId) === qualId && !this.deletedQualReqIds.includes(Number(r.id))
          );
          if (!alreadyExists && !this.pendingNewQualReqs.includes(qualId)) {
            this.pendingNewQualReqs.push(qualId);
          }
        }
        this.validateQualifications();
      },
      error: () => {
        // Silently skip if template quals can't be loaded
        this.validateQualifications();
      }
    });
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
    const control = this.assignments.at(index);
    if (control) {
      const current = control.get('_editing')?.value ?? false;
      if (control.get('_editing')) {
        control.get('_editing')!.setValue(!current);
      } else {
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
  // UTC ↔ Local Date Helpers
  // -------------------------------------------------------------------------

  /**
   * Convert a UTC ISO 8601 string to a local datetime-local input value.
   * e.g. "2026-02-10T14:00:00.000Z" → "2026-02-10T10:30" (for NST -3:30)
   */
  private toLocalDateTimeString(utcIso: string): string {
    const d = new Date(utcIso);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  /**
   * Convert a local datetime-local input value to a UTC ISO 8601 string.
   * e.g. "2026-02-10T10:30" → "2026-02-10T14:00:00.000Z" (for NST -3:30)
   */
  private toUtcIsoString(localStr: string): string {
    return new Date(localStr).toISOString();
  }

  // -------------------------------------------------------------------------
  // Lookup Helpers
  // -------------------------------------------------------------------------
  getResourceName(resourceId: any): string {
    if (!resourceId) return '—';
    const resource = this.resources.find(r => Number(r.id) === Number(resourceId));
    return resource?.name || `Resource #${resourceId}`;
  }

  getCrewName(crewId: any): string {
    if (!crewId) return '—';
    const crew = this.crews.find(c => Number(c.id) === Number(crewId));
    return crew?.name || `Crew #${crewId}`;
  }

  getRoleName(roleId: any): string {
    if (!roleId) return '—';
    const role = this.roles.find(r => Number(r.id) === Number(roleId));
    return role?.name || `Role #${roleId}`;
  }

  // -------------------------------------------------------------------------
  // Recurrence Exceptions
  // -------------------------------------------------------------------------
  showAddException(): void {
    this.addingException = true;
    this.newException = { exceptionDateTime: '', movedToDateTime: '', reason: '' };
  }

  cancelAddException(): void {
    this.addingException = false;
  }

  confirmAddException(): void {
    if (!this.newException.exceptionDateTime) return;

    this.pendingNewExceptions.push({
      exceptionDateTime: this.newException.exceptionDateTime,
      movedToDateTime: this.newException.movedToDateTime || null,
      reason: this.newException.reason || null
    });

    this.addingException = false;
    this.newException = { exceptionDateTime: '', movedToDateTime: '', reason: '' };
  }

  removeExistingException(index: number): void {
    const exception = this.recurrenceExceptions[index];
    if (exception) {
      this.deletedExceptionIds.push(Number(exception.id));
      this.recurrenceExceptions.splice(index, 1);
    }
  }

  removePendingException(index: number): void {
    this.pendingNewExceptions.splice(index, 1);
  }

  // -------------------------------------------------------------------------
  // Qualification Validation
  // -------------------------------------------------------------------------
  private async validateQualifications(): Promise<void> {
    this.qualificationWarnings = [];

    const targetId = this.eventForm.get('schedulingTargetId')?.value;
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
          // Silently skip
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

    // From event-specific qualification requirements
    for (const req of this.existingEventQualReqs) {
      if (!this.deletedQualReqIds.includes(Number(req.id))) {
        requiredQualIds.add(Number(req.qualificationId));
      }
    }
    for (const qualId of this.pendingNewQualReqs) {
      requiredQualIds.add(qualId);
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
    try {
      this.attributesParsed = eventData.attributes ? JSON.parse(eventData.attributes) : {};
    } catch (e) {
      this.attributesParsed = {};
    }

    this.eventForm.patchValue({
      name: eventData.name,
      description: eventData.description,
      schedulingTargetId: eventData.schedulingTargetId,
      startDateTime: this.toLocalDateTimeString(eventData.startDateTime),
      endDateTime: this.toLocalDateTimeString(eventData.endDateTime),
      location: eventData.location,
      timeZoneId: eventData.timeZoneId,
      notes: eventData.notes,
      externalId: eventData.externalId,
      eventStatusId: eventData.eventStatusId,
      priorityId: eventData.priorityId,
      officeId: eventData.officeId,
      clientId: eventData.clientId,
      bookingSourceTypeId: eventData.bookingSourceTypeId,
      color: eventData.color,
      isAllDay: eventData.isAllDay || false,
      bookingContactName: eventData.bookingContactName || '',
      bookingContactEmail: eventData.bookingContactEmail || '',
      bookingContactPhone: eventData.bookingContactPhone || '',
      isOpenForVolunteers: eventData.isOpenForVolunteers || false,
      maxVolunteerSlots: eventData.maxVolunteerSlots || null
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
      this.recurrenceRule = new RecurrenceRuleData();
      this.recurrenceRule.id = 0 as any;
      this.recurrenceRule.recurrenceFrequencyId = 2; // Daily default
      this.recurrenceRule.interval = 1;
      this.recurrenceRule.versionNumber = 0 as any;
      this.recurrenceRule.active = true;
      this.recurrenceRule.deleted = false;
    }
  }

  // -------------------------------------------------------------------------
  // Rental Agreement Helpers
  // -------------------------------------------------------------------------
  getRentalStatus(): string {
    return this.attributesParsed?.rentalAgreement?.status ?? 'none';
  }

  getRentalField(field: string): any {
    return this.attributesParsed?.rentalAgreement?.[field] ?? null;
  }

  setRentalField(field: string, value: any): void {
    if (!this.attributesParsed.rentalAgreement) {
      this.attributesParsed.rentalAgreement = {};
    }

    if (field === 'status' && value === 'none') {
      // Clear the entire rental agreement when set to "No Agreement"
      delete this.attributesParsed.rentalAgreement;
    } else {
      this.attributesParsed.rentalAgreement[field] = value || null;
    }

    this.eventForm.markAsDirty();
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

    //
    // Pre-save conflict check: warn if any assignments overlap other events
    //
    try {
      await this.checkForConflictsOnSave();
    } catch {
      // Non-blocking — continue saving even if the check fails
    }

    //
    // Pre-save availability & shift checks: warn if resources are on blackout or out of shift
    //
    this.schedulingWarnings = [];
    try {
      await this.checkResourceAvailabilityConflicts();
      await this.checkShiftBoundaryWarnings();
    } catch {
      // Non-blocking
    }

    if (this.schedulingWarnings.length > 0) {
      const warningList = this.schedulingWarnings.map(w => `• ${w}`).join('\n');
      const proceed = confirm(
        `Scheduling Warnings:\n\n${warningList}\n\nDo you want to proceed anyway?`
      );
      if (!proceed) {
        this.saving = false;
        return;
      }
      // Audit: user dismissed warnings and proceeded
      console.warn('[Scheduling Audit] User dismissed scheduling warnings:', this.schedulingWarnings);
      try {
        await this.logSchedulingWarningDismissal(this.schedulingWarnings);
      } catch {
        // Non-blocking audit — don't prevent save
      }
    }

    try {
      let recurrenceRuleId: number | null = null;

      // 1. Handle Recurrence Rule Save/Update
      if (this.isRecurring && this.recurrenceRule) {
        const ruleId = Number(this.recurrenceRule.id || 0);
        if (ruleId > 0) {
          await lastValueFrom(this.recurrenceRuleService.PutRecurrenceRule(this.recurrenceRule.id, this.recurrenceRule));
          recurrenceRuleId = ruleId;
        } else {
          const newRule = await lastValueFrom(this.recurrenceRuleService.PostRecurrenceRule(this.recurrenceRule));
          recurrenceRuleId = Number(newRule.id);
        }
      } else {
        recurrenceRuleId = null;
      }

      // 2. Prepare Event Data
      const formVal = this.eventForm.value;
      const submitData: ScheduledEventSubmitData = {
        id: this.event?.id || 0,
        name: formVal.name?.trim(),
        description: formVal.description?.trim() || null,
        schedulingTargetId: formVal.schedulingTargetId || null,
        startDateTime: this.toUtcIsoString(formVal.startDateTime),
        endDateTime: this.toUtcIsoString(formVal.endDateTime),
        location: formVal.location?.trim() || null,
        timeZoneId: formVal.timeZoneId || null,
        notes: formVal.notes?.trim() || null,
        externalId: formVal.externalId?.trim() || null,
        versionNumber: this.event?.versionNumber || 0,
        recurrenceRuleId: recurrenceRuleId as number,
        scheduledEventTemplateId: null,
        clientId: formVal.clientId || null,
        resourceId: null,
        crewId: null,
        parentScheduledEventId: null,
        recurrenceInstanceDate: null,
        attributes: Object.keys(this.attributesParsed).length > 0 ? JSON.stringify(this.attributesParsed) : null,
        isAllDay: formVal.isAllDay || false,
        isOpenForVolunteers: formVal.isOpenForVolunteers || false,
        maxVolunteerSlots: formVal.maxVolunteerSlots || null,
        priorityId: formVal.priorityId || null,
        eventStatusId: formVal.eventStatusId || 1,
        bookingSourceTypeId: formVal.bookingSourceTypeId || null,
        officeId: formVal.officeId || null,
        eventTypeId: formVal.eventTypeId || null,
        partySize: null,
        bookingContactName: formVal.bookingContactName?.trim() || null,
        bookingContactEmail: formVal.bookingContactEmail?.trim() || null,
        bookingContactPhone: formVal.bookingContactPhone?.trim() || null,
        color: formVal.color || null,
        active: true,
        deleted: false
      };

      // 3. Save Event
      const savedEvent = this.isEditMode
        ? await lastValueFrom(this.scheduledEventService.PutScheduledEvent(submitData.id, submitData))
        : await lastValueFrom(this.scheduledEventService.PostScheduledEvent(submitData));

      // 4. Handle Assignments
      await this.handleAssignmentsSave(Number(savedEvent.id));

      // 5. Handle Recurrence Exceptions
      await this.handleExceptionsSave(Number(savedEvent.id));

      // 6. Handle Dependencies
      await this.handleDependenciesSave(Number(savedEvent.id));

      // 7. Handle Event-Specific Qualification Requirements
      await this.handleQualificationReqsSave(Number(savedEvent.id));

      // 8. Handle Calendar Assignments
      await this.handleCalendarAssignmentsSave(Number(savedEvent.id));

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

  private async checkForConflictsOnSave(): Promise<void> {
    const formVal = this.eventForm.value;
    const start = formVal.startDateTime;
    const end = formVal.endDateTime;

    if (!start || !end) return;

    // Collect resource/crew IDs from current assignments
    const resourceIds = new Set<number>();
    const crewIds = new Set<number>();

    for (const control of this.assignments.controls) {
      const resId = control.get('resourceId')?.value;
      const crewId = control.get('crewId')?.value;
      if (resId) resourceIds.add(Number(resId));
      if (crewId) crewIds.add(Number(crewId));
    }

    if (resourceIds.size === 0 && crewIds.size === 0) return;

    // Load events in a window around this event's time range
    const rangeStart = new Date(new Date(start).getTime() - 86400000).toISOString(); // -1 day
    const rangeEnd = new Date(new Date(end).getTime() + 86400000).toISOString();     // +1 day

    const nearbyEvents = await lastValueFrom(
      this.schedulerHelperService.GetCalendarEvents(rangeStart, rangeEnd)
    );

    // Build a virtual event representing the form state for each resource/crew
    const virtualEvents: ScheduledEventData[] = [];
    const currentId = Number(this.event?.id || -9999);

    for (const resId of resourceIds) {
      const ve = new ScheduledEventData();
      ve.id = currentId as any;
      ve.name = formVal.name || 'This Event';
      ve.startDateTime = start;
      ve.endDateTime = end;
      ve.resourceId = resId as any;
      ve.crewId = null as any;
      virtualEvents.push(ve);
    }

    for (const crId of crewIds) {
      const ve = new ScheduledEventData();
      ve.id = currentId as any;
      ve.name = formVal.name || 'This Event';
      ve.startDateTime = start;
      ve.endDateTime = end;
      ve.resourceId = null as any;
      ve.crewId = crId as any;
      virtualEvents.push(ve);
    }

    // Filter out the current event from nearby events (to avoid self-conflict in edit mode)
    const otherEvents = nearbyEvents.filter(e => Number(e.id) !== currentId);

    // Combine and detect
    const allEvents = [...otherEvents, ...virtualEvents];
    const conflicts = this.conflictDetectionService.detectConflicts(allEvents);

    if (conflicts.length > 0) {
      const names = conflicts.map(c => {
        const other = Number(c.eventA.id) === currentId ? c.eventB.name : c.eventA.name;
        return other;
      });
      const uniqueNames = [...new Set(names)].slice(0, 3);
      const msg = `Potential scheduling conflict with: ${uniqueNames.join(', ')}${names.length > 3 ? ` (+${names.length - 3} more)` : ''}`;
      this.alertService.showMessage('Conflict Warning', msg, MessageSeverity.warn);
    }
  }

  // -------------------------------------------------------------------------
  // Resource Availability & Shift Warnings
  // -------------------------------------------------------------------------

  // -------------------------------------------------------------------------
  // Timezone & Shift Helpers
  // -------------------------------------------------------------------------

  /**
   * Converts a UTC datetime to a resource's timezone, returning day-of-week and minutes-since-midnight.
   * Uses the resource's ianaTimeZone if available, falls back to standardUTCOffsetHours.
   */
  private getEventTimeInResourceTZ(eventDateTimeLocal: string, resource: ResourceData): { dayOfWeek: number; minutes: number } {
    const date = new Date(eventDateTimeLocal);
    const tz = (resource as any).timeZone;

    if (tz?.ianaTimeZone) {
      try {
        // Use Intl API for accurate TZ conversion (handles DST)
        const parts = new Intl.DateTimeFormat('en-US', {
          timeZone: tz.ianaTimeZone,
          hour12: false,
          weekday: 'short',
          hour: '2-digit',
          minute: '2-digit'
        }).formatToParts(date);

        const hourPart = parts.find(p => p.type === 'hour');
        const minutePart = parts.find(p => p.type === 'minute');
        const weekdayPart = parts.find(p => p.type === 'weekday');

        const hours = parseInt(hourPart?.value || '0', 10);
        const mins = parseInt(minutePart?.value || '0', 10);

        const dayMap: Record<string, number> = { Sun: 0, Mon: 1, Tue: 2, Wed: 3, Thu: 4, Fri: 5, Sat: 6 };
        const dayOfWeek = dayMap[weekdayPart?.value || ''] ?? date.getDay();

        return { dayOfWeek, minutes: hours * 60 + mins };
      } catch {
        // Intl failed — fall through to offset-based approach
      }
    }

    if (tz?.standardUTCOffsetHours != null) {
      // Offset-based fallback (doesn't handle DST)
      const utcMs = date.getTime();
      const offsetMs = Number(tz.standardUTCOffsetHours) * 3600000;
      const resourceDate = new Date(utcMs + offsetMs);
      return {
        dayOfWeek: resourceDate.getUTCDay(),
        minutes: resourceDate.getUTCHours() * 60 + resourceDate.getUTCMinutes()
      };
    }

    // No timezone info — use local browser time
    return { dayOfWeek: date.getDay(), minutes: date.getHours() * 60 + date.getMinutes() };
  }

  /**
   * Resolves a crew's member resources by fetching crew members and returning their resource IDs.
   */
  private async resolveCrewMemberResourceIds(crewId: number): Promise<number[]> {
    const members = await lastValueFrom(
      this.crewMemberService.GetCrewMemberList({ crewId, active: true, deleted: false })
    );
    return members.map(m => Number(m.resourceId)).filter(id => id > 0);
  }

  /**
   * Loads shift preview text for a specific assignment row.
   * Shows the resource's shift window for the event's day-of-week.
   */
  async loadShiftPreview(rowIndex: number, resourceId: number): Promise<void> {
    if (!resourceId) {
      this.shiftPreviews.delete(rowIndex);
      return;
    }

    this.loadingShiftPreviews.add(rowIndex);
    const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

    try {
      const resource = this.resources.find(r => Number(r.id) === Number(resourceId));
      const startDT = this.eventForm.value.startDateTime;
      if (!startDT || !resource) {
        this.shiftPreviews.delete(rowIndex);
        this.loadingShiftPreviews.delete(rowIndex);
        return;
      }

      const { dayOfWeek } = this.getEventTimeInResourceTZ(startDT, resource);

      const shifts = await lastValueFrom(
        this.resourceShiftService.GetResourceShiftList({ resourceId, active: true, deleted: false })
      );

      if (!shifts || shifts.length === 0) {
        this.shiftPreviews.set(rowIndex, 'No shifts defined');
        this.loadingShiftPreviews.delete(rowIndex);
        return;
      }

      const dayShifts = shifts.filter(s => Number(s.dayOfWeek) === dayOfWeek);
      if (dayShifts.length === 0) {
        this.shiftPreviews.set(rowIndex, `No shift on ${dayNames[dayOfWeek]}`);
      } else {
        const descriptions = dayShifts.map(s => {
          const tp = String(s.startTime).split(':');
          const startMin = parseInt(tp[0], 10) * 60 + parseInt(tp[1] || '0', 10);
          const endMin = startMin + Number(s.hours) * 60;
          const startStr = `${Math.floor(startMin / 60)}:${String(startMin % 60).padStart(2, '0')}`;
          const endStr = `${Math.floor(endMin / 60)}:${String(Math.floor(endMin % 60)).padStart(2, '0')}`;
          return `${startStr}–${endStr}${s.label ? ' (' + s.label + ')' : ''}`;
        });
        this.shiftPreviews.set(rowIndex, `${dayNames[dayOfWeek]}: ${descriptions.join(', ')}`);
      }
    } catch {
      this.shiftPreviews.set(rowIndex, 'Unable to load shift');
    }
    this.loadingShiftPreviews.delete(rowIndex);
  }

  /**
   * Called when the resource selection changes in an assignment row.
   */
  onResourceChanged(rowIndex: number): void {
    const control = this.assignments.at(rowIndex);
    const resourceId = control?.get('resourceId')?.value;
    if (resourceId) {
      this.loadShiftPreview(rowIndex, Number(resourceId));
    } else {
      this.shiftPreviews.delete(rowIndex);
    }
    this.validateQualifications();
  }

  // -------------------------------------------------------------------------
  // Resource Availability & Shift Warnings
  // -------------------------------------------------------------------------

  /**
   * Checks each assigned resource for blackout/unavailability overlaps with the event time range.
   * Handles both individual resource assignments and crew member resources.
   * Populates schedulingWarnings[] with any found conflicts.
   */
  private async checkResourceAvailabilityConflicts(): Promise<void> {
    const formVal = this.eventForm.value;
    const start = formVal.startDateTime;
    const end = formVal.endDateTime;
    if (!start || !end) return;

    const eventStart = new Date(start).getTime();
    const eventEnd = new Date(end).getTime();

    // Collect all resource IDs to check (individual + crew members)
    const resourceChecks: { resourceId: number; source: string }[] = [];

    for (const control of this.assignments.controls) {
      const resourceId = control.get('resourceId')?.value;
      if (resourceId) {
        resourceChecks.push({ resourceId: Number(resourceId), source: 'direct' });
      }

      const crewId = control.get('crewId')?.value;
      if (crewId) {
        try {
          const memberResourceIds = await this.resolveCrewMemberResourceIds(Number(crewId));
          const crew = this.crews.find(c => Number(c.id) === Number(crewId));
          const crewName = crew?.name || `Crew #${crewId}`;
          for (const mResId of memberResourceIds) {
            resourceChecks.push({ resourceId: mResId, source: `crew ${crewName}` });
          }
        } catch {
          // Skip crew member resolution on error
        }
      }
    }

    for (const { resourceId, source } of resourceChecks) {
      try {
        const availabilities = await lastValueFrom(
          this.resourceAvailabilityService.GetResourceAvailabilityList({
            resourceId: resourceId,
            active: true,
            deleted: false
          })
        );

        for (const avail of availabilities) {
          const blackoutStart = new Date(avail.startDateTime).getTime();
          const blackoutEnd = avail.endDateTime
            ? new Date(avail.endDateTime).getTime()
            : new Date('2999-12-31').getTime();

          if (eventStart < blackoutEnd && blackoutStart < eventEnd) {
            const resource = this.resources.find(r => Number(r.id) === Number(resourceId));
            const resourceName = resource?.name || `Resource #${resourceId}`;
            const displayName = source === 'direct' ? resourceName : `${resourceName} (via ${source})`;
            const reason = avail.reason || 'No reason specified';
            const startStr = new Date(avail.startDateTime).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' });
            const endStr = avail.endDateTime
              ? new Date(avail.endDateTime).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
              : 'indefinitely';

            this.schedulingWarnings.push(
              `${displayName} has a blackout from ${startStr} to ${endStr} — Reason: ${reason}`
            );
          }
        }
      } catch {
        // Skip availability check on error for this resource
      }
    }
  }

  /**
   * Checks if the event falls outside each assigned resource's shift hours for the event's day of week.
   * Handles both individual resource assignments and crew member resources.
   * Uses timezone-aware time conversion when resource timezone data is available.
   * Populates schedulingWarnings[] with any found issues.
   */
  private async checkShiftBoundaryWarnings(): Promise<void> {
    const formVal = this.eventForm.value;
    const start = formVal.startDateTime;
    const end = formVal.endDateTime;
    if (!start || !end) return;

    const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

    // Collect all resource IDs to check (individual + crew members)
    const resourceChecks: { resourceId: number; source: string }[] = [];

    for (const control of this.assignments.controls) {
      const resourceId = control.get('resourceId')?.value;
      if (resourceId) {
        resourceChecks.push({ resourceId: Number(resourceId), source: 'direct' });
      }

      // Resolve crew member resources
      const crewId = control.get('crewId')?.value;
      if (crewId) {
        try {
          const memberResourceIds = await this.resolveCrewMemberResourceIds(Number(crewId));
          const crew = this.crews.find(c => Number(c.id) === Number(crewId));
          const crewName = crew?.name || `Crew #${crewId}`;
          for (const mResId of memberResourceIds) {
            resourceChecks.push({ resourceId: mResId, source: `crew ${crewName}` });
          }
        } catch {
          // Skip crew member resolution on error
        }
      }
    }

    for (const { resourceId, source } of resourceChecks) {
      try {
        const shifts = await lastValueFrom(
          this.resourceShiftService.GetResourceShiftList({
            resourceId: resourceId,
            active: true,
            deleted: false
          })
        );

        // If no shifts are defined at all for this resource, skip (don't warn)
        if (!shifts || shifts.length === 0) continue;

        const resource = this.resources.find(r => Number(r.id) === Number(resourceId));
        const resourceName = resource?.name || `Resource #${resourceId}`;
        const displayName = source === 'direct' ? resourceName : `${resourceName} (via ${source})`;

        // Use timezone-aware time computation
        const startTZ = resource
          ? this.getEventTimeInResourceTZ(start, resource)
          : { dayOfWeek: new Date(start).getDay(), minutes: new Date(start).getHours() * 60 + new Date(start).getMinutes() };
        const endTZ = resource
          ? this.getEventTimeInResourceTZ(end, resource)
          : { dayOfWeek: new Date(end).getDay(), minutes: new Date(end).getHours() * 60 + new Date(end).getMinutes() };

        const eventDayOfWeek = startTZ.dayOfWeek;
        const dayShifts = shifts.filter(s => Number(s.dayOfWeek) === eventDayOfWeek);

        if (dayShifts.length === 0) {
          this.schedulingWarnings.push(
            `${displayName} has no shift defined for ${dayNames[eventDayOfWeek]}`
          );
          continue;
        }

        // Check if the event falls within any of the defined shift windows
        let withinAnyShift = false;
        let shiftDescriptions: string[] = [];

        for (const shift of dayShifts) {
          const timeParts = String(shift.startTime).split(':');
          const shiftStartMinutes = parseInt(timeParts[0], 10) * 60 + parseInt(timeParts[1] || '0', 10);
          const shiftEndMinutes = shiftStartMinutes + (Number(shift.hours) * 60);

          const shiftStartStr = `${Math.floor(shiftStartMinutes / 60)}:${String(shiftStartMinutes % 60).padStart(2, '0')}`;
          const shiftEndStr = `${Math.floor(shiftEndMinutes / 60)}:${String(Math.floor(shiftEndMinutes % 60)).padStart(2, '0')}`;
          shiftDescriptions.push(`${shiftStartStr}–${shiftEndStr}${shift.label ? ' (' + shift.label + ')' : ''}`);

          if (startTZ.minutes >= shiftStartMinutes && endTZ.minutes <= shiftEndMinutes) {
            withinAnyShift = true;
            break;
          }
        }

        if (!withinAnyShift) {
          this.schedulingWarnings.push(
            `${displayName}'s shift on ${dayNames[eventDayOfWeek]} is ${shiftDescriptions.join(' / ')}, event may fall outside`
          );
        }
      } catch {
        // Skip shift check on error for this resource
      }
    }
  }

  /**
   * Posts dismissed scheduling warnings to the server for audit logging.
   */
  private async logSchedulingWarningDismissal(warnings: string[]): Promise<void> {
    const resourceIds: number[] = [];
    for (const control of this.assignments.controls) {
      const resId = control.get('resourceId')?.value;
      if (resId) resourceIds.push(Number(resId));
    }

    const payload = {
      eventId: this.event?.id ? Number(this.event.id) : 0,
      eventName: this.eventForm.value.name || 'New Event',
      warnings: warnings,
      resourceIds: resourceIds
    };

    try {
      await lastValueFrom(
        this.resourceScheduleContextService.LogSchedulingWarningDismissal(payload)
      );
    } catch {
      // Audit logging failure should not block the save
    }
  }


  private async handleAssignmentsSave(eventId: number): Promise<void> {
    const currentFormIds = new Set<number>();

    for (const control of this.assignments.controls) {
      const assignmentId = control.get('id')?.value;
      if (assignmentId) {
        currentFormIds.add(Number(assignmentId));
      }
    }

    // Delete removed assignments
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
      const aStart = control.get('assignmentStartDateTime')?.value;
      const aEnd = control.get('assignmentEndDateTime')?.value;
      submitData.assignmentStartDateTime = aStart ? this.toUtcIsoString(aStart) : null;
      submitData.assignmentEndDateTime = aEnd ? this.toUtcIsoString(aEnd) : null;
      submitData.notes = control.get('notes')?.value || null;
      submitData.active = true;
      submitData.deleted = false;
      submitData.isVolunteer = this.initialIsVolunteer
        && !!submitData.resourceId
        && Number(submitData.resourceId) === Number(this.initialResourceId);
      submitData.reimbursementRequested = false;
      submitData.assignmentStatusId = 1;

      try {
        if (assignmentId && Number(assignmentId) > 0) {
          submitData.id = Number(assignmentId);
          submitData.versionNumber = 0;
          await lastValueFrom(this.assignmentService.PutEventResourceAssignment(submitData.id, submitData));
        } else {
          submitData.id = 0 as any;
          submitData.versionNumber = 0 as any;
          await lastValueFrom(this.assignmentService.PostEventResourceAssignment(submitData));
        }
      } catch (err: any) {
        console.error('Failed to save assignment', err);
      }
    }
  }

  private async handleExceptionsSave(eventId: number): Promise<void> {
    // Delete removed exceptions
    for (const deletedId of this.deletedExceptionIds) {
      try {
        await lastValueFrom(this.recurrenceExceptionService.DeleteRecurrenceException(deletedId));
      } catch (err: any) {
        console.error(`Failed to delete exception ${deletedId}`, err);
      }
    }

    // Create new exceptions
    for (const pending of this.pendingNewExceptions) {
      const submitData = new RecurrenceExceptionSubmitData();
      submitData.id = 0 as any;
      submitData.scheduledEventId = eventId;
      submitData.exceptionDateTime = pending.exceptionDateTime;
      submitData.movedToDateTime = pending.movedToDateTime;
      submitData.reason = pending.reason;
      submitData.versionNumber = 0 as any;
      submitData.active = true;
      submitData.deleted = false;

      try {
        await lastValueFrom(this.recurrenceExceptionService.PostRecurrenceException(submitData));
      } catch (err: any) {
        console.error('Failed to create exception', err);
      }
    }

    // Reset state
    this.deletedExceptionIds = [];
    this.pendingNewExceptions = [];
  }


  // -------------------------------------------------------------------------
  // Calendar Assignment
  // -------------------------------------------------------------------------

  /**
   * Load existing EventCalendar associations for this event (edit mode).
   */
  private loadExistingCalendarAssignments(): void {
    if (!this.event) return;

    this.eventCalendarService.GetEventCalendarList({
      scheduledEventId: Number(this.event.id),
      active: true,
      deleted: false
    }).subscribe(eventCalendars => {
      this.existingEventCalendars = eventCalendars;
      this.selectedCalendarIds = new Set(
        eventCalendars.map(ec => Number(ec.calendarId))
      );
    });
  }


  /**
   * Toggle a calendar assignment on/off.
   */
  toggleCalendarAssignment(calendarId: bigint | number): void {
    const id = Number(calendarId);

    if (this.selectedCalendarIds.has(id)) {
      this.selectedCalendarIds.delete(id);
    } else {
      this.selectedCalendarIds.add(id);
    }
  }


  /**
   * Check if a calendar is currently assigned.
   */
  isCalendarAssigned(calendarId: bigint | number): boolean {
    return this.selectedCalendarIds.has(Number(calendarId));
  }


  /**
   * Diff-based save: delete removed associations, create new ones.
   */
  private async handleCalendarAssignmentsSave(eventId: number): Promise<void> {
    const desiredIds = this.selectedCalendarIds;
    const existingIds = new Set(this.existingEventCalendars.map(ec => Number(ec.calendarId)));

    //
    // Delete removed associations
    //
    for (const ec of this.existingEventCalendars) {
      if (!desiredIds.has(Number(ec.calendarId))) {
        try {
          await lastValueFrom(this.eventCalendarService.DeleteEventCalendar(ec.id));
        } catch (err: any) {
          console.error(`Failed to delete EventCalendar ${ec.id}`, err);
        }
      }
    }

    //
    // Create new associations
    //
    for (const calId of desiredIds) {
      if (!existingIds.has(calId)) {
        const submitData = new EventCalendarSubmitData();
        submitData.id = 0 as any;
        submitData.scheduledEventId = eventId;
        submitData.calendarId = calId;
        submitData.active = true;
        submitData.deleted = false;

        try {
          await lastValueFrom(this.eventCalendarService.PostEventCalendar(submitData));
        } catch (err: any) {
          console.error('Failed to create EventCalendar', err);
        }
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
    this.eventForm.reset({ isAllDay: false });
    this.assignments.clear();
    this.isRecurring = false;
    this.recurrenceRule = null;
    this.existingAssignmentIds.clear();
    this.recurrenceExceptions = [];
    this.pendingNewExceptions = [];
    this.deletedExceptionIds = [];
    this.activeTab = 'basic';

    if (this.isEditMode && this.event) {
      this.populateForm(this.event);
    }
  }

  close(): void {
    this.isVisible = false;
    this.activeModal.dismiss('cancel');
  }


  // -------------------------------------------------------------------------
  // Dependencies
  // -------------------------------------------------------------------------

  loadExistingDependencies(): void {
    if (!this.event) return;
    const eventId = Number(this.event.id);

    // Load predecessors (this event appears as successorEventId)
    this.subscriptions.add(
      this.dependencyService.GetScheduledEventDependencyList({
        successorEventId: eventId, active: true, deleted: false, includeRelations: true
      }).subscribe(deps => this.predecessors = deps)
    );

    // Load successors (this event appears as predecessorEventId)
    this.subscriptions.add(
      this.dependencyService.GetScheduledEventDependencyList({
        predecessorEventId: eventId, active: true, deleted: false, includeRelations: true
      }).subscribe(deps => this.successors = deps)
    );

    // Load dependency types lookup
    this.subscriptions.add(
      this.dependencyTypeService.GetDependencyTypeList({ active: true, deleted: false }).subscribe(
        types => this.dependencyTypes = types
      )
    );

    // Load all events for picker dropdown
    this.subscriptions.add(
      this.scheduledEventService.GetScheduledEventsBasicListData({ active: true, deleted: false }).subscribe(
        events => this.allEventsList = events.filter(e => Number(e.id) !== eventId)
      )
    );
  }


  showAddDependency(direction: 'predecessor' | 'successor'): void {
    this.addingDependency = true;
    this.newDep = { eventId: 0, dependencyTypeId: this.dependencyTypes[0]?.id as number || 0, lagMinutes: 0, direction };
  }

  cancelAddDependency(): void {
    this.addingDependency = false;
  }

  confirmAddDependency(): void {
    if (!this.newDep.eventId || !this.newDep.dependencyTypeId) return;

    const currentEventId = Number(this.event?.id || 0);
    if (this.newDep.direction === 'predecessor') {
      this.pendingNewDeps.push({
        predecessorEventId: this.newDep.eventId,
        successorEventId: currentEventId,
        dependencyTypeId: this.newDep.dependencyTypeId,
        lagMinutes: this.newDep.lagMinutes
      });
    } else {
      this.pendingNewDeps.push({
        predecessorEventId: currentEventId,
        successorEventId: this.newDep.eventId,
        dependencyTypeId: this.newDep.dependencyTypeId,
        lagMinutes: this.newDep.lagMinutes
      });
    }

    this.addingDependency = false;
  }

  removeExistingDep(dep: ScheduledEventDependencyData, type: 'predecessor' | 'successor'): void {
    this.deletedDepIds.push(Number(dep.id));
    if (type === 'predecessor') {
      this.predecessors = this.predecessors.filter(d => Number(d.id) !== Number(dep.id));
    } else {
      this.successors = this.successors.filter(d => Number(d.id) !== Number(dep.id));
    }
  }

  removePendingDep(index: number): void {
    this.pendingNewDeps.splice(index, 1);
  }

  getDependencyTypeName(typeId: any): string {
    return this.dependencyTypes.find(t => Number(t.id) === Number(typeId))?.name || 'Unknown';
  }

  getDependencyTypeColor(typeId: any): string {
    return this.dependencyTypes.find(t => Number(t.id) === Number(typeId))?.color || '#6c757d';
  }

  getEventNameById(eventId: number): string {
    return this.allEventsList.find(e => Number(e.id) === eventId)?.name || `Event #${eventId}`;
  }

  private async handleDependenciesSave(eventId: number): Promise<void> {
    // Delete removed dependencies
    for (const deletedId of this.deletedDepIds) {
      try {
        await lastValueFrom(this.dependencyService.DeleteScheduledEventDependency(deletedId));
      } catch (err: any) {
        console.error(`Failed to delete dependency ${deletedId}`, err);
      }
    }

    // Create new dependencies
    for (const pending of this.pendingNewDeps) {
      const submitData = new ScheduledEventDependencySubmitData();
      submitData.id = 0 as any;
      submitData.predecessorEventId = pending.predecessorEventId === 0 ? eventId : pending.predecessorEventId;
      submitData.successorEventId = pending.successorEventId === 0 ? eventId : pending.successorEventId;
      submitData.dependencyTypeId = pending.dependencyTypeId;
      submitData.lagMinutes = pending.lagMinutes;
      submitData.versionNumber = 0 as any;
      submitData.active = true;
      submitData.deleted = false;

      try {
        await lastValueFrom(this.dependencyService.PostScheduledEventDependency(submitData));
      } catch (err: any) {
        console.error('Failed to create dependency', err);
      }
    }

    this.deletedDepIds = [];
    this.pendingNewDeps = [];
  }

  // -------------------------------------------------------------------------
  // Save As Template
  // -------------------------------------------------------------------------
  async saveAsTemplate(): Promise<void> {
    const templateName = await this.inputDialogService.promptText('Save as Template', {
      inputLabel: 'Template Name',
      inputPlaceholder: 'e.g., Standard Installation',
      confirmButtonText: 'Save Template',
      cancelButtonText: 'Cancel'
    });

    if (!templateName || !templateName.trim()) return;

    const formVal = this.eventForm.value;

    // Calculate duration from start/end
    let durationMinutes = 60;
    if (formVal.startDateTime && formVal.endDateTime) {
      const diffMs = new Date(formVal.endDateTime).getTime() - new Date(formVal.startDateTime).getTime();
      durationMinutes = Math.max(1, Math.round(diffMs / 60000));
    }

    const submitData = new ScheduledEventTemplateSubmitData();
    submitData.id = 0 as any;
    submitData.name = templateName.trim();
    submitData.description = formVal.description || null;
    submitData.defaultAllDay = formVal.isAllDay || false;
    submitData.defaultDurationMinutes = durationMinutes;
    submitData.priorityId = formVal.priorityId ? Number(formVal.priorityId) : null;
    submitData.defaultLocationPattern = formVal.location || null;
    submitData.versionNumber = 0 as any;
    submitData.active = true;
    submitData.deleted = false;

    try {
      const savedTemplate = await lastValueFrom(this.templateService.PostScheduledEventTemplate(submitData));
      this.alertService.showMessage('Template Saved', `"${templateName}" saved as template`, MessageSeverity.success);

      // Copy event qualification requirements to the new template
      const qualIdsToSave: number[] = [];
      // From existing event quals (non-deleted)
      for (const req of this.existingEventQualReqs) {
        if (!this.deletedQualReqIds.includes(Number(req.id))) {
          qualIdsToSave.push(Number(req.qualificationId));
        }
      }
      // From pending new event quals
      for (const qualId of this.pendingNewQualReqs) {
        if (!qualIdsToSave.includes(qualId)) {
          qualIdsToSave.push(qualId);
        }
      }
      // Save each as a template qual requirement
      for (const qualId of qualIdsToSave) {
        const tqSubmit: ScheduledEventTemplateQualificationRequirementSubmitData = {
          id: 0 as any,
          scheduledEventTemplateId: savedTemplate.id as any,
          qualificationId: qualId,
          isRequired: true,
          versionNumber: 0 as any,
          active: true,
          deleted: false
        };
        await lastValueFrom(
          this.templateQualReqService.PostScheduledEventTemplateQualificationRequirement(tqSubmit)
        );
      }

      // Refresh templates list
      this.templateService.GetScheduledEventTemplateList({ active: true }).subscribe(templates => {
        this.templates = templates;
      });
    } catch (err: any) {
      this.alertService.showMessage('Error', 'Failed to save template', MessageSeverity.error);
    }
  }

  // -------------------------------------------------------------------------
  // Event-Specific Qualification Requirements
  // -------------------------------------------------------------------------
  private loadExistingEventQualReqs(): void {
    if (!this.event?.id) return;
    this.eventQualReqService.GetScheduledEventQualificationRequirementList({
      scheduledEventId: this.event.id,
      active: true
    }).subscribe({
      next: (reqs) => {
        this.existingEventQualReqs = reqs;
      },
      error: () => {
        this.existingEventQualReqs = [];
      }
    });
  }

  addEventQualReq(): void {
    if (!this.selectedNewQualId) return;
    const qualId = Number(this.selectedNewQualId);
    // Avoid duplicates
    const alreadyExists = this.existingEventQualReqs.some(
      r => Number(r.qualificationId) === qualId && !this.deletedQualReqIds.includes(Number(r.id))
    );
    const alreadyPending = this.pendingNewQualReqs.includes(qualId);
    if (alreadyExists || alreadyPending) return;

    this.pendingNewQualReqs.push(qualId);
    this.selectedNewQualId = null;
  }

  removeExistingQualReq(reqId: number): void {
    this.deletedQualReqIds.push(reqId);
    this.existingEventQualReqs = this.existingEventQualReqs.filter(r => Number(r.id) !== reqId);
  }

  removePendingQualReq(index: number): void {
    this.pendingNewQualReqs.splice(index, 1);
  }

  getQualificationName(qualId: number): string {
    const qual = this.allQualifications.find(q => Number(q.id) === qualId);
    return qual?.name || `Qualification #${qualId}`;
  }

  private async handleQualificationReqsSave(eventId: number): Promise<void> {
    // Delete removed requirements
    for (const reqId of this.deletedQualReqIds) {
      await lastValueFrom(this.eventQualReqService.DeleteScheduledEventQualificationRequirement(reqId));
    }

    // Create new requirements
    for (const qualId of this.pendingNewQualReqs) {
      const submitData: ScheduledEventQualificationRequirementSubmitData = {
        id: 0 as any,
        scheduledEventId: eventId,
        qualificationId: qualId,
        versionNumber: 0 as any,
        active: true,
        deleted: false
      };
      await lastValueFrom(this.eventQualReqService.PostScheduledEventQualificationRequirement(submitData));
    }

    this.deletedQualReqIds = [];
    this.pendingNewQualReqs = [];
  }

  // -------------------------------------------------------------------------
  // Print Booking Summary
  // -------------------------------------------------------------------------

  /**
   * Opens a print-friendly window with the booking summary.
   * Includes event name, date/time, location, booking contact,
   * charges with deposit status, and overall event status.
   */
  printBookingSummary(): void {
    if (!this.event) return;

    const formVal = this.eventForm.value;
    const statusName = this.eventStatuses.find(
      s => Number(s.id) === Number(formVal.eventStatusId)
    )?.name || '—';

    const startDate = formVal.startDateTime
      ? new Date(formVal.startDateTime).toLocaleString()
      : '—';
    const endDate = formVal.endDateTime
      ? new Date(formVal.endDateTime).toLocaleString()
      : '—';

    // Build charges table rows
    let chargeRows = '';
    if (this.eventCharges.length > 0) {
      for (const charge of this.eventCharges) {
        const typeName = charge.chargeType?.name || '—';
        const depositLabel = charge.isDeposit
          ? (charge.depositRefundedDate
            ? ' <span style="color:green">(Refunded)</span>'
            : ' <span style="color:darkorange">(Held)</span>')
          : '';
        chargeRows += `<tr>
          <td>${typeName}${depositLabel}</td>
          <td>${charge.description || '—'}</td>
          <td style="text-align:right">$${(Number(charge.extendedAmount) || 0).toFixed(2)}</td>
          <td style="text-align:right">$${(Number(charge.taxAmount) || 0).toFixed(2)}</td>
          <td style="text-align:right"><strong>$${(Number(charge.totalAmount) || 0).toFixed(2)}</strong></td>
        </tr>`;
      }
    } else {
      chargeRows = '<tr><td colspan="5" style="text-align:center;color:#999">No charges recorded</td></tr>';
    }

    // Contact section
    const contactName = formVal.bookingContactName || '';
    const contactEmail = formVal.bookingContactEmail || '';
    const contactPhone = formVal.bookingContactPhone || '';
    const hasContact = contactName || contactEmail || contactPhone;
    const contactSection = hasContact ? `
      <div style="margin-bottom:18px">
        <h3 style="margin:0 0 8px;font-size:14px;color:#555;text-transform:uppercase;letter-spacing:1px">Booking Contact</h3>
        <table style="width:100%;border-collapse:collapse">
          ${contactName ? `<tr><td style="padding:3px 8px;color:#777;width:80px">Name</td><td style="padding:3px 8px">${contactName}</td></tr>` : ''}
          ${contactEmail ? `<tr><td style="padding:3px 8px;color:#777;width:80px">Email</td><td style="padding:3px 8px">${contactEmail}</td></tr>` : ''}
          ${contactPhone ? `<tr><td style="padding:3px 8px;color:#777;width:80px">Phone</td><td style="padding:3px 8px">${contactPhone}</td></tr>` : ''}
        </table>
      </div>
    ` : '';

    const html = `<!DOCTYPE html>
<html>
<head>
  <title>Booking Confirmation — ${formVal.name || 'Event'}</title>
  <style>
    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; margin: 40px; color: #333; }
    h1 { margin: 0 0 4px; font-size: 22px; }
    h2 { margin: 0 0 20px; font-size: 14px; color: #777; font-weight: normal; }
    .detail-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 8px 24px; margin-bottom: 18px; }
    .detail-label { font-size: 12px; color: #999; text-transform: uppercase; letter-spacing: 0.5px; }
    .detail-value { font-size: 14px; margin-bottom: 8px; }
    table.charges { width: 100%; border-collapse: collapse; font-size: 13px; }
    table.charges th { background: #f5f5f5; padding: 6px 8px; text-align: left; border-bottom: 2px solid #ddd; font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; color: #777; }
    table.charges td { padding: 6px 8px; border-bottom: 1px solid #eee; }
    .footer { margin-top: 30px; padding-top: 12px; border-top: 1px solid #ddd; font-size: 11px; color: #999; }
    @media print { body { margin: 20px; } }
  </style>
</head>
<body>
  <h1>${formVal.name || 'Booking Confirmation'}</h1>
  <h2>Status: ${statusName}</h2>

  <div class="detail-grid">
    <div>
      <div class="detail-label">Start</div>
      <div class="detail-value">${startDate}</div>
    </div>
    <div>
      <div class="detail-label">End</div>
      <div class="detail-value">${endDate}</div>
    </div>
    <div>
      <div class="detail-label">Location</div>
      <div class="detail-value">${formVal.location || '—'}</div>
    </div>
    <div>
      <div class="detail-label">All Day</div>
      <div class="detail-value">${formVal.isAllDay ? 'Yes' : 'No'}</div>
    </div>
  </div>

  ${contactSection}

  ${formVal.description ? `<div style="margin-bottom:18px"><div class="detail-label">Description</div><div class="detail-value">${formVal.description}</div></div>` : ''}
  ${formVal.notes ? `<div style="margin-bottom:18px"><div class="detail-label">Notes</div><div class="detail-value">${formVal.notes}</div></div>` : ''}

  <h3 style="margin:0 0 8px;font-size:14px;color:#555;text-transform:uppercase;letter-spacing:1px">Charges</h3>
  <table class="charges">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
        <th style="text-align:right">Amount</th>
        <th style="text-align:right">Tax</th>
        <th style="text-align:right">Total</th>
      </tr>
    </thead>
    <tbody>
      ${chargeRows}
    </tbody>
  </table>

  <div class="footer">
    Printed ${new Date().toLocaleString()}
  </div>

  <script>window.print();</script>
</body>
</html>`;

    const printWindow = window.open('', '_blank');
    if (printWindow) {
      printWindow.document.write(html);
      printWindow.document.close();
    }
  }
}


interface TimelineEntry {
  date: string;
  type: string;
  icon: string;
  description: string;
  amount: number;
  isPositive: boolean;
}

interface ConflictEvent {
  id: number;
  name: string;
  startDateTime: string;
  endDateTime: string;
  location: string;
}

interface ConflictResponse {
  hasConflict: boolean;
  conflictCount: number;
  conflicts: ConflictEvent[];
}
