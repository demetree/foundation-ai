/*
   GENERATED FORM FOR THE EVENTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EventType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to event-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EventTypeService, EventTypeData, EventTypeSubmitData } from '../../../scheduler-data-services/event-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { ChargeTypeService } from '../../../scheduler-data-services/charge-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface EventTypeFormValues {
  name: string,
  description: string,
  color: string | null,
  iconId: number | bigint | null,       // For FK link number
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  requiresRentalAgreement: boolean,
  requiresExternalContact: boolean,
  requiresPayment: boolean,
  requiresDeposit: boolean,
  requiresBarService: boolean,
  allowsTicketSales: boolean,
  isInternalEvent: boolean,
  defaultPrice: string | null,     // Stored as string for form input, converted to number on submit.
  chargeTypeId: number | bigint | null,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-event-type-add-edit',
  templateUrl: './event-type-add-edit.component.html',
  styleUrls: ['./event-type-add-edit.component.scss']
})
export class EventTypeAddEditComponent {
  @ViewChild('eventTypeModal') eventTypeModal!: TemplateRef<any>;
  @Output() eventTypeChanged = new Subject<EventTypeData[]>();
  @Input() eventTypeSubmitData: EventTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EventTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public eventTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        color: [''],
        iconId: [null],
        sequence: [''],
        requiresRentalAgreement: [false],
        requiresExternalContact: [false],
        requiresPayment: [false],
        requiresDeposit: [false],
        requiresBarService: [false],
        allowsTicketSales: [false],
        isInternalEvent: [false],
        defaultPrice: [''],
        chargeTypeId: [null],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  eventTypes$ = this.eventTypeService.GetEventTypeList();
  icons$ = this.iconService.GetIconList();
  chargeTypes$ = this.chargeTypeService.GetChargeTypeList();

  constructor(
    private modalService: NgbModal,
    private eventTypeService: EventTypeService,
    private iconService: IconService,
    private chargeTypeService: ChargeTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(eventTypeData?: EventTypeData) {

    if (eventTypeData != null) {

      if (!this.eventTypeService.userIsSchedulerEventTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Event Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.eventTypeSubmitData = this.eventTypeService.ConvertToEventTypeSubmitData(eventTypeData);
      this.isEditMode = true;
      this.objectGuid = eventTypeData.objectGuid;

      this.buildFormValues(eventTypeData);

    } else {

      if (!this.eventTypeService.userIsSchedulerEventTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Event Types`,
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
        this.eventTypeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.eventTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.eventTypeModal, {
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

    if (this.eventTypeService.userIsSchedulerEventTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Event Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.eventTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.eventTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.eventTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const eventTypeSubmitData: EventTypeSubmitData = {
        id: this.eventTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        color: formValue.color?.trim() || null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        requiresRentalAgreement: !!formValue.requiresRentalAgreement,
        requiresExternalContact: !!formValue.requiresExternalContact,
        requiresPayment: !!formValue.requiresPayment,
        requiresDeposit: !!formValue.requiresDeposit,
        requiresBarService: !!formValue.requiresBarService,
        allowsTicketSales: !!formValue.allowsTicketSales,
        isInternalEvent: !!formValue.isInternalEvent,
        defaultPrice: formValue.defaultPrice ? Number(formValue.defaultPrice) : null,
        chargeTypeId: formValue.chargeTypeId ? Number(formValue.chargeTypeId) : null,
        versionNumber: this.eventTypeSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateEventType(eventTypeSubmitData);
      } else {
        this.addEventType(eventTypeSubmitData);
      }
  }

  private addEventType(eventTypeData: EventTypeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    eventTypeData.versionNumber = 0;
    eventTypeData.active = true;
    eventTypeData.deleted = false;
    this.eventTypeService.PostEventType(eventTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newEventType) => {

        this.eventTypeService.ClearAllCaches();

        this.eventTypeChanged.next([newEventType]);

        this.alertService.showMessage("Event Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/eventtype', newEventType.id]);
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
                                   'You do not have permission to save this Event Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Event Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Event Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateEventType(eventTypeData: EventTypeSubmitData) {
    this.eventTypeService.PutEventType(eventTypeData.id, eventTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedEventType) => {

        this.eventTypeService.ClearAllCaches();

        this.eventTypeChanged.next([updatedEventType]);

        this.alertService.showMessage("Event Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Event Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Event Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Event Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(eventTypeData: EventTypeData | null) {

    if (eventTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.eventTypeForm.reset({
        name: '',
        description: '',
        color: '',
        iconId: null,
        sequence: '',
        requiresRentalAgreement: false,
        requiresExternalContact: false,
        requiresPayment: false,
        requiresDeposit: false,
        requiresBarService: false,
        allowsTicketSales: false,
        isInternalEvent: false,
        defaultPrice: '',
        chargeTypeId: null,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.eventTypeForm.reset({
        name: eventTypeData.name ?? '',
        description: eventTypeData.description ?? '',
        color: eventTypeData.color ?? '',
        iconId: eventTypeData.iconId,
        sequence: eventTypeData.sequence?.toString() ?? '',
        requiresRentalAgreement: eventTypeData.requiresRentalAgreement ?? false,
        requiresExternalContact: eventTypeData.requiresExternalContact ?? false,
        requiresPayment: eventTypeData.requiresPayment ?? false,
        requiresDeposit: eventTypeData.requiresDeposit ?? false,
        requiresBarService: eventTypeData.requiresBarService ?? false,
        allowsTicketSales: eventTypeData.allowsTicketSales ?? false,
        isInternalEvent: eventTypeData.isInternalEvent ?? false,
        defaultPrice: eventTypeData.defaultPrice?.toString() ?? '',
        chargeTypeId: eventTypeData.chargeTypeId,
        versionNumber: eventTypeData.versionNumber?.toString() ?? '',
        active: eventTypeData.active ?? true,
        deleted: eventTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.eventTypeForm.markAsPristine();
    this.eventTypeForm.markAsUntouched();
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


  public userIsSchedulerEventTypeReader(): boolean {
    return this.eventTypeService.userIsSchedulerEventTypeReader();
  }

  public userIsSchedulerEventTypeWriter(): boolean {
    return this.eventTypeService.userIsSchedulerEventTypeWriter();
  }
}
