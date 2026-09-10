import { Component, Input, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import * as L from 'leaflet';
import { LocationDialog } from '../location-dialog/location-dialog';

@Component({
  selector: 'app-leaflet-map',
  imports: [],
  templateUrl: './leaflet-map.html',
  styleUrl: './leaflet-map.scss',
})
export class LeafletMap implements OnInit {
  //@Input() collectors: Collector[] = [];
  @Input() collectors: Collector[] | null = [];

  private map!: L.Map;
  private centroid: L.LatLngExpression = [-1.4495645, -48.4729685]; // Coordenadas de Belém
  private marker?: L.Marker;

  constructor(private dialog: MatDialog) {}

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
          this.dialog.open(LocationDialog, {
            width: '500px',
            data: {
              lat,
              lng,
              collectors: this.collectors,
            },
          });
        });
      });
      this.marker.openPopup();
    });
  }

  ngOnInit(): void {
    this.initMap();
  }

  // ngAfterViewInit(): void {
  //   setTimeout(() => {
  //     this.initMap();
  //   }, 0);
  // }
}
