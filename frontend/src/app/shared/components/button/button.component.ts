import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

type ButtonVariant = 'primary' | 'secondary';
type ButtonType = 'button' | 'submit' | 'reset';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './button.component.html',
  styleUrls: ['./button.component.scss'],
})
export class ButtonComponent {
  @Input() variant: ButtonVariant = 'primary';
  @Input() type: ButtonType = 'button';
  @Input() disabled: boolean = false;
  @Input() loading: boolean = false;
  @Input() fullWidth: boolean = true;
  // NOTE: kept for backwards compatibility with existing templates,
  // but this component intentionally does not render <a>.
  @Input() routerLink?: string | any[];

  @Output() click = new EventEmitter<MouseEvent>();

  get buttonClasses(): string[] {
    const classes = [`btn-${this.variant}`];
    if (this.fullWidth) {
      classes.push('btn-full-width');
    }
    return classes;
  }

  onClick(event: MouseEvent) {
    if (!this.disabled && !this.loading) {
      this.click.emit(event);
    }
  }
}
