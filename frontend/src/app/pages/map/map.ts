import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { LeafletMap } from '../../components/leaflet-map/leaflet-map';
import { CollectorService } from '../../services/collector';

@Component({
  selector: 'app-map',
  imports: [LeafletMap, AsyncPipe],
  templateUrl: './map.html',
  styleUrl: './map.scss',
})
export class Map {
  collectorService = inject(CollectorService);
  //collectors: Collector[] = [];
  collectors$ = this.collectorService.getCollectors();
  /* ngOnInit() {
    this.collectorService.getCollectors().subscribe({
      next: (collectors) => {
        this.collectors = collectors;
      },
    });
  } */
}
