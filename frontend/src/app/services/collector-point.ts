import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class CollectorPointService {
  private http = inject(HttpClient);

  createCollectorPoint(data: CreateCollectorPointDTO, image: File) {
    const formData = new FormData();
    formData.append('image', image);

    formData.append('data', JSON.stringify(data));

    return this.http.post(`${environment.apiUrl}/collector-points`, formData);
  }
}
