import { ComponentFixture, TestBed } from '@angular/core/testing';
import { InputComponent } from './input.component';

describe('InputComponent', () => {
  let component: InputComponent;
  let fixture: ComponentFixture<InputComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InputComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(InputComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render with default type text', () => {
    expect(component.type).toBe('text');
  });

  it('should emit valueChange on input', () => {
    let changedValue: string | undefined;
    component.valueChange.subscribe((value) => {
      changedValue = value;
    });
    component.onValueChange('test');
    expect(changedValue).toBe('test');
  });

  it('should display label when provided', () => {
    component.label = 'Email Address';
    fixture.detectChanges();
    const label = fixture.nativeElement.querySelector('.form-label');
    expect(label.textContent).toContain('Email Address');
  });

  it('should display error message when provided', () => {
    component.error = 'Invalid email';
    fixture.detectChanges();
    const errorEl = fixture.nativeElement.querySelector('.form-error-text');
    expect(errorEl).toBeTruthy();
    expect(errorEl.textContent).toBe('Invalid email');
  });

  it('should apply error class when error is present', () => {
    component.error = 'Error message';
    fixture.detectChanges();
    const input = fixture.nativeElement.querySelector('.form-input');
    expect(input.classList.contains('form-input-error')).toBe(true);
  });

  it('should emit blur event', () => {
    let blurred = false;
    component.blur.subscribe(() => {
      blurred = true;
    });
    component.onBlur();
    expect(blurred).toBe(true);
  });

  it('should set input type', () => {
    component.type = 'email';
    fixture.detectChanges();
    const input = fixture.nativeElement.querySelector('input');
    expect(input.type).toBe('email');
  });

  it('should display required indicator when required is true', () => {
    component.label = 'Email';
    component.required = true;
    fixture.detectChanges();
    const indicator = fixture.nativeElement.querySelector('.required-indicator');
    expect(indicator).toBeTruthy();
  });
});
