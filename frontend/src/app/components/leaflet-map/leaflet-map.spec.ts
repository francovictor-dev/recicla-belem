import { CollectionPointDialog } from '../collection-point-dialog/collection-point-dialog';
import { CollectorService } from '../../services/collector';
import { Router } from '@angular/router';
import { AuthStore } from '../../store/user.store';
import { AuthRequiredDialog } from '../auth-required-dialog/auth-required-dialog';
import { LocationDialog } from '../location-dialog/location-dialog';
import { of } from 'rxjs';
import { CollectorPointService } from '../../services/collector-point';
import { MatDialog } from '@angular/material/dialog';
import { LeafletMap } from './leaflet-map';

const leaflet = vi.hoisted(() => {
  const map = { on: vi.fn(), setView: vi.fn(), invalidateSize: vi.fn(), remove: vi.fn() };
  const marker = { addTo: vi.fn().mockReturnThis(), bindPopup: vi.fn().mockReturnThis(), on: vi.fn().mockReturnThis(), remove: vi.fn() };
  const layer = { addTo: vi.fn().mockReturnThis(), clearLayers: vi.fn() };
  return { map, marker, layer, createMarker: vi.fn((_coordinates: unknown, _options: any) => marker) };
});
vi.mock('leaflet', () => ({
  map: vi.fn(() => leaflet.map),
  layerGroup: vi.fn(() => leaflet.layer),
  tileLayer: vi.fn(() => ({ addTo: vi.fn() })),
  marker: leaflet.createMarker,
  divIcon: vi.fn(value => value),
}));

