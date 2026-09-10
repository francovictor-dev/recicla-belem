import { TestBed } from '@angular/core/testing';

import { CollectorPoint } from './collector-point';

describe('CollectorPoint', () => {
  let service: CollectorPoint;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CollectorPoint);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
