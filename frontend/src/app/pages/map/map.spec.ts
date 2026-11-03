import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Map } from './map';
import { LeafletMap } from '../../components/leaflet-map/leaflet-map';

@Component({ selector: 'app-leaflet-map', template: '' })
class MapStub {}

describe('Map', () => {
  it('renderiza o mapa que solicita localização', async () => {
    TestBed.configureTestingModule({ imports: [Map] }).overrideComponent(Map, { remove: { imports: [LeafletMap] }, add: { imports: [MapStub] } });
    const fixture = TestBed.createComponent(Map); await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('app-leaflet-map')).toBeTruthy();
  });
});
