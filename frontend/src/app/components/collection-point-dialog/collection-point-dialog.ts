import { MatDialog } from '@angular/material/dialog';
import { ReportDialog } from '../report-dialog/report-dialog';
import { Component, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { CollectionPointResult } from '../../services/collector-point';
import { Flag, LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'app-collection-point-dialog',
  imports: [MatDialogModule, LucideAngularModule],
  templateUrl: './collection-point-dialog.html',
  styleUrl: './collection-point-dialog.scss',
})
export class CollectionPointDialog {
  private readonly dialog = inject(MatDialog);

  openReport(): void {
    this.dialog.open(ReportDialog, { width: '520px', maxWidth: 'calc(100vw - 32px)', data: { collectionPointId: this.point.id } });
  }

  readonly reportIcon = Flag;
  readonly point = inject<CollectionPointResult>(MAT_DIALOG_DATA);
  readonly images = this.point.images ?? [];
  readonly index = signal(0);
  readonly imageFailed = signal(false);

  select(index: number): void {
    if (!this.images.length) return;
    this.index.set((index + this.images.length) % this.images.length);
    this.imageFailed.set(false);
  }
}
