import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModaleditSystemComponent } from './modaledit-system.component';

describe('ModaleditSystemComponent', () => {
  let component: ModaleditSystemComponent;
  let fixture: ComponentFixture<ModaleditSystemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ModaleditSystemComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModaleditSystemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
