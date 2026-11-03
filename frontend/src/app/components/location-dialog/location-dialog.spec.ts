import { MatSnackBar } from '@angular/material/snack-bar';
import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { of, throwError } from 'rxjs';
import { LocationDialog } from './location-dialog';
import { CollectorService } from '../../services/collector';
import { CollectorPointService } from '../../services/collector-point';

describe('LocationDialog', () => {
  const collectors = [{ id: 'guid-papel', name: 'Papel', color: '#0000FF', description: '' }, { id: 'guid-vidro', name: 'Vidro', color: '#008000', description: '' }];
  const getCollectors = vi.fn();
  const create = vi.fn();
  const close = vi.fn();
  const toast = { open: vi.fn() };
  beforeEach(() => {
    create.mockReset(); close.mockReset(); toast.open.mockReset();
    getCollectors.mockReset().mockReturnValue(of(collectors));
    TestBed.configureTestingModule({ imports: [LocationDialog], providers: [
      { provide: MAT_DIALOG_DATA, useValue: { lat: -1, lng: -48 } },
      { provide: CollectorService, useValue: { getCollectors } },
      { provide: CollectorPointService, useValue: { createCollectorPoint: create } },
      { provide: MatDialogRef, useValue: { close, disableClose: false } },
      { provide: MatSnackBar, useValue: toast },
    ] }).overrideProvider(MatSnackBar, { useValue: toast });
  });
  it('carrega coletores e adiciona/remove IDs pelo chip sem duplicar', async () => {
    const fixture = TestBed.createComponent(LocationDialog); await fixture.whenStable();
    expect(getCollectors).toHaveBeenCalledTimes(1);
    const chips = fixture.nativeElement.querySelectorAll('.collector-chip');
    expect(chips.length).toBe(2);
    chips[0].click(); chips[1].click(); fixture.detectChanges();
    expect(fixture.componentInstance.selectedIds).toEqual(['guid-papel', 'guid-vidro']);
    expect(chips[0].getAttribute('aria-pressed')).toBe('true');
    chips[0].click();
    expect(fixture.componentInstance.selectedIds).toEqual(['guid-vidro']);
  });
  it('mostra erro e permite tentar novamente', async () => {
    getCollectors.mockReturnValueOnce(throwError(() => new Error('offline')));
    const fixture = TestBed.createComponent(LocationDialog); await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain('Não foi possível');
    const retry = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>).find(button => button.textContent?.includes('Tentar novamente'));
    retry!.click();
    await fixture.whenStable();
    expect(fixture.componentInstance.collectors()).toEqual(collectors);
    expect(fixture.componentInstance.collectorsError()).toBe('');
  });
  it('informa quando a lista está vazia', async () => {
    getCollectors.mockReturnValue(of([]));
    const fixture = TestBed.createComponent(LocationDialog); await fixture.whenStable();
    expect(fixture.nativeElement.textContent).toContain('Nenhum coletor cadastrado');
  });
  it('envia imagens e coletores e fecha com o ponto criado', async () => {
    const fixture = TestBed.createComponent(LocationDialog); await fixture.whenStable();
    const point = { id: 'point-id' }; create.mockReturnValue(of(point));
    fixture.componentInstance.selectedIds = ['guid-papel'];
    fixture.componentInstance.images = [new File(['png'], 'p.png', { type: 'image/png' })];
    await fixture.componentInstance.submit();
    expect(create).toHaveBeenCalledWith({ lat: -1, lng: -48, collectorIds: ['guid-papel'] }, fixture.componentInstance.images);
    expect(close).toHaveBeenCalledWith(point);
    expect(fixture.componentInstance.submitting()).toBe(false);
  });
  it('mantém diálogo aberto quando a criação falha', async () => {
    const fixture = TestBed.createComponent(LocationDialog); await fixture.whenStable();
    create.mockReturnValue(throwError(() => new Error('offline')));
    fixture.componentInstance.selectedIds = ['guid-papel'];
    fixture.componentInstance.images = [new File(['png'], 'p.png', { type: 'image/png' })];
    await fixture.componentInstance.submit();
    expect(close).not.toHaveBeenCalled();
    expect(toast.open).toHaveBeenCalled();
    expect(fixture.componentInstance.submitting()).toBe(false);
  });

});
