import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GuestAccessComponent } from './GuestAccessComponent';

describe('GuestAccessComponent', () => {
  let component: GuestAccessComponent;
  let fixture: ComponentFixture<GuestAccessComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [GuestAccessComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GuestAccessComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
