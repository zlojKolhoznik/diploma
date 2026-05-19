import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `<span class="badge" [class]="badgeClass()">{{ status }}</span>`,
  styles: [`
    .badge {
      display: inline-block;
      padding: 2px var(--space-sm);
      border-radius: var(--radius-full);
      font-size: var(--text-xs);
      font-weight: 600;
      text-transform: capitalize;
    }
    .badge--pending { background-color: rgba(255,193,7,0.15); color: var(--state-warning); }
    .badge--confirmed { background-color: rgba(40,167,69,0.15); color: var(--state-success); }
    .badge--cancelled { background-color: rgba(220,53,69,0.15); color: var(--state-error); }
    .badge--completed { background-color: rgba(23,162,184,0.15); color: var(--state-info); }
    .badge--seated { background-color: rgba(40,167,69,0.15); color: var(--state-success); }
    .badge--in-progress { background-color: rgba(255,193,7,0.15); color: var(--state-warning); }
    .badge--ready { background-color: rgba(23,162,184,0.15); color: var(--state-info); }
    .badge--delivered { background-color: rgba(40,167,69,0.15); color: var(--state-success); }
    .badge--default { background-color: var(--surface-700); color: var(--text-secondary); }
  `],
})
export class StatusBadgeComponent {
  @Input() status: string = '';
  @Input() type: 'reservation' | 'order' = 'reservation';

  badgeClass(): string {
    const key = this.status?.toLowerCase().replace(/\s+/g, '-') ?? 'default';
    const known = ['pending', 'confirmed', 'cancelled', 'completed', 'seated', 'in-progress', 'ready', 'delivered'];
    return 'badge--' + (known.includes(key) ? key : 'default');
  }
}

