import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JobScanRomComponent } from './JobScanRomComponent';

describe('JobScanRomComponent', () => {
  let component: JobScanRomComponent;
  let fixture: ComponentFixture<JobScanRomComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [JobScanRomComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobScanRomComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
