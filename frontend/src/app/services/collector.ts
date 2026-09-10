import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, shareReplay } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class CollectorService {
  private http = inject(HttpClient);
  private collectors$?: Observable<Collector[]>;

  getCollectors(): Observable<Collector[]> {
    if (!this.collectors$) {
      this.collectors$ = this.http
        .get<Collector[]>(`${environment.apiUrl}/collectors`)
        .pipe(shareReplay(1));
    }

    return this.collectors$;
  }
}
