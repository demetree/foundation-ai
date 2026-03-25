import { ComponentFixture, TestBed } from '@angular/core/testing';

import { QuickAddJobModalComponent } from './quick-add-job-modal.component';

describe('QuickAddJobModalComponent', () => {
  let component: QuickAddJobModalComponent;
  let fixture: ComponentFixture<QuickAddJobModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [QuickAddJobModalComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(QuickAddJobModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
