import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [RouterModule],
  template: `
    <footer class="footer">
      <div class="footer-container">
        <span class="footer-copy">&copy; {{ year }} Lapti Steaks. All rights reserved.</span>
        <nav class="footer-links">
          <a routerLink="/">Home</a>
          <a routerLink="/restaurants">Restaurants</a>
        </nav>
      </div>
    </footer>
  `,
  styles: [`
    .footer {
      background-color: var(--surface-800);
      border-top: 1px solid var(--border-default);
      padding: var(--space-md) 0;
    }
    .footer-container {
      max-width: 1280px;
      margin: 0 auto;
      padding: 0 var(--space-lg);
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: var(--space-md);
    }
    .footer-copy {
      font-size: var(--text-sm);
      color: var(--text-tertiary);
    }
    .footer-links {
      display: flex;
      gap: var(--space-md);
      a {
        font-size: var(--text-sm);
        color: var(--text-secondary);
        text-decoration: none;
        &:hover { color: var(--accent-primary); }
      }
    }
  `],
})
export class FooterComponent {
  readonly year = new Date().getFullYear();
}

