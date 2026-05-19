import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterModule],
  template: `
    <div class="error-page">
      <h1>404</h1>
      <h2>Page Not Found</h2>
      <p>The page you're looking for doesn't exist.</p>
      <a routerLink="/" class="btn-home">Go Home</a>
    </div>
  `,
  styles: [`
    .error-page {
      display: flex; flex-direction: column; align-items: center; justify-content: center;
      min-height: 60vh; text-align: center; padding: var(--space-xl);
    }
    h1 { font-size: 6rem; color: var(--accent-primary); margin-bottom: 0; }
    h2 { margin-bottom: var(--space-md); }
    p { color: var(--text-secondary); margin-bottom: var(--space-xl); }
    .btn-home {
      background: var(--accent-primary); color: white;
      padding: var(--space-md) var(--space-xl); border-radius: var(--radius-md);
      text-decoration: none; font-weight: 600;
      &:hover { background: var(--accent-primary-hover); text-decoration: none; }
    }
  `],
})
export class NotFoundComponent {}

