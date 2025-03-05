import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PartVideogameLaunchbuttonComponent } from './part-videogame-launchbutton.component';

describe('PartVideogameLaunchbuttonComponent', () => {
  let component: PartVideogameLaunchbuttonComponent;
  let fixture: ComponentFixture<PartVideogameLaunchbuttonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PartVideogameLaunchbuttonComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PartVideogameLaunchbuttonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
