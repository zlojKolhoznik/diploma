import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TablerIconComponent } from '@tabler/icons-angular';
import {
  AdminsService,
  AdminUserListItem,
  AdminUserRole,
} from '../../core/services/admins.service';
import { AuthService } from '../../core/services/auth.service';
import { RestaurantsService } from '../../core/services/restaurants.service';
import { RestaurantBrief } from '../../core/models/restaurant.models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { NotificationService } from '../../core/services/notification.service';
import { RegisterRequest } from '../../core/models/auth.models';
import { finalize, switchMap } from 'rxjs/operators';
import {
  isEmailValid,
  isNameValid,
  isPasswordStrong,
  isPasswordsMatch,
} from '../../auth/validators/auth.validators';

type CreatedUserRole = 'admin' | 'waiter';
type UserTab = 'admins' | 'waiters' | 'customers';

const TAB_CONFIG: ReadonlyArray<{ key: UserTab; label: string; role: AdminUserRole }> = [
  { key: 'admins', label: 'Admins', role: 'Admin' },
  { key: 'waiters', label: 'Waiters', role: 'Waiter' },
  { key: 'customers', label: 'Customers', role: 'Customer' },
];

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule, TablerIconComponent, SpinnerComponent],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <p class="eyebrow">Administration</p>
          <h2>User Management</h2>
        </div>
      </div>

      <section class="panel panel--create">
        <div class="panel__header">
          <div>
            <p class="panel__eyebrow">Create account</p>
            <h3>Create restaurant admin or waiter</h3>
          </div>
          <p class="panel__description">
            Create a new account, then immediately assign the right role so the user can sign in with the correct permissions.
          </p>
        </div>

        <form class="create-form" (ngSubmit)="createAccount()">
          <div class="field-grid">
            <label class="field">
              <span>First name</span>
              <input [(ngModel)]="form.firstName" name="firstName" type="text" autocomplete="given-name" />
            </label>
            <label class="field">
              <span>Last name</span>
              <input [(ngModel)]="form.lastName" name="lastName" type="text" autocomplete="family-name" />
            </label>
            <label class="field field--wide">
              <span>Email</span>
              <input [(ngModel)]="form.email" name="email" type="email" autocomplete="email" />
            </label>
            <label class="field">
              <span>Password</span>
              <input [(ngModel)]="form.password" name="password" type="password" autocomplete="new-password" />
            </label>
            <label class="field">
              <span>Confirm password</span>
              <input [(ngModel)]="form.confirmPassword" name="confirmPassword" type="password" autocomplete="new-password" />
            </label>
            <label class="field field--wide">
              <span>Account type</span>
              <select [(ngModel)]="form.role" name="role">
                <option value="admin">Restaurant admin</option>
                <option value="waiter">Waiter</option>
              </select>
            </label>
            <label class="field field--wide">
              <span>Restaurant</span>
              <select [(ngModel)]="form.restaurantId" name="restaurantId">
                <option value="">No restaurant assignment</option>
                @for (restaurant of restaurants(); track restaurant.id) {
                  <option [value]="restaurant.id">
                    {{ restaurant.city || 'Restaurant' }} - {{ restaurant.address || restaurant.id }}
                  </option>
                }
              </select>
            </label>
          </div>

          <div class="form-actions">
            <div class="form-note">
              <span class="form-note__label">Requirements:</span>
              First and last name must be at least 2 characters, password must be strong, and waiter accounts need a restaurant selected.
            </div>
            <button class="action-btn action-btn--primary" type="submit" [disabled]="saving()">
              @if (saving()) {
                Creating...
              } @else {
                Create account
              }
            </button>
          </div>
        </form>
      </section>

      @if (errorMessage()) {
        <p class="error-note">{{ errorMessage() }}</p>
      }

      @if (loading()) { <app-spinner></app-spinner> }
      @else {
        <div class="table-wrapper panel">
          <div class="panel__header panel__header--compact">
            <div>
              <p class="panel__eyebrow">Existing users</p>
              <h3>Users by role</h3>
            </div>
          </div>

          <div class="tabs" role="tablist" aria-label="User role filters">
            @for (tab of tabs; track tab.key) {
              <button
                type="button"
                role="tab"
                class="tab-btn"
                [class.active]="activeTab() === tab.key"
                [attr.aria-selected]="activeTab() === tab.key"
                (click)="changeTab(tab.key)">
                {{ tab.label }}
              </button>
            }
          </div>

          <table class="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                @if (showRestaurantColumn()) {
                  <th>Restaurant address</th>
                }
                <th>Role</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (u of users(); track u.userId) {
                <tr>
                  <td>{{ u.firstName }} {{ u.lastName }}</td>
                  <td>{{ u.email }}</td>
                  @if (showRestaurantColumn()) {
                    <td>{{ u.restaurantAddress || '-' }}</td>
                  }
                  <td><span class="role-badge">{{ u.role }}</span></td>
                  <td class="actions">
                    @if (u.role !== 'Admin') {
                      <button class="action-btn" (click)="makeAdmin(u)" aria-label="Appoint as admin">
                        <tabler-icon [icon]="'shield'" [size]="14"></tabler-icon> Appoint Admin
                      </button>
                    } @else {
                      <span class="empty-action">-</span>
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>

          <div class="pagination">
            <button class="action-btn" type="button" (click)="previousPage()" [disabled]="page() <= 1">Previous</button>
            <span class="pagination__summary">
              Page {{ page() }} of {{ totalPages() || 1 }}
              <span class="pagination__meta">({{ totalCount() }} users)</span>
            </span>
            <button class="action-btn" type="button" (click)="nextPage()" [disabled]="page() >= totalPages() || totalPages() === 0">Next</button>
          </div>

          @if (users().length === 0) { <p class="empty-note">No users found.</p> }
        </div>
      }
    </div>
  `,
  styles: [`
    .page-container { padding: var(--space-xl) var(--space-lg); max-width: 1120px; }
    .page-header { display: flex; align-items: flex-end; justify-content: space-between; margin-bottom: var(--space-lg); }
    .eyebrow, .panel__eyebrow { margin: 0 0 6px; font-size: var(--text-xs); font-weight: 700; text-transform: uppercase; letter-spacing: 0.12em; color: var(--text-tertiary); }
    h2, h3 { margin: 0; }
    h2 { font-size: 1.9rem; }
    .panel { background: var(--surface-800); border: 1px solid var(--border-default); border-radius: var(--radius-lg); padding: var(--space-lg); margin-bottom: var(--space-xl); box-shadow: 0 10px 30px rgba(0,0,0,0.08); }
    .panel--create { background: linear-gradient(180deg, rgba(255,107,53,0.08), rgba(255,255,255,0)); }
    .panel__header { display: flex; justify-content: space-between; gap: var(--space-lg); align-items: flex-start; margin-bottom: var(--space-lg); }
    .panel__header--compact { margin-bottom: var(--space-md); }
    .panel__description { margin: 0; max-width: 620px; color: var(--text-secondary); line-height: 1.5; }
    .create-form { display: flex; flex-direction: column; gap: var(--space-lg); }
    .field-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: var(--space-md); }
    .field { display: flex; flex-direction: column; gap: var(--space-xs); }
    .field--wide { grid-column: 1 / -1; }
    .field span { font-size: var(--text-sm); font-weight: 600; color: var(--text-secondary); }
    .field input, .field select {
      width: 100%; border: 1px solid var(--border-default); border-radius: var(--radius-md);
      background: var(--surface-700); color: var(--text-primary); padding: var(--space-sm) var(--space-md);
      font-size: var(--text-sm); outline: none;
      &:focus { border-color: var(--accent-primary); box-shadow: 0 0 0 3px rgba(255,107,53,0.15); }
    }
    .form-actions { display: flex; justify-content: space-between; gap: var(--space-md); align-items: center; }
    .form-note { flex: 1; color: var(--text-secondary); font-size: var(--text-sm); line-height: 1.5; }
    .form-note__label { font-weight: 700; color: var(--text-primary); }
    .error-note { margin: 0 0 var(--space-lg); color: var(--state-error); font-size: var(--text-sm); }
    .table-wrapper { overflow-x: auto; }
    .tabs { display: flex; gap: var(--space-xs); margin-bottom: var(--space-md); flex-wrap: wrap; }
    .tab-btn {
      border: 1px solid var(--border-default); background: var(--surface-700); color: var(--text-secondary);
      border-radius: var(--radius-md); padding: var(--space-xs) var(--space-md); font-size: var(--text-sm);
      cursor: pointer;
      &:hover { background: var(--surface-600); color: var(--text-primary); }
      &.active { border-color: transparent; background: var(--accent-primary); color: white; }
    }
    .data-table { width: 100%; border-collapse: collapse; font-size: var(--text-sm); }
    .data-table th, .data-table td { padding: var(--space-md); text-align: left; border-bottom: 1px solid var(--border-subtle); }
    .data-table th { color: var(--text-tertiary); font-weight: 600; }
    .data-table tr:hover td { background: var(--surface-700); }
    .role-badge { background: rgba(255,107,53,0.1); color: var(--accent-primary); font-size: var(--text-xs); font-weight: 600; padding: 2px var(--space-sm); border-radius: var(--radius-full); }
    .actions { white-space: nowrap; }
    .empty-action { color: var(--text-tertiary); font-size: var(--text-xs); }
    .action-btn {
      display: inline-flex; align-items: center; justify-content: center; gap: 4px; background: var(--surface-700);
      border: 1px solid var(--border-default); color: var(--text-secondary); padding: var(--space-xs) var(--space-sm);
      border-radius: var(--radius-sm); font-size: var(--text-xs); cursor: pointer; text-decoration: none;
      &:hover { background: var(--surface-600); color: var(--text-primary); }
      &:disabled { opacity: 0.55; cursor: not-allowed; }
    }
    .action-btn--primary { background: var(--accent-primary); border-color: transparent; color: white; padding: var(--space-sm) var(--space-lg); font-size: var(--text-sm); font-weight: 600; &:hover { background: var(--accent-primary-hover); color: white; } }
    .pagination { margin-top: var(--space-md); display: flex; align-items: center; justify-content: flex-end; gap: var(--space-md); }
    .pagination__summary { color: var(--text-secondary); font-size: var(--text-sm); }
    .pagination__meta { color: var(--text-tertiary); }
    .empty-note { color: var(--text-secondary); padding: var(--space-lg) 0; }
    @media (max-width: 900px) {
      .page-header, .panel__header, .form-actions { flex-direction: column; align-items: stretch; }
      .field-grid { grid-template-columns: 1fr; }
      .field--wide { grid-column: auto; }
      .pagination { justify-content: space-between; }
    }
  `],
})
export class AdminUsersComponent implements OnInit {
  private readonly service = inject(AdminsService);
  private readonly authService = inject(AuthService);
  private readonly restaurantsService = inject(RestaurantsService);
  private readonly notifications = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly users = signal<AdminUserListItem[]>([]);
  readonly restaurants = signal<RestaurantBrief[]>([]);
  readonly errorMessage = signal('');
  readonly activeTab = signal<UserTab>('admins');
  readonly page = signal(1);
  readonly pageSize = signal(10);
  readonly totalPages = signal(0);
  readonly totalCount = signal(0);
  readonly tabs = TAB_CONFIG;

  form: {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    confirmPassword: string;
    role: CreatedUserRole;
    restaurantId: string;
  } = {
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
    role: 'admin',
    restaurantId: '',
  };

  ngOnInit(): void {
    this.loadUsers();

    this.restaurantsService.getAll().subscribe({
      next: res => this.restaurants.set(res),
      error: () => this.notifications.error('Failed to load restaurants for the create form.'),
    });
  }

  makeAdmin(u: AdminUserListItem): void {
    this.service.appointAdmin({ adminUserIdToAppoint: u.userId }).subscribe({
      next: () => {
        this.notifications.success(`${u.email} appointed as Admin.`);
        this.loadUsers();
      },
      error: () => this.notifications.error('Failed to appoint admin.'),
    });
  }

  createAccount(): void {
    this.errorMessage.set('');

    if (!isNameValid(this.form.firstName)) {
      this.errorMessage.set('First name must be at least 2 characters.');
      return;
    }

    if (!isNameValid(this.form.lastName)) {
      this.errorMessage.set('Last name must be at least 2 characters.');
      return;
    }

    if (!isEmailValid(this.form.email)) {
      this.errorMessage.set('Please enter a valid email address.');
      return;
    }

    if (!isPasswordStrong(this.form.password)) {
      this.errorMessage.set('Password does not meet the strength requirements.');
      return;
    }

    if (!isPasswordsMatch(this.form.password, this.form.confirmPassword)) {
      this.errorMessage.set('Passwords do not match.');
      return;
    }

    if (this.form.role === 'waiter' && !this.form.restaurantId) {
      this.errorMessage.set('Waiter accounts require a restaurant assignment.');
      return;
    }

    this.saving.set(true);
    const account: RegisterRequest = {
      email: this.form.email.trim(),
      password: this.form.password,
      firstName: this.form.firstName.trim(),
      lastName: this.form.lastName.trim(),
    };

    this.authService.register(account).pipe(
      switchMap(() => this.service.getUsers('Customer', 1, 200)),
      switchMap(response => {
        const createdUser = response.items.find(user => user.email.toLowerCase() === account.email.toLowerCase());

        if (!createdUser) {
          throw new Error('Created user was not found after registration.');
        }

        return this.form.role === 'admin'
          ? this.service.appointAdmin({ adminUserIdToAppoint: createdUser.userId, restaurantId: this.form.restaurantId || null })
          : this.service.assignWaiterRole(createdUser.userId, this.form.restaurantId);
      }),
      finalize(() => this.saving.set(false)),
    ).subscribe({
      next: () => {
        this.notifications.success(
          this.form.role === 'admin' ? 'Restaurant admin account created.' : 'Waiter account created.',
        );
        this.form = {
          firstName: '',
          lastName: '',
          email: '',
          password: '',
          confirmPassword: '',
          role: 'admin',
          restaurantId: '',
        };
        this.page.set(1);
        this.loadUsers();
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.detail ?? err?.error?.message ?? 'Failed to create account.');
      },
    });
  }

  changeTab(tab: UserTab): void {
    if (this.activeTab() === tab) {
      return;
    }

    this.activeTab.set(tab);
    this.page.set(1);
    this.loadUsers();
  }

  previousPage(): void {
    if (this.page() <= 1) {
      return;
    }

    this.page.update(value => value - 1);
    this.loadUsers();
  }

  nextPage(): void {
    if (this.totalPages() === 0 || this.page() >= this.totalPages()) {
      return;
    }

    this.page.update(value => value + 1);
    this.loadUsers();
  }

  showRestaurantColumn(): boolean {
    return this.activeTab() !== 'customers';
  }

  private loadUsers(): void {
    this.loading.set(true);
    const role = TAB_CONFIG.find(tab => tab.key === this.activeTab())?.role ?? 'Admin';
    this.service.getUsers(role, this.page(), this.pageSize()).subscribe({
      next: response => {
        this.users.set(response.items);
        this.totalPages.set(response.totalPages);
        this.totalCount.set(response.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.notifications.error('Failed to load users.');
      },
    });
  }
}


