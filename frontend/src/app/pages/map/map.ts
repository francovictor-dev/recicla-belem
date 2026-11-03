import { Component } from '@angular/core';
import { LeafletMap } from '../../components/leaflet-map/leaflet-map';

@Component({
  selector: 'app-map',
  imports: [LeafletMap],
  templateUrl: './map.html',
  styleUrl: './map.scss',
})
export class Map {}
