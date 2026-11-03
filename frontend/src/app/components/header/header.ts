import { MatMenuModule } from '@angular/material/menu';
import { signal, OnInit } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { LogOut, LucideAngularModule } from 'lucide-angular';
import { AuthService } from '../../services/auth.service.ts';
import { AuthStore } from '../../store/user.store';

@Component({
  selector: 'app-header',
  imports: [NgOptimizedImage, CommonModule, LucideAngularModule, MatMenuModule, RouterLink],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header implements OnInit {
  readonly imageFailed = signal(false);

  async ngOnInit(): Promise<void> {
    if (!localStorage.getItem('token') || this.authStore.isAuth()) return;
    try {
      this.authStore.setUser(await firstValueFrom(this.authService.me()));
      this.authStore.setAuth(true);
    } catch {
      this.authService.logout();
    }
  }

  private authService = inject(AuthService);
  authStore = inject(AuthStore);
  private router = inject(Router);
  readonly icons = { LogOut };

  logout() {
    this.authService.logout();
    this.authStore.clearAll();
    this.router.navigate(['/autenticacao/login']);
  }
}
