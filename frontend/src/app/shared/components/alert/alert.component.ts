import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../icon/icon.component';

type AlertVariant = 'error' | 'success' | 'warning' | 'info';

@Component({
  selector: 'app-alert',
  standalone: true,
  imports: [CommonModule, IconComponent],
  templateUrl: './alert.component.html',
  styleUrls: ['./alert.component.scss'],
})
export class AlertComponent {
  @Input() variant: AlertVariant = 'info';
  @Input() message: string = '';
  @Input() dismissible: boolean = false;

  @Output() dismiss = new EventEmitter<void>();

  get alertClass(): string {
    return `alert-${this.variant}`;
  }

  get iconName(): string {
    const icons: Record<AlertVariant, string> = {
      error: 'alert-circle',
      success: 'check-circle',
      warning: 'alert-triangle',
      info: 'info-circle',
    };
    return icons[this.variant];
  }

  onDismiss() {
    this.dismiss.emit();
  }
}
