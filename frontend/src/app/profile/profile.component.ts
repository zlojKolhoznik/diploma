import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProfileService } from '../core/services/profile.service';
import { ProfileResponse } from '../core/models/profile.models';
import { SpinnerComponent } from '../shared/components/spinner/spinner.component';
import { NotificationService } from '../core/services/notification.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, SpinnerComponent],
  template: `
    <div class="page-container">
      <h2>My Profile</h2>

      @if (loading()) {
        <app-spinner></app-spinner>
      } @else if (profile()) {
        <form class="profile-form" (ngSubmit)="save()" novalidate>
          <div class="form-group">
            <label for="email">Email</label>
            <input id="email" type="email" [value]="profile()!.email ?? ''" disabled />
          </div>
          <div class="form-row">
            <div class="form-group">
              <label for="firstName">First Name</label>
              <input id="firstName" type="text" [(ngModel)]="edited.firstName" name="firstName" />
            </div>
            <div class="form-group">
              <label for="lastName">Last Name</label>
              <input id="lastName" type="text" [(ngModel)]="edited.lastName" name="lastName" />
            </div>
          </div>
          <div class="form-group">
            <label for="phone">Phone Number</label>
            <input id="phone" type="tel" [(ngModel)]="edited.phoneNumber" name="phone" />
          </div>
          <button type="submit" class="btn-save" [disabled]="saving()">
            {{ saving() ? 'Saving…' : 'Save Changes' }}
          </button>
        </form>
      }
    </div>
  `,
  styles: [`
    .page-container { max-width: 480px; margin: 0 auto; padding: var(--space-xl) var(--space-lg); }
    h2 { margin-bottom: var(--space-xl); }
    .profile-form { display: flex; flex-direction: column; gap: var(--space-lg); }
    .form-group { display: flex; flex-direction: column; gap: var(--space-xs); }
    label { font-size: var(--text-sm); font-weight: 500; color: var(--text-secondary); }
    input { padding: var(--space-md); border-radius: var(--radius-md); font-size: var(--text-base); }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-md); }
    .btn-save {
      background: var(--accent-primary); color: white; border: none;
      padding: var(--space-md) var(--space-xl); border-radius: var(--radius-md);
      font-weight: 600; cursor: pointer; align-self: flex-start;
      &:hover { background: var(--accent-primary-hover); }
      &:disabled { opacity: 0.5; cursor: not-allowed; }
    }
  `],
})
export class ProfileComponent implements OnInit {
  private readonly service = inject(ProfileService);
  private readonly notifications = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly profile = signal<ProfileResponse | null>(null);

  edited = { firstName: '', lastName: '', phoneNumber: '' };

  ngOnInit(): void {
    this.loading.set(true);
    this.service.get().subscribe({
      next: res => {
        this.profile.set(res);
        this.edited = {
          firstName: res.firstName ?? '',
          lastName: res.lastName ?? '',
          phoneNumber: res.phoneNumber ?? '',
        };
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  save(): void {
    this.saving.set(true);
    this.service.update({
      firstName: this.edited.firstName || null,
      lastName: this.edited.lastName || null,
      phoneNumber: this.edited.phoneNumber || null,
    }).subscribe({
      next: res => {
        this.profile.set(res);
        this.saving.set(false);
        this.notifications.success('Profile updated!');
      },
      error: () => {
        this.saving.set(false);
        this.notifications.error('Failed to update profile.');
      },
    });
  }
}

