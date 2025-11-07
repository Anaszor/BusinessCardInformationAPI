
import { Injectable, signal } from '@angular/core';

export type NotificationType = 'success' | 'error' | 'info';

export interface Notification {
  message: string;
  type: NotificationType;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  notification = signal<Notification | null>(null);

  show(message: string, type: NotificationType = 'info') {
    this.notification.set({ message, type });
    setTimeout(() => this.clear(), 3000);
  }

  clear() {
    this.notification.set(null);
  }
}
