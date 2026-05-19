import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TablerIconComponent } from '@tabler/icons-angular';
import { NotificationService, Toast } from '../../../core/services/notification.service';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule, TablerIconComponent, AsyncPipe],
  template: `
    <div class="toast-container" aria-live="polite" aria-label="Notifications">
      @for (toast of (notifications.toasts$ | async) ?? []; track toast.id) {
        <div class="toast" [class]="'toast--' + toast.type" role="alert">
          <tabler-icon [icon]="iconFor(toast)" [size]="18" class="toast-icon"></tabler-icon>
          <span class="toast-message">{{ toast.message }}</span>
          <button class="toast-close" (click)="dismiss(toast)" aria-label="Dismiss">
            <tabler-icon [icon]="'x'" [size]="16"></tabler-icon>
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      bottom: var(--space-lg);
      right: var(--space-lg);
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: var(--space-sm);
      max-width: 360px;
    }
    .toast {
      display: flex;
      align-items: flex-start;
      gap: var(--space-sm);
      padding: var(--space-md);
      border-radius: var(--radius-md);
      background-color: var(--surface-700);
      border: 1px solid var(--border-default);
      box-shadow: var(--shadow-md);
      animation: slide-in 0.2s ease;
    }
    .toast--success { border-color: var(--state-success); }
    .toast--error { border-color: var(--state-error); }
    .toast--warning { border-color: var(--state-warning); }
    .toast--info { border-color: var(--state-info); }
    .toast-icon { flex-shrink: 0; margin-top: 2px; }
    .toast--success .toast-icon { color: var(--state-success); }
    .toast--error .toast-icon { color: var(--state-error); }
    .toast--warning .toast-icon { color: var(--state-warning); }
    .toast--info .toast-icon { color: var(--state-info); }
    .toast-message { flex: 1; font-size: var(--text-sm); color: var(--text-primary); }
    .toast-close {
      background: transparent;
      border: none;
      color: var(--text-tertiary);
      cursor: pointer;
      padding: 2px;
      border-radius: var(--radius-sm);
      &:hover { color: var(--text-primary); }
    }
    @keyframes slide-in {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
  `],
})
export class ToastComponent {
  readonly notifications = inject(NotificationService);

  iconFor(toast: Toast): string {
    switch (toast.type) {
      case 'success': return 'circle-check';
      case 'error': return 'circle-x';
      case 'warning': return 'alert-triangle';
      default: return 'info-circle';
    }
  }

  dismiss(toast: Toast): void {
    this.notifications.dismiss(toast.id);
  }
}


