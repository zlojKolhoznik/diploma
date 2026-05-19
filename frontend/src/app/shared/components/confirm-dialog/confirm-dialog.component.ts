import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalComponent } from '../modal/modal.component';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, ModalComponent],
  template: `
    <app-modal [title]="title" [open]="open" size="sm" (closed)="cancel()">
      <p class="confirm-message">{{ message }}</p>
      <div class="confirm-actions">
        <button class="btn btn--ghost" (click)="cancel()" type="button">Cancel</button>
        <button class="btn btn--danger" (click)="confirm()" type="button">{{ confirmLabel }}</button>
      </div>
    </app-modal>
  `,
  styles: [`
    .confirm-message { color: var(--text-secondary); margin-bottom: var(--space-lg); }
    .confirm-actions { display: flex; gap: var(--space-sm); justify-content: flex-end; }
    .btn {
      padding: var(--space-sm) var(--space-lg);
      border-radius: var(--radius-md);
      font-size: var(--text-sm);
      font-weight: 600;
      cursor: pointer;
      border: none;
      transition: all 0.2s;
    }
    .btn--ghost {
      background: transparent;
      border: 1px solid var(--border-default);
      color: var(--text-secondary);
      &:hover { background: var(--surface-700); color: var(--text-primary); }
    }
    .btn--danger {
      background: var(--state-error);
      color: white;
      &:hover { opacity: 0.85; }
    }
  `],
})
export class ConfirmDialogComponent {
  @Input() title: string = 'Are you sure?';
  @Input() message: string = 'This action cannot be undone.';
  @Input() confirmLabel: string = 'Delete';
  @Input() open: boolean = false;
  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  confirm(): void { this.confirmed.emit(); }
  cancel(): void { this.cancelled.emit(); }
}

