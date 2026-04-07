import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CardComponent } from './card.component';

describe('CardComponent', () => {
  let component: CardComponent;
  let fixture: ComponentFixture<CardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CardComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have default max-width of 420px', () => {
    expect(component.maxWidth).toBe('420px');
  });

  it('should apply custom max-width', () => {
    component.maxWidth = '600px';
    fixture.detectChanges();
    const card = fixture.nativeElement.querySelector('.card');
    expect(card.style.maxWidth).toBe('600px');
  });

  it('should project content via ng-content', () => {
    fixture.nativeElement.innerHTML = '<app-card><div class="test-content">Test</div></app-card>';
    fixture.detectChanges();
    const content = fixture.nativeElement.querySelector('.test-content');
    expect(content).toBeTruthy();
    expect(content.textContent).toBe('Test');
  });
});
