/*
   GENERATED FORM FOR THE NOTIFICATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Notification table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to notification-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { NotificationService, NotificationData, NotificationSubmitData } from '../../../scheduler-data-services/notification.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { NotificationTypeService } from '../../../scheduler-data-services/notification-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface NotificationFormValues {
  notificationTypeId: number | bigint | null,       // For FK link number
  createdByUserId: string | null,     // Stored as string for form input, converted to number on submit.
  message: string,
  priority: string,     // Stored as string for form input, converted to number on submit.
  entity: string | null,
  entityId: string | null,     // Stored as string for form input, converted to number on submit.
  externalURL: string | null,
  dateTimeCreated: string,
  dateTimeDistributed: string | null,
  distributionCompleted: boolean,
  userId: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-notification-add-edit',
  templateUrl: './notification-add-edit.component.html',
  styleUrls: ['./notification-add-edit.component.scss']
})
export class NotificationAddEditComponent {
  @ViewChild('notificationModal') notificationModal!: TemplateRef<any>;
  @Output() notificationChanged = new Subject<NotificationData[]>();
  @Input() notificationSubmitData: NotificationSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<NotificationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public notificationForm: FormGroup = this.fb.group({
        notificationTypeId: [null],
        createdByUserId: [''],
        message: ['', Validators.required],
        priority: ['', Validators.required],
        entity: [''],
        entityId: [''],
        externalURL: [''],
        dateTimeCreated: ['', Validators.required],
        dateTimeDistributed: [''],
        distributionCompleted: [false],
        userId: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  notifications$ = this.notificationService.GetNotificationList();
  notificationTypes$ = this.notificationTypeService.GetNotificationTypeList();

  constructor(
    private modalService: NgbModal,
    private notificationService: NotificationService,
    private notificationTypeService: NotificationTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(notificationData?: NotificationData) {

    if (notificationData != null) {

      if (!this.notificationService.userIsSchedulerNotificationReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Notifications`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.notificationSubmitData = this.notificationService.ConvertToNotificationSubmitData(notificationData);
      this.isEditMode = true;
      this.objectGuid = notificationData.objectGuid;

      this.buildFormValues(notificationData);

    } else {

      if (!this.notificationService.userIsSchedulerNotificationWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Notifications`,
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
        this.notificationForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.notificationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.notificationModal, {
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

    if (this.notificationService.userIsSchedulerNotificationWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Notifications`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.notificationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.notificationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.notificationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const notificationSubmitData: NotificationSubmitData = {
        id: this.notificationSubmitData?.id || 0,
        notificationTypeId: formValue.notificationTypeId ? Number(formValue.notificationTypeId) : null,
        createdByUserId: formValue.createdByUserId ? Number(formValue.createdByUserId) : null,
        message: formValue.message!.trim(),
        priority: Number(formValue.priority),
        entity: formValue.entity?.trim() || null,
        entityId: formValue.entityId ? Number(formValue.entityId) : null,
        externalURL: formValue.externalURL?.trim() || null,
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        dateTimeDistributed: formValue.dateTimeDistributed ? dateTimeLocalToIsoUtc(formValue.dateTimeDistributed.trim()) : null,
        distributionCompleted: !!formValue.distributionCompleted,
        userId: formValue.userId ? Number(formValue.userId) : null,
        versionNumber: this.notificationSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateNotification(notificationSubmitData);
      } else {
        this.addNotification(notificationSubmitData);
      }
  }

  private addNotification(notificationData: NotificationSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    notificationData.versionNumber = 0;
    notificationData.active = true;
    notificationData.deleted = false;
    this.notificationService.PostNotification(notificationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newNotification) => {

        this.notificationService.ClearAllCaches();

        this.notificationChanged.next([newNotification]);

        this.alertService.showMessage("Notification added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/notification', newNotification.id]);
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
                                   'You do not have permission to save this Notification.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Notification.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Notification could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateNotification(notificationData: NotificationSubmitData) {
    this.notificationService.PutNotification(notificationData.id, notificationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedNotification) => {

        this.notificationService.ClearAllCaches();

        this.notificationChanged.next([updatedNotification]);

        this.alertService.showMessage("Notification updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Notification.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Notification.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Notification could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(notificationData: NotificationData | null) {

    if (notificationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.notificationForm.reset({
        notificationTypeId: null,
        createdByUserId: '',
        message: '',
        priority: '',
        entity: '',
        entityId: '',
        externalURL: '',
        dateTimeCreated: '',
        dateTimeDistributed: '',
        distributionCompleted: false,
        userId: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.notificationForm.reset({
        notificationTypeId: notificationData.notificationTypeId,
        createdByUserId: notificationData.createdByUserId?.toString() ?? '',
        message: notificationData.message ?? '',
        priority: notificationData.priority?.toString() ?? '',
        entity: notificationData.entity ?? '',
        entityId: notificationData.entityId?.toString() ?? '',
        externalURL: notificationData.externalURL ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(notificationData.dateTimeCreated) ?? '',
        dateTimeDistributed: isoUtcStringToDateTimeLocal(notificationData.dateTimeDistributed) ?? '',
        distributionCompleted: notificationData.distributionCompleted ?? false,
        userId: notificationData.userId?.toString() ?? '',
        versionNumber: notificationData.versionNumber?.toString() ?? '',
        active: notificationData.active ?? true,
        deleted: notificationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.notificationForm.markAsPristine();
    this.notificationForm.markAsUntouched();
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


  public userIsSchedulerNotificationReader(): boolean {
    return this.notificationService.userIsSchedulerNotificationReader();
  }

  public userIsSchedulerNotificationWriter(): boolean {
    return this.notificationService.userIsSchedulerNotificationWriter();
  }
}
