import { GoogleSignIn } from '../google-sign-in/google-sign-in';
import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../services/auth.service.ts';
import { Loading } from '../loading/loading';
import { Component, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { Button } from '../button/button';

@Component({
  selector: 'app-register-dialog',
  imports: [GoogleSignIn, ReactiveFormsModule, MatDialogModule, MatSnackBarModule, Button, Loading],
  templateUrl: './register-dialog.html',
  styleUrl: './register-dialog.scss',
})
export class RegisterDialog {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly dialog = inject(MatDialogRef<RegisterDialog>);
  private readonly toast = inject(MatSnackBar);
  private readonly router = inject(Router);
  readonly loading = signal(false);
  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    name: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(100)]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required],
  }, {
    validators: (control: AbstractControl) =>
      control.get('password')?.value === control.get('confirmPassword')?.value
        ? null : { passwordMismatch: true },
  });
  selectedImage: File | null = null;
  imageError = '';

  async submit(): Promise<void> {
    if (this.loading()) return;
    this.form.markAllAsTouched();
    if (this.form.invalid || this.imageError) return;
    this.loading.set(true);
    this.dialog.disableClose = true;
    let created = false;
    try {
      const data = this.form.getRawValue();
      await firstValueFrom(this.auth.register(data, this.selectedImage));
      created = true;
      await this.auth.signIn(data);
      this.toast.open('Cadastro realizado com sucesso!', 'Fechar', { duration: 5000, verticalPosition: 'top', horizontalPosition: 'center' });
      this.dialog.close(true);
      await this.router.navigate(['/mapa']);
    } catch (error) {
      const message = created
        ? 'Conta criada, mas não foi possível entrar. Faça login para continuar.'
        : error instanceof HttpErrorResponse && error.status === 409
          ? 'Este e-mail já está cadastrado.'
          : 'Não foi possível realizar o cadastro. Tente novamente.';
      this.toast.open(message, 'Fechar', { duration: 7000, verticalPosition: 'top', horizontalPosition: 'center' });
      if (created) this.dialog.close(true);
    } finally {
      this.loading.set(false);
      this.dialog.disableClose = false;
    }
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    this.selectedImage = null;
    this.imageError = '';
    if (!file) return;

    if (file.type !== 'image/png') {
      this.imageError = 'Selecione uma imagem PNG.';
    } else if (file.size > 300 * 1024) {
      this.imageError = 'A imagem deve ter no máximo 300 KB.';
    } else {
      this.selectedImage = file;
    }
    if (this.imageError) input.value = '';
  }
}
