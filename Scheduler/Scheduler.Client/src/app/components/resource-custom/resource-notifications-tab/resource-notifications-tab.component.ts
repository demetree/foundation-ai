import { Component, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { Subject } from 'rxjs'
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ResourceData } from '../../../scheduler-data-services/resource.service';
import { NotificationSubscriptionService, NotificationSubscriptionData } from '../../../scheduler-data-services/notification-subscription.service';
import { NotificationSubscriptionCustomAddEditModalComponent } from '../notification-subscription-custom-add-edit-modal/notification-subscription-custom-add-edit-modal.component';
import { ConfirmationService } from '../../../services/confirmation-service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

/**
 * Notification Subscriptions tab for the Resource detail page.
 *
 * Displays and manages notification preferences (Email, SMS, Push) for this resource.
 * Supports:
 * - List with recipient and trigger summary
 * - Add/Edit/Delete subscriptions
 * - Bitmask triggerEvents handled via checkboxes
 * - Badge count on tab
 */
@Component({
  selector: 'app-resource-notifications-tab',
  templateUrl: './resource-notifications-tab.component.html',
  styleUrls: ['./resource-notifications-tab.component.scss']
})
export class ResourceNotificationsTabComponent implements OnChanges {
  @Input() resource!: ResourceData | null;

  // Triggers when a notification subscription is changed.  To be implemented by users of this component.
  @Output() resourceNotificationSubscriptionChanged = new Subject<NotificationSubscriptionData>();


  public subscriptions: NotificationSubscriptionData[] | null = null;
  public isLoading = true;
  public error: string | null = null;

  // Bitmask constants for triggerEvents
  public readonly TRIGGER_ASSIGNED = 1;
  public readonly TRIGGER_CANCELED = 2;
  public readonly TRIGGER_MODIFIED = 4;
  public readonly TRIGGER_REMINDER = 8;

  constructor(private modalService: NgbModal,
    private notificationSubscriptionService: NotificationSubscriptionService,
    private confirmationService: ConfirmationService,
    private alertService: AlertService,
  ) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['resource'] && this.resource) {

      this.resource.ClearNotificationSubscriptionsCache();

      this.loadSubscriptions();
    }
  }

  /**
   * Loads subscriptions using the lazy promise on the resource object.
   */
  public loadSubscriptions(): void {
    if (!this.resource) {
      this.subscriptions = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    this.resource.NotificationSubscriptions
      .then(subs => {
        this.subscriptions = subs ?? [];
        this.isLoading = false;
      })
      .catch(err => {
        console.error('Failed to load notification subscriptions', err);
        this.error = 'Unable to load notification subscriptions';
        this.subscriptions = [];
        this.isLoading = false;
      });
  }

  /**
   * Opens modal to add or edit a subscription.
   */
  public openAddEditModal(subscription?: NotificationSubscriptionData): void {
    if (!this.resource) return;

    const modalRef = this.modalService.open(NotificationSubscriptionCustomAddEditModalComponent, {
      size: 'md',
      backdrop: 'static'
    });

    modalRef.componentInstance.resource = this.resource;
    if (subscription) {
      modalRef.componentInstance.existingSubscription = subscription;
    }

    modalRef.componentInstance.subscriptionChanged.subscribe((data: NotificationSubscriptionData) => {
      this.resource?.ClearNotificationSubscriptionsCache();

      this.resourceNotificationSubscriptionChanged.next(data);

      this.loadSubscriptions();
    });
  }

  /**
   * Deletes a subscription.
   */
  public deleteSubscription(sub: NotificationSubscriptionData): void {

    this.confirmationService
      .confirm('Delete Notification Subscription', 'Are you sure you want to delete this Notification Subscription?')
      .then((result) => {
        if (result) {
          this.deleteNotificationSubscription(sub);
        }
      })
      .catch(() => { });
  }


  private deleteNotificationSubscription(notificationSubscriptionData: NotificationSubscriptionData): void {
    this.notificationSubscriptionService.DeleteNotificationSubscription(notificationSubscriptionData.id).subscribe({
      next: (data) => {
        this.notificationSubscriptionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        this.resource?.ClearNotificationSubscriptionsCache();

        this.loadSubscriptions(); // Reload the data list after deletion

        this.resourceNotificationSubscriptionChanged.next(data);
      },
      error: (err) => {
        this.alertService.showMessage("Error deleting Notification Subscription", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  /**
   * Helper: check if a trigger flag is set in the bitmask.
   */
  public hasTrigger(triggerEvents: number, flag: number): boolean {
    return ((triggerEvents as number) & flag) === flag;
  }

  /**
   * Helper: get human-readable trigger list.
   */
  public getTriggerNames(triggerEvents: number | bigint): string {
    const triggers: string[] = [];
    if (this.hasTrigger(triggerEvents as number, this.TRIGGER_ASSIGNED)) triggers.push('Assigned');
    if (this.hasTrigger(triggerEvents as number, this.TRIGGER_CANCELED)) triggers.push('Canceled');
    if (this.hasTrigger(triggerEvents as number, this.TRIGGER_MODIFIED)) triggers.push('Modified');
    if (this.hasTrigger(triggerEvents as number, this.TRIGGER_REMINDER)) triggers.push('Reminder');
    return triggers.length > 0 ? triggers.join(', ') : 'None';
  }
}
