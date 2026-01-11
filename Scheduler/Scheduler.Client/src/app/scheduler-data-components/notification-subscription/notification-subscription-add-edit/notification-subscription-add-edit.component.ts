/*
   GENERATED FORM FOR THE NOTIFICATIONSUBSCRIPTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from NotificationSubscription table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to notification-subscription-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { NotificationSubscriptionService, NotificationSubscriptionData, NotificationSubscriptionSubmitData } from '../../../scheduler-data-services/notification-subscription.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { NotificationTypeService } from '../../../scheduler-data-services/notification-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface NotificationSubscriptionFormValues {
  resourceId: number | bigint | null,       // For FK link number
  contactId: number | bigint | null,       // For FK link number
  notificationTypeId: number | bigint,       // For FK link number
  triggerEvents: string,     // Stored as string for form input, converted to number on submit.
  recipientAddress: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-notification-subscription-add-edit',
  templateUrl: './notification-subscription-add-edit.component.html',
  styleUrls: ['./notification-subscription-add-edit.component.scss']
})
export class NotificationSubscriptionAddEditComponent {
  @ViewChild('notificationSubscriptionModal') notificationSubscriptionModal!: TemplateRef<any>;
  @Output() notificationSubscriptionChanged = new Subject<NotificationSubscriptionData[]>();
  @Input() notificationSubscriptionSubmitData: NotificationSubscriptionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<NotificationSubscriptionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public notificationSubscriptionForm: FormGroup = this.fb.group({
        resourceId: [null],
        contactId: [null],
        notificationTypeId: [null, Validators.required],
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

  notificationSubscriptions$ = this.notificationSubscriptionService.GetNotificationSubscriptionList();
  resources$ = this.resourceService.GetResourceList();
  contacts$ = this.contactService.GetContactList();
  notificationTypes$ = this.notificationTypeService.GetNotificationTypeList();

  constructor(
    private modalService: NgbModal,
    private notificationSubscriptionService: NotificationSubscriptionService,
    private resourceService: ResourceService,
    private contactService: ContactService,
    private notificationTypeService: NotificationTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(notificationSubscriptionData?: NotificationSubscriptionData) {

    if (notificationSubscriptionData != null) {

      if (!this.notificationSubscriptionService.userIsSchedulerNotificationSubscriptionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Notification Subscriptions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.notificationSubscriptionSubmitData = this.notificationSubscriptionService.ConvertToNotificationSubscriptionSubmitData(notificationSubscriptionData);
      this.isEditMode = true;
      this.objectGuid = notificationSubscriptionData.objectGuid;

      this.buildFormValues(notificationSubscriptionData);

    } else {

      if (!this.notificationSubscriptionService.userIsSchedulerNotificationSubscriptionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Notification Subscriptions`,
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
        this.notificationSubscriptionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.notificationSubscriptionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.notificationSubscriptionModal, {
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

    if (this.notificationSubscriptionService.userIsSchedulerNotificationSubscriptionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Notification Subscriptions`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.notificationSubscriptionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.notificationSubscriptionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.notificationSubscriptionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const notificationSubscriptionSubmitData: NotificationSubscriptionSubmitData = {
        id: this.notificationSubscriptionSubmitData?.id || 0,
        resourceId: formValue.resourceId ? Number(formValue.resourceId) : null,
        contactId: formValue.contactId ? Number(formValue.contactId) : null,
        notificationTypeId: Number(formValue.notificationTypeId),
        triggerEvents: Number(formValue.triggerEvents),
        recipientAddress: formValue.recipientAddress!.trim(),
        versionNumber: this.notificationSubscriptionSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateNotificationSubscription(notificationSubscriptionSubmitData);
      } else {
        this.addNotificationSubscription(notificationSubscriptionSubmitData);
      }
  }

  private addNotificationSubscription(notificationSubscriptionData: NotificationSubscriptionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    notificationSubscriptionData.versionNumber = 0;
    notificationSubscriptionData.active = true;
    notificationSubscriptionData.deleted = false;
    this.notificationSubscriptionService.PostNotificationSubscription(notificationSubscriptionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newNotificationSubscription) => {

        this.notificationSubscriptionService.ClearAllCaches();

        this.notificationSubscriptionChanged.next([newNotificationSubscription]);

        this.alertService.showMessage("Notification Subscription added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/notificationsubscription', newNotificationSubscription.id]);
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
                                   'You do not have permission to save this Notification Subscription.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Notification Subscription.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Notification Subscription could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateNotificationSubscription(notificationSubscriptionData: NotificationSubscriptionSubmitData) {
    this.notificationSubscriptionService.PutNotificationSubscription(notificationSubscriptionData.id, notificationSubscriptionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedNotificationSubscription) => {

        this.notificationSubscriptionService.ClearAllCaches();

        this.notificationSubscriptionChanged.next([updatedNotificationSubscription]);

        this.alertService.showMessage("Notification Subscription updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Notification Subscription.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Notification Subscription.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Notification Subscription could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(notificationSubscriptionData: NotificationSubscriptionData | null) {

    if (notificationSubscriptionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.notificationSubscriptionForm.reset({
        resourceId: null,
        contactId: null,
        notificationTypeId: null,
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
        this.notificationSubscriptionForm.reset({
        resourceId: notificationSubscriptionData.resourceId,
        contactId: notificationSubscriptionData.contactId,
        notificationTypeId: notificationSubscriptionData.notificationTypeId,
        triggerEvents: notificationSubscriptionData.triggerEvents?.toString() ?? '',
        recipientAddress: notificationSubscriptionData.recipientAddress ?? '',
        versionNumber: notificationSubscriptionData.versionNumber?.toString() ?? '',
        active: notificationSubscriptionData.active ?? true,
        deleted: notificationSubscriptionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.notificationSubscriptionForm.markAsPristine();
    this.notificationSubscriptionForm.markAsUntouched();
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


  public userIsSchedulerNotificationSubscriptionReader(): boolean {
    return this.notificationSubscriptionService.userIsSchedulerNotificationSubscriptionReader();
  }

  public userIsSchedulerNotificationSubscriptionWriter(): boolean {
    return this.notificationSubscriptionService.userIsSchedulerNotificationSubscriptionWriter();
  }
}
