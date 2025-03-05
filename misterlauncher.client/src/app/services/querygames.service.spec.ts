
import { TestBed } from '@angular/core/testing';

import { QuerygamesService } from './QuerygamesService';

describe('QuerygamesService', () => {
  let service: QuerygamesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(QuerygamesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
