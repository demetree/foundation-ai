import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ToastNotificationService, ToastNotification } from '../../services/toast-notification.service';


@Component({
  selector: 'app-toast-notification',
  templateUrl: './toast-notification.component.html',
  styleUrls: ['./toast-notification.component.scss']
})
export class ToastNotificationComponent implements OnInit, OnDestroy {

  toasts: ToastNotification[] = [];
  private subscriptions: Subscription[] = [];


  constructor(
    public toastService: ToastNotificationService,
    private router: Router
  ) {
  }

  ngOnInit(): void {
    this.toasts = this.toastService.toasts;

    this.subscriptions.push(
      this.toastService.toastAdded$.subscribe(() => {
        this.toasts = [...this.toastService.toasts];
      })
    );

    this.subscriptions.push(
      this.toastService.toastRemoved$.subscribe(() => {
        this.toasts = [...this.toastService.toasts];
      })
    );
  }

  dismiss(id: number): void {
    this.toastService.dismiss(id);
  }

  onToastClick(toast: ToastNotification): void {
    this.toastService.toastClicked$.next(toast);

    if (toast.routerLink) {
      this.router.navigate(toast.routerLink);
    }
    this.dismiss(toast.id);
  }

  getIcon(toast: ToastNotification): string {
    if (toast.icon) return toast.icon;

    switch (toast.type) {
      case 'message': return 'fa-solid fa-comment';
      case 'success': return 'fa-solid fa-check-circle';
      case 'warning': return 'fa-solid fa-triangle-exclamation';
      case 'error': return 'fa-solid fa-circle-xmark';
      default: return 'fa-solid fa-bell';
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(s => s.unsubscribe());
  }
}
