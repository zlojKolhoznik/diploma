import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LogoComponent } from './logo.component';

describe('LogoComponent', () => {
  let component: LogoComponent;
  let fixture: ComponentFixture<LogoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LogoComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(LogoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render with default size md', () => {
    expect(component.sizeClass).toBe('logo-md');
  });

  it('should render with size sm', () => {
    component.size = 'sm';
    expect(component.sizeClass).toBe('logo-sm');
  });

  it('should render with size lg', () => {
    component.size = 'lg';
    expect(component.sizeClass).toBe('logo-lg');
  });

  it('should set src and alt attributes', () => {
    component.src = '/assets/custom-logo.png';
    component.alt = 'Custom Logo';
    fixture.detectChanges();
    const img = fixture.nativeElement.querySelector('img');
    expect(img.src).toContain('custom-logo.png');
    expect(img.alt).toBe('Custom Logo');
  });
});
