import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { LogOut, LucideAngularModule } from 'lucide-angular';
import { AuthService } from '../../services/auth.service.ts';
import { AuthStore } from '../../store/user.store';
import { Button } from '../button/button';

@Component({
  selector: 'app-header',
  imports: [NgOptimizedImage, CommonModule, LucideAngularModule, Button],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header {
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
