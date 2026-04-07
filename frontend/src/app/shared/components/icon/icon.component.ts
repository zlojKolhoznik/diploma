import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TablerIconComponent } from '@tabler/icons-angular';

type IconSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-icon',
  standalone: true,
  imports: [CommonModule, TablerIconComponent],
  templateUrl: './icon.component.html',
  styleUrls: ['./icon.component.scss'],
})
export class IconComponent {
  @Input() name: string = 'help-circle';
  @Input() size: IconSize = 'md';
  @Input() ariaLabel?: string;
  @Input() ariaHidden?: boolean = false;

  get sizePixels(): number {
    const sizeMap = { sm: 16, md: 24, lg: 32 };
    return sizeMap[this.size];
  }
}

