import { TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogRef } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { GoogleSignIn } from './google-sign-in';
import { GoogleAuthService } from '../../services/google-auth.service';
import { AuthService } from '../../services/auth.service.ts';

describe('GoogleSignIn', () => {
  const toast = { open: vi.fn() };
  const render = vi.fn();
  const auth = { signInWithGoogle: vi.fn() };
  const router = { navigate: vi.fn() };
  const dialog = { close: vi.fn(), disableClose: false };
  let received: (credential: string) => void;
  const dispose = vi.fn();
  beforeEach(() => {
    vi.resetAllMocks(); dialog.disableClose = false;
    render.mockImplementation(async (_element, callback) => { received = callback; return dispose; });
    auth.signInWithGoogle.mockResolvedValue(undefined);
    router.navigate.mockResolvedValue(true);
  });
  function configure(configured = true) {
    TestBed.configureTestingModule({ providers: [
      { provide: GoogleAuthService, useValue: { configured, render } },
      { provide: MatSnackBar, useValue: toast },
      { provide: AuthService, useValue: auth },
      { provide: Router, useValue: router },
      { provide: MatDialogRef, useValue: dialog },
    ] });
    return TestBed.createComponent(GoogleSignIn);
  }
  it('avisa indisponibilidade ao clicar sem configuração', async () => {
    const fixture = configure(false); await fixture.whenStable();
    fixture.nativeElement.querySelector('button').click();
    expect(render).not.toHaveBeenCalled(); expect(toast.open).toHaveBeenCalled();
    expect(auth.signInWithGoogle).not.toHaveBeenCalled();
  });
  it('autentica credencial, fecha cadastro e navega para mapa', async () => {
    const fixture = configure(); await fixture.whenStable();
    received('google-id-token');
    await vi.waitFor(() => expect(router.navigate).toHaveBeenCalledWith(['/mapa']));
    expect(auth.signInWithGoogle).toHaveBeenCalledWith('google-id-token');
    expect(dialog.close).toHaveBeenCalledWith(true);
    expect(toast.open).toHaveBeenCalledWith(expect.stringContaining('sucesso'), 'Fechar', expect.objectContaining({ verticalPosition: 'top' }));
    fixture.destroy(); expect(dispose).toHaveBeenCalled();
  });
  it('mostra loading e impede duplicação enquanto autentica', async () => {
    let complete!: () => void;
    auth.signInWithGoogle.mockReturnValue(new Promise<void>(resolve => { complete = resolve; }));
    const fixture = configure(); await fixture.whenStable();
    received('token'); received('token'); fixture.detectChanges();
    expect(fixture.componentInstance.loading()).toBe(true);
    expect(fixture.nativeElement.querySelector('app-loading')).toBeTruthy();
    expect(auth.signInWithGoogle).toHaveBeenCalledTimes(1);
    expect(dialog.disableClose).toBe(true);
    complete();
    await vi.waitFor(() => expect(fixture.componentInstance.loading()).toBe(false));
    expect(dialog.disableClose).toBe(false);
  });
  it.each([0, 401, 500])('mostra erro HTTP %s sem navegar ou fechar cadastro', async status => {
    auth.signInWithGoogle.mockRejectedValue(new HttpErrorResponse({ status, error: { message: 'Conta não autorizada.' } }));
    const fixture = configure(); await fixture.whenStable();
    received('token');
    await vi.waitFor(() => expect(toast.open).toHaveBeenCalled());
    expect(router.navigate).not.toHaveBeenCalled(); expect(dialog.close).not.toHaveBeenCalled();
    expect(fixture.componentInstance.loading()).toBe(false);
  });
});
