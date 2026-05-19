import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TablerIconComponent } from '@tabler/icons-angular';
import { DishesService } from '../../core/services/dishes.service';
import { DishBrief, DishDetail, CreateDishRequest } from '../../core/models/dish.models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-admin-dishes',
  standalone: true,
  imports: [CommonModule, FormsModule, TablerIconComponent, SpinnerComponent, ModalComponent, ConfirmDialogComponent],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h2>Dishes</h2>
        <button class="btn-primary" (click)="openCreate()" aria-label="Add dish"><tabler-icon [icon]="'plus'" [size]="16"></tabler-icon> Add Dish</button>
      </div>

      @if (loading()) { <app-spinner></app-spinner> }
      @else {
        <div class="table-wrapper">
          <table class="data-table">
            <thead><tr><th>Image</th><th>Name</th><th>Price</th><th>Actions</th></tr></thead>
            <tbody>
              @for (d of dishes(); track d.id) {
                <tr>
                  <td>@if (d.imageUrl) { <img [src]="d.imageUrl" width="48" height="48" style="object-fit:cover;border-radius:4px" [alt]="d.name ?? ''" /> }</td>
                  <td>{{ d.name }}</td>
                  <td>{{ d.price | number:'1.2-2' }} ₴</td>
                  <td>
                    <div class="actions">
                      <button class="icon-btn" (click)="openEdit(d)" aria-label="Edit dish"><tabler-icon [icon]="'edit'" [size]="16"></tabler-icon></button>
                    <button
                      class="icon-btn"
                      (click)="openImagePicker(dishImageInput, d.id)"
                      [disabled]="uploadingImageId() === d.id"
                      aria-label="Upload dish image">
                      Upload image
                    </button>
                    <button class="icon-btn icon-btn--danger" (click)="confirmDelete(d)" aria-label="Delete dish"><tabler-icon [icon]="'trash'" [size]="16"></tabler-icon></button>
                    </div>
                    </td>
                </tr>
              }
            </tbody>
          </table>
          @if (dishes().length === 0) { <p class="empty-note">No dishes found.</p> }
        </div>
        <input
          #dishImageInput
          type="file"
          accept="image/*"
          class="visually-hidden"
          (change)="onDishImageSelected($event)" />
      }

      <app-modal [title]="editId() ? 'Edit Dish' : 'Add Dish'" [open]="showForm()" (closed)="showForm.set(false)">
        <form (ngSubmit)="saveDish()" class="form">
          <div class="form-group"><label for="dname">Name *</label><input id="dname" type="text" [(ngModel)]="form.name" name="name" required /></div>
          <div class="form-group"><label for="ddesc">Description *</label><textarea id="ddesc" [(ngModel)]="form.description" name="description" rows="3" required></textarea></div>
          <div class="form-group"><label for="dprice">Price *</label><input id="dprice" type="number" [(ngModel)]="form.price" name="price" min="0.01" step="0.01" required /></div>
          <div class="form-actions">
            <button type="button" class="btn-ghost" (click)="showForm.set(false)">Cancel</button>
            <button type="submit" class="btn-primary" [disabled]="saving()">{{ saving() ? 'Saving...' : 'Save' }}</button>
          </div>
        </form>
      </app-modal>

      <app-confirm-dialog [open]="deleteId() !== null" (confirmed)="doDelete()" (cancelled)="deleteId.set(null)"></app-confirm-dialog>
    </div>
  `,
  styles: [`
    .page-container { padding: var(--space-xl) var(--space-lg); max-width: 1100px; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: var(--space-xl); gap: var(--space-md); }
    h2 { margin: 0; }
    .table-wrapper { overflow-x: auto; }
    .data-table { width: 100%; border-collapse: collapse; font-size: var(--text-sm); }
    .data-table th, .data-table td { padding: var(--space-md); text-align: left; border-bottom: 1px solid var(--border-subtle); }
    .data-table td { color: var(--text-primary); }
    .data-table th { color: var(--text-tertiary); font-weight: 600; }
    .data-table tr:hover td { background: var(--surface-700); }
    .actions { display: flex; gap: var(--space-xs); }
    .icon-btn { background: var(--surface-700); border: 1px solid var(--border-default); color: var(--text-secondary); padding: var(--space-xs) var(--space-sm); border-radius: var(--radius-sm); cursor: pointer; display: inline-flex; align-items: center; justify-content: center; min-height: 32px; font-size: var(--text-xs); &:hover { color: var(--text-primary); } }
    .icon-btn--danger { &:hover { border-color: var(--state-error); color: var(--state-error); } }
    .visually-hidden { position: absolute; width: 1px; height: 1px; margin: -1px; border: 0; padding: 0; clip: rect(0 0 0 0); overflow: hidden; }
    .btn-primary { display: inline-flex; align-items: center; gap: var(--space-xs); background: var(--accent-primary); color: white; border: none; padding: var(--space-sm) var(--space-lg); border-radius: var(--radius-md); font-weight: 600; cursor: pointer; font-size: var(--text-sm); &:hover { background: var(--accent-primary-hover); } &:disabled { opacity: 0.5; cursor: not-allowed; } }
    .btn-ghost { background: transparent; border: 1px solid var(--border-default); color: var(--text-secondary); padding: var(--space-sm) var(--space-lg); border-radius: var(--radius-md); cursor: pointer; font-size: var(--text-sm); }
    .form { display: flex; flex-direction: column; gap: var(--space-lg); }
    .form-group { display: flex; flex-direction: column; gap: var(--space-xs); label { font-size: var(--text-sm); font-weight: 500; color: var(--text-secondary); } textarea { resize: vertical; } }
    .form-actions { display: flex; gap: var(--space-sm); justify-content: flex-end; }
    .empty-note { color: var(--text-secondary); padding: var(--space-lg) 0; }
  `],
})
export class AdminDishesComponent implements OnInit {
  private readonly dishesService = inject(DishesService);
  private readonly notifications = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly dishes = signal<DishBrief[]>([]);
  readonly showForm = signal(false);
  readonly deleteId = signal<string | null>(null);
  readonly editId = signal<string | null>(null);
  readonly uploadingImageId = signal<string | null>(null);

  form: CreateDishRequest = { name: '', description: '', price: 0 };
  private imageUploadTargetId: string | null = null;

  ngOnInit(): void {
    this.loadDishes();
  }

  loadDishes(): void {
    this.loading.set(true);
    this.dishesService.getAll().subscribe({
      next: res => { this.dishes.set(res); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void {
    this.editId.set(null);
    this.form = { name: '', description: '', price: 0 };
    this.showForm.set(true);
  }

  openEdit(dish: DishBrief): void {
    this.loading.set(true);
    this.dishesService.getById(dish.id).subscribe({
      next: (detail: DishDetail) => {
        this.editId.set(detail.id);
        this.form = {
          name: detail.name ?? '',
          description: detail.description ?? '',
          price: detail.price,
        };
        this.showForm.set(true);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.notifications.error('Failed to load dish details.');
      },
    });
  }

  saveDish(): void {
    this.saving.set(true);
    const id = this.editId();
    const request = { ...this.form };
    const obs = id ? this.dishesService.update(id, request) : this.dishesService.create(request);
    obs.subscribe({
      next: () => {
        this.saving.set(false);
        this.showForm.set(false);
        this.notifications.success(id ? 'Dish updated.' : 'Dish created.');
        this.loadDishes();
      },
      error: () => {
        this.saving.set(false);
        this.notifications.error(id ? 'Failed to update dish.' : 'Failed to create dish.');
      },
    });
  }

  openImagePicker(input: HTMLInputElement, dishId: string): void {
    this.imageUploadTargetId = dishId;
    input.value = '';
    input.click();
  }

  onDishImageSelected(event: Event): void {
    const dishId = this.imageUploadTargetId;
    const target = event.target as HTMLInputElement | null;
    const file = target?.files?.[0];
    if (!dishId || !file) return;

    this.uploadingImageId.set(dishId);
    this.dishesService.uploadImage(dishId, file).subscribe({
      next: ({ url }) => {
        this.dishes.update(list =>
          list.map(d => (d.id === dishId ? { ...d, imageUrl: url } : d)),
        );
        this.uploadingImageId.set(null);
        this.notifications.success('Dish image uploaded.');
      },
      error: () => {
        this.uploadingImageId.set(null);
        this.notifications.error('Failed to upload dish image.');
      },
    });
  }

  confirmDelete(d: DishBrief): void { this.deleteId.set(d.id); }

  doDelete(): void {
    const id = this.deleteId();
    if (!id) return;
    this.dishesService.delete(id).subscribe({
      next: () => {
        this.dishes.update(list => list.filter(d => d.id !== id));
        this.notifications.success('Dish deleted.'); this.deleteId.set(null);
      },
      error: () => { this.notifications.error('Delete failed.'); this.deleteId.set(null); },
    });
  }
}


