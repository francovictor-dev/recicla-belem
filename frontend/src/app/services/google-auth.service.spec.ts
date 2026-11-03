import { TestBed } from '@angular/core/testing';
import { GoogleAuthService, GoogleIdentityApi } from './google-auth.service';
import { environment } from '../../environments/environment';

describe('GoogleAuthService', () => {
  const original = environment.googleClientId;
  afterEach(() => { environment.googleClientId = original; vi.unstubAllGlobals(); });
  it('não carrega SDK quando Client ID não foi configurado', async () => {
    environment.googleClientId = '';
    const service = TestBed.inject(GoogleAuthService);
    await expect(service.render(document.createElement('div'), vi.fn(), () => true)).rejects.toThrow('Client ID');
  });
  it('inicializa uma vez e entrega credencial somente ao botão acionado', async () => {
    environment.googleClientId = 'test.apps.googleusercontent.com';
    const initialize = vi.fn(); const renderButton = vi.fn();
    vi.stubGlobal('google', { accounts: { id: { initialize, renderButton } as GoogleIdentityApi } });
    const service = TestBed.inject(GoogleAuthService);
    const first = vi.fn(); const second = vi.fn();
    const dispose = await service.render(document.createElement('div'), first, () => true);
    await service.render(document.createElement('div'), second, () => true);
    expect(initialize).toHaveBeenCalledTimes(1);
    renderButton.mock.calls[1][1].click_listener();
    initialize.mock.calls[0][0].callback({ credential: 'google-id-token' });
    expect(second).toHaveBeenCalledWith('google-id-token'); expect(first).not.toHaveBeenCalled();
    renderButton.mock.calls[0][1].click_listener(); dispose();
    initialize.mock.calls[0][0].callback({ credential: 'late-token' });
    expect(first).not.toHaveBeenCalled();
  });
  it('não renderiza quando componente já foi destruído', async () => {
    environment.googleClientId = 'test.apps.googleusercontent.com';
    const renderButton = vi.fn();
    vi.stubGlobal('google', { accounts: { id: { initialize: vi.fn(), renderButton } } });
    await TestBed.inject(GoogleAuthService).render(document.createElement('div'), vi.fn(), () => false);
    expect(renderButton).not.toHaveBeenCalled();
  });
  it('compartilha carregamento do script e propaga falha de rede', async () => {
    environment.googleClientId = 'test.apps.googleusercontent.com';
    vi.stubGlobal('google', undefined);
    const service = TestBed.inject(GoogleAuthService);
    const first = service.render(document.createElement('div'), vi.fn(), () => true);
    const second = service.render(document.createElement('div'), vi.fn(), () => true);
    const scripts = document.querySelectorAll<HTMLScriptElement>('script[src="https://accounts.google.com/gsi/client"]');
    expect(scripts.length).toBe(1);
    scripts[0].dispatchEvent(new Event('error'));
    await expect(first).rejects.toThrow('carregar'); await expect(second).rejects.toThrow('carregar');
  });
});
