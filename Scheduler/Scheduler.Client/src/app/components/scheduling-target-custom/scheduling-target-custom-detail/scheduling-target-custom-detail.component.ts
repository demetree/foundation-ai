import { Component, OnInit, Input, OnDestroy } from '@angular/core';
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
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';

interface SchedulingTargetFormValues {
    name: string;
    description: string | null;
    officeId: number | bigint | null;
    clientId: number | bigint;
    schedulingTargetTypeId: number | bigint;
    timeZoneId: number | bigint;
    calendarId: number | bigint | null;
    notes: string | null;
    externalId: string | null;
    color: string | null;
    attributes: string | null;
    avatarFileName: string | null;
    avatarSize: string | null;
    avatarData: string | null;
    avatarMimeType: string | null;
    versionNumber: string;
    active: boolean;
    deleted: boolean;
}

@Component({
    selector: 'app-scheduling-target-custom-detail',
    templateUrl: './scheduling-target-custom-detail.component.html',
    styleUrls: ['./scheduling-target-custom-detail.component.scss']
})
export class SchedulingTargetCustomDetailComponent implements OnInit, OnDestroy, CanComponentDeactivate {

    @Input() preSeededData: Partial<SchedulingTargetFormValues> | null = null;
    @Input() hiddenFields: string[] = [];

    public activeTab: string = 'overview';

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

    public isLoadingSubject = new BehaviorSubject<boolean>(true);
    public isLoading$ = this.isLoadingSubject.asObservable();

    public isSaving = false;
    public isEditMode = true;

    public attributesParsed: any = {};

    onDynamicAttributeChange(data: any) {
        this.attributesParsed = data;
        this.schedulingTargetForm.markAsDirty();
    }

