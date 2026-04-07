import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
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
  email: string = '';
  password: string = '';
  isLoading: boolean = false;
  errorMessage: string = '';
  rememberMe: boolean = false;
  emailDirty: boolean = false;
  passwordDirty: boolean = false;

  onEmailBlur() {
    this.emailDirty = true;
  }

  onPasswordBlur() {
    this.passwordDirty = true;
  }

  isEmailValid(): boolean {
    return isEmailValid(this.email);
  }

  isPasswordValid(): boolean {
    return isPasswordValid(this.password);
  }

  isFormValid(): boolean {
    return isLoginFormValid(this.email, this.password, this.emailDirty, this.passwordDirty);
  }

  onSubmit(e: Event) {
    e.preventDefault();
    this.errorMessage = '';

    // Mark fields as dirty before validation
    this.emailDirty = true;
    this.passwordDirty = true;

    // Validation
    if (!this.isEmailValid()) {
      this.errorMessage = 'Please enter a valid email address.';
      return;
    }

    if (!this.isPasswordValid()) {
      this.errorMessage = 'Password must be at least 8 characters.';
      return;
    }

    // TODO: Call auth service to submit login
    console.log('Login attempt:', { email: this.email, password: this.password });
    this.isLoading = true;

    // Placeholder: simulate API call
    setTimeout(() => {
      this.isLoading = false;
      // Replace with actual auth service call
      console.log('Login would be processed here');
    }, 1000);
  }
}
