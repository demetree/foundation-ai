/*
   GENERATED FORM FOR THE EVENTNOTIFICATIONTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EventNotificationType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to event-notification-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EventNotificationTypeService, EventNotificationTypeData, EventNotificationTypeSubmitData } from '../../../scheduler-data-services/event-notification-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface EventNotificationTypeFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  color: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-event-notification-type-add-edit',
  templateUrl: './event-notification-type-add-edit.component.html',
  styleUrls: ['./event-notification-type-add-edit.component.scss']
})
export class EventNotificationTypeAddEditComponent {
  @ViewChild('eventNotificationTypeModal') eventNotificationTypeModal!: TemplateRef<any>;
  @Output() eventNotificationTypeChanged = new Subject<EventNotificationTypeData[]>();
  @Input() eventNotificationTypeSubmitData: EventNotificationTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EventNotificationTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public eventNotificationTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        color: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  eventNotificationTypes$ = this.eventNotificationTypeService.GetEventNotificationTypeList();

  constructor(
    private modalService: NgbModal,
    private eventNotificationTypeService: EventNotificationTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(eventNotificationTypeData?: EventNotificationTypeData) {

    if (eventNotificationTypeData != null) {

      if (!this.eventNotificationTypeService.userIsSchedulerEventNotificationTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Event Notification Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.eventNotificationTypeSubmitData = this.eventNotificationTypeService.ConvertToEventNotificationTypeSubmitData(eventNotificationTypeData);
      this.isEditMode = true;
      this.objectGuid = eventNotificationTypeData.objectGuid;

      this.buildFormValues(eventNotificationTypeData);

    } else {

      if (!this.eventNotificationTypeService.userIsSchedulerEventNotificationTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Event Notification Types`,
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
        this.eventNotificationTypeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.eventNotificationTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.eventNotificationTypeModal, {
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

    if (this.eventNotificationTypeService.userIsSchedulerEventNotificationTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Event Notification Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.eventNotificationTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.eventNotificationTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.eventNotificationTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const eventNotificationTypeSubmitData: EventNotificationTypeSubmitData = {
        id: this.eventNotificationTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateEventNotificationType(eventNotificationTypeSubmitData);
      } else {
        this.addEventNotificationType(eventNotificationTypeSubmitData);
      }
  }

  private addEventNotificationType(eventNotificationTypeData: EventNotificationTypeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    eventNotificationTypeData.active = true;
    eventNotificationTypeData.deleted = false;
    this.eventNotificationTypeService.PostEventNotificationType(eventNotificationTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newEventNotificationType) => {

        this.eventNotificationTypeService.ClearAllCaches();

        this.eventNotificationTypeChanged.next([newEventNotificationType]);

        this.alertService.showMessage("Event Notification Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/eventnotificationtype', newEventNotificationType.id]);
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
                                   'You do not have permission to save this Event Notification Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Event Notification Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Event Notification Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateEventNotificationType(eventNotificationTypeData: EventNotificationTypeSubmitData) {
    this.eventNotificationTypeService.PutEventNotificationType(eventNotificationTypeData.id, eventNotificationTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedEventNotificationType) => {

        this.eventNotificationTypeService.ClearAllCaches();

        this.eventNotificationTypeChanged.next([updatedEventNotificationType]);

        this.alertService.showMessage("Event Notification Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Event Notification Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Event Notification Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Event Notification Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(eventNotificationTypeData: EventNotificationTypeData | null) {

    if (eventNotificationTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.eventNotificationTypeForm.reset({
        name: '',
        description: '',
        sequence: '',
        color: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.eventNotificationTypeForm.reset({
        name: eventNotificationTypeData.name ?? '',
        description: eventNotificationTypeData.description ?? '',
        sequence: eventNotificationTypeData.sequence?.toString() ?? '',
        color: eventNotificationTypeData.color ?? '',
        active: eventNotificationTypeData.active ?? true,
        deleted: eventNotificationTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.eventNotificationTypeForm.markAsPristine();
    this.eventNotificationTypeForm.markAsUntouched();
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


  public userIsSchedulerEventNotificationTypeReader(): boolean {
    return this.eventNotificationTypeService.userIsSchedulerEventNotificationTypeReader();
  }

  public userIsSchedulerEventNotificationTypeWriter(): boolean {
    return this.eventNotificationTypeService.userIsSchedulerEventNotificationTypeWriter();
  }
}
