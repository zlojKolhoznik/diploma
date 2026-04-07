import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi, expect } from 'vitest';
import { ButtonComponent } from './button.component';

describe('ButtonComponent', () => {
  let component: ButtonComponent;
  let fixture: ComponentFixture<ButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ButtonComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render with primary variant', () => {
    component.variant = 'primary';
    fixture.detectChanges();
    expect(component.buttonClasses).toContain('btn-primary');
  });

  it('should render with secondary variant', () => {
    component.variant = 'secondary';
    fixture.detectChanges();
    expect(component.buttonClasses).toContain('btn-secondary');
  });

  it('should emit click event when clicked', async () => {
    let emitted = false;
    component.click.subscribe(() => {
      emitted = true;
    });
    const button = fixture.nativeElement.querySelector('button');
    button.click();
    expect(emitted).toBe(true);
  });

  it('should not emit click when disabled', () => {
    const emitSpy = vi.spyOn(component.click, 'emit');
    component.disabled = true;
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button');
    button.click();
    expect(emitSpy).not.toHaveBeenCalled();
  });

  it('should be disabled when loading', () => {
    component.loading = true;
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button');
    expect(button.disabled).toBe(true);
  });

  it('should show loading spinner when loading', () => {
    component.loading = true;
    fixture.detectChanges();
    const spinner = fixture.nativeElement.querySelector('.btn-loading-spinner');
    expect(spinner).toBeTruthy();
  });

  it('should apply full width class when fullWidth is true', () => {
    component.fullWidth = true;
    fixture.detectChanges();
    expect(component.buttonClasses).toContain('btn-full-width');
  });
});
