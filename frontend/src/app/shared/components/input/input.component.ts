import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

type InputType = 'text' | 'email' | 'password' | 'number';

@Component({
  selector: 'app-input',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './input.component.html',
  styleUrls: ['./input.component.scss'],
})
export class InputComponent {
  @Input() type: InputType = 'text';
  @Input() id?: string;
  @Input() name?: string;
  @Input() label?: string;
  @Input() placeholder?: string;
  @Input() value: string = '';
  @Input() disabled: boolean = false;
  @Input() error?: string;
  @Input() required: boolean = false;
  @Input() autocomplete?: string;

  @Output() valueChange = new EventEmitter<string>();
  @Output() blur = new EventEmitter<void>();

  onValueChange(newValue: string) {
    this.value = newValue;
    this.valueChange.emit(newValue);
  }

  onBlur() {
    this.blur.emit();
  }

  get hasError(): boolean {
    return !!this.error && this.error.length > 0;
  }

  get inputClasses(): string[] {
    const classes = ['form-input'];
    if (this.hasError) {
      classes.push('form-input-error');
    }
    return classes;
  }
}
