import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-divider',
  standalone: true,
  templateUrl: './divider.component.html',
  styleUrls: ['./divider.component.scss'],
})
export class DividerComponent {
  @Input() text?: string;
}
