import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

export type CollectionPointResult = {
  id: string;
  isActive: boolean;
  status: number;
  address: { road: string; lat: number; lng: number } | null;
  collectors?: { id: string; name: string; color: string }[] | null;
  images: { id: string; storagePath: string }[] | null;
};

@Injectable({ providedIn: 'root' })
export class CollectorPointService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/api/CollectionPoint`;

  getAll() {
    return this.http.get<CollectionPointResult[]>(this.url, { params: { include: 'address,collector,image' } });
  }

  createCollectorPoint(data: { lat: number; lng: number; collectorIds: string[] }, images: File[]) {
    const body = new FormData();
    body.set('Lat', String(data.lat));
    body.set('Lng', String(data.lng));
    data.collectorIds.forEach(id => body.append('CollectorIds', id));
    images.forEach(image => body.append('Images', image));
    return this.http.post<CollectionPointResult>(this.url, body, { params: { include: 'address,collector,image' } });
  }
}
