import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ItemlistVideogameComponent } from './itemlist-videogame.component';

describe('ItemlistVideogameComponent', () => {
  let component: ItemlistVideogameComponent;
  let fixture: ComponentFixture<ItemlistVideogameComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ItemlistVideogameComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ItemlistVideogameComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
