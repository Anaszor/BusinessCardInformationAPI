
import { ChangeDetectionStrategy, Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationComponent {
  notificationService = inject(NotificationService);
  notification = this.notificationService.notification;

  colorClasses = computed(() => {
    const notif = this.notification();
    if (!notif) return {};
    switch (notif.type) {
      case 'success':
        return {
          'bg-green-100': true,
          'border-green-400': true,
          'text-green-700': true,
        };
      case 'error':
        return {
          'bg-red-100': true,
          'border-red-400': true,
          'text-red-700': true,
        };
      case 'info':
      default:
        return {
          'bg-blue-100': true,
          'border-blue-400': true,
          'text-blue-700': true,
        };
    }
  });
}
