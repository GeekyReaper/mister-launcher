import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FilterVideogameComponent } from './filter-videogame.component';

describe('FilterVideogameComponent', () => {
  let component: FilterVideogameComponent;
  let fixture: ComponentFixture<FilterVideogameComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [FilterVideogameComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FilterVideogameComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
