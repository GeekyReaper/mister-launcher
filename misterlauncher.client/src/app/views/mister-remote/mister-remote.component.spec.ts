import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MisterRemoteComponent } from './mister-remote.component';

describe('MisterRemoteComponent', () => {
  let component: MisterRemoteComponent;
  let fixture: ComponentFixture<MisterRemoteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MisterRemoteComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MisterRemoteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
