//
// Quick Add Job Modal — creates Client → SchedulingTarget → ScheduledEvent in one flow.
//
// AI-Developed — This file was significantly developed with AI assistance.
//
import { Component, OnInit, Output, ViewChild, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Subject, forkJoin, take, switchMap, map, finalize } from 'rxjs';
import { ClientService, ClientSubmitData } from '../../../scheduler-data-services/client.service';
import { SchedulingTargetService, SchedulingTargetSubmitData } from '../../../scheduler-data-services/scheduling-target.service';
import { ScheduledEventService, ScheduledEventData, ScheduledEventSubmitData } from '../../../scheduler-data-services/scheduled-event.service';
import { ClientTypeService } from '../../../scheduler-data-services/client-type.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { StateProvinceService } from '../../../scheduler-data-services/state-province.service';
import { CountryService } from '../../../scheduler-data-services/country.service';
import { EventStatusService } from '../../../scheduler-data-services/event-status.service';
import { SchedulingTargetTypeService } from '../../../scheduler-data-services/scheduling-target-type.service';
import { TerminologyService } from '../../../services/terminology.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { CurrentUserService } from '../../../services/current-user.service';

@Component({
  selector: 'app-quick-add-job-modal',
  templateUrl: './quick-add-job-modal.component.html',
  styleUrls: ['./quick-add-job-modal.component.scss']
})
export class QuickAddJobModalComponent implements OnInit {
  @ViewChild('quickAddJobModal') quickAddJobModal!: TemplateRef<any>;
  @Output() jobCreated = new Subject<ScheduledEventData>();

  public jobForm!: FormGroup;
  public isSaving = false;
  public modalIsDisplayed = false;
  private modalRef?: NgbModalRef;

  // Dynamic defaults loaded from type tables
  private defaultClientTypeId: number = 0;
  private defaultCurrencyId: number = 0;
  private defaultTimeZoneId: number = 0;
  private defaultStateProvinceId: number = 0;
  private defaultCountryId: number = 0;
  private defaultEventStatusId: number = 0;
  private defaultTargetTypeId: number = 0;
  public defaultsLoaded = false;

  constructor(
    private formBuilder: FormBuilder,
    private modalService: NgbModal,
    private clientService: ClientService,
    private targetService: SchedulingTargetService,
    private eventService: ScheduledEventService,
    private clientTypeService: ClientTypeService,
    private currencyService: CurrencyService,
    private timeZoneService: TimeZoneService,
    private stateProvinceService: StateProvinceService,
    private countryService: CountryService,
    private eventStatusService: EventStatusService,
    private targetTypeService: SchedulingTargetTypeService,
    public terminology: TerminologyService,
    private alertService: AlertService,
    private authService: AuthService,
    private currentUserService: CurrentUserService
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadDefaults();
  }

  private buildForm(): void {
    this.jobForm = this.formBuilder.group({
      customerName: ['', Validators.required],
      email: [''],
      phone: [''],
      addressLine1: ['', Validators.required],
      city: ['', Validators.required],
      jobName: ['', Validators.required],
      date: ['', Validators.required],
      time: ['', Validators.required],
      durationHours: [1, [Validators.required, Validators.min(0.5)]]
    });
  }

  /** Load first active record from each type table to use as defaults for quick-add. */
  private loadDefaults(): void {
    forkJoin({
      clientTypes: this.clientTypeService.GetClientTypeList().pipe(take(1)),
      currencies: this.currencyService.GetCurrencyList().pipe(take(1)),
      timeZones: this.timeZoneService.GetTimeZoneList().pipe(take(1)),
      stateProvinces: this.stateProvinceService.GetStateProvinceList().pipe(take(1)),
      countries: this.countryService.GetCountryList().pipe(take(1)),
      eventStatuses: this.eventStatusService.GetEventStatusList().pipe(take(1)),
      targetTypes: this.targetTypeService.GetSchedulingTargetTypeList().pipe(take(1))
    }).subscribe({
      next: (data) => {
        this.defaultClientTypeId = data.clientTypes?.[0]?.id ? Number(data.clientTypes[0].id) : 0;
        this.defaultCurrencyId = data.currencies?.[0]?.id ? Number(data.currencies[0].id) : 0;
        this.defaultTimeZoneId = this.currentUserService.defaultTimeZoneId
            || (data.timeZones?.[0]?.id ? Number(data.timeZones[0].id) : 0);
        this.defaultStateProvinceId = data.stateProvinces?.[0]?.id ? Number(data.stateProvinces[0].id) : 0;
        this.defaultCountryId = data.countries?.[0]?.id ? Number(data.countries[0].id) : 0;
        this.defaultEventStatusId = data.eventStatuses?.[0]?.id ? Number(data.eventStatuses[0].id) : 0;
        this.defaultTargetTypeId = data.targetTypes?.[0]?.id ? Number(data.targetTypes[0].id) : 0;
        this.defaultsLoaded = true;
      },
      error: () => {
        // If defaults fail to load, alert but do not block — modal will validate on submit
        this.alertService.showMessage(
          'Could not load configuration defaults',
          'Please try again or use the full form.',
          MessageSeverity.warn
        );
      }
    });
  }

