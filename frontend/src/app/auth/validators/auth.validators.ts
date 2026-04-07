/**
 * Authentication Form Validators
 * Reusable validation functions for login and registration forms
 */

/**
 * Password strength rules interface
 */
export interface PasswordStrengthRules {
  minLength: boolean;
  hasUppercase: boolean;
  hasLowercase: boolean;
  hasDigit: boolean;
  hasSpecialChar: boolean;
}

/**
 * Validates email format using a standard email pattern
 * @param email - The email address to validate
 * @returns true if email is valid, false otherwise
 */
export function isValidEmail(email: string): boolean {
  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailPattern.test(email);
}

/**
 * Validates that email is not empty and has valid format
 * @param email - The email address to validate
 * @returns true if email is present and valid, false otherwise
 */
export function isEmailValid(email: string): boolean {
  return email.length > 0 && isValidEmail(email);
}

/**
 * Validates name (first or last) - at least 2 characters
 * @param name - The name to validate
 * @returns true if name is valid, false otherwise
 */
export function isNameValid(name: string): boolean {
  return name.trim().length >= 2;
}

/**
 * Validates basic password length (no longer used for registration)
 * @param password - The password to validate
 * @param minLength - Minimum required password length (default: 8)
 * @returns true if password meets minimum length requirement, false otherwise
 */
export function isPasswordValid(password: string, minLength: number = 8): boolean {
  return password.length >= minLength;
}

/**
 * Validates password against all strength rules
 * @param password - The password to validate
 * @returns Object containing results of all strength checks
 */
export function getPasswordStrength(password: string): PasswordStrengthRules {
  return {
    minLength: password.length >= 8,
    hasUppercase: /[A-Z]/.test(password),
    hasLowercase: /[a-z]/.test(password),
    hasDigit: /\d/.test(password),
    hasSpecialChar: /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password),
  };
}

/**
 * Checks if all password strength rules are met
 * @param password - The password to validate
 * @returns true if all strength requirements are met, false otherwise
 */
export function isPasswordStrong(password: string): boolean {
  const strength = getPasswordStrength(password);
  return (
    strength.minLength &&
    strength.hasUppercase &&
    strength.hasLowercase &&
    strength.hasDigit &&
    strength.hasSpecialChar
  );
}

/**
 * Validates entire login form
 * @param email - The email address to validate
 * @param password - The password to validate
 * @param emailDirty - Whether email field has been touched
 * @param passwordDirty - Whether password field has been touched
 * @returns true if all fields are dirty and valid, false otherwise
 */
export function isLoginFormValid(
  email: string,
  password: string,
  emailDirty: boolean,
  passwordDirty: boolean
): boolean {
  return isEmailValid(email) && isPasswordValid(password) && emailDirty && passwordDirty;
}

/**
 * Validates that two passwords match
 * @param password - The password to compare
 * @param confirmPassword - The confirm password field to compare
 * @returns true if passwords match, false otherwise
 */
export function isPasswordsMatch(password: string, confirmPassword: string): boolean {
  return password === confirmPassword;
}
