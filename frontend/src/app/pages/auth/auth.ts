import { GoogleSignIn } from '../../components/google-sign-in/google-sign-in';
import { HttpErrorResponse } from '@angular/common/http';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { RegisterDialog } from '../../components/register-dialog/register-dialog';
import { LucideAngularModule, RectangleGogglesIcon } from 'lucide-angular';
import { Button } from '../../components/button/button';
import { Card } from '../../components/card/card';
import { InputComponent } from '../../components/input/input';
import { AuthService } from '../../services/auth.service.ts';
@Component({
  selector: 'app-auth',
  imports: [
    GoogleSignIn,
    MatDialogModule,
    MatSnackBarModule,
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
  private readonly toast = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  openRegister(): void {
    this.dialog.open(RegisterDialog, {
      width: '460px',
      maxWidth: 'calc(100vw - 32px)',
      maxHeight: '90vh',
      autoFocus: 'first-tabbable',
    });
  }

  private authService = inject(AuthService);
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
      await this.authService.signIn(this.loginForm.getRawValue());
      this.router.navigate(['/mapa']);
    } catch (error) {
      const message = error instanceof HttpErrorResponse && error.status === 401
        ? 'E-mail ou senha inválidos.'
        : 'Não foi possível entrar. Tente novamente.';
      this.toast.open(message, 'Fechar', { duration: 7000, verticalPosition: 'top', horizontalPosition: 'center' });
    }
  }
}
