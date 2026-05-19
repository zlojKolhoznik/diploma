import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import {
  isEmailValid,
  isPasswordValid,
  isLoginFormValid,
} from '../../validators/auth.validators';
import {
  LogoComponent,
  AlertComponent,
  InputComponent,
  CheckboxComponent,
  LinkComponent,
  ButtonComponent,
  DividerComponent,
  CardComponent,
} from '../../../shared/components';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    LogoComponent,
    AlertComponent,
    InputComponent,
    CheckboxComponent,
    LinkComponent,
    ButtonComponent,
    DividerComponent,
    CardComponent,
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  host: {
    class: 'auth-page-host',
  },
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  email: string = '';
  password: string = '';
  isLoading: boolean = false;
  errorMessage: string = '';
  rememberMe: boolean = false;
  emailDirty: boolean = false;
  passwordDirty: boolean = false;

  onEmailBlur() { this.emailDirty = true; }
  onPasswordBlur() { this.passwordDirty = true; }

  isEmailValid(): boolean { return isEmailValid(this.email); }
  isPasswordValid(): boolean { return isPasswordValid(this.password); }
  isFormValid(): boolean {
    return isLoginFormValid(this.email, this.password, this.emailDirty, this.passwordDirty);
  }

  onSubmit(e: Event) {
    e.preventDefault();
    this.errorMessage = '';
    this.emailDirty = true;
    this.passwordDirty = true;

    if (!this.isEmailValid()) {
      this.errorMessage = 'Please enter a valid email address.';
      return;
    }
    if (!this.isPasswordValid()) {
      this.errorMessage = 'Password must be at least 8 characters.';
      return;
    }

    this.isLoading = true;
    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: () => {
        this.isLoading = false;
        if (this.authService.hasRole('Admin')) {
          this.router.navigate(['/admin']);
        } else if (this.authService.hasRole('Waiter')) {
          this.router.navigate(['/waiter']);
        } else {
          this.router.navigate(['/restaurants']);
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage =
          err?.error?.detail ?? err?.error?.message ?? 'Login failed. Please check your credentials.';
      },
    });
  }
}
