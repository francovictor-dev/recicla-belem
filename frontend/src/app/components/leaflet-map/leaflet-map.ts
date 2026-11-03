import { CollectionPointDialog } from '../collection-point-dialog/collection-point-dialog';
import { MatMenuModule } from '@angular/material/menu';
import { LucideAngularModule, LocateFixed, ListFilter } from 'lucide-angular';
import { CollectorService } from '../../services/collector';
import { Router } from '@angular/router';
import { AuthStore } from '../../store/user.store';
import { AuthRequiredDialog } from '../auth-required-dialog/auth-required-dialog';
import { Subscription } from 'rxjs';
import { CollectorPointService, CollectionPointResult } from '../../services/collector-point';
import { Component, AfterViewInit, OnDestroy, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import * as L from 'leaflet';
import { LocationDialog } from '../location-dialog/location-dialog';

@Component({
  selector: 'app-leaflet-map',
  imports: [MatMenuModule, LucideAngularModule],
  templateUrl: './leaflet-map.html',
  styleUrl: './leaflet-map.scss',
})
export class LeafletMap implements AfterViewInit, OnDestroy {
  readonly locationMessage = signal('');
  readonly locating = signal(false);
  private destroyed = false;
  private userMarker?: L.Marker;

  private map!: L.Map;
  private centroid: L.LatLngExpression = [-1.4495645, -48.4729685]; // Coordenadas de Belém
  private marker?: L.Marker;
  private pointLayer?: L.LayerGroup;
  private readonly subscriptions = new Subscription();
  readonly pointsError = signal('');
  readonly filterCollectors = signal<Collector[]>([]);
  readonly selectedCollectorIds = signal<string[]>([]);
  readonly filtersError = signal('');
  readonly filtersLoading = signal(false);
  readonly icons = { LocateFixed, ListFilter };
  private collectionPoints: CollectionPointResult[] = [];

  constructor(private dialog: MatDialog, private points: CollectorPointService, private auth: AuthStore, private router: Router, private collectorsService: CollectorService) {}

  initMap() {
    this.map = L.map('map', {
      center: this.centroid,
      zoom: 14,
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      minZoom: 3,
      attribution:
        '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
    }).addTo(this.map);

    this.map.on('click', (event: L.LeafletMouseEvent) => {
      const lat = event.latlng.lat;
      const lng = event.latlng.lng;

      // Remove marker antigo
      if (this.marker) {
        this.map.removeLayer(this.marker);
      }

      // Cria novo marker
      this.marker = L.marker([lat, lng]).addTo(this.map).bindPopup(`
        <p class="open-dialog-btn" style="cursor: pointer;">
          + Adicionar novo ponto de coleta
        </p>
      `);

      this.marker.on('popupopen', (e: any) => {
        const popupElement = e.popup.getElement();
        const button = popupElement.querySelector('.open-dialog-btn');

        button?.addEventListener('click', () => {
          this.openPointDialog(lat, lng);
        });
      });
      this.marker.openPopup();
    });
  }

  ngAfterViewInit(): void {
    this.initMap();
    this.pointLayer = L.layerGroup().addTo(this.map);
    this.loadPoints();
    this.loadCollectors();
    this.requestLocation();
  }

  openPointDialog(lat: number, lng: number): void {
    if (!this.auth.isAuth()) {
      const notice = this.dialog.open(AuthRequiredDialog, {
        width: '460px', maxWidth: 'calc(100vw - 32px)',
        ariaLabel: 'Autenticação necessária', autoFocus: 'first-tabbable',
      });
      this.subscriptions.add(notice.afterClosed().subscribe(login => {
        if (login === true && !this.destroyed) void this.router.navigate(['/autenticacao/login']);
      }));
      return;
    }
    const ref = this.dialog.open(LocationDialog, { width: '500px', data: { lat, lng } });
    this.subscriptions.add(ref.afterClosed().subscribe(point => {
      if (point && !this.destroyed) {
        this.marker?.remove();
        this.loadPoints();
      }
    }));
  }

  loadPoints(): void {
    this.subscriptions.add(this.points.getAll().subscribe({
      next: points => {
        if (this.destroyed) return;
        this.collectionPoints = points;
        this.renderPoints();
        this.pointsError.set('');
      },
      error: () => this.pointsError.set('Não foi possível atualizar os pontos de coleta.'),
    }));
  }

  loadCollectors(): void {
    this.filtersLoading.set(true);
    this.filtersError.set('');
    this.subscriptions.add(this.collectorsService.getCollectors().subscribe({
      next: collectors => { this.filterCollectors.set(collectors); this.filtersLoading.set(false); },
      error: () => { this.filtersError.set('Não foi possível carregar os filtros.'); this.filtersLoading.set(false); },
    }));
  }

  toggleCollectorFilter(id: string): void {
    this.selectedCollectorIds.update(selected => selected.includes(id) ? selected.filter(value => value !== id) : [...selected, id]);
    this.renderPoints();
  }

  clearCollectorFilters(): void {
    this.selectedCollectorIds.set([]);
    this.renderPoints();
  }

  private renderPoints(): void {
        this.pointLayer?.clearLayers();
        for (const point of this.collectionPoints) {
          if (!point.isActive || point.status !== 1 || !point.address) continue;
          const selected = this.selectedCollectorIds();
          if (selected.length && !point.collectors?.some(collector => selected.includes(collector.id))) continue;
          const matchingCollector = selected.length
            ? point.collectors?.find(collector => selected.includes(collector.id))
            : point.collectors?.[0];
          const firstColor = matchingCollector?.color ?? '';
          const color = /^#[0-9a-f]{6}$/i.test(firstColor) ? firstColor : '#386641';
          L.marker([point.address.lat, point.address.lng], {
            icon: L.divIcon({ className: 'collection-point-pin', html: `<svg xmlns="http://www.w3.org/2000/svg" width="32" height="42" viewBox="0 0 32 42" aria-hidden="true" style="display:block;filter:drop-shadow(0 2px 2px #0005)"><path d="M16 1C7.7 1 1 7.7 1 16c0 11 15 25 15 25s15-14 15-25C31 7.7 24.3 1 16 1Z" fill="${color}" stroke="white" stroke-width="2"/><circle cx="16" cy="15" r="5" fill="white"/></svg>`, iconSize: [32, 42], iconAnchor: [16, 42], popupAnchor: [0, -38] }),
            title: point.address.road || 'Ponto de coleta',
            bubblingMouseEvents: false,
          }).addTo(this.pointLayer!).on('click', () => {
            this.dialog.open(CollectionPointDialog, {
              data: point, width: '900px', maxWidth: 'calc(100vw - 32px)', maxHeight: '92vh',
              autoFocus: 'first-tabbable',
            });
          });
        }
  }

  requestLocation(): void {
    if (this.locating() || this.destroyed) return;
    if (!navigator.geolocation) {
      this.locationMessage.set('Seu navegador não oferece localização. Você pode explorar o mapa manualmente.');
      return;
    }
    this.locating.set(true);
    this.locationMessage.set('Obtendo sua localização…');
    this.locate(false);
  }

  private locate(highAccuracy: boolean): void {
    navigator.geolocation.getCurrentPosition(
      position => {
        if (this.destroyed) return;
        const coordinates: L.LatLngExpression = [position.coords.latitude, position.coords.longitude];
        this.userMarker?.remove();
        this.userMarker = L.marker(coordinates, {
          title: 'Sua localização',
          icon: L.divIcon({
            className: 'user-location-pin',
            html: '<span style="display:block;width:20px;height:20px;border:3px solid white;border-radius:50%;background:#2196f3;box-shadow:0 0 0 5px rgba(33,150,243,0.25)"></span>',
            iconSize: [26, 26], iconAnchor: [13, 13],
          }),
        }).addTo(this.map).bindPopup('Você está aqui');
        this.map.invalidateSize({ pan: false });
        this.map.setView(coordinates, 16, { animate: false });
        this.locating.set(false);
        this.locationMessage.set('');
      },
      error => {
        if (this.destroyed) return;
        if (!highAccuracy && (error.code === 2 || error.code === 3)) {
          this.locationMessage.set('Buscando sua localização com maior precisão…');
          this.locate(true);
          return;
        }
        this.locating.set(false);
        const message = error.code === 1
          ? 'Localização não autorizada. Permita o acesso nas configurações do navegador.'
          : error.code === 3
            ? 'O navegador demorou para obter sua localização. Tente novamente.'
            : 'O navegador não conseguiu determinar sua localização. Verifique a localização do dispositivo e tente novamente.';
        this.locationMessage.set(`${message} Você pode explorar o mapa manualmente.`);
      },
      { enableHighAccuracy: highAccuracy, timeout: highAccuracy ? 30000 : 20000, maximumAge: 0 },
    );
  }

  ngOnDestroy(): void {
    this.destroyed = true;
    this.subscriptions.unsubscribe();
    this.map?.remove();
  }
}
