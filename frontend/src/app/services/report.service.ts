import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

export type ReportRequest = { message: string; reportType: number; collectionPointId: string; userId: string };

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly http = inject(HttpClient);
  create(data: ReportRequest) { return this.http.post(`${environment.apiUrl}/api/Report`, data); }
}
