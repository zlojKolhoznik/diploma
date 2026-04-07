import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LinkComponent } from './link.component';

describe('LinkComponent', () => {
  let component: LinkComponent;
  let fixture: ComponentFixture<LinkComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LinkComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(LinkComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render with default variant', () => {
    expect(component.variantClass).toBe('link-default');
  });

  it('should render with ghost variant', () => {
    component.variant = 'ghost';
    expect(component.variantClass).toBe('link-ghost');
  });

  it('should set href attribute', () => {
    component.href = 'https://example.com';
    component.routerLink = undefined;
    fixture.detectChanges();
    const link = fixture.nativeElement.querySelector('a');
    expect(link.href).toContain('example.com');
  });

  it('should open external links in new tab', () => {
    component.href = 'https://external.com';
    component.external = true;
    component.routerLink = undefined;
    fixture.detectChanges();
    const link = fixture.nativeElement.querySelector('a');
    expect(link.target).toBe('_blank');
    expect(link.rel).toContain('noopener');
  });
});
