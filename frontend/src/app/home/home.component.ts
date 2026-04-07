import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import {
  LogoComponent,
  ButtonComponent,
  CardComponent,
  LinkComponent,
  IconComponent,
} from '../shared/components';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    LogoComponent,
    ButtonComponent,
    CardComponent,
    LinkComponent,
    IconComponent,
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent {}