import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { HttpLoadingService } from './services/http-loading.service';

describe('App', () => {
  beforeEach(() => TestBed.configureTestingModule({ imports: [App], providers: [provideRouter([])] }));
  it('exibe e remove overlay de loading global com estado acessível', async () => {
    const fixture = TestBed.createComponent(App); await fixture.whenStable();
    const loading = TestBed.inject(HttpLoadingService);
    expect(fixture.nativeElement.querySelector('.request-overlay')).toBeNull();
    loading.start(); await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('.request-overlay app-loading')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('.page-content').hasAttribute('inert')).toBe(true);
    expect(fixture.nativeElement.querySelector('.page-content').getAttribute('aria-busy')).toBe('true');
    loading.finish(); await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('.request-overlay')).toBeNull();
    expect(fixture.nativeElement.querySelector('.page-content').hasAttribute('inert')).toBe(false);
  });
});
