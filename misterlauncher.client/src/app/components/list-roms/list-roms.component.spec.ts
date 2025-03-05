import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListRomsComponent } from './list-roms.component';

describe('ListRomsComponent', () => {
  let component: ListRomsComponent;
  let fixture: ComponentFixture<ListRomsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ListRomsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListRomsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
