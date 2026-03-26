import { Component, ViewChild, Output, Input, TemplateRef, SimpleChanges } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SchedulingTargetService, SchedulingTargetData, SchedulingTargetSubmitData } from '../../../scheduler-data-services/scheduling-target.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { SchedulingTargetTypeService } from '../../../scheduler-data-services/scheduling-target-type.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { CalendarService } from '../../../scheduler-data-services/calendar.service';
import { SchedulerHelperService } from '../../../services/scheduler-helper.service';
import { AuthService } from '../../../services/auth.service';
import { CurrentUserService } from '../../../services/current-user.service';

@Component({
  selector: 'app-scheduling-target-custom-add-edit',
  templateUrl: './scheduling-target-custom-add-edit.component.html',
  styleUrls: ['./scheduling-target-custom-add-edit.component.scss'],
  animations: [
    trigger('collapse', [
      state('false', style({ height: '0', overflow: 'hidden', opacity: 0 })),
      state('true', style({ height: '*', opacity: 1 })),
      transition('false <=> true', animate('300ms ease-in-out'))
    ])
  ]
})
export class SchedulingTargetCustomAddEditComponent {
  @ViewChild('schedulingTargetModal') schedulingTargetModal!: TemplateRef<any>;
  @Output() schedulingTargetChanged = new Subject<SchedulingTargetData[]>();
  @Input() schedulingTargetSubmitData: SchedulingTargetSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;
  @Input() officeId: number | null = null;
  @Input() clientId: number | null = null;


  public schedulingTargetData: SchedulingTargetData | null = null;

  public currentAvatarUrl: string | null = null;
  public isAvatarPanelOpen = false;
  public isDragOver = false;

  public attributesParsed: any = {};

  onDynamicAttributeChange(data: any) {
    this.attributesParsed = data;
    this.schedulingTargetForm.markAsDirty();
  }

  schedulingTargetForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    officeId: [this.officeId ?? this.currentUserService.defaultOfficeId],
    clientId: [this.clientId ?? null, Validators.required],
    schedulingTargetTypeId: [null, Validators.required],
    timeZoneId: [this.currentUserService.defaultTimeZoneId, Validators.required],
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

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  // To get the count of offices to allow the offices field to be invisible if there are no offices
  public officeCount$ = this.schedulerHelperService.ActiveOfficeCount$;

  public offices$ = this.officeService.GetOfficeList();
  public clients$ = this.clientService.GetClientList();
  public schedulingTargetTypes$ = this.schedulingTargetTypeService.GetSchedulingTargetTypeList();
  public timeZones$ = this.timeZoneService.GetTimeZoneList();
  public calendars$ = this.calendarService.GetCalendarList();

  constructor(
    private modalService: NgbModal,
    private schedulingTargetService: SchedulingTargetService,
    private officeService: OfficeService,
    private clientService: ClientService,
    private schedulingTargetTypeService: SchedulingTargetTypeService,
    private timeZoneService: TimeZoneService,
    private calendarService: CalendarService,
    private authService: AuthService,
    private alertService: AlertService,
    private schedulerHelperService: SchedulerHelperService,
    private currentUserService: CurrentUserService,
    private router: Router,
    private fb: FormBuilder) {
  }

  ngOnChanges(changes: SimpleChanges): void {

    if (changes['officeId']) {
      if (this.schedulingTargetData != null) {
        this.schedulingTargetData.officeId = this.officeId as number;
      }
      this.schedulingTargetForm.patchValue({
        officeId: this.officeId
      });
    }

    if (changes['clientId']) {
      if (this.schedulingTargetData != null) {
        this.schedulingTargetData.clientId = this.clientId as number;
      }
      this.schedulingTargetForm.patchValue({
        clientId: this.clientId
      });
    }
  }

