import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TablerIconComponent } from '@tabler/icons-angular';
import { RestaurantBrief } from '../../../core/models/restaurant.models';

@Component({
  selector: 'app-restaurant-card',
  standalone: true,
  imports: [CommonModule, RouterModule, TablerIconComponent],
  template: `
    <a [routerLink]="['/restaurants', restaurant.id]" class="restaurant-card" aria-label="View {{ restaurant.city }} restaurant">
      <div class="restaurant-card__image">
        @if (restaurant.imageUrl) {
          <img [src]="restaurant.imageUrl" [alt]="restaurant.city + ' restaurant'" loading="lazy" />
        } @else {
          <div class="restaurant-card__placeholder">
            <tabler-icon [icon]="'building'" [size]="40"></tabler-icon>
          </div>
        }
        @if (restaurant.hasAvailablePlaces) {
          <span class="restaurant-card__badge">Available</span>
        }
      </div>
      <div class="restaurant-card__body">
        <div class="restaurant-card__meta">
          <tabler-icon [icon]="'map-pin'" [size]="14"></tabler-icon>
          <span>{{ restaurant.city }}</span>
        </div>
        <p class="restaurant-card__address">{{ restaurant.address }}</p>
      </div>
    </a>
  `,
  styles: [`
    .restaurant-card {
      display: block;
      background: var(--surface-800);
      border: 1px solid var(--border-default);
      border-radius: var(--radius-lg);
      overflow: hidden;
      text-decoration: none;
      transition: transform 0.2s, box-shadow 0.2s, border-color 0.2s;
      &:hover {
        transform: translateY(-2px);
        box-shadow: var(--shadow-md);
        border-color: var(--accent-primary);
        text-decoration: none;
      }
    }
    .restaurant-card__image {
      position: relative;
      height: 180px;
      background: var(--surface-700);
      overflow: hidden;
      img { width: 100%; height: 100%; object-fit: cover; }
    }
    .restaurant-card__placeholder {
      width: 100%;
      height: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--text-tertiary);
    }
    .restaurant-card__badge {
      position: absolute;
      top: var(--space-sm);
      right: var(--space-sm);
      background: var(--state-success);
      color: white;
      font-size: var(--text-xs);
      font-weight: 600;
      padding: 2px var(--space-sm);
      border-radius: var(--radius-full);
    }
    .restaurant-card__body {
      padding: var(--space-md);
    }
    .restaurant-card__meta {
      display: flex;
      align-items: center;
      gap: var(--space-xs);
      color: var(--accent-primary);
      font-size: var(--text-sm);
      font-weight: 600;
      margin-bottom: var(--space-xs);
    }
    .restaurant-card__address {
      font-size: var(--text-sm);
      color: var(--text-secondary);
      margin: 0;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
  `],
})
export class RestaurantCardComponent {
  @Input({ required: true }) restaurant!: RestaurantBrief;
}


