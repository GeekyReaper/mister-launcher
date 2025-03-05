import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MisterScriptComponent } from './mister-script.component';

describe('MisterScriptComponent', () => {
  let component: MisterScriptComponent;
  let fixture: ComponentFixture<MisterScriptComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MisterScriptComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MisterScriptComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
