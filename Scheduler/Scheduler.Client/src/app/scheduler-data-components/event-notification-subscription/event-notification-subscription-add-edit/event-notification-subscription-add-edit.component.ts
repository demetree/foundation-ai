/*
   GENERATED FORM FOR THE EVENTNOTIFICATIONSUBSCRIPTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EventNotificationSubscription table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to event-notification-subscription-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EventNotificationSubscriptionService, EventNotificationSubscriptionData, EventNotificationSubscriptionSubmitData } from '../../../scheduler-data-services/event-notification-subscription.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { EventNotificationTypeService } from '../../../scheduler-data-services/event-notification-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface EventNotificationSubscriptionFormValues {
  resourceId: number | bigint | null,       // For FK link number
  contactId: number | bigint | null,       // For FK link number
  eventNotificationTypeId: number | bigint,       // For FK link number
  triggerEvents: string,     // Stored as string for form input, converted to number on submit.
  recipientAddress: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-event-notification-subscription-add-edit',
  templateUrl: './event-notification-subscription-add-edit.component.html',
  styleUrls: ['./event-notification-subscription-add-edit.component.scss']
})
export class EventNotificationSubscriptionAddEditComponent {
  @ViewChild('eventNotificationSubscriptionModal') eventNotificationSubscriptionModal!: TemplateRef<any>;
  @Output() eventNotificationSubscriptionChanged = new Subject<EventNotificationSubscriptionData[]>();
  @Input() eventNotificationSubscriptionSubmitData: EventNotificationSubscriptionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EventNotificationSubscriptionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public eventNotificationSubscriptionForm: FormGroup = this.fb.group({
        resourceId: [null],
        contactId: [null],
        eventNotificationTypeId: [null, Validators.required],
        triggerEvents: ['', Validators.required],
        recipientAddress: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  eventNotificationSubscriptions$ = this.eventNotificationSubscriptionService.GetEventNotificationSubscriptionList();
  resources$ = this.resourceService.GetResourceList();
  contacts$ = this.contactService.GetContactList();
  eventNotificationTypes$ = this.eventNotificationTypeService.GetEventNotificationTypeList();

  constructor(
    private modalService: NgbModal,
    private eventNotificationSubscriptionService: EventNotificationSubscriptionService,
    private resourceService: ResourceService,
    private contactService: ContactService,
    private eventNotificationTypeService: EventNotificationTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(eventNotificationSubscriptionData?: EventNotificationSubscriptionData) {

    if (eventNotificationSubscriptionData != null) {

      if (!this.eventNotificationSubscriptionService.userIsSchedulerEventNotificationSubscriptionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Event Notification Subscriptions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.eventNotificationSubscriptionSubmitData = this.eventNotificationSubscriptionService.ConvertToEventNotificationSubscriptionSubmitData(eventNotificationSubscriptionData);
      this.isEditMode = true;
      this.objectGuid = eventNotificationSubscriptionData.objectGuid;

      this.buildFormValues(eventNotificationSubscriptionData);

    } else {

      if (!this.eventNotificationSubscriptionService.userIsSchedulerEventNotificationSubscriptionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Event Notification Subscriptions`,
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
        this.eventNotificationSubscriptionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.eventNotificationSubscriptionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.eventNotificationSubscriptionModal, {
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

    if (this.eventNotificationSubscriptionService.userIsSchedulerEventNotificationSubscriptionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Event Notification Subscriptions`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.eventNotificationSubscriptionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.eventNotificationSubscriptionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.eventNotificationSubscriptionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const eventNotificationSubscriptionSubmitData: EventNotificationSubscriptionSubmitData = {
        id: this.eventNotificationSubscriptionSubmitData?.id || 0,
        resourceId: formValue.resourceId ? Number(formValue.resourceId) : null,
        contactId: formValue.contactId ? Number(formValue.contactId) : null,
        eventNotificationTypeId: Number(formValue.eventNotificationTypeId),
        triggerEvents: Number(formValue.triggerEvents),
        recipientAddress: formValue.recipientAddress!.trim(),
        versionNumber: this.eventNotificationSubscriptionSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateEventNotificationSubscription(eventNotificationSubscriptionSubmitData);
      } else {
        this.addEventNotificationSubscription(eventNotificationSubscriptionSubmitData);
      }
  }

  private addEventNotificationSubscription(eventNotificationSubscriptionData: EventNotificationSubscriptionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    eventNotificationSubscriptionData.versionNumber = 0;
    eventNotificationSubscriptionData.active = true;
    eventNotificationSubscriptionData.deleted = false;
    this.eventNotificationSubscriptionService.PostEventNotificationSubscription(eventNotificationSubscriptionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newEventNotificationSubscription) => {

        this.eventNotificationSubscriptionService.ClearAllCaches();

        this.eventNotificationSubscriptionChanged.next([newEventNotificationSubscription]);

        this.alertService.showMessage("Event Notification Subscription added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/eventnotificationsubscription', newEventNotificationSubscription.id]);
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
                                   'You do not have permission to save this Event Notification Subscription.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Event Notification Subscription.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Event Notification Subscription could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateEventNotificationSubscription(eventNotificationSubscriptionData: EventNotificationSubscriptionSubmitData) {
    this.eventNotificationSubscriptionService.PutEventNotificationSubscription(eventNotificationSubscriptionData.id, eventNotificationSubscriptionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedEventNotificationSubscription) => {

        this.eventNotificationSubscriptionService.ClearAllCaches();

        this.eventNotificationSubscriptionChanged.next([updatedEventNotificationSubscription]);

        this.alertService.showMessage("Event Notification Subscription updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Event Notification Subscription.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Event Notification Subscription.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Event Notification Subscription could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(eventNotificationSubscriptionData: EventNotificationSubscriptionData | null) {

    if (eventNotificationSubscriptionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.eventNotificationSubscriptionForm.reset({
        resourceId: null,
        contactId: null,
        eventNotificationTypeId: null,
        triggerEvents: '',
        recipientAddress: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.eventNotificationSubscriptionForm.reset({
        resourceId: eventNotificationSubscriptionData.resourceId,
        contactId: eventNotificationSubscriptionData.contactId,
        eventNotificationTypeId: eventNotificationSubscriptionData.eventNotificationTypeId,
        triggerEvents: eventNotificationSubscriptionData.triggerEvents?.toString() ?? '',
        recipientAddress: eventNotificationSubscriptionData.recipientAddress ?? '',
        versionNumber: eventNotificationSubscriptionData.versionNumber?.toString() ?? '',
        active: eventNotificationSubscriptionData.active ?? true,
        deleted: eventNotificationSubscriptionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.eventNotificationSubscriptionForm.markAsPristine();
    this.eventNotificationSubscriptionForm.markAsUntouched();
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


  public userIsSchedulerEventNotificationSubscriptionReader(): boolean {
    return this.eventNotificationSubscriptionService.userIsSchedulerEventNotificationSubscriptionReader();
  }

  public userIsSchedulerEventNotificationSubscriptionWriter(): boolean {
    return this.eventNotificationSubscriptionService.userIsSchedulerEventNotificationSubscriptionWriter();
  }
}
