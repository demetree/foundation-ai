/*
   GENERATED FORM FOR THE INCIDENTTIMELINEEVENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IncidentTimelineEvent table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to incident-timeline-event-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IncidentTimelineEventService, IncidentTimelineEventData, IncidentTimelineEventSubmitData } from '../../../alerting-data-services/incident-timeline-event.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { IncidentService } from '../../../alerting-data-services/incident.service';
import { IncidentEventTypeService } from '../../../alerting-data-services/incident-event-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface IncidentTimelineEventFormValues {
  incidentId: number | bigint,       // For FK link number
  incidentEventTypeId: number | bigint,       // For FK link number
  timestamp: string,
  actorObjectGuid: string | null,
  detailsJson: string | null,
  notes: string | null,
  source: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-incident-timeline-event-add-edit',
  templateUrl: './incident-timeline-event-add-edit.component.html',
  styleUrls: ['./incident-timeline-event-add-edit.component.scss']
})
export class IncidentTimelineEventAddEditComponent {
  @ViewChild('incidentTimelineEventModal') incidentTimelineEventModal!: TemplateRef<any>;
  @Output() incidentTimelineEventChanged = new Subject<IncidentTimelineEventData[]>();
  @Input() incidentTimelineEventSubmitData: IncidentTimelineEventSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IncidentTimelineEventFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public incidentTimelineEventForm: FormGroup = this.fb.group({
        incidentId: [null, Validators.required],
        incidentEventTypeId: [null, Validators.required],
        timestamp: ['', Validators.required],
        actorObjectGuid: [''],
        detailsJson: [''],
        notes: [''],
        source: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  incidentTimelineEvents$ = this.incidentTimelineEventService.GetIncidentTimelineEventList();
  incidents$ = this.incidentService.GetIncidentList();
  incidentEventTypes$ = this.incidentEventTypeService.GetIncidentEventTypeList();

  constructor(
    private modalService: NgbModal,
    private incidentTimelineEventService: IncidentTimelineEventService,
    private incidentService: IncidentService,
    private incidentEventTypeService: IncidentEventTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(incidentTimelineEventData?: IncidentTimelineEventData) {

    if (incidentTimelineEventData != null) {

      if (!this.incidentTimelineEventService.userIsAlertingIncidentTimelineEventReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Incident Timeline Events`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.incidentTimelineEventSubmitData = this.incidentTimelineEventService.ConvertToIncidentTimelineEventSubmitData(incidentTimelineEventData);
      this.isEditMode = true;
      this.objectGuid = incidentTimelineEventData.objectGuid;

      this.buildFormValues(incidentTimelineEventData);

    } else {

      if (!this.incidentTimelineEventService.userIsAlertingIncidentTimelineEventWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Incident Timeline Events`,
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
        this.incidentTimelineEventForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.incidentTimelineEventForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.incidentTimelineEventModal, {
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

    if (this.incidentTimelineEventService.userIsAlertingIncidentTimelineEventWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Incident Timeline Events`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.incidentTimelineEventForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.incidentTimelineEventForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.incidentTimelineEventForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const incidentTimelineEventSubmitData: IncidentTimelineEventSubmitData = {
        id: this.incidentTimelineEventSubmitData?.id || 0,
        incidentId: Number(formValue.incidentId),
        incidentEventTypeId: Number(formValue.incidentEventTypeId),
        timestamp: dateTimeLocalToIsoUtc(formValue.timestamp!.trim())!,
        actorObjectGuid: formValue.actorObjectGuid?.trim() || null,
        detailsJson: formValue.detailsJson?.trim() || null,
        notes: formValue.notes?.trim() || null,
        source: formValue.source?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateIncidentTimelineEvent(incidentTimelineEventSubmitData);
      } else {
        this.addIncidentTimelineEvent(incidentTimelineEventSubmitData);
      }
  }

  private addIncidentTimelineEvent(incidentTimelineEventData: IncidentTimelineEventSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    incidentTimelineEventData.active = true;
    incidentTimelineEventData.deleted = false;
    this.incidentTimelineEventService.PostIncidentTimelineEvent(incidentTimelineEventData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newIncidentTimelineEvent) => {

        this.incidentTimelineEventService.ClearAllCaches();

        this.incidentTimelineEventChanged.next([newIncidentTimelineEvent]);

        this.alertService.showMessage("Incident Timeline Event added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/incidenttimelineevent', newIncidentTimelineEvent.id]);
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
                                   'You do not have permission to save this Incident Timeline Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident Timeline Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident Timeline Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateIncidentTimelineEvent(incidentTimelineEventData: IncidentTimelineEventSubmitData) {
    this.incidentTimelineEventService.PutIncidentTimelineEvent(incidentTimelineEventData.id, incidentTimelineEventData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedIncidentTimelineEvent) => {

        this.incidentTimelineEventService.ClearAllCaches();

        this.incidentTimelineEventChanged.next([updatedIncidentTimelineEvent]);

        this.alertService.showMessage("Incident Timeline Event updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Incident Timeline Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident Timeline Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident Timeline Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(incidentTimelineEventData: IncidentTimelineEventData | null) {

    if (incidentTimelineEventData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.incidentTimelineEventForm.reset({
        incidentId: null,
        incidentEventTypeId: null,
        timestamp: '',
        actorObjectGuid: '',
        detailsJson: '',
        notes: '',
        source: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.incidentTimelineEventForm.reset({
        incidentId: incidentTimelineEventData.incidentId,
        incidentEventTypeId: incidentTimelineEventData.incidentEventTypeId,
        timestamp: isoUtcStringToDateTimeLocal(incidentTimelineEventData.timestamp) ?? '',
        actorObjectGuid: incidentTimelineEventData.actorObjectGuid ?? '',
        detailsJson: incidentTimelineEventData.detailsJson ?? '',
        notes: incidentTimelineEventData.notes ?? '',
        source: incidentTimelineEventData.source ?? '',
        active: incidentTimelineEventData.active ?? true,
        deleted: incidentTimelineEventData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.incidentTimelineEventForm.markAsPristine();
    this.incidentTimelineEventForm.markAsUntouched();
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


  public userIsAlertingIncidentTimelineEventReader(): boolean {
    return this.incidentTimelineEventService.userIsAlertingIncidentTimelineEventReader();
  }

  public userIsAlertingIncidentTimelineEventWriter(): boolean {
    return this.incidentTimelineEventService.userIsAlertingIncidentTimelineEventWriter();
  }
}
