import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TablerIconComponent } from '@tabler/icons-angular';
import { AuthService } from '../../core/services/auth.service';
import { LogoComponent } from '../../shared/components/logo/logo.component';

interface NavLink {
  label: string;
  path: string;
  roles?: string[]; // if undefined, shown to all
  guestOnly?: boolean;
  authRequired?: boolean;
}

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule, TablerIconComponent, LogoComponent],
  template: `
    <nav class="navbar">
      <div class="navbar-container">
        <!-- Logo -->
        <a routerLink="/" class="navbar-logo">
          <app-logo size="sm"></app-logo>
        </a>

        <!-- Desktop links -->
        <ul class="navbar-links" [class.open]="menuOpen()">
          @for (link of visibleLinks(); track link.path) {
            <li>
              <a [routerLink]="link.path" routerLinkActive="active" [routerLinkActiveOptions]="{exact: link.path === '/'}">
                {{ link.label }}
              </a>
            </li>
          }
        </ul>

        <!-- Right side -->
        <div class="navbar-actions">
          @if (auth.isLoggedIn()) {
            <span class="navbar-user">
              {{ userDisplayName() }}
            </span>
            <button class="navbar-btn navbar-btn--ghost" (click)="logout()" aria-label="Logout">
              <tabler-icon [icon]="'logout'" [size]="18"></tabler-icon>
              <span class="btn-label">Logout</span>
            </button>
          } @else {
            <a routerLink="/login" class="navbar-btn navbar-btn--ghost">Login</a>
            <a routerLink="/register" class="navbar-btn navbar-btn--primary">Sign Up</a>
          }

          <!-- Hamburger -->
          <button class="navbar-hamburger" (click)="toggleMenu()" aria-label="Toggle menu">
            <tabler-icon [icon]="'menu-2'" [size]="24"></tabler-icon>
          </button>
        </div>
      </div>
    </nav>
  `,
  styleUrls: ['./navbar.component.scss'],
})
export class NavbarComponent {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly menuOpen = signal(false);

  private readonly allLinks: NavLink[] = [
    { label: 'Home', path: '/' },
    { label: 'Restaurants', path: '/restaurants' },
    { label: 'My Reservations', path: '/my/reservations', authRequired: true, roles: ['Customer', 'Admin', 'Waiter'] },
    { label: 'My Orders', path: '/my/orders', authRequired: true, roles: ['Customer', 'Admin', 'Waiter'] },
    { label: 'Profile', path: '/profile', authRequired: true },
    { label: 'Dashboard', path: '/waiter', roles: ['Waiter', 'Admin'] },
    { label: 'Admin', path: '/admin', roles: ['Admin'] },
  ];

  readonly visibleLinks = () => {
    return this.allLinks.filter(link => {
      if (link.authRequired && !this.auth.isLoggedIn()) return false;
      if (link.roles && link.roles.length > 0) {
        return link.roles.some(r => this.auth.hasRole(r));
      }
      return true;
    });
  };

  userDisplayName(): string {
    const payload = this.auth.currentUser();
    if (!payload) return '';
    return payload.given_name ?? payload.email ?? '';
  }

  toggleMenu(): void {
    this.menuOpen.update(v => !v);
  }

  logout(): void {
    this.auth.logout();
  }
}


