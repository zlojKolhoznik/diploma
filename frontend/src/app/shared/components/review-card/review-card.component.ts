import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TablerIconComponent } from '@tabler/icons-angular';
import { StarRatingComponent } from '../star-rating/star-rating.component';
import { ReviewResponse } from '../../../core/models/review.models';

@Component({
  selector: 'app-review-card',
  standalone: true,
  imports: [CommonModule, TablerIconComponent, StarRatingComponent],
  template: `
    <div class="review-card">
      <div class="review-card__header">
        <div class="review-card__ratings">
          <div class="review-card__rating-row">
            <span class="review-card__label">Cuisine</span>
            <app-star-rating [value]="review.cuisineRating" [readonly]="true" [iconSize]="16"></app-star-rating>
          </div>
          <div class="review-card__rating-row">
            <span class="review-card__label">Service</span>
            <app-star-rating [value]="review.serviceRating" [readonly]="true" [iconSize]="16"></app-star-rating>
          </div>
        </div>
        <div class="review-card__meta">
          <span class="review-card__date">{{ review.createdAtUtc | date:'mediumDate' }}</span>
          @if (showDelete) {
            <button class="review-card__delete" (click)="deleteReview.emit(review.id)" aria-label="Delete review">
              <tabler-icon [icon]="'trash'" [size]="16"></tabler-icon>
            </button>
          }
        </div>
      </div>
      @if (review.cuisineComment) {
        <p class="review-card__comment"><em>Cuisine:</em> {{ review.cuisineComment }}</p>
      }
      @if (review.serviceComment) {
        <p class="review-card__comment"><em>Service:</em> {{ review.serviceComment }}</p>
      }
    </div>
  `,
  styles: [`
    .review-card {
      background: var(--surface-800);
      border: 1px solid var(--border-default);
      border-radius: var(--radius-lg);
      padding: var(--space-md);
    }
    .review-card__header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: var(--space-sm); }
    .review-card__ratings { display: flex; flex-direction: column; gap: var(--space-xs); }
    .review-card__rating-row { display: flex; align-items: center; gap: var(--space-sm); }
    .review-card__label { font-size: var(--text-xs); color: var(--text-tertiary); width: 60px; }
    .review-card__meta { display: flex; align-items: center; gap: var(--space-sm); }
    .review-card__date { font-size: var(--text-xs); color: var(--text-tertiary); }
    .review-card__delete {
      background: transparent; border: none; color: var(--text-tertiary); cursor: pointer;
      padding: 4px; border-radius: var(--radius-sm);
      &:hover { color: var(--state-error); }
    }
    .review-card__comment { font-size: var(--text-sm); color: var(--text-secondary); margin: 0 0 var(--space-xs); }
  `],
})
export class ReviewCardComponent {
  @Input({ required: true }) review!: ReviewResponse;
  @Input() showDelete: boolean = false;
  @Output() deleteReview = new EventEmitter<string>();
}


