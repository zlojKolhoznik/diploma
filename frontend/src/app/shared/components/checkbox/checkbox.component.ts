import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-checkbox',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './checkbox.component.html',
  styleUrls: ['./checkbox.component.scss'],
})
export class CheckboxComponent {
  @Input() label: string = '';
  @Input() checked: boolean = false;
  @Input() disabled: boolean = false;
  @Input() id?: string;
  @Input() name?: string;

  @Output() checkedChange = new EventEmitter<boolean>();

  onCheckboxChange(checked: boolean) {
    this.checked = checked;
    this.checkedChange.emit(checked);
  }
}
