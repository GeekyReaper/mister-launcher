import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RomLinkComponent } from './rom-link.component';

describe('RomLinkComponent', () => {
  let component: RomLinkComponent;
  let fixture: ComponentFixture<RomLinkComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [RomLinkComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RomLinkComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
