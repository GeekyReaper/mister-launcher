import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModaleditVideogameComponent } from './modaledit-videogame.component';

describe('ModaleditVideogameComponent', () => {
  let component: ModaleditVideogameComponent;
  let fixture: ComponentFixture<ModaleditVideogameComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ModaleditVideogameComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModaleditVideogameComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
