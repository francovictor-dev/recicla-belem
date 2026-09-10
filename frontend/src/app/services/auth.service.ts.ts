import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { User } from '../types/user.type';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);

  login(data: { email: string; password: string }): Observable<{ access_token: string }> {
    return this.http.post(`${environment.apiUrl}/login`, data) as Observable<{
      access_token: string;
    }>;
  }

  logout() {
    localStorage.removeItem('token');
  }

  me(): Observable<User> {
    return this.http.get(`${environment.apiUrl}/auth/me`) as Observable<User>;
  }
}
