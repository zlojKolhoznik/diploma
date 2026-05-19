import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { TablerIconComponent } from '@tabler/icons-angular';
import { ReservationResponse } from '../../core/models/reservation.models';
import { RestaurantBrief } from '../../core/models/restaurant.models';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { ReservationsService } from '../../core/services/reservations.service';
import { RestaurantsService } from '../../core/services/restaurants.service';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-create-reservation',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, TablerIconComponent, SpinnerComponent],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <p class="eyebrow">Customer</p>
          <h2>Create Reservation</h2>
        </div>
        <a routerLink="/my/reservations" class="btn btn--ghost">My Reservations</a>
      </div>

      @if (loadingRestaurants()) {
        <app-spinner></app-spinner>
      } @else if (restaurants().length === 0) {
        <section class="empty-state" aria-live="polite">
          <tabler-icon [icon]="'building'" [size]="48"></tabler-icon>
          <h3>No restaurants available</h3>
          <p>Try again later when restaurants are published.</p>
        </section>
      } @else {
        <form class="reservation-form" [formGroup]="form" (ngSubmit)="submit()">
          <label class="field field--wide">
            <span>Restaurant</span>
            <select formControlName="restaurantId" aria-label="Restaurant">
              <option value="">Select a restaurant</option>
              @for (restaurant of restaurants(); track restaurant.id) {
                <option [value]="restaurant.id">{{ restaurantLabel(restaurant) }}</option>
              }
            </select>
          </label>

          <div class="field-grid">
            <label class="field">
              <span>Date</span>
              <input formControlName="startDate" type="date" aria-label="Date" />
            </label>

            <label class="field">
              <span>Time</span>
              <input formControlName="startTime" type="time" step="900" aria-label="Time" />
            </label>

            <label class="field">
              <span>Number of guests</span>
              <input formControlName="numberOfGuests" type="number" min="1" max="20" aria-label="Number of guests" />
            </label>

            <label class="field">
              <span>Duration (minutes)</span>
              <input formControlName="approximateDurationMinutes" type="number" min="30" step="15" aria-label="Duration in minutes" />
            </label>
          </div>

          <label class="field field--wide">
            <span>Guest name (optional)</span>
            <input formControlName="guestName" type="text" maxlength="80" placeholder="Name for host stand" aria-label="Guest name" />
          </label>

          @if (errorMessage()) {
            <p class="error-note" role="alert">{{ errorMessage() }}</p>
          }

          <div class="form-actions">
            <p class="form-note">Reservations can be cancelled later from My Reservations.</p>
            <button class="btn btn--primary" type="submit" [disabled]="form.invalid || submitting()">
              @if (submitting()) {
                Creating...
              } @else {
                Create reservation
              }
            </button>
          </div>
        </form>
      }
    </div>
  `,
  styles: [`
    .page-container { max-width: 860px; margin: 0 auto; padding: var(--space-xl) var(--space-lg); }
    .page-header { display: flex; justify-content: space-between; align-items: flex-end; gap: var(--space-md); margin-bottom: var(--space-xl); }
    .eyebrow { margin: 0 0 6px; font-size: var(--text-xs); letter-spacing: 0.1em; text-transform: uppercase; color: var(--text-tertiary); }
    h2 { margin: 0; }
    .reservation-form {
      background: var(--surface-800);
      border: 1px solid var(--border-default);
      border-radius: var(--radius-lg);
      padding: var(--space-xl);
      display: flex;
      flex-direction: column;
      gap: var(--space-md);
    }
    .field-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: var(--space-md); }
    .field { display: flex; flex-direction: column; gap: var(--space-xs); }
    .field--wide { grid-column: 1 / -1; }
    .field span { color: var(--text-secondary); font-size: var(--text-sm); font-weight: 600; }
    .field input, .field select {
      background: var(--surface-700);
      border: 1px solid var(--border-default);
      border-radius: var(--radius-md);
      color: var(--text-primary);
      padding: var(--space-sm) var(--space-md);
      font-size: var(--text-sm);
      outline: none;
    }
    .field input:focus, .field select:focus { border-color: var(--accent-primary); box-shadow: 0 0 0 3px rgba(255,107,53,0.15); }
    .error-note { margin: 0; color: var(--state-error); font-size: var(--text-sm); }
    .form-actions { display: flex; justify-content: space-between; align-items: center; gap: var(--space-md); }
    .form-note { margin: 0; color: var(--text-secondary); font-size: var(--text-sm); }
    .btn {
      border: none;
      border-radius: var(--radius-md);
      cursor: pointer;
      text-decoration: none;
      padding: var(--space-sm) var(--space-md);
      font-size: var(--text-sm);
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-height: 38px;
    }
    .btn--primary { background: var(--accent-primary); color: white; font-weight: 600; }
    .btn--primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn--ghost { background: var(--surface-700); border: 1px solid var(--border-default); color: var(--text-secondary); }
    .empty-state { text-align: center; color: var(--text-tertiary); padding: var(--space-2xl); }
    .empty-state h3 { margin-bottom: var(--space-xs); color: var(--text-primary); }
    .empty-state p { margin-top: 0; }
    @media (max-width: 768px) {
      .page-header, .form-actions { flex-direction: column; align-items: stretch; }
      .field-grid { grid-template-columns: 1fr; }
    }
  `],
})
export class CreateReservationComponent {
  private readonly reservationsService = inject(ReservationsService);
  private readonly restaurantsService = inject(RestaurantsService);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  readonly loadingRestaurants = signal(true);
  readonly submitting = signal(false);
  readonly restaurants = signal<RestaurantBrief[]>([]);
  readonly errorMessage = signal('');

  readonly defaultGuestName = computed(() => {
    const user = this.auth.currentUser();
    if (!user) return '';
    const fullName = `${user.given_name ?? ''} ${user.family_name ?? ''}`.trim();
    return fullName || user.email || '';
  });

  readonly form = this.fb.nonNullable.group({
    restaurantId: ['', [Validators.required]],
    startDate: [this.getTodayDateValue(), [Validators.required]],
    startTime: ['', [Validators.required]],
    numberOfGuests: [2, [Validators.required, Validators.min(1), Validators.max(20)]],
    approximateDurationMinutes: [90, [Validators.required, Validators.min(30)]],
    guestName: [''],
  });

  constructor() {
    this.loadRestaurants();
  }

  restaurantLabel(restaurant: RestaurantBrief): string {
    const city = restaurant.city?.trim();
    const address = restaurant.address?.trim();
    if (city && address) return `${city} - ${address}`;
    return city || address || restaurant.id;
  }

  submit(): void {
    this.errorMessage.set('');

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.errorMessage.set('Please fill out all required fields.');
      return;
    }

    const value = this.form.getRawValue();
    const startDateTime = new Date(`${value.startDate}T${value.startTime}`);

    if (Number.isNaN(startDateTime.getTime())) {
      this.errorMessage.set('Please provide a valid reservation date and time.');
      return;
    }

    if (startDateTime <= new Date()) {
      this.errorMessage.set('Reservation time must be in the future.');
      return;
    }

    this.submitting.set(true);
    this.reservationsService.create({
      restaurantId: value.restaurantId,
      guestId: this.auth.currentUser()?.sub,
      guestName: value.guestName.trim() || this.defaultGuestName() || undefined,
      startTime: startDateTime.toISOString(),
      approximateDurationMinutes: value.approximateDurationMinutes,
      numberOfGuests: value.numberOfGuests,
    }).pipe(
      finalize(() => this.submitting.set(false))
    ).subscribe({
      next: (reservation: ReservationResponse) => {
        this.notifications.success('Reservation created successfully.');
        this.router.navigate(['/my/reservations'], { queryParams: { created: reservation.id } });
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.detail ?? err?.error?.message ?? 'Could not create reservation.');
      },
    });
  }

  private loadRestaurants(): void {
    this.loadingRestaurants.set(true);

    this.restaurantsService.getAll().pipe(
      finalize(() => this.loadingRestaurants.set(false))
    ).subscribe({
      next: restaurants => {
        this.restaurants.set(restaurants);

        const restaurantId = this.route.snapshot.queryParamMap.get('restaurantId');
        if (restaurantId && restaurants.some(r => r.id === restaurantId)) {
          this.form.controls.restaurantId.setValue(restaurantId);
        }
      },
      error: () => {
        this.errorMessage.set('Failed to load restaurants. Please try again.');
      },
    });
  }

  private getTodayDateValue(): string {
    const now = new Date();
    return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
  }
}
