import { Component, Input } from '@angular/core';

type LogoSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-logo',
  standalone: true,
  templateUrl: './logo.component.html',
  styleUrls: ['./logo.component.scss'],
})
export class LogoComponent {
  @Input() size: LogoSize = 'md';
  @Input() src: string = '/assets/logo.png';
  @Input() alt: string = 'Lapti Steaks';

  get sizeClass(): string {
    return `logo-${this.size}`;
  }
}
