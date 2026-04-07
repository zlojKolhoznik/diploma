import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import {
  isEmailValid,
  isNameValid,
  isPasswordStrong,
  isPasswordsMatch,
  getPasswordStrength,
  type PasswordStrengthRules,
} from '../../validators/auth.validators';
import {
  LogoComponent,
  AlertComponent,
  InputComponent,
  ButtonComponent,
  DividerComponent,
  CardComponent,
  LinkComponent,
} from '../../../shared/components';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    LogoComponent,
    AlertComponent,
    InputComponent,
    ButtonComponent,
    DividerComponent,
    CardComponent,
    LinkComponent,
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
  host: {
    class: 'auth-page-host',
  },
})
export class RegisterComponent {
  // Form fields
  email: string = '';
  password: string = '';
  confirmPassword: string = '';
  firstName: string = '';
  lastName: string = '';
  
  // State tracking
  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';
  
  // Field touched tracking for validation display
  emailDirty: boolean = false;
  passwordDirty: boolean = false;
  confirmPasswordDirty: boolean = false;
  firstNameDirty: boolean = false;
  lastNameDirty: boolean = false;

  // Password strength tracking
  passwordStrength: PasswordStrengthRules = {
    minLength: false,
    hasUppercase: false,
    hasLowercase: false,
    hasDigit: false,
    hasSpecialChar: false,
  };

  // Track which fields have been touched
  onEmailBlur() {
    this.emailDirty = true;
  }

  onPasswordBlur() {
    this.passwordDirty = true;
  }

  onPasswordInput() {
    this.passwordDirty = true;
    this.passwordStrength = getPasswordStrength(this.password);
  }

  onConfirmPasswordBlur() {
    this.confirmPasswordDirty = true;
  }

  onFirstNameBlur() {
    this.firstNameDirty = true;
  }

  onLastNameBlur() {
    this.lastNameDirty = true;
  }

  // Validation methods
  isEmailValid(): boolean {
    return isEmailValid(this.email);
  }

  isPasswordValid(): boolean {
    return isPasswordStrong(this.password);
  }

  isConfirmPasswordValid(): boolean {
    return (
      isPasswordStrong(this.password) &&
      isPasswordsMatch(this.password, this.confirmPassword) &&
      this.confirmPassword.length > 0
    );
  }

  isFirstNameValid(): boolean {
    return isNameValid(this.firstName);
  }

  isLastNameValid(): boolean {
    return isNameValid(this.lastName);
  }

  isFormValid(): boolean {
    return (
      this.isEmailValid() &&
      this.isPasswordValid() &&
      this.isConfirmPasswordValid() &&
      this.isFirstNameValid() &&
      this.isLastNameValid() &&
      this.emailDirty &&
      this.passwordDirty &&
      this.confirmPasswordDirty &&
      this.firstNameDirty &&
      this.lastNameDirty
    );
  }

  onSubmit(e: Event) {
    e.preventDefault();
    this.errorMessage = '';
    this.successMessage = '';

    // Mark all fields as dirty before validation
    this.emailDirty = true;
    this.passwordDirty = true;
    this.confirmPasswordDirty = true;
    this.firstNameDirty = true;
    this.lastNameDirty = true;

    // Validation
    if (!this.isFirstNameValid()) {
      this.errorMessage = 'First name must be at least 2 characters.';
      return;
    }

    if (!this.isLastNameValid()) {
      this.errorMessage = 'Last name must be at least 2 characters.';
      return;
    }

    if (!this.isEmailValid()) {
      this.errorMessage = 'Please enter a valid email address.';
      return;
    }

    if (!this.isPasswordValid()) {
      this.errorMessage = 'Password does not meet all requirements.';
      return;
    }

    if (!this.isConfirmPasswordValid()) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    // TODO: Call auth service to submit registration
    console.log('Register attempt:', {
      firstName: this.firstName,
      lastName: this.lastName,
      email: this.email,
      password: this.password,
    });
    this.isLoading = true;

    // Placeholder: simulate API call
    setTimeout(() => {
      this.isLoading = false;
      this.successMessage = 'Account created successfully! Redirecting...';
      // Replace with actual auth service call
      console.log('Registration would be processed here');
    }, 1500);
  }
}
