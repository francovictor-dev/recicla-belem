import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CollectorPointService } from './collector-point';
import { environment } from '../../environments/environment';

describe('CollectorPointService', () => {
  it('envia arquivos e ids repetidos e consulta pontos com endereço', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const service = TestBed.inject(CollectorPointService);
    const http = TestBed.inject(HttpTestingController);
    const images = [new File(['a'], 'a.png', { type: 'image/png' }), new File(['b'], 'b.png', { type: 'image/png' })];
    service.createCollectorPoint({ lat: -1, lng: -48, collectorIds: ['id1', 'id2'] }, images).subscribe();
    const post = http.expectOne(req => req.url === `${environment.apiUrl}/api/CollectionPoint` && req.method === 'POST');
    expect(post.request.body.get('Lat')).toBe('-1');
    expect(post.request.body.getAll('CollectorIds')).toEqual(['id1', 'id2']);
    expect(post.request.body.getAll('Images')).toEqual(images);
    post.flush({});
    service.getAll().subscribe();
    const get = http.expectOne(req => req.method === 'GET');
    expect(get.request.params.get('include')).toBe('address,collector,image'); get.flush([]);
    http.verify();
  });
});
