import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, Subject, throwError } from 'rxjs';
import { ReportDialog } from './report-dialog';
import { ReportService } from '../../services/report.service';
import { AuthStore } from '../../store/user.store';

describe('ReportDialog', () => {
  const create = vi.fn(); const close = vi.fn(); const toast = { open: vi.fn() };
  beforeEach(() => {
    vi.resetAllMocks();
    TestBed.configureTestingModule({ imports: [ReportDialog], providers: [
      { provide: MAT_DIALOG_DATA, useValue: { collectionPointId: 'point-id' } },
      { provide: MatDialogRef, useValue: { close, disableClose: false } },
      { provide: ReportService, useValue: { create } }, { provide: MatSnackBar, useValue: toast },
    ] });
    const auth = TestBed.inject(AuthStore); auth.setAuth(true);
    auth.setUser({ id: 'user-id', name: 'Maria', email: 'm@example.com', profileImageUrl: null });
  });
  it('renderiza descrição multiline e três tipos de denúncia', async () => {
    const fixture = TestBed.createComponent(ReportDialog); await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('textarea')).toBeTruthy();
    expect(fixture.nativeElement.querySelectorAll('select option').length).toBe(4);
    await fixture.componentInstance.submit(); expect(create).not.toHaveBeenCalled();
  });
  it('envia dados corretos e mostra sucesso', async () => {
    create.mockReturnValue(of({}));
    const fixture = TestBed.createComponent(ReportDialog);
    fixture.componentInstance.form.setValue({ message: ' Problema encontrado ', reportType: 0 });
    await fixture.componentInstance.submit();
    expect(create).toHaveBeenCalledWith({ message: 'Problema encontrado', reportType: 0, userId: 'user-id', collectionPointId: 'point-id' });
    expect(close).toHaveBeenCalledWith(true);
    expect(toast.open).toHaveBeenCalledWith('Denúncia enviada com sucesso!', 'Fechar', expect.objectContaining({ verticalPosition: 'top' }));
  });
  it('bloqueia envio duplicado e mantém formulário em erro', async () => {
    const pending = new Subject<object>(); create.mockReturnValue(pending);
    const fixture = TestBed.createComponent(ReportDialog);
    fixture.componentInstance.form.setValue({ message: 'Problema', reportType: 2 });
    const promise = fixture.componentInstance.submit(); await fixture.componentInstance.submit();
    expect(create).toHaveBeenCalledTimes(1);
    expect(fixture.componentInstance.submitting()).toBe(true);
    pending.error(new Error('Falha')); await promise;
    expect(fixture.componentInstance.submitting()).toBe(false); expect(close).not.toHaveBeenCalled();
    expect(toast.open).toHaveBeenCalledWith(expect.stringContaining('Não foi possível'), 'Fechar', expect.any(Object));
  });
  it('não envia sem usuário autenticado', async () => {
    TestBed.inject(AuthStore).clearAll();
    const fixture = TestBed.createComponent(ReportDialog);
    fixture.componentInstance.form.setValue({ message: 'Problema', reportType: 1 });
    await fixture.componentInstance.submit(); expect(create).not.toHaveBeenCalled();
    expect(toast.open).toHaveBeenCalled();
  });
});
