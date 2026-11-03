import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service.ts';
import { AuthStore } from '../store/user.store';
import { environment } from '../../environments/environment';

describe('AuthService', () => {
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

  let service: AuthService;
  let http: HttpTestingController;
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => { http.verify(); localStorage.clear(); });
  it('envia cadastro multipart com imagem', () => {
    const image = new File(['png'], 'profile.png', { type: 'image/png' });
    service.register({ name: ' Nome ', email: 'a@example.com', password: 'password123' }, image).subscribe();
    const request = http.expectOne(`${environment.apiUrl}/api/User/default`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body.get('Profile.Name')).toBe('Nome');
    expect(request.request.body.get('User.Email')).toBe('a@example.com');
    expect(request.request.body.get('Image')).toBe(image);
    request.flush({});
  });
  it('autentica e preenche sessão com resposta camelCase', async () => {
    const promise = service.signIn({ email: 'a@example.com', password: 'password123' });
    http.expectOne(`${environment.apiUrl}/api/Auth/login`).flush({ accessToken: 'token' });
    await Promise.resolve();
    http.expectOne(`${environment.apiUrl}/api/Auth/me`).flush({ id: '1', name: 'Nome', email: 'a@example.com', profileImageUrl: null });
    await promise;
    expect(localStorage.getItem('token')).toBe('token');
    expect(TestBed.inject(AuthStore).user()?.name).toBe('Nome');
    service.logout();
    expect(localStorage.getItem('token')).toBeNull();
    expect(TestBed.inject(AuthStore).user()).toBeNull();
  });
  it('envia ID token para Auth/google e carrega sessão da aplicação', async () => {
    const promise = service.signInWithGoogle('google-id-token');
    const request = http.expectOne(`${environment.apiUrl}/api/Auth/google`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ idToken: 'google-id-token' });
    request.flush({ accessToken: 'application-jwt' });
    await Promise.resolve();
    http.expectOne(`${environment.apiUrl}/api/Auth/me`).flush({ id: '1', name: 'Maria', email: 'maria@gmail.com', profileImageUrl: 'https://example.com/photo.jpg' });
    await promise;
    expect(localStorage.getItem('token')).toBe('application-jwt');
    expect(TestBed.inject(AuthStore).isAuth()).toBe(true);
    expect(TestBed.inject(AuthStore).user()?.name).toBe('Maria');
  });
  it('limpa token e estado quando consulta do perfil Google falha', async () => {
    const promise = service.signInWithGoogle('google-id-token');
    const rejected = expect(promise).rejects.toBeTruthy();
    http.expectOne(`${environment.apiUrl}/api/Auth/google`).flush({ accessToken: 'application-jwt' });
    await Promise.resolve();
    http.expectOne(`${environment.apiUrl}/api/Auth/me`).flush({}, { status: 401, statusText: 'Unauthorized' });
    await rejected;
    expect(localStorage.getItem('token')).toBeNull();
    expect(TestBed.inject(AuthStore).user()).toBeNull();
  });

});
