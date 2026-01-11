import { Component, ViewChild, Output, Input, TemplateRef, SimpleChanges } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CalendarService, CalendarData, CalendarSubmitData } from '../../../scheduler-data-services/calendar.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-calendar-custom-add-edit',
  templateUrl: './calendar-custom-add-edit.component.html',
  styleUrls: ['./calendar-custom-add-edit.component.scss'],
  animations: [
    trigger('collapse', [
      state('false', style({ height: '0', overflow: 'hidden', opacity: 0 })),
      state('true', style({ height: '*', opacity: 1 })),
      transition('false <=> true', animate('300ms ease-in-out'))
    ])
  ]
})
export class CalendarCustomAddEditComponent {
  @ViewChild('calendarModal') calendarModal!: TemplateRef<any>;
  @Output() calendarChanged = new Subject<CalendarData[]>();
  @Input() calendarSubmitData: CalendarSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  @Input() officeId: number | null = null;      // to allow the office ID to be provided.  Will remove the office ID control from the form.

  public calendarData: CalendarData | null = null;

  calendarForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        officeId: [this.officeId],
        isDefault: [false],
        iconId: [null],
        color: [''],
        active: [true],
        deleted: [false],
        versionNumber: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isAppearancePanelOpen: boolean = false;

  public isSaving: boolean = false;

  calendars$ = this.calendarService.GetCalendarList();
  offices$ = this.officeService.GetOfficeList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private calendarService: CalendarService,
    private officeService: OfficeService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['officeId']) {

      if (this.calendarData != null) {
        this.calendarData.officeId = this.officeId as number;
      }

      this.calendarForm.patchValue({
        officeId: this.officeId
      });
    }
  }

  openModal(calendarData?: CalendarData) {

    if (calendarData != null) {

      if (!this.calendarService.userIsSchedulerCalendarReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Calendars`,
          '',
          MessageSeverity.info
        );
        return;
      }

      this.calendarData = calendarData;
      this.calendarSubmitData = this.calendarService.ConvertToCalendarSubmitData(calendarData);
      this.isEditMode = true;
      this.objectGuid = calendarData.objectGuid;

      this.buildFormValues(calendarData);

    } else {

      if (!this.calendarService.userIsSchedulerCalendarWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Calendars`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.calendarModal, {
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

    if (this.calendarService.userIsSchedulerCalendarWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Calendars`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.calendarForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.calendarForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.calendarForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const calendarSubmitData: CalendarSubmitData = {
        id: this.calendarSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        isDefault: formValue.isDefault == true ? true : formValue.isDefault == false ? false : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
        versionNumber: this.calendarSubmitData?.versionNumber ?? 0,
   };

      if (this.isEditMode) {
        this.updateCalendar(calendarSubmitData);
      } else {
        this.addCalendar(calendarSubmitData);
      }
  }

  private addCalendar(calendarData: CalendarSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    calendarData.versionNumber = 0;
    calendarData.active = true;
    calendarData.deleted = false;
    this.calendarService.PostCalendar(calendarData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newCalendar) => {

        this.calendarService.ClearAllCaches();

        this.calendarChanged.next([newCalendar]);

        this.alertService.showMessage("Calendar added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/calendar', newCalendar.id]);
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
                                   'You do not have permission to save this Calendar.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Calendar.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Calendar could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateCalendar(calendarData: CalendarSubmitData) {
    this.calendarService.PutCalendar(calendarData.id, calendarData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedCalendar) => {

        this.calendarService.ClearAllCaches();

        this.calendarChanged.next([updatedCalendar]);

        this.alertService.showMessage("Calendar updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Calendar.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Calendar.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Calendar could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(calendarData: CalendarData | null) {

    if (calendarData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.calendarForm.reset({
        name: '',
        description: '',
        officeId: this.officeId,
        isDefault: false,
        iconId: null,
        color: '',
        active: true,
        deleted: false,
        versionNumber: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.calendarForm.reset({
        name: calendarData.name ?? '',
        description: calendarData.description ?? '',
        officeId: this.officeId || calendarData.officeId,
        isDefault: calendarData.isDefault ?? false,
        iconId: calendarData.iconId,
        color: calendarData.color ?? '',
        active: calendarData.active ?? true,
        deleted: calendarData.deleted ?? false,
        versionNumber: calendarData.versionNumber?.toString() ?? '',
      }, { emitEvent: false});
    }

    this.calendarForm.markAsPristine();
    this.calendarForm.markAsUntouched();
  }

  public userIsSchedulerCalendarReader(): boolean {
    return this.calendarService.userIsSchedulerCalendarReader();
  }

  public userIsSchedulerCalendarWriter(): boolean {
    return this.calendarService.userIsSchedulerCalendarWriter();
  }
}
