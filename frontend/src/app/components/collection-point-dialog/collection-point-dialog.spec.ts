import { MatDialog } from '@angular/material/dialog';
import { ReportDialog } from '../report-dialog/report-dialog';
import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CollectionPointDialog } from './collection-point-dialog';

describe('CollectionPointDialog', () => {
  const open = vi.fn();
  beforeEach(() => { open.mockReset(); TestBed.overrideComponent(CollectionPointDialog, { add: { providers: [{ provide: MatDialog, useValue: { open } }] } }); });
  const point = { id: 'point', address: { road: 'Rua exemplo' }, collectors: [{ id: 'paper', name: 'Papel', color: '#0000FF' }, { id: 'glass', name: 'Vidro', color: '#008000' }], images: [{ id: '1', storagePath: 'https://example.com/1.png' }, { id: '2', storagePath: 'https://example.com/2.png' }] };
  it('navega pelo carrossel e exibe todos os coletores e Denunciar', async () => {
    TestBed.configureTestingModule({ providers: [{ provide: MAT_DIALOG_DATA, useValue: point }] });
    const fixture = TestBed.createComponent(CollectionPointDialog); await fixture.whenStable();
    expect(fixture.nativeElement.querySelectorAll('.collector-chip').length).toBe(2);
    expect(fixture.nativeElement.querySelector('.point-image').src).toBe(point.images[0].storagePath);
    fixture.nativeElement.querySelector('.next').click(); await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('.point-image').src).toBe(point.images[1].storagePath);
    fixture.nativeElement.querySelector('.next').click(); await fixture.whenStable();
    expect(fixture.componentInstance.index()).toBe(0);
    fixture.nativeElement.querySelector('.previous').click(); await fixture.whenStable();
    expect(fixture.componentInstance.index()).toBe(1);
    expect(fixture.nativeElement.querySelector('.report-button').textContent).toContain('Denunciar');
    expect(fixture.nativeElement.querySelector('.report-button lucide-icon')).toBeTruthy();
    fixture.nativeElement.querySelector('.report-button').click();
    expect(open).toHaveBeenCalledWith(ReportDialog, expect.objectContaining({ data: { collectionPointId: 'point' } }));
  });
  it('trata ponto sem imagens e coletores', async () => {
    TestBed.configureTestingModule({ providers: [{ provide: MAT_DIALOG_DATA, useValue: { images: null, collectors: null } }] });
    const fixture = TestBed.createComponent(CollectionPointDialog); await fixture.whenStable();
    expect(fixture.nativeElement.textContent).toContain('não possui imagens');
    expect(fixture.nativeElement.textContent).toContain('Nenhum tipo');
    fixture.componentInstance.select(1); expect(fixture.componentInstance.index()).toBe(0);
  });
  it('trata imagem indisponível e permite navegar para outra', async () => {
    TestBed.configureTestingModule({ providers: [{ provide: MAT_DIALOG_DATA, useValue: point }] });
    const fixture = TestBed.createComponent(CollectionPointDialog); await fixture.whenStable();
    fixture.nativeElement.querySelector('img').dispatchEvent(new Event('error')); await fixture.whenStable();
    expect(fixture.nativeElement.textContent).toContain('Não foi possível carregar');
    fixture.nativeElement.querySelector('.next').click(); await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('img')).toBeTruthy();
  });
});
