import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JobScanComponent } from './job-scan.component';

describe('JobScanComponent', () => {
  let component: JobScanComponent;
  let fixture: ComponentFixture<JobScanComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [JobScanComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobScanComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
