import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Auth } from './auth';
import { provideRouter, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../services/auth.service.ts';

describe('Auth', () => {
  const signIn = vi.fn();
  const toast = { open: vi.fn() };
  let component: Auth;
  let fixture: ComponentFixture<Auth>;

  beforeEach(async () => {
    vi.resetAllMocks();
    await TestBed.configureTestingModule({
      imports: [Auth],
      providers: [provideRouter([]), { provide: AuthService, useValue: { signIn } }, { provide: MatSnackBar, useValue: toast }],
    }).overrideProvider(MatSnackBar, { useValue: toast }).compileComponents();

    fixture = TestBed.createComponent(Auth);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it.each([
    [401, 'E-mail ou senha inválidos.'],
    [0, 'Não foi possível entrar. Tente novamente.'],
    [500, 'Não foi possível entrar. Tente novamente.'],
  ])('mostra toast para erro HTTP %s e permanece no login', async (status, message) => {
    component.loginForm.setValue({ email: 'a@example.com', password: 'password123' });
    signIn.mockRejectedValue(new HttpErrorResponse({ status }));
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate');
    await component.submit();
    expect(toast.open).toHaveBeenCalledWith(message, 'Fechar', { duration: 7000, verticalPosition: 'top', horizontalPosition: 'center' });
    expect(navigate).not.toHaveBeenCalled();
  });

  it('não envia formulário inválido', async () => {
    await component.submit();
    expect(signIn).not.toHaveBeenCalled();
    expect(toast.open).not.toHaveBeenCalled();
  });

  it('navega para mapa após autenticar', async () => {
    component.loginForm.setValue({ email: 'a@example.com', password: 'password123' });
    signIn.mockResolvedValue(undefined);
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    await component.submit();
    expect(navigate).toHaveBeenCalledWith(['/mapa']);
    expect(toast.open).not.toHaveBeenCalled();
  });

  it('should create' , () => {
    expect(component).toBeTruthy();
  });
});
