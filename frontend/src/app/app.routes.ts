import { Routes } from '@angular/router';
import { AppShellComponent } from './layout/app-shell/app-shell.component';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: '',
    component: AppShellComponent,
    children: [
      {
        path: '',
        loadComponent: () => import('./home/home.component').then(m => m.HomeComponent),
      },
      {
        path: 'login',
        loadComponent: () => import('./auth/components/login/login.component').then(m => m.LoginComponent),
      },
      {
        path: 'register',
        loadComponent: () => import('./auth/components/register/register.component').then(m => m.RegisterComponent),
      },
      {
        path: 'restaurants',
        loadComponent: () => import('./restaurants/restaurant-list/restaurant-list.component').then(m => m.RestaurantListComponent),
      },
      {
        path: 'restaurants/:id',
        loadComponent: () => import('./restaurants/restaurant-detail/restaurant-detail.component').then(m => m.RestaurantDetailComponent),
      },
      {
        path: 'reservations/new',
        canActivate: [authGuard],
        loadComponent: () => import('./reservations/create-reservation/create-reservation.component').then(m => m.CreateReservationComponent),
      },
      {
        path: 'my/reservations',
        canActivate: [authGuard],
        loadComponent: () => import('./reservations/my-reservations/my-reservations.component').then(m => m.MyReservationsComponent),
      },
      {
        path: 'my/reservations/:id/review',
        canActivate: [authGuard],
        loadComponent: () => import('./reservations/review-reservation/review-reservation.component').then(m => m.ReviewReservationComponent),
      },
      {
        path: 'my/orders',
        canActivate: [authGuard],
        loadComponent: () => import('./orders/my-orders/my-orders.component').then(m => m.MyOrdersComponent),
      },
      {
        path: 'profile',
        canActivate: [authGuard],
        loadComponent: () => import('./profile/profile.component').then(m => m.ProfileComponent),
      },
      {
        path: 'waiter',
        canActivate: [roleGuard('Waiter', 'Admin')],
        loadComponent: () => import('./waiter/waiter-layout/waiter-layout.component').then(m => m.WaiterLayoutComponent),
        children: [
          {
            path: '',
            redirectTo: 'reservations',
            pathMatch: 'full',
          },
          {
            path: 'reservations',
            loadComponent: () => import('./waiter/waiter-reservations/waiter-reservations.component').then(m => m.WaiterReservationsComponent),
          },
          {
            path: 'orders',
            loadComponent: () => import('./waiter/waiter-orders/waiter-orders.component').then(m => m.WaiterOrdersComponent),
          },
          {
            path: 'schedule',
            loadComponent: () => import('./waiter/my-schedule/my-schedule.component').then(m => m.MyScheduleComponent),
          },
        ],
      },
      {
        path: 'admin',
        canActivate: [roleGuard('Admin')],
        loadComponent: () => import('./admin/admin-layout/admin-layout.component').then(m => m.AdminLayoutComponent),
        children: [
          {
            path: '',
            redirectTo: 'restaurants',
            pathMatch: 'full',
          },
          {
            path: 'restaurants',
            loadComponent: () => import('./admin/restaurants/admin-restaurants.component').then(m => m.AdminRestaurantsComponent),
          },
          {
            path: 'dishes',
            loadComponent: () => import('./admin/dishes/admin-dishes.component').then(m => m.AdminDishesComponent),
          },
          {
            path: 'tables',
            loadComponent: () => import('./admin/tables/admin-tables.component').then(m => m.AdminTablesComponent),
          },
          {
            path: 'waiters',
            loadComponent: () => import('./admin/waiters/admin-waiters.component').then(m => m.AdminWaitersComponent),
          },
          {
            path: 'schedules',
            loadComponent: () => import('./admin/schedules/admin-schedules.component').then(m => m.AdminSchedulesComponent),
          },
          {
            path: 'analytics',
            loadComponent: () => import('./admin/analytics/admin-analytics.component').then(m => m.AdminAnalyticsComponent),
          },
          {
            path: 'users',
            loadComponent: () => import('./admin/users/admin-users.component').then(m => m.AdminUsersComponent),
          },
        ],
      },
      {
        path: 'forbidden',
        loadComponent: () => import('./shared/components/forbidden/forbidden.component').then(m => m.ForbiddenComponent),
      },
      {
        path: 'not-found',
        loadComponent: () => import('./shared/components/not-found/not-found.component').then(m => m.NotFoundComponent),
      },
      { path: '**', redirectTo: 'not-found' },
    ],
  },
];
