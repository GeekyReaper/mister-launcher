import { TestBed } from '@angular/core/testing';

import { MisterSignalrService } from './mister-signalr.service';

describe('MisterSignalrService', () => {
  let service: MisterSignalrService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MisterSignalrService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
