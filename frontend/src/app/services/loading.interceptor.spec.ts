import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { HttpLoadingService } from './http-loading.service';
import { loadingInterceptor } from './loading.interceptor';

describe('loadingInterceptor', () => {
  let client: HttpClient;
  let http: HttpTestingController;
  let loading: HttpLoadingService;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(withInterceptors([loadingInterceptor])), provideHttpClientTesting()] });
    client = TestBed.inject(HttpClient); http = TestBed.inject(HttpTestingController); loading = TestBed.inject(HttpLoadingService);
  });
  afterEach(() => http.verify());
  it('encerra somente quando todas as chamadas terminam', () => {
    client.get('/first').subscribe(); client.get('/second').subscribe();
    expect(loading.active()).toBe(true);
    http.expectOne('/first').flush({}); expect(loading.active()).toBe(true);
    http.expectOne('/second').flush({}); expect(loading.active()).toBe(false);
  });
  it('encerra em caso de erro', () => {
    client.get('/failed').subscribe({ error: () => {} });
    http.expectOne('/failed').flush({}, { status: 500, statusText: 'Server error' });
    expect(loading.active()).toBe(false);
  });
  it('encerra em cancelamento e conta novamente em nova inscrição', () => {
    const request = client.get('/cancel');
    expect(loading.active()).toBe(false);
    const subscription = request.subscribe();
    const pending = http.expectOne('/cancel');
    subscription.unsubscribe();
    expect(pending.cancelled).toBe(true); expect(loading.active()).toBe(false);
    request.subscribe(); expect(loading.active()).toBe(true);
    http.expectOne('/cancel').flush({}); expect(loading.active()).toBe(false);
  });
});
