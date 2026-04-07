import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DividerComponent } from './divider.component';

describe('DividerComponent', () => {
  let component: DividerComponent;
  let fixture: ComponentFixture<DividerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DividerComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DividerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render single line without text', () => {
    component.text = undefined;
    fixture.detectChanges();
    const lines = fixture.nativeElement.querySelectorAll('.divider-line');
    expect(lines.length).toBe(1);
  });

  it('should render two lines with text', () => {
    component.text = 'Or';
    fixture.detectChanges();
    const lines = fixture.nativeElement.querySelectorAll('.divider-line');
    expect(lines.length).toBe(2);
  });

  it('should display text when provided', () => {
    component.text = 'Continue with';
    fixture.detectChanges();
    const textEl = fixture.nativeElement.querySelector('.divider-text');
    expect(textEl.textContent).toBe('Continue with');
  });
});
