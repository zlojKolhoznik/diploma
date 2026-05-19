import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TablerIconComponent } from '@tabler/icons-angular';

@Component({
  selector: 'app-star-rating',
  standalone: true,
  imports: [CommonModule, TablerIconComponent],
  template: `
    <div class="stars" [class.readonly]="readonly" [attr.aria-label]="'Rating: ' + value + ' out of 5'">
      @for (i of [1,2,3,4,5]; track i) {
        <button
          type="button"
          class="star"
          [class.filled]="i <= value"
          (click)="!readonly && select(i)"
          (mouseenter)="!readonly && (hovered = i)"
          (mouseleave)="!readonly && (hovered = 0)"
          [attr.aria-label]="'Rate ' + i + ' star' + (i > 1 ? 's' : '')"
          [disabled]="readonly">
          <tabler-icon [icon]="(hovered ? i <= hovered : i <= value) ? 'star-filled' : 'star'" [size]="iconSize"></tabler-icon>
        </button>
      }
    </div>
  `,
  styles: [`
    .stars {
      display: flex;
      gap: 2px;
    }
    .star {
      background: none;
      border: none;
      padding: 2px;
      cursor: pointer;
      color: var(--text-tertiary);
      transition: color 0.15s, transform 0.1s;
      border-radius: var(--radius-sm);
      &.filled { color: var(--brand-warm-gold); }
      &:hover:not(:disabled) { transform: scale(1.15); }
      &:disabled { cursor: default; }
    }
    .readonly .star { cursor: default; }
  `],
})
export class StarRatingComponent {
  @Input() value: number = 0;
  @Input() readonly: boolean = false;
  @Input() iconSize: number = 20;
  @Output() valueChange = new EventEmitter<number>();

  hovered = 0;

  select(star: number): void {
    this.value = star;
    this.valueChange.emit(star);
  }
}


