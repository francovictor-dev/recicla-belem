import { Injectable, signal } from '@angular/core';

import { User } from '../types/user.type';

@Injectable({
  providedIn: 'root',
})
export class AuthStore {
  user = signal<User | null>(null);
  isAuth = signal<boolean | null>(null);

  setUser(user: User) {
    this.user.set(user);
  }

  setAuth(data: boolean) {
    this.isAuth.set(data);
  }

  clearAll() {
    this.user.set(null);
    this.isAuth.set(null);
  }
}
