import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideogamePlaylistComponent } from './videogame-playlist.component';

describe('VideogamePlaylistComponent', () => {
  let component: VideogamePlaylistComponent;
  let fixture: ComponentFixture<VideogamePlaylistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [VideogamePlaylistComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideogamePlaylistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
