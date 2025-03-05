import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ItemlistSystemComponent } from './itemlist-system.component';

describe('ItemlistSystemComponent', () => {
  let component: ItemlistSystemComponent;
  let fixture: ComponentFixture<ItemlistSystemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ItemlistSystemComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ItemlistSystemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
