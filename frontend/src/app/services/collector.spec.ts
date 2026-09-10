import { TestBed } from '@angular/core/testing';

import { Collector } from './collector';

describe('Collector', () => {
  let service: Collector;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Collector);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
