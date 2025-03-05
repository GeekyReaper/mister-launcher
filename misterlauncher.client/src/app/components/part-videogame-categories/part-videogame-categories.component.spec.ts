import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PartVideogameCategoriesComponent } from './part-videogame-categories.component';

describe('PartVideogameCategoriesComponent', () => {
  let component: PartVideogameCategoriesComponent;
  let fixture: ComponentFixture<PartVideogameCategoriesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PartVideogameCategoriesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PartVideogameCategoriesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
