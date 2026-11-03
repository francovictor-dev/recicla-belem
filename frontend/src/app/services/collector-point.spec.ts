import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CollectorPointService } from './collector-point';
import { environment } from '../../environments/environment';

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
