import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AlertComponent } from './alert.component';

describe('AlertComponent', () => {
  let component: AlertComponent;
  let fixture: ComponentFixture<AlertComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AlertComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AlertComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display message', () => {
    component.message = 'Test error';
    fixture.detectChanges();
    const message = fixture.nativeElement.querySelector('.alert-message');
    expect(message.textContent).toBe('Test error');
  });

  it('should apply error variant class', () => {
    component.variant = 'error';
    fixture.detectChanges();
    const alert = fixture.nativeElement.querySelector('div');
    expect(alert.classList.contains('alert-error')).toBe(true);
  });

  it('should apply success variant class', () => {
    component.variant = 'success';
    fixture.detectChanges();
    const alert = fixture.nativeElement.querySelector('div');
    expect(alert.classList.contains('alert-success')).toBe(true);
  });

  it('should show close button when dismissible is true', () => {
    component.dismissible = true;
    fixture.detectChanges();
    const closeBtn = fixture.nativeElement.querySelector('.alert-close');
    expect(closeBtn).toBeTruthy();
  });

  it('should emit dismiss event when close button clicked', () => {
    component.dismissible = true;
    let dismissCalled = false;
    component.dismiss.subscribe(() => {
      dismissCalled = true;
    });
    fixture.detectChanges();
    const closeBtn = fixture.nativeElement.querySelector('.alert-close');
    closeBtn.click();
    expect(dismissCalled).toBe(true);
  });

  it('should have role alert', () => {
    fixture.detectChanges();
    const alert = fixture.nativeElement.querySelector('div');
    expect(alert.getAttribute('role')).toBe('alert');
  });

  it('should use appropriate icon for error variant', () => {
    component.variant = 'error';
    expect(component.iconName).toBe('alert-circle');
  });

  it('should use appropriate icon for success variant', () => {
    component.variant = 'success';
    expect(component.iconName).toBe('check-circle');
  });
});
