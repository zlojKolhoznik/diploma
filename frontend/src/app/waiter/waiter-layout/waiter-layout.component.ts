import { Component } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { TablerIconComponent } from '@tabler/icons-angular';

@Component({
  selector: 'app-waiter-layout',
  standalone: true,
  imports: [RouterModule, RouterOutlet, TablerIconComponent],
  template: `
    <div class="waiter-layout">
      <nav class="sidebar" aria-label="Waiter navigation">
        <ul>
          <li>
            <a routerLink="/waiter/reservations" routerLinkActive="active">
              <tabler-icon [icon]="'calendar'" [size]="18"></tabler-icon> Reservations
            </a>
          </li>
          <li>
            <a routerLink="/waiter/orders" routerLinkActive="active">
              <tabler-icon [icon]="'utensils-fork'" [size]="18"></tabler-icon> Orders
            </a>
          </li>
          <li>
            <a routerLink="/waiter/schedule" routerLinkActive="active">
              <tabler-icon [icon]="'clock'" [size]="18"></tabler-icon> My Schedule
            </a>
          </li>
        </ul>
      </nav>
      <main class="sidebar-content">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styles: [`
    .waiter-layout { display: flex; min-height: calc(100vh - 124px); }
    .sidebar {
      width: 220px; flex-shrink: 0; background: var(--surface-800);
      border-right: 1px solid var(--border-default); padding: var(--space-lg);
      ul { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: var(--space-xs); }
      a {
        display: flex; align-items: center; gap: var(--space-sm);
        color: var(--text-secondary); text-decoration: none; font-size: var(--text-sm);
        padding: var(--space-sm) var(--space-md); border-radius: var(--radius-md);
        &:hover { background: var(--surface-700); color: var(--text-primary); }
        &.active { background: rgba(255,107,53,0.1); color: var(--accent-primary); }
      }
    }
    .sidebar-content { flex: 1; overflow-x: auto; }
    @media (max-width: 768px) {
      .waiter-layout { flex-direction: column; }
      .sidebar { width: 100%; border-right: none; border-bottom: 1px solid var(--border-default); ul { flex-direction: row; flex-wrap: wrap; } }
    }
  `],
})
export class WaiterLayoutComponent {}


