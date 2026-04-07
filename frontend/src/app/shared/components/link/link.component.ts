import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

type LinkVariant = 'default' | 'ghost';

@Component({
  selector: 'app-link',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './link.component.html',
  styleUrls: ['./link.component.scss'],
})
export class LinkComponent {
  @Input() href?: string;
  @Input() routerLink?: string | any[];
  @Input() variant: LinkVariant = 'default';
  @Input() external: boolean = false;

  get variantClass(): string {
    return `link-${this.variant}`;
  }
}
