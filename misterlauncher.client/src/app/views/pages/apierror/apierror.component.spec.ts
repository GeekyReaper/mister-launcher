import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ApierrorComponent } from './apierror.component';

describe('ApierrorComponent', () => {
  let component: ApierrorComponent;
  let fixture: ComponentFixture<ApierrorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ApierrorComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ApierrorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
