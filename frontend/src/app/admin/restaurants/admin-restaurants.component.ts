import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TablerIconComponent } from '@tabler/icons-angular';
import { RestaurantsService } from '../../core/services/restaurants.service';
import { RestaurantBrief, CreateRestaurantRequest } from '../../core/models/restaurant.models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { NotificationService } from '../../core/services/notification.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-restaurants',
  standalone: true,
  imports: [CommonModule, FormsModule, TablerIconComponent, SpinnerComponent, ModalComponent, ConfirmDialogComponent],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h2>Restaurants</h2>
        <button class="btn-primary" (click)="openCreate()" aria-label="Add restaurant">
          <tabler-icon [icon]="'plus'" [size]="16"></tabler-icon> Add Restaurant
        </button>
      </div>

      @if (loading()) {
        <app-spinner></app-spinner>
      } @else {
        <div class="table-wrapper">
          <table class="data-table">
            <thead>
              <tr><th>Image</th><th>City</th><th>Address</th><th>Available</th><th>Actions</th></tr>
            </thead>
            <tbody>
              @for (r of restaurants(); track r.id) {
                <tr>
                  <td>
                    @if (r.imageUrl) {
                      <img [src]="r.imageUrl" width="48" height="48" style="object-fit:cover;border-radius:4px" [alt]="r.city ?? 'Restaurant image'" />
                    }
                  </td>
                  <td>{{ r.city }}</td>
                  <td>{{ r.address }}</td>
                  <td>{{ r.hasAvailablePlaces ? 'Yes' : 'No' }}</td>
                  <td>
                    <div class="actions">
                      <button
                      class="icon-btn"
                      (click)="openImagePicker(restaurantImageInput, r.id)"
                      [disabled]="uploadingImageId() === r.id"
                      aria-label="Upload restaurant image">
                      Upload image
                    </button>
                    <button class="icon-btn" (click)="openEdit(r)" aria-label="Edit restaurant"><tabler-icon [icon]="'edit'" [size]="16"></tabler-icon></button>
                    <button class="icon-btn icon-btn--danger" (click)="confirmDelete(r)" aria-label="Delete restaurant"><tabler-icon [icon]="'trash'" [size]="16"></tabler-icon></button>
                  
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
          @if (restaurants().length === 0) {
            <p class="empty-note">No restaurants yet.</p>
          }
        </div>
        <input
          #restaurantImageInput
          type="file"
          accept="image/*"
          class="visually-hidden"
          (change)="onRestaurantImageSelected($event)" />
      }

      <!-- Create/Edit modal -->
      <app-modal [title]="editId() ? 'Edit Restaurant' : 'Add Restaurant'" [open]="showForm()" (closed)="showForm.set(false)">
        <form (ngSubmit)="saveRestaurant()" class="form">
          <div class="form-group">
            <label for="city">City *</label>
            <input id="city" type="text" [(ngModel)]="form.city" name="city" required />
          </div>
          <div class="form-group">
            <label for="address">Address *</label>
            <input id="address" type="text" [(ngModel)]="form.address" name="address" required />
          </div>
          <div class="form-actions">
            <button type="button" class="btn-ghost" (click)="showForm.set(false)">Cancel</button>
            <button type="submit" class="btn-primary" [disabled]="saving()">{{ saving() ? 'Saving...' : 'Save' }}</button>
          </div>
        </form>
      </app-modal>

      <app-confirm-dialog
        [open]="deleteId() !== null"
        (confirmed)="doDelete()"
        (cancelled)="deleteId.set(null)">
      </app-confirm-dialog>
    </div>
  `,
  styles: [`
    .page-container { padding: var(--space-xl) var(--space-lg); max-width: 1100px; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: var(--space-xl); }
    h2 { margin: 0; }
    .table-wrapper { overflow-x: auto; }
    .data-table { width: 100%; border-collapse: collapse; font-size: var(--text-sm); }
    .data-table th, .data-table td { padding: var(--space-md); text-align: left; border-bottom: 1px solid var(--border-subtle); }
    .data-table td { color: var(--text-primary); }
    .data-table th { color: var(--text-tertiary); font-weight: 600; }
    .data-table tr:hover td { background: var(--surface-700); }
    .actions { display: flex; gap: var(--space-xs); }
    .icon-btn { background: var(--surface-700); border: 1px solid var(--border-default); color: var(--text-secondary); padding: var(--space-xs) var(--space-sm); border-radius: var(--radius-sm); cursor: pointer; display: inline-flex; align-items: center; justify-content: center; min-height: 32px; font-size: var(--text-xs); &:hover { background: var(--surface-600); color: var(--text-primary); } }
    .icon-btn--danger { &:hover { border-color: var(--state-error); color: var(--state-error); } }
    .visually-hidden { position: absolute; width: 1px; height: 1px; margin: -1px; border: 0; padding: 0; clip: rect(0 0 0 0); overflow: hidden; }
    .btn-primary { display: inline-flex; align-items: center; gap: var(--space-xs); background: var(--accent-primary); color: white; border: none; padding: var(--space-sm) var(--space-lg); border-radius: var(--radius-md); font-weight: 600; cursor: pointer; font-size: var(--text-sm); &:hover { background: var(--accent-primary-hover); } &:disabled { opacity: 0.5; cursor: not-allowed; } }
    .btn-ghost { background: transparent; border: 1px solid var(--border-default); color: var(--text-secondary); padding: var(--space-sm) var(--space-lg); border-radius: var(--radius-md); cursor: pointer; font-size: var(--text-sm); &:hover { background: var(--surface-700); } }
    .form { display: flex; flex-direction: column; gap: var(--space-lg); }
    .form-group { display: flex; flex-direction: column; gap: var(--space-xs); label { font-size: var(--text-sm); font-weight: 500; color: var(--text-secondary); } }
    .form-actions { display: flex; gap: var(--space-sm); justify-content: flex-end; }
    .empty-note { color: var(--text-secondary); padding: var(--space-lg) 0; }
  `],
})
export class AdminRestaurantsComponent implements OnInit {
  private readonly service = inject(RestaurantsService);
  private readonly notifications = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly restaurants = signal<RestaurantBrief[]>([]);
  readonly showForm = signal(false);
  readonly editId = signal<string | null>(null);
  readonly deleteId = signal<string | null>(null);
  readonly uploadingImageId = signal<string | null>(null);

  form: CreateRestaurantRequest = { city: '', address: '' };
  private imageUploadTargetId: string | null = null;

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: res => { this.restaurants.set(res); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void {
    this.editId.set(null);
    this.form = { city: '', address: '' };
    this.showForm.set(true);
  }

  openEdit(r: RestaurantBrief): void {
    this.editId.set(r.id);
    this.form = { city: r.city ?? '', address: r.address ?? '' };
    this.showForm.set(true);
  }

  saveRestaurant(): void {
    this.saving.set(true);
    const id = this.editId();
    const obs = id ? this.service.update(id, this.form) : this.service.create(this.form);
    obs.subscribe({
      next: () => {
        this.saving.set(false); this.showForm.set(false);
        this.notifications.success(id ? 'Restaurant updated.' : 'Restaurant created.');
        this.load();
      },
      error: () => { this.saving.set(false); this.notifications.error('Operation failed.'); },
    });
  }

  confirmDelete(r: RestaurantBrief): void { this.deleteId.set(r.id); }

  openImagePicker(input: HTMLInputElement, restaurantId: string): void {
    this.imageUploadTargetId = restaurantId;
    input.value = '';
    input.click();
  }

  onRestaurantImageSelected(event: Event): void {
    const restaurantId = this.imageUploadTargetId;
    const target = event.target as HTMLInputElement | null;
    const file = target?.files?.[0];
    if (!restaurantId || !file) return;

    this.uploadingImageId.set(restaurantId);
    this.service.uploadImage(restaurantId, file).subscribe({
      next: ({ url }) => {
        this.restaurants.update(list =>
          list.map(r => (r.id === restaurantId ? { ...r, imageUrl: url } : r)),
        );
        this.uploadingImageId.set(null);
        this.notifications.success('Restaurant image uploaded.');
      },
      error: () => {
        this.uploadingImageId.set(null);
        this.notifications.error('Failed to upload restaurant image.');
      },
    });
  }

  doDelete(): void {
    const id = this.deleteId();
    if (!id) return;
    this.service.delete(id).subscribe({
      next: () => {
        this.restaurants.update(list => list.filter(r => r.id !== id));
        this.notifications.success('Restaurant deleted.');
        this.deleteId.set(null);
      },
      error: () => { this.notifications.error('Delete failed.'); this.deleteId.set(null); },
    });
  }
}


