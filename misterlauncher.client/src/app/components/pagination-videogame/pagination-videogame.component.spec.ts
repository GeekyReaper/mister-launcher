import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PaginationVideogameComponent } from './pagination-videogame.component';

describe('PaginationVideogameComponent', () => {
  let component: PaginationVideogameComponent;
  let fixture: ComponentFixture<PaginationVideogameComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PaginationVideogameComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PaginationVideogameComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
