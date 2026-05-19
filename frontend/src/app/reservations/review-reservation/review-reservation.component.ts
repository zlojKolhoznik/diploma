import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { ReservationsService } from '../../core/services/reservations.service';
import { ReviewsService } from '../../core/services/reviews.service';
import { NotificationService } from '../../core/services/notification.service';
import { ReservationResponse } from '../../core/models/reservation.models';
import { StarRatingComponent } from '../../shared/components/star-rating/star-rating.component';
import { ModerationResultComponent } from '../../shared/components/moderation-result/moderation-result.component';
import { ReviewModerationResult } from '../../core/models/review.models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-review-reservation',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    StarRatingComponent,
    ModerationResultComponent,
    SpinnerComponent,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h2>Leave a Review</h2>
        <a routerLink="/my/reservations" class="btn btn--ghost">Back to My Reservations</a>
      </div>

      @if (loading()) {
        <app-spinner></app-spinner>
      } @else if (!reservation()) {
        <p class="error-note">Reservation not found.</p>
      } @else if (!canReview()) {
        <p class="error-note">You can only review completed past reservations.</p>
      } @else {
        <div class="review-form">
          <p class="meta">Reservation on {{ reservation()!.startTime | date:'medium' }}</p>

          <label class="field">
            <span>Cuisine rating</span>
            <app-star-rating [value]="cuisineRating()" (valueChange)="cuisineRating.set($event)"></app-star-rating>
          </label>

          <label class="field">
            <span>Cuisine comment</span>
            <textarea [(ngModel)]="cuisineComment" rows="3" maxlength="500"></textarea>
          </label>

          <label class="field">
            <span>Service rating</span>
            <app-star-rating [value]="serviceRating()" (valueChange)="serviceRating.set($event)"></app-star-rating>
          </label>

          <label class="field">
            <span>Service comment</span>
            <textarea [(ngModel)]="serviceComment" rows="3" maxlength="500"></textarea>
          </label>

          @if (errorMessage()) {
            <p class="error-note" role="alert">{{ errorMessage() }}</p>
          }

          <app-moderation-result [result]="moderationResult()"></app-moderation-result>

          <div class="actions">
            <button class="btn btn--primary" type="button" [disabled]="submitting()" (click)="submit()">
              {{ submitting() ? 'Submitting...' : 'Submit review' }}
            </button>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .page-container { max-width: 780px; margin: 0 auto; padding: var(--space-xl) var(--space-lg); }
    .page-header { display: flex; justify-content: space-between; align-items: center; gap: var(--space-md); margin-bottom: var(--space-xl); }
    h2 { margin: 0; }
    .review-form {
      background: var(--surface-800);
      border: 1px solid var(--border-default);
      border-radius: var(--radius-lg);
      padding: var(--space-lg);
      display: flex;
      flex-direction: column;
      gap: var(--space-md);
    }
    .meta { margin: 0; color: var(--text-secondary); font-size: var(--text-sm); }
    .field { display: flex; flex-direction: column; gap: var(--space-xs); }
    .field span { color: var(--text-secondary); font-size: var(--text-sm); font-weight: 600; }
    textarea {
      resize: vertical;
      min-height: 84px;
      border: 1px solid var(--border-default);
      border-radius: var(--radius-md);
      background: var(--surface-700);
      color: var(--text-primary);
      padding: var(--space-sm) var(--space-md);
      font-size: var(--text-sm);
      outline: none;
    }
    textarea:focus { border-color: var(--accent-primary); box-shadow: 0 0 0 3px rgba(255,107,53,0.15); }
    .actions { display: flex; justify-content: flex-end; }
    .btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      border: none;
      border-radius: var(--radius-md);
      cursor: pointer;
      text-decoration: none;
      padding: var(--space-sm) var(--space-md);
      font-size: var(--text-sm);
      min-height: 38px;
    }
    .btn--primary { background: var(--accent-primary); color: white; font-weight: 600; }
    .btn--primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn--ghost { background: var(--surface-700); border: 1px solid var(--border-default); color: var(--text-secondary); }
    .error-note { margin: 0; color: var(--state-error); font-size: var(--text-sm); }
    @media (max-width: 768px) {
      .page-header { flex-direction: column; align-items: stretch; }
    }
  `],
})
export class ReviewReservationComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly reservationsService = inject(ReservationsService);
  private readonly reviewsService = inject(ReviewsService);
  private readonly notifications = inject(NotificationService);

  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly reservation = signal<ReservationResponse | null>(null);
  readonly moderationResult = signal<ReviewModerationResult | null>(null);
  readonly errorMessage = signal('');

  readonly cuisineRating = signal(5);
  readonly serviceRating = signal(5);

  cuisineComment = '';
  serviceComment = '';

  constructor() {
    const reservationId = this.route.snapshot.paramMap.get('id');
    if (!reservationId) {
      this.errorMessage.set('Reservation id is missing.');
      this.loading.set(false);
      return;
    }

    this.reservationsService.getMy().subscribe({
      next: reservations => {
        const targetReservation = reservations.find(r => r.id === reservationId) ?? null;
        this.reservation.set(targetReservation);
        if (!targetReservation) {
          this.errorMessage.set('Reservation not found in your account.');
        }
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not load your reservations.');
        this.loading.set(false);
      },
    });
  }

  canReview(): boolean {
    const reservation = this.reservation();
    if (!reservation) return false;

    const isPast = new Date(reservation.startTime) < new Date();
    const isCancelled = reservation.status?.toLowerCase() === 'cancelled';
    return isPast && !isCancelled;
  }

  submit(): void {
    const reservation = this.reservation();
    if (!reservation) {
      this.errorMessage.set('Reservation not found.');
      return;
    }

    if (!this.canReview()) {
      this.errorMessage.set('You can only review completed past reservations.');
      return;
    }

    this.errorMessage.set('');
    this.submitting.set(true);

    const payload = {
      cuisineRating: this.cuisineRating(),
      cuisineComment: this.cuisineComment.trim() || null,
      serviceRating: this.serviceRating(),
      serviceComment: this.serviceComment.trim() || null,
    };

    this.reviewsService.moderate(reservation.restaurantId, reservation.id, payload).pipe(
      finalize(() => this.submitting.set(false))
    ).subscribe({
      next: moderation => {
        this.moderationResult.set(moderation);
        if (!moderation.approved) {
          this.errorMessage.set('Review needs revision before submission.');
          return;
        }

        this.submitReview(reservation.restaurantId, reservation.id, payload);
      },
      error: err => {
        // Some backend versions do not expose a dedicated moderation endpoint.
        if (err?.status === 404 || err?.status === 405) {
          this.submitReview(reservation.restaurantId, reservation.id, payload);
          return;
        }

        this.errorMessage.set(err?.error?.detail ?? err?.error?.message ?? 'Failed to moderate review.');
      },
    });
  }

  private submitReview(
    restaurantId: string,
    reservationId: string,
    payload: {
      cuisineRating: number;
      cuisineComment: string | null;
      serviceRating: number;
      serviceComment: string | null;
    }
  ): void {
    this.submitting.set(true);
    this.reviewsService.submit(restaurantId, reservationId, payload).pipe(
      finalize(() => this.submitting.set(false))
    ).subscribe({
      next: () => {
        this.notifications.success('Review submitted successfully.');
        this.router.navigate(['/restaurants', restaurantId], { queryParams: { tab: 'reviews' } });
      },
      error: err => {
        this.errorMessage.set(err?.error?.detail ?? err?.error?.message ?? 'Failed to submit review.');
      },
    });
  }
}
