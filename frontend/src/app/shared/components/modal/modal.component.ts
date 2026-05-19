import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TablerIconComponent } from '@tabler/icons-angular';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule, TablerIconComponent],
  template: `
    @if (open) {
      <div class="modal-backdrop" (click)="onBackdropClick($event)" role="dialog" [attr.aria-label]="title" aria-modal="true">
        <div class="modal-panel" [class]="'modal-panel--' + size" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3 class="modal-title">{{ title }}</h3>
            <button class="modal-close" (click)="close()" aria-label="Close modal">
              <tabler-icon [icon]="'x'" [size]="18"></tabler-icon>
            </button>
          </div>
          <div class="modal-body">
            <ng-content></ng-content>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .modal-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(0,0,0,0.7);
      z-index: 1000;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: var(--space-lg);
      animation: fade-in 0.15s ease;
    }
    .modal-panel {
      background: var(--surface-800);
      border: 1px solid var(--border-default);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-lg);
      width: 100%;
      max-height: 90vh;
      overflow-y: auto;
      animation: slide-up 0.2s ease;
      &--sm { max-width: 400px; }
      &--md { max-width: 560px; }
      &--lg { max-width: 800px; }
    }
    .modal-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: var(--space-lg);
      border-bottom: 1px solid var(--border-subtle);
    }
    .modal-title {
      font-size: var(--text-lg);
      font-weight: 600;
      color: var(--text-primary);
      margin: 0;
    }
    .modal-close {
      background: transparent;
      border: none;
      color: var(--text-tertiary);
      cursor: pointer;
      padding: var(--space-xs);
      border-radius: var(--radius-sm);
      &:hover { color: var(--text-primary); background: var(--surface-700); }
    }
    .modal-body { padding: var(--space-lg); }
    @keyframes fade-in { from { opacity: 0; } to { opacity: 1; } }
    @keyframes slide-up { from { transform: translateY(20px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }
  `],
})
export class ModalComponent {
  @Input() title: string = '';
  @Input() open: boolean = false;
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Output() closed = new EventEmitter<void>();

  close(): void { this.closed.emit(); }
  onBackdropClick(e: MouseEvent): void { this.close(); }
}


