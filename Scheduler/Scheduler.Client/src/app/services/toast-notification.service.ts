/*

   TOAST NOTIFICATION SERVICE
   ==========================
   Manages a stack of transient toast notifications that appear in the top-right corner.
   Any component can inject this service and call show() to display a toast.

   Toasts auto-dismiss after a configurable duration (default 6 seconds).
   Supports click-to-navigate via optional routerLink.

   AI-developed as part of Messaging UI Phase 1, March 2026.

*/
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';


export interface ToastNotification {
  id: number;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error' | 'message';
  icon?: string;
  senderName?: string;
  routerLink?: string[];
  data?: any;
  duration: number;
  createdAt: Date;
}

export interface ToastOptions {
  type?: 'info' | 'success' | 'warning' | 'error' | 'message';
  icon?: string;
  senderName?: string;
  routerLink?: string[];
  data?: any;
  duration?: number;
}


@Injectable({
  providedIn: 'root'
})
export class ToastNotificationService {

  private nextId = 1;
  private maxToasts = 5;

  public toasts: ToastNotification[] = [];
  public toastAdded$ = new Subject<ToastNotification>();
  public toastRemoved$ = new Subject<number>();
  public toastClicked$ = new Subject<ToastNotification>();


  /**
   * Show a new toast notification.
   */
  show(message: string, options: ToastOptions = {}): void {
    const toast: ToastNotification = {
      id: this.nextId++,
      message,
      type: options.type || 'info',
      icon: options.icon,
      senderName: options.senderName,
      routerLink: options.routerLink,
      data: options.data,
      duration: options.duration || 6000,
      createdAt: new Date()
    };

    //
    // Limit the number of visible toasts
    //
    if (this.toasts.length >= this.maxToasts) {
      const oldest = this.toasts[0];
      this.dismiss(oldest.id);
    }

    this.toasts.push(toast);
    this.toastAdded$.next(toast);

    //
    // Auto-dismiss after duration
    //
    setTimeout(() => {
      this.dismiss(toast.id);
    }, toast.duration);
  }


  /**
   * Dismiss a toast by its ID.
   */
  dismiss(id: number): void {
    const index = this.toasts.findIndex(t => t.id === id);
    if (index !== -1) {
      this.toasts.splice(index, 1);
      this.toastRemoved$.next(id);
    }
  }


  /**
   * Convenience: show a message-type toast (for new chat messages).
   */
  showMessage(senderName: string, messagePreview: string, data?: any): void {
    this.show(messagePreview, {
      type: 'message',
      senderName,
      icon: 'fa-solid fa-comment',
      data,
      duration: 6000
    });
  }


  /**
   * Convenience: show a notification-type toast (for system notifications).
   */
  showNotification(message: string, notificationType?: string, routerLink?: string[], data?: any): void {
    this.show(message, {
      type: 'info',
      icon: 'fa-solid fa-bell',
      routerLink,
      data,
      duration: 8000
    });
  }
}
