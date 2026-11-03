import { Component } from '@angular/core';
import { MatDialogModule } from '@angular/material/dialog';
import { LucideAngularModule, TriangleAlert } from 'lucide-angular';
import { Button } from '../button/button';

@Component({
  selector: 'app-auth-required-dialog',
  imports: [MatDialogModule, LucideAngularModule, Button],
  templateUrl: './auth-required-dialog.html',
  styleUrl: './auth-required-dialog.scss',
})
export class AuthRequiredDialog {
  readonly warningIcon = TriangleAlert;
}