  public openModal(): void {
    // Permission check — matches established pattern
    if (!this.authService.isSchedulerReaderWriter) {
      this.alertService.showMessage(
        'Permission Denied',
        'You do not have permission to create records.',
        MessageSeverity.info
      );
      return;
    }

    this.jobForm.reset({
      customerName: '',
      email: '',
      phone: '',
      addressLine1: '',
      city: '',
      jobName: '',
      date: '',
      time: '',
      durationHours: 1
    });
    this.modalIsDisplayed = true;
    this.modalRef = this.modalService.open(this.quickAddJobModal, {
      size: 'lg',
      backdrop: 'static',
      keyboard: true
    });
  }

  public closeModal(): void {
    this.modalIsDisplayed = false;
    this.modalRef?.dismiss();
  }

  public onSubmit(): void {
    if (this.jobForm.invalid) {
      this.jobForm.markAllAsTouched();
      return;
    }

    if (!this.defaultsLoaded) {
      this.alertService.showMessage(
        'Configuration Not Ready',
        'System defaults have not finished loading. Please try again.',
        MessageSeverity.warn
      );
      return;
    }

    this.isSaving = true;
    const vals = this.jobForm.value;

    // Step 1: Create Client
    const newClient: ClientSubmitData = {
      id: 0,
      name: vals.customerName?.trim(),
      email: vals.email?.trim() || null,
      phone: vals.phone?.trim() || null,
      addressLine1: vals.addressLine1?.trim(),
      city: vals.city?.trim(),
      clientTypeId: this.defaultClientTypeId,
      currencyId: this.defaultCurrencyId,
      timeZoneId: this.defaultTimeZoneId,
      stateProvinceId: this.defaultStateProvinceId,
      countryId: this.defaultCountryId,
      versionNumber: 0,
      active: true,
      deleted: false
    } as ClientSubmitData;

    this.clientService.PostClient(newClient).pipe(
      // Step 2: Create SchedulingTarget using Client ID
      switchMap(createdClient => {
        if (!createdClient?.id) {
          throw new Error('Failed to create customer record.');
        }

        const newTarget: SchedulingTargetSubmitData = {
          id: 0,
          name: vals.addressLine1?.trim(),
          clientId: Number(createdClient.id),
          schedulingTargetTypeId: this.defaultTargetTypeId,
          timeZoneId: this.defaultTimeZoneId,
          versionNumber: 0,
          active: true,
          deleted: false
        } as SchedulingTargetSubmitData;

        return this.targetService.PostSchedulingTarget(newTarget).pipe(
          map(target => ({ client: createdClient, target }))
        );
      }),
      // Step 3: Create ScheduledEvent using Target ID
      switchMap(({ client, target }) => {
        if (!target?.id) {
          throw new Error('Failed to create location record.');
        }

        // Build ISO date string directly to avoid local timezone conversion issues.
        // Format: "YYYY-MM-DDTHH:mm:00" — server handles timezone via timeZoneId.
        const startIso = `${vals.date}T${vals.time}:00`;
        const startDate = new Date(startIso);
        const endDate = new Date(startDate.getTime());
        endDate.setHours(endDate.getHours() + Math.floor(vals.durationHours));
        endDate.setMinutes(endDate.getMinutes() + ((vals.durationHours % 1) * 60));

        const newEvent: ScheduledEventSubmitData = {
          id: 0,
          name: vals.jobName?.trim(),
          schedulingTargetId: Number(target.id),
          startDateTime: startIso,
          endDateTime: endDate.toISOString().replace('Z', ''),
          eventStatusId: this.defaultEventStatusId,
          isOpenForVolunteers: false,
          versionNumber: 0,
          active: true,
          deleted: false
        } as ScheduledEventSubmitData;

        return this.eventService.PostScheduledEvent(newEvent);
      }),
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (createdEvent) => {
        if (!createdEvent) {
          this.alertService.showMessage('Error', 'Failed to create job record.', MessageSeverity.error);
          return;
        }

        this.clientService.ClearAllCaches();
        this.targetService.ClearAllCaches();
        this.eventService.ClearAllCaches();

        this.jobCreated.next(createdEvent);
        this.alertService.showMessage(
          `${this.terminology.getTerm('ScheduledEvent')} created successfully`,
          '',
          MessageSeverity.success
        );
        this.closeModal();
      },
      error: (err) => {
        const errorMessage = this.extractErrorMessage(err,
          `An error occurred while creating the ${this.terminology.getTerm('ScheduledEvent')}.`);
        this.alertService.showMessage(
          `Could not create ${this.terminology.getTerm('ScheduledEvent')}`,
          errorMessage,
          MessageSeverity.error
        );
      }
    });
  }

  /** Structured error extraction matching established app patterns. */
  private extractErrorMessage(err: any, fallback: string): string {
    if (err instanceof Error) {
      return err.message || fallback;
    }
    if (err?.status && err?.error) {
      if (err.status === 403) {
        return err.error?.message || 'You do not have permission to perform this action.';
      }
      return err.error?.message || err.error?.error_description || err.error?.detail || fallback;
    }
    return fallback;
  }
}
