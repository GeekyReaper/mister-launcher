import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SelectRomComponent } from './select-rom.component';

describe('SelectRomComponent', () => {
  let component: SelectRomComponent;
  let fixture: ComponentFixture<SelectRomComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SelectRomComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SelectRomComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
