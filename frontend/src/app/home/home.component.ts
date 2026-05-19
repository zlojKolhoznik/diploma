import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TablerIconComponent } from '@tabler/icons-angular';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, TablerIconComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent {
  private readonly router = inject(Router);
  readonly city = signal('');

  search(): void {
    const q = this.city().trim();
    this.router.navigate(['/restaurants'], q ? { queryParams: { city: q } } : {});
  }
}
