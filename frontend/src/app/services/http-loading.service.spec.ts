import { HttpLoadingService } from './http-loading.service';

describe('HttpLoadingService', () => {
  it('mantém ativo enquanto existem requisições e não fica negativo', () => {
    const service = new HttpLoadingService();
    expect(service.active()).toBe(false);
    service.start(); service.start(); service.finish();
    expect(service.active()).toBe(true);
    service.finish(); service.finish();
    expect(service.active()).toBe(false);
    service.start(); expect(service.active()).toBe(true);
  });
});