describe('LeafletMap', () => {
  let component: LeafletMap;
  const getCurrentPosition = vi.fn();
  const getAll = vi.fn();
  const open = vi.fn();
  const navigate = vi.fn();
  let auth: AuthStore;
  beforeEach(() => {
    vi.clearAllMocks();
    getAll.mockReturnValue(of([]));
    open.mockReturnValue({ afterClosed: () => of({ id: 'new-point' }) });
    navigate.mockResolvedValue(true);
    auth = new AuthStore(); auth.setAuth(true);
    vi.stubGlobal('navigator', { geolocation: { getCurrentPosition } });
    component = new LeafletMap({ open } as unknown as MatDialog, { getAll } as unknown as CollectorPointService, auth, { navigate } as unknown as Router, { getCollectors: () => of([{ id: 'paper', name: 'Papel', color: '#0000FF', description: '' }]) } as unknown as CollectorService);
  });
  afterEach(() => vi.unstubAllGlobals());
  it('solicita localização na entrada e centraliza o pino autorizado', () => {
    component.ngAfterViewInit();
    expect(getCurrentPosition).toHaveBeenCalledTimes(1);
    const success = getCurrentPosition.mock.calls[0][0];
    success({ coords: { latitude: -1.45, longitude: -48.5 } });
    expect(leaflet.createMarker).toHaveBeenCalledWith([-1.45, -48.5], expect.objectContaining({ title: 'Sua localização' }));
    expect(leaflet.map.setView).toHaveBeenCalledWith([-1.45, -48.5], 16, { animate: false });
  });
  it('mantém o mapa disponível quando a localização é negada', () => {
    component.ngAfterViewInit();
    getCurrentPosition.mock.calls[0][1]({ code: 1 });
    expect(component.locationMessage()).toContain('explorar o mapa manualmente');
    expect(leaflet.map.setView).not.toHaveBeenCalled();
  });
  it('trata navegador sem geolocalização', () => {
    vi.stubGlobal('navigator', {});
    component.ngAfterViewInit();
    expect(component.locationMessage()).toContain('não oferece localização');
  });
  it('tenta alta precisão após timeout e mostra falha visível se não obtiver posição', () => {
    component.ngAfterViewInit();
    expect(component.locating()).toBe(true);
    getCurrentPosition.mock.calls[0][1]({ code: 3 });
    expect(getCurrentPosition).toHaveBeenCalledTimes(2);
    expect(getCurrentPosition.mock.calls[1][2].enableHighAccuracy).toBe(true);
    getCurrentPosition.mock.calls[1][1]({ code: 3 });
    expect(component.locating()).toBe(false);
    expect(component.locationMessage()).toContain('demorou');
    component.requestLocation();
    expect(getCurrentPosition).toHaveBeenCalledTimes(3);
  });
  it('centraliza após a segunda tentativa obter posição', () => {
    component.ngAfterViewInit();
    getCurrentPosition.mock.calls[0][1]({ code: 2 });
    getCurrentPosition.mock.calls[1][0]({ coords: { latitude: -1.4, longitude: -48.4 } });
    expect(leaflet.map.invalidateSize).toHaveBeenCalledWith({ pan: false });
    expect(leaflet.map.setView).toHaveBeenCalledWith([-1.4, -48.4], 16, { animate: false });
    expect(component.locating()).toBe(false);
  });
  it('impede solicitações duplicadas enquanto está localizando', () => {
    component.ngAfterViewInit(); component.requestLocation();
    expect(getCurrentPosition).toHaveBeenCalledTimes(1);
  });
  it('remove o mapa ao sair e ignora resposta tardia de localização', () => {
    component.ngAfterViewInit();
    component.ngOnDestroy();
    getCurrentPosition.mock.calls[0][0]({ coords: { latitude: 1, longitude: 2 } });
    expect(leaflet.map.remove).toHaveBeenCalled();
    expect(leaflet.createMarker).not.toHaveBeenCalled();
  });
  it('recarrega pontos após o diálogo retornar sucesso e atualiza a camada', () => {
    component.ngAfterViewInit();
    getAll.mockReturnValue(of([{ id: 'new-point', isActive: true, status: 1, address: { road: 'Rua', lat: -1, lng: -48 } }]));
    component.openPointDialog(-1, -48);
    expect(getAll).toHaveBeenCalledTimes(2);
    expect(leaflet.layer.clearLayers).toHaveBeenCalledTimes(2);
    expect(leaflet.createMarker).toHaveBeenCalledWith([-1, -48], expect.objectContaining({
      icon: expect.objectContaining({
        className: 'collection-point-pin', iconSize: [32, 42], iconAnchor: [16, 42],
        html: expect.stringContaining('<svg'),
      }),
    }));
  });

  it('abre criação diretamente para usuário autenticado', () => {
    component.openPointDialog(-1, -48);
    expect(open).toHaveBeenCalledWith(LocationDialog, expect.objectContaining({ data: { lat: -1, lng: -48 } }));
  });
  it.each([false, undefined])('mostra aviso e não navega ao cancelar ou fechar (%s)', result => {
    auth.clearAll(); open.mockReturnValue({ afterClosed: () => of(result) });
    component.openPointDialog(-1, -48);
    expect(open).toHaveBeenCalledTimes(1);
    expect(open).toHaveBeenCalledWith(AuthRequiredDialog, expect.any(Object));
    expect(navigate).not.toHaveBeenCalled();
    expect(getAll).not.toHaveBeenCalled();
  });
  it('vai para login quando visitante confirma', () => {
    auth.clearAll(); open.mockReturnValue({ afterClosed: () => of(true) });
    component.openPointDialog(-1, -48);
    expect(navigate).toHaveBeenCalledWith(['/autenticacao/login']);
    expect(open).not.toHaveBeenCalledWith(LocationDialog, expect.any(Object));
  });

  it('usa a cor do primeiro coletor e filtra por qualquer opção selecionada', () => {
    getAll.mockReturnValue(of([
      { id: 'a', isActive: true, status: 1, address: { lat: 1, lng: 2, road: 'Rua A' }, collectors: [{ id: 'paper', color: '#0000FF' }, { id: 'glass', color: '#008000' }] },
      { id: 'b', isActive: true, status: 1, address: { lat: 3, lng: 4, road: 'Rua B' }, collectors: [{ id: 'plastic', color: '#FF0000' }] },
    ]));
    component.ngAfterViewInit();
    expect(leaflet.createMarker.mock.calls[0][1].icon.html).toContain('fill="#0000FF"');
    expect(component.filterCollectors().length).toBe(1);
    leaflet.createMarker.mockClear();
    component.toggleCollectorFilter('glass');
    expect(leaflet.createMarker).toHaveBeenCalledTimes(1);
    expect(leaflet.createMarker.mock.calls[0][0]).toEqual([1, 2]);
    expect(leaflet.createMarker.mock.calls[0][1].icon.html).toContain('fill="#008000"');
    leaflet.createMarker.mockClear();
    component.toggleCollectorFilter('plastic');
    expect(leaflet.createMarker).toHaveBeenCalledTimes(2);
    expect(leaflet.createMarker.mock.calls[0][1].icon.html).toContain('fill="#008000"');
    expect(leaflet.createMarker.mock.calls[1][1].icon.html).toContain('fill="#FF0000"');
    leaflet.createMarker.mockClear();
    component.toggleCollectorFilter('glass');
    expect(leaflet.createMarker).toHaveBeenCalledTimes(1);
    expect(leaflet.createMarker.mock.calls[0][0]).toEqual([3, 4]);
    leaflet.createMarker.mockClear(); component.clearCollectorFilters();
    expect(leaflet.createMarker).toHaveBeenCalledTimes(2);
    expect(leaflet.createMarker.mock.calls[0][1].icon.html).toContain('fill="#0000FF"');
  });

  it('usa o primeiro coletor compatível quando vários filtros correspondem ao mesmo ponto', () => {
    getAll.mockReturnValue(of([{ id: 'a', isActive: true, status: 1, address: { lat: 1, lng: 2, road: 'Rua' }, collectors: [{ id: 'paper', color: '#0000FF' }, { id: 'glass', color: '#008000' }] }]));
    component.ngAfterViewInit();
    component.toggleCollectorFilter('glass');
    leaflet.createMarker.mockClear(); component.toggleCollectorFilter('paper');
    expect(leaflet.createMarker.mock.calls[0][1].icon.html).toContain('fill="#0000FF"');
    leaflet.createMarker.mockClear(); component.toggleCollectorFilter('paper');
    expect(leaflet.createMarker.mock.calls[0][1].icon.html).toContain('fill="#008000"');
  });

  it('abre diálogo grande com dados completos ao clicar no pino sem popup de texto', () => {
    const point = { id: 'point', isActive: true, status: 1, address: { road: 'Rua', lat: 1, lng: 2 }, images: [], collectors: [] };
    getAll.mockReturnValue(of([point]));
    component.ngAfterViewInit();
    const click = leaflet.marker.on.mock.calls.find(call => call[0] === 'click')![1];
    click();
    expect(leaflet.marker.bindPopup).not.toHaveBeenCalled();
    expect(open).toHaveBeenCalledWith(CollectionPointDialog, expect.objectContaining({ data: point, width: '900px' }));
  });

});
