import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { TablerIconComponent } from '@tabler/icons-angular';
import { RestaurantsService } from '../../core/services/restaurants.service';
import { RestaurantBrief } from '../../core/models/restaurant.models';
import { RestaurantCardComponent } from '../../shared/components/restaurant-card/restaurant-card.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { PaginatorComponent } from '../../shared/components/paginator/paginator.component';

@Component({
  selector: 'app-restaurant-list',
  standalone: true,
  imports: [CommonModule, RouterModule, TablerIconComponent, RestaurantCardComponent, SpinnerComponent, PaginatorComponent],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h2>{{ city() ? 'Restaurants in ' + city() : 'All Restaurants' }}</h2>
      </div>

      @if (loading()) {
        <app-spinner></app-spinner>
      } @else if (restaurants().length === 0) {
        <div class="empty-state">
          <tabler-icon [icon]="'building'" [size]="48"></tabler-icon>
          <h3>No restaurants found</h3>
          <p>Try searching in a different city.</p>
        </div>
      } @else {
        <div class="restaurant-grid">
          @for (r of pagedRestaurants(); track r.id) {
            <app-restaurant-card [restaurant]="r"></app-restaurant-card>
          }
        </div>
        @if (restaurants().length > pageSize) {
          <app-paginator
            [total]="restaurants().length"
            [page]="page()"
            [pageSize]="pageSize"
            (pageChange)="page.set($event)">
          </app-paginator>
        }
      }
    </div>
  `,
  styles: [`
    .page-container { max-width: 1280px; margin: 0 auto; padding: var(--space-xl) var(--space-lg); }
    .page-header { margin-bottom: var(--space-xl); }
    .page-header h2 { margin: 0; }
    .restaurant-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: var(--space-lg);
    }
    .empty-state {
      text-align: center; padding: var(--space-2xl);
      color: var(--text-tertiary);
      h3 { margin: var(--space-md) 0 var(--space-sm); }
      p { margin: 0; }
    }
  `],
})
export class RestaurantListComponent implements OnInit {
  private readonly service = inject(RestaurantsService);
  private readonly route = inject(ActivatedRoute);

  readonly loading = signal(false);
  readonly restaurants = signal<RestaurantBrief[]>([]);
  readonly city = signal('');
  readonly page = signal(1);
  readonly pageSize = 12;

  readonly pagedRestaurants = () => {
    const start = (this.page() - 1) * this.pageSize;
    return this.restaurants().slice(start, start + this.pageSize);
  };

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      const city = params.get('city') ?? '';
      this.city.set(city);
      this.page.set(1);
      this.load(city);
    });
  }

  private load(city: string): void {
    this.loading.set(true);
    this.service.getAll(city || undefined).subscribe({
      next: res => { this.restaurants.set(res); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}


