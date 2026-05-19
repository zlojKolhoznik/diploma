import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TablerIconComponent } from '@tabler/icons-angular';
import { DishDetail } from '../../../core/models/dish.models';

@Component({
  selector: 'app-dish-card',
  standalone: true,
  imports: [CommonModule, TablerIconComponent],
  template: `
    <div class="dish-card">
      <div class="dish-card__image">
        @if (dish.imageUrl) {
          <img [src]="dish.imageUrl" [alt]="dish.name ?? 'Dish'" loading="lazy" />
        } @else {
          <div class="dish-card__placeholder">
            <tabler-icon [icon]="'utensils-fork'" [size]="32"></tabler-icon>
          </div>
        }
      </div>
      <div class="dish-card__body">
        <h4 class="dish-card__name">{{ dish.name }}</h4>
        <p class="dish-card__desc">{{ dish.description }}</p>
        <div class="dish-card__footer">
          <span class="dish-card__price">{{ dish.price | number:'1.2-2' }} ₴</span>
          @if (showAvailabilityToggle) {
            <label class="toggle" [attr.aria-label]="'Toggle availability for ' + dish.name">
              <input type="checkbox" [checked]="available" (change)="toggleAvailability.emit(!available)" />
              <span class="toggle-pill"></span>
              <span class="toggle-label">{{ available ? 'Available' : 'Unavailable' }}</span>
            </label>
          }
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dish-card {
      background: var(--surface-800);
      border: 1px solid var(--border-default);
      border-radius: var(--radius-lg);
      overflow: hidden;
      transition: border-color 0.2s;
      &:hover { border-color: var(--surface-600); }
    }
    .dish-card__image {
      height: 140px;
      background: var(--surface-700);
      overflow: hidden;
      img { width: 100%; height: 100%; object-fit: cover; }
    }
    .dish-card__placeholder {
      width: 100%; height: 100%;
      display: flex; align-items: center; justify-content: center;
      color: var(--text-tertiary);
    }
    .dish-card__body { padding: var(--space-md); }
    .dish-card__name { font-size: var(--text-base); font-weight: 600; margin-bottom: var(--space-xs); color: var(--text-primary); }
    .dish-card__desc { font-size: var(--text-sm); color: var(--text-secondary); margin-bottom: var(--space-sm); line-height: 1.4; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
    .dish-card__footer { display: flex; align-items: center; justify-content: space-between; }
    .dish-card__price { font-weight: 700; color: var(--accent-primary); }
    .toggle { display: flex; align-items: center; gap: var(--space-xs); cursor: pointer; }
    .toggle input { display: none; }
    .toggle-pill {
      width: 36px; height: 20px; background: var(--surface-600); border-radius: var(--radius-full);
      position: relative; transition: background 0.2s;
      &::after { content: ''; position: absolute; width: 14px; height: 14px; border-radius: 50%; background: white; top: 3px; left: 3px; transition: transform 0.2s; }
    }
    .toggle input:checked + .toggle-pill { background: var(--state-success); }
    .toggle input:checked + .toggle-pill::after { transform: translateX(16px); }
    .toggle-label { font-size: var(--text-xs); color: var(--text-tertiary); }
  `],
})
export class DishCardComponent {
  @Input({ required: true }) dish!: DishDetail;
  @Input() available: boolean = true;
  @Input() showAvailabilityToggle: boolean = false;
  @Output() toggleAvailability = new EventEmitter<boolean>();
}


