import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { firstValueFrom } from 'rxjs';
import { ReportService } from '../../services/report.service';
import { AuthStore } from '../../store/user.store';
import { Loading } from '../loading/loading';

@Component({
  selector: 'app-report-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, Loading],
  templateUrl: './report-dialog.html',
  styleUrl: './report-dialog.scss',
})
export class ReportDialog {
  readonly data = inject<{ collectionPointId: string }>(MAT_DIALOG_DATA);
  private readonly auth = inject(AuthStore);
  private readonly reports = inject(ReportService);
  private readonly dialog = inject(MatDialogRef<ReportDialog>);
  private readonly toast = inject(MatSnackBar);
  readonly submitting = signal(false);
  readonly types = [
    { value: 0, label: 'Ponto de coleta inexistente' },
    { value: 1, label: 'Localização incorreta' },
    { value: 2, label: 'Foto inconsistente' },
  ];
  readonly form = inject(FormBuilder).group({
    message: ['', [Validators.required, Validators.pattern(/\S/)]],
    reportType: [null as number | null, [Validators.required, Validators.min(0), Validators.max(2)]],
  });

  async submit(): Promise<void> {
    if (this.submitting()) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    const user = this.auth.user();
    if (!this.auth.isAuth() || !user) { this.notify('Faça login para enviar uma denúncia.'); return; }
    this.submitting.set(true);
    this.dialog.disableClose = true;
    try {
      const data = this.form.getRawValue();
      await firstValueFrom(this.reports.create({ message: data.message!.trim(), reportType: data.reportType!, userId: user.id, collectionPointId: this.data.collectionPointId }));
      this.notify('Denúncia enviada com sucesso!');
      this.dialog.close(true);
    } catch {
      this.notify('Não foi possível enviar a denúncia. Tente novamente.');
    } finally {
      this.submitting.set(false);
      this.dialog.disableClose = false;
    }
  }

  private notify(message: string): void {
    this.toast.open(message, 'Fechar', { duration: 6000, verticalPosition: 'top', horizontalPosition: 'center' });
  }
}
