import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CollectorService } from './collector';
import { environment } from '../../environments/environment';

describe('CollectorService', () => {
  it('consulta a rota do backend e compartilha a lista carregada', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const service = TestBed.inject(CollectorService);
    const http = TestBed.inject(HttpTestingController);
    const values = [{ id: 'guid', name: 'Vidro', color: '#008000', description: '' }];
    service.getCollectors().subscribe(result => expect(result).toEqual(values));
    http.expectOne(`${environment.apiUrl}/api/Collector`).flush(values);
    service.getCollectors().subscribe(result => expect(result).toEqual(values));
    http.expectNone(`${environment.apiUrl}/api/Collector`);
    http.verify();
  });
});
