import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { finalize } from 'rxjs';

import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ResourceData } from '../../../scheduler-data-services/resource.service';
import { NotificationSubscriptionData, NotificationSubscriptionService, NotificationSubscriptionSubmitData } from '../../../scheduler-data-services/notification-subscription.service';
import { NotificationTypeService } from '../../../scheduler-data-services/notification-type.service';

/**
 * Modal for adding or editing a NotificationSubscription for a Resource.
 *
 * Features:
 * - Select notification type (Email, SMS, Push)
 * - Enter recipient address
 * - Checkboxes for trigger events (bitmask handled transparently)
 * - Full support for add and edit modes
 * - Emits change event on success for parent refresh
 */
@Component({
  selector: 'app-notification-subscription-custom-add-edit-modal',
  templateUrl: './notification-subscription-custom-add-edit-modal.component.html',
  styleUrls: ['./notification-subscription-custom-add-edit-modal.component.scss']
})
export class NotificationSubscriptionCustomAddEditModalComponent {
  @Input() resource!: ResourceData;
  @Input() existingSubscription: NotificationSubscriptionData | null = null;

  @Output() subscriptionChanged = new EventEmitter<NotificationSubscriptionData>();

  public subscriptionForm: FormGroup;
  public isEditMode = false;
  public isSaving = false;

  // Dropdown data
  public notificationTypes$ = this.notificationTypeService.GetNotificationTypeList();

  // Bitmask constants for triggerEvents
  private readonly TRIGGER_ASSIGNED = 1;
  private readonly TRIGGER_CANCELED = 2;
  private readonly TRIGGER_MODIFIED = 4;
  private readonly TRIGGER_REMINDER = 8;

  constructor(
    public activeModal: NgbActiveModal,
    private fb: FormBuilder,
    private notificationSubscriptionService: NotificationSubscriptionService,
    private notificationTypeService: NotificationTypeService,
    private alertService: AlertService
  ) {
    this.subscriptionForm = this.fb.group({
      notificationTypeId: [null, Validators.required],
      recipientAddress: ['', [Validators.required, Validators.maxLength(250)]],
      triggerAssigned: [false],
      triggerCanceled: [false],
      triggerModified: [false],
      triggerReminder: [false]
    });
  }

  /**
   * Called by parent when opening modal — sets up form for add or edit.
   */
  ngOnInit(): void {
    this.isEditMode = !!this.existingSubscription;

    if (this.isEditMode && this.existingSubscription) {
      const triggerEvents = this.existingSubscription.triggerEvents as number;

      this.subscriptionForm.patchValue({
        notificationTypeId: this.existingSubscription.notificationTypeId,
        recipientAddress: this.existingSubscription.recipientAddress,
        triggerAssigned: this.hasTrigger(triggerEvents, this.TRIGGER_ASSIGNED),
        triggerCanceled: this.hasTrigger(triggerEvents, this.TRIGGER_CANCELED),
        triggerModified: this.hasTrigger(triggerEvents, this.TRIGGER_MODIFIED),
        triggerReminder: this.hasTrigger(triggerEvents, this.TRIGGER_REMINDER)
      });
    }
  }

  /**
   * Builds the bitmask from checkbox values.
   */
  private buildTriggerEvents(): number {
    let mask = 0;
    if (this.subscriptionForm.get('triggerAssigned')?.value) mask |= this.TRIGGER_ASSIGNED;
    if (this.subscriptionForm.get('triggerCanceled')?.value) mask |= this.TRIGGER_CANCELED;
    if (this.subscriptionForm.get('triggerModified')?.value) mask |= this.TRIGGER_MODIFIED;
    if (this.subscriptionForm.get('triggerReminder')?.value) mask |= this.TRIGGER_REMINDER;
    return mask;
  }

  /**
   * Checks if a specific trigger flag is set in the bitmask.
   */
  private hasTrigger(triggerEvents: number, flag: number): boolean {
    return (triggerEvents & flag) === flag;
  }

  /**
   * Submit handler — creates or updates the subscription.
   */
  public submitForm(): void {
    if (this.isSaving) return;

    if (!this.subscriptionForm.valid) {
      this.subscriptionForm.markAllAsTouched();
      this.alertService.showMessage('Please fix form errors', '', MessageSeverity.warn);
      return;
    }

    this.isSaving = true;

    const formValue = this.subscriptionForm.getRawValue();

    const submitData: NotificationSubscriptionSubmitData = {
      id: this.existingSubscription?.id || 0,
      resourceId: this.resource.id,
      contactId: null,      // this needs to be null in this context
      notificationTypeId: Number(formValue.notificationTypeId),
      triggerEvents: this.buildTriggerEvents(),
      recipientAddress: formValue.recipientAddress.trim(),
      versionNumber: this.existingSubscription?.versionNumber ?? 0,
      active: true,
      deleted: false
    };

    const operation$ = this.isEditMode
      ? this.notificationSubscriptionService.PutNotificationSubscription(submitData.id, submitData)
      : this.notificationSubscriptionService.PostNotificationSubscription(submitData);

    operation$.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (data) => {
        this.alertService.showMessage(
          this.isEditMode ? 'Subscription updated' : 'Subscription added',
          '',
          MessageSeverity.success
        );
        this.subscriptionChanged.emit(data);
        this.activeModal.close();
      },
      error: (err) => {
        this.alertService.showMessage(
          'Save failed',
          err?.error?.message || 'An error occurred',
          MessageSeverity.error
        );
      }
    });
  }

  public closeModal(): void {
    this.activeModal.dismiss('cancel');
  }
}
