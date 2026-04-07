import { ComponentFixture, TestBed } from '@angular/core/testing';
import { IconComponent } from './icon.component';

describe('IconComponent', () => {
  let component: IconComponent;
  let fixture: ComponentFixture<IconComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IconComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(IconComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render with default size md', () => {
    expect(component.sizePixels).toBe(24);
  });

  it('should render with size lg', () => {
    component.size = 'lg';
    expect(component.sizePixels).toBe(32);
  });

  it('should render with size sm', () => {
    component.size = 'sm';
    expect(component.sizePixels).toBe(16);
  });

  it('should apply aria-label when provided', () => {
    component.ariaLabel = 'error icon';
    fixture.detectChanges();
    const icon = fixture.nativeElement.querySelector('tabler-icon');
    expect(icon.getAttribute('aria-label')).toBe('error icon');
  });

  it('should set aria-hidden when specified', () => {
    component.ariaHidden = true;
    fixture.detectChanges();
    const icon = fixture.nativeElement.querySelector('tabler-icon');
    expect(icon.getAttribute('aria-hidden')).toBe('true');
  });

  it('should use provided icon name', () => {
    component.name = 'alert-circle';
    expect(component.name).toBe('alert-circle');
  });

  it('should default to help-circle icon', () => {
    expect(component.name).toBe('help-circle');
  });
});