    public offices$ = this.officeService.GetOfficeList();
    public clients$ = this.clientService.GetClientList();
    public schedulingTargetTypes$ = this.schedulingTargetTypeService.GetSchedulingTargetTypeList();
    public timeZones$ = this.timeZoneService.GetTimeZoneList();
    public calendars$ = this.calendarService.GetCalendarList();

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
        private authService: AuthService,
        private route: ActivatedRoute,
        private router: Router,
        private fb: FormBuilder,
        private alertService: AlertService,
        private navigationService: NavigationService
    ) { }

    ngOnInit(): void {
        this.schedulingTargetId = this.route.snapshot.paramMap.get('schedulingTargetId');

        if (this.schedulingTargetId === 'new' || this.schedulingTargetId == null) {
            this.isEditMode = false;
            this.schedulingTargetData = null;
            this.buildFormValues(null);

            if (this.preSeededData !== null && this.preSeededData !== undefined) {
                this.schedulingTargetForm.patchValue(this.preSeededData);
            }

            this.hiddenFields.forEach(fieldName => {
                const control = this.schedulingTargetForm.get(fieldName);
                if (control) {
                    control.clearValidators();
                    control.updateValueAndValidity();
                }
            });

            this.isLoadingSubject.next(false);
            document.title = 'Add New Scheduling Target';
        } else {
            this.isEditMode = true;
            document.title = 'Edit Scheduling Target';
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

    public loadData(forceReload: boolean | null = null): void {
        this.isLoadingSubject.next(true);

        if (!this.schedulingTargetService.userIsSchedulerSchedulingTargetReader()) {
            this.alertService.showMessage('Access Denied', 'Insufficient Permissions', MessageSeverity.warn);
            this.isLoadingSubject.next(false);
            return;
        }

        if (!this.schedulingTargetId) {
            this.isLoadingSubject.next(false);
            return;
        }

        const id = Number(this.schedulingTargetId);
        if (isNaN(id) || id <= 0) {
            this.alertService.showMessage('Invalid ID', 'Error', MessageSeverity.error);
            this.isLoadingSubject.next(false);
            return;
        }

        if (forceReload) {
            this.schedulingTargetService.ClearRecordCache(id, true);
        }

        this.schedulingTargetService.GetSchedulingTarget(id, true)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (data) => {
                    if (data) {
                        this.schedulingTargetData = data;
                        this.buildFormValues(data);
                        if (forceReload) this.alertService.showMessage('Scheduling Target Reloaded', '', MessageSeverity.success);
                    } else {
                        this.alertService.showMessage('Scheduling Target not found', 'Not Found', MessageSeverity.warn);
                    }
                    this.isLoadingSubject.next(false);
                },
                error: (err) => {
                    this.alertService.showMessage('Failed to load Scheduling Target', '', MessageSeverity.error);
                    this.isLoadingSubject.next(false);
                }
            });
    }

    private buildFormValues(data: SchedulingTargetData | null) {
        if (!data) {
            this.attributesParsed = {};
            this.schedulingTargetForm.reset({
                name: '', description: '', officeId: null, clientId: null, schedulingTargetTypeId: null,
                timeZoneId: null, calendarId: null, notes: '', externalId: '', color: '', attributes: '',
                avatarFileName: '', avatarSize: '', avatarData: '', avatarMimeType: '', versionNumber: '',
                active: true, deleted: false
            }, { emitEvent: false });
        } else {
            try {
                this.attributesParsed = data.attributes ? JSON.parse(data.attributes) : {};
            } catch (e) {
                this.attributesParsed = {};
            }
            this.schedulingTargetForm.reset({
                name: data.name ?? '',
                description: data.description ?? '',
                officeId: data.officeId,
                clientId: data.clientId,
                schedulingTargetTypeId: data.schedulingTargetTypeId,
                timeZoneId: data.timeZoneId,
                calendarId: data.calendarId,
                notes: data.notes ?? '',
                externalId: data.externalId ?? '',
                color: data.color ?? '',
                attributes: data.attributes ?? '',
                avatarFileName: data.avatarFileName ?? '',
                avatarSize: data.avatarSize?.toString() ?? '',
                avatarData: data.avatarData ?? '',
                avatarMimeType: data.avatarMimeType ?? '',
                versionNumber: data.versionNumber?.toString() ?? '',
                active: data.active ?? true,
                deleted: data.deleted ?? false
            }, { emitEvent: false });
        }
        this.schedulingTargetForm.markAsPristine();
        this.schedulingTargetForm.markAsUntouched();
    }

    public submitForm() {
        if (this.isSaving) return;
        if (!this.schedulingTargetForm.valid) {
            this.schedulingTargetForm.markAllAsTouched();
            return;
        }

        this.isSaving = true;
        const formValue = this.schedulingTargetForm.getRawValue();

        const submitData: SchedulingTargetSubmitData = {
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
            attributes: Object.keys(this.attributesParsed).length > 0 ? JSON.stringify(this.attributesParsed) : null,
            avatarFileName: formValue.avatarFileName?.trim() || null,
            avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
            avatarData: formValue.avatarData?.trim() || null,
            avatarMimeType: formValue.avatarMimeType?.trim() || null,
            versionNumber: this.schedulingTargetData?.versionNumber ?? 0,
            active: !!formValue.active,
            deleted: !!formValue.deleted,
        };

        const saveObs = this.isEditMode
            ? this.schedulingTargetService.PutSchedulingTarget(submitData.id, submitData)
            : this.schedulingTargetService.PostSchedulingTarget(submitData);

        saveObs.pipe(finalize(() => this.isSaving = false)).subscribe({
            next: (data) => {
                this.schedulingTargetService.ClearAllCaches();
                if (!this.isEditMode) {
                    this.schedulingTargetForm.markAsPristine();
                    this.router.navigate(['/schedulingtargets', data.id]);
                    this.alertService.showMessage('Scheduling Target created', '', MessageSeverity.success);
                } else {
                    this.schedulingTargetData = data;
                    this.buildFormValues(data);
                    this.alertService.showMessage('Scheduling Target saved', '', MessageSeverity.success);
                }
            },
            error: (err) => {
                this.alertService.showMessage('Save failed', err.message, MessageSeverity.error);
            }
        });
    }

    public goBack(): void {
        this.navigationService.goBack();
    }

    public canGoBack(): boolean {
        return this.navigationService.canGoBack();
    }

    public isFieldHidden(fieldName: string): boolean {
        return this.hiddenFields && this.hiddenFields.includes(fieldName);
    }

    public userIsSchedulerSchedulingTargetReader(): boolean {
        return this.schedulingTargetService.userIsSchedulerSchedulingTargetReader();
    }

    public userIsSchedulerSchedulingTargetWriter(): boolean {
        return this.schedulingTargetService.userIsSchedulerSchedulingTargetWriter();
    }
}
