import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LucideAngularModule, RectangleGogglesIcon } from 'lucide-angular';
import { firstValueFrom } from 'rxjs';
import { Button } from '../../components/button/button';
import { Card } from '../../components/card/card';
import { InputComponent } from '../../components/input/input';
import { AuthService } from '../../services/auth.service.ts';
import { AuthStore } from '../../store/user.store';
@Component({
  selector: 'app-auth',
  imports: [
    Card,
    InputComponent,
    ReactiveFormsModule,
    CommonModule,
    Button,
    NgOptimizedImage,
    LucideAngularModule,
  ],
  templateUrl: './auth.html',
  styleUrl: './auth.scss',
})
export class Auth {
  private authService = inject(AuthService);
  private authStore = inject(AuthStore);
  private router = inject(Router);

  constructor(private fb: FormBuilder) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],

      password: ['', [Validators.required, Validators.minLength(8)]],
    });
  }

  icons = { RectangleGogglesIcon };

  loginForm!: FormGroup;

  async submit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    try {
      // LOGIN
      const loginResponse = await firstValueFrom(
        this.authService.login(this.loginForm.getRawValue()),
      );

      localStorage.setItem('token', loginResponse.access_token);

      const data = await firstValueFrom(this.authService.me());

      this.authStore.setUser(data);
      this.authStore.setAuth(true);
      this.router.navigate(['/mapa']);
    } catch (error) {
      console.error('Login failed:', error);
    }
  }
}
