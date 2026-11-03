import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { MatDialogRef } from '@angular/material/dialog';
import { AuthService } from '../../services/auth.service.ts';
import { Loading } from '../loading/loading';
import {
  AfterViewInit,
  Component,
  ElementRef,
  inject,
  OnDestroy,
  signal,
  ViewChild,
} from "@angular/core";
import { MatSnackBar } from "@angular/material/snack-bar";
import { GoogleAuthService } from "../../services/google-auth.service";
import { Button } from "../button/button";

@Component({
  selector: "app-google-sign-in",
  imports: [Button, Loading],
  template: `
    <div #googleButton class="google-button" [hidden]="loading()" [attr.aria-busy]="loading()"></div>
    @if (loading()) { <app-loading /> }
    @if (unavailable()) {
      <app-button
        class="full"
        variant="outline"
        type="button"
        (click)="showUnavailable()"
      >
        <img src="/svgs/google.svg" alt="" width="24" height="24" />
        <span>Continuar com Google</span>
      </app-button>
    }
  `,
  styles:
    ":host { display: block; width: 100%; } .google-button { display: flex; justify-content: center; } .google-button[hidden] { display: none; } span { margin-left: 0.5rem; }",
})
export class GoogleSignIn implements AfterViewInit, OnDestroy {
  private readonly google = inject(GoogleAuthService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialogRef, { optional: true });
  readonly loading = signal(false);
  private readonly toast = inject(MatSnackBar);
  @ViewChild("googleButton", { static: true })
  private button!: ElementRef<HTMLElement>;
  readonly unavailable = signal(!this.google.configured);
  private alive = true;
  private dispose?: () => void;

  async ngAfterViewInit(): Promise<void> {
    if (!this.google.configured) return;
    try {
      this.dispose = await this.google.render(
        this.button.nativeElement,
        credential => { void this.signIn(credential); },
        () => this.alive,
      );
    } catch {
      if (this.alive) {
        this.unavailable.set(true);
        this.showUnavailable();
      }
    }
  }

  private async signIn(credential: string): Promise<void> {
    if (this.loading() || !this.alive) return;
    this.loading.set(true);
    const previousDisableClose = this.dialog?.disableClose;
    if (this.dialog) this.dialog.disableClose = true;
    try {
      await this.auth.signInWithGoogle(credential);
      if (!this.alive) return;
      this.toast.open('Login com Google realizado com sucesso!', 'Fechar', {
        duration: 5000, verticalPosition: 'top', horizontalPosition: 'center',
      });
      this.dialog?.close(true);
      await this.router.navigate(['/mapa']);
    } catch (error) {
      if (!this.alive) return;
      const message = error instanceof HttpErrorResponse && error.status === 401
        ? error.error?.message || 'Não foi possível autenticar esta conta Google.'
        : 'Não foi possível entrar com Google. Tente novamente.';
      this.toast.open(message, 'Fechar', { duration: 7000, verticalPosition: 'top', horizontalPosition: 'center' });
    } finally {
      this.loading.set(false);
      if (this.dialog) this.dialog.disableClose = previousDisableClose;
    }
  }

  showUnavailable(): void {
    this.toast.open(
      "Não foi possível carregar o Google. Tente novamente ou use e-mail e senha.",
      "Fechar",
      {
        duration: 7000,
        verticalPosition: "top",
        horizontalPosition: "center",
      },
    );
  }

  ngOnDestroy(): void {
    this.alive = false;
    this.dispose?.();
  }
}
