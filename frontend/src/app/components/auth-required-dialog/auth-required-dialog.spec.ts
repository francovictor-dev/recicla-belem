import { TestBed } from '@angular/core/testing';
import { MatDialogRef } from '@angular/material/dialog';
import { AuthRequiredDialog } from './auth-required-dialog';

describe('AuthRequiredDialog', () => {
  it.each([['Cancelar', false], ['Fazer login', true]])('retorna a decisão ao clicar em %s', async (label, result) => {
    const close = vi.fn();
    TestBed.configureTestingModule({ imports: [AuthRequiredDialog], providers: [{ provide: MatDialogRef, useValue: { close } }] });
    const fixture = TestBed.createComponent(AuthRequiredDialog); await fixture.whenStable();
    expect(fixture.nativeElement.textContent).toContain('É necessário estar autenticado');
    expect(fixture.nativeElement.querySelector('lucide-icon')).toBeTruthy();
    const button = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>).find(button => button.textContent?.includes(label as string));
    button!.click();
    expect(close).toHaveBeenCalledWith(result);
  });
});
