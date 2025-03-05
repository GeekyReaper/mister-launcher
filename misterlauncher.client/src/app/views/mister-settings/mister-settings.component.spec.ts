import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MisterSettingsComponent } from './mister-settings.component';

describe('MisterSettingsComponent', () => {
  let component: MisterSettingsComponent;
  let fixture: ComponentFixture<MisterSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MisterSettingsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MisterSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
