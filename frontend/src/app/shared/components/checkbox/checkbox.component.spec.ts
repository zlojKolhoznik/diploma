import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CheckboxComponent } from './checkbox.component';

describe('CheckboxComponent', () => {
  let component: CheckboxComponent;
  let fixture: ComponentFixture<CheckboxComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CheckboxComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CheckboxComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display label text', () => {
    component.label = 'Remember me';
    fixture.detectChanges();
    const label = fixture.nativeElement.querySelector('.checkbox-text');
    expect(label.textContent).toBe('Remember me');
  });

  it('should emit change event when toggled', () => {
    let changedValue: boolean | undefined;
    component.checkedChange.subscribe((checked) => {
      changedValue = checked;
    });
    component.onCheckboxChange(true);
    expect(changedValue).toBe(true);
  });

  it('should be disabled when disabled input is true', () => {
    component.disabled = true;
    fixture.detectChanges();
    const input = fixture.nativeElement.querySelector('input');
    expect(input.disabled).toBe(true);
  });

  it('should set checked state', () => {
    component.checked = true;
    fixture.detectChanges();
    const input = fixture.nativeElement.querySelector('input');
    expect(input.checked).toBe(true);
  });
});
