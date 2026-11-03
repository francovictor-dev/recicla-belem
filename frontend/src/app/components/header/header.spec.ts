import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { Header } from './header';
import { AuthService } from '../../services/auth.service.ts';
import { AuthStore } from '../../store/user.store';

describe('Header', () => {
  beforeEach(() => {
    const values = new Map<string, string>();
    vi.stubGlobal('localStorage', {
      getItem: (key: string) => values.get(key) ?? null,
      setItem: (key: string, value: string) => { values.set(key, value); },
      removeItem: (key: string) => { values.delete(key); },
      clear: () => values.clear(),
    });
  });
  afterEach(() => vi.unstubAllGlobals());

  beforeEach(() => { localStorage.clear(); TestBed.configureTestingModule({ imports: [Header], providers: [provideRouter([]), { provide: AuthService, useValue: { logout: vi.fn() } }] }); });
  it('mostra nome e foto no botão de conta', async () => {
    const store = TestBed.inject(AuthStore);
    store.setAuth(true); store.setUser({ id: '1', name: 'Maria', email: 'a@example.com', profileImageUrl: 'https://example.com/profile.png' });
    const fixture = TestBed.createComponent(Header); await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('.profile-button').textContent).toContain('Maria');
    expect(fixture.nativeElement.querySelector('.avatar')).toBeTruthy();
  });
  it('limpa sessão e volta para login no logout', () => {
    const fixture = TestBed.createComponent(Header);
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture.componentInstance.logout();
    expect(TestBed.inject(AuthService).logout).toHaveBeenCalled();
    expect(TestBed.inject(AuthStore).user()).toBeNull();
    expect(navigate).toHaveBeenCalledWith(['/autenticacao/login']);
  });
});
