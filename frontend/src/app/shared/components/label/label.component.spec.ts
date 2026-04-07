import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LabelComponent } from './label.component';

describe('LabelComponent', () => {
  let component: LabelComponent;
  let fixture: ComponentFixture<LabelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LabelComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(LabelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set htmlFor attribute', () => {
    component.htmlFor = 'test-input';
    fixture.detectChanges();
    const label = fixture.nativeElement.querySelector('label');
    expect(label.getAttribute('for')).toBe('test-input');
  });

  it('should display required indicator when required is true', () => {
    component.required = true;
    fixture.detectChanges();
    const indicator = fixture.nativeElement.querySelector('.required-indicator');
    expect(indicator).toBeTruthy();
    expect(indicator.textContent).toBe('*');
  });

  it('should not display required indicator when required is false', () => {
    component.required = false;
    fixture.detectChanges();
    const indicator = fixture.nativeElement.querySelector('.required-indicator');
    expect(indicator).toBeFalsy();
  });

  it('should project label text via ng-content', () => {
    fixture.nativeElement.innerHTML = '<app-label>Email Address</app-label>';
    fixture.detectChanges();
    const label = fixture.nativeElement.querySelector('label');
    expect(label.textContent).toContain('Email Address');
  });
});
