import { Component } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { TablerIconComponent } from '@tabler/icons-angular';

const ADMIN_LINKS = [
  { path: '/admin/restaurants', icon: 'building', label: 'Restaurants' },
  { path: '/admin/dishes', icon: 'utensils-fork', label: 'Dishes' },
  { path: '/admin/tables', icon: 'table', label: 'Tables' },
  { path: '/admin/waiters', icon: 'users', label: 'Waiters' },
  { path: '/admin/schedules', icon: 'clock', label: 'Schedules' },
  { path: '/admin/analytics', icon: 'chart-bar', label: 'Analytics' },
  { path: '/admin/users', icon: 'shield', label: 'Users' },
];

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterModule, RouterOutlet, TablerIconComponent],
  template: `
    <div class="admin-layout">
      <nav class="admin-sidebar" aria-label="Admin navigation">
        <div class="admin-sidebar__title">Admin Panel</div>
        <ul>
          @for (link of links; track link.path) {
            <li>
              <a [routerLink]="link.path" routerLinkActive="active">
                <tabler-icon [icon]="link.icon" [size]="18"></tabler-icon>
                {{ link.label }}
              </a>
            </li>
          }
        </ul>
      </nav>
      <main class="admin-content">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styles: [`
    .admin-layout { display: flex; min-height: calc(100vh - 124px); }
    .admin-sidebar {
      width: 240px; flex-shrink: 0; background: var(--surface-800);
      border-right: 1px solid var(--border-default); padding: var(--space-lg);
    }
    .admin-sidebar__title {
      font-size: var(--text-xs); font-weight: 700; text-transform: uppercase; letter-spacing: 0.1em;
      color: var(--text-tertiary); margin-bottom: var(--space-lg); padding: 0 var(--space-md);
    }
    ul { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: var(--space-xs); }
    a {
      display: flex; align-items: center; gap: var(--space-sm);
      color: var(--text-secondary); text-decoration: none; font-size: var(--text-sm);
      padding: var(--space-sm) var(--space-md); border-radius: var(--radius-md);
      &:hover { background: var(--surface-700); color: var(--text-primary); text-decoration: none; }
      &.active { background: rgba(255,107,53,0.1); color: var(--accent-primary); }
    }
    .admin-content { flex: 1; overflow-x: auto; }
    @media (max-width: 900px) {
      .admin-layout { flex-direction: column; }
      .admin-sidebar { width: 100%; border-right: none; border-bottom: 1px solid var(--border-default); }
      ul { flex-direction: row; flex-wrap: wrap; }
    }
  `],
})
export class AdminLayoutComponent {
  readonly links = ADMIN_LINKS;
}


