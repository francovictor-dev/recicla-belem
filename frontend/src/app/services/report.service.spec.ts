import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ReportService } from './report.service';
import { environment } from '../../environments/environment';

describe('ReportService', () => {
  it('envia denúncia vinculada ao ponto e usuário', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const data = { message: 'Local incorreto', reportType: 1, collectionPointId: 'point-id', userId: 'user-id' };
    TestBed.inject(ReportService).create(data).subscribe();
    const http = TestBed.inject(HttpTestingController);
    const request = http.expectOne(`${environment.apiUrl}/api/Report`);
    expect(request.request.method).toBe('POST'); expect(request.request.body).toEqual(data);
    request.flush({}); http.verify();
  });
});
