import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
  id: string;
  type: ToastType;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly _toasts = new BehaviorSubject<Toast[]>([]);
  readonly toasts$ = this._toasts.asObservable();

  success(message: string): void {
    this.add('success', message);
  }

  error(message: string): void {
    this.add('error', message);
  }

  warning(message: string): void {
    this.add('warning', message);
  }

  info(message: string): void {
    this.add('info', message);
  }

  dismiss(id: string): void {
    this._toasts.next(this._toasts.value.filter(t => t.id !== id));
  }

  private add(type: ToastType, message: string): void {
    const id = Math.random().toString(36).slice(2);
    const toast: Toast = { id, type, message };
    this._toasts.next([...this._toasts.value, toast]);
    setTimeout(() => this.dismiss(id), 4000);
  }
}

