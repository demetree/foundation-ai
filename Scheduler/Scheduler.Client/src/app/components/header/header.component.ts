import { Component, OnInit, OnDestroy, EventEmitter, Output, Input, Inject, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { NgbDropdown } from '@ng-bootstrap/ng-bootstrap';

import { AuthService } from '../../services/auth.service';
import { AlertService } from '../../services/alert.service';
import { MessagingApiService, NotificationSummary } from '../../services/messaging-api.service';
import { MessagingSignalRService } from '../../services/messaging-signalr.service';
import { ToastNotificationService } from '../../services/toast-notification.service';
import { SchedulerHelperService } from '../../services/scheduler-helper.service';
import { ThemeService, ThemeDefinition } from '../../services/theme.service';


@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit, OnDestroy {
  @Input() isUserLoggedIn!: boolean;
  @Output() toggle = new EventEmitter();
  @Output() invokeLogout = new EventEmitter();
  @Output() toggleMessaging = new EventEmitter<void>();
  @Input() projectId: number | null = null;

  /** Show in the dropdown footer. Pass from root if you want CI-stamped values (e.g., v1.12.3+abcd123). */
  @Input() versionInfo: string = 'v0.0.0';

  //
  // Notification bell state
  //
  showNotificationDropdown = false;
  notifications: NotificationSummary[] = [];
  isLoadingNotifications = false;
  unreadNotificationCount = 0;

  //
  // Unread message count (driven by sidebar via @Input)
  //
  @Input() unreadMessageCount: number = 0;

  public showNotifications: boolean = false;
  public totalEventCount: number = 0;
  public filterText: string | null = null;

  // PHONE PART: mobile menu state
  public isMobileMenuOpen = false;
  public mobileVolunteersExpanded = false;
  public mobileSetupExpanded = false;

  // To get the count of offices to allow the offices button to be invisible if there are no offices (It can always be found under Administration)
  public officeCount$ = this.schedulerHelperService.ActiveOfficeCount$;

  //
  // Entity → route mappings for deep-link navigation on notification click
  //
  private entityRouteMap: Record<string, string> = {
    'Resource': '/resources',
    'ScheduledEvent': '/scheduled-events',
    'Contact': '/contacts',
    'Client': '/clients',
    'Crew': '/crews'
  };

  private subscriptions: Subscription[] = [];


  constructor(
    public authService: AuthService,
    private alertService: AlertService,
    @Inject('BASE_URL') private baseUrl: string,
    private http: HttpClient,
    private router: Router,
    private messagingApi: MessagingApiService,
    private signalR: MessagingSignalRService,
    private toastService: ToastNotificationService,
    private schedulerHelperService: SchedulerHelperService,
    public themeService: ThemeService
  ) { }


  ngOnInit(): void {

    //
    // Load the initial unread notification count when the header renders.
    //
    if (this.isUserLoggedIn) {
      this.loadNotificationCount();
    }

    //
    // Subscribe to real-time notifications via SignalR for badge updates and toasts
    //
    this.subscriptions.push(
      this.signalR.notificationReceived$.subscribe((payload) => {
        this.unreadNotificationCount++;

        //
        // If dropdown is open, push the new notification into the visible list
        //
        if (this.showNotificationDropdown) {
          this.notifications.unshift({
            notificationDistributionId: 0,
            notificationId: 0,
            message: payload.message,
            entity: payload.entity,
            entityId: payload.entityId,
            externalURL: payload.externalURL,
            priority: payload.priority,
            notificationType: payload.notificationType,
            acknowledged: false,
            dateTimeCreated: payload.dateTimeCreated
          });
        }

        //
        // Build routerLink from entity context for click-to-navigate
        //
        let routerLink: string[] | undefined;
        if (payload.entity && payload.entityId) {
          const basePath = this.entityRouteMap[payload.entity];
          if (basePath) {
            routerLink = [basePath, payload.entityId.toString()];
          }
        }

        //
        // Pass notification data through toast for click-to-acknowledge
        //
        this.toastService.showNotification(
          payload.message,
          payload.notificationType,
          routerLink,
          {
            entity: payload.entity,
            entityId: payload.entityId,
            externalURL: payload.externalURL
          }
        );
      })
    );

    //
    // Subscribe to notification toast clicks for externalURL navigation + badge update.
    //
    this.subscriptions.push(
      this.toastService.toastClicked$.subscribe(toast => {
        if (toast.type === 'info' && toast.data != null) {
          if (toast.data.externalURL && !toast.routerLink) {
            window.open(toast.data.externalURL, '_blank');
          }

          this.unreadNotificationCount = Math.max(0, this.unreadNotificationCount - 1);
        }
      })
    );
  }


  // ─── Notification Bell ──────────────────────────────────────────────────────

  toggleNotificationDropdown(event: MouseEvent): void {
    event.stopPropagation();
    this.showNotificationDropdown = !this.showNotificationDropdown;

    if (this.showNotificationDropdown) {
      this.loadNotifications();
      this.loadNotificationCount();
    }
  }

  closeNotificationDropdown(): void {
    this.showNotificationDropdown = false;
  }

  loadNotifications(): void {
    this.isLoadingNotifications = true;

    this.messagingApi.getNotifications().subscribe({
      next: (notifications) => {
        this.notifications = notifications;
        this.isLoadingNotifications = false;
      },
      error: (error) => {
        console.error('Failed to load notifications', error);
        this.isLoadingNotifications = false;
      }
    });
  }

  loadNotificationCount(): void {
    this.messagingApi.getUnreadNotificationCount().subscribe({
      next: (result) => {
        this.unreadNotificationCount = result.unreadCount;
      }
    });
  }

  acknowledgeNotification(notification: NotificationSummary): void {
    this.messagingApi.acknowledgeNotification(notification.notificationDistributionId).subscribe({
      next: () => {
        this.notifications = this.notifications.filter(
          n => n.notificationDistributionId !== notification.notificationDistributionId
        );
        this.unreadNotificationCount = Math.max(0, this.unreadNotificationCount - 1);
      },
      error: (error) => {
        console.error('Failed to acknowledge notification', error);
      }
    });
  }

  acknowledgeAllNotifications(): void {
    this.messagingApi.acknowledgeAllNotifications().subscribe({
      next: () => {
        this.notifications = [];
        this.unreadNotificationCount = 0;
      },
      error: (error) => {
        console.error('Failed to acknowledge all notifications', error);
      }
    });
  }

  navigateToNotificationEntity(notification: NotificationSummary): void {
    if (notification.entity && notification.entityId) {
      const basePath = this.entityRouteMap[notification.entity];
      if (basePath) {
        this.router.navigate([basePath, notification.entityId.toString()]);
        this.closeNotificationDropdown();
        return;
      }
    }

    if (notification.externalURL) {
      window.open(notification.externalURL, '_blank');
      this.closeNotificationDropdown();
    }
  }

  hasNotificationLink(notification: NotificationSummary): boolean {
    if (notification.entity && notification.entityId) {
      return !!this.entityRouteMap[notification.entity];
    }
    return !!notification.externalURL;
  }

  formatNotificationTime(dateTime: string): string {
    const date = new Date(dateTime);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    return date.toLocaleDateString();
  }


  // ─── Message Icon ──────────────────────────────────────────────────────────

  onToggleMessaging(): void {
    this.router.navigate(['/messaging']);
  }


  // ─── Close dropdowns on outside click ──────────────────────────────────────

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;

    //
    // Close the notification dropdown when clicking outside of it
    //
    if (this.showNotificationDropdown &&
        target.closest('.header-notification-bell') == null &&
        target.closest('.header-notification-dropdown') == null) {
      this.showNotificationDropdown = false;
    }
  }


  // ─── Existing Methods ──────────────────────────────────────────────────────

  toggleNotifications(event: MouseEvent) {
    event.stopPropagation();
    this.showNotifications = !this.showNotifications;
  }

  closeNotificationsPanel() {
    this.showNotifications = false;
  }

  logout() {
    this.invokeLogout.emit();
    this.isMobileMenuOpen = false;
  }

  goToSettings(dropdown: NgbDropdown) {
    this.router.navigate(['/settings']);
    dropdown.close();
  }

  // PHONE PART: mobile menu controls
  toggleMobileMenu() {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu() {
    this.isMobileMenuOpen = false;
  }

  get tenantName(): string {
    return this.authService.currentUser?.tenantName ?? '';
  }

  get tenantAndUserName(): string {
    const u = this.authService.currentUser;
    return u ? `${u.tenantName} - ${u.fullName}` : '';
  }

  get currentUserFirstName(): string {
    const name = this.authService.currentUser?.fullName || '';
    return name.replace(/,/g, '');
  }

  get userInitials(): string {
    const name = this.authService.currentUser?.fullName || '';
    const initials = name
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map(part => part[0])
      .join('');
    return initials || 'U';
  }


  ngOnDestroy(): void {
    this.subscriptions.forEach(s => s.unsubscribe());
  }
}
