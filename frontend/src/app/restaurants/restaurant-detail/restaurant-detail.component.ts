import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { TablerIconComponent } from '@tabler/icons-angular';
import { RestaurantsService } from '../../core/services/restaurants.service';
import { DishesService } from '../../core/services/dishes.service';
import { ReviewsService } from '../../core/services/reviews.service';
import { AuthService } from '../../core/services/auth.service';
import { RestaurantDetail } from '../../core/models/restaurant.models';
import { DishDetail } from '../../core/models/dish.models';
import { ReviewResponse } from '../../core/models/review.models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { DishCardComponent } from '../../shared/components/dish-card/dish-card.component';
import { ReviewCardComponent } from '../../shared/components/review-card/review-card.component';

type Tab = 'menu' | 'reviews' | 'reserve';

@Component({
  selector: 'app-restaurant-detail',
  standalone: true,
  imports: [
    CommonModule, RouterModule, TablerIconComponent,
    SpinnerComponent, DishCardComponent, ReviewCardComponent,
  ],
  template: `
    <div class="page-container">
      @if (loading()) {
        <app-spinner></app-spinner>
      } @else if (restaurant()) {
        <div class="restaurant-hero">
          @if (restaurant()!.imageUrl) {
            <img [src]="restaurant()!.imageUrl" class="restaurant-banner" alt="Restaurant banner" />
          }
          <div class="restaurant-info">
            <div class="restaurant-meta">
              <tabler-icon [icon]="'map-pin'" [size]="16"></tabler-icon>
              <span>{{ restaurant()!.city }}</span>
              <span class="sep">|</span>
              <span>{{ restaurant()!.address }}</span>
            </div>
            @if (restaurant()!.hasAvailablePlaces) {
              <span class="badge-available">Tables Available</span>
            }
          </div>
        </div>

        <div class="tabs">
          <button class="tab" [class.active]="activeTab() === 'menu'" (click)="activeTab.set('menu')">Menu</button>
          <button class="tab" [class.active]="activeTab() === 'reviews'" (click)="loadReviews()">Reviews</button>
          @if (auth.isLoggedIn()) {
            <button class="tab" [class.active]="activeTab() === 'reserve'" (click)="activeTab.set('reserve')">Reserve</button>
          }
        </div>

        <div class="tab-content">
          @if (activeTab() === 'menu') {
            @if (dishLoading()) { <app-spinner></app-spinner> }
            @else if (dishes().length === 0) {
              <p class="empty-note">No dishes available.</p>
            } @else {
              <div class="dishes-grid">
                @for (d of dishes(); track d.id) {
                  <app-dish-card [dish]="d"></app-dish-card>
                }
              </div>
            }
          }

          @if (activeTab() === 'reviews') {
            @if (reviewLoading()) { <app-spinner></app-spinner> }
            @else if (reviews().length === 0) {
              <p class="empty-note">No reviews yet.</p>
            } @else {
              <div class="reviews-list">
                @for (r of reviews(); track r.id) {
                  <app-review-card [review]="r"></app-review-card>
                }
              </div>
            }
          }

          @if (activeTab() === 'reserve') {
            <div class="reserve-panel">
              <p class="empty-note">Pick your preferred time and guest count to book a table at this restaurant.</p>
              <a class="reserve-cta" [routerLink]="['/reservations/new']" [queryParams]="{ restaurantId: restaurant()!.id }">Create reservation</a>
            </div>
          }
        </div>
      } @else {
        <div class="empty-state">
          <h3>Restaurant not found.</h3>
          <a routerLink="/restaurants">Back to list</a>
        </div>
      }
    </div>
  `,
  styles: [`
    .page-container { max-width: 1280px; margin: 0 auto; padding: var(--space-xl) var(--space-lg); }
    .restaurant-banner { width: 100%; height: 280px; object-fit: cover; border-radius: var(--radius-lg); margin-bottom: var(--space-lg); }
    .restaurant-info { display: flex; align-items: center; gap: var(--space-md); flex-wrap: wrap; margin-bottom: var(--space-lg); }
    .restaurant-meta { display: flex; align-items: center; gap: var(--space-xs); color: var(--text-secondary); font-size: var(--text-sm); }
    .sep { color: var(--text-tertiary); }
    .badge-available { background: rgba(40,167,69,0.15); color: var(--state-success); font-size: var(--text-xs); font-weight: 600; padding: 2px var(--space-sm); border-radius: var(--radius-full); }
    .tabs { display: flex; gap: var(--space-xs); border-bottom: 1px solid var(--border-default); margin-bottom: var(--space-xl); }
    .tab {
      background: transparent; border: none; color: var(--text-secondary); padding: var(--space-sm) var(--space-lg);
      cursor: pointer; font-size: var(--text-sm); font-weight: 500; border-bottom: 2px solid transparent;
      transition: all 0.2s; margin-bottom: -1px;
      &:hover { color: var(--text-primary); }
      &.active { color: var(--accent-primary); border-bottom-color: var(--accent-primary); }
    }
    .dishes-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: var(--space-lg); }
    .reviews-list { display: flex; flex-direction: column; gap: var(--space-md); }
    .empty-note { color: var(--text-secondary); padding: var(--space-xl) 0; }
    .reserve-cta {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      border-radius: var(--radius-md);
      padding: var(--space-sm) var(--space-lg);
      text-decoration: none;
      font-size: var(--text-sm);
      font-weight: 600;
      color: white;
      background: var(--accent-primary);
    }
    .reserve-cta:hover { background: var(--accent-primary-hover); text-decoration: none; }
    .empty-state { text-align: center; padding: var(--space-2xl); }
  `],
})
export class RestaurantDetailComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly restaurantsService = inject(RestaurantsService);
  private readonly dishesService = inject(DishesService);
  private readonly reviewsService = inject(ReviewsService);

  readonly loading = signal(false);
  readonly dishLoading = signal(false);
  readonly reviewLoading = signal(false);
  readonly restaurant = signal<RestaurantDetail | null>(null);
  readonly dishes = signal<DishDetail[]>([]);
  readonly reviews = signal<ReviewResponse[]>([]);
  readonly activeTab = signal<Tab>('menu');

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loading.set(true);
    this.restaurantsService.getById(id).subscribe({
      next: res => {
        this.restaurant.set(res);
        this.loading.set(false);
        this.loadDishes(id);
      },
      error: () => this.loading.set(false),
    });
  }

  private loadDishes(id: string): void {
    this.dishLoading.set(true);
    this.dishesService.getByRestaurant(id).subscribe({
      next: res => { this.dishes.set(res); this.dishLoading.set(false); },
      error: () => this.dishLoading.set(false),
    });
  }

  loadReviews(): void {
    this.activeTab.set('reviews');
    if (this.reviews().length > 0) return;
    const id = this.route.snapshot.paramMap.get('id')!;
    this.reviewLoading.set(true);
    this.reviewsService.getAll(id).subscribe({
      next: res => { this.reviews.set(res); this.reviewLoading.set(false); },
      error: () => this.reviewLoading.set(false),
    });
  }
}


