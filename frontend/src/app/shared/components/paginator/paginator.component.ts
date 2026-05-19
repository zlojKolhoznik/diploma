import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TablerIconComponent } from '@tabler/icons-angular';

@Component({
  selector: 'app-paginator',
  standalone: true,
  imports: [CommonModule, TablerIconComponent],
  template: `
    <div class="paginator" aria-label="Pagination">
      <button class="pag-btn" (click)="prev()" [disabled]="page <= 1" aria-label="Previous page">
        <tabler-icon [icon]="'chevron-left'" [size]="16"></tabler-icon>
      </button>
      <span class="pag-info">Page {{ page }} of {{ totalPages() }}</span>
      <button class="pag-btn" (click)="next()" [disabled]="page >= totalPages()" aria-label="Next page">
        <tabler-icon [icon]="'chevron-right'" [size]="16"></tabler-icon>
      </button>
    </div>
  `,
  styles: [`
    .paginator {
      display: flex;
      align-items: center;
      gap: var(--space-md);
      justify-content: center;
      padding: var(--space-md) 0;
    }
    .pag-btn {
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--surface-700);
      border: 1px solid var(--border-default);
      color: var(--text-secondary);
      border-radius: var(--radius-md);
      width: 36px;
      height: 36px;
      cursor: pointer;
      transition: all 0.2s;
      &:hover:not(:disabled) {
        background: var(--surface-600);
        color: var(--text-primary);
      }
      &:disabled { opacity: 0.4; cursor: not-allowed; }
    }
    .pag-info {
      font-size: var(--text-sm);
      color: var(--text-secondary);
    }
  `],
})
export class PaginatorComponent {
  @Input() total: number = 0;
  @Input() page: number = 1;
  @Input() pageSize: number = 10;
  @Output() pageChange = new EventEmitter<number>();

  totalPages(): number {
    return Math.max(1, Math.ceil(this.total / this.pageSize));
  }

  prev(): void {
    if (this.page > 1) this.pageChange.emit(this.page - 1);
  }

  next(): void {
    if (this.page < this.totalPages()) this.pageChange.emit(this.page + 1);
  }
}