  openModal(schedulingTargetData?: SchedulingTargetData) {

    if (schedulingTargetData != null) {

      if (!this.schedulingTargetService.userIsSchedulerSchedulingTargetReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Scheduling Targets`,
          '',
          MessageSeverity.info
        );
        return;
      }

      this.schedulingTargetData = schedulingTargetData;

      this.schedulingTargetSubmitData = this.schedulingTargetService.ConvertToSchedulingTargetSubmitData(schedulingTargetData);
      this.isEditMode = true;
      this.objectGuid = schedulingTargetData.objectGuid;

      this.buildFormValues(schedulingTargetData);

    } else {

      if (!this.schedulingTargetService.userIsSchedulerSchedulingTargetWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Scheduling Targets`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.schedulingTargetData = null;

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.schedulingTargetModal, {
      size: 'xl',
      scrollable: true,
      backdrop: 'static',
      keyboard: true,
      windowClass: 'custom-modal'
    });
    this.modalIsDisplayed = true;
  }


  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;

    const files = event.dataTransfer?.files;
    if (!files || files.length === 0) return;

    const file = files[0];
    if (!file.type.startsWith('image/')) {
      this.alertService.showMessage('Invalid file type', 'Please drop an image file', MessageSeverity.warn);
      return;
    }

    const fakeEvent = { target: { files: [file] } } as any;
    this.onAvatarSelected(fakeEvent);
  }


  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];

    // Enforce 2MB limit
    if (file.size > 2 * 1024 * 1024) {
      this.alertService.showMessage(
        'Image too large',
        'Please select an image under 2MB',
        MessageSeverity.warn
      );
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      const result = e.target?.result as string;

      if (!result) return;

      // Extract only the base64 part (remove data:image/png;base64, prefix)
      const base64Data = result.split(',')[1];

      if (!base64Data) {
        this.alertService.showMessage('Invalid image data', '', MessageSeverity.error);
        return;
      }

      this.currentAvatarUrl = result; // Full data URL for preview (includes prefix)

      // Populate form fields
      this.schedulingTargetForm.patchValue({
        avatarFileName: file.name,
        avatarSize: file.size,
        avatarData: base64Data,
        avatarMimeType: file.type
      });

      this.schedulingTargetForm.markAsDirty();
    };

    reader.onerror = () => {
      this.alertService.showMessage('Failed to read file', '', MessageSeverity.error);
    };

    reader.readAsDataURL(file);
  }

  clearAvatar(): void {
    this.currentAvatarUrl = null;
    this.schedulingTargetForm.patchValue({
      avatarFileName: null,
      avatarSize: null,
      avatarData: null,
      avatarMimeType: null
    });
    this.schedulingTargetForm.markAsDirty();
  }

  closeModal() {
    if (this.modalRef) {
      this.modalRef.dismiss('cancel');
    }
    this.isAvatarPanelOpen = false;
    this.modalIsDisplayed = false;
  }


  submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.schedulingTargetService.userIsSchedulerSchedulingTargetWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Scheduling Targets`,
        '',
        MessageSeverity.info
      );
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
      id: this.schedulingTargetSubmitData?.id || 0,
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
      versionNumber: this.schedulingTargetSubmitData?.versionNumber ?? 0,
      active: !!formValue.active,
      deleted: !!formValue.deleted,
    };

    if (this.isEditMode) {
      this.updateSchedulingTarget(schedulingTargetSubmitData);
    } else {
      this.addSchedulingTarget(schedulingTargetSubmitData);
    }
  }

  private addSchedulingTarget(schedulingTargetData: SchedulingTargetSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    schedulingTargetData.versionNumber = 0;
    schedulingTargetData.active = true;
    schedulingTargetData.deleted = false;
    this.schedulingTargetService.PostSchedulingTarget(schedulingTargetData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSchedulingTarget) => {

        this.schedulingTargetService.ClearAllCaches();

        this.schedulingTargetChanged.next([newSchedulingTarget]);

        this.alertService.showMessage("Scheduling Target added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/schedulingtargets', newSchedulingTarget.id]);
        }
      },
      error: (err) => {
        let errorMessage: string;

        if (err instanceof Error) {
          errorMessage = err.message || 'An unexpected error occurred.';
        }
        else if (err.status && err.error) {
          if (err.status === 403) {
            errorMessage = err.error?.message ||
              'You do not have permission to save this Scheduling Target.';
          }
          else {
            errorMessage = err.error?.message ||
              err.error?.error_description ||
              err.error?.detail ||
              'An error occurred while saving the Scheduling Target.';
          }
        }
        else {
          errorMessage = 'An unexpected error occurred.';
        }

        this.alertService.showMessage('Scheduling Target could not be saved',
          errorMessage,
          MessageSeverity.error);
      }
    });
  }


  private updateSchedulingTarget(schedulingTargetData: SchedulingTargetSubmitData) {
    this.schedulingTargetService.PutSchedulingTarget(schedulingTargetData.id, schedulingTargetData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSchedulingTarget) => {

        this.schedulingTargetService.ClearAllCaches();

        this.schedulingTargetChanged.next([updatedSchedulingTarget]);

        this.alertService.showMessage("Scheduling Target updated successfully", '', MessageSeverity.success);

        this.closeModal();
      },
      error: (err) => {
        let errorMessage: string;

        if (err instanceof Error) {
          errorMessage = err.message || 'An unexpected error occurred.';
        }
        else if (err.status && err.error) {
          if (err.status === 403) {
            errorMessage = err.error?.message ||
              'You do not have permission to save this Scheduling Target.';
          }
          else {
            errorMessage = err.error?.message ||
              err.error?.error_description ||
              err.error?.detail ||
              'An error occurred while saving the Scheduling Target.';
          }
        }
        else {
          errorMessage = 'An unexpected error occurred.';
        }

        this.alertService.showMessage('Scheduling Target could not be saved',
          errorMessage,
          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(schedulingTargetData: SchedulingTargetData | null) {

    if (schedulingTargetData == null) {

      this.attributesParsed = {};
      this.currentAvatarUrl = null;
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.schedulingTargetForm.reset({
        name: '',
        description: '',
        officeId: this.officeId ?? this.currentUserService.defaultOfficeId,
        clientId: this.clientId ?? null,
        schedulingTargetTypeId: null,
        timeZoneId: this.currentUserService.defaultTimeZoneId,
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
      }, { emitEvent: false });

    }
    else {

      try {
        this.attributesParsed = schedulingTargetData.attributes ? JSON.parse(schedulingTargetData.attributes) : {};
      } catch (e) {
        this.attributesParsed = {};
      }

      // Reconstruct full data URL for preview if we have base64 data
      if (schedulingTargetData.avatarData && schedulingTargetData.avatarMimeType) {
        this.currentAvatarUrl = `data:${schedulingTargetData.avatarMimeType};base64,${schedulingTargetData.avatarData}`;
      } else {
        this.currentAvatarUrl = null;
      }

      //
      // Reset the form with properly formatted values
      //
      this.schedulingTargetForm.reset({
        name: schedulingTargetData.name ?? '',
        description: schedulingTargetData.description ?? '',
        officeId: this.officeId ?? schedulingTargetData.officeId ?? this.currentUserService.defaultOfficeId,
        clientId: schedulingTargetData.clientId,
        schedulingTargetTypeId: schedulingTargetData.schedulingTargetTypeId,
        timeZoneId: schedulingTargetData.timeZoneId ?? this.currentUserService.defaultTimeZoneId,
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
      }, { emitEvent: false });
    }

    this.schedulingTargetForm.markAsPristine();
    this.schedulingTargetForm.markAsUntouched();
  }

  public userIsSchedulerSchedulingTargetReader(): boolean {
    return this.schedulingTargetService.userIsSchedulerSchedulingTargetReader();
  }

  public userIsSchedulerSchedulingTargetWriter(): boolean {
    return this.schedulingTargetService.userIsSchedulerSchedulingTargetWriter();
  }

  public userIsSchedulerAdministrator(): boolean {
    return this.authService.isSchedulerAdministrator;
  }

  public userIsFoundationAdministrator(): boolean {
    return this.authService.isFoundationAdmin;
  }
}
