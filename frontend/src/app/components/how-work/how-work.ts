import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { RegisterDialog } from '../register-dialog/register-dialog';
import { Component, inject } from '@angular/core';
import { LucideAngularModule, Map, MapPin } from 'lucide-angular';
import { Button } from '../button/button';
import { Card } from '../card/card';

@Component({
  selector: 'app-how-work',
  imports: [Card, Button, LucideAngularModule, MatDialogModule],
  templateUrl: './how-work.html',
  styleUrl: './how-work.scss',
})
export class HowWork {
  private readonly dialog = inject(MatDialog);

  openRegister(): void {
    this.dialog.open(RegisterDialog, {
      width: '460px',
      maxWidth: 'calc(100vw - 32px)',
      maxHeight: '90vh',
      autoFocus: 'first-tabbable',
    });
  }

  readonly icons = { MapPin, Map };
}
