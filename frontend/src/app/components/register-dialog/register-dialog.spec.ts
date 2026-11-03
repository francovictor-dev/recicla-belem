import { TestBed } from '@angular/core/testing';
import { MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { RegisterDialog } from './register-dialog';
import { AuthService } from '../../services/auth.service.ts';

describe('RegisterDialog', () => {
  const auth = { register: vi.fn(), signIn: vi.fn() };
  const dialog = { close: vi.fn(), disableClose: false };
  const toast = { open: vi.fn() };
  const router = { navigate: vi.fn() };
  let component: RegisterDialog;
  beforeEach(() => {
    vi.resetAllMocks();
    TestBed.configureTestingModule({ providers: [
      { provide: AuthService, useValue: auth }, { provide: MatDialogRef, useValue: dialog },
      { provide: MatSnackBar, useValue: toast }, { provide: Router, useValue: router },
    ] });
    component = TestBed.runInInjectionContext(() => new RegisterDialog());
  });
  const valid = () => component.form.setValue({ name: 'Nome', email: 'a@example.com', password: 'password123', confirmPassword: 'password123' });
  it('valida antes de enviar', async () => {
    await component.submit();
    expect(auth.register).not.toHaveBeenCalled();
    valid(); component.form.controls.confirmPassword.setValue('outrasenha');
    await component.submit();
    expect(auth.register).not.toHaveBeenCalled();
  });
  it('mostra loading, impede duplicação e autentica antes de navegar', async () => {
    valid(); const pending = new Subject<object>();
    auth.register.mockReturnValue(pending); auth.signIn.mockResolvedValue(undefined);
    const promise = component.submit();
    expect(component.loading()).toBe(true);
    await component.submit(); expect(auth.register).toHaveBeenCalledTimes(1);
    pending.next({}); await promise;
    expect(auth.signIn).toHaveBeenCalled();
    expect(toast.open).toHaveBeenCalledWith('Cadastro realizado com sucesso!', 'Fechar', { duration: 5000, verticalPosition: 'top', horizontalPosition: 'center' });
    expect(router.navigate).toHaveBeenCalledWith(['/mapa']);
    expect(component.loading()).toBe(false);
  });
  it('mostra toast de erro sem navegar', async () => {
    valid(); auth.register.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 409 })));
    await component.submit();
    expect(toast.open).toHaveBeenCalledWith('Este e-mail já está cadastrado.', 'Fechar', { duration: 7000, verticalPosition: 'top', horizontalPosition: 'center' });
    expect(router.navigate).not.toHaveBeenCalled();
    expect(component.loading()).toBe(false);
  });
  it('informa quando a conta foi criada mas login falhou', async () => {
    valid(); auth.register.mockReturnValue(of({})); auth.signIn.mockRejectedValue(new Error());
    await component.submit();
    expect(dialog.close).toHaveBeenCalled(); expect(router.navigate).not.toHaveBeenCalled();
  });
  it('rejeita arquivos que não são PNG ou excedem 300 KB', () => {
    for (const file of [new File(['a'], 'a.jpg', { type: 'image/jpeg' }), new File([new Uint8Array(307201)], 'a.png', { type: 'image/png' })]) {
      component.onImageSelected({ target: { files: [file], value: 'arquivo' } } as unknown as Event);
      expect(component.imageError).toBeTruthy(); expect(component.selectedImage).toBeNull();
    }
  });
});
