import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `<div class="spinner" role="status" aria-label="Loading"><div class="spinner-ring"></div></div>`,
  styles: [`
    .spinner {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: var(--space-xl);
    }
    .spinner-ring {
      width: 36px;
      height: 36px;
      border: 3px solid var(--surface-600);
      border-top-color: var(--brand-ember);
      border-radius: 50%;
      animation: spin 0.7s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
  `],
})
export class SpinnerComponent {}

